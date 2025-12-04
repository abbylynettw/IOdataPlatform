﻿using System.Globalization;
using System.Windows.Data;

namespace LYSoft.Libs.Wpf.Converters;

/// <summary>
/// 相等性判断转布尔值转换器
/// 通过Equals方法判断值和参数是否相等，转换为布尔值结果
/// 需要通过ConverterParameter提供比较目标值，不支持ConvertBack，仅用于单向数据绑定
/// 常用于选中状态判断、条件显示、状态匹配等场景，如RadioButton选中状态等
/// </summary>
[ValueConversion(typeof(object), typeof(bool))]
public class EqualsToBooleanConverter : IValueConverter {

    /// <summary>
    /// 是否反转转换器的标志
    /// 当为false时：相等→true, 不相等→false
    /// 当为true时：相等→false, 不相等→true
    /// 默认值为false，适用于大多数相等性判断的场景
    /// </summary>
    public bool IsInverse { get; set; } = false;

    /// <summary>
    /// 将值与参数进行相等性判断并转换为布尔值
    /// 使用Equals方法比较value和parameter，根据IsInverse属性决定返回结果
    /// </summary>
    /// <param name="value">要比较的值</param>
    /// <param name="targetType">目标类型，通常为bool</param>
    /// <param name="parameter">用于比较的参数值，必须提供</param>
    /// <param name="culture">区域性信息（未使用）</param>
    /// <returns>返回比较结果的布尔值，可能经过IsInverse反转</returns>
    /// <remarks>
    /// 如果value或parameter为null，则返回false。
    /// 使用异或(^)操作符实现IsInverse功能。
    /// </remarks>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value == null) { return false; }
        if (parameter == null) { return false; }
        return value.Equals(parameter) ^ IsInverse;
    }

    /// <summary>
    /// 反向转换方法（不支持）
    /// 此转换器不支持从布尔值到原始值的反向转换，仅用于单向数据绑定
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