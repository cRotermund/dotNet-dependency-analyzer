using DependencyAnalysis.Tests.TestClasses;
using DependencyAnalysis.TypeAnalysis;

namespace DependencyAnalysis.Tests;

public class Tests
{
    [Test]
    public void TestGenericParameters()
    {
        var analyzer = new TypeAnalyzer();
        var res = analyzer.GetDependencies(typeof(GenericType<>), false);
    }
    
    [Test]
    public void TestGenericParameters2()
    {
        var analyzer = new TypeAnalyzer();
        var res = analyzer.GetDependencies(typeof(ConstructedGenericType), false);
    }
}