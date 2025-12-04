using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.Configs;

/// <summary>
/// 数据库连接配置类
/// 管理多数据库环境的连接字符串和相关参数
/// </summary>
public class DatabaseConfig
{
	/// <summary>
	/// 主数据库连接字符串（SQL Server）
	/// 用于存储核心业务数据
	/// </summary>
	[Required]
	public string ConnectionString { get; set; } = string.Empty;

	/// <summary>
	/// 辅助数据库连接字符串（Access）
	/// 用于兼容性支持和临时数据存储
	/// </summary>
	public string ConnectionString2 { get; set; } = string.Empty;

	/// <summary>
	/// 连接池最大连接数
	/// </summary>
	[Range(1, 1000)]
	public int MaxPoolSize { get; set; } = 100;

	/// <summary>
	/// 命令执行超时时间（秒）
	/// </summary>
	[Range(1, 300)]
	public int CommandTimeout { get; set; } = 30;

	/// <summary>
	/// 是否启用自动迁移
	/// </summary>
	public bool EnableAutoMigration { get; set; }
}
