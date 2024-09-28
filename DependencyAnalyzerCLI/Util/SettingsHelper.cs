namespace DependencyAnalyzerCLI.Util;

public class SettingsHelper
{
    public static string AssemblyNameParser(string val)
    {
        if (val.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
        {
            val = val.Replace(".dll", "", StringComparison.InvariantCultureIgnoreCase);
        }

        return val;
    }
}