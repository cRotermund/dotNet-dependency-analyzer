namespace DependencyAnalysis.DataStructures.Graph
{
    public class GraphNode
    {
        public Type TypeObj { get; }

        public HashSet<GraphNode> Dependents { get; } = new HashSet<GraphNode>();
        
        public HashSet<GraphNode> Dependencies { get; } = new HashSet<GraphNode>();

        public GraphNode(Type typeObj)
        {
            TypeObj = typeObj;
        }
    }
}