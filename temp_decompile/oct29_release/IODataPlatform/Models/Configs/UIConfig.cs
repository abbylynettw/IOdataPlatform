using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.Configs;

/// <summary>
/// 用户界面配置类
/// 管理应用程序的外观和行为设置
/// </summary>
public class UIConfig
{
	/// <summary>
	/// 应用程序主题（Light, Dark, Auto）
	/// </summary>
	public string Theme { get; set; } = "Light";

	/// <summary>
	/// 应用程序语言（zh-CN, en-US）
	/// </summary>
	public string Language { get; set; } = "zh-CN";

	/// <summary>
	/// 是否记住窗口位置和大小
	/// </summary>
	public bool RememberWindowState { get; set; } = true;

	/// <summary>
	/// 默认窗口宽度
	/// </summary>
	[Range(800, 3840)]
	public int DefaultWindowWidth { get; set; } = 1200;

	/// <summary>
	/// 默认窗口高度
	/// </summary>
	[Range(600, 2160)]
	public int DefaultWindowHeight { get; set; } = 800;

	/// <summary>
	/// 是否启用动画效果
	/// </summary>
	public bool EnableAnimations { get; set; } = true;

	/// <summary>
	/// 数据网格每页显示行数
	/// </summary>
	[Range(10, 1000)]
	public int DataGridPageSize { get; set; } = 50;
}
