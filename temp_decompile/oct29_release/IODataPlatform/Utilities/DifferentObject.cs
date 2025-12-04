using System.Collections.Generic;

namespace IODataPlatform.Utilities;

/// <summary>
/// 差异对象类
/// 封装单个对象的变更信息，包括主键、变更类型和具体的属性差异列表
/// 支持泛型主键，提供细粒度的变更追踪和显示
/// </summary>
/// <typeparam name="T">主键类型</typeparam>
public class DifferentObject<T>
{
	/// <summary>对象的唯一标识符（主键值）</summary>
	public required T Key { get; set; }

	/// <summary>数据变更类型（新增、移除、覆盖）</summary>
	public required DifferentType Type { get; set; }

	/// <summary>具体的属性差异列表，包含所有发生变化的属性信息</summary>
	public List<DifferentProperty> DiffProps { get; } = new List<DifferentProperty>();
}
