using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Daramkun.dWriter
{
	public class PageNumberConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			ListViewItem item = ( ListViewItem ) value;
			ListView listView = ItemsControl.ItemsControlFromItemContainer ( item ) as ListView;
			int index = listView.ItemContainerGenerator.IndexFromContainer ( item );
			return string.Format ( "{0:000}", index + 1 );
		}

		public object ConvertBack ( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			throw new NotImplementedException ();
		}
	}
}
