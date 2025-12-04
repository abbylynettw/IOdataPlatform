﻿﻿﻿﻿﻿using System.Reactive.Linq;
using System.Reflection;

using IODataPlatform.Views.Windows;

using Microsoft.Extensions.DependencyInjection;

namespace IODataPlatform.Services;

/// <summary>
/// 视图服务扩展类
/// 提供自动注册所有视图和视图模型的扩展方法
/// 通过反射扫描程序集中的视图类，根据命名空间约定选择适当的服务生命周期
/// </summary>
public static class ViewServiceExtension {

    /// <summary>
    /// 向服务容器中注册所有视图和视图模型
    /// 根据不同的命名空间和用途选择适当的生命周期管理
    /// </summary>
    /// <param name="services">服务容器</param>
    /// <returns>返回服务容器以支持链式调用</returns>
    public static IServiceCollection AddAllViews(this IServiceCollection services) {

        // 注册主窗口和导航相关服务（单例）
        services.AddSingleton<INavigationWindow, MainWindow>();
        services.AddSingleton<MainWindowViewModel>();

        // 注册登录窗口和其视图模型（单例）
        services.AddSingleton<LoginWindow>();
        services.AddSingleton<LoginWindowViewModel>();
        
        // 注册其他窗口（瞬时，每次打开时创建新实例）
        services.AddTransient<ValveSortingWindow>();

        // 通过反射获取所有视图相关类型
        var allViewTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => $"{x.Namespace}".StartsWith("IODataPlatform.Views"))
            .Where(x => x.Name.EndsWith("ViewModel") || x.Name.EndsWith("Page"))
            .ToArray();

        // 注册主页面类型（单例，因为需要保持状态）
        allViewTypes.Where(x => x.Namespace == "IODataPlatform.Views.Pages")
            .AllDo(x => services.AddSingleton(x));

        // 注册子页面类型（瞬时，除了Common和Project的）
        allViewTypes.Where(x => x.Namespace!.StartsWith("IODataPlatform.Views.SubPages"))
            .Where(x => x.Namespace != "IODataPlatform.Views.SubPages.Common")
            .AllDo(x => services.AddTransient(x));
        
        // 注册Common和Project中的通用页面（单例，因为需要在绑定前传参修改标题）
        allViewTypes.Where(x => x.Namespace == "IODataPlatform.Views.SubPages.Common"|| x.Namespace == "IODataPlatform.Views.SubPages.Project")
            .AllDo(x => services.AddSingleton(x));

        return services;
    }
    
}