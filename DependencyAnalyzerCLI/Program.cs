using DependencyAnalysis.AssemblyAnalysis;
using DependencyAnalysis.TypeAnalysis;
using DependencyAnalyzerCLI.Commands;
using DependencyAnalyzerCLI.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

//Register services for DI things...
var sc = new ServiceCollection();

sc.AddTransient<ITypeAnalyzer>(_ => new TypeAnalyzer(Directory.GetCurrentDirectory()));
sc.AddTransient<IAssemblyAnalyzer>(_ => new AssemblyAnalyzer(Directory.GetCurrentDirectory()));
sc.AddTransient<ITypeVisibilityHelper>(_ => new TypeVisibilityHelper());

sc.AddTransient<ListAssembliesCommand>();
sc.AddTransient<ListTypesCommand>();
sc.AddTransient<ListDependenciesOfTypeCommand>();
sc.AddTransient<ListDependenciesOfAssemblyCommand>();

var tr = new TypeRegistrar(sc);

//Instantiate the app, with commands...
var app = new CommandApp(tr);
app.Configure(cfg =>
{
    cfg.AddCommand<ListAssembliesCommand>("la");
    cfg.AddCommand<ListTypesCommand>("lt");
    cfg.AddCommand<ListDependenciesOfTypeCommand>("dot");
    cfg.AddCommand<ListDependenciesOfAssemblyCommand>("doa");
});

app.Run(args);