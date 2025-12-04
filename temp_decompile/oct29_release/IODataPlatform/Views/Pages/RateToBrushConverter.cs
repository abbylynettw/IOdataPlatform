using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace IODataPlatform.Views.Pages;

public class RateToBrushConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		int num = (int)(double)values[0];
		int num2 = (int)values[1];
		if (num >= num2)
		{
			return (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
		}
		return Brushes.Red;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
