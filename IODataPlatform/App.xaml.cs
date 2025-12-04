﻿﻿// 引入命名空间
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Threading;

// 引入自定义库和第三方库的命名空间
using IODataPlatform.Models;
using IODataPlatform.Models.Configs;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using IODataPlatform.Views.Windows;
using LYSoft.Libs.Config;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.Services;
using LYSoft.Libs.Wpf.WpfUI;

// 引入Microsoft的命名空间
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DataGrid = Wpf.Ui.Controls.DataGrid;

// 定义App类，它是一个部分类（partial class），意味着它的定义可能分布在多个文件中
namespace IODataPlatform;
public partial class App
{

    // 定义一个静态只读的IHost实例，用于应用程序的生命周期管理
    private static readonly IHost _host = Host
        .CreateDefaultBuilder() // 创建默认的主机构建器
        .ConfigureAppConfiguration(c => // 配置应用程序配置
            c.SetBasePath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "Assets", "Settings")) // 设置设置文件的基路径
            .SetConfigBasePath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "Assets", "Configs")) // 设置配置文件的基路径
        )
        .ConfigureServices((context, services) => { // 配置服务
            // 添加服务到依赖注入容器
            services.AddHostedService<ApplicationHostService>();
            services.AddSqlSugar(context.Configuration.GetConnectionString(nameof(SqlSugarContext))!, context.Configuration.GetConnectionString("SqlSugarContext2")!);

            services.Configure<WebServiceConfig>(context.Configuration.GetSection(nameof(WebServiceConfig)));
            services.Configure<OtherPlatFormConfig>(context.Configuration.GetSection(nameof(OtherPlatFormConfig)));

            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<ITaskBarService, TaskBarService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IContentDialogService, ContentDialogService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<IMessageService, WpfUIMessageService>();
            services.AddSingleton<IPickerService, PickerService>();

            services.AddSingleton<StorageService>();
            services.AddSingleton<DatabaseService>();
            services.AddSingleton<NavigationParameterService>();
            services.AddSingleton<CloudExportConfigService>();
            services.AddExcelService();
            services.AddPdfService();
            services.AddWordService();

            services.AddAllModels();
            services.AddAllViews();
        }).Build(); // 构建主机

    // 定义一个泛型方法，用于从服务容器中获取服务
    public static T GetService<T>() where T : class
    {
        return _host.Services.GetRequiredService<T>();
    }

    // 定义应用程序启动时的事件处理程序
    private void OnStartup(object sender, StartupEventArgs e)
    {
        _host.Start(); // 启动主机        
        ShutdownMode = ShutdownMode.OnExplicitShutdown; // 设置关闭模式为显式关闭
    }

    // 定义应用程序退出时的事件处理程序
    private async void OnExit(object sender, ExitEventArgs e)
    {
        await _host.StopAsync(); // 异步停止主机
        _host.Dispose(); // 释放主机资源
    }

    // 定义未处理异常的事件处理程序
    private async void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true; // 设置异常已处理
        await GetService<IMessageService>().ErrorAsync(e.Exception.Message, "系统提示"); // 显示错误消息
        try { GetService<Status>().Error(e.Exception.Message); } catch { } // 尝试记录错误状态，如果失败则忽略
    }  

}