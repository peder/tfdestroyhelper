using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurgeTFS
{
	public class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("----------------------------");
			Console.WriteLine("     TFDestroyHelper");
			Console.WriteLine("----------------------------");
			Console.WriteLine("A fine product by Peder Rice");

			if(args.Length != 3)
			{
				Console.Error.WriteLine("Must specify TFS path and valid date");
				Console.Error.WriteLine("Usage:");
				Console.Error.WriteLine("PurgeTFS.exe [Local TFS Directory] [Server TFS Path] [Valid Date YYYY-MM-DD]");
				Console.Error.WriteLine();
				Console.Error.WriteLine("Example Usage:");
				Console.Error.WriteLine("PurgeTFS.exe C:\\Projects $/MyProject/Branches 2015-09-01");
				return;
			}

			var workingDirectory = args[0];

			var tfsPath = args[1];
			DateTime validDate;
			DateTime.TryParse(args[2], out validDate);

			if (tfsPath.EndsWith("/"))
			{
				tfsPath = tfsPath.Substring(0, tfsPath.Length - 1);
			}

			Console.WriteLine("TF.exe location: " + ConfigurationManager.AppSettings["TFExecutablePath"]);
			Console.WriteLine("Working directory: " + workingDirectory);
			Console.WriteLine("Purge directory: " + tfsPath);
			Console.WriteLine("Will delete everything before " + validDate.ToString("yyyy/MM/dd") + ". Ok to purge [Y/N]?");
			var userInput = Console.ReadLine();

			if (userInput.ToUpper().Trim() != "Y")
			{
				Console.Error.WriteLine("Canceling");
				return;
			}

			var directoryProcess = new Process();
			directoryProcess.StartInfo.WorkingDirectory = workingDirectory;
			directoryProcess.StartInfo.FileName = ConfigurationManager.AppSettings["TFExecutablePath"];
			directoryProcess.StartInfo.CreateNoWindow = true;
			directoryProcess.StartInfo.RedirectStandardError = true;
			directoryProcess.StartInfo.RedirectStandardOutput = true;
			directoryProcess.StartInfo.UseShellExecute = false;

			directoryProcess.StartInfo.Arguments = "dir /folders " + tfsPath;

			directoryProcess.Start();
			var directoryProcessOutput = directoryProcess.StandardOutput.ReadToEnd();
			directoryProcess.WaitForExit();

			var directories = directoryProcessOutput.Split('\n').Select(folderName => folderName.Trim().Replace("$", "")).Where(folderName => folderName.Contains(".") && !folderName.Contains("/"));

			Console.WriteLine();
			Console.WriteLine();

			foreach(var directory in directories)
			{
				var historyProcess = new Process();
				historyProcess.StartInfo.WorkingDirectory = workingDirectory;
				historyProcess.StartInfo.FileName = ConfigurationManager.AppSettings["TFExecutablePath"];
				historyProcess.StartInfo.CreateNoWindow = true;
				historyProcess.StartInfo.RedirectStandardError = true;
				historyProcess.StartInfo.RedirectStandardOutput = true;
				historyProcess.StartInfo.UseShellExecute = false;

				historyProcess.StartInfo.Arguments = "hist /noprompt " + tfsPath + "/" + directory;

				historyProcess.Start();
				var historyProcessOutput = historyProcess.StandardOutput.ReadToEnd();
				historyProcess.WaitForExit();

				var lastModifiedDateString = String.Concat(String.Concat(historyProcessOutput.TrimEnd().Reverse()).Split(' ', '\t')[0].Reverse());

				DateTime lastModifiedDate;
				var willDelete = DateTime.TryParse(lastModifiedDateString, out lastModifiedDate) && lastModifiedDate < validDate && lastModifiedDate > DateTime.MinValue;

				var fullPath = tfsPath + "/" + directory;

				if (willDelete)
				{
					var process = new Process();
					process.StartInfo.WorkingDirectory = workingDirectory;
					process.StartInfo.FileName = ConfigurationManager.AppSettings["TFExecutablePath"];
					process.StartInfo.CreateNoWindow = true;
					process.StartInfo.RedirectStandardError = true;
					process.StartInfo.RedirectStandardOutput = true;
					process.StartInfo.UseShellExecute = false;

					process.StartInfo.Arguments = "destroy /silent /noprompt " + fullPath;

					Console.WriteLine("Purging " + fullPath + " [" + lastModifiedDate.ToString("yyyy/MM/dd") + "]");

					process.Start();

					var error = process.StandardError.ReadToEnd();
					var output = process.StandardOutput.ReadToEnd();

					process.WaitForExit();

					if (!String.IsNullOrWhiteSpace(error))
						Console.WriteLine(error);

					if (!String.IsNullOrWhiteSpace(output))
						Console.WriteLine(output);
				}
				else
				{
					Console.WriteLine("Retaining " + fullPath + " [" + lastModifiedDate.ToString("yyyy/MM/dd") + "]");
				}

				historyProcessOutput.ToString();
			}



			//for (var i = 282; i < 364; i++)
			//{
			//	var process = new Process();
			//	process.StartInfo.WorkingDirectory = "C:\\Projects\\";
			//	process.StartInfo.FileName = "C:\\Program Files (x86)\\Common7\\IDE\\tf.exe";
			//	process.StartInfo.CreateNoWindow = true;
			//	process.StartInfo.RedirectStandardError = true;
			//	process.StartInfo.RedirectStandardOutput = true;
			//	process.StartInfo.UseShellExecute = false;

			//	process.StartInfo.Arguments = "destroy /silent /noprompt $/Tenaska.BuildRepository/VersionedBuilds/GMS/4.0." + i.ToString();

			//	process.Start();

			//	var error = process.StandardError.ReadToEnd();
			//	var output = process.StandardOutput.ReadToEnd();

			//	process.WaitForExit();

			//	if (!String.IsNullOrWhiteSpace(error))
			//		Console.WriteLine(error);

			//	if (!String.IsNullOrWhiteSpace(output))
			//		Console.WriteLine(output);
			//}

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("Press [Enter] to continue");
			Console.ReadLine();
		}
	}
}
