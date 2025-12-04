using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using IODataPlatform.Models.ExcelModels;
using LYSoft.Libs;

namespace IODataPlatform.Views.SubPages.Common;

public class PointToBrushConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		IoFullData ioFullData = value.As<IoFullData>();
		if (ioFullData != null)
		{
			switch (ioFullData.PointType)
			{
			case TagType.Normal:
				if (ioFullData.NetType == "Net1")
				{
					return Brushes.Green;
				}
				return Brushes.LightGreen;
			case TagType.Alarm:
				return Brushes.Red;
			case TagType.BackUp:
				return Brushes.Gray;
			}
		}
		return Brushes.LightGray;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
