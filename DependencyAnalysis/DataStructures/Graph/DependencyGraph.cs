namespace DependencyAnalysis.DataStructures.Graph
{
    public class DependencyGraph
    {
        private Dictionary<Type, GraphNode> _nodeLookup { get; } = new();
        
        public GraphNode Root { get; }
        
        public List<GraphNode> Nodes => _nodeLookup.Values.ToList();

        public DependencyGraph(Type t)
        {
            Root = new GraphNode(t);
            _nodeLookup.Add(t, Root);
        }

        public bool AddDependency(Type dependencyType, Type dependentType)
        {
            bool alreadyInGraph = true;
            
            //Grab or create the node for the dependency.  This may exist already b/c of circular references,
            //which can (and do) exist within assemblies.
            GraphNode? dependencyNode;
            if (!_nodeLookup.TryGetValue(dependencyType, out dependencyNode))
            {
                dependencyNode = new GraphNode(dependencyType);
                _nodeLookup.Add(dependencyType, dependencyNode);
                alreadyInGraph = false;
            }

            //Grab the node for the parent dependent.  We assume this to exist already in the graph.
            //Link the dependent and dependency in the graph
            var dependentNode = _nodeLookup[dependentType];
            
            //Establish links
            dependencyNode.Dependents.Add(dependentNode);
            dependentNode.Dependencies.Add(dependencyNode);

            return alreadyInGraph;
        }
    }
}