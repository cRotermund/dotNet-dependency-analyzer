namespace DependencyAnalysis.Tests.TestClasses;

public class ConstructedGenericType : GenericType<object>
{
    public override object DoSomething()
    {
        return new();
    }
}