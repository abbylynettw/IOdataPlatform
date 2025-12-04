﻿﻿using System.Globalization;
using System.Windows.Data;

namespace LYSoft.Libs.Wpf.Converters;

/// <summary>
/// 布尔值和可见性转换器
/// 用于WPF数据绑定中将布尔值转换为Visibility枚举值，控制UI元素的显示和隐藏
/// 支持正向和反向转换模式，适用于条件性UI元素显示控制的场景
/// 支持双向数据绑定，可以从Visibility反向转换到布尔值
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BooleanToVisiblityConverter : IValueConverter {

    /// <summary>
    /// 是否反转转换器的标志
    /// 当为false时：true→Visible, false→Collapsed
    /// 当为true时：true→Collapsed, false→Visible
    /// 默认值为false，适用于大多数显示元素的场景
    /// </summary>
    public bool IsInverse { get; set; } = false;

    /// <summary>
    /// 将布尔值转换为Visibility枚举值
    /// 根据IsInverse属性决定转换逻辑，支持正向和反向转换模式
    /// </summary>
    /// <param name="value">要转换的布尔值</param>
    /// <param name="targetType">目标类型，通常为Visibility</param>
    /// <param name="parameter">转换参数（未使用）</param>
    /// <param name="culture">区域性信息（未使用）</param>
    /// <returns>返回对应的Visibility枚举值</returns>
    /// <exception cref="InvalidCastException">当value不能转换为bool类型时抛出</exception>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (IsInverse) {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        } else {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// 将Visibility枚举值反向转换为布尔值
    /// 支持从Visibility到bool的反向转换，适用于双向数据绑定场景
    /// </summary>
    /// <param name="value">要反向转换的Visibility值</param>
    /// <param name="targetType">目标类型，通常为bool</param>
    /// <param name="parameter">转换参数（未使用）</param>
    /// <param name="culture">区域性信息（未使用）</param>
    /// <returns>返回对应的布尔值</returns>
    /// <exception cref="InvalidCastException">当value不能转换为Visibility类型时抛出</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return (Visibility)value == Visibility.Visible;
    }

}

/// <summary>
/// 布尔值和可见性反向转换器
/// 专用于反向转换的转换器，将false转换为Visible，true转换为Collapsed
/// 适用于需要隐藏元素当条件为true的场景，如错误信息显示、加载指示器等
/// 支持双向数据绑定，可以从Visibility反向转换到布尔值
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BooleanToVisiblityReverseConverter : IValueConverter {

    /// <summary>
    /// 将布尔值反向转换为Visibility枚举值
    /// false→Visible, true→Collapsed，适用于需要反向显示逻辑的场景
    /// </summary>
    /// <param name="value">要转换的布尔值</param>
    /// <param name="targetType">目标类型，通常为Visibility</param>
    /// <param name="parameter">转换参数（未使用）</param>
    /// <param name="culture">区域性信息（未使用）</param>
    /// <returns>返回对应的Visibility枚举值</returns>
    /// <exception cref="InvalidCastException">当value不能转换为bool类型时抛出</exception>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return !(bool)value ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// 将Visibility枚举值反向转换为布尔值
    /// Visible→false, 其他→true，支持双向数据绑定
    /// </summary>
    /// <param name="value">要反向转换的Visibility值</param>
    /// <param name="targetType">目标类型，通常为bool</param>
    /// <param name="parameter">转换参数（未使用）</param>
    /// <param name="culture">区域性信息（未使用）</param>
    /// <returns>返回对应的布尔值</returns>
    /// <exception cref="InvalidCastException">当value不能转换为Visibility类型时抛出</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return (Visibility)value != Visibility.Visible;
    }

}