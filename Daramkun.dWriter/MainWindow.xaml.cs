using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Flatcode.Presentation;
using Microsoft.Win32;

namespace Daramkun.dWriter
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{
		dWriterDocument document = new dWriterDocument ();
		bool isSaved = true;
		string savedPath = null;

		public static RoutedCommand CommandNewFile = new RoutedCommand ();
		public static RoutedCommand CommandOpenFile = new RoutedCommand ();
		public static RoutedCommand CommandSaveFile = new RoutedCommand ();

		public dWriterDocument Document { get { return document; } }

		public MainWindow ()
		{
			InitializeComponent ();

			ApplyTitlebar ();

			listPages.ItemsSource = document.Pages;
			listPages.SelectedIndex = 0;
		}

		private void Window_Closing ( object sender, System.ComponentModel.CancelEventArgs e )
		{
			if ( !isSaved )
			{
				var result = TaskDialog.ShowModal ( this, Globalizer.Strings[ "ask_from_dontsave_instruction" ],
					Globalizer.Strings [ "ask_from_dontsave_content" ], Globalizer.Strings [ "ask_from_dontsave_title" ], TaskDialogButtons.YesNoCancel );

				switch ( result.SelectedButton )
				{
					case 0:
						if ( savedPath == null )
						{
							SaveFileDialog saveFileDialog = new SaveFileDialog ();
							saveFileDialog.Filter = "dWriter file (*.dw)|*.dw";
							if ( saveFileDialog.ShowDialog ( this ) == true )
								savedPath = saveFileDialog.FileName;
							else return;
						}
						using ( FileStream stream = new FileStream ( savedPath, FileMode.Create, FileAccess.Write ) )
							document.Save ( stream );
						isSaved = true;
						break;

					case 2:
						e.Cancel = true;
						break;
				}
			}
		}

		private void Window_DragEnter ( object sender, DragEventArgs e )
		{
			if ( e.Data.GetDataPresent ( DataFormats.FileDrop ) )
			{
				e.Effects = DragDropEffects.None;
				e.Handled = true;
			}
		}

		private void Window_Drop ( object sender, DragEventArgs e )
		{
			if ( e.Data.GetDataPresent ( DataFormats.FileDrop ) )
			{
				List<string> temp = new List<string> ( e.Data.GetData ( DataFormats.FileDrop ) as string [] );
				for ( int i = 0; i < temp.Count; ++i )
				{
					var ext = System.IO.Path.GetExtension ( temp [ i ] ).ToLower ();
					if ( ext != ".dw" )
					{
						switch ( ext )
						{
							case ".png":
							case ".gif":
							case ".bmp":
							case ".jpg":
							case ".jpeg":
							case ".tif":
							case ".tiff":
								if ( textBoxText.IsEnabled == false ) continue;

								Paragraph para = new Paragraph ();
								BitmapImage bitmap = new BitmapImage ( new Uri ( temp [ i ] ) );
								Image image = new Image ();
								image.Source = bitmap;
								if ( bitmap.Width > 640 )
									image.Width = 640;
								para.Inlines.Add ( image );
								FlowDocument doc = new FlowDocument ();
								doc.Blocks.Add ( para );

								document.Insert ( document.Pages.Count, new dWriterPage ()
								{
									Title = System.IO.Path.GetFileName ( temp [ i ] ),
									Text = doc
								} );

								temp.RemoveAt ( i );
								--i;

								e.Handled = true;
								break;
						}
					}
					else if ( i == 0 )
					{
						if ( !isSaved )
						{
							var result = TaskDialog.ShowModal ( this, Globalizer.Strings [ "ask_from_dontsave_instruction" ],
								Globalizer.Strings [ "ask_from_dontsave_content" ], Globalizer.Strings [ "ask_from_dontsave_title" ],
								TaskDialogButtons.YesNoCancel );

							switch ( result.SelectedButton )
							{
								case 0:
									if ( savedPath == null )
									{
										SaveFileDialog saveFileDialog = new SaveFileDialog ();
										saveFileDialog.Filter = "dWriter file (*.dw)|*.dw";
										if ( saveFileDialog.ShowDialog ( this ) == true )
											savedPath = saveFileDialog.FileName;
										else return;
									}
									using ( FileStream stream = new FileStream ( savedPath, FileMode.Create, FileAccess.Write ) )
										document.Save ( stream );
									isSaved = true;
									break;

								case 2:
									return;
							}
						}

						document.Initialize ();
						ApplyTitlebar ();

						try
						{
							using ( FileStream stream = new FileStream ( temp [ 0 ], FileMode.Open, FileAccess.Read ) )
								document.Load ( stream );

							ApplyTitlebar ();
							savedPath = temp [ 0 ];
							isSaved = true;

							listPages.SelectedIndex = -1;
						}
						catch
						{
							savedPath = null;
							isSaved = true;

							TaskDialog.ShowModal ( this, Globalizer.Strings [ "notice_loading_error_instruction" ], 
								Globalizer.Strings [ "notice_loading_error_content" ], Globalizer.Strings [ "notice_loading_error_title" ],
								TaskDialogButtons.OK, TaskDialogIcon.Error );
						}
					}
					else
					{
						var process = new Process ();
						process.StartInfo.FileName = Process.GetCurrentProcess ().StartInfo.FileName;
						process.StartInfo.Arguments = temp [ i ];
						process.Start ();
					}
				}
			}
		}

		private void ApplyTitlebar ()
		{
			Title = $"{document.Title} - {Globalizer.Strings [ "dwriter" ]}";
		}

		private void Button_New_Click ( object sender, RoutedEventArgs e )
		{
			if ( !isSaved )
			{
				var result = TaskDialog.ShowModal ( this, Globalizer.Strings [ "ask_from_dontsave_instruction" ],
					Globalizer.Strings [ "ask_from_dontsave_content" ], Globalizer.Strings [ "ask_from_dontsave_title" ], TaskDialogButtons.YesNoCancel );

				switch ( result.SelectedButton )
				{
					case 0:
						if ( savedPath == null )
						{
							SaveFileDialog saveFileDialog = new SaveFileDialog ();
							saveFileDialog.Filter = "dWriter file (*.dw)|*.dw";
							if ( saveFileDialog.ShowDialog ( this ) == true )
								savedPath = saveFileDialog.FileName;
							else return;
						}
						using ( FileStream stream = new FileStream ( savedPath, FileMode.Create, FileAccess.Write ) )
							document.Save ( stream );
						isSaved = true;
						break;

					case 2:
						return;
				}
			}

			document.Initialize ();
			ApplyTitlebar ();

			isSaved = true;
			savedPath = null;

			listPages.SelectedIndex = -1;
		}

		private void Button_Open_Click ( object sender, RoutedEventArgs e )
		{
			if ( !isSaved )
			{
				var result = TaskDialog.ShowModal ( this, Globalizer.Strings [ "ask_from_dontsave_instruction" ],
					Globalizer.Strings [ "ask_from_dontsave_content" ], Globalizer.Strings [ "ask_from_dontsave_title" ], TaskDialogButtons.YesNoCancel );

				switch ( result.SelectedButton )
				{
					case 0:
						if ( savedPath == null )
						{
							SaveFileDialog saveFileDialog = new SaveFileDialog ();
							saveFileDialog.Filter = "dWriter file (*.dw)|*.dw";
							if ( saveFileDialog.ShowDialog ( this ) == true )
								savedPath = saveFileDialog.FileName;
							else return;
						}
						using ( FileStream stream = new FileStream ( savedPath, FileMode.Create, FileAccess.Write ) )
							document.Save ( stream );
						isSaved = true;
						break;

					case 2:
						return;
				}
			}

			OpenFileDialog openFileDialog = new OpenFileDialog ();
			openFileDialog.Filter = "dWriter file (*.dw)|*.dw";
			if ( openFileDialog.ShowDialog ( this ) == true )
			{
				document.Initialize ();
				ApplyTitlebar ();

				try
				{
					using ( FileStream stream = new FileStream ( openFileDialog.FileName, FileMode.Open, FileAccess.Read ) )
						document.Load ( stream );

					ApplyTitlebar ();
					savedPath = openFileDialog.FileName;
					isSaved = true;

					listPages.SelectedIndex = -1;
				}
				catch 
				{
					savedPath = null;
					isSaved = true;

					TaskDialog.ShowModal ( this, Globalizer.Strings [ "notice_loading_error_instruction" ],
						Globalizer.Strings [ "notice_loading_error_content" ], Globalizer.Strings [ "notice_loading_error_title" ],
						TaskDialogButtons.OK, TaskDialogIcon.Error );
				}
			}
		}

		private void Button_Save_Click ( object sender, RoutedEventArgs e )
		{
			if ( savedPath == null )
			{
				SaveFileDialog saveFileDialog = new SaveFileDialog ();
				saveFileDialog.Filter = "dWriter file (*.dw)|*.dw";
				if ( saveFileDialog.ShowDialog ( this ) == true )
					savedPath = saveFileDialog.FileName;
				else return;
			}

			using ( FileStream stream = new FileStream ( savedPath, FileMode.Create, FileAccess.Write ) )
				document.Save ( stream );
			isSaved = true;
		}

		private void commandNewFile_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			Button_New_Click ( sender, e );
		}

		private void commandOpenFile_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			Button_Open_Click ( sender, e );
		}

		private void commandSaveFile_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			Button_Save_Click ( sender, e );
		}

		private void Button_TextAlign_Left_Click ( object sender, RoutedEventArgs e )
		{
			if ( !textBoxText.IsEnabled ) return;
			textBoxText.SelectionToLeftAlign ();
		}

		private void Button_TextAlign_Center_Click ( object sender, RoutedEventArgs e )
		{
			if ( !textBoxText.IsEnabled ) return;
			textBoxText.SelectionToCenterAlign ();
		}

		private void Button_TextAlign_Right_Click ( object sender, RoutedEventArgs e )
		{
			if ( !textBoxText.IsEnabled ) return;
			textBoxText.SelectionToRightAlign ();
		}

		private void Button_TextStyle_Bold_Click ( object sender, RoutedEventArgs e )
		{
			if ( !textBoxText.IsEnabled ) return;
			textBoxText.SelectionToBold ();
		}

		private void Button_TextStyle_Italic_Click ( object sender, RoutedEventArgs e )
		{
			if ( !textBoxText.IsEnabled ) return;
			textBoxText.SelectionToItalic ();
		}

		private void Button_TextStyle_CancelLine_Click ( object sender, RoutedEventArgs e )
		{
			if ( !textBoxText.IsEnabled ) return;
			textBoxText.SelectionToStrikethrough ();
		}

		private void Button_TextStyle_Underline_Click ( object sender, RoutedEventArgs e )
		{
			if ( !textBoxText.IsEnabled ) return;
			textBoxText.SelectionToUnderline ();
		}

		private void Button_TextStyle_Superscript_Click ( object sender, RoutedEventArgs e )
		{
			if ( !textBoxText.IsEnabled ) return;
			textBoxText.SelectionToSuperscript ();
		}

		private void Button_TextStyle_Subscript_Click ( object sender, RoutedEventArgs e )
		{
			if ( !textBoxText.IsEnabled ) return;
			textBoxText.SelectionToSubscript ();
		}

		private void Button_TextStyle_Normal_Click ( object sender, RoutedEventArgs e )
		{
			if ( !textBoxText.IsEnabled ) return;
			textBoxText.SelectionToNormal ();
		}

		private void Button_AddPage_Click ( object sender, RoutedEventArgs e )
		{
			document.AddPage ( listPages.SelectedIndex );
			isSaved = false;

			ICollectionView view = CollectionViewSource.GetDefaultView ( listPages.ItemsSource );
			view.Refresh ();
		}

		private void Button_DeletePage_Click ( object sender, RoutedEventArgs e )
		{
			if ( listPages.SelectedItem == null ) return;

			if ( TaskDialog.ShowModal ( this, Globalizer.Strings[ "ask_from_deletepage_instruction" ], Globalizer.Strings [ "ask_from_deletepage_content" ],
				Globalizer.Strings [ "ask_from_deletepage_title" ], TaskDialogButtons.YesNo, TaskDialogIcon.Warning ).SelectedButton == 6 )
			{
				document.RemovePage ( listPages.SelectedItem as dWriterPage );
				isSaved = false;

				ICollectionView view = CollectionViewSource.GetDefaultView ( listPages.ItemsSource );
				view.Refresh ();	
			}
		}

		private void Button_EditDocInfo_Click ( object sender, RoutedEventArgs e )
		{
			var infoWindow = new InfoWindow ( document );
			if ( infoWindow.ShowDialog () == true )
			{
				document.Title = infoWindow.DocumentTitle;
				document.Copyright = infoWindow.Copyright;
				document.Authors.Clear ();
				foreach ( var author in infoWindow.Authors )
					document.Authors.Add ( author );

				ApplyTitlebar ();

				isSaved = false;
			}
		}

		private void Button_Export_Click ( object sender, RoutedEventArgs e )
		{
			( sender as Button ).ContextMenu.IsEnabled = true;
			( sender as Button ).ContextMenu.PlacementTarget = ( sender as Button );
			( sender as Button ).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
			( sender as Button ).ContextMenu.IsOpen = true;
		}

		private void MenuItem_Export_PDF ( object sender, RoutedEventArgs e )
		{

		}

		private void MenuItem_Export_RTF ( object sender, RoutedEventArgs e )
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog ();
			saveFileDialog.Filter = "RTF file (*.rtf)|*.rtf";
			if ( saveFileDialog.ShowDialog ( this ) == true )
			{
				using ( FileStream stream = new FileStream ( saveFileDialog.FileName, FileMode.Create, FileAccess.Write ) )
					document.ExportToRTF ( stream );
			}
		}

		private void MenuItem_Export_HTML ( object sender, RoutedEventArgs e )
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog ();
			saveFileDialog.Filter = "HTML file (*.html;*.htm)|*.html;*.htm";
			if ( saveFileDialog.ShowDialog ( this ) == true )
			{
				using ( FileStream stream = new FileStream ( saveFileDialog.FileName, FileMode.Create, FileAccess.Write ) )
					document.ExportToHTML ( stream );
			}
		}

		private void MenuItem_Export_TXT ( object sender, RoutedEventArgs e )
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog ();
			saveFileDialog.Filter = "Text file (*.txt)|*.txt";
			if ( saveFileDialog.ShowDialog ( this ) == true )
			{
				using ( FileStream stream = new FileStream ( saveFileDialog.FileName, FileMode.Create, FileAccess.Write ) )
					document.ExportToTXT ( stream );
			}
		}

		private void listPages_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( listPages.SelectedIndex != -1 )
			{
				bool temp = isSaved;
				textBoxTitle.DataContext = ( listPages.SelectedItem as dWriterPage );
				textBoxTitle.SetBinding ( TextBox.TextProperty, new Binding ( "Title" ) );
				textBoxTitle.IsReadOnly = false;
				textBoxText.Document = ( listPages.SelectedItem as dWriterPage ).Text;
				textBoxText.IsReadOnly = false;
				isSaved = temp;
			}
			else
			{
				bool temp = isSaved;
				textBoxTitle.DataContext = null;
				textBoxTitle.Text = "";
				textBoxTitle.IsReadOnly = true;
				textBoxText.Document = new FlowDocument ();
				textBoxText.IsReadOnly = true;
				isSaved = temp;
			}
		}

		private void textBoxText_TextChanged ( object sender, TextChangedEventArgs e )
		{
			isSaved = false;
		}

		private void textBoxText_DragOver ( object sender, DragEventArgs e )
		{
			if ( e.Data.GetDataPresent ( DataFormats.FileDrop ) )
			{
				e.Effects = DragDropEffects.Copy;
				e.Handled = true;
			}
		}

		private void textBoxText_Drop ( object sender, DragEventArgs e )
		{
			foreach ( var data in e.Data.GetData ( DataFormats.FileDrop ) as string [] )
			{
				switch ( System.IO.Path.GetExtension ( data ).ToLower () )
				{
					case ".png":
					case ".gif":
					case ".bmp":
					case ".jpg":
					case ".jpeg":
					case ".tif":
					case ".tiff":
						if ( textBoxText.IsEnabled == false ) continue;

						Paragraph para = new Paragraph ();
						BitmapImage bitmap = new BitmapImage ( new Uri ( data ) );
						Image image = new Image ();
						image.Source = bitmap;
						if ( bitmap.Width > 640 )
							image.Width = 640;
						para.Inlines.Add ( image );
						textBoxText.Document.Blocks.Add ( para );

						e.Handled = true;
						break;
				}
			}
		}
	}
}
