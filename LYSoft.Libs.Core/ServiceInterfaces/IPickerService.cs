﻿﻿﻿﻿namespace LYSoft.Libs.ServiceInterfaces;

/// <summary>
/// 应用程序的文件选择器服务接口
/// 定义各种文件和文件夹选择功能，包括打开文件、保存文件、选择文件夹等
/// 支持单文件和多文件选择，并提供文件类型筛选功能
/// 适用于WPF、WinForms和其他桌面应用程序的文件操作需求
/// </summary>
public interface IPickerService
{

    /// <summary>
    /// 打开多个文件选择对话框
    /// 允许用户同时选择多个文件，适用于批量处理文件的场景
    /// </summary>
    /// <returns>返回选中文件的完整路径数组，如用户取消操作则返回空数组</returns>
    public string[] OpenFiles();

    /// <summary>
    /// 打开多个文件选择对话框（带文件类型筛选）
    /// </summary>
    /// <param name="filter">文件类型筛选器，格式如："Text files(*.txt)|*.txt|All files(*.*)|*.*"</param>
    /// <returns>返回选中文件的完整路径数组，如用户取消操作则返回空数组</returns>
    public string[] OpenFiles(string filter);

    /// <summary>
    /// 打开单个文件选择对话框
    /// 仅允许用户选择一个文件，适用于大多数文件打开场景
    /// </summary>
    /// <returns>返回选中文件的完整路径，如用户取消操作则返回null</returns>
    public string? OpenFile();

    /// <summary>
    /// 打开单个文件选择对话框（带文件类型筛选）
    /// </summary>
    /// <param name="filter">文件类型筛选器</param>
    /// <returns>返回选中文件的完整路径，如用户取消操作则返回null</returns>
    public string? OpenFile(string filter);

    /// <summary>
    /// 打开单个文件选择对话框（带筛选器和默认文件名）
    /// 将在对话框中预设指定的文件名，方便用户操作
    /// </summary>
    /// <param name="filter">文件类型筛选器</param>
    /// <param name="filename">默认显示的文件名</param>
    /// <returns>返回选中文件的完整路径，如用户取消操作则返回null</returns>
    public string? OpenFile(string filter, string filename);

    /// <summary>
    /// 打开保存文件对话框
    /// 用于获取用户指定的文件保存路径，不实际保存文件
    /// </summary>
    /// <returns>返回用户选择的文件保存路径，如用户取消操作则返回null</returns>
    public string? SaveFile();

    /// <summary>
    /// 打开保存文件对话框（带文件类型筛选）
    /// </summary>
    /// <param name="filter">文件类型筛选器，用于限制保存的文件类型</param>
    /// <returns>返回用户选择的文件保存路径，如用户取消操作则返回null</returns>
    public string? SaveFile(string filter);

    /// <summary>
    /// 打开保存文件对话框（带筛选器和默认文件名）
    /// 将在保存对话框中预设指定的文件名
    /// </summary>
    /// <param name="filter">文件类型筛选器</param>
    /// <param name="filename">默认显示的文件名</param>
    /// <returns>返回用户选择的文件保存路径，如用户取消操作则返回null</returns>
    public string? SaveFile(string filter, string filename);

    /// <summary>
    /// 打开文件夹选择对话框
    /// 用于获取用户选择的目录路径，适用于批量处理文件或设置工作目录
    /// </summary>
    /// <returns>返回用户选择的文件夹完整路径，如用户取消操作则返回null</returns>
    public string? PickFolder();

    /// <summary>
    /// 选择文件夹并获取其中符合模式的文件列表
    /// 组合了文件夹选择和文件搜索的操作，适用于快速获取特定类型的文件
    /// </summary>
    /// <param name="searchPattern">文件搜索模式，如"*.txt"、"*.jpg"等</param>
    /// <returns>返回符合模式的文件路径数组</returns>
    public string[] PickFolderAndGetFiles(string searchPattern);

    /// <summary>
    /// 获取文件名称并去除扩展名
    /// 从完整的文件路径中提取不包含扩展名的纯文件名
    /// </summary>
    /// <param name="filePath">文件的完整路径</param>
    /// <returns>返回去除扩展名的文件名称</returns>
    public string GetFileNameWithoutExtension(string filePath);   

}