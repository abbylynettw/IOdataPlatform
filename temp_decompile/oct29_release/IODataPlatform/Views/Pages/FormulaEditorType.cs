using System;
using IODataPlatform.Models;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 公式编辑器的目标类型封装类
/// 将控制系统枚举和对应的.NET类型绑定在一起
/// 用于在界面上显示可选的控制系统并支持后续的类型操作
/// </summary>
/// <param name="system">控制系统枚举值</param>
/// <param name="type">对应的.NET数据类型</param>
public class FormulaEditorType(ControlSystem system, Type type)
{
	/// <summary>控制系统名称</summary>
	public ControlSystem Name { get; } = system;

	/// <summary>对应的数据类型</summary>
	public Type Type { get; } = type;
}
