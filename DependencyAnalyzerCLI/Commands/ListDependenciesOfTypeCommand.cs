using System.ComponentModel;
using DependencyAnalysis.TypeAnalysis;
using DependencyAnalyzerCLI.Util;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DependencyAnalyzerCLI.Commands;

[Description("List the dependencies of a specific type within an assembly.")]
internal sealed class ListDependenciesOfTypeCommand : Command<ListDependenciesOfTypeCommand.Settings>
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
        
        [CommandArgument(0, "[typeName]")] 
        public string TypeName { get; init; } = "";

        [CommandOption("-a|--assembliesOnly")] 
        public bool AssembliesOnly { get; init; } = false;

        [CommandOption("-s|--includeSystemTypes")]
        public bool IncludeSystemTypes { get; init; } = false;
    }
    
    private readonly ITypeAnalyzer _analyzer;
    
    public ListDependenciesOfTypeCommand(ITypeAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        var deps = _analyzer.GetDependencies(
            settings.AssemblyName, 
            settings.TypeName, 
            settings.IncludeSystemTypes);
        
        var depTree = deps.AssemblyDependencyTree;
        
        //create the console tree model
        var printableTree = new Tree($"[grey]{MarkupHelper.AssemblyName(depTree.Root.AssemblyName)}[/]");
        printableTree.AddNode(new Table()
            .AddColumn("Root Type")
            .BorderColor(Color.Blue)
            .AddRow($"[blue]{MarkupHelper.Type(depTree.Root.ReferencedTypeList.First())}[/]"));

        //add children and walk down the tree recursively, slightly repetetive because the root is not
        //the same type as the child nodes in the console library.
        foreach (var child in depTree.Root.Children)
        {
            var tn = printableTree.AddNode($"[grey]{MarkupHelper.AssemblyName(child.AssemblyName)}[/]");
            if (!settings.AssembliesOnly)
            {
                var typeTable = new Table()
                    .RoundedBorder()
                    .BorderColor(Color.Blue)
                    .AddColumn("Referenced Types");
            
                foreach (var t in child.ReferencedTypeList)
                {
                    typeTable.AddRow(MarkupHelper.Type(t));
                }

                tn.AddNode(typeTable);
            }
            WalkChild(tn, child, settings);
        }
        
        AnsiConsole.Write(printableTree);
        
        return 0;
    }

    private void WalkChild(
        Spectre.Console.TreeNode p, 
        DependencyAnalysis.DataStructures.Tree.TreeNode c,
        Settings settings)
    {
        foreach (var child in c.Children)
        {
            var tn = p.AddNode($"[grey]{MarkupHelper.AssemblyName(child.AssemblyName)}[/]");
            
            if (!settings.AssembliesOnly)
            {
                var typeTable = new Table()
                    .RoundedBorder()
                    .BorderColor(Color.Blue)
                    .AddColumn("Referenced Types");
            
                foreach (var t in child.ReferencedTypeList)
                {
                    typeTable.AddRow(MarkupHelper.Type(t));
                }

                tn.AddNode(typeTable);
            }
            
            WalkChild(tn, child, settings);
        }
    }
}