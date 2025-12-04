﻿﻿﻿﻿﻿namespace LYSoft.Libs.ServiceInterfaces;

/// <summary>
/// 应用程序的消息提示服务接口
/// 定义应用程序中各种类型的消息提示和用户交互功能
/// 包括一般消息、成功消息、警告消息、错误消息等不同级别的通知
/// 支持弹窗提示、确认对话框等交互形式，所有方法都采用异步实现
/// </summary>
public interface IMessageService
{

    /// <summary>
    /// 显示一般消息提示
    /// 用于显示无特定级别的信息内容，通常为中性提示
    /// </summary>
    /// <param name="message">要显示的消息内容</param>
    /// <returns>返回表示异步操作的Task</returns>
    public Task MessageAsync(string message);

    /// <summary>
    /// 显示带标题的一般消息提示
    /// </summary>
    /// <param name="message">要显示的消息内容</param>
    /// <param name="title">消息窗口的标题</param>
    /// <returns>返回表示异步操作的Task</returns>
    public Task MessageAsync(string message, string title);

    /// <summary>
    /// 显示成功消息提示
    /// 用于通知用户操作成功完成，通常使用绿色或成功图标显示
    /// </summary>
    /// <param name="message">成功消息内容</param>
    /// <returns>返回表示异步操作的Task</returns>
    public Task SuccessAsync(string message);

    /// <summary>
    /// 显示带标题的成功消息提示
    /// </summary>
    /// <param name="message">成功消息内容</param>
    /// <param name="title">消息窗口的标题</param>
    /// <returns>返回表示异步操作的Task</returns>
    public Task SuccessAsync(string message, string title);

    /// <summary>
    /// 显示警告消息提示
    /// 用于通知用户需要注意的信息或潜在问题，通常使用黄色或警告图标显示
    /// </summary>
    /// <param name="message">警告消息内容</param>
    /// <returns>返回表示异步操作的Task</returns>
    public Task WarnAsync(string message);

    /// <summary>
    /// 显示带标题的警告消息提示
    /// </summary>
    /// <param name="message">警告消息内容</param>
    /// <param name="title">消息窗口的标题</param>
    /// <returns>返回表示异步操作的Task</returns>
    public Task WarnAsync(string message, string title);

    /// <summary>
    /// 显示错误消息提示
    /// 用于通知用户发生了错误或异常情况，通常使用红色或错误图标显示
    /// </summary>
    /// <param name="message">错误消息内容</param>
    /// <returns>返回表示异步操作的Task</returns>
    public Task ErrorAsync(string message);

    /// <summary>
    /// 显示带标题的错误消息提示
    /// </summary>
    /// <param name="message">错误消息内容</param>
    /// <param name="title">消息窗口的标题</param>
    /// <returns>返回表示异步操作的Task</returns>
    public Task ErrorAsync(string message, string title);

    /// <summary>
    /// 显示模态弹窗消息提示
    /// 与一般消息不同，此方法显示的是模态对话框，用户必须关闭才能继续操作
    /// </summary>
    /// <param name="message">要显示的消息内容</param>
    /// <param name="title">弹窗的标题</param>
    /// <returns>返回表示异步操作的Task</returns>
    public Task AlertAsync(string message, string title);
    
    /// <summary>
    /// 显示模态弹稗消息提示（使用默认标题）
    /// </summary>
    /// <param name="message">要显示的消息内容</param>
    /// <returns>返回表示异步操作的Task</returns>
    public Task AlertAsync(string message);

    /// <summary>
    /// 显示确认对话框（包含“确认”和“取消”按钮）
    /// 用于获取用户的确认或取消选择，常用于删除、修改等重要操作的二次确认
    /// </summary>
    /// <param name="message">要显示的确认消息内容</param>
    /// <returns>用户点击确认返回true，点击取消或关闭返回false</returns>
    public Task<bool> ConfirmAsync(string message);

    /// <summary>
    /// 显示带标题的确认对话框
    /// </summary>
    /// <param name="message">要显示的确认消息内容</param>
    /// <param name="title">确认对话框的标题</param>
    /// <returns>用户点击确认返回true，点击取消或关闭返回false</returns>
    public Task<bool> ConfirmAsync(string message, string title);

    /// <summary>
    /// 显示三选项确认对话框（包含"是"、"否"和"取消"按钮）
    /// 用于需要三种选择的场景，如保存文件时遇到同名文件的处理
    /// </summary>
    /// <param name="message">要显示的确认消息内容</param>
    /// <returns>点击"是"返回true，点击"否"返回false，点击"取消"或关闭返回null</returns>
    public Task<bool?> ConfirmWithCancelAsync(string message);

    /// <summary>
    /// 显示带标题的三选项确认对话框
    /// </summary>
    /// <param name="message">要显示的确认消息内容</param>
    /// <param name="title">确认对话框的标题</param>
    /// <returns>点击"是"返回true，点击"否"返回false，点击"取消"或关闭返回null</returns>
    public Task<bool?> ConfirmWithCancelAsync(string message, string title);

}