﻿using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Views.Pages;
using IODataPlatform.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IODataPlatform.Services;

/// <summary>
/// 应用程序主机服务类
/// 实现IHostedService接口，负责应用程序的启动和停止生命周期管理
/// 在应用程序启动时显示登录窗口，在停止时执行清理操作
/// </summary>
public class ApplicationHostService(IServiceProvider serviceProvider, GlobalModel model, SqlSugarContext context) : IHostedService {

    /// <summary>
    /// 启动应用程序服务
    /// 从服务容器中获取LoginWindow实例并显示登录界面
    /// </summary>
    /// <param name="cancellationToken">取消令牌，用于取消异步操作</param>
    /// <returns>表示异步操作完成的任务</returns>
    public async Task StartAsync(CancellationToken cancellationToken) {
        serviceProvider.GetService<LoginWindow>()!.Show();
        await Task.CompletedTask;
    }

    /// <summary>
    /// 停止应用程序服务
    /// 执行应用程序关闭时的清理操作
    /// </summary>
    /// <param name="cancellationToken">取消令牌，用于取消异步操作</param>
    /// <returns>表示异步操作完成的任务</returns>
    public async Task StopAsync(CancellationToken cancellationToken) {
        await Task.CompletedTask;
    }

 
}
