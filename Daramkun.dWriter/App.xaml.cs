using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Flatcode.Presentation;

namespace Daramkun.dWriter
{
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application
	{
		string [] args;

		protected override void OnStartup ( StartupEventArgs e )
		{
			args = e.Args;
			base.OnStartup ( e );
		}

		protected override void OnActivated ( EventArgs e )
		{
			if ( args != null && args.Length == 1 )
			{
				try
				{
					using ( FileStream stream = new FileStream ( args [ 0 ], FileMode.Open, FileAccess.Read ) )
						( MainWindow as MainWindow ).Document.Load ( stream );
				}
				catch
				{
					TaskDialog.ShowModal ( MainWindow, "Loading fail!", "This file is invalid.", "ERROR", TaskDialogButtons.OK, TaskDialogIcon.Error );
					MainWindow.Close ();
				}
				args = null;
			}

			base.OnActivated ( e );
		}
	}
}
