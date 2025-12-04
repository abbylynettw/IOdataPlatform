using System;
using System.Globalization;
using System.Windows.Data;

namespace IODataPlatform.Views.SubPages.Common;

/// <summary>
/// bool值到切换按鑒文本转换器
/// </summary>
public class BoolToToggleButtonTextConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		bool flag = default(bool);
		int num;
		if (value is bool)
		{
			flag = (bool)value;
			num = 1;
		}
		else
		{
			num = 0;
		}
		if (((uint)num & (flag ? 1u : 0u)) == 0)
		{
			return "+";
		}
		return "-";
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
