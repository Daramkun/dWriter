using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Daramkun.dWriter
{
	public class dWriterPage : INotifyPropertyChanged
	{
		string _title;

		public string Title { get { return _title; } set { _title = value; PC ( "Title" ); } }
		public FlowDocument Text { get; set; }
		public DateTime Created { get; set; }
		public DateTime Modified { get; set; }

		public dWriterPage () { Title = ""; Text = new FlowDocument (); Created = Modified = DateTime.Now; }

		private void PC ( string name ) { if ( PropertyChanged != null ) PropertyChanged ( this, new PropertyChangedEventArgs ( name ) ); }
		public event PropertyChangedEventHandler PropertyChanged;
	}

	public class dWriterDocument : INotifyPropertyChanged
	{
		string _title, _copyright;

		public string Title { get { return _title; } set { _title = value; PC ( "Title" ); } }
		public ObservableCollection<string> Authors { get; private set; }
		public string Copyright { get { return _copyright; } set { _copyright = value; PC ( "Copyright" ); } }

		ObservableCollection<dWriterPage> pages;
		public IReadOnlyCollection<dWriterPage> Pages { get { return pages; } }

		public dWriterDocument ()
		{
			Authors = new ObservableCollection<string> ();
			pages = new ObservableCollection<dWriterPage> ();

			Initialize ();
		}

		public void Initialize ()
		{
			Title = "Untitled";
			Authors.Clear ();
			pages.Clear ();
			Copyright = string.Format ( "Copyright ⓒ {0} <Author> All Rights Reserved.", DateTime.Today.Year );

			AddPage ();
		}

		public void Load ( Stream stream, string key = "dWriter" )
		{
			using ( CryptoStream crypto = new CryptoStream ( stream, new AesCryptoServiceProvider ()
			{
				Key = Encoding.UTF8.GetBytes ( key ),
				IV = Encoding.UTF8.GetBytes ( "DARAMWORLD" )
			}.CreateDecryptor (), CryptoStreamMode.Read ) )
			{
				using ( DeflateStream deflate = new DeflateStream ( crypto, CompressionLevel.Optimal, true ) )
				{
					BinaryReader reader = new BinaryReader ( deflate );
					Title = reader.ReadString ();
					int authorCount = reader.ReadInt32 ();
					for ( int i = 0; i < authorCount; ++i )
						Authors.Add ( reader.ReadString () );
					Copyright = reader.ReadString ();

					int pageCount = reader.ReadInt32 ();
					for ( int i = 0; i < pageCount; ++i )
					{
						dWriterPage page = new dWriterPage ();
						page.Title = reader.ReadString ();
						page.Created = DateTime.Parse ( reader.ReadString () );
						page.Modified = DateTime.Parse ( reader.ReadString () );
						FlowDocument flowDocument = new FlowDocument ();
						var textRange = new TextRange ( flowDocument.ContentStart, flowDocument.ContentEnd );
						textRange.Load ( stream, DataFormats.Xaml );
						page.Text = flowDocument;
					}
				}
			}
		}

		public void Save ( Stream stream, string key = "dWriter" )
		{
			using ( CryptoStream crypto = new CryptoStream ( stream, new AesCryptoServiceProvider ()
			{
				Key = Encoding.UTF8.GetBytes ( key ),
				IV = Encoding.UTF8.GetBytes ( "DARAMWORLD" )
			}.CreateEncryptor (), CryptoStreamMode.Write ) )
			{
				using ( DeflateStream deflate = new DeflateStream ( crypto, CompressionLevel.Optimal, true ) )
				{
					BinaryWriter writer = new BinaryWriter ( deflate );
					writer.Write ( Title );
					writer.Write ( Authors.Count );
					foreach ( var author in Authors )
						writer.Write ( author );
					writer.Write ( Copyright );

					writer.Write ( Pages.Count );
					foreach ( var page in Pages )
					{
						writer.Write ( page.Title );
						writer.Write ( page.Created.ToString () );
						writer.Write ( page.Modified.ToString () );
						var textRange = new TextRange ( page.Text.ContentStart, page.Text.ContentEnd );
						textRange.Save ( stream, DataFormats.XamlPackage );
					}
				}
			}
		}

		public void AddPage ( int index = -1 )
		{
			if ( index == -1 ) pages.Add ( new dWriterPage () { Title = "New Page" } );
			else pages.Insert ( index + 1, new dWriterPage () { Title = "New Page" } );
		}

		public void RemovePage ( dWriterPage page ) { pages.Remove ( page ); }

		public void Insert ( int index, dWriterPage page )
		{
			pages.Insert ( index, page );
		}

		public void RemoveAt ( int index ) { pages.RemoveAt ( index ); }

		private void PC ( string name ) { if ( PropertyChanged != null ) PropertyChanged ( this, new PropertyChangedEventArgs ( name ) ); }
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
