using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace IODataPlatform.Views.Pages;

public class BoolToBrushConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if ((bool)value)
		{
			return Application.Current.Resources["TextFillColorPrimaryBrush"];
		}
		return Brushes.Red;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
