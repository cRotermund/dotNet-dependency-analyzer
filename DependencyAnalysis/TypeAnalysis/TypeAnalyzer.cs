using System.Reflection;
using DependencyAnalysis.AssemblyAnalysis;
using DependencyAnalysis.DataStructures.Graph;
using DependencyAnalysis.DataStructures.Tree;
using DependencyAnalysis.Infrastructure;
using DependencyAnalysis.Utils;

namespace DependencyAnalysis.TypeAnalysis
{
    public interface ITypeAnalyzer
    {
        TypeDependenciesResult GetDependencies(string assemblyName, string typeName, bool includeSystemTypes);
    }
    
    public class TypeAnalyzer : ITypeAnalyzer
    {
        private readonly AssemblyLoader _loader;

        //This constructor will use the local assembly context.
        public TypeAnalyzer()
        {
            _loader = new AssemblyLoader();
        }
        
        public TypeAnalyzer(string binPath)
        {
            _loader = new AssemblyLoader(binPath);
            
            if (!Directory.Exists(binPath))
            {
                throw new FileNotFoundException("The binPath doesn't exist");
            }
        }
        
        public TypeDependenciesResult GetDependencies(string assemblyName, string typeName, bool includeSystemTypes)
        {   
            var assembly = _loader.Load(new AssemblyName(assemblyName));

            var targetType = ReflectionHelper.GetTypeFromAssembly(assembly, typeName);
            if (targetType == null)
            {
                throw new ArgumentException(
                    $"Target type ({typeName}) not found in target assembly: {assemblyName}.");
            }

            return GetDependencies(targetType, includeSystemTypes);
        }

        public TypeDependenciesResult GetDependencies(Type t, bool includeSystemTypes)
        {
            var graph = new DependencyGraph(t);
            BuildDependencyGraph(t, graph, includeSystemTypes);

            var tree = BuildDependencyTreeFromGraph(graph);

            return new TypeDependenciesResult
            {
                TypeDependencyGraph = graph,
                AssemblyDependencyTree = tree
            };
        }
        
        private void BuildDependencyGraph(Type t, DependencyGraph graph, bool includeSystemTypes)
        {
            HashSet<Type> dependencies = new();
            
            //Scan for generic types on class
            HashSetHelper.SetAddRange(dependencies, ReflectionHelper.GetClassGenericTypeArgs(t));
            
            //Scan for inheritance types
            HashSetHelper.SetAddRange(dependencies, ReflectionHelper.GetInheritanceChainTypes(t));
            
            //Scan method signatures and bodies (params and return and local variables)
            HashSetHelper.SetAddRange(dependencies, ReflectionHelper.GetMethodTypes(t));

            //Scan properties
            HashSetHelper.SetAddRange(dependencies, ReflectionHelper.GetPropertyTypes(t));
            
            //For each dependency detected, explode it appropriately if generic
            //Add the necessary dependencies to our graph, eliminating duplicates and determining if recursion is
            //necessary
            
            List<Type> recurseList = new();
            foreach (var dep in dependencies)
            {
                var explodedDeps = ReflectionHelper.ExplodeTypeIfGeneric(dep);

                foreach (var exploded in explodedDeps)
                {
                    if (includeSystemTypes || !ReflectionHelper.IsSystemType(exploded))
                    {
                        if (!graph.AddDependency(exploded, t))
                        {
                            recurseList.Add(exploded);
                        }
                    }
                }
            }

            foreach (var dep in recurseList)
            {
                BuildDependencyGraph(dep, graph, includeSystemTypes);
            }
        }

        private DependencyTree BuildDependencyTreeFromGraph(DependencyGraph g)
        {
            var treeRoot = new TreeNode(g.Root.TypeObj.Assembly);
            treeRoot.ReferencedTypeList.Add(g.Root.TypeObj);
            var tree = new DependencyTree(treeRoot);
            
            //A graph node will only ever relate to one tree node.
            Dictionary<GraphNode, TreeNode> nodeMap = new();
            nodeMap.Add(g.Root, treeRoot);
            
            //The queue of vertices (v) and the parent on the search path (p), Using a tuple (p,v)
            var nodeQueue = new Queue<(GraphNode, GraphNode)>();
            
            //Begin the queue starting with the root's children
            foreach (var node in g.Root.Dependencies)
            {
                nodeQueue.Enqueue((g.Root, node));
            }

            //Continue on the BFS until the queue is drained.
            while (nodeQueue.Count > 0)
            {
                var (curParent, curNode) = nodeQueue.Dequeue();
                
                //If we have already visited this graph node, we can skip it
                if (nodeMap.ContainsKey(curNode))
                {
                    continue;
                }
                
                //Queue up the children of this node for the BFS
                foreach (var child in curNode.Dependencies)
                {
                    nodeQueue.Enqueue((curNode, child));
                }
                
                //Assume the parent has a tree node already, it should, if our algorithm is working
                var parentTreeNode = nodeMap[curParent];
                
                //If our parent graph node's tree node is the same assembly, this is an inner-assembly reference
                if (parentTreeNode.AssemblyObj == curNode.TypeObj.Assembly)
                {
                    parentTreeNode.ReferencedTypeList.Add(curNode.TypeObj);
                    nodeMap.Add(curNode, parentTreeNode);
                }
                //Otherwise, we are referencing outside of the current assembly
                else
                {
                    //This assembly jump may still have been accounted for in the BFS.
                    //Don't create a new tree node, unless necessary
                    TreeNode? existingTreeNode = parentTreeNode.Children
                        .FirstOrDefault(tn => tn.AssemblyObj == curNode.TypeObj.Assembly);
                    
                    if (existingTreeNode != null)
                    {
                        existingTreeNode.ReferencedTypeList.Add(curNode.TypeObj);
                        nodeMap.Add(curNode, existingTreeNode);
                    }
                    else
                    {
                        TreeNode newTreeNode = new(curNode.TypeObj.Assembly);
                        newTreeNode.ReferencedTypeList.Add(curNode.TypeObj);
                        tree.AddNode(newTreeNode, parentTreeNode);
                        nodeMap.Add(curNode, newTreeNode);
                    }
                }
            }

            return tree;
        }
    }
}