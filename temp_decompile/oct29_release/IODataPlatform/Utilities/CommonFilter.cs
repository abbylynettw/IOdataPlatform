using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using LYSoft.Libs;

namespace IODataPlatform.Utilities;

/// <summary>
/// 通用过滤器组件
/// 提供带有标题和选项的下拉框过滤功能，支持观察者模式
/// 默认包含"全部"选项，支持动态更新选项列表和清空操作
/// 广泛用于数据列表的筛选和过滤场景
/// </summary>
/// <inheritdoc />
public class CommonFilter(string title) : ObservableObject()
{
	/// <summary>当前选中的选项，默认为"全部"</summary>
	[ObservableProperty]
	private string option = "全部";

	/// <summary>过滤器的显示标题</summary>
	public string Title { get; } = title;

	/// <summary>可选项集合，默认包含"全部"选项</summary>
	public ObservableCollection<string> Options { get; } = new ObservableCollection<string> { "全部" };

	/// <inheritdoc cref="F:IODataPlatform.Utilities.CommonFilter.option" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Option
	{
		get
		{
			return option;
		}
		[MemberNotNull("option")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(option, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Option);
				option = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Option);
			}
		}
	}

	/// <summary>
	/// 清除所有过滤条件，重置为初始状态
	/// 将选中项设置为"全部"，并清空所有自定义选项
	/// </summary>
	public void ClearAll()
	{
		Option = "全部";
		Options.Reset(new _003C_003Ez__ReadOnlySingleElementList<string>("全部"));
	}

	/// <summary>
	/// 设置新的选项列表
	/// 自动去重、过滤空值，并在前面添加"全部"选项
	/// 如果当前选中项不在新列表中，则自动重置为"全部"
	/// </summary>
	/// <param name="newOptions">新的选项集合</param>
	public void SetOptions(IEnumerable<string> newOptions)
	{
		IEnumerable<string> enumerable = (from x in newOptions.Distinct()
			where !string.IsNullOrWhiteSpace(x)
			select x).Prepend("全部");
		string text = (enumerable.Contains(Option) ? Option : "全部");
		Options.Reset(enumerable);
		Option = text;
	}
}
