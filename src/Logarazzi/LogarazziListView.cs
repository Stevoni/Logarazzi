using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Logarazzi
{
	public partial class LogarazziListView : ListView
	{
		
		private LogFileWatcher watcher;

		public LogarazziListView() : base()
		{
			InitializeComponent();
			AddHandlers();
		}

		private void AddHandlers()
		{
			RemoveHandlers();	
		
				
		}

		private void RemoveHandlers()
		{

		}

		public LogFileWatcher Watcher
		{
			get
			{
				return watcher;
			}
			set
			{
				UpdateWatcher(value);
			}
		}

		private void UpdateWatcher(LogFileWatcher value)
		{
			if (watcher != null)
			{
				RemoveWatcherHandlers(watcher);
			}

			watcher = value;
			AddWatcherHandlers(value);
		}


		private void AddWatcherHandlers(LogFileWatcher watcher)
		{
			RemoveWatcherHandlers(watcher);

		}

		private void RemoveWatcherHandlers(LogFileWatcher watcher)
		{

		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
		}

	}
}
