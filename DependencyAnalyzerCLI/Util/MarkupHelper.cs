using System.Reflection;
using Spectre.Console;

namespace DependencyAnalyzerCLI.Util;

public class MarkupHelper
{
    public static string AssemblyName(string name)
    {
        return Markup.Escape(name);
    }

    public static string AssemblyName(Assembly assembly)
    {
        return AssemblyName(assembly.GetName().Name ?? assembly.ToString());
    }

    public static string Type(Type t)
    {
        string typeName = t.FullName ?? t.ToString();
        string underlyingName = t.UnderlyingSystemType.FullName ?? t.UnderlyingSystemType.ToString();
        if (typeName != underlyingName)
        {
            return $"{Markup.Escape(typeName)} ({Markup.Escape(underlyingName)})";
        }
        else
        {
            return $"{Markup.Escape(typeName)}";
        }
    }
}