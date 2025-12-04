﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LYSoft.Libs.Wpf.Converters
{
    /// <summary>
    /// 空字符串到可见性转换器
    /// 用于WPF数据绑定中根据字符串是否为空来控制UI元素的显示和隐藏
    /// 支持正向和反向转换模式，适用于数据验证、内容显示控制、加载状态指示器等场景
    /// 常用于输入验证、数据加载提示、空状态占位符显示等场景
    /// </summary>
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class EmptyToVisibilityConverter : IValueConverter
    {

        /// <summary>
        /// 是否反向转换的标志
        /// 当为false时：空字符串→Collapsed, 非空字符串→Visible
        /// 当为true时：空字符串→Visible, 非空字符串→Collapsed
        /// 默认值为false，适用于在有内容时显示元素的场景
        /// </summary>
        public bool Inverse { get; set; } = false;

        /// <summary>
        /// 将字符串值转换为Visibility枚举值
        /// 根据字符串是否为null或空来决定可见性，支持正向和反向转换
        /// </summary>
        /// <param name="value">要转换的字符串值</param>
        /// <param name="targetType">目标类型，通常为Visibility</param>
        /// <param name="parameter">转换参数（未使用）</param>
        /// <param name="culture">区域性信息（未使用）</param>
        /// <returns>返回对应的Visibility枚举值</returns>
        /// <exception cref="NullReferenceException">当value为null且调用ToString()时可能抛出</exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Inverse)
            {
                return string.IsNullOrEmpty(value.ToString()) ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return string.IsNullOrEmpty(value.ToString()) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>
        /// 反向转换方法（不支持）
        /// 此转换器不支持从Visibility到字符串的反向转换，仅用于单向绑定
        /// </summary>
        /// <param name="value">要反向转换的值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">转换参数</param>
        /// <param name="culture">区域性信息</param>
        /// <returns>不返回任何值</returns>
        /// <exception cref="NotImplementedException">调用此方法时总是抛出</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
