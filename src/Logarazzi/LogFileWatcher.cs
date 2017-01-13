using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Text;
using System.Threading.Tasks;

namespace Logarazzi
{
	public class LogFileWatcher : IDisposable
	{
		#region Events
		public event FileUpdatedHandler FileUpdated;

		public delegate void FileUpdatedHandler(object sender, FileUpdatedEventArgs e);

		#endregion
		private string lastLine = string.Empty;

		public int DefaultMaxLineCount { get; set; } = 100;

		private FileSystemWatcher watcher = null;

		public string FilePath
		{
			get
			{
				if (watcher != null)
				{
					return Path.Combine(watcher.Path, watcher.Filter);
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
			if (watcher != null && string.Equals(watcher.Path, filePath) == false)
			{
				RemoveWatcherEvents();
				watcher.Dispose();
				watcher = null;
			}

			watcher = GetNewFileSystemWatcher(filePath);
			watcher.BeginInit();
			AddWatcherEvents();
			watcher.EndInit();

		}

		private FileSystemWatcher GetNewFileSystemWatcher(string filePath)
		{
			FileSystemWatcher result = new FileSystemWatcher(Path.GetDirectoryName(filePath), Path.GetFileName(filePath))
			{
				EnableRaisingEvents = true,
				NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime 
			};
			
			return result;
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
			OnUpdate(FileUpdatedEventArgs.UpdateTypes.Renamed);
		}

		private void watcher_Error(object sender, ErrorEventArgs e)
		{
			Console.WriteLine("watcher_Error");
		}

		private void watcher_Deleted(object sender, FileSystemEventArgs e)
		{
			Console.WriteLine("watcher_Deleted");
			OnUpdate(FileUpdatedEventArgs.UpdateTypes.Deleted);
		}

		private void watcher_Created(object sender, FileSystemEventArgs e)
		{
			Console.WriteLine("watcher_Created");
			OnUpdate(FileUpdatedEventArgs.UpdateTypes.Created);
		}

		private void watcher_Changed(object sender, FileSystemEventArgs e)
		{
			Console.WriteLine("watcher_Changed");
			OnUpdate(FileUpdatedEventArgs.UpdateTypes.Modified);
		}

		private void OnUpdate(FileUpdatedEventArgs.UpdateTypes updateType)
		{
			if (FileUpdated != null)
			{
				FileUpdated(this, new FileUpdatedEventArgs(this.FilePath, updateType));
			}
		}

		#endregion

		private async Task<List<string>> InternalGetLines(int start, int stop = -1)
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
						await  reader.ReadLineAsync();
						currentLine++;
					}

					while (currentLine < stop && reader.EndOfStream == false)
					{
						await reader.ReadLineAsync().ContinueWith((taskResult) => result.Add(taskResult.Result));
						currentLine++;
					}
				}
			}

			return result;
		}

		public List<string> GetLines(int start, int stop = -1)
		{
			return InternalGetLines(start, stop).Result;
		}

		public void Dispose()
		{
			watcher?.Dispose();
		}
	}
}
