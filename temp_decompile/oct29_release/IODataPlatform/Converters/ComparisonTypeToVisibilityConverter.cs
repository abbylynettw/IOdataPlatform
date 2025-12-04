using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using IODataPlatform.Views.Pages;

namespace IODataPlatform.Converters;

/// <summary>
/// 将比较类型转换为按钮可见性
/// 仅修改行显示"详情"按钮，其他类型不显示
/// </summary>
public class ComparisonTypeToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is ComparisonType comparisonType)
		{
			return (comparisonType != ComparisonType.Modified) ? Visibility.Collapsed : Visibility.Visible;
		}
		return Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
