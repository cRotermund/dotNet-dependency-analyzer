using System.ComponentModel;
using DependencyAnalysis.AssemblyAnalysis;
using DependencyAnalyzerCLI.Util;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DependencyAnalyzerCLI.Commands;

[Description("List the dependencies of an assembly.")]
internal sealed class ListDependenciesOfAssemblyCommand : Command<ListDependenciesOfAssemblyCommand.Settings>
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

        [Description("Set the recursion depth when building the dependency tree. Defaults to 5 levels deep")]
        [CommandOption("--depth")]
        public int Depth { get; init; } = 5;

        [Description("Set this flag to include system references in the dependency tree.  Will NOT recurse by default.")]
        [CommandOption("-s | --includeSystem")]
        public bool IncludeSystem { get; init; } = false;

        [Description("If specified will include and recurse into system references in the dependency tree.  Be wary of depth")]
        [CommandOption("-S | --recurseSystem")]
        public bool RecurseSystem { get; init; } = false;

        //Helper to handle the override value based on recursion setting
        public bool ShouldIncludeSystem => IncludeSystem || RecurseSystem;
    }

    private readonly IAssemblyAnalyzer _analyzer;

    public ListDependenciesOfAssemblyCommand(IAssemblyAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    public override int Execute(CommandContext context, Settings settings)
    { 
        var refTree = _analyzer.GetAssemblyReferences(
            settings.AssemblyName, 
            settings.Depth,
            settings.ShouldIncludeSystem,
            settings.RecurseSystem);
        
        //create the console tree model
        var printableTree = new Tree($"{MarkupHelper.AssemblyName(refTree.Root.AssemblyName)}");

        //add children and walk down the tree recursively, slightly repetetive because the root is not
        //the same type as the child nodes in the console library.
        foreach (var child in refTree.Root.Children)
        {
            var tn = printableTree.AddNode($"{MarkupHelper.AssemblyName(child.AssemblyName)}");
            WalkChild(tn, child);
        }
        
        AnsiConsole.Write(printableTree);
        
        return 0;
    }

    private void WalkChild(Spectre.Console.TreeNode p, DependencyAnalysis.DataStructures.Tree.TreeNode c)
    {
        foreach (var child in c.Children)
        {
            var tn = p.AddNode($"{MarkupHelper.AssemblyName(child.AssemblyName)}");
            WalkChild(tn, child);
        }
    }
}