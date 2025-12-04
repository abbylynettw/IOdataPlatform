using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IODataPlatform.Views.Pages;

internal class CategoryToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (!($"{value}" == "电缆")) ? Visibility.Collapsed : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
