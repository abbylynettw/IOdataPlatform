using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IODataPlatform.Models.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IODataPlatform.Services;

/// <summary>
/// 内存缓存服务实现
/// 提供高性能的内存缓存功能，支持LRU淘汰策略和自动过期清理
/// 实现线程安全的并发访问和内存使用优化
/// </summary>
public class MemoryCacheService : ICacheService, IDisposable
{
	private readonly ConcurrentDictionary<string, CacheItem> _cache;

	private readonly CacheConfig _config;

	private readonly ILogger<MemoryCacheService> _logger;

	private readonly Timer _cleanupTimer;

	private readonly SemaphoreSlim _semaphore;

	private readonly CacheStatistics _statistics;

	private bool _disposed;

	/// <summary>
	/// 构造函数
	/// 初始化缓存服务，设置定时清理和配置参数
	/// </summary>
	/// <param name="config">缓存配置</param>
	/// <param name="logger">日志记录器</param>
	public MemoryCacheService(IOptions<CacheConfig> config, ILogger<MemoryCacheService> logger)
	{
		_config = config.Value;
		_logger = logger;
		_cache = new ConcurrentDictionary<string, CacheItem>();
		_semaphore = new SemaphoreSlim(1, 1);
		_statistics = new CacheStatistics();
		_cleanupTimer = new Timer(CleanupExpiredItems, null, TimeSpan.FromMinutes(5.0), TimeSpan.FromMinutes(5.0));
		_logger.LogInformation("内存缓存服务已启动，配置: 启用={Enabled}, 默认过期时间={DefaultExpiration}分钟, 最大项数={MaxItems}", _config.Enabled, _config.DefaultExpirationMinutes, _config.MaxCacheItems);
	}

	/// <summary>
	/// 异步获取缓存项
	/// 实现线程安全访问和统计信息记录
	/// </summary>
	/// <typeparam name="T">缓存数据类型</typeparam>
	/// <param name="key">缓存键</param>
	/// <returns>缓存数据</returns>
	public async Task<T?> GetAsync<T>(string key) where T : class
	{
		if (!_config.Enabled)
		{
			return null;
		}
		try
		{
			if (_cache.TryGetValue(key, out CacheItem value))
			{
				if (value.IsExpired)
				{
					Task.Run(() => _cache.TryRemove(key, out CacheItem _));
					_statistics.RecordMiss();
					return null;
				}
				value.LastAccessed = DateTime.UtcNow;
				_statistics.RecordHit();
				T result = JsonSerializer.Deserialize<T>(value.Data);
				_logger.LogDebug("缓存命中: {Key}", key);
				return result;
			}
			_statistics.RecordMiss();
			_logger.LogDebug("缓存未命中: {Key}", key);
			return null;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "获取缓存项失败: {Key}", key);
			_statistics.RecordError();
			return null;
		}
	}

	/// <summary>
	/// 异步设置缓存项
	/// 实现容量控制和LRU淘汰策略
	/// </summary>
	/// <typeparam name="T">缓存数据类型</typeparam>
	/// <param name="key">缓存键</param>
	/// <param name="value">缓存数据</param>
	/// <param name="expiration">过期时间</param>
	/// <returns>操作是否成功</returns>
	public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
	{
		if (!_config.Enabled || value == null)
		{
			return false;
		}
		try
		{
			await _semaphore.WaitAsync();
			try
			{
				if (_cache.Count >= _config.MaxCacheItems)
				{
					await EvictLeastRecentlyUsedAsync();
				}
				TimeSpan value2 = expiration ?? TimeSpan.FromMinutes(_config.DefaultExpirationMinutes);
				string data = JsonSerializer.Serialize(value);
				CacheItem cacheItem = new CacheItem
				{
					Data = data,
					ExpiresAt = DateTime.UtcNow.Add(value2),
					LastAccessed = DateTime.UtcNow,
					CreatedAt = DateTime.UtcNow
				};
				_cache.AddOrUpdate(key, cacheItem, (string k, CacheItem v) => cacheItem);
				_statistics.RecordSet();
				_logger.LogDebug("缓存设置成功: {Key}, 过期时间: {ExpiresAt}", key, cacheItem.ExpiresAt);
				return true;
			}
			finally
			{
				_semaphore.Release();
			}
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "设置缓存项失败: {Key}", key);
			_statistics.RecordError();
			return false;
		}
	}

	/// <summary>
	/// 异步移除缓存项
	/// </summary>
	/// <param name="key">缓存键</param>
	/// <returns>操作是否成功</returns>
	public async Task<bool> RemoveAsync(string key)
	{
		try
		{
			CacheItem value;
			bool flag = _cache.TryRemove(key, out value);
			if (flag)
			{
				_statistics.RecordRemove();
				_logger.LogDebug("缓存项已移除: {Key}", key);
			}
			return flag;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "移除缓存项失败: {Key}", key);
			_statistics.RecordError();
			return false;
		}
	}

	/// <summary>
	/// 异步清空所有缓存
	/// </summary>
	/// <returns>操作是否成功</returns>
	public async Task<bool> ClearAsync()
	{
		try
		{
			int count = _cache.Count;
			_cache.Clear();
			_statistics.RecordClear(count);
			_logger.LogInformation("已清空所有缓存项，数量: {Count}", count);
			return true;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "清空缓存失败");
			_statistics.RecordError();
			return false;
		}
	}

	/// <summary>
	/// 异步检查缓存项是否存在
	/// </summary>
	/// <param name="key">缓存键</param>
	/// <returns>是否存在</returns>
	public async Task<bool> ExistsAsync(string key)
	{
		if (!_config.Enabled)
		{
			return false;
		}
		if (_cache.TryGetValue(key, out CacheItem value))
		{
			if (value.IsExpired)
			{
				Task.Run(() => _cache.TryRemove(key, out CacheItem _));
				return false;
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// 获取或设置缓存项
	/// 实现缓存穿透保护，确保工厂方法只调用一次
	/// </summary>
	/// <typeparam name="T">缓存数据类型</typeparam>
	/// <param name="key">缓存键</param>
	/// <param name="factory">数据工厂方法</param>
	/// <param name="expiration">过期时间</param>
	/// <returns>缓存数据</returns>
	public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null) where T : class
	{
		T val = await GetAsync<T>(key);
		if (val != null)
		{
			return val;
		}
		await _semaphore.WaitAsync();
		try
		{
			val = await GetAsync<T>(key);
			if (val != null)
			{
				return val;
			}
			T value = await factory();
			if (value != null)
			{
				await SetAsync(key, value, expiration);
			}
			return value;
		}
		finally
		{
			_semaphore.Release();
		}
	}

	/// <summary>
	/// 获取缓存统计信息
	/// </summary>
	/// <returns>缓存统计信息</returns>
	public CacheStatistics GetStatistics()
	{
		_statistics.CurrentItemCount = _cache.Count;
		return _statistics;
	}

	/// <summary>
	/// LRU策略淘汰最少使用的缓存项
	/// </summary>
	private async Task EvictLeastRecentlyUsedAsync()
	{
		try
		{
			List<string> list = (from kvp in _cache.OrderBy<KeyValuePair<string, CacheItem>, DateTime>((KeyValuePair<string, CacheItem> kvp) => kvp.Value.LastAccessed).Take(_config.MaxCacheItems / 10)
				select kvp.Key).ToList();
			foreach (string item in list)
			{
				_cache.TryRemove(item, out CacheItem _);
			}
			_logger.LogDebug("LRU淘汰完成，移除项数: {Count}", list.Count);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "LRU淘汰失败");
		}
	}

	/// <summary>
	/// 定时清理过期项
	/// </summary>
	private void CleanupExpiredItems(object? state)
	{
		try
		{
			List<string> list = (from kvp in _cache
				where kvp.Value.IsExpired
				select kvp.Key).ToList();
			foreach (string item in list)
			{
				_cache.TryRemove(item, out CacheItem _);
			}
			if (list.Count > 0)
			{
				_logger.LogDebug("定时清理完成，移除过期项数: {Count}", list.Count);
			}
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "定时清理失败");
		}
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_cleanupTimer?.Dispose();
			_semaphore?.Dispose();
			_cache?.Clear();
			_disposed = true;
		}
	}
}
