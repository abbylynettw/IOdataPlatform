using IODataPlatform.Models.ExportModels;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// 扩展方法
/// </summary>
public static class ExportTypeExtensions
{
	/// <summary>
	/// 获取枚举描述
	/// </summary>
	public static string GetDescription(this ExportType exportType)
	{
		return exportType switch
		{
			ExportType.CompleteList => "完整IO清单", 
			ExportType.CurrentSystemList => "当前控制系统IO清单", 
			ExportType.PublishedList => "发布的IO清单", 
			_ => exportType.ToString(), 
		};
	}
}
