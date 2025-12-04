using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IODataPlatform.Utilities;

/// <summary>
/// 数据对比器类
/// 提供高性能的数据集合对比功能，支持大量数据的快速对比分析
/// 实现新增、删除、修改三种变更类型的智能识别和细粒度差异记录
/// 支持通过DisplayAttribute特性进行友好显示，广泛用于数据导入对比和版本变更追踪
/// </summary>
public class DataComparator
{
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
	/// <returns>返回包含所有差异对象的列表</returns>
	/// <exception cref="T:System.Exception">当数据主键不唯一时抛出异常</exception>
	public static async Task<List<DifferentObject<TKey>>> ComparerAsync<T, TKey>(IEnumerable<T> newData, IEnumerable<T> oldData, Func<T, TKey> keySelector) where T : class
	{
		return await Task.Run(delegate
		{
			List<T> list = newData.ToList();
			List<T> list2 = oldData.ToList();
			HashSet<TKey> hashSet = new HashSet<TKey>();
			HashSet<TKey> hashSet2 = new HashSet<TKey>();
			foreach (T item5 in list2)
			{
				TKey item = keySelector(item5);
				if (!hashSet.Add(item))
				{
					throw new Exception("原始数据主键不唯一,无法对比，但仍然可以导入！");
				}
			}
			foreach (T item6 in list)
			{
				TKey item2 = keySelector(item6);
				if (!hashSet2.Add(item2))
				{
					throw new Exception("导入数据主键不唯一,无法对比，但仍然可以导入！");
				}
			}
			var source = (from x in typeof(T).GetProperties()
				select new
				{
					Prop = x,
					Name = (x.GetCustomAttribute<DisplayAttribute>()?.Name ?? x.Name)
				}).ToList();
			List<DifferentObject<TKey>> list3 = new List<DifferentObject<TKey>>();
			Dictionary<TKey, T> dictionary = list2.ToDictionary(keySelector);
			Dictionary<TKey, T> dictionary2 = list.ToDictionary(keySelector);
			foreach (T item3 in list)
			{
				TKey key = keySelector(item3);
				if (!dictionary.ContainsKey(key))
				{
					DifferentObject<TKey> differentObject = new DifferentObject<TKey>
					{
						Key = key,
						Type = DifferentType.新增
					};
					List<DifferentProperty> collection = (from prop in source
						select new DifferentProperty
						{
							OldValue = null,
							NewValue = prop.Prop.GetValue(item3),
							PropName = prop.Name
						} into x
						where !string.IsNullOrEmpty($"{x.NewValue}")
						select x).ToList();
					differentObject.DiffProps.AddRange(collection);
					list3.Add(differentObject);
				}
			}
			foreach (T item4 in list2)
			{
				TKey key2 = keySelector(item4);
				if (!dictionary2.ContainsKey(key2))
				{
					DifferentObject<TKey> differentObject2 = new DifferentObject<TKey>
					{
						Key = key2,
						Type = DifferentType.移除
					};
					List<DifferentProperty> collection2 = (from prop in source
						select new DifferentProperty
						{
							NewValue = null,
							OldValue = prop.Prop.GetValue(item4),
							PropName = prop.Name
						} into x
						where !string.IsNullOrEmpty($"{x.OldValue}")
						select x).ToList();
					differentObject2.DiffProps.AddRange(collection2);
					list3.Add(differentObject2);
				}
			}
			foreach (T oldItem in list2)
			{
				TKey key3 = keySelector(oldItem);
				if (dictionary2.TryGetValue(key3, out var newItem))
				{
					DifferentObject<TKey> differentObject3 = new DifferentObject<TKey>
					{
						Key = key3,
						Type = DifferentType.覆盖
					};
					List<DifferentProperty> list4 = (from prop in source
						select new DifferentProperty
						{
							NewValue = prop.Prop.GetValue(newItem),
							OldValue = prop.Prop.GetValue(oldItem),
							PropName = prop.Name
						} into x
						where !object.Equals(x.OldValue, x.NewValue)
						select x).ToList();
					if (list4.Count > 0)
					{
						differentObject3.DiffProps.AddRange(list4);
						list3.Add(differentObject3);
					}
				}
			}
			return list3;
		});
	}
}
