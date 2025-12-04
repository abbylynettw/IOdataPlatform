using System;

namespace IODataPlatform.Models;

/// <summary>
/// 部门枚举，支持使用Flags特性实现多部门归属
/// 用于定义用户所属的部门信息和相关权限控制
/// </summary>
[Flags]
public enum Department
{
	/// <summary>系统一室</summary>
	系统一室 = 1,
	/// <summary>系统二室</summary>
	系统二室 = 2,
	/// <summary>工程硬件室</summary>
	工程硬件室 = 4,
	/// <summary>安全级室</summary>
	安全级室 = 8
}
