using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;

namespace IODataPlatform.Models;

/// <summary>
/// 用户信息模型类
/// 管理用户的基本信息、部门归属和权限信息
/// 提供用户权限检查和显示文本格式化功能
/// </summary>
/// <inheritdoc />
public class UserInfo : ObservableObject
{
	/// <summary>用户姓名</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor("DisplayText")]
	private string name = string.Empty;

	/// <summary>用户所属部门，支持多部门归属（使用Flags枚举）</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor("DisplayText")]
	private Department department;

	/// <summary>用户权限，支持多权限组合（使用Flags枚举）</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor("DisplayText")]
	private UserPermission permission;

	/// <summary>用户的唯一标识符</summary>
	public int Id { get; set; }

	/// <summary>
	/// 获取用于界面显示的用户信息文本
	/// 格式：[部门] 用户名 或 未登录（当未设置用户名时）
	/// 如果用户属于多个部门，则不显示部门信息
	/// </summary>
	public string DisplayText
	{
		get
		{
			string text = Department.ToString();
			text = (text.Contains(',') ? "" : ("[" + text + "] "));
			if (!string.IsNullOrEmpty(Name))
			{
				return text + Name;
			}
			return "未登录";
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.UserInfo.name" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Name
	{
		get
		{
			return name;
		}
		[MemberNotNull("name")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(name, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Name);
				name = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Name);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DisplayText);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.UserInfo.department" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public Department Department
	{
		get
		{
			return department;
		}
		set
		{
			if (!EqualityComparer<Department>.Default.Equals(department, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Department);
				department = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Department);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DisplayText);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.UserInfo.permission" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public UserPermission Permission
	{
		get
		{
			return permission;
		}
		set
		{
			if (!EqualityComparer<UserPermission>.Default.Equals(permission, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Permission);
				permission = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Permission);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DisplayText);
			}
		}
	}

	/// <summary>
	/// 统一使用此方法检查用户是否拥有指定权限
	/// </summary>
	/// <param name="department">需要检查的部门权限，不需要检查则传入 null</param>
	/// <param name="permission">需要检查的用户权限，不需要检查则传入 null</param>
	/// <returns>如果用户拥有所有指定的权限则返回true，否则返回false</returns>
	public bool Check(Department? department, UserPermission? permission)
	{
		if (department.HasValue && !Department.HasFlag(department.Value))
		{
			return false;
		}
		if (permission.HasValue && !Permission.HasFlag(permission.Value))
		{
			return false;
		}
		return true;
	}
}
