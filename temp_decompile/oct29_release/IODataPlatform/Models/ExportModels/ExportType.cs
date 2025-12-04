using System.ComponentModel;

namespace IODataPlatform.Models.ExportModels;

/// <summary>
/// 导出类型枚举
/// </summary>
public enum ExportType
{
	/// <summary>
	/// 完整IO清单
	/// </summary>
	[Description("完整IO清单")]
	CompleteList,
	/// <summary>
	/// 当前控制系统IO清单
	/// </summary>
	[Description("当前控制系统IO清单")]
	CurrentSystemList,
	/// <summary>
	/// 发布的IO清单
	/// </summary>
	[Description("发布的IO清单")]
	PublishedList
}
