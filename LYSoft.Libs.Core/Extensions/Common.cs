﻿﻿using System.Text.Json;

namespace LYSoft.Libs;

/// <summary>
/// 通用扩展方法类
/// 提供常用的对象操作扩展方法，包括类型转换、对象克隆、属性复制等
/// 支持JSON序列化克隆、反射属性复制、异常处理等多种实用功能
/// 提供链式调用支持，简化对象操作代码
/// </summary>
public static partial class Extension {

    /// <summary>
    /// 安全的类型转换方法，转换失败时返回default值
    /// 使用强制类型转换(T)obj，在转换失败时捕获异常并返回default(T)
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="obj">要转换的对象</param>
    /// <returns>转换成功返回转换后的值，失败返回默认值</returns>
    public static T? As<T>(this object obj) { 
        try {
            return (T)obj;
        } catch {
            return default;
        }
    }

    /// <summary>
    /// 强制类型转换方法，不处理转换异常
    /// 直接使用(T)obj进行强制转换，如果转换失败会抛出异常
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="obj">要转换的对象</param>
    /// <returns>返回转换后的值</returns>
    /// <exception cref="InvalidCastException">当转换失败时抛出</exception>
    public static T AsTrue<T>(this object obj) {
        return (T)obj;
    }

    /// <summary>
    /// 泛型约束的类型转换方法
    /// 利用泛型约束TSocrce : TTarget确保转换的安全性
    /// </summary>
    /// <typeparam name="TSocrce">源类型，必须是目标类型的子类型</typeparam>
    /// <typeparam name="TTarget">目标类型</typeparam>
    /// <param name="obj">要转换的对象</param>
    /// <returns>返回转换后的对象</returns>
    public static TTarget As<TSocrce, TTarget>(this TSocrce obj) where TSocrce : TTarget {
        return obj;
    }

    /// <summary>
    /// 使用JSON序列化实现对象的深度克隆
    /// 通过JSON序列化和反序列化来实现对象的完全复制，简单替代传统的深度拷贝
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj">要克隆的对象</param>
    /// <returns>返回克隆后的新对象，如果序列化失败则返回默认值</returns>
    public static T? JsonClone<T>(this T obj) {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj));
    }

    /// <summary>
    /// 使用JSON序列化实现对象的深度克隆（非泛型版本）
    /// 通过JSON序列化和反序列化来实现对象的完全复制，使用反射获取对象类型
    /// </summary>
    /// <param name="obj">要克隆的对象</param>
    /// <returns>返回克隆后的新对象，如果序列化失败则返回默认值</returns>
    public static object? JsonClone(this object obj) {
        var jsonText = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize(jsonText, obj.GetType());
    }

    /// <summary>
    /// 获取异常的最底层内部异常
    /// 递归查找异常的InnerException，直到找到最深层的异常原因
    /// </summary>
    /// <param name="ex">要分析的异常</param>
    /// <returns>返回最底层的异常对象</returns>
    public static Exception GetInnestException(this Exception ex) {
        return ex.InnerException == null ? ex : GetInnestException(ex.InnerException);
    }

    /// <summary>
    /// 从源对象的属性创建record类型的目标对象
    /// 使用反射获取源对象属性，并通过record类型的构造函数创建目标对象
    /// 适用于record类型的对象转换和创建场景
    /// </summary>
    /// <typeparam name="TTarget">目标record类型</typeparam>
    /// <param name="source">源对象</param>
    /// <returns>返回创建的目标对象</returns>
    public static TTarget CopyPropertiesToRecord<TTarget>(this object source) where TTarget : class {
        var sourceProps = source.GetType().GetProperties().Select(x => new { x.PropertyType, x.Name, Value = x.GetValue(source) }).ToArray();
        var parameters = typeof(TTarget).GetConstructors().Where(x => x.IsPublic).First().GetParameters().Select(x => new { x.ParameterType, x.Name });
        var result = (TTarget)Activator.CreateInstance(typeof(TTarget), parameters.Select(x => sourceProps.SingleOrDefault(y => y.Name == x.Name)?.Value ?? default).ToArray())!;
        return result.CopyPropertiesFrom(source);
    }

    /// <summary>从目标对象中复制同名属性值到源对象中，返回源对象</summary>
    public static TSource CopyPropertiesFrom<TSource, TTarget>(this TSource source, TTarget target) {
        var propsSource = typeof(TSource).GetProperties().Where(x => x.CanWrite).Select(x => x.Name);
        var propsTarget = typeof(TTarget).GetProperties().Where(x => x.CanRead).Select(x => x.Name);

        var propsCommon = propsSource.Intersect(propsTarget);

        foreach (var prop in propsCommon) {
            try {
                var value = typeof(TTarget).GetProperty(prop)!.GetValue(target);
                typeof(TSource).GetProperty(prop)!.SetValue(source, value);
            } catch {
                continue;
            }
        }

        return source;
    }

    /// <summary>从源对象中复制属性到目标对象中，返回目标对象</summary>
    public static TTarget CopyPropertiesTo<TSource, TTarget>(this TSource source, TTarget target) {
        return target.CopyPropertiesFrom(source);
    }

    /// <summary>根据源对象的属性创建目标对象，返回目标对象</summary>
    public static TTarget CopyPropertiesTo<TTarget>(this object source) where TTarget : new() {
        var target = new TTarget();
        return target.CopyPropertiesFrom(source);
    }

    /// <summary>对单一对象执行方法并返回该对象</summary>
    public static T Do<T>(this T obj, Action<T> action) {
        action(obj);
        return obj;
    }

    /// <summary>满足条件时，对单一对象执行方法并返回该对象</summary>
    public static T DoIf<T>(this T obj, Action<T> action, params bool[] conditions) {
        if (conditions.All(x => x)) { obj.Do(action); }
        return obj;
    }

    /// <summary>对集合的所有对象执行指定方法</summary>
    public static IEnumerable<T> AllDo<T>(this IEnumerable<T> source, Action<T> action) {
        foreach (var item in source) {
            action(item);
        }
        return source;
    }
    
    /// <summary>对集合的所有对象执行指定方法</summary>
    public static IEnumerable<T> AllDo<T>(this IEnumerable<T> source, Action<T, int> action) {
        var index = 0;
        foreach (var item in source) {
            action(item, index);
            index++;
        }
        return source;
    }

    /// <summary>满足条件时，对集合的所有对象执行指定方法</summary>
    public static IEnumerable<T> AllDoIf<T>(this IEnumerable<T> source, Action<T> action, params bool[] conditions) {
        var run = conditions.All(x => x);
        foreach (var item in source) {
            if (run) { action(item); }
        }
        return source;
    }

    /// <summary>满足条件时，对集合的所有对象执行指定方法</summary>
    public static IEnumerable<T> AllDoIf<T>(this IEnumerable<T> source, Action<T, int> action, params bool[] conditions) {
        var run = conditions.All(x => x);
        var index = 0;
        foreach (var item in source) {
            if (run) { action(item, index); }
            index++;
        }
        return source;
    }

}