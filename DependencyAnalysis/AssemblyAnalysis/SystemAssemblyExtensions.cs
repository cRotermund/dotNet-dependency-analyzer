using System.Reflection;
using System.Text.RegularExpressions;

namespace DependencyAnalysis.AssemblyAnalysis;

public static class SystemAssemblyExtensions
{
    private static Regex SYSTEM_PATTERN = new(@"^((System\..+)|(System)|(mscorlib))$");
    
    public static bool IsSystemAssembly(this Assembly assembly)
    {
        var name = assembly.GetName().Name ?? assembly.ToString();
        return name.IsSystemAssemblyName();
    }

    public static bool IsSystemAssemblyName(this string name)
    {
        return SYSTEM_PATTERN.IsMatch(name);
    }
}