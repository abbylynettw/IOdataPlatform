using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace IODataPlatform.Converters;

public class StringToBrushConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string text && !string.IsNullOrEmpty(text))
		{
			try
			{
				text.StartsWith("#");
				return new BrushConverter().ConvertFromString(text);
			}
			catch
			{
				return Brushes.Transparent;
			}
		}
		return Brushes.Transparent;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
