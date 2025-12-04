﻿namespace IODataPlatform.Models;

/// <summary>
/// 用户信息模型类
/// 管理用户的基本信息、部门归属和权限信息
/// 提供用户权限检查和显示文本格式化功能
/// </summary>
public partial class UserInfo : ObservableObject {

    /// <summary>用户姓名</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private string name = string.Empty;

    /// <summary>用户所属部门，支持多部门归属（使用Flags枚举）</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private Department department;

    /// <summary>用户权限，支持多权限组合（使用Flags枚举）</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private UserPermission permission;

    /// <summary>用户的唯一标识符</summary>
    public int Id { get; set; } 

    /// <summary>
    /// 获取用于界面显示的用户信息文本
    /// 格式：[部门] 用户名 或 未登录（当未设置用户名时）
    /// 如果用户属于多个部门，则不显示部门信息
    /// </summary>
    public string DisplayText {
        get {
            var depText = Department.ToString();
            depText = depText.Contains(',') ? "" : $"[{depText}] ";
            return string.IsNullOrEmpty(Name) ? "未登录" : $"{depText}{Name}";
        }
    }

    /// <summary>
    /// 统一使用此方法检查用户是否拥有指定权限
    /// </summary>
    /// <param name="department">需要检查的部门权限，不需要检查则传入 null</param>
    /// <param name="permission">需要检查的用户权限，不需要检查则传入 null</param>
    /// <returns>如果用户拥有所有指定的权限则返回true，否则返回false</returns>
    public bool Check(Department? department, UserPermission? permission) {
        if (department != null && !Department.HasFlag(department.Value)) { return false; }
        if (permission != null && !Permission.HasFlag(permission.Value)) { return false; }
        return true;
    }

}