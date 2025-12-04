﻿﻿﻿namespace LYSoft.Libs;

/// <summary>
/// 条件链式编程扩展方法类
/// 提供基于条件的链式操作方法，支持在满足或不满足特定条件时执行相应的操作
/// 简化条件判断代码，提供更流畅的API体验，支持异常抛出和方法执行
/// 所有方法都返回原对象，保持链式调用的连续性
/// </summary>
public static partial class Extensions {

    #region if链式函数
    /// <summary>
    /// 测试值，如测试成功则执行指定方法，无论是否满足条件都返回值本身
    /// 支持链式调用，允许在满足条件时执行业务逻辑而不破坏调用链
    /// </summary>
    /// <typeparam name="T">对象类型，必须是引用类型</typeparam>
    /// <param name="obj">要测试的对象</param>
    /// <param name="condition">条件测试方法，接收对象参数并返回判断结果</param>
    /// <param name="action">测试成功时调用的方法</param>
    /// <returns>返回原对象，支持链式调用</returns>
    public static T If<T>(this T obj, Func<T, bool> condition, Action<T> action) where T : class {
        if (condition(obj)) { action(obj); }
        return obj;
    }

    /// <summary>
    /// 测试值，如测试成功则执行指定方法，无论是否满足条件都返回值本身
    /// 重载版本，接收布尔值条件，无需额外的函数包装
    /// </summary>
    /// <typeparam name="T">对象类型，必须是引用类型</typeparam>
    /// <param name="obj">要测试的对象</param>
    /// <param name="condition">布尔值测试条件</param>
    /// <param name="action">测试成功时调用的方法</param>
    /// <returns>返回原对象，支持链式调用</returns>
    public static T If<T>(this T obj, bool condition, Action<T> action) where T : class {
        return obj.If(_ => condition, action);
    }

    /// <summary>
    /// 测试值，如测试成功则抛出指定异常，无论是否满足条件都返回值本身
    /// 用于在满足特定条件时中断程序执行并报告错误
    /// </summary>
    /// <typeparam name="T">对象类型，必须是引用类型</typeparam>
    /// <param name="obj">要测试的对象</param>
    /// <param name="condition">条件测试方法</param>
    /// <param name="exception">测试成功时抛出的异常</param>
    /// <returns>返回原对象，支持链式调用</returns>
    /// <exception cref="Exception">当条件满足时抛出指定异常</exception>
    public static T If<T>(this T obj, Func<T, bool> condition, Exception exception) where T : class {
        return obj.If(condition, _ => throw exception);
    }

    /// <summary>
    /// 测试值，如测试成功则抛出指定异常，无论是否满足条件都返回值本身
    /// 重载版本，接收布尔值条件
    /// </summary>
    /// <typeparam name="T">对象类型，必须是引用类型</typeparam>
    /// <param name="obj">要测试的对象</param>
    /// <param name="condition">布尔值测试条件</param>
    /// <param name="exception">测试成功时抛出的异常</param>
    /// <returns>返回原对象，支持链式调用</returns>
    /// <exception cref="Exception">当条件满足时抛出指定异常</exception>
    public static T If<T>(this T obj, bool condition, Exception exception) where T : class {
        return obj.If(_ => condition, _ => throw exception);
    }

    /// <summary>
    /// 测试值，如测试失败则执行指定方法，无论是否满足条件都返回值本身
    /// 与If方法相反，在条件不满足时执行操作逻辑
    /// </summary>
    /// <typeparam name="T">对象类型，必须是引用类型</typeparam>
    /// <param name="obj">要测试的对象</param>
    /// <param name="condition">条件测试方法</param>
    /// <param name="action">测试失败时调用的方法</param>
    /// <returns>返回原对象，支持链式调用</returns>
    public static T IfNot<T>(this T obj, Func<T, bool> condition, Action<T> action) where T : class {
        if (!condition(obj)) { action(obj); }
        return obj;
    }

    /// <summary>
    /// 测试值，如测试失败则执行指定方法，无论是否满足条件都返回值本身
    /// 重载版本，接收布尔值条件
    /// </summary>
    /// <typeparam name="T">对象类型，必须是引用类型</typeparam>
    /// <param name="obj">要测试的对象</param>
    /// <param name="condition">布尔值测试条件</param>
    /// <param name="action">测试失败时调用的方法</param>
    /// <returns>返回原对象，支持链式调用</returns>
    public static T IfNot<T>(this T obj, bool condition, Action<T> action) where T : class {
        return obj.IfNot(_ => condition, action);
    }

    /// <summary>
    /// 测试值，如测试失败则抛出指定异常，无论是否满足条件都返回值本身
    /// 用于在不满足特定条件时中断程序执行并报告错误
    /// </summary>
    /// <typeparam name="T">对象类型，必须是引用类型</typeparam>
    /// <param name="obj">要测试的对象</param>
    /// <param name="condition">条件测试方法</param>
    /// <param name="exception">测试失败时抛出的异常</param>
    /// <returns>返回原对象，支持链式调用</returns>
    /// <exception cref="Exception">当条件不满足时抛出指定异常</exception>
    public static T IfNot<T>(this T obj, Func<T, bool> condition, Exception exception) where T : class {
        return obj.IfNot(condition, _ => throw exception);
    }

    /// <summary>
    /// 测试值，如测试失败则抛出指定异常，无论是否满足条件都返回值本身
    /// 重载版本，接收布尔值条件
    /// </summary>
    /// <typeparam name="T">对象类型，必须是引用类型</typeparam>
    /// <param name="obj">要测试的对象</param>
    /// <param name="condition">布尔值测试条件</param>
    /// <param name="exception">测试失败时抛出的异常</param>
    /// <returns>返回原对象，支持链式调用</returns>
    /// <exception cref="Exception">当条件不满足时抛出指定异常</exception>
    public static T IfNot<T>(this T obj, bool condition, Exception exception) where T : class {
        return obj.IfNot(_ => condition, _ => throw exception);
    }
    #endregion

}