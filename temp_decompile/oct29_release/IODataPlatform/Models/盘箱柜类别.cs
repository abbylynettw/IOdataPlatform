using System;

namespace IODataPlatform.Models;

/// <summary>
/// 盘箱柜类别枚举，支持使用Flags特性实现多类别组合
/// 用于对工程中的盘箱柜设备进行分类管理
/// </summary>
[Flags]
public enum 盘箱柜类别
{
	/// <summary>机柜类型</summary>
	机柜 = 1,
	/// <summary>盘台类型</summary>
	盘台 = 2,
	/// <summary>阀箱类型</summary>
	阀箱 = 4
}
