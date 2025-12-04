﻿#pragma warning disable CA1822 // 将成员标记为 static

namespace LYSoft.Libs.Editor;

/// <summary>
/// 对象编辑器服务类
/// 提供对象属性的可视化编辑功能，支持动态构建各种类型的编辑器界面
/// 通过EditorOptionBuilder构建器模式创建灵活的编辑器配置
/// 支持文本、数字、日期、布尔值、选择器等多种编辑器类型
/// </summary>
public class EditorService {

    /// <summary>
    /// 从现有对象创建编辑器构造器
    /// 为指定的对象实例创建一个编辑器配置构造器，用于后续配置编辑器选项
    /// </summary>
    /// <typeparam name="T">要编辑的对象类型</typeparam>
    /// <param name="obj">要编辑的对象实例</param>
    /// <returns>返回配置好对象的编辑器构造器实例</returns>
    public EditorOptionBuilder<T> CreateEditorBuilder<T>(T obj) => new(obj);

}

/// <summary>
/// 对象编辑器扩展方法类
/// 提供便捷的扩展方法，使任何对象都可以直接调用编辑器构造器方法
/// 简化编辑器的创建流程，提供更流畅的API体验
/// </summary>
public static class EditorExtension {

    /// <summary>
    /// 从现有对象创建编辑器构造器
    /// 扩展方法，允许任何对象直接调用此方法来创建编辑器构造器
    /// </summary>
    /// <typeparam name="T">要编辑的对象类型</typeparam>
    /// <param name="obj">要编辑的对象实例</param>
    /// <returns>返回配置好对象的编辑器构造器实例</returns>
    public static EditorOptionBuilder<T> CreateEditorBuilder<T>(this T obj) => new(obj);

    /// <summary>
    /// 创建空的编辑器构造器
    /// 创建一个未指定编辑对象的构造器，需要后续使用WithObject方法指定要编辑的对象
    /// 适用于需要延迟指定编辑对象或动态配置编辑对象的场景
    /// </summary>
    /// <typeparam name="T">要编辑的对象类型</typeparam>
    /// <returns>返回未配置对象的编辑器构造器实例</returns>
    /// <seealso cref="EditorOptionBuilder{TObject}.WithObject(TObject)"/>
    public static EditorOptionBuilder<T> CreateEditorBuilder<T>() => new();

}