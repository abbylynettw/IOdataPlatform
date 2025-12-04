using IODataPlatform.Models.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;

namespace IODataPlatform.Services;

/// <summary>
/// 缓存服务接口
/// 定义应用程序缓存操作的核心功能，支持泛型、过期时间和统计信息
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// 获取缓存项
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <returns>缓存数据，不存在或已过期返回null</returns>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// 设置缓存项
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存数据</param>
    /// <param name="expiration">过期时间，为null使用默认过期时间</param>
    /// <returns>操作是否成功</returns>
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// 移除缓存项
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <returns>操作是否成功</returns>
    Task<bool> RemoveAsync(string key);

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    /// <returns>操作是否成功</returns>
    Task<bool> ClearAsync();

    /// <summary>
    /// 检查缓存项是否存在
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// 获取或设置缓存项（如果不存在则通过工厂方法创建）
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="factory">数据工厂方法</param>
    /// <param name="expiration">过期时间</param>
    /// <returns>缓存数据</returns>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// 获取缓存统计信息
    /// </summary>
    /// <returns>缓存统计信息</returns>
    CacheStatistics GetStatistics();
}

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

        // 启动定时清理任务（每5分钟清理一次过期项）
        _cleanupTimer = new Timer(CleanupExpiredItems, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

        _logger.LogInformation("内存缓存服务已启动，配置: 启用={Enabled}, 默认过期时间={DefaultExpiration}分钟, 最大项数={MaxItems}",
            _config.Enabled, _config.DefaultExpirationMinutes, _config.MaxCacheItems);
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
            if (_cache.TryGetValue(key, out var cacheItem))
            {
                if (cacheItem.IsExpired)
                {
                    // 异步移除过期项
                    _ = Task.Run(() => _cache.TryRemove(key, out _));
                    _statistics.RecordMiss();
                    return null;
                }

                // 更新访问时间（用于LRU策略）
                cacheItem.LastAccessed = DateTime.UtcNow;
                _statistics.RecordHit();

                var data = JsonSerializer.Deserialize<T>(cacheItem.Data);
                _logger.LogDebug("缓存命中: {Key}", key);
                return data;
            }

            _statistics.RecordMiss();
            _logger.LogDebug("缓存未命中: {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取缓存项失败: {Key}", key);
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
                // 检查缓存容量，必要时进行LRU清理
                if (_cache.Count >= _config.MaxCacheItems)
                {
                    await EvictLeastRecentlyUsedAsync();
                }

                var expirationTime = expiration ?? TimeSpan.FromMinutes(_config.DefaultExpirationMinutes);
                var serializedData = JsonSerializer.Serialize(value);

                var cacheItem = new CacheItem
                {
                    Data = serializedData,
                    ExpiresAt = DateTime.UtcNow.Add(expirationTime),
                    LastAccessed = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _cache.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
                _statistics.RecordSet();

                _logger.LogDebug("缓存设置成功: {Key}, 过期时间: {ExpiresAt}", key, cacheItem.ExpiresAt);
                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置缓存项失败: {Key}", key);
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
            var removed = _cache.TryRemove(key, out _);
            if (removed)
            {
                _statistics.RecordRemove();
                _logger.LogDebug("缓存项已移除: {Key}", key);
            }
            return removed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除缓存项失败: {Key}", key);
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
            var count = _cache.Count;
            _cache.Clear();
            _statistics.RecordClear(count);
            _logger.LogInformation("已清空所有缓存项，数量: {Count}", count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空缓存失败");
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

        if (_cache.TryGetValue(key, out var cacheItem))
        {
            if (cacheItem.IsExpired)
            {
                // 异步移除过期项
                _ = Task.Run(() => _cache.TryRemove(key, out _));
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
        var cachedValue = await GetAsync<T>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        // 使用信号量防止缓存穿透
        await _semaphore.WaitAsync();
        try
        {
            // 双重检查
            cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            // 调用工厂方法获取数据
            var value = await factory();
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
            var itemsToRemove = _cache
                .OrderBy(kvp => kvp.Value.LastAccessed)
                .Take(_config.MaxCacheItems / 10) // 移除10%的项目
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in itemsToRemove)
            {
                _cache.TryRemove(key, out _);
            }

            _logger.LogDebug("LRU淘汰完成，移除项数: {Count}", itemsToRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LRU淘汰失败");
        }
    }

    /// <summary>
    /// 定时清理过期项
    /// </summary>
    private void CleanupExpiredItems(object? state)
    {
        try
        {
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.IsExpired)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogDebug("定时清理完成，移除过期项数: {Count}", expiredKeys.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "定时清理失败");
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

/// <summary>
/// 缓存统计信息类
/// 记录缓存的性能指标和使用情况
/// </summary>
public class CacheStatistics
{
    private long _hits;
    private long _misses;
    private long _sets;
    private long _removes;
    private long _errors;

    /// <summary>缓存命中次数</summary>
    public long Hits => _hits;

    /// <summary>缓存未命中次数</summary>
    public long Misses => _misses;

    /// <summary>缓存设置次数</summary>
    public long Sets => _sets;

    /// <summary>缓存移除次数</summary>
    public long Removes => _removes;

    /// <summary>错误次数</summary>
    public long Errors => _errors;

    /// <summary>当前缓存项数量</summary>
    public int CurrentItemCount { get; set; }

    /// <summary>缓存命中率</summary>
    public double HitRatio => _hits + _misses > 0 ? (double)_hits / (_hits + _misses) : 0;

    /// <summary>记录缓存命中</summary>
    internal void RecordHit() => Interlocked.Increment(ref _hits);

    /// <summary>记录缓存未命中</summary>
    internal void RecordMiss() => Interlocked.Increment(ref _misses);

    /// <summary>记录缓存设置</summary>
    internal void RecordSet() => Interlocked.Increment(ref _sets);

    /// <summary>记录缓存移除</summary>
    internal void RecordRemove() => Interlocked.Increment(ref _removes);

    /// <summary>记录错误</summary>
    internal void RecordError() => Interlocked.Increment(ref _errors);

    /// <summary>记录清空操作</summary>
    internal void RecordClear(int count) => Interlocked.Add(ref _removes, count);
}