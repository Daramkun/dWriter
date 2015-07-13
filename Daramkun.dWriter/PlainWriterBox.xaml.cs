using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Daramkun.dWriter
{
	/// <summary>
	/// PlainWriterBox.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class PlainWriterBox : UserControl
	{
		[Bindable ( true )]
		public FlowDocument Document
		{
			get { return textBox.Document; }
			set
			{
				textBox.Document = value;
				foreach ( var block in textBox.Document.Blocks )
				{
					if ( block is Paragraph )
					{
						( block as Paragraph ).TextIndent = 10;
						( block as Paragraph ).LineHeight = 8;
						( block as Paragraph ).Margin = new Thickness ( 0, 0, 0, 8 );
					}
				}
			}
		}
		[Bindable ( true )]
		public bool IsReadOnly { get { return textBox.IsReadOnly; } set { textBox.IsReadOnly = value; } }

		public event TextChangedEventHandler TextChanged { add { textBox.TextChanged += value; } remove { textBox.TextChanged -= value; } }

		public PlainWriterBox ()
		{
			InitializeComponent ();
		}

		public void SelectionToBold ()
		{
			textBox.Selection.ApplyPropertyValue ( Paragraph.FontWeightProperty, FontWeights.Bold );
		}

		public void SelectionToItalic ()
		{
			textBox.Selection.ApplyPropertyValue ( Paragraph.FontStyleProperty, FontStyles.Italic );
		}

		public void SelectionToStrikethrough ()
		{
			textBox.Selection.ApplyPropertyValue ( Paragraph.TextDecorationsProperty, TextDecorations.Strikethrough );
		}

		public void SelectionToUnderline ()
		{
			textBox.Selection.ApplyPropertyValue ( Paragraph.TextDecorationsProperty, TextDecorations.Underline );
		}

		public void SelectionToSuperscript ()
		{
			textBox.Selection.ApplyPropertyValue ( Inline.BaselineAlignmentProperty, BaselineAlignment.Superscript );
			textBox.Selection.ApplyPropertyValue ( Paragraph.FontSizeProperty, 8.0 );
		}

		public void SelectionToSubscript ()
		{
			textBox.Selection.ApplyPropertyValue ( Inline.BaselineAlignmentProperty, BaselineAlignment.Subscript );
			textBox.Selection.ApplyPropertyValue ( Paragraph.FontSizeProperty, 8.0 );
		}

		public void SelectionToNormal ()
		{
			textBox.Selection.ApplyPropertyValue ( Paragraph.FontWeightProperty, FontWeights.Normal );
			textBox.Selection.ApplyPropertyValue ( Paragraph.FontStyleProperty, FontStyles.Normal );
			textBox.Selection.ApplyPropertyValue ( Paragraph.FontSizeProperty, 14.0 );
			textBox.Selection.ApplyPropertyValue ( Paragraph.TextDecorationsProperty, null );
			textBox.Selection.ApplyPropertyValue ( Inline.BaselineAlignmentProperty, BaselineAlignment.Baseline );
		}

		public void SelectionToLeftAlign ()
		{
			textBox.Selection.ApplyPropertyValue ( Paragraph.TextAlignmentProperty, TextAlignment.Left );
		}

		public void SelectionToCenterAlign ()
		{
			textBox.Selection.ApplyPropertyValue ( Paragraph.TextAlignmentProperty, TextAlignment.Center );
		}

		public void SelectionToRightAlign ()
		{
			textBox.Selection.ApplyPropertyValue ( Paragraph.TextAlignmentProperty, TextAlignment.Right );
		}

		private Paragraph GetPreviousParagraph ( Paragraph paragraph )
		{
			Block prev = null;
			foreach ( Block block in textBox.Document.Blocks )
			{
				if ( block == paragraph ) break;
				if ( block is Paragraph )
					prev = block;
			}
			return prev as Paragraph;
		}

		private void textBox_KeyDown ( object sender, KeyEventArgs e )
		{
			if ( e.Key == Key.Back )
			{
				if ( textBox.Selection.Start.Paragraph == null )
					textBox.Document.Blocks.Add ( new Paragraph () );
				if ( textBox.Selection.Start.Paragraph.TextIndent == 0 )
				{
					if ( textBox.Document.Blocks.Count > 1 )
					{
						Paragraph prev = GetPreviousParagraph ( textBox.Selection.Start.Paragraph );
						new TextRange ( prev.ContentEnd, textBox.Selection.Start ).Text = "";
						textBox.Selection.Start.Paragraph.TextIndent = 10;
						textBox.Selection.Start.Paragraph.LineHeight = 8;
					}
					else
					{
						( textBox.Document.Blocks.First () as Paragraph ).TextIndent = 10;
						( textBox.Document.Blocks.First () as Paragraph ).LineHeight = 10;
						( textBox.Document.Blocks.First () as Paragraph ).Margin = new Thickness ( 0, 0, 0, 8 );
					}
				}
			}
		}
	}
}
