﻿using Microsoft.Extensions.DependencyInjection;

namespace IODataPlatform.Models;

/// <summary>
/// 全局数据模型，作为应用程序的核心数据容器
/// 整合状态管理、用户信息等全局共享数据，支持依赖注入
/// 通过MVVM模式为界面提供数据绑定支持
/// </summary>
public partial class GlobalModel(Status status, UserInfo user) : ObservableObject {

    /// <summary>获取全局状态管理器，用于管理应用程序的状态栏信息</summary>
    public Status Status { get; } = status;

    /// <summary>获取当前登录用户的信息，包含用户权限和部门信息</summary>
    public UserInfo User { get; } = user;

}

/// <summary>
/// GlobalModel的依赖注入扩展方法类
/// 提供便捷的服务注册方法，统一管理所有模型类的生命周期
/// </summary>
public static class GlobalModelExtension {

    /// <summary>
    /// 向服务容器中注册所有模型类
    /// 将GlobalModel、Status、UserInfo注册为单例服务
    /// </summary>
    /// <param name="services">服务容器</param>
    /// <returns>返回服务容器以支持链式调用</returns>
    public static IServiceCollection AddAllModels(this IServiceCollection services) {

        services.AddSingleton<GlobalModel>();
        services.AddSingleton<Status>();
        services.AddSingleton<UserInfo>();

        return services;
    }
}