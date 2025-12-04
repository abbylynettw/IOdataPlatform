﻿using System;
using System.Threading.Tasks;

using LYSoft.Libs.ServiceInterfaces;

using Microsoft.Extensions.DependencyInjection;

using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace LYSoft.Libs.Wpf.WpfUI;

/// <summary>
/// 基于WpfUI的消息提示服务实现类
/// 实现IMessageService接口，提供现代化的消息提示功能，支持多种消息类型和用户交互
/// 集成WpfUI的ContentDialog和Snackbar组件，提供美观的消息展示体验
/// 支持服务降级，当WpfUI组件不可用时自动回退到标准MessageBox
/// 适用于需要现代化用户界面的WPF应用程序消息提示场景
/// </summary>
/// <param name="service">依赖注入服务提供者，用于获取WpfUI相关服务</param>
public class WpfUIMessageService(IServiceProvider service) : IMessageService {

    /// <summary>WpfUI内容对话框服务，用于显示模态对话框</summary>
    private IContentDialogService? dialog = TryGetService<IContentDialogService>(service);
    /// <summary>WpfUI快捷通知服务，用于显示临时通知消息</summary>
    private ISnackbarService? snackbar = TryGetService<ISnackbarService>(service);

    /// <summary>
    /// 安全获取服务的辅助方法
    /// 尝试从依赖注入容器中获取指定类型的服务，失败时返回默认值而不抛出异常
    /// 用于处理WpfUI服务可能不可用的情况，实现服务降级功能
    /// </summary>
    /// <typeparam name="T">要获取的服务类型</typeparam>
    /// <param name="service">服务提供者</param>
    /// <returns>返回服务实例，如果获取失败则返回默认值</returns>
    private static T? TryGetService<T>(IServiceProvider service) {
        try { return service.GetService<T>(); } catch { return default; }
    }

    /// <inheritdoc/>
    public async Task AlertAsync(string message) {
        await AlertAsync(message, "系统提示");
    }

    /// <inheritdoc/>
    public async Task AlertAsync(string message, string title) {
        try {
            if (dialog is null) { throw new(); }
            await dialog.ShowSimpleDialogAsync(new() { CloseButtonText = "确认", Content = message, Title = title });
        } catch {
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ConfirmAsync(string message) {
        return await ConfirmAsync(message, "系统提示");
    }

    /// <inheritdoc/>
    public async Task<bool> ConfirmAsync(string message, string title) {
        try {
            if (dialog is null) { throw new(); }
            return await dialog.ShowSimpleDialogAsync(new() { CloseButtonText = "取消", PrimaryButtonText = "确认", Content = message, Title = title }) == ContentDialogResult.Primary;
        } catch { }

        return System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes;
    }

    /// <inheritdoc/>
    public async Task<bool?> ConfirmWithCancelAsync(string message) {
        return await ConfirmWithCancelAsync(message, "系统提示");
    }

    /// <inheritdoc/>
    public async Task<bool?> ConfirmWithCancelAsync(string message, string title) {
        try {
            if (dialog is null) { throw new(); }
            return await dialog.ShowSimpleDialogAsync(new() { CloseButtonText = "取消", PrimaryButtonText = "是", SecondaryButtonText = "否", Content = message, Title = title }) switch {
                ContentDialogResult.Primary => true,
                ContentDialogResult.Secondary => false,
                _ => null,
            };
        } catch { }

        return System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.YesNoCancel) switch {
            System.Windows.MessageBoxResult.Yes => true,
            System.Windows.MessageBoxResult.No => false,
            _ => null,
        };
    }

    /// <inheritdoc/>
    public async Task ErrorAsync(string message) {
        await ErrorAsync(message, "系统提示");
    }

    /// <inheritdoc/>
    public async Task ErrorAsync(string message, string title) {
        try {
            if (snackbar is null) { throw new(); }
            snackbar.Show(title, message, ControlAppearance.Danger, new SymbolIcon() { Symbol = SymbolRegular.Info20 }, TimeSpan.FromSeconds(2.5));
            return;
        } catch {
        }

        try {
            if (dialog is null) { throw new(); }
            await dialog.ShowSimpleDialogAsync(new() { CloseButtonText = "确认", Content = message, Title = title });
            return;
        } catch {
        }

        System.Windows.MessageBox.Show(message, title);
    }

    /// <inheritdoc/>
    public async Task MessageAsync(string message) {
        await MessageAsync(message, "系统提示");
    }

    /// <inheritdoc/>
    public async Task MessageAsync(string message, string title) {
        try {
            if (snackbar is null) { throw new(); }
            snackbar.Show(title, message, ControlAppearance.Secondary, new SymbolIcon() { Symbol = SymbolRegular.Info20 }, TimeSpan.FromSeconds(2.5));
            return;
        } catch {
        }

        try {
            if (dialog is null) { throw new(); }
            await dialog.ShowSimpleDialogAsync(new() { CloseButtonText = "确认", Content = message, Title = title });
            return;
        } catch {
        }

        System.Windows.MessageBox.Show(message, title);
    }

    /// <inheritdoc/>
    public async Task SuccessAsync(string message) {
        await SuccessAsync(message, "系统提示");
    }

    /// <inheritdoc/>
    public async Task SuccessAsync(string message, string title) {
        try {
            if (snackbar is null) { throw new(); }
            snackbar.Show(title, message, ControlAppearance.Success, new SymbolIcon() { Symbol = SymbolRegular.Checkmark20 }, TimeSpan.FromSeconds(2.5));
            return;
        } catch {
        }

        try {
            if (dialog is null) { throw new(); }
            await dialog.ShowSimpleDialogAsync(new() { CloseButtonText = "确认", Content = message, Title = title });
            return;
        } catch {
        }

        await MessageAsync(message, title);
    }

    /// <inheritdoc/>
    public async Task WarnAsync(string message) {
        await WarnAsync(message, "系统提示");
    }

    /// <inheritdoc/>
    public async Task WarnAsync(string message, string title) {
        try {
            if (snackbar is null) { throw new(); }
            snackbar.Show(title, message, ControlAppearance.Caution, new SymbolIcon() { Symbol = SymbolRegular.Warning20 }, TimeSpan.FromSeconds(2.5));
            return;
        } catch {
        }

        try {
            if (dialog is null) { throw new(); }
            await dialog.ShowSimpleDialogAsync(new() { CloseButtonText = "确认", Content = message, Title = title });
            return;
        } catch {
        }

        await MessageAsync(message, title);
    }
}