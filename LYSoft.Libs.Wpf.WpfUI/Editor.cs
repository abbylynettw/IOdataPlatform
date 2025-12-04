﻿using LYSoft.Libs.Editor;

namespace LYSoft.Libs.Wpf.WpfUI;

/// <summary>
/// 对象编辑器扩展方法类
/// 提供基于WpfUI的对象编辑功能扩展，允许使用现代化的WpfUI组件显示编辑对话框
/// 支持多种编辑器类型，提供统一的编辑界面和交互体验
/// 适用于需要美观的现代化对象编辑界面的场景
/// </summary>
public static class Extension
{

    /// <summary>
    /// 使用WpfUI编辑对象
    /// 创建一个基于WpfUI的编辑对话框，显示指定的编辑器选项并允许用户编辑对象属性
    /// 支持多种编辑器类型，包括文本、数字、日期、布尔值、选择器、文件选择器等
    /// </summary>
    /// <param name="options">编辑器选项配置，包含所有编辑器的定义和设置</param>
    /// <returns>返回true表示用户确认了编辑，false表示用户取消了编辑</returns>
    /// <remarks>
    /// 注意：目前暂未实现PickImageEditorOption图片选择编辑器
    /// 将创建一个EditorDialog对话框并以模态形式显示
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code>
    /// var options = new EditorOptions();
    /// options.AddTextEditor("Name", "名称");
    /// var result = options.EditWithWpfUI();
    /// if (result) {
    ///     // 用户确认了编辑
    /// }
    /// </code>
    /// </example>
    public static bool EditWithWpfUI(this EditorOptions options)
    {
        return new EditorDialog(options).ShowDialog().GetValueOrDefault(false);
    }

}