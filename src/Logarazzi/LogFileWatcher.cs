using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Text;
using System.Threading.Tasks;

namespace Logarazzi
{
	public class LogFileWatcher
	{
		public int DefaultMaxLineCount { get; set; } = 100;

		private FileSystemWatcher watcher = null;

		public string FilePath
		{
			get
			{
				if (watcher != null)
				{
					return watcher.Path;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				SetWatcher(value);
			}
		}

		private void SetWatcher(string filePath)
		{
			if (watcher != null || string.Equals(watcher.Path, filePath) == false)
			{
				RemoveWatcherEvents();
				watcher.Dispose();
				watcher = null;
			}

			watcher = GetNewFileSystemWatcher(filePath);
			AddWatcherEvents();
		}

		private FileSystemWatcher GetNewFileSystemWatcher(string filePath)
		{
			return new FileSystemWatcher(filePath);
		}

		#region Event handlers
		private void AddWatcherEvents()
		{
			RemoveWatcherEvents();

			watcher.Changed += watcher_Changed;
			watcher.Created += watcher_Created;
			watcher.Deleted += watcher_Deleted;
			watcher.Error += watcher_Error;
			watcher.Renamed += watcher_Renamed;
		}

		private void RemoveWatcherEvents()
		{
			watcher.Changed -= watcher_Changed;
			watcher.Created -= watcher_Created;
			watcher.Deleted -= watcher_Deleted;
			watcher.Error -= watcher_Error;
			watcher.Renamed -= watcher_Renamed;
		}

		private void watcher_Renamed(object sender, RenamedEventArgs e)
		{
			Console.WriteLine("watcher_Renamed");
		}

		private void watcher_Error(object sender, ErrorEventArgs e)
		{
			Console.WriteLine("watcher_Error");
		}

		private void watcher_Deleted(object sender, FileSystemEventArgs e)
		{
			Console.WriteLine("watcher_Deleted");
		}

		private void watcher_Created(object sender, FileSystemEventArgs e)
		{
			Console.WriteLine("watcher_Created");
		}

		private void watcher_Changed(object sender, FileSystemEventArgs e)
		{
			Console.WriteLine("watcher_Changed");
		}

		#endregion

		private List<string> GetLines(int start, int stop = -1)
		{
			List<string> result = new List<string>();

			if (stop == -1)
			{
				stop = DefaultMaxLineCount + start;
			}

			using (FileStream fileStream = new System.IO.FileStream(FilePath, FileMode.Open, FileAccess.Read))
			{
				using (StreamReader reader = new StreamReader(fileStream))
				{
					int currentLine = 0;

					while (currentLine < start && reader.EndOfStream == false)
					{
						reader.ReadLineAsync();
						currentLine++;
					}

					while (currentLine < stop && reader.EndOfStream == false)
					{
						reader.ReadLineAsync().ContinueWith((taskResult) => result.Add(taskResult.Result));
						currentLine++;
					}
				}
			}

			return result;
		}
	}
}
