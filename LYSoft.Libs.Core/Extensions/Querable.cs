﻿using System.Linq.Expressions;

namespace LYSoft.Libs;

/// <summary>
/// 查询扩展方法类
/// 为IQueryable和IEnumerable提供条件化查询的扩展方法
/// 支持根据条件动态添加Where子句，简化条件查询的代码编写
/// 适用于Entity Framework、LINQ to SQL等ORM查询和内存集合查询
/// </summary>
public static partial class Extensions {

    /// <summary>
    /// 条件化的Where查询扩展方法（适用于IQueryable）
    /// 当所有条件都为true时才添加Where子句，否则返回原查询
    /// 适用于Entity Framework等支持表达式树的ORM查询
    /// </summary>
    /// <typeparam name="T">查询元素类型</typeparam>
    /// <param name="query">原始查询</param>
    /// <param name="predicate">查询表达式</param>
    /// <param name="conditions">条件数组，全部为true时才应用谓词</param>
    /// <returns>返回条件化查询结果</returns>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, params bool[] conditions) {
        if (conditions.All(x => x)) { query = query.Where(predicate); }
        return query;
    }

    /// <summary>
    /// 条件化的Where查询扩展方法（适用于IEnumerable）
    /// 当所有条件都为true时才添加Where子句，否则返回原查询
    /// 适用于内存集合的LINQ查询
    /// </summary>
    /// <typeparam name="T">查询元素类型</typeparam>
    /// <param name="query">原始查询</param>
    /// <param name="predicate">查询委托</param>
    /// <param name="conditions">条件数组，全部为true时才应用谓词</param>
    /// <returns>返回条件化查询结果</returns>
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> query, Func<T, bool> predicate, params bool[] conditions) {
        if (conditions.All(x => x)) { query = query.Where(predicate); }
        return query;
    }

}