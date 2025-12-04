using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IODataPlatform.Views.SubPages.Common;

/// <summary>
/// 修改类型到按鑒可见性转换器：仅修改行显示详情按鑒
/// </summary>
public class ModificationTypeToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (!(value is ChangeType changeType) || changeType != ChangeType.修改) ? Visibility.Collapsed : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
