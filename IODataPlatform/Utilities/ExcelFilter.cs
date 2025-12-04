using System.Collections.ObjectModel;

namespace IODataPlatform.Utilities;

/// <summary>
/// Excel风格的多选筛选器
/// 支持搜索、全选、反选等功能
/// </summary>
public partial class ExcelFilter : ObservableObject
{
    public ExcelFilter(string title)
    {
        Title = title;
    }

    /// <summary>搜索关键词</summary>
    [ObservableProperty]
    private string searchText = string.Empty;

    /// <summary>过滤器的显示标题</summary>
    public string Title { get; }

    /// <summary>所有可选项（包含选中状态）</summary>
    public ObservableCollection<FilterOption> AllOptions { get; } = new();

    /// <summary>过滤后显示的选项</summary>
    public ObservableCollection<FilterOption> FilteredOptions { get; } = new();

    /// <summary>
    /// 是否有任何筛选条件（即是否有未选中的项）
    /// </summary>
    public bool HasFilter => AllOptions.Any(x => !x.IsSelected);

    /// <summary>
    /// 设置新的选项列表
    /// </summary>
    /// <param name="newOptions">新的选项值列表</param>
    /// <param name="preserveSelection">是否保留之前的选中状态（默认false，切换数据源时应为false）</param>
    public void SetOptions(IEnumerable<string> newOptions, bool preserveSelection = false)
    {
        // 统计每个值的数量并排序
        var optionCounts = newOptions
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .GroupBy(x => x)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());

        // 根据参数决定是否保存当前选中的值
        var selectedValues = preserveSelection 
            ? AllOptions.Where(x => x.IsSelected).Select(x => x.Value).ToHashSet()
            : new HashSet<string>();

        // 重新生成选项列表
        AllOptions.Clear();
        foreach (var kvp in optionCounts)
        {
            AllOptions.Add(new FilterOption
            {
                Value = kvp.Key,
                Count = kvp.Value,
                // 如果不保留选中状态，所有选项默认选中；否则根据之前的状态
                IsSelected = !preserveSelection || selectedValues.Count == 0 || selectedValues.Contains(kvp.Key)
            });
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
        var searchLower = SearchText?.ToLower() ?? string.Empty;

        foreach (var option in AllOptions)
        {
            if (string.IsNullOrEmpty(searchLower) || option.Value.ToLower().Contains(searchLower))
            {
                FilteredOptions.Add(option);
            }
        }
    }

    /// <summary>
    /// 全选当前显示的选项
    /// </summary>
    public void SelectAll()
    {
        foreach (var option in FilteredOptions)
        {
            option.IsSelected = true;
        }
    }

    /// <summary>
    /// 取消全选当前显示的选项
    /// </summary>
    public void UnselectAll()
    {
        foreach (var option in FilteredOptions)
        {
            option.IsSelected = false;
        }
    }

    /// <summary>
    /// 反选当前显示的选项
    /// </summary>
    public void InverseSelect()
    {
        foreach (var option in FilteredOptions)
        {
            option.IsSelected = !option.IsSelected;
        }
    }

    /// <summary>
    /// 获取所有选中的值
    /// </summary>
    public HashSet<string> GetSelectedValues()
    {
        return AllOptions.Where(x => x.IsSelected).Select(x => x.Value).ToHashSet();
    }

    /// <summary>
    /// 清除所有选择（全选）
    /// </summary>
    public void ClearAll()
    {
        foreach (var option in AllOptions)
        {
            option.IsSelected = true;
        }
        SearchText = string.Empty;
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterOptions();
    }
}

/// <summary>
/// 筛选选项
/// </summary>
public partial class FilterOption : ObservableObject
{
    [ObservableProperty]
    private string value = string.Empty;

    [ObservableProperty]
    private int count;

    [ObservableProperty]
    private bool isSelected = true;
}
