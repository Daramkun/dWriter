using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace Daramkun.dWriter
{
	/// <summary>
	/// InfoWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class InfoWindow : Window
	{
		public string DocumentTitle { get { return textBoxTitle.Text; } }
		public string Copyright { get { return textBoxCopyright.Text; } }
		public ObservableCollection<string> Authors { get { return listBoxAuthor.ItemsSource as ObservableCollection<string>; } }

		public InfoWindow ( dWriterDocument document )
		{
			InitializeComponent ();

			textBoxTitle.Text = document.Title;
			textBoxCopyright.Text = document.Copyright;
			listBoxAuthor.ItemsSource = new ObservableCollection<string> ( document.Authors );
		}

		private void Button_Add_Author_Click ( object sender, RoutedEventArgs e )
		{
			if ( textBoxAuthor.Text.Trim () == "" ) return;
			( listBoxAuthor.ItemsSource as ObservableCollection<string> ).Add ( textBoxAuthor.Text );
			textBoxAuthor.Text = "";
		}

		private void Button_Del_Author_Click ( object sender, RoutedEventArgs e )
		{
			if ( listBoxAuthor.SelectedItem == null ) return;
			( listBoxAuthor.ItemsSource as ObservableCollection<string> ).Remove ( listBoxAuthor.SelectedItem as string );
		}

		private void Button_Apply_Click ( object sender, RoutedEventArgs e )
		{
			DialogResult = true;
		}

		private void Button_Cancel_Click ( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}
	}
}
