using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;

namespace IODataPlatform.Views.Pages;

// 排序和筛选部分

partial class DepXT2ViewModel {


    
    [ObservableProperty]
    private ObservableCollection<IoFullData>? displayPoints;

    [ObservableProperty]
    private int displayPointCount;
    
    [ObservableProperty]
    private int allPointCount;
    
    [ObservableProperty]
    private int allDataCount;

    [ObservableProperty] 
    private bool isAscending = true;

    /// <summary>
    /// 筛选模式是否启用（显示列头筛选图标）
    /// </summary>
    [ObservableProperty]
    private bool isFilterModeEnabled = false;

    private bool isRefreshingOptions = false;

    partial void OnDisplayPointsChanged(ObservableCollection<IoFullData>? value) {
        DisplayPointCount = value?.Count ?? 0;
    }


    partial void OnAllDataChanged(ObservableCollection<IoFullData>? value) {
        DisplayPoints = null;
        AllPointCount = AllData?.Count ?? 0;
        
        // 计算机柜数量
        if (AllData is null) 
        {
            AllDataCount = 0;
            return; 
        }
        
        AllDataCount = AllData.GroupBy(x => x.CabinetNumber).Count();
        
        // 如果 Filters 还没有初始化，先初始化
        if (Filters.Count == 0)
        {
            InitializeFilters();
        }
        
        RefreshFilterOptions();
        FilterAndSort();
    }

    private void InitializeFilters()
    {
        var filtersList = new List<ExcelFilter>();
        
        // 使用反射获取所有带Display特性的属性
        var properties = typeof(IoFullData).GetProperties()
            .Where(p => p.GetCustomAttribute<DisplayAttribute>() != null)
            .OrderBy(p => p.GetCustomAttribute<DisplayAttribute>()?.GetOrder() ?? 999)
            .ToList();

        foreach (var prop in properties)
        {
            var displayName = prop.GetCustomAttribute<DisplayAttribute>()?.Name ?? prop.Name;
            filtersList.Add(new ExcelFilter(displayName));
        }

        Filters = [.. filtersList];
    }

    public ImmutableList<ExcelFilter> Filters { get; private set; } = [];

    [RelayCommand]
    private async Task ClearAllFilterOptions() {
        if (!await message.ConfirmAsync("确认重置全部筛选条件")) { return; }
        isRefreshingOptions = true;
        Filters.AllDo(x => x.ClearAll());
        isRefreshingOptions = false;
        FilterAndSort();
    }

    partial void OnIsAscendingChanged(bool value) {
        FilterAndSort();
    }

    private void RefreshFilterOptions() {
        isRefreshingOptions = true;

        if (AllData == null) {
            Filters.ForEach(x => x.AllOptions.Clear());
        } else {
            // 动态填充每个筛选器的选项（不保留旧选中状态）
            var properties = typeof(IoFullData).GetProperties()
                .Where(p => p.GetCustomAttribute<DisplayAttribute>() != null)
                .ToDictionary(p => p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name, p => p);

            foreach (var filter in Filters)
            {
                if (properties.TryGetValue(filter.Title, out var prop))
                {
                    var values = AllData.Select(x => 
                    {
                        var value = prop.GetValue(x);
                        if (value == null) return string.Empty;
                        if (value is DateTime dt) return dt.ToString("d");
                        return value.ToString() ?? string.Empty;
                    });
                    // 使用preserveSelection=false，不保留旧的选中状态
                    filter.SetOptions(values, preserveSelection: false);
                }
            }
        }

        isRefreshingOptions = false;
    }

    [RelayCommand]
    public void FilterAndSort() {
        if (isRefreshingOptions) { return; }
        if (AllData is null) { return; }

        var data = AllData.AsEnumerable();
        
        // 动态应用所有筛选器
        var properties = typeof(IoFullData).GetProperties()
            .Where(p => p.GetCustomAttribute<DisplayAttribute>() != null)
            .ToDictionary(p => p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name, p => p);

        foreach (var filter in Filters)
        {
            var selectedValues = filter.GetSelectedValues();
            
            // 如果有筛选条件（即有未选中的项）
            if (selectedValues.Count > 0 && selectedValues.Count < filter.AllOptions.Count)
            {
                if (properties.TryGetValue(filter.Title, out var prop))
                {
                    data = data.Where(x =>
                    {
                        var value = prop.GetValue(x);
                        var strValue = value == null ? string.Empty : 
                                      value is DateTime dt ? dt.ToString("d") : 
                                      value.ToString() ?? string.Empty;
                        return selectedValues.Contains(strValue);
                    });
                }
            }
        }

        DisplayPoints = [.. data];
    }
}
