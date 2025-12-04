﻿using System.Windows.Media;

namespace IODataPlatform.Models;

/// <summary>
/// 应用程序全局状态管理类
/// 负责管理状态栏显示文本、进度条、忙碌状态和状态颜色
/// 用于在UI中实时反馈操作状态和进度信息
/// </summary>
public partial class Status : ObservableObject {

    /// <summary>通用数据字段1，用于临时存储业务相关的对象</summary>
    public object? Data1 = null;

    /// <summary>通用数据字段2，用于临时存储业务相关的对象</summary>
    public object? Data2 = null;

    /// <summary>通用数据字段3，用于临时存储业务相关的对象</summary>
    public object? Data3 = null;

    /// <summary>状态栏显示的文本信息</summary>
    [ObservableProperty]
    private string statusText = "就绪";

    /// <summary>进度条的进度值，范围0-100</summary>
    [ObservableProperty]
    private double progress = 0;

    /// <summary>指示应用程序是否处于忙碌状态</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFree))]
    private bool isBusy = false;

    /// <summary>指示应用程序是否处于空闲状态（与IsBusy相反）</summary>
    public bool IsFree { get => !IsBusy; }

    /// <summary>状态栏文本的颜色画刷，用于区分不同类型的状态信息</summary>
    [ObservableProperty]
    private SolidColorBrush brush = new(Colors.White);

    /// <summary>
    /// 重置状态到初始状态
    /// 状态文本设为"就绪"，进度重置为0，取消忙碌状态，颜色设为白色
    /// </summary>
    public void Reset() {
        StatusText = "就绪";
        IsBusy = false;
        Progress = 0;
        Brush = new(Colors.White);
    }

    /// <summary>
    /// 设置为忙碌状态
    /// </summary>
    /// <param name="message">可选的状态消息，如果提供则更新状态文本</param>
    public void Busy(string? message = null) {
        if (!string.IsNullOrEmpty(message)) { StatusText = message; }
        Brush = new(Colors.White);
        IsBusy = true;
    }

    /// <summary>
    /// 设置为空闲状态
    /// </summary>
    /// <param name="message">可选的状态消息，如果提供则更新状态文本</param>
    public void Free(string? message = null) {
        if (!string.IsNullOrEmpty(message)) { StatusText = message; }
        Brush = new(Colors.White);
        IsBusy = false;
    }

    /// <summary>
    /// 设置为成功状态，显示绿色文本，进度设为100%，取消忙碌状态
    /// </summary>
    /// <param name="message">成功状态的消息文本</param>
    public void Success(string message) {
        StatusText = message;
        Brush = new(Colors.Green);
        Progress = 100;
        IsBusy = false;
    }

    /// <summary>
    /// 显示普通消息，使用白色文本
    /// </summary>
    /// <param name="message">要显示的消息文本</param>
    public void Message(string message) {
        StatusText = message;
        Brush = new(Colors.White);
    }

    /// <summary>
    /// 设置为错误状态，显示红色文本，取消忙碌状态
    /// </summary>
    /// <param name="message">错误状态的消息文本</param>
    public void Error(string message) {
        StatusText = message;
        Brush = new(Colors.Red);
        IsBusy = false;
    }

    /// <summary>
    /// 设置为警告状态，显示橙色文本，取消忙碌状态
    /// </summary>
    /// <param name="message">警告状态的消息文本</param>
    public void Warn(string message) {
        StatusText = message;
        Brush = new(Colors.Orange);
        IsBusy = false;
    }

    /// <summary>
    /// 设置为运行状态，更新进度信息
    /// </summary>
    /// <param name="progress">当前进度值（0-100）</param>
    /// <param name="message">可选的状态消息，如果提供则更新状态文本</param>
    public void Running(int progress, string? message = null) {
        if (!string.IsNullOrEmpty(message)) { StatusText = message; }
        IsBusy = false; 
        Brush = new(Colors.White);
        Progress = progress;
    }

}