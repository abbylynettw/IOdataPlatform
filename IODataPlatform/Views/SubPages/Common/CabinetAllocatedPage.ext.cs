﻿﻿﻿using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using IODataPlatform.Models.ExcelModels;

namespace IODataPlatform.Views.SubPages.Common;

public class BorderBrushValueConverter : IMultiValueConverter
{

    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value[0] != null && value[0] == value[1])
        {
            return Brushes.LightPink;
        }
        else
        {
            return new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)); // Application.Current.Resources["TextFillColorPrimaryBrush"];
        }
    }

    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

}

public class ChannelForegroundValueConverter : IMultiValueConverter
{

    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        var defaultBrush = Brushes.White;
        if (value[0] is not IoFullData point) { return defaultBrush; }
        if ($"{value[1]}" == "全部" && $"{value[2]}" == "全部") { return defaultBrush; }
        if ($"{value[1]}" != "全部" && point.LocalBoxNumber != $"{value[1]}") { return defaultBrush; }
        if ($"{value[2]}" != "全部" && point.PowerType != $"{value[2]}") { return defaultBrush; }
        return Brushes.Yellow;
    }

    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

}

public class PointToBrushConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var tag = value.As<IoFullData>();
        if (tag != null)
        {
            switch (tag.PointType)
            {
                case TagType.Normal:
                    return Brushes.Green; // 所有普通信号使用深绿色
                case TagType.Alarm:
                    return Brushes.Red;
                case TagType.BackUp:
                    return Brushes.Gray;
                case TagType.CommunicationReserved:
                    return Brushes.Orange; // 通讯预留信号使用橙色
                case TagType.AlarmReserved:
                    return Brushes.LightCoral; // 报警预留使用浅红色
                default:
                    break;
            }
        }
        return Brushes.LightGray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}