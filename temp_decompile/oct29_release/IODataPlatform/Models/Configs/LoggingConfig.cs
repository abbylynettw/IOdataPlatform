using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.Configs;

/// <summary>
/// 日志系统配置类
/// 定义日志记录的各项参数和策略
/// </summary>
public class LoggingConfig
{
	/// <summary>
	/// 日志级别（Debug, Information, Warning, Error, Critical）
	/// </summary>
	public string LogLevel { get; set; } = "Information";

	/// <summary>
	/// 日志文件存储路径
	/// </summary>
	public string LogPath { get; set; } = "Logs";

	/// <summary>
	/// 是否启用文件日志
	/// </summary>
	public bool EnableFileLogging { get; set; } = true;

	/// <summary>
	/// 是否启用控制台日志
	/// </summary>
	public bool EnableConsoleLogging { get; set; } = true;

	/// <summary>
	/// 日志文件保留天数
	/// </summary>
	[Range(1, 365)]
	public int RetainDays { get; set; } = 30;

	/// <summary>
	/// 单个日志文件最大大小（MB）
	/// </summary>
	[Range(1, 100)]
	public int MaxFileSizeMB { get; set; } = 10;
}
