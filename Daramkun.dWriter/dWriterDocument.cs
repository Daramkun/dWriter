﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace Daramkun.dWriter
{
	public class dWriterPage : INotifyPropertyChanged
	{
		string _title;

		public string Title { get { return _title; } set { _title = value; PC (); } }
		public FlowDocument Text { get; set; }
		public DateTime Created { get; set; }
		public DateTime Modified { get; set; }

		public dWriterPage () { Title = ""; Text = new FlowDocument (); Created = Modified = DateTime.Now; }

		private void PC ( [CallerMemberName] string name = "" ) { PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( name ) ); }
		public event PropertyChangedEventHandler PropertyChanged;
	}

	public enum EncryptType
	{
		None,
		AES256,
		RSA,
	}

	public class dWriterDocument : INotifyPropertyChanged
	{
		string _title, _copyright;

		public string Title { get { return _title; } set { _title = value; PC (); } }
		public ObservableCollection<string> Authors { get; private set; }
		public string Copyright { get { return _copyright; } set { _copyright = value; PC (); } }

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
			Title = Globalizer.Strings [ "untitled" ];
			Authors.Clear ();
			pages.Clear ();
			Copyright = string.Format ( "Copyright ⓒ {0} <Author> All Rights Reserved.", DateTime.Today.Year );

			AddPage ();
		}

		public void Load ( Stream stream )
		{
			using ( SignatureStream signature = new SignatureStream ( "DWRT", stream ) )
			{
				using ( DeflateStream deflate = new DeflateStream ( stream, CompressionMode.Decompress, true ) )
				{
					Initialize ();
					pages.Clear ();

					BinaryReader reader = new BinaryReader ( deflate );
					switch ( reader.ReadByte () )
					{
						case 0:
							Title = reader.ReadString ();
							int authorCount = reader.ReadInt32 ();
							for ( int i = 0; i < authorCount; ++i )
								Authors.Add ( reader.ReadString () );
							Copyright = reader.ReadString ();

							int pageCount = reader.ReadInt32 ();
							for ( int i = 0; i < pageCount; ++i )
							{
								dWriterPage page = new dWriterPage ()
								{
									Title = reader.ReadString (),
									Created = DateTime.Parse ( reader.ReadString () ),
									Modified = DateTime.Parse ( reader.ReadString () )
								};
								switch ( reader.ReadByte () )
								{
									case 0:
										MemoryStream tempStream = new MemoryStream ( Convert.FromBase64String ( reader.ReadString () ) );
										FlowDocument flowDocument = new FlowDocument ();
										var textRange = new TextRange ( flowDocument.ContentStart, flowDocument.ContentEnd );
										textRange.Load ( tempStream, DataFormats.Rtf );
										page.Text = flowDocument;
										tempStream.Dispose ();
										break;
								}

								pages.Add ( page );
							}
							break;
					}
				}
			}
		}

		public void Save ( Stream stream )
		{
			using ( SignatureStream signature = new SignatureStream ( "DWRT", stream ) )
			{
				using ( DeflateStream deflate = new DeflateStream ( stream, CompressionLevel.Optimal, true ) )
				{
					BinaryWriter writer = new BinaryWriter ( deflate );
					writer.Write ( ( byte ) 0 );

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
						writer.Write ( ( byte ) 0 );
						var textRange = new TextRange ( page.Text.ContentStart, page.Text.ContentEnd );
						MemoryStream tempStream = new MemoryStream ();
						textRange.Save ( tempStream, DataFormats.Rtf );
						string text = Convert.ToBase64String ( tempStream.ToArray () );
						tempStream.Dispose ();
						writer.Write ( text );
					}
				}
			}
		}

		public void ExportToHTML ( Stream stream )
		{
			StreamWriter writer = new StreamWriter ( stream );
			writer.WriteLine ( $"<!DOCTYPE html><html><head><meta charset='utf-8'><title>{Title}</title></head><body><h1>{Title}</h1>" );
			writer.WriteLine ( $"<p>{Copyright.Replace ( "<", "&lt;" ).Replace ( ">", "&gt;" )}</p>" );
			writer.WriteLine ( $"<p>Author: {string.Join ( ", ", Authors ).Replace ( "<", "&lt;" ).Replace ( ">", "&gt;" )}</p>" );

			writer.Flush ();

			var markupConverter = new MarkupConverter.MarkupConverter ();
			foreach ( var page in Pages )
			{
				writer.WriteLine ( $"<hr><h2>{page.Title.Replace ( "<", "&lt;" ).Replace ( ">", "&gt;" )}</h2>" );
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
			writer.WriteLine ( $"Author: {string.Join ( ", ", Authors )}" );
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

		public void ExportToRTF ( Stream stream )
		{
			FlowDocument tempDocument = new FlowDocument ();

			var titleParagraph = new Paragraph ( new Run ( Title ) )
			{
				TextAlignment = TextAlignment.Center,
				FontSize = 32
			};
			tempDocument.Blocks.Add ( titleParagraph );

			var copyrightParagraph = new Paragraph ( new Run ( Copyright ) )
			{
				TextAlignment = TextAlignment.Center,
				FontSize = 16
			};
			tempDocument.Blocks.Add ( copyrightParagraph );

			var authorsParagraph = new Paragraph ( new Run ( $"Author: {string.Join ( ", ", Authors )}" ) )
			{
				TextAlignment = TextAlignment.Center
			};
			tempDocument.Blocks.Add ( authorsParagraph );

			foreach ( var page in Pages )
			{
				var section = new Section ();

				var pageTitleParagraph = new Paragraph ( new Run ( page.Title ) )
				{
					FontSize = 24
				};
				section.Blocks.Add ( pageTitleParagraph );

				var pageTempParagraph = new Paragraph ( new Run () );
				section.Blocks.Add ( pageTempParagraph );

				TextRange range = new TextRange ( page.Text.ContentStart, page.Text.ContentEnd );
				MemoryStream tempStream = new MemoryStream ();
				System.Windows.Markup.XamlWriter.Save ( range, tempStream );
				range.Save ( tempStream, DataFormats.XamlPackage );
				TextRange range2 = new TextRange ( section.ContentEnd, section.ContentEnd );
				range2.Load ( tempStream, DataFormats.XamlPackage );

				tempDocument.Blocks.Add ( section );
			}

			StreamWriter writer = new StreamWriter ( stream );
			MemoryStream tempStream2 = new MemoryStream ();
			var textRange = new TextRange ( tempDocument.ContentStart, tempDocument.ContentEnd );
			textRange.Save ( tempStream2, DataFormats.Rtf );
			string text = Encoding.UTF8.GetString ( tempStream2.ToArray () );
			tempStream2.Dispose ();
			writer.Write ( text );
			writer.Flush ();
		}

		public void AddPage ( int index = -1 )
		{
			if ( index == -1 ) pages.Add ( new dWriterPage () { Title = Globalizer.Strings [ "new_page" ] } );
			else pages.Insert ( index + 1, new dWriterPage () { Title = Globalizer.Strings [ "new_page" ] } );
		}

		public void RemovePage ( dWriterPage page ) { pages.Remove ( page ); }

		public void Insert ( int index, dWriterPage page )
		{
			pages.Insert ( index, page );
		}

		public void RemoveAt ( int index ) { pages.RemoveAt ( index ); }

		private void PC ( [CallerMemberName] string name = "" ) { PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( name ) ); }
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
