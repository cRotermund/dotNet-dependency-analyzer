namespace DependencyAnalysis.Tests.TestClasses;

public class GenericType<T> where T : class, new()
{
    public virtual T DoSomething()
    {
        return new T();
    }
}