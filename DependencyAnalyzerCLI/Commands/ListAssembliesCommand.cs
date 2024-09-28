using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DependencyAnalyzerCLI.Commands;

[Description("List assemblies in the current directory.")]
internal sealed class ListAssembliesCommand : Command<ListAssembliesCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-p|--pattern")]
        public string? SearchPattern { get; init; }
    }
    
    private readonly string _path;

    public ListAssembliesCommand()
    {
        _path = Directory.GetCurrentDirectory();
    }
    
    public ListAssembliesCommand(string path)
    {
        _path = path;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        EnumerationOptions searchOptions = new();

        var searchPattern = settings.SearchPattern ?? "*.dll";
        
        var files = new DirectoryInfo(_path)
            .GetFiles(searchPattern, searchOptions)
            .Where(f => string.Equals(f.Extension, ".dll", StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        foreach (var f in files)
        {
            AnsiConsole.MarkupLine($"{f} [blue]{f.Length:N0}[/] bytes");
        }

        return 0;
    }
}