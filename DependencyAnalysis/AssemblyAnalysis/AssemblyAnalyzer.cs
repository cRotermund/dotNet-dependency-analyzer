using System.Reflection;
using DependencyAnalysis.DataStructures.Tree;
using DependencyAnalysis.Infrastructure;
using DependencyAnalysis.Utils;

namespace DependencyAnalysis.AssemblyAnalysis;

public interface IAssemblyAnalyzer
{
    List<Type> GetAssemblyTypes(string assemblyName);
    DependencyTree GetAssemblyReferences(
        string assemblyName, 
        int maxDepth, 
        bool includeSystem=true, 
        bool recurseSystem=false);
}

public class AssemblyAnalyzer : IAssemblyAnalyzer
{
    
    private readonly string _targetBinPath;
    private readonly AssemblyLoader _loader;
    
    public AssemblyAnalyzer(string binPath)
    {
        _targetBinPath = binPath;
        _loader = new AssemblyLoader(_targetBinPath);
            
        if (!Directory.Exists(_targetBinPath))
        {
            throw new FileNotFoundException("The binPath doesn't exist");
        }
    }
    
    public List<Type> GetAssemblyTypes(string assemblyName)
    {
        var assembly = _loader.Load(new AssemblyName(assemblyName));
        if (assembly == null)
        {
            throw new FileNotFoundException(
                $"Target assembly: {assemblyName} not found in bin path: {_targetBinPath}");
        }

        return assembly.GetTypes().ToList();
    }

    public DependencyTree GetAssemblyReferences(
        string assemblyName, 
        int maxDepth, 
        bool includeSystem=true,
        bool recurseSystem=false)
    {
        var assembly = _loader.Load(new AssemblyName(assemblyName));
        if (assembly == null)
        {
            throw new FileNotFoundException(
                $"Target assembly: {assemblyName} not found in bin path: {_targetBinPath}");
        }
        
        var root = new TreeNode(assembly);
        var tree = new DependencyTree(root);

        BuildDependencyTree(tree, root, 1, maxDepth, includeSystem, recurseSystem);

        return tree;
    }

    private void BuildDependencyTree(
        DependencyTree tree, 
        TreeNode curNode, 
        int curDepth, 
        int maxDepth,
        bool includeSystem,
        bool recurseSystem)
    {
        var refAssemblies = curNode.AssemblyObj.GetReferencedAssemblies();
        foreach (var refAssemblyName in refAssemblies)
        {
            if (!includeSystem && ReflectionHelper.IsSystemAssemblyName(refAssemblyName.Name))
            {
                continue;
            }
            
            Assembly? _loadedRef = _loader.Load(refAssemblyName);
            
            if (_loadedRef != null)
            {
                var newNode = new TreeNode(_loadedRef);
                tree.AddNode(newNode, curNode);
                
                //Recurse if under max depth, and if system constraint eligible
                if (curDepth < maxDepth 
                    && (recurseSystem || !ReflectionHelper.IsSystemAssemblyName(refAssemblyName.Name)))
                {
                    BuildDependencyTree(tree, newNode, curDepth+1, maxDepth, includeSystem, recurseSystem);
                }
            }
            else
            {
                var newNode = new TreeNode(refAssemblyName.Name);
                tree.AddNode(newNode, curNode);
            }
        }
    }
}