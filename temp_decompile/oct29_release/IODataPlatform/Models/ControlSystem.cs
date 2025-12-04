namespace IODataPlatform.Models;

/// <summary>
/// 控制系统枚举，定义系统中支持的各种控制系统类型
/// 用于区分不同IO设备所属的控制系统
/// </summary>
public enum ControlSystem
{
	/// <summary>龙鳍系统</summary>
	龙鳍 = 1,
	/// <summary>中控系统</summary>
	中控,
	/// <summary>龙核系统</summary>
	龙核,
	/// <summary>一室系统</summary>
	一室,
	/// <summary>安全级模拟系统</summary>
	安全级模拟系统
}
