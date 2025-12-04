﻿﻿﻿namespace IODataPlatform.Models;

/// <summary>
/// 部门枚举，支持使用Flags特性实现多部门归属
/// 用于定义用户所属的部门信息和相关权限控制
/// </summary>
[Flags]
public enum Department {
    /// <summary>系统一室</summary>
    系统一室 = 1 << 0,
    /// <summary>系统二室</summary>
    系统二室 = 1 << 1,
    /// <summary>工程硬件室</summary>
    工程硬件室 = 1 << 2,
    /// <summary>安全级室</summary>
    安全级室 = 1 << 3,
}
/// <summary>
/// 控制系统枚举，定义系统中支持的各种控制系统类型
/// 用于区分不同IO设备所属的控制系统
/// </summary>
public enum ControlSystem
{
    /// <summary>龙鳍系统</summary>
    龙鳍 = 1,
    /// <summary>中控系统</summary>
    中控 = 2,
    /// <summary>龙核系统</summary>
    龙核 = 3,
    /// <summary>一室系统</summary>
    一室 = 4,
    /// <summary>安全级模拟系统</summary>
    安全级模拟系统 = 5
}

/// <summary>
/// 用户权限枚举，支持使用Flags特性实现多权限组合
/// 定义用户在系统中可以执行的操作权限
/// </summary>
[Flags]
public enum UserPermission {
    /// <summary>公式编辑权限</summary>
    公式编辑 = 1 << 0,
    /// <summary>IO数据编辑权限</summary>
    IO编辑 = 1 << 1,
    /// <summary>预留权限3（待定义）</summary>
    权限3 = 1 << 2,
}

/// <summary>
/// 盘箱柜类别枚举，支持使用Flags特性实现多类别组合
/// 用于对工程中的盘箱柜设备进行分类管理
/// </summary>
[Flags]
public enum 盘箱柜类别 {
    /// <summary>机柜类型</summary>
    机柜 = 1 << 0,
    /// <summary>盘台类型</summary>
    盘台 = 1 << 1,
    /// <summary>阀箱类型</summary>
    阀箱 = 1 << 2,
}

/// <summary>
/// 工作状态枚举，支持使用Flags特性实现多状态组合
/// 用于跟踪和管理项目或任务的执行状态
/// </summary>
[Flags]
public enum WorkStatus
{
    /// <summary>任务尚未开始</summary>
    未开始 = 1 << 0,
    /// <summary>任务正在进行中</summary>
    进行中 = 1 << 1,
    /// <summary>任务已经完成</summary>
    已完成 = 1 << 2,
}

/// <summary>
/// 预留板卡用途枚举
/// 用于区分预留板卡的使用目的
/// </summary>
public enum ReservedPurpose
{
    /// <summary>通讯预留</summary>
    通讯预留 = 1,
    /// <summary>报警预留</summary>
    报警预留 = 2
}