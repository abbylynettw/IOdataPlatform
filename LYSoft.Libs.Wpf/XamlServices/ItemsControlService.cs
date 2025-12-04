﻿namespace LYSoft.Libs.Wpf.XamlServices;

/// <summary>
/// ItemsControl的XAML附加属性服务类
/// 提供ItemsControl控件的附加属性功能，用于简化枚举类型数据的绑定
/// 通过设置EnumType附加属性，自动将指定枚举类型的所有值绑定到ItemsControl的ItemsSource
/// 常用于ComboBox、ListBox、ListView等控件显示枚举选项的场景
/// </summary>
public static class ItemsControlService {
    /// <summary>
    /// 设置ItemsControl显示枚举类型内容的依赖属性
    /// 定义一个附加属性，用于在XAML中设置要显示的枚举类型
    /// 当该属性值发生变化时，会自动调用OnEnumTypeChanged方法更新ItemsSource
    /// </summary>
    public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.RegisterAttached("EnumType", typeof(Type), typeof(ItemsControlService), new(null, OnEnumTypeChanged));
    /// <summary>
    /// 枚举类型变化时的回调方法
    /// 当EnumType附加属性的值发生改变时自动调用，更新ItemsControl的ItemsSource
    /// 使用Enum.GetValues方法获取指定枚举类型的所有值并赋值给ItemsSource
    /// </summary>
    /// <param name="sender">触发事件的ItemsControl对象</param>
    /// <param name="e">包含新旧值的事件参数</param>
    private static void OnEnumTypeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => ((ItemsControl)sender).ItemsSource = Enum.GetValues((Type)e.NewValue);
    /// <summary>
    /// 设置ItemsControl显示的枚举类型内容
    /// 在代码中设置ItemsControl的EnumType附加属性值
    /// </summary>
    /// <param name="obj">要设置属性的ItemsControl对象</param>
    /// <param name="value">要显示的枚举类型</param>
    /// <example>
    /// 使用示例：
    /// <code>
    /// ItemsControlService.SetEnumType(myComboBox, typeof(MyEnum));
    /// </code>
    /// </example>
    public static void SetEnumType(ItemsControl obj, Type value) => obj.SetValue(EnumTypeProperty, value);
    /// <summary>
    /// 获取ItemsControl显示的枚举类型内容
    /// 从代码中获取ItemsControl的EnumType附加属性值
    /// </summary>
    /// <param name="obj">要获取属性的ItemsControl对象</param>
    /// <returns>返回当前设置的枚举类型</returns>
    /// <example>
    /// 使用示例：
    /// <code>
    /// var enumType = ItemsControlService.GetEnumType(myComboBox);
    /// </code>
    /// </example>
    public static Type GetEnumType(ItemsControl obj) => (Type)obj.GetValue(EnumTypeProperty);
}