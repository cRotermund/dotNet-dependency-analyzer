using DependencyAnalysis.AssemblyAnalysis;
using DependencyAnalysis.TypeAnalysis;
using DependencyAnalysis.Utils;
using DependencyAnalyzerCLI.Commands;

namespace DependencyAnalyzerCLI.Tests;

public class CommandTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestListAssembliesCommand()
    {
        var cmd = new ListAssembliesCommand("C:\\dev\\complete\\bin");
        cmd.Execute(null, new());
    }
    
    [Test]
    public void TestListAssembliesCommandWithFilter()
    {
        var cmd = new ListAssembliesCommand("C:\\dev\\complete\\bin");
        cmd.Execute(null, 
            new()
            {
                SearchPattern = "ChannelAdvisor.Inventory*"
            });
    }

    [Test]
    public void TestListTypesCommand()
    {
        var cmd = new ListTypesCommand(new AssemblyAnalyzer("C:\\dev\\complete\\bin"), new TypeVisibilityHelper());
        cmd.Execute(null, 
            new()
            {
                AssemblyName = "ChannelAdvisor.Inventory.API.Implementation"
            });
    }
    
    [Test]
    public void TestListTypesCommandWithExtension()
    {
        var cmd = new ListTypesCommand(new AssemblyAnalyzer("C:\\dev\\complete\\bin"), new TypeVisibilityHelper());
        cmd.Execute(null, 
            new()
            {
                AssemblyName = "ChannelAdvisor.Inventory.API.Implementation.dll"
            });
    }

    [Test]
    public void TestListTypesCommandWithMask()
    {
        var cmd = new ListTypesCommand(new AssemblyAnalyzer("C:\\dev\\complete\\bin"), new TypeVisibilityHelper());
        cmd.Execute(null, new()
        {
            AssemblyName = "ChannelAdvisor.Inventory.API.Implementation", 
            VisibilityMask = "0100"
        });
    }

    [Test]
    public void TestListDependenciesOfTypeCommand()
    {
        var cmd = new ListDependenciesOfTypeCommand(new TypeAnalyzer("C:\\dev\\complete\\bin"));
        cmd.Execute(null,
            new()
            {
                AssemblyName = "ChannelAdvisor.Inventory.API.Implementation",
                TypeName = "ChannelAdvisor.Inventory.API.Implementation.BulkInventoryExtractorV4"
            });
    }
    
    [Test]
    public void TestListDependenciesOfTypeAssembliesOnlyCommand()
    {
        var cmd = new ListDependenciesOfTypeCommand(new TypeAnalyzer("C:\\dev\\complete\\bin"));
        cmd.Execute(null,
            new()
            {
                AssemblyName = "ChannelAdvisor.Inventory.API.Implementation",
                TypeName = "ChannelAdvisor.Inventory.API.Implementation.BulkInventoryExtractorV4",
                AssembliesOnly = true
            });
    }

    [Test]
    public void TestListDependenciesOfAssemblyCommand()
    {
        var cmd = new ListDependenciesOfAssemblyCommand(new AssemblyAnalyzer("C:\\dev\\complete\\bin"));
        cmd.Execute(null,
            new()
            {
                AssemblyName = "ChannelAdvisor.Inventory.API.Implementation"
            });
    }
}