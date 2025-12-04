using CommunityToolkit.Mvvm.ComponentModel;

namespace IODataPlatform.Models;

/// <summary>
/// 全局数据模型，作为应用程序的核心数据容器
/// 整合状态管理、用户信息等全局共享数据，支持依赖注入
/// 通过MVVM模式为界面提供数据绑定支持
/// </summary>
public class GlobalModel(Status status, UserInfo user) : ObservableObject()
{
	/// <summary>获取全局状态管理器，用于管理应用程序的状态栏信息</summary>
	public Status Status { get; } = status;

	/// <summary>获取当前登录用户的信息，包含用户权限和部门信息</summary>
	public UserInfo User { get; } = user;
}
