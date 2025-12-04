using System;
using IODataPlatform.Models.Configs;

namespace IODataPlatform.Services;

/// <summary>
/// 配置变更事件参数类
/// 包含变更后的配置信息
/// </summary>
public class ConfigurationChangedEventArgs : EventArgs
{
	/// <summary>
	/// 新的配置对象
	/// </summary>
	public ApplicationConfig NewConfiguration { get; }

	/// <summary>
	/// 构造函数
	/// </summary>
	/// <param name="newConfiguration">新的配置对象</param>
	public ConfigurationChangedEventArgs(ApplicationConfig newConfiguration)
	{
		NewConfiguration = newConfiguration;
	}
}
