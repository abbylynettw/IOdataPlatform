using System.Collections.Generic;

namespace IODataPlatform.Utilities;

/// <summary>
/// 显示信息容器类
/// 存储类型的显示名称和字段显示名称的映射关系
/// 用于支持DisplayAttribute特性的动态获取和缓存
/// </summary>
public class DisplayInfo
{
	/// <summary>类的显示名称，来自DisplayAttribute或默认类名</summary>
	public string ClassName { get; set; }

	/// <summary>字段名到显示名称的映射字典</summary>
	public Dictionary<string, string> FieldDisplayNames { get; set; }
}
