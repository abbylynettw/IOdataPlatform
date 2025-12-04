using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IODataPlatform.Utilities;

/// <summary>
/// ObservableCollection的扩展方法集合
/// 提供针对ObservableCollection的常用操作扩展
/// 支持批量添加等高效操作，广泛用于UI数据绑定场景
/// </summary>
public static class ObservableCollectionExtensions
{
	/// <summary>
	/// 批量添加元素到ObservableCollection中
	/// 通过循环逐个添加，确保触发每个元素的变更通知
	/// </summary>
	/// <typeparam name="T">集合元素类型</typeparam>
	/// <param name="collection">目标ObservableCollection</param>
	/// <param name="items">要添加的元素集合</param>
	public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			collection.Add(item);
		}
	}
}
