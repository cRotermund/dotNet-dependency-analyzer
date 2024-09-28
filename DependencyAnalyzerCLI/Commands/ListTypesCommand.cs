using System.ComponentModel;
using DependencyAnalysis.AssemblyAnalysis;
using DependencyAnalysis.TypeAnalysis;
using DependencyAnalyzerCLI.Util;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DependencyAnalyzerCLI.Commands;

[Description("List types in an assembly.")]
internal sealed class ListTypesCommand : Command<ListTypesCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[assemblyName]")] 
        public string AssemblyName
        {
            get => _assemblyName;
            init => _assemblyName = SettingsHelper.AssemblyNameParser(value);
        }
        private string _assemblyName;

        [Description(
            "Define the visibility of types to return, defaults to all '1111'. This value is a 4-bit mask, with " +
            "digits corresponding to 'public', 'internal', 'protected' and 'private', respectively.\r\n" +
            "To retrieve only public types for example, specity '1000'.\r\n" +
            "To retrieve only internals, specify '0100'.\r\n" +
            "To specify public and internals, '1100', and so forth.")]
        [CommandOption("--visibility")]
        public string VisibilityMask { get; init; } = "1111";
    }
    
    private readonly IAssemblyAnalyzer _analyzer;
    private readonly ITypeVisibilityHelper _vizHelper;
    
    public ListTypesCommand(IAssemblyAnalyzer analyzer, ITypeVisibilityHelper vizHelper)
    {
        _analyzer = analyzer;
        _vizHelper = vizHelper;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        List<Type> types = _analyzer.GetAssemblyTypes(settings.AssemblyName);
        List<Type> filtered = new();

        bool includePublic = settings.VisibilityMask[0] == '1';
        bool includeInternal = settings.VisibilityMask[1] == '1';
        bool includeProtected = settings.VisibilityMask[2] == '1';
        bool includePrivate = settings.VisibilityMask[3] == '1';
        
        //Filter types based on visibility and command inputs
        foreach (var t in types)
        {
            if (!includePublic && _vizHelper.IsPublic(t)
                || !includeInternal && _vizHelper.IsInternal(t)
                || !includeInternal && _vizHelper.IsInternal(t)
                || !includeProtected && _vizHelper.IsProtected(t)
                || !includePrivate && _vizHelper.IsPrivate(t))
            {
                continue;
            }

            filtered.Add(t);
        }

        foreach (var t in types)
        {
            AnsiConsole.MarkupLine($"{MarkupHelper.Type(t)})");
        }

        return 0;
    }
}