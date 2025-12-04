using System;

namespace IODataPlatform.Models;

/// <summary>
/// 用户权限枚举，支持使用Flags特性实现多权限组合
/// 定义用户在系统中可以执行的操作权限
/// </summary>
[Flags]
public enum UserPermission
{
	/// <summary>公式编辑权限</summary>
	公式编辑 = 1,
	/// <summary>IO数据编辑权限</summary>
	IO编辑 = 2,
	/// <summary>预留权限3（待定义）</summary>
	权限3 = 4
}
