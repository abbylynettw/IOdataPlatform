using System;
using System.Linq;
using System.Reflection;
using IODataPlatform.Views.Windows;
using LYSoft.Libs;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;

namespace IODataPlatform.Services;

/// <summary>
/// 视图服务扩展类
/// 提供自动注册所有视图和视图模型的扩展方法
/// 通过反射扫描程序集中的视图类，根据命名空间约定选择适当的服务生命周期
/// </summary>
public static class ViewServiceExtension
{
	/// <summary>
	/// 向服务容器中注册所有视图和视图模型
	/// 根据不同的命名空间和用途选择适当的生命周期管理
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <returns>返回服务容器以支持链式调用</returns>
	public static IServiceCollection AddAllViews(this IServiceCollection services)
	{
		services.AddSingleton<INavigationWindow, MainWindow>();
		services.AddSingleton<MainWindowViewModel>();
		services.AddSingleton<LoginWindow>();
		services.AddSingleton<LoginWindowViewModel>();
		services.AddTransient<ValveSortingWindow>();
		Type[] source = (from x in Assembly.GetExecutingAssembly().GetTypes()
			where (x.Namespace ?? "").StartsWith("IODataPlatform.Views")
			where x.Name.EndsWith("ViewModel") || x.Name.EndsWith("Page")
			select x).ToArray();
		source.Where((Type x) => x.Namespace == "IODataPlatform.Views.Pages").AllDo(delegate(Type x)
		{
			services.AddSingleton(x);
		});
		(from x in source
			where x.Namespace.StartsWith("IODataPlatform.Views.SubPages")
			where x.Namespace != "IODataPlatform.Views.SubPages.Common"
			select x).AllDo(delegate(Type x)
		{
			services.AddTransient(x);
		});
		source.Where((Type x) => x.Namespace == "IODataPlatform.Views.SubPages.Common" || x.Namespace == "IODataPlatform.Views.SubPages.Project").AllDo(delegate(Type x)
		{
			services.AddSingleton(x);
		});
		return services;
	}
}
