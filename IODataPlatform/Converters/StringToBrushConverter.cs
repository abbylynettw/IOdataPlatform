using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace IODataPlatform.Converters;

/// <summary>
/// 字符串转画刷转换器
/// 将颜色字符串（如 "#FF0000" 或 "Red"）转换为 Brush 对象
/// </summary>
public class StringToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text && !string.IsNullOrEmpty(text))
        {
            try
            {
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
