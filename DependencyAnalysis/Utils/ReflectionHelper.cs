using System.Reflection;
using System.Text.RegularExpressions;
using DependencyAnalysis.AssemblyAnalysis;

namespace DependencyAnalysis.Utils;

public class ReflectionHelper
{
    private static Regex SYSTEM_PATTERN = new(@"^((System\..+)|(System)|(mscorlib))$");
    
    public static HashSet<Type> GetClassGenericTypeArgs(Type t)
    {
        var hs = new HashSet<Type>();
            
        foreach (var gt in t.GetGenericArguments())
        {
            //A generic type parameter is useful in that it might have constraints - which need to be recorded,
            //Otherwise, it can be discarded.
            if (gt.IsGenericTypeParameter)
            {
                foreach (var constraint in gt.GetGenericParameterConstraints())
                {
                    hs.Add(constraint);
                }
                    
                continue;
            }
                
            hs.Add(gt);
        }

        return hs;
    }

    public static HashSet<Type> GetInheritanceChainTypes(Type t)
    {
        var hs = new HashSet<Type>();

        Type? curType = t.BaseType;
            
        while (curType != null)
        {
            hs.Add(curType);
            curType = curType.BaseType;
        }

        return hs;
    }

    public static HashSet<Type> GetMethodTypes(Type t)
    {
        var hs = new HashSet<Type>();
            
        //Given that we will later recurse, only look at this level of the inheritance chain.
        var mList = t.GetMethods(
            BindingFlags.DeclaredOnly 
            | BindingFlags.Instance
            | BindingFlags.NonPublic
            | BindingFlags.Public
            | BindingFlags.Static);

        foreach (var mInfo in mList)
        {
            //Add the return type
            if (!mInfo.ReturnType.IsGenericParameter && !mInfo.ReturnType.IsGenericMethodParameter)
            {
                hs.Add(mInfo.ReturnType);
            }

            //Add each of the parameter types (may include generics...)
            foreach (var paramInfo in mInfo.GetParameters())
            {
                var pType = paramInfo.ParameterType;
                
                //Ignore anything that is a generic parameter
                if (pType.IsGenericParameter || pType.IsGenericMethodParameter)
                {
                    continue;
                }
                
                hs.Add(pType);
            }

            //Add each of the generic type parameters
            foreach (var gt in mInfo.GetGenericArguments())
            {
                //The constraint is a dependency
                foreach (var constraint in gt.GetGenericParameterConstraints())
                {
                    hs.Add(constraint);
                }
                
                //We only need to trace down the actual type of the parameter if it has been constructed, or defined.
                if (gt.IsConstructedGenericType)
                {
                    hs.Add(gt);
                }
            }

            //Grab the method bodies, and add all local variable types
            MethodBody? body = mInfo.GetMethodBody();
            if (body != null)
            {
                foreach (var lv in body.LocalVariables)
                {
                    //Again... skip generic params
                    if (lv.LocalType.IsGenericParameter || lv.LocalType.IsGenericMethodParameter)
                    {
                        continue;
                    }
                    
                    hs.Add(lv.LocalType);
                }
            }
        }
        
        return hs; 
    }

    public static HashSet<Type> GetPropertyTypes(Type t)
    {
        var hs = new HashSet<Type>();

        var pList = t.GetProperties(
            BindingFlags.DeclaredOnly
            | BindingFlags.Instance
            | BindingFlags.NonPublic
            | BindingFlags.Public
            | BindingFlags.Static);

        foreach (var pInfo in pList)
        {
            hs.Add(pInfo.PropertyType);
        }
            
        return hs;
    }

    /// <summary>
    /// When a type is processed for dependency tracing, we need to properly account for generics.
    ///
    /// Concrete definitions for generic type parameters are dependencies which need to be traced, but when
    /// tracing into the generic type itself, we must only observe the GenericTypeDefinition, not the type
    /// instance.
    ///
    /// Exploring the type instance will result in invalid circular references back to the declaring type.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static HashSet<Type> ExplodeTypeIfGeneric(Type t)
    {
        HashSet<Type> typeSet = new();
            
        //Nothing special needed if not generic, just return the type
        if (!t.IsGenericType)
        {
            typeSet.Add(t);
            return typeSet;
        }

        //Ensure that each type argument is handled, recursing into this function for the same handling
        foreach (var gt in t.GetGenericArguments())
        {
            if (!gt.IsGenericParameter && !gt.IsGenericMethodParameter)
            {
                HashSetHelper.SetAddRange(typeSet, ExplodeTypeIfGeneric(gt));   
            }
        }
            
        //Add the type definition itself.
        typeSet.Add(t.GetGenericTypeDefinition());

        return typeSet;
    }

    public static bool IsSystemType(Type t)
    {
        return IsSystemAssembly(t.Assembly);
    }
    
    public static bool IsSystemAssembly(Assembly a)
    {
        var name = a.GetName().Name ?? a.ToString();
        return IsSystemAssemblyName(name);
    }

    public static bool IsSystemAssemblyName(string name)
    {
        return SYSTEM_PATTERN.IsMatch(name);
    }

    public static Type? GetTypeFromAssembly(Assembly assy, string typeName)
    {
        var types = assy.GetTypes();
            
        foreach (var t in types)
        {
            if (string.Equals(t.FullName, typeName, StringComparison.InvariantCultureIgnoreCase))
            {
                return t;
            }
        }

        return null;
    }
}