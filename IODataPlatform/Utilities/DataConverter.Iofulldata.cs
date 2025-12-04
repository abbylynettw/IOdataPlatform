﻿using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Models;
using LYSoft.Libs.ServiceInterfaces;
using SqlSugar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Utilities
{
    /// <summary>
    /// 数据转换器的IoFullData处理部分
    /// 专门处理旧版本数据向IoFullData模型的转换
    /// 支持多种控制系统的字段映射和数据类型转换
    /// 包含完整的错误处理和缺失字段检测机制
    /// </summary>
    public partial class DataConverter
    {
        /// <summary>
        /// 将旧版本数据表转换为IoFullData对象集合
        /// 根据不同控制系统的字段映射配置，智能转换数据类型和值
        /// 支持缺失字段检测和额外字段提示，确保数据转换的完整性
        /// 使用并发集合提高大数据量处理性能
        /// </summary>
        /// <param name="dataTable">源数据表，包含旧版本格式的IO数据</param>
        /// <param name="msg">消息服务，用于显示转换过程中的提示信息</param>
        /// <param name="db">SqlSugar数据库上下文，用于查询字段映射配置</param>
        /// <param name="controlSystem">目标控制系统类型</param>
        /// <returns>返回转换后的IoFullData对象集合</returns>
        public static IEnumerable<IoFullData> ConvertOldDataTableToIoFullData(
       this DataTable dataTable,
       IMessageService msg,
       SqlSugarClient db,
       ControlSystem controlSystem)
        {
            var mappings = new List<config_controlSystem_mapping>();
            switch (controlSystem)
            {
                case ControlSystem.龙鳍:
                    mappings = db.Queryable<config_controlSystem_mapping>()
                        .Where(it => !string.IsNullOrEmpty(it.LqOld) && !string.IsNullOrEmpty(it.StdField)).ToList();
                    break;
                case ControlSystem.中控:
                    mappings = db.Queryable<config_controlSystem_mapping>()
                        .Where(it => !string.IsNullOrEmpty(it.ZkOld) && !string.IsNullOrEmpty(it.StdField)).ToList();
                    break;
                case ControlSystem.龙核:
                    mappings = db.Queryable<config_controlSystem_mapping>()
                        .Where(it => !string.IsNullOrEmpty(it.LhOld) && !string.IsNullOrEmpty(it.StdField)).ToList();
                    break;
                case ControlSystem.一室:
                    mappings = db.Queryable<config_controlSystem_mapping>()
                         .Where(it => !string.IsNullOrEmpty(it.Xt1Old) && !string.IsNullOrEmpty(it.StdField)).ToList();
                    break;
                case ControlSystem.安全级模拟系统:
                    mappings = db.Queryable<config_controlSystem_mapping>()
                         .Where(it => !string.IsNullOrEmpty(it.AQJMNOld) && !string.IsNullOrEmpty(it.StdField)).ToList();
                    break;
                default:
                    mappings = db.Queryable<config_controlSystem_mapping>()
                         .Where(it => !string.IsNullOrEmpty(it.StdField)).ToList(); break;
                   
            }           
            var propertyInfos = typeof(IoFullData).GetProperties()
                .Where(p => p.CanWrite)
                .Select(p => new
                {
                    Property = p,
                    DisplayName = p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name
                })
                .ToDictionary(p => p.DisplayName, p => p.Property);

            var mappingDict = mappings
                .Where(mapping => !string.IsNullOrEmpty(mapping.StdField))
                .GroupBy(mapping => mapping.StdField)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(mapping => controlSystem switch
                    {
                        ControlSystem.龙鳍 when !string.IsNullOrEmpty(mapping.LqOld) => mapping.LqOld,
                        ControlSystem.中控 when !string.IsNullOrEmpty(mapping.ZkOld) => mapping.ZkOld,
                        ControlSystem.龙核 when !string.IsNullOrEmpty(mapping.LhOld) => mapping.LhOld,
                        ControlSystem.一室 when !string.IsNullOrEmpty(mapping.Xt1Old) => mapping.Xt1Old,
                        ControlSystem.安全级模拟系统 when !string.IsNullOrEmpty(mapping.AQJMNOld) => mapping.AQJMNOld,
                        _ => mapping.StdField
                    }).First()
                );


            var columnsIndexCache = mappingDict
                .Select(kvp => new
                {
                    StdField = kvp.Key,
                    ColumnName = kvp.Value,
                    ColumnIndex = dataTable.Columns.Contains(kvp.Value) ? dataTable.Columns[kvp.Value].Ordinal : -1,
                    Property = propertyInfos.ContainsKey(kvp.Key) ? propertyInfos[kvp.Key] : null
                })
                .Where(x => x.Property != null)
                .ToList();

            var ioFullDataList = new ConcurrentBag<IoFullData>();

            var missingColumns = new List<string>();
            var extraColumns = new List<string>();

            // 找出DataTable中有但在配置表中没有的列
            foreach (DataColumn column in dataTable.Columns)
            {
                if (!mappingDict.ContainsValue(column.ColumnName))
                {
                    extraColumns.Add(column.ColumnName);
                }
            }

            foreach (var row in dataTable.AsEnumerable())
            {
                var result = new IoFullData();
                bool anyColumnFound = false;

                foreach (var mapping in columnsIndexCache)
                {
                    // 先检查列是否存在
                    if (mapping.ColumnIndex >= 0)
                    {
                        var value = row[mapping.ColumnIndex]?.ToString(); // 允许值为空
                        if (!string.IsNullOrEmpty(value))
                        {
                            // 即使 value 为空，列也存在，处理为默认值即可
                            SetIoFullDataValue(result, mapping.Property, value);
                            anyColumnFound = true;
                        }
                    }
                    else
                    {
                        // 只有在列不存在时，才记录缺失的列
                        missingColumns.Add($"当前平台列名： {mapping.ColumnName}");
                    }
                }

                if (anyColumnFound)
                {
                    ioFullDataList.Add(result);
                }
            }        

            // 生成并输出提示信息
            var log = new StringBuilder();

            if (missingColumns.Any())
            {
                log.AppendLine("以下配置表中的列未在Excel文件中找到：");
                foreach (var col in missingColumns.Distinct())
                {
                    log.AppendLine($"{col}");
                }
            }

            if (extraColumns.Any())
            {
                log.AppendLine("以下Excel中的列未在配置表中映射，请到数据资产中心配置表配置：");
                foreach (var col in extraColumns.Distinct())
                {
                    log.AppendLine($"- {col}");
                }
            }

            if (log.Length > 0)
            {
                msg.AlertAsync(log.ToString());
            }

            return ioFullDataList;
        }

        private static void SetIoFullDataValue(IoFullData result, PropertyInfo property, string value)
        {
            var propertyType = property.PropertyType;

            if (propertyType == typeof(string))
            {
                property.SetValue(result, value);
            }
            else if (propertyType == typeof(bool))
            {
                property.SetValue(result, bool.TryParse(value, out var resultValue) ? resultValue : false);
            }
            else if (propertyType == typeof(int))
            {
                property.SetValue(result, int.TryParse(value, out var resultValue) ? resultValue : 0);
            }
            else if (propertyType == typeof(int?))
            {
                property.SetValue(result, int.TryParse(value, out var resultValue) ? resultValue : (int?)null);
            }
            else if (propertyType == typeof(float))
            {
                property.SetValue(result, float.TryParse(value, out var resultValue) ? resultValue : 0f);
            }
            else if (propertyType == typeof(float?))
            {
                property.SetValue(result, float.TryParse(value, out var resultValue) ? resultValue : (float?)null);
            }
            else if (propertyType == typeof(double))
            {
                property.SetValue(result, double.TryParse(value, out var resultValue) ? resultValue : 0d);
            }
            else if (propertyType == typeof(double?))
            {
                property.SetValue(result, double.TryParse(value, out var resultValue) ? (double?)resultValue : null);
            }
            else if (propertyType == typeof(DateTime?))
            {
                property.SetValue(result, DateTime.TryParse(value, out var resultValue) ? resultValue : (DateTime?)null);
            }
            else if (propertyType == typeof(TagType))
            {
                // 尝试将字符串解析为整数并直接转换为 TagType 枚举
                if (int.TryParse(value, out int intValue))
                {
                    property.SetValue(result, (TagType)intValue);
                }
                else
                {
                    // 设置默认值或处理错误情况
                    property.SetValue(result, TagType.Normal);
                }
            }

            else if (propertyType == typeof(Xt2NetType))
            {
                property.SetValue(result, value switch
                {
                    "Net1" => Xt2NetType.Net1,
                    "Net2" => Xt2NetType.Net2,
                    _ => Xt2NetType.Net1,
                });
            }
            else if (propertyType == typeof(盘箱柜类别))
            {
                property.SetValue(result, value switch
                {
                    "盘台" => 盘箱柜类别.盘台,
                    "阀箱" => 盘箱柜类别.阀箱,
                    "机柜" => 盘箱柜类别.机柜,
                    _ => throw new NotImplementedException(),
                });
            }
            else
            {
                throw new NotImplementedException($"No converter implemented for type '{propertyType.Name}'.");
            }
        }



        public static string GetControlSystemField(SqlSugarClient db, ControlSystem controlSystem, string stdDisplayName)
        {
            // 查询映射表，根据不同的控制系统筛选字段
            var mapping = db.Queryable<config_controlSystem_mapping>()
                .Where(m => m.StdField == stdDisplayName).ToList().FirstOrDefault();

            // 如果没有找到匹配的映射，返回 null
            if (mapping == null) throw new Exception($"未在config_controlSystem_mapping表中找到{stdDisplayName} 标准字段");

            // 根据控制系统获取对应的字段名称
            string? fieldName = controlSystem switch
            {
                ControlSystem.龙鳍 => mapping.LqOld,
                ControlSystem.中控 => mapping.ZkOld,
                ControlSystem.龙核 => mapping.LhOld,
                ControlSystem.一室 => mapping.Xt1Old,
                _ => null
            };

            // 返回对应的字段名称，如果 FieldName 为空，返回 StdField
            return !string.IsNullOrEmpty(fieldName) ? fieldName : mapping.StdField;
        }
    }
}
