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

		public void Load ( Stream stream/*, string key = "dWriter"*/ )
		{
			/*using ( CryptoStream crypto = new CryptoStream ( stream, new RijndaelManaged ()
			{
				KeySize = 256,
				Key = Encoding.UTF8.GetBytes ( ( "!@#$%^&*" + key + "a2w3s4e5d6r" ).PadRight ( 32 ) ),
				Mode = CipherMode.CBC,
				Padding = PaddingMode.PKCS7,
				IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
				BlockSize = 128,
			}.CreateDecryptor (), CryptoStreamMode.Read ) )
			{*/
				//using ( DeflateStream deflate = new DeflateStream ( /*crypto*/stream, CompressionLevel.Optimal, true ) )
				//{
					Initialize ();
					pages.Clear ();

					BinaryReader reader = new BinaryReader ( /*deflate*/stream );
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
						MemoryStream tempStream = new MemoryStream ( Encoding.UTF8.GetBytes ( reader.ReadString () ) );
						FlowDocument flowDocument = new FlowDocument ();
						var textRange = new TextRange ( flowDocument.ContentStart, flowDocument.ContentEnd );
						textRange.Load ( tempStream, DataFormats.Rtf );
						page.Text = flowDocument;
						tempStream.Dispose ();

						pages.Add ( page );
					}
				//}
			//}
		}

		public void Save ( Stream stream/*, string key = "dWriter"*/ )
		{
			/*using ( CryptoStream crypto = new CryptoStream ( stream, new RijndaelManaged ()
			{
				KeySize = 256,
				Key = Encoding.UTF8.GetBytes ( ( "!@#$%^&*" + key + "a2w3s4e5d6r" ).PadRight ( 32 ) ),
				Mode = CipherMode.CBC,
				Padding = PaddingMode.PKCS7,
				IV = new byte [] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
				BlockSize = 128,
			}.CreateEncryptor (), CryptoStreamMode.Write ) )
			{*/
				//using ( DeflateStream deflate = new DeflateStream ( /*crypto*/stream, CompressionLevel.Optimal, true ) )
				//{
					BinaryWriter writer = new BinaryWriter ( /*deflate*/stream );
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
						MemoryStream tempStream = new MemoryStream ();
						textRange.Save ( tempStream, DataFormats.Rtf );
						string text = Encoding.UTF8.GetString ( tempStream.ToArray () );
						tempStream.Dispose ();
						writer.Write ( text );
					}
				//}
			//}
		}

		public void ExportToHTML ( Stream stream )
		{
			StreamWriter writer = new StreamWriter ( stream );
			writer.WriteLine ( string.Format ( "<!DOCTYPE html><html><head><meta charset='utf-8'><title>{0}</title></head><body><h1>{0}</h1>", Title ) );
			writer.WriteLine ( string.Format ( "<p>{0}</p>", Copyright.Replace ( "<", "&lt;" ).Replace ( ">", "&gt;" ) ) );
			writer.WriteLine ( string.Format ( "<p>Author: {0}</p>", string.Join ( ", ", Authors ).Replace ( "<", "&lt;" ).Replace ( ">", "&gt;" ) ) );

			writer.Flush ();

			var markupConverter = new MarkupConverter.MarkupConverter ();
			foreach ( var page in Pages )
			{
				writer.WriteLine ( string.Format ( "<hr><h2>{0}</h2>", page.Title.Replace ( "<", "&lt;" ).Replace ( ">", "&gt;" ) ) );
				var textRange = new TextRange ( page.Text.ContentStart, page.Text.ContentEnd );
				MemoryStream tempStream = new MemoryStream ();
				textRange.Save ( tempStream, DataFormats.Rtf );
				var rtf = Encoding.UTF8.GetString ( tempStream.ToArray () );
				tempStream.Dispose ();
				var html = markupConverter.ConvertRtfToHtml ( rtf );
				writer.WriteLine ( html );

				writer.Flush ();
			}

			writer.WriteLine ( "</body></html>" );
			writer.Flush ();
		}

		public void ExportToTXT ( Stream stream )
		{
			StreamWriter writer = new StreamWriter ( stream );
			writer.WriteLine ( Title );
			writer.WriteLine ( Copyright );
			writer.WriteLine ( string.Format ( "Author: {0}", string.Join ( ", ", Authors ) ) );
			writer.WriteLine ();

			writer.Flush ();

			foreach ( var page in Pages )
			{
				writer.WriteLine ( "================" );
				writer.WriteLine ( page.Title );
				writer.WriteLine ( "================" );
				MemoryStream tempStream = new MemoryStream ();
				var textRange = new TextRange ( page.Text.ContentStart, page.Text.ContentEnd );
				textRange.Save ( tempStream, DataFormats.Text );
				string text = Encoding.UTF8.GetString ( tempStream.ToArray () );
				tempStream.Dispose ();
				writer.Write ( text );
				writer.WriteLine ();

				writer.Flush ();
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
