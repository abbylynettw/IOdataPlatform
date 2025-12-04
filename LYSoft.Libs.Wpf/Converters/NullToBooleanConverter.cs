﻿using System.Globalization;
using System.Windows.Data;

namespace LYSoft.Libs.Wpf.Converters;

/// <summary>
/// 空引用到布尔值转换器
/// 用于WPF数据绑定中根据对象是否为null来转换为布尔值
/// 支持正向和反向转换模式，适用于数据验证、加载状态判断、条件显示控制等场景
/// 常用于表单验证、数据加载状态、空值判断等业务场景
/// </summary>
[ValueConversion(typeof(object), typeof(bool))]
public class NullToBooleanConverter : IValueConverter {

    /// <summary>
    /// 是否反转转换器的标志
    /// 当为false时：null→true, 非null→false
    /// 当为true时：null→false, 非null→true
    /// 默认值为false，适用于检测空值并返回true的场景
    /// </summary>
    public bool IsInverse { get; set; } = false;

    /// <summary>
    /// 将对象的空引用状态转换为布尔值
    /// 使用is null模式匹配判断空值，根据IsInverse属性决定返回结果
    /// </summary>
    /// <param name="value">要检测的对象</param>
    /// <param name="targetType">目标类型，通常为bool</param>
    /// <param name="parameter">转换参数（未使用）</param>
    /// <param name="culture">区域性信息（未使用）</param>
    /// <returns>返回空值判断的布尔结果，可能经过IsInverse反转</returns>
    /// <remarks>
    /// 使用异或(^)操作符实现IsInverse功能。
    /// </remarks>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is null ^ IsInverse;
    }

    /// <summary>
    /// 反向转换方法（不支持）
    /// 此转换器不支持从布尔值到对象的反向转换，仅用于单向数据绑定
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