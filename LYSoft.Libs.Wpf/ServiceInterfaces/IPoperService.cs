﻿namespace LYSoft.Libs.Wpf.ServiceInterfaces;

/// <summary>
/// WPF应用程序弹窗服务接口
/// 提供统一的弹窗显示功能，支持展示内容和编辑对象的业务场景
/// 支持动态控件加载和泛型控件显示，适用于对话框、编辑窗口、详情查看等场景
/// 支持异步操作，提供用户确认结果的返回值
/// </summary>
public interface IPoperService {

    /// <summary>
    /// 弹窗显示指定的用户控件
    /// 使用提供的UserControl实例在弹窗中显示内容，支持传递业务数据对象
    /// </summary>
    /// <param name="control">要在弹窗中显示的用户控件对象</param>
    /// <param name="title">弹窗的标题文本，可为null</param>
    /// <param name="obj">要传递给控件的业务数据对象，可为null</param>
    /// <returns>返回表示异步操作的Task，结果为true表示用户确认，false表示用户取消</returns>
    public Task<bool> PopAsync(System.Windows.Controls.UserControl control, string? title = null, object? obj = null);

    /// <summary>
    /// 弹窗显示指定类型的用户控件
    /// 使用泛型参数指定UserControl类型，系统将自动创建实例并在弹窗中显示
    /// </summary>
    /// <typeparam name="T">要显示的UserControl类型</typeparam>
    /// <param name="title">弹窗的标题文本，可为null</param>
    /// <param name="obj">要传递给控件的业务数据对象，可为null</param>
    /// <returns>返回表示异步操作的Task，结果为true表示用户确认，false表示畈户取消</returns>
    /// <example>
    /// 使用示例：
    /// <code>
    /// var result = await poperService.PopAsync&lt;MyEditControl&gt;("编辑数据", dataModel);
    /// if (result) {
    ///     // 用户确认了操作
    /// }
    /// </code>
    /// </example>
    public Task<bool> PopAsync<T>(string? title = null, object? obj = null) where T : System.Windows.Controls.UserControl;


}