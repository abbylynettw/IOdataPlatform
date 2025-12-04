using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using IODataPlatform.Models.ExcelModels;

namespace IODataPlatform.Views.SubPages.Common;

/// <summary>
/// 通讯板卡可见性转换器
/// </summary>
public class CommunicationBoardVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BoardType boardType)
        {
            return boardType == BoardType.Communication ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
