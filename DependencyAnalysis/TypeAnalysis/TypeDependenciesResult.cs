using DependencyAnalysis.DataStructures.Graph;
using DependencyAnalysis.DataStructures.Tree;

namespace DependencyAnalysis.TypeAnalysis;

public class TypeDependenciesResult
{
    public DependencyGraph TypeDependencyGraph { get; set; }
    public DependencyTree AssemblyDependencyTree { get; set; }
}