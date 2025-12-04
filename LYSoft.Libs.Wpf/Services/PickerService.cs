﻿﻿﻿﻿using LYSoft.Libs.ServiceInterfaces;

using Microsoft.Win32;
using System.IO;

namespace LYSoft.Libs.Wpf.Services;

/// <summary>
/// WPF应用选择器服务实现类
/// 提供一组默认的文件和文件夹选择器方法，基于Windows原生对话框实现
/// 实现IPickerService接口，支持单文件、多文件、文件夹选择和文件保存功能
/// 支持文件过滤器、默认文件名设置，适用于文件管理、数据导入导出等业务场景
/// </summary>
public class PickerService : IPickerService
{

    /// <summary>
    /// 选择文件夹路径
    /// 打开Windows系统的文件夹选择对话框，允许用户选择一个文件夹
    /// </summary>
    /// <returns>返回用户选择的文件夹完整路径，如果用户取消则返回null</returns>
    public string? PickFolder()
    {
        var picker = new OpenFolderDialog();
        if (!picker.ShowDialog().GetValueOrDefault(false)) { return null; }
        return picker.FolderName;
    }

    /// <summary>
    /// 打开多个文件
    /// 打开Windows系统的文件选择对话框，允许用户同时选择多个文件
    /// </summary>
    /// <returns>返回用户选择的所有文件的完整路径数组，如果用户取消则返回空数组</returns>
    public string[] OpenFiles()
    {
        var picker = new Microsoft.Win32.OpenFileDialog() { Multiselect = true };
        if (!picker.ShowDialog().GetValueOrDefault(false)) { return []; }
        return picker.FileNames;
    }

    /// <summary>
    /// 按指定过滤器打开多个文件
    /// 打开Windows系统的文件选择对话框，按照指定的文件类型过滤器显示文件
    /// </summary>
    /// <param name="filter">文件类型过滤器，格式如"文本文件|*.txt|所有文件|*.*"</param>
    /// <returns>返回用户选择的所有文件的完整路径数组，如果用户取消则返回空数组</returns>
    public string[] OpenFiles(string filter)
    {
        var picker = new Microsoft.Win32.OpenFileDialog() { Filter = filter, Multiselect = true };
        if (!picker.ShowDialog().GetValueOrDefault(false)) { return []; }
        return picker.FileNames;
    }

    /// <summary>
    /// 打开单个文件
    /// 打开Windows系统的文件选择对话框，允许用户选择一个文件
    /// </summary>
    /// <returns>返回用户选择的文件完整路径，如果用户取消则返回null</returns>
    public string? OpenFile()
    {
        var picker = new Microsoft.Win32.OpenFileDialog();
        if (!picker.ShowDialog().GetValueOrDefault(false)) { return null; }
        return picker.FileName;
    }

    /// <summary>
    /// 按指定过滤器打开单个文件
    /// 打开Windows系统的文件选择对话框，按照指定的文件类型过滤器显示文件
    /// </summary>
    /// <param name="filter">文件类型过滤器，格式如"文本文件|*.txt|所有文件|*.*"</param>
    /// <returns>返回用户选择的文件完整路径，如果用户取消则返回null</returns>
    public string? OpenFile(string filter)
    {
        var picker = new Microsoft.Win32.OpenFileDialog() { Filter = filter };
        if (!picker.ShowDialog().GetValueOrDefault(false)) { return null; }
        return picker.FileName;
    }

    /// <summary>
    /// 按指定过滤器和默认文件名打开单个文件
    /// 打开Windows系统的文件选择对话框，并设置默认的文件名
    /// </summary>
    /// <param name="filter">文件类型过滤器，格式如"文本文件|*.txt|所有文件|*.*"</param>
    /// <param name="filename">默认的文件名（不包含路径）</param>
    /// <returns>返回用户选择的文件完整路径，如果用户取消则返回null</returns>
    public string? OpenFile(string filter, string filename)
    {
        var picker = new Microsoft.Win32.OpenFileDialog
        {
            Filter = filter,
            FileName = filename
        };
        if (!picker.ShowDialog().GetValueOrDefault(false)) { return null; }
        return picker.FileName;
    }

    /// <summary>
    /// 保存文件
    /// 打开Windows系统的文件保存对话框，允许用户选择文件保存位置
    /// </summary>
    /// <returns>返回用户选择的文件保存路径，如果用户取消则返回null</returns>
    public string? SaveFile()
    {
        var picker = new Microsoft.Win32.SaveFileDialog();
        if (!picker.ShowDialog().GetValueOrDefault(false)) { return null; }
        return picker.FileName;
    }

    /// <summary>
    /// 按指定过滤器保存文件
    /// 打开Windows系统的文件保存对话框，按照指定的文件类型过滤器显示可保存的文件格式
    /// </summary>
    /// <param name="filter">文件类型过滤器，格式如"文本文件|*.txt|Excel文件|*.xlsx"</param>
    /// <returns>返回用户选择的文件保存路径，如果用户取消则返回null</returns>
    public string? SaveFile(string filter)
    {
        var picker = new Microsoft.Win32.SaveFileDialog
        {
            Filter = filter
        };
        if (!picker.ShowDialog().GetValueOrDefault(false)) { return null; }
        return picker.FileName;
    }

    /// <summary>
    /// 按指定过滤器和默认文件名保存文件
    /// 打开Windows系统的文件保存对话框，并设置默认的文件名
    /// </summary>
    /// <param name="filter">文件类型过滤器，格式如"文本文件|*.txt|Excel文件|*.xlsx"</param>
    /// <param name="filename">默认的文件名（不包含路径）</param>
    /// <returns>返回用户选择的文件保存路径，如果用户取消则返回null</returns>
    public string? SaveFile(string filter, string filename)
    {
        var picker = new Microsoft.Win32.SaveFileDialog
        {
            Filter = filter,
            FileName = filename
        };
        if (!picker.ShowDialog().GetValueOrDefault(false)) { return null; }
        return picker.FileName;
    }
    /// <summary>
    /// 选择文件夹并获取匹配指定模式的所有文件
    /// 先让用户选择一个文件夹，然后根据搜索模式递归查找匹配的文件
    /// 支持多种文件扩展名的组合搜索，递归遍历所有子目录
    /// </summary>
    /// <param name="searchPattern">搜索模式，格式如"Excel文件|*.xls;*.xlsx"，会提取后面的扩展名部分</param>
    /// <returns>返回找到的所有匹配文件的完整路径数组，如果用户取消或无匹配文件则返回空数组</returns>
    /// <example>
    /// 使用示例：
    /// <code>
    /// var files = pickerService.PickFolderAndGetFiles("Excel文件|*.xls;*.xlsx");
    /// foreach (var file in files) {
    ///     // 处理每个找到的Excel文件
    /// }
    /// </code>
    /// </example>
    public string[] PickFolderAndGetFiles(string searchPattern)
    {
        var folderPath = PickFolder();
        if (string.IsNullOrEmpty(folderPath))
        {
            return Array.Empty<string>();
        }

        // 提取出扩展名模式，第二部分会是 "*.xls; *.xlsx"
        var patterns = searchPattern.Split('|').Last().Split(';').Select(p => p.Trim());

        var files = new List<string>();
        foreach (var pattern in patterns)
        {
            files.AddRange(Directory.GetFiles(folderPath, pattern, SearchOption.AllDirectories));
        }

        return files.ToArray();
    }





    /// <summary>
    /// 获取文件名（不包括扩展名）
    /// 从完整的文件路径中提取不包含扩展名的文件名部分
    /// </summary>
    /// <param name="filePath">完整的文件路径</param>
    /// <returns>返回不包含扩展名的文件名</returns>
    /// <example>
    /// 使用示例：
    /// <code>
    /// var name = pickerService.GetFileNameWithoutExtension(@"C:\path\to\file.xlsx");
    /// // 结果为"file"
    /// </code>
    /// </example>
    public string GetFileNameWithoutExtension(string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }
}