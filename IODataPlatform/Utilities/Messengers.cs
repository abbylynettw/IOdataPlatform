﻿using System.Reactive.Subjects;

namespace IODataPlatform.Utilities;

/// <summary>
/// 全局消息通信中心
/// 使用ReactiveX模式提供全局消息发布和订阅服务
/// 支持跨组件、跨页面的异步消息通信，实现松耦合的事件驱动架构
/// </summary>
public static class Messengers {

    /// <summary>
    /// 全屏显示消息主题
    /// 用于通知全屏状态变更，同时控制导航栏的折叠状态
    /// true：进入全屏模式，隐藏导航栏；false：退出全屏模式，显示导航栏
    /// </summary>
    public static Subject<bool> FullScreen { get; } = new();

}