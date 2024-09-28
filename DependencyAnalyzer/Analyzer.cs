using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DependencyAnalyzer
{
	public class Analyzer
	{
		private Dictionary<int, HashSet<string>> REFERENCES = new Dictionary<int, HashSet<string>>();
		private HashSet<string> GATHERED_REFERENCES = new HashSet<string>();
		private readonly string _binPath;

		public Analyzer(string binPath)
		{
			_binPath = binPath;
		}

		public void FindReferences(string find)
		{
			Console.Clear();
			Console.WriteLine($"---==={find}===---");
			int count = 0;

			foreach (var file in Directory.GetFiles(_binPath)
				.Where(o => o.Contains("ChannelAdvisor") && o.EndsWith(".dll")))
			{
				Ref r = new Ref();
				var refs = r.GetRefs(file).Select(o => o.Split(',').First());

				if (refs.Contains(find))
				{
					count++;
					Console.WriteLine(file);
				}
			}

			if (count == 0)
			{
				Console.WriteLine("No References Found");
			}
		}

		public void PrintDependencies(string root)
		{
			Console.Clear();
			Console.WriteLine($"---==={root}===---");

			LoadRefs(root, 1);
			PrintReferences();
		}

		private void LoadRefs(string file, int depth)
		{
			Ref r = new Ref();

			HashSet<string> nextTier = new HashSet<string>();
			HashSet<string> currentTier = r.GetRefs(_binPath + file).Select(o => GetFileNameFromAssembly(o)).ToHashSet<string>();

			foreach (var val in currentTier)
			{
				if (!GATHERED_REFERENCES.Contains(val))
				{
					if (!REFERENCES.ContainsKey(depth)) { REFERENCES[depth] = new HashSet<string>(); }

					REFERENCES[depth].Add(val);

					nextTier.Add(val);

					GATHERED_REFERENCES.Add(val);
				}
			}

			foreach (var refpath in nextTier)
			{
				LoadRefs(refpath, depth + 1);
			}
		}

		private string GetFileNameFromAssembly(string val)
		{
			val = val.Replace(_binPath, "");
			string result = val.Substring(0, val.IndexOf(','));
			return result + ".dll";
		}

		private void PrintReferences()
		{
			HashSet<string> printedRefs = new HashSet<string>();

			if (REFERENCES != null && REFERENCES.Count > 0)
			{
				foreach (var refWDepth in REFERENCES)
				{
					foreach (string val in refWDepth.Value)
					{
						if (!printedRefs.Contains(val))
						{
							Print(refWDepth.Key, val);
							printedRefs.Add(val);
						}
					}
				}
			}
			else
			{
				Console.WriteLine("No Dependencies Found");
			}
		}

		private void Print(int depth, string val)
		{
			string line = "";
			for (int i = 0; i < depth; i++)
			{
				line += "\t";
			}

			Console.WriteLine(line + val);
		}
	}
}