﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace IODataPlatform.Utilities
{      
    /// <summary>
    /// 首页显示控制转换器
    /// 根据当前页面是否为“首页”来控制UI元素的显示或隐藏
    /// 当值为“首页”时返回Collapsed，其他情况返回Visible
    /// 广泛用于导航菜单和面包屑的条件显示控制
    /// </summary>
    [ValueConversion(typeof(object), typeof(bool))]
    public class HomePageToVisibilityConverter : IValueConverter
    {

        /// <summary>
        /// 正向转换：将输入值转换为可见性枚举值
        /// 当输入为“首页”时返回Collapsed（隐藏），其他情况返回Visible（显示）
        /// </summary>
        /// <param name="value">要转换的源值，期望为字符串类型</param>
        /// <param name="targetType">目标类型，应为Visibility枚举</param>
        /// <param name="parameter">转换参数（未使用）</param>
        /// <param name="culture">区域设置信息（未使用）</param>
        /// <returns>返回Visibility.Collapsed或Visibility.Visible</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() == "首页")
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        /// <summary>
        /// 反向转换：将Visibility枚举值转换回原始值
        /// 此转换器不支持反向转换，调用时会抛出异常
        /// </summary>
        /// <param name="value">要反向转换的值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">转换参数</param>
        /// <param name="culture">区域设置信息</param>
        /// <returns>不返回任何值，直接抛出异常</returns>
        /// <exception cref="NotImplementedException">始终抛出，表示不支持反向转换</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
