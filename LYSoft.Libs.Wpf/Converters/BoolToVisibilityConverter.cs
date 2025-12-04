using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LYSoft.Libs.Wpf.Converters
{
    /// <summary>
    /// 布尔值到可见性转换器
    /// 用于WPF数据绑定中将布尔值转换为Visibility枚举值，控制界面元素的显示和隐藏
    /// 支持正向和反向转换模式，适用于根据条件动态控制UI元素可见性的场景
    /// 常用于按钮启用状态、数据加载状态指示器、条件性内容显示等场景
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {

        /// <summary>
        /// 是否反向转换的标志
        /// 当为false时：true→Collapsed, false→Visible
        /// 当为true时：true→Visible, false→Collapsed
        /// 默认值为false，适用于大多数隐藏元素的场景
        /// </summary>
        public bool Inverse { get; set; } = false;

        /// <summary>
        /// 将布尔值转换为Visibility枚举值
        /// 根据Inverse属性决定转换逻辑，支持正向和反向转换模式
        /// </summary>
        /// <param name="value">要转换的布尔值</param>
        /// <param name="targetType">目标类型，通常为Visibility</param>
        /// <param name="parameter">转换参数（未使用）</param>
        /// <param name="culture">区域性信息（未使用）</param>
        /// <returns>返回对应的Visibility枚举值</returns>
        /// <exception cref="InvalidCastException">当value不能转换为bool类型时抛出</exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Inverse)
            {
                return (bool) value ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return (bool)value ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>
        /// 反向转换方法（不支持）
        /// 此转换器不支持从Visibility到bool的反向转换，仅用于单向绑定
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
