using DependencyAnalyzer;

public class Program
{
	private static string PATH = @"C:\CADev\complete\bin\";
	private static string DEFAULT_OPERATION = "References";
	private static string DEFAULT_PROJECT = "ChannelAdvisor.Synchronization";

	private static void Main(string[] args)
	{
		string startingProject;
		string operation;

		if (args == null || args.Length == 0)
		{
			startingProject = DEFAULT_PROJECT;
			operation = DEFAULT_OPERATION;
		}
		else if (args.Length == 2)
		{
			startingProject = args[0];
			operation = args[1];
		}
		else
		{
			throw new ArgumentException("wrong number of args");
		}

		startingProject = startingProject.Replace(".dll", "");

		Analyzer analyzer = new Analyzer(PATH);

		if (operation.ToLower() == "references")
		{
			analyzer.FindReferences(startingProject);
		}
		else
		{
			analyzer.PrintDependencies($"{startingProject}.dll");
		}
	}
}