﻿namespace IODataPlatform.Utilities;

/// <summary>
/// 通用过滤器组件
/// 提供带有标题和选项的下拉框过滤功能，支持观察者模式
/// 默认包含"全部"选项，支持动态更新选项列表和清空操作
/// 广泛用于数据列表的筛选和过滤场景
/// </summary>
public partial class CommonFilter(string title) : ObservableObject {

    /// <summary>过滤器的显示标题</summary>
    public string Title { get; } = title;

    /// <summary>可选项集合，默认包含"全部"选项</summary>
    public ObservableCollection<string> Options { get; } = ["全部"];

    /// <summary>当前选中的选项，默认为"全部"</summary>
    [ObservableProperty]
    private string option = "全部";

    /// <summary>
    /// 清除所有过滤条件，重置为初始状态
    /// 将选中项设置为"全部"，并清空所有自定义选项
    /// </summary>
    public void ClearAll() {
        Option = "全部";
        Options.Reset(["全部"]);
    }

    /// <summary>
    /// 设置新的选项列表
    /// 自动去重、过滤空值，并在前面添加"全部"选项
    /// 如果当前选中项不在新列表中，则自动重置为"全部"
    /// </summary>
    /// <param name="newOptions">新的选项集合</param>
    public void SetOptions(IEnumerable<string> newOptions) {
        var options = newOptions.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).Prepend("全部");
        var option = options.Contains(Option) ? Option : "全部";
        Options.Reset(options);
        Option = option;
    }

}