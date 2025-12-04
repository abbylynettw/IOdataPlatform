﻿using System.Globalization;
using System.Windows.Data;

namespace LYSoft.Libs.Wpf.Converters;

/// <summary>
/// 空引用到可见性转换器
/// 用于WPF数据绑定中根据对象是否为null来控制UI元素的显示和隐藏
/// 支持正向和反向转换模式，适用于数据加载状态、条件显示、空状态占位符等场景
/// 常用于加载中状态、数据验证、空内容提示等业务场景
/// </summary>
[ValueConversion(typeof(object), typeof(Visibility))]
public class NullToVisibilityConverter : IValueConverter {

    /// <summary>
    /// 是否反向转换的标志
    /// 当为false时：null→Collapsed, 非null→Visible
    /// 当为true时：null→Visible, 非null→Collapsed
    /// 默认值为false，适用于在有数据时显示元素的场景
    /// </summary>
    public bool Inverse { get; set; } = false;

    /// <summary>
    /// 将对象的空引用状态转换为Visibility枚举值
    /// 根据对象是否为null和Inverse属性决定可见性
    /// </summary>
    /// <param name="value">要检测的对象</param>
    /// <param name="targetType">目标类型，通常为Visibility</param>
    /// <param name="parameter">转换参数（未使用）</param>
    /// <param name="culture">区域性信息（未使用）</param>
    /// <returns>返回对应的Visibility枚举值</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (Inverse) {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        } else {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    /// <summary>
    /// 反向转换方法（不支持）
    /// 此转换器不支持从Visibility到对象的反向转换，仅用于单向数据绑定
    /// </summary>
    /// <param name="value">要反向转换的值</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">转换参数</param>
    /// <param name="culture">区域性信息</param>
    /// <returns>不返回任何值</returns>
    /// <exception cref="NotImplementedException">调用此方法时总是抛出</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
