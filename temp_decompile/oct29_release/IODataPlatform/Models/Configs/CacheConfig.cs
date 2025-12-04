using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.Configs;

/// <summary>
/// 缓存系统配置类
/// 定义内存缓存、分布式缓存等缓存策略
/// </summary>
public class CacheConfig
{
	/// <summary>
	/// 是否启用缓存功能
	/// </summary>
	public bool Enabled { get; set; } = true;

	/// <summary>
	/// 默认缓存过期时间（分钟）
	/// </summary>
	[Range(1, 1440)]
	public int DefaultExpirationMinutes { get; set; } = 30;

	/// <summary>
	/// 配置数据缓存过期时间（分钟）
	/// 由于配置数据变更频率较低，可以设置更长的缓存时间
	/// </summary>
	[Range(5, 720)]
	public int ConfigCacheMinutes { get; set; } = 120;

	/// <summary>
	/// 最大缓存项数量
	/// 防止内存过度使用
	/// </summary>
	[Range(100, 10000)]
	public int MaxCacheItems { get; set; } = 1000;
}
