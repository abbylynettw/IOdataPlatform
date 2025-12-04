using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;

namespace IODataPlatform.Utilities;

/// <summary>
/// Excel风格的多选筛选器
/// 支持搜索、全选、反选等功能
/// </summary>
/// <inheritdoc />
public class ExcelFilter(string title) : ObservableObject()
{
	/// <summary>搜索关键词</summary>
	[ObservableProperty]
	private string searchText = string.Empty;

	/// <summary>过滤器的显示标题</summary>
	public string Title { get; } = title;

	/// <summary>所有可选项（包含选中状态）</summary>
	public ObservableCollection<FilterOption> AllOptions { get; } = new ObservableCollection<FilterOption>();

	/// <summary>过滤后显示的选项</summary>
	public ObservableCollection<FilterOption> FilteredOptions { get; } = new ObservableCollection<FilterOption>();

	/// <summary>
	/// 是否有任何筛选条件（即是否有未选中的项）
	/// </summary>
	public bool HasFilter => AllOptions.Any((FilterOption x) => !x.IsSelected);

	/// <inheritdoc cref="F:IODataPlatform.Utilities.ExcelFilter.searchText" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SearchText
	{
		get
		{
			return searchText;
		}
		[MemberNotNull("searchText")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(searchText, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SearchText);
				searchText = value;
				OnSearchTextChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SearchText);
			}
		}
	}

	/// <summary>
	/// 设置新的选项列表
	/// </summary>
	public void SetOptions(IEnumerable<string> newOptions)
	{
		Dictionary<string, int> dictionary = (from x in newOptions
			where !string.IsNullOrWhiteSpace(x)
			group x by x into g
			orderby g.Key
			select g).ToDictionary((IGrouping<string, string> g) => g.Key, (IGrouping<string, string> g) => g.Count());
		HashSet<string> hashSet = (from x in AllOptions
			where x.IsSelected
			select x.Value).ToHashSet();
		AllOptions.Clear();
		foreach (KeyValuePair<string, int> item2 in dictionary)
		{
			FilterOption item = new FilterOption
			{
				Value = item2.Key,
				Count = item2.Value,
				IsSelected = (hashSet.Count == 0 || hashSet.Contains(item2.Key))
			};
			AllOptions.Add(item);
		}
		SearchText = string.Empty;
		FilterOptions();
	}

	/// <summary>
	/// 过滤选项
	/// </summary>
	private void FilterOptions()
	{
		FilteredOptions.Clear();
		string value = SearchText?.ToLower() ?? string.Empty;
		foreach (FilterOption allOption in AllOptions)
		{
			if (string.IsNullOrEmpty(value) || allOption.Value.ToLower().Contains(value))
			{
				FilteredOptions.Add(allOption);
			}
		}
	}

	/// <summary>
	/// 全选当前显示的选项
	/// </summary>
	public void SelectAll()
	{
		foreach (FilterOption filteredOption in FilteredOptions)
		{
			filteredOption.IsSelected = true;
		}
	}

	/// <summary>
	/// 取消全选当前显示的选项
	/// </summary>
	public void UnselectAll()
	{
		foreach (FilterOption filteredOption in FilteredOptions)
		{
			filteredOption.IsSelected = false;
		}
	}

	/// <summary>
	/// 反选当前显示的选项
	/// </summary>
	public void InverseSelect()
	{
		foreach (FilterOption filteredOption in FilteredOptions)
		{
			filteredOption.IsSelected = !filteredOption.IsSelected;
		}
	}

	/// <summary>
	/// 获取所有选中的值
	/// </summary>
	public HashSet<string> GetSelectedValues()
	{
		return (from x in AllOptions
			where x.IsSelected
			select x.Value).ToHashSet();
	}

	/// <summary>
	/// 清除所有选择（全选）
	/// </summary>
	public void ClearAll()
	{
		foreach (FilterOption allOption in AllOptions)
		{
			allOption.IsSelected = true;
		}
		SearchText = string.Empty;
	}

	/// <summary>
	/// 搜索文本变化时过滤选项
	/// </summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnSearchTextChanged(string value)
	{
		FilterOptions();
	}
}
