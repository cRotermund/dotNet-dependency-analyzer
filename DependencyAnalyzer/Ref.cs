using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyAnalyzer
{
	internal class Ref
	{
		internal HashSet<string> GetRefs(string path)
		{
			var assembly = System.Reflection.Assembly.LoadFrom(path);
			var refs = new HashSet<string>();

			foreach (var assemblyName in assembly.GetReferencedAssemblies())
			{
				string name = assemblyName.ToString();

				if (name.StartsWith("ChannelAdvisor"))
				{
					refs.Add(name);
				}
			}

			return refs;
		}
	}
}