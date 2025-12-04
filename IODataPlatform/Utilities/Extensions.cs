﻿using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using LYSoft.Libs.ServiceInterfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SqlSugar;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text;

namespace IODataPlatform.Utilities;

/// <summary>
/// 通用扩展方法集合
/// 提供数据类型转换、DataTable操作、对象映射等核心功能
/// 支持DisplayAttribute特性的自动映射，广泛用于Excel导入导出和数据绑定
/// 实现复杂类型的智能转换，包括枚举、可空类型、基础数据类型等
/// </summary>
public static partial class Extensions {

    /// <summary>
    /// 将DataTable转换为指定类型的对象集合
    /// 使用DisplayAttribute特性进行属性名映射，支持多种数据类型的智能转换
    /// 包含完整的类型安全处理和异常容错机制
    /// </summary>
    /// <typeparam name="T">目标对象类型，必须具有无参构造函数</typeparam>
    /// <param name="table">源DataTable对象</param>
    /// <returns>返回转换后的对象集合</returns>
    /// <exception cref="NotImplementedException">当遇到未支持的数据类型时抛出</exception>
    public static IEnumerable<T> StringTableToIEnumerableByDiplay<T>(this DataTable table) where T : new() {
        // 获取类型T的可写属性，并建立DisplayAttribute映射关系
        var properties = typeof(T).GetProperties()
            .Where(x => x.CanWrite)
            .Select(x => new { Property = x, DisplayName = x.GetCustomAttribute<DisplayAttribute>()?.Name ?? x.Name })
            .Where(x => table.Columns.Contains(x.DisplayName))
            .ToList();

        return table.Rows.Cast<DataRow>().Select(row => {
            var result = new T();
            foreach (var property in properties) {
                try {
                    var value = $"{row[property.DisplayName]}";
                    
                    // 处理空值情况
                    if (string.IsNullOrEmpty(value))
                    {
                        if (property.Property.PropertyType == typeof(int) || property.Property.PropertyType == typeof(int?))
                        {
                            property.Property.SetValue(result, 0);
                        }
                        else
                        {
                            property.Property.SetValue(result, null);
                        }
                    }
                    // 字符串类型直接赋值
                    else if (property.Property.PropertyType == typeof(string))
                    {
                        property.Property.SetValue(result, value);
                    }
                    // 布尔类型转换，异常时默认为false
                    else if (property.Property.PropertyType == typeof(bool))
                    {
                        try { property.Property.SetValue(result, bool.Parse(value)); } catch { property.Property.SetValue(result, false); }
                    }
                    // 整数类型转换，异常时默认为0
                    else if (property.Property.PropertyType == typeof(int))
                    {
                        try { property.Property.SetValue(result, int.Parse(value)); } catch { property.Property.SetValue(result, 0); }
                    }
                    // 可空整数类型转换，异常时为null
                    else if (property.Property.PropertyType == typeof(int?))
                    {
                        try { property.Property.SetValue(result, int.Parse(value)); } catch { property.Property.SetValue(result, null); }
                    }
                    // 浮点数类型转换，异常时默认为0
                    else if (property.Property.PropertyType == typeof(float))
                    {
                        try { property.Property.SetValue(result, float.Parse(value)); } catch { property.Property.SetValue(result, 0); }
                    }
                    // 可空浮点数类型转换，异常时为null
                    else if (property.Property.PropertyType == typeof(float?))
                    {
                        try { property.Property.SetValue(result, float.Parse(value)); } catch { property.Property.SetValue(result, null); }
                    }
                    // 十进制数类型转换，异常时默认为0
                    else if (property.Property.PropertyType == typeof(decimal))
                    {
                        try { property.Property.SetValue(result, decimal.Parse(value)); } catch { property.Property.SetValue(result, 0); }
                    }
                    // 可空十进制数类型转换，异常时为null
                    else if (property.Property.PropertyType == typeof(decimal?))
                    {
                        try { property.Property.SetValue(result, decimal.Parse(value)); } catch { property.Property.SetValue(result, null); }
                    }
                    // 双精度浮点数类型转换，异常时默认为0
                    else if (property.Property.PropertyType == typeof(double))
                    {
                        try { property.Property.SetValue(result, double.Parse(value)); } catch { property.Property.SetValue(result, 0); }
                    }
                    // 可空双精度浮点数类型转换，异常时为null
                    else if (property.Property.PropertyType == typeof(double?))
                    {
                        try { property.Property.SetValue(result, double.Parse(value)); } catch { property.Property.SetValue(result, null); }
                    }
                    // 可空日期时间类型转换，异常时为null
                    else if (property.Property.PropertyType == typeof(DateTime?))
                    {
                        try { property.Property.SetValue(result, DateTime.Parse(value)); } catch { property.Property.SetValue(result, null); }
                    }
                    // TagType枚举转换，支持整数和默认值处理
                    else if (property.Property.PropertyType == typeof(TagType))
                    {
                        // 尝试将字符串解析为整数并直接转换为 TagType 枚举
                        if (int.TryParse(value, out int intValue))
                        {
                            property.Property.SetValue(result, (TagType)intValue);
                        }
                        else
                        {
                            // 设置默认值或处理错误情况
                            property.Property.SetValue(result, TagType.Normal);
                        }
                    }
                    // Xt2NetType枚举转换，支持字符串匹配
                    else if (property.Property.PropertyType == typeof(Xt2NetType))
                    {
                        property.Property.SetValue(result, value switch
                        {
                            "Net1" => Xt2NetType.Net1,
                            "Net2" => Xt2NetType.Net2,
                            _ => Xt2NetType.Net1,
                        });
                    }
                    // 盘箱柜类别枚举转换，支持中文字符串匹配
                    else if (property.Property.PropertyType == typeof(盘箱柜类别))
                    {
                        property.Property.SetValue(result, value switch
                        {
                            "盘台" => 盘箱柜类别.盘台,
                            "阀箱" => 盘箱柜类别.阀箱,
                            "机柜" => 盘箱柜类别.机柜,
                            _ => throw new NotImplementedException(),
                        });
                    }
                    // config_cable_systemNumber类型跳过处理
                    else if (property.Property.PropertyType == typeof(config_cable_systemNumber))
                    {
                        continue;                    
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                } catch (NotImplementedException ex) {
                    throw new("开发人员注意：需要匹配更多类型");
                }
                catch (Exception ex)
                {
                    // 静默处理其他异常，继续处理下一个属性
                }

            }
            return result;
        });
    }

    /// <summary>
    /// 将对象集合转换为DataTable，使用DisplayAttribute特性作为列名
    /// 支持异步操作，自动处理可空类型和DBNull值转换
    /// 广泛用于Excel导出和数据绑定场景
    /// </summary>
    /// <typeparam name="T">源对象类型</typeparam>
    /// <param name="data">要转换的对象集合</param>
    /// <returns>返回包含转换结果的DataTable</returns>
    public static async Task<DataTable> ToTableByDisplayAttributeAsync<T>(this IEnumerable<T> data)
    {
        return await Task.Run(() => {
            var dataTable = new DataTable();
            var props = typeof(T).GetProperties().ToList();
            // 建立属性名到DisplayAttribute显示名的映射字典
            var mappingDic = props.ToDictionary(x => x.Name, x => x.GetCustomAttribute<DisplayAttribute>()?.Name ?? x.Name);
            // 创建列，使用DisplayAttribute名称，并处理可空类型
            props.ForEach(x => dataTable.Columns.Add(mappingDic[x.Name], Nullable.GetUnderlyingType(x.PropertyType) ?? x.PropertyType));
            // 填充数据行，null值转换为DBNull.Value
            data.ToList().ForEach(x => {
                var row = dataTable.NewRow();
                props.ForEach(prop => row[mappingDic[prop.Name]] = prop.GetValue(x) ?? DBNull.Value);
                dataTable.Rows.Add(row);
            });
            return dataTable;
        });
    }

    /// <summary>
    /// 将对象集合转换为DataTable，直接使用属性名作为列名
    /// 支持异步操作，适用于不需要DisplayAttribute映射的简单转换场景
    /// </summary>
    /// <typeparam name="T">源对象类型</typeparam>
    /// <param name="data">要转换的对象集合</param>
    /// <returns>返回包含转换结果的DataTable</returns>
    public static async Task<DataTable> ToTableByPropertyNameAsync<T>(this IEnumerable<T> data)
    {
        return await Task.Run(() => {
            var dataTable = new DataTable();
            var props = typeof(T).GetProperties().ToList();
            // 直接使用属性名创建列
            props.ForEach(x => dataTable.Columns.Add(x.Name));
            // 填充数据行
            data.ToList().ForEach(x => {
                var row = dataTable.NewRow();
                props.ForEach(prop => row[prop.Name] = prop.GetValue(x));
                dataTable.Rows.Add(row);
            });
            return dataTable;
        });
    }
 

}