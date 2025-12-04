﻿namespace LYSoft.Libs.Wpf.Extensions;

/// <summary>
/// WPF控件相关的扩展方法类
/// 提供WPF控件对象的通用扩展方法，简化控件属性访问和类型转换操作
/// 支持泛型类型转换，提供类型安全的控件属性访问方式
/// 常用于Tag属性数据提取、控件类型转换等场景
/// </summary>
public static partial class Extension {

    /// <summary>
    /// 获取控件对象Tag属性中的内容
    /// 将控件对象转换为FrameworkElement，然后获取其Tag属性并转换为指定类型
    /// 提供类型安全的Tag属性访问，适用于在Tag中存储业务数据的场景
    /// </summary>
    /// <typeparam name="T">目标类型，必须是引用类型（class）</typeparam>
    /// <param name="control">要获取Tag属性的控件对象</param>
    /// <returns>返回Tag属性中的内容，转换为指定类型，如果转换失败则返回null</returns>
    /// <example>
    /// 使用示例：
    /// <code>
    /// var data = button.GetTag&lt;MyDataModel&gt;();
    /// if (data != null) {
    ///     // 处理业务数据
    /// }
    /// </code>
    /// </example>
    public static T? GetTag<T>(this object control) where T : class {
        return control.As<FrameworkElement>()?.Tag.As<T>();
    }

}