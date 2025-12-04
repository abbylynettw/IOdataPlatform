using Microsoft.Extensions.DependencyInjection;

namespace IODataPlatform.Models;

/// <summary>
/// GlobalModel的依赖注入扩展方法类
/// 提供便捷的服务注册方法，统一管理所有模型类的生命周期
/// </summary>
public static class GlobalModelExtension
{
	/// <summary>
	/// 向服务容器中注册所有模型类
	/// 将GlobalModel、Status、UserInfo注册为单例服务
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <returns>返回服务容器以支持链式调用</returns>
	public static IServiceCollection AddAllModels(this IServiceCollection services)
	{
		services.AddSingleton<GlobalModel>();
		services.AddSingleton<Status>();
		services.AddSingleton<UserInfo>();
		return services;
	}
}
