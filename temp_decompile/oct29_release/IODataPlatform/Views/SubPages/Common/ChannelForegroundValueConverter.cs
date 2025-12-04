using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using IODataPlatform.Models.ExcelModels;

namespace IODataPlatform.Views.SubPages.Common;

public class ChannelForegroundValueConverter : IMultiValueConverter
{
	public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
	{
		SolidColorBrush white = Brushes.White;
		if (!(value[0] is IoFullData ioFullData))
		{
			return white;
		}
		if ($"{value[1]}" == "全部" && $"{value[2]}" == "全部")
		{
			return white;
		}
		if ($"{value[1]}" != "全部" && ioFullData.LocalBoxNumber != $"{value[1]}")
		{
			return white;
		}
		if ($"{value[2]}" != "全部" && ioFullData.PowerType != $"{value[2]}")
		{
			return white;
		}
		return Brushes.Yellow;
	}

	public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
