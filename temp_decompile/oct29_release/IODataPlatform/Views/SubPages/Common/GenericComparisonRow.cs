using System.Collections.Generic;
using IODataPlatform.Utilities;

namespace IODataPlatform.Views.SubPages.Common;

/// <summary>
/// 通用对比结果行
/// </summary>
public class GenericComparisonRow
{
	public string Key { get; set; }

	public GenericDataChangeType Type { get; set; }

	public DynamicObject Data { get; set; }

	public List<DifferentProperty> DiffProps { get; set; } = new List<DifferentProperty>();

	/// <summary>
	/// 是否被删除（用于显示删除线）
	/// </summary>
	public bool IsDeleted => Type == GenericDataChangeType.删除;

	/// <summary>
	/// 行背景色（仅新增和删除有背景色）
	/// </summary>
	public string BackgroundColor => Type switch
	{
		GenericDataChangeType.新增 => "#FFF9C4", 
		GenericDataChangeType.删除 => "#F5F5F5", 
		_ => "Transparent", 
	};
}
