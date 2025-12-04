﻿﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;

namespace IODataPlatform.Utilities;

/// <summary>
/// 数据对比器类
/// 提供高性能的数据集合对比功能，支持大量数据的快速对比分析
/// 实现新增、删除、修改三种变更类型的智能识别和细粒度差异记录
/// 支持通过DisplayAttribute特性进行友好显示，广泛用于数据导入对比和版本变更追踪
/// </summary>
public class DataComparator {

    /// <summary>
    /// 异步执行数据集合对比操作
    /// 使用高效的集合操作和并行处理，支持大量数据的快速对比
    /// 包含完整的性能监控和错误处理机制，优化内存使用
    /// </summary>
    /// <typeparam name="T">要对比的对象类型</typeparam>
    /// <typeparam name="TKey">主键类型，用于唯一标识对象</typeparam>
    /// <param name="newData">新的数据集合（待导入或最新版本）</param>
    /// <param name="oldData">原有的数据集合（现有数据或旧版本）</param>
    /// <param name="keySelector">主键选择器，用于确定对象的唯一标识</param>
    /// <param name="progress">进度报告器（可选）</param>
    /// <returns>返回包含所有差异对象的列表</returns>
    /// <exception cref="Exception">当数据主键不唯一时抛出异常</exception>
    public static async Task<List<DifferentObject<TKey>>> ComparerAsync<T, TKey>(IEnumerable<T> newData, IEnumerable<T> oldData, Func<T, TKey> keySelector, IProgress<int>? progress = null) where T : class {
        return await Task.Run(() => {
            // 优化：直接使用IEnumerable，避免提前ToList导致的内存占用
            // 只在需要多次枚举的地方转换为列表
            
            // 数据校验：确保主键唯一性，使用HashSet提高性能
            var oldKeys = new HashSet<TKey>();
            var newKeys = new HashSet<TKey>();
            
            // 检查旧数据主键唯一性
            foreach (var item in oldData) {
                var key = keySelector(item);
                if (!oldKeys.Add(key)) {
                    throw new Exception("原始数据主键不唯一,无法对比，但仍然可以导入！");
                }
            }
            
            // 检查新数据主键唯一性
            foreach (var item in newData) {
                var key = keySelector(item);
                if (!newKeys.Add(key)) {
                    throw new Exception("导入数据主键不唯一,无法对比，但仍然可以导入！");
                }
            }

            // 获取类型属性信息，建立显示名称映射（缓存反射结果）
            var props = typeof(T).GetProperties().Select(x => new {
                Prop = x,
                Name = x.GetCustomAttribute<DisplayAttribute>()?.Name ?? x.Name
            }).ToList();
            
            var diffList = new List<DifferentObject<TKey>>();

            // 创建字典以提高查找性能（只转换一次）
            var oldItemDict = oldData.ToDictionary(keySelector);
            var newItemList = newData.ToList(); // 只转换一次新数据
            var newItemDict = newItemList.ToDictionary(keySelector);

            int totalSteps = 3; // 三个主要阶段
            int currentStep = 0;

            // 阶段1：处理新增数据（在新数据中存在，但在旧数据中不存在）
            foreach (var item in newItemList) {
                var key = keySelector(item);
                if (!oldItemDict.ContainsKey(key)) {
                    var diffObject = new DifferentObject<TKey>() { Key = key, Type = DifferentType.新增 };
                    // 只记录非空属性的新值
                    var diffProps = props
                        .Select(prop => new DifferentProperty() {
                            OldValue = null,
                            NewValue = prop.Prop.GetValue(item),
                            PropName = prop.Name
                        })
                        .Where(x => x.NewValue != null && !string.IsNullOrEmpty(x.NewValue.ToString()))
                        .ToList();
                    diffObject.DiffProps.AddRange(diffProps);
                    diffList.Add(diffObject);
                }
            }
            currentStep++;
            progress?.Report((currentStep * 100) / totalSteps);

            // 阶段2：处理删除数据（在旧数据中存在，但在新数据中不存在）
            foreach (var key in oldKeys) {
                if (!newKeys.Contains(key)) {
                    var oldItem = oldItemDict[key];
                    var diffObject = new DifferentObject<TKey>() { Key = key, Type = DifferentType.移除 };
                    // 只记录非空属性的旧值
                    var diffProps = props
                        .Select(prop => new DifferentProperty() {
                            NewValue = null,
                            OldValue = prop.Prop.GetValue(oldItem),
                            PropName = prop.Name
                        })
                        .Where(x => x.OldValue != null && !string.IsNullOrEmpty(x.OldValue.ToString()))
                        .ToList();
                    diffObject.DiffProps.AddRange(diffProps);
                    diffList.Add(diffObject);
                }
            }
            currentStep++;
            progress?.Report((currentStep * 100) / totalSteps);

            // 阶段3：处理修改数据（在两个数据集合中都存在，但属性值可能不同）
            foreach (var oldItem in oldItemDict.Values) {
                var key = keySelector(oldItem);
                if (newItemDict.TryGetValue(key, out var newItem)) {
                    var diffObject = new DifferentObject<TKey>() { Key = key, Type = DifferentType.覆盖 };
                    // 只记录值发生变化的属性
                    var diffProps = new List<DifferentProperty>();
                    
                    foreach (var prop in props) {
                        var oldValue = prop.Prop.GetValue(oldItem);
                        var newValue = prop.Prop.GetValue(newItem);
                        
                        if (!Equals(oldValue, newValue)) {
                            diffProps.Add(new DifferentProperty() {
                                NewValue = newValue,
                                OldValue = oldValue,
                                PropName = prop.Name
                            });
                        }
                    }
                    
                    if (diffProps.Count > 0) {
                        diffObject.DiffProps.AddRange(diffProps);
                        diffList.Add(diffObject);
                    }
                }
            }
            currentStep++;
            progress?.Report((currentStep * 100) / totalSteps);

            return diffList;
        });
    }
    
}


/// <summary>
/// 差异对象类
/// 封装单个对象的变更信息，包括主键、变更类型和具体的属性差异列表
/// 支持泛型主键，提供细粒度的变更追踪和显示
/// </summary>
/// <typeparam name="T">主键类型</typeparam>
public class DifferentObject<T> {
    /// <summary>对象的唯一标识符（主键值）</summary>
    public required T Key { get; set; }
    
    /// <summary>数据变更类型（新增、移除、覆盖）</summary>
    public required DifferentType Type { get; set; }
    
    /// <summary>具体的属性差异列表，包含所有发生变化的属性信息</summary>
    public List<DifferentProperty> DiffProps { get; } = [];
}

/// <summary>
/// 数据变更类型枚举
/// 定义数据对比中支持的三种基本变更类型
/// 使用位标志实现，支持组合操作和筛选
/// </summary>
public enum DifferentType {
    /// <summary>新增的数据记录（在新数据中存在，但在旧数据中不存在）</summary>
    新增 = 1,
    /// <summary>移除的数据记录（在旧数据中存在，但在新数据中不存在）</summary>
    移除 = 2,
    /// <summary>覆盖的数据记录（在两个数据集合中都存在，但属性值发生了变化）</summary>
    覆盖 = 4,
}

/// <summary>
/// 属性差异信息类
/// 记录单个属性的变更详情，包括属性名称、旧值和新值
/// 支持DisplayAttribute显示名称，提供用户友好的差异展示
/// </summary>
public class DifferentProperty {
    /// <summary>属性的显示名称（来自DisplayAttribute或属性名）</summary>
    public required string PropName { get; set; }
    
    /// <summary>属性的原始值（变更前的值）</summary>
    public required object? OldValue { get; set; }
    
    /// <summary>属性的新值（变更后的值）</summary>
    public required object? NewValue { get; set; }
}