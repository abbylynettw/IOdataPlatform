using System.Collections.Immutable;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;

namespace IODataPlatform.Views.Pages;

// 排序和筛选部分

partial class TerminationViewModel {

    [ObservableProperty]
    private ObservableCollection<TerminationData>? displayData;

    [ObservableProperty]
    private int allDataCount;
    
    [ObservableProperty]
    private int displayDataCount;

    private bool isRefreshingOptions = false;

    public ImmutableList<CommonFilter> Filters { get; } = [ new("机柜号"), new("点名")];

    [RelayCommand]
    private async Task ClearAllFilterOptions() {
        if (!await message.ConfirmAsync("确认重置全部筛选条件")) { return; }
        isRefreshingOptions = true;
        Filters.AllDo(x => x.Option = "全部");
        isRefreshingOptions = false;
        Filter();
    }

    private void RefreshFilterOptions() {
        isRefreshingOptions = true;

        if (AllData == null) {
            Filters.ForEach(x => x.ClearAll());
        } else {
            var filterDic = Filters.ToDictionary(x => x.Title);
            filterDic["机柜号"].SetOptions(AllData.Select(x => x.CabinetNumber));
            filterDic["点名"].SetOptions(AllData.Select(x => x.IOPointName));
        }

        isRefreshingOptions = false;
    }

    [RelayCommand]
    private void Filter() {
        if (isRefreshingOptions) { return; }
        if (AllData is null) { return; }

        var filterDic = Filters.ToDictionary(x => x.Title);

        var data = AllData
            .WhereIf(x => x.CabinetNumber == filterDic["机柜号"].Option, filterDic["机柜号"].Option != "全部")
            .WhereIf(x => x.IOPointName == filterDic["点名"].Option, filterDic["点名"].Option != "全部");

        DisplayData = new(data);
    }

    partial void OnDisplayDataChanged(ObservableCollection<TerminationData>? value) {
        DisplayDataCount = DisplayData?.Count ?? 0;
    }

    partial void OnAllDataChanged(ObservableCollection<TerminationData>? value) {
        DisplayData = null;
        AllDataCount = AllData?.Count ?? 0;
        if (AllData is null) { return; }
        RefreshFilterOptions();
        Filter();
    }
     
}