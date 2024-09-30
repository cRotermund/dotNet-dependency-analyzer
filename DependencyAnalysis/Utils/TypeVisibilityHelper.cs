namespace DependencyAnalysis.Utils;

public interface ITypeVisibilityHelper
{
    bool IsPublic(Type t);
    bool IsInternal(Type t);
    bool IsPrivate(Type t);
    bool IsProtected(Type t);
}

public class TypeVisibilityHelper : ITypeVisibilityHelper
{
    public bool IsPublic(Type t) 
    {
        return
            t.IsVisible
            && t.IsPublic
            && !t.IsNotPublic
            && !t.IsNested
            && !t.IsNestedPublic
            && !t.IsNestedFamily
            && !t.IsNestedPrivate
            && !t.IsNestedAssembly
            && !t.IsNestedFamORAssem
            && !t.IsNestedFamANDAssem;
    }

    public bool IsInternal(Type t) 
    {
        return
            !t.IsVisible
            && !t.IsPublic
            && t.IsNotPublic
            && !t.IsNested
            && !t.IsNestedPublic
            && !t.IsNestedFamily
            && !t.IsNestedPrivate
            && !t.IsNestedAssembly
            && !t.IsNestedFamORAssem
            && !t.IsNestedFamANDAssem;
    }

    // only nested types can be declared "protected"
    public bool IsPrivate(Type t) 
    {
        return
            !t.IsVisible
            && !t.IsPublic
            && !t.IsNotPublic
            && t.IsNested
            && !t.IsNestedPublic
            && t.IsNestedFamily
            && !t.IsNestedPrivate
            && !t.IsNestedAssembly
            && !t.IsNestedFamORAssem
            && !t.IsNestedFamANDAssem;
    }

    // only nested types can be declared "private"
    public bool IsProtected(Type t) 
    {
        return
            !t.IsVisible
            && !t.IsPublic
            && !t.IsNotPublic
            && t.IsNested
            && !t.IsNestedPublic
            && !t.IsNestedFamily
            && t.IsNestedPrivate
            && !t.IsNestedAssembly
            && !t.IsNestedFamORAssem
            && !t.IsNestedFamANDAssem;
    }
}