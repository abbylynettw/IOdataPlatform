using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace IODataPlatform.Views.SubPages.Common;

public class BorderBrushValueConverter : IMultiValueConverter
{
	public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value[0] != null && value[0] == value[1])
		{
			return Brushes.LightPink;
		}
		return new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
	}

	public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
