﻿﻿using System.Collections.ObjectModel;

namespace LYSoft.Libs;

/// <summary>
/// 集合类型的扩展方法类
/// 提供对ICollection、ObservableCollection等集合类型的常用操作扩展
/// 包括批量添加、批量替换、条件删除等功能，简化集合操作代码
/// 支持链式调用，所有方法都返回原集合对象
/// </summary>
public static partial class Extension {

    /// <summary>
    /// 在集合末尾批量添加一组数据
    /// 使用循环批量添加，相比于多次调用Add方法更加高效
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">目标集合</param>
    /// <param name="datas">要添加的数据集合</param>
    /// <returns>返回原集合，支持链式调用</returns>
    public static ICollection<T> AppendRange<T>(this ICollection<T> source, IEnumerable<T> datas) {
        foreach (var data in datas) {
            source.Add(data);
        }
        return source;
    }

    /// <summary>
    /// 在ObservableCollection的开头批量添加一组数据
    /// 数据将按顺序插入集合的第0位，最终顺序与原数据集合一致
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">目标ObservableCollection</param>
    /// <param name="datas">要添加的数据集合</param>
    /// <returns>返回原集合，支持链式调用</returns>
    public static ObservableCollection<T> PrependRange<T>(this ObservableCollection<T> source, IEnumerable<T> datas) {
        foreach (var data in datas) {
            source.Insert(0, data);
        }
        return source;
    }

    /// <summary>
    /// 用新的一组数据完全替换现有的全部数据
    /// 先清空原集合，然后添加新数据，相当于Clear + AppendRange的组合操作
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">目标集合</param>
    /// <param name="datas">用于替换的新数据集合</param>
    /// <returns>返回原集合，支持链式调用</returns>
    public static ICollection<T> Reset<T>(this ICollection<T> source, IEnumerable<T> datas) {
        var newData = datas.ToList();
        source.Clear();
        return source.AppendRange(newData);
    }

    /// <summary>
    /// 在ObservableCollection中用一个新的数据替换一个旧的数据，保持位置不变
    /// 找到旧项目的位置，删除后在相同位置插入新项目，保持集合顺序
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">目标ObservableCollection</param>
    /// <param name="oldItem">要被替换的旧数据</param>
    /// <param name="newItem">用于替换的新数据</param>
    /// <returns>返回原集合，支持链式调用</returns>
    public static ObservableCollection<T> Replace<T>(this ObservableCollection<T> source, T oldItem, T newItem) {
        var index = source.IndexOf(oldItem);
        source.RemoveAt(index);
        source.Insert(index, newItem);
        return source;
    }

    /// <summary>
    /// 移除集合中满足指定条件的所有数据
    /// 使用LINQ查询符合条件的项目，然后逐一从集合中移除
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">目标集合</param>
    /// <param name="condition">用于判断是否删除的条件函数</param>
    /// <returns>返回原集合，支持链式调用</returns>
    public static ICollection<T> RemoveWhere<T>(this ICollection<T> source, Func<T, bool> condition) {
        foreach (var item in source.Where(condition).ToList()) {
            source.Remove(item);
        }
        return source;
    }

}