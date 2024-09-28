using System.Reflection;

namespace DependencyAnalysis.DataStructures.Tree;

public class TreeNode
{
    public string AssemblyName { get; }
    public Assembly? AssemblyObj { get; }
    public HashSet<Type> ReferencedTypeList { get; } = new();
    
    public TreeNode? Parent { get; set; }
    public List<TreeNode> Children { get; } = new();

    public TreeNode(Assembly a)
    {
        if (a == null)
        {
            throw new ArgumentException("Assembly cannot be null", nameof(a));
        }
        
        AssemblyName = a.GetName().Name ?? "";
        AssemblyObj = a;
    }
    
    public TreeNode(string name)
    {
        AssemblyName = name;
    }
}