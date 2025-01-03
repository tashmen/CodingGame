﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SourceCombiner
{
	public sealed class SourceCombiner
	{
		private static readonly List<string> SourceFilesToIgnore = new List<string>
		{
			"AssemblyInfo.cs"
		};

		static void Main(string[] args)
		{
			if (args == null || args.Length < 2)
			{
				Console.WriteLine("You must provide at least 2 arguments. The first is the csproj file path and the second is the output file path.");
				return;
			}

			string projectFilePath = args[0];
			string outputFilePath = args[1];

			bool openFile = false;
			if (args.Length > 2)
			{
				Boolean.TryParse(args[2], out openFile);
			}
			string projectFileLocation = args[3];

			var filesToParse = GetSourceFileNames(projectFileLocation);
			Console.Error.WriteLine($"Found {filesToParse.Count} files.");
			var namespaces = GetUniqueNamespaces(filesToParse, projectFileLocation);

			string outputSource = GenerateCombinedSource(namespaces, filesToParse, projectFileLocation);
			outputSource = RemoveTabSpaces(outputSource);
			File.WriteAllText(outputFilePath, outputSource);

			if (openFile)
			{
				Process.Start(outputFilePath);
			}
		}

		private static string RemoveTabSpaces(string output)
		{
			return output.Replace("    ", "");
		}

		private static string GenerateCombinedSource(List<string> namespaces, List<string> files, string projectFileLocation)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(@"/*");
			sb.AppendLine($" * File generated by SourceCombiner.exe using {files.Count} source files.");
			sb.AppendLine($" * Created On: {DateTime.Now}");
			sb.AppendLine(@"*/");

			foreach (var ns in namespaces.OrderBy(s => s))
			{
				sb.AppendLine("using " + ns + ";");
			}

			foreach (var file in files)
			{
				IEnumerable<string> sourceLines = File.ReadAllLines(file);
				sb.AppendLine(@"//*** SourceCombiner -> original file " + Path.GetFileName(file) + " ***");
				var openingTag = "using ";
				foreach (var sourceLine in sourceLines)
				{
					var trimmedLine = sourceLine.Trim().Replace("  ", " ");
					var isUsingDir = trimmedLine.StartsWith(openingTag) && trimmedLine.EndsWith(";");
					if (!string.IsNullOrWhiteSpace(sourceLine) && !isUsingDir)
					{
						sb.AppendLine(sourceLine);
					}
				}
			}

			return sb.ToString();
		}

		private static List<string> GetSourceFileNames(string projectFileDirectory)
		{
			List<string> files = new List<string>();



			files = Directory.GetFiles(projectFileDirectory, "*.cs", SearchOption.AllDirectories)
				.Where(fileName => !fileName.Contains("\\obj\\") && !fileName.Contains("\\bin\\") && !fileName.Contains("\\packages\\")).ToList();

			return files;
		}


		private static List<string> GetUniqueNamespaces(List<string> files, string projectFileLocation)
		{
			var names = new List<string>();
			const string openingTag = "using ";
			const int namespaceStartIndex = 6;

			foreach (var file in files)
			{
				IEnumerable<string> sourceLines = File.ReadAllLines(file);

				foreach (var sourceLine in sourceLines)
				{
					var trimmedLine = sourceLine.Trim().Replace("  ", " ");
					if (trimmedLine.StartsWith(openingTag) && trimmedLine.EndsWith(";"))
					{
						var name = trimmedLine.Substring(namespaceStartIndex, trimmedLine.Length - namespaceStartIndex - 1);

						if (!names.Contains(name))
						{
							names.Add(name);
						}
					}
				}
			}

			return names;
		}
	}
}