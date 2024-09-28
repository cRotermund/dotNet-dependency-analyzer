using System.Reflection;
using DependencyAnalysis.AssemblyAnalysis;
using DependencyAnalysis.DataStructures.Graph;
using DependencyAnalysis.DataStructures.Tree;
using DependencyAnalysis.Infrastructure;

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

            var targetType = GetTypeFromAssembly(assembly, typeName);
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
            foreach (Type x in GetClassGenericTypesArgs(t))
            {
                foreach (Type y in ExplodeGenericTypeDependencies(x))
                {
                    if (includeSystemTypes || !y.Assembly.IsSystemAssembly())
                    {
                        dependencies.Add(y);
                    }
                }
            }

            //Scan for inheritance types
            foreach (Type x in GetInheritanceChainTypes(t))
            {
                foreach (Type y in ExplodeGenericTypeDependencies(x))
                {
                    if (includeSystemTypes || !y.Assembly.IsSystemAssembly())
                    {
                        dependencies.Add(y);
                    }
                }
            }
            
            //Scan method signatures and bodies (params and return and local variables)
            foreach (Type x in GetMethodSignatureTypes(t))
            {
                foreach (Type y in ExplodeGenericTypeDependencies(x))
                {
                    if (includeSystemTypes || !y.Assembly.IsSystemAssembly())
                    {
                        dependencies.Add(y);
                    }
                }
            }
            
            //Scan properties
            foreach (Type x in GetPropertyTypes(t))
            {
                foreach (Type y in ExplodeGenericTypeDependencies(x))
                {
                    if (includeSystemTypes || !y.Assembly.IsSystemAssembly())
                    {
                        dependencies.Add(y);
                    }
                }
            }
            
            //Add the collection of dependencies to the graph, and then recurse, walking into each if necessary
            List<Type> recurseList = new();
            foreach (var dep in dependencies)
            {
                //If the graph add comes back false, we've already traced this dep.
                if (!graph.AddDependency(dep, t))
                {
                    recurseList.Add(dep);
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

        private HashSet<Type> GetClassGenericTypesArgs(Type t)
        {
            var hs = new HashSet<Type>();
            
            foreach (var gt in t.GetGenericArguments())
            {
                //A generic type parameter is useful in that it might have constraints - which need to be recorded,
                //Otherwise, it can be discarded.
                if (gt.IsGenericTypeParameter)
                {
                    foreach (var constraint in gt.GetGenericParameterConstraints())
                    {
                        hs.Add(constraint);
                    }
                    
                    continue;
                }
                
                hs.Add(gt);
            }

            return hs;
        }

        private HashSet<Type> GetInheritanceChainTypes(Type t)
        {
            var hs = new HashSet<Type>();

            Type? curType = t.BaseType;
            
            while (curType != null)
            {
                hs.Add(curType);
                curType = curType.BaseType;
            }

            return hs;
        }

        private HashSet<Type> GetMethodSignatureTypes(Type t)
        {
            var hs = new HashSet<Type>();
            
            //Given that we will later recurse, only look at this level of the inheritance chain.
            var mList = t.GetMethods(
                BindingFlags.DeclaredOnly 
                | BindingFlags.Instance
                | BindingFlags.NonPublic
                | BindingFlags.Public
                | BindingFlags.Static);

            foreach (var mInfo in mList)
            {
                //Add the return type
                if (!mInfo.ReturnType.IsGenericParameter && !mInfo.ReturnType.IsGenericMethodParameter)
                {
                    hs.Add(mInfo.ReturnType);
                }

                //Add each of the parameter types (may include generics...)
                foreach (var paramInfo in mInfo.GetParameters())
                {
                    var pType = paramInfo.ParameterType;
                    
                    //Ignore anything that is a generic parameter
                    if (pType.IsGenericParameter || pType.IsGenericMethodParameter)
                    {
                        continue;
                    }
                    
                    hs.Add(pType);
                }

                //Add each of the generic type parameters
                foreach (var gt in mInfo.GetGenericArguments())
                {
                    //The constraint is a dependency
                    foreach (var constraint in gt.GetGenericParameterConstraints())
                    {
                        hs.Add(constraint);
                    }
                    
                    //We only need to trace down the actual type of the parameter if it has been constructed, or defined.
                    if (gt.IsConstructedGenericType)
                    {
                        hs.Add(gt);
                    }
                }

                //Grab the method bodies, and add all local variable types
                MethodBody? body = mInfo.GetMethodBody();
                if (body != null)
                {
                    foreach (var lv in body.LocalVariables)
                    {
                        //Again... skip generic params
                        if (lv.LocalType.IsGenericParameter || lv.LocalType.IsGenericMethodParameter)
                        {
                            continue;
                        }
                        
                        hs.Add(lv.LocalType);
                    }
                }
            }
            
            return hs; 
        }
        
        private HashSet<Type> GetPropertyTypes(Type t)
        {
            var hs = new HashSet<Type>();

            var pList = t.GetProperties(
                BindingFlags.DeclaredOnly
                | BindingFlags.Instance
                | BindingFlags.NonPublic
                | BindingFlags.Public
                | BindingFlags.Static);

            foreach (var pInfo in pList)
            {
                hs.Add(pInfo.PropertyType);
            }
            
            return hs;
        }

        private Type? GetTypeFromAssembly(Assembly assy, string typeName)
        {
            var types = assy.GetTypes();
            
            foreach (var t in types)
            {
                if (string.Equals(t.FullName, typeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return t;
                }
            }

            return null;
        }

        /// <summary>
        /// When a type is processed for dependency tracing, we need to properly account for generics.
        ///
        /// Concrete definitions for generic type parameters are dependencies which need to be traced, but when
        /// tracing into the generic type itself, we must only observe the GenericTypeDefinition, not the type
        /// instance.
        ///
        /// Exploring the type instance will result in invalid circular references back to the declaring type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private List<Type> ExplodeGenericTypeDependencies(Type t)
        {
            List<Type> typeList = new();
            
            //Nothing special needed if not generic, just return the type
            if (!t.IsGenericType)
            {
                typeList.Add(t);
                return typeList;
            }

            //Ensure that each type argument is handled, recursing into this function for the same handling
            foreach (var gt in t.GetGenericArguments())
            {
                if (!gt.IsGenericParameter && !gt.IsGenericMethodParameter)
                {
                    typeList.AddRange(ExplodeGenericTypeDependencies(gt));   
                }
            }
            
            //Add the type definition itself.
            typeList.Add(t.GetGenericTypeDefinition());

            return typeList;
        }
    }
}