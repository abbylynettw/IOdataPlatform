using System.Collections.Generic;

namespace IODataPlatform.Views.SubPages.Common;

/// <summary>
/// 动态对象类
/// </summary>
public class DynamicObject
{
	public string Key { get; set; }

	public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

	public object GetPropertyValue(string propertyName)
	{
		if (!Properties.TryGetValue(propertyName, out object value))
		{
			return null;
		}
		return value;
	}
}
