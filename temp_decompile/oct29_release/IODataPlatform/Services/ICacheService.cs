using System;
using System.Threading.Tasks;

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
