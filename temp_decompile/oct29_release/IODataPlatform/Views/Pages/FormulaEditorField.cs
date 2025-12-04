using System.Reflection;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 公式编辑器的字段封装类
/// 将字段的显示名称和对应的属性信息绑定在一起
/// 用于在界面上显示可编辑的字段并支持后续的反射操作
/// </summary>
/// <param name="Name">字段的显示名称</param>
/// <param name="property">对应的属性信息</param>
public class FormulaEditorField
{
	/// <summary>字段的自定义显示名称</summary>
	public string OwnDisplayName { get; }

	/// <summary>对应的属性名称</summary>
	public string PropertyName { get; }

	/// <summary>对应的属性信息对象</summary>
	public PropertyInfo Property { get; }

	/// <summary>
	/// 公式编辑器的字段封装类
	/// 将字段的显示名称和对应的属性信息绑定在一起
	/// 用于在界面上显示可编辑的字段并支持后续的反射操作
	/// </summary>
	/// <param name="Name">字段的显示名称</param>
	/// <param name="property">对应的属性信息</param>
	public FormulaEditorField(string Name, PropertyInfo property)
	{
		OwnDisplayName = Name;
		PropertyName = property.Name;
		Property = property;
		base._002Ector();
	}
}
