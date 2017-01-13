using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logarazzi
{
	public class FileUpdatedEventArgs : EventArgs
	{
		public FileUpdatedEventArgs(string fileName, UpdateTypes updateType) : base()
		{
			FileName = fileName;
			UpdateType = updateType;
		}	

		public string FileName { get; private set; }

		public UpdateTypes UpdateType { get; private set; }

		public enum UpdateTypes
		{
			Created,
			Renamed,
			Deleted,
			Modified
		}
	}
}
