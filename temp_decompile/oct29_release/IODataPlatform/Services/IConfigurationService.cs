using System;
using System.Threading.Tasks;
using IODataPlatform.Models.Configs;

namespace IODataPlatform.Services;

/// <summary>
/// 配置管理服务接口
/// 定义配置读取、验证、更新和监控的核心功能
/// 支持强类型配置访问和实时配置变更通知
/// </summary>
public interface IConfigurationService
{
	/// <summary>
	/// 配置变更事件
	/// </summary>
	event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

	/// <summary>
	/// 获取应用程序配置
	/// </summary>
	ApplicationConfig GetConfiguration();

	/// <summary>
	/// 获取特定类型的配置节点
	/// </summary>
	/// <typeparam name="T">配置类型</typeparam>
	/// <param name="sectionName">配置节点名称</param>
	/// <returns>配置对象</returns>
	T GetSection<T>(string sectionName) where T : class, new();

	/// <summary>
	/// 更新配置并保存到文件
	/// </summary>
	/// <param name="config">新的配置对象</param>
	/// <returns>操作是否成功</returns>
	Task<bool> UpdateConfigurationAsync(ApplicationConfig config);

	/// <summary>
	/// 验证配置的有效性
	/// </summary>
	/// <param name="config">要验证的配置对象</param>
	/// <returns>验证结果</returns>
	ValidationResult ValidateConfiguration(ApplicationConfig config);
}
