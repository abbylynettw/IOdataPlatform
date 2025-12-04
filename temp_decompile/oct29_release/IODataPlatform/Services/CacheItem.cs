using System;

namespace IODataPlatform.Services;

/// <summary>
/// 缓存项内部类
/// 包含缓存数据、过期时间和访问统计信息
/// </summary>
internal class CacheItem
{
	/// <summary>序列化后的缓存数据</summary>
	public required string Data { get; set; }

	/// <summary>过期时间</summary>
	public required DateTime ExpiresAt { get; set; }

	/// <summary>最后访问时间（用于LRU策略）</summary>
	public required DateTime LastAccessed { get; set; }

	/// <summary>创建时间</summary>
	public required DateTime CreatedAt { get; set; }

	/// <summary>是否已过期</summary>
	public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}
