using Microsoft.VisualStudio.TestTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Logarazzi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Logarazzi.Tests
{
	[TestClass()]
	public class LogFileWatcherTests
	{
		private const string LogFileSample = "..\\..\\apache_access.csv";
		private static readonly List<string> createdLogFiles = new List<string>();
		private const string LogFileSampleLine1 = @"""70.69.152.165 - - [Fri Mar 04 19:39:27 UTC 2016] """"GET /_media/logo_my_v2.png HTTP/1.1"""" 304 6936 """"http://www.bing.com/search?q=sumo%20logic&src=IE-SearchBox&FORM=IE11SR"""" """"Mozilla/5.0 (Windows NT 6.2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1467.0 Safari/537.36""""""";
		private const string LogFileSampleLine100 = @"""5.35.225.115 - - [Fri Mar 04 23:08:54 UTC 2016] """"GET /_media/resource_thumb_video_cloud_based.jpg HTTP/1.1"""" 200 6904 """"http://www.bing.com/search?q=SIEM&src=IE-SearchBox&FORM=IE11SR"""" """"Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)""""""";
		[TestInitialize]
		private void Initialize()
		{
			Assert.IsTrue(System.IO.File.Exists(LogFileSample));
		}

		[TestCleanup]
		private void CleanupTests()
		{
			foreach (string tempFile in createdLogFiles)
			{
				DeleteFile(tempFile);
			}
		}

		private static void DeleteFile(string fileName)
		{
			try
			{
				File.Delete(fileName);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception occured while deleting file: {ex.Message}");
			}
			if (createdLogFiles.Contains(fileName) == true)
			{
				createdLogFiles.Remove(fileName);
			}
		}

		private static void MoveFile(string source, string destination)
		{
			//File.Move(source, destination);
			string directoryName = Path.GetDirectoryName(source);
			destination = Path.Combine(directoryName, destination);
			string backup = Path.Combine(directoryName, destination + ".bak");

			File.Create(destination).Close();
			File.Create(backup).Close();
			new FileInfo(source).Replace(destination, backup);

			if (createdLogFiles.Contains(source) == true)
			{
				createdLogFiles.Remove(source);
			}
			createdLogFiles.Add(destination);
			createdLogFiles.Add(backup);
		}

		private static void WriteLines(string filePath, List<string> linesToWrite)
		{
			if (File.Exists(filePath) == false)
			{
				File.WriteAllText(filePath, string.Join("\\r\\n", linesToWrite.ToArray()));
			}
			else
			{
				using (StreamWriter sw = new StreamWriter(filePath))
				{
					sw.AutoFlush = false;

					foreach (string line in linesToWrite)
					{
						sw.WriteLine(line);
					}

					sw.Flush();
				}
			}
		}

		private static string CreateSampleCopy()
		{
			return CreateSampleCopy(Path.GetTempFileName());
		}

		private static string CreateSampleCopy(string fileName)
		{
			string result = fileName;

			File.WriteAllText(result, File.ReadAllText(Path.GetFullPath(LogFileSample)));

			createdLogFiles.Add(result);

			return result;
		}


		[TestMethod()]
		public void LogFileWatcher_GetLines_Defaults()
		{
			using (LogFileWatcher watcher = new LogFileWatcher())
			{
				watcher.FilePath = CreateSampleCopy();
				List<string> testLines = watcher.GetLines(0);

				Assert.AreEqual(watcher.DefaultMaxLineCount, testLines.Count);
				Assert.AreEqual(LogFileSampleLine1, testLines[0]);
				Assert.AreEqual(LogFileSampleLine100, testLines[testLines.Count - 1]);
			}
		}

		[TestMethod]
		public void LogFileWatcher_FileUpdated_Created()
		{
			int counter = 0;

			using (LogFileWatcher watcher = new LogFileWatcher())
			{
				watcher.FilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

				watcher.FileUpdated += (sender, e) =>
				                       {
					                       if (e.UpdateType == FileUpdatedEventArgs.UpdateTypes.Created && e.FileName == watcher.FilePath && sender == watcher)
					                       {
						                       counter++;
					                       }
				                       };

				CreateSampleCopy(watcher.FilePath);
				System.Threading.Thread.Sleep(1000);
				Assert.AreEqual(1, counter);
			}
		}

		[TestMethod]
		public void LogFileWatcher_FileUpdated_Renamed()
		{
			int counter = 0;

			using (LogFileWatcher watcher = new LogFileWatcher())
			{
				watcher.FilePath = CreateSampleCopy();

				watcher.FileUpdated += (sender, e) =>
				                       {
					                       if (e.UpdateType == FileUpdatedEventArgs.UpdateTypes.Renamed && e.FileName == watcher.FilePath && sender == watcher)
					                       {
						                       counter++;
					                       }
				                       };

				MoveFile(watcher.FilePath, Path.GetRandomFileName());
				System.Threading.Thread.Sleep(1000);

				Assert.AreEqual(1, counter);
			}
		}

		[TestMethod]
		public void LogFileWatcher_FileUpdated_Deleted()
		{
			int counter = 0;

			using (LogFileWatcher watcher = new LogFileWatcher())
			{
				watcher.FilePath = CreateSampleCopy();

				watcher.FileUpdated += (sender, e) =>
				                       {
					                       if (e.UpdateType == FileUpdatedEventArgs.UpdateTypes.Deleted && e.FileName == watcher.FilePath && sender == watcher)
					                       {
						                       counter++;
					                       }
				                       };

				DeleteFile(watcher.FilePath);
				System.Threading.Thread.Sleep(1000);
				Assert.AreEqual(1, counter);
			}
		}

		[TestMethod]
		public void LogFileWatcher_FileUpdated_Modified()
		{
			int counter = 0;

			using (LogFileWatcher watcher = new LogFileWatcher())
			{
				watcher.FilePath = CreateSampleCopy();

				watcher.FileUpdated += (sender, e) =>
				                       {
					                       if (e.UpdateType == FileUpdatedEventArgs.UpdateTypes.Modified && e.FileName == watcher.FilePath && sender == watcher)
					                       {
						                       counter++;
					                       }
				                       };

				File.AppendAllText(watcher.FilePath, "New text");
				System.Threading.Thread.Sleep(1000);
				Assert.AreEqual(1, counter);
			}
		}
	}
}