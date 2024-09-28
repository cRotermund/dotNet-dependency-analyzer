namespace DependencyAnalysis.DataStructures.Tree;

public class DependencyTree
{
    public TreeNode Root { get; }
    
    public DependencyTree(TreeNode root)
    {
        Root = root;
    }

    public void AddNode(TreeNode node, TreeNode parent)
    {
        node.Parent = parent;
        parent.Children.Add(node);
    }
}