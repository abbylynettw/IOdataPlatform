using System;

namespace IODataPlatform.Models;

/// <summary>
/// 工作状态枚举，支持使用Flags特性实现多状态组合
/// 用于跟踪和管理项目或任务的执行状态
/// </summary>
[Flags]
public enum WorkStatus
{
	/// <summary>任务尚未开始</summary>
	未开始 = 1,
	/// <summary>任务正在进行中</summary>
	进行中 = 2,
	/// <summary>任务已经完成</summary>
	已完成 = 4
}
