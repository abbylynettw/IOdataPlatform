using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace IODataPlatform.Utilities;

/// <summary>
/// DisplayAttribute特性工具类
/// 提供获取类型和属性的DisplayAttribute信息的工具方法
/// 支持通过反射动态获取显示名称，用于界面显示和数据映射
/// </summary>
public static class DisplayAttributeHelper
{
	/// <summary>
	/// 获取指定类型的显示信息
	/// 包括类的显示名称和所有属性的显示名称映射
	/// </summary>
	/// <typeparam name="T">要获取显示信息的类型</typeparam>
	/// <returns>返回包含类名和字段显示名称的DisplayInfo对象</returns>
	public static DisplayInfo GetDisplayInfo<T>()
	{
		Type typeFromHandle = typeof(T);
		DisplayInfo displayInfo = new DisplayInfo();
		displayInfo.FieldDisplayNames = new Dictionary<string, string>();
		displayInfo.ClassName = typeFromHandle.GetCustomAttribute<DisplayAttribute>()?.Name ?? typeFromHandle.Name;
		PropertyInfo[] properties = typeFromHandle.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			DisplayAttribute customAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();
			if (customAttribute != null)
			{
				displayInfo.FieldDisplayNames.Add(propertyInfo.Name, customAttribute.Name);
			}
		}
		return displayInfo;
	}
}
