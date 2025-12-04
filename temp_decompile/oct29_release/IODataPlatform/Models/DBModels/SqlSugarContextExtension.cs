using System;
using Microsoft.Extensions.DependencyInjection;

namespace IODataPlatform.Models.DBModels;

/// <summary>
/// SqlSugarContext的依赖注入扩展方法类
/// 提供便捷的SqlSugar服务注册方法
/// </summary>
public static class SqlSugarContextExtension
{
	/// <summary>
	/// 向服务容器中注册SqlSugar数据库上下文
	/// 使用单例模式确保连接池的有效管理
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <param name="connectionString">主数据库连接字符串（SQL Server）</param>
	/// <param name="connectionString2">辅助数据库连接字符串（Access）</param>
	/// <returns>返回服务容器以支持链式调用</returns>
	public static IServiceCollection AddSqlSugar(this IServiceCollection services, string connectionString, string connectionString2)
	{
		return services.AddSingleton((IServiceProvider provider) => new SqlSugarContext(connectionString, connectionString2));
	}
}
