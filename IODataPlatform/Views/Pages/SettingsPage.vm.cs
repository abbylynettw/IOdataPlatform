﻿namespace IODataPlatform.Views.Pages;

/// <summary>
/// 设置页面视图模型类
/// 负责应用程序的全局设置管理，包括主题切换和版本信息显示
/// 支持多种主题模式（亮色、暗色、系统默认）之间的动态切换
/// 提供程序版本信息的实时显示和管理功能
/// </summary>
public partial class SettingsViewModel : ObservableObject, INavigationAware {
    /// <summary>页面初始化状态标记，确保初始化操作只执行一次</summary>
    private bool _isInitialized = false;

    /// <summary>应用程序版本信息字符串，包含程序名称和版本号</summary>
    [ObservableProperty]
    private string _appVersion = string.Empty;

    /// <summary>当前应用的主题模式，默认为未知状态</summary>
    [ObservableProperty]
    private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

    /// <summary>
    /// 页面导航到此页面时触发
    /// 首次访问时执行初始化操作，加载当前主题和版本信息
    /// </summary>
    public void OnNavigatedTo() {
        if (!_isInitialized) { InitializeViewModel(); }
    }

    /// <summary>
    /// 页面导航离开时触发
    /// 当前实现为空，预留用于后续设置保存操作
    /// </summary>
    public void OnNavigatedFrom() { }

    /// <summary>
    /// 初始化视图模型的私有方法
    /// 加载当前系统主题设置和应用程序版本信息
    /// 设置初始化完成标记，防止重复初始化
    /// </summary>
    private void InitializeViewModel() {
        CurrentTheme = ApplicationThemeManager.GetAppTheme();
        AppVersion = $"IODataPlatform - {GetAssemblyVersion()}";
        _isInitialized = true;
    }

    /// <summary>
    /// 获取当前程序集的版本号信息
    /// 通过反射读取程序集的Version属性
    /// 如果无法获取版本信息则返回空字符串
    /// </summary>
    /// <returns>版本号字符串或空字符串</returns>
    private static string GetAssemblyVersion() {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            ?? string.Empty;
    }

    /// <summary>
    /// 主题切换命令
    /// 接收用户选择的主题参数，应用新的主题并更新当前主题状态
    /// 如果新主题与当前主题相同则不执行任何操作
    /// </summary>
    /// <param name="parameter">用户选择的新主题模式</param>
    [RelayCommand]
    private void OnChangeTheme(ApplicationTheme parameter) {
        if (parameter == CurrentTheme) { return; }
        ApplicationThemeManager.Apply(parameter);
        CurrentTheme = parameter;
    }

    /// <summary>
    /// 当前主题属性变更时触发的部分方法
    /// 目前为空实现，预留用于主题变更后的额外处理逻辑
    /// 例如保存用户主题偏好或触发相关事件
    /// </summary>
    /// <param name="value">新的主题值</param>
    partial void OnCurrentThemeChanged(ApplicationTheme value) {
        
    }
}