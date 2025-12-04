using IODataPlatform.Models.ExcelModels;

namespace IODataPlatform.Views.SubPages.Common;

// 筛选部分

partial class CabinetAllocatedViewModel
{

    [ObservableProperty]
    private ObservableCollection<string> localBoxNumberOptions = [];

    [ObservableProperty]
    private ObservableCollection<string> powerTypeOptions = [];

    [ObservableProperty]
    private string localBoxNumber1 = string.Empty;

    [ObservableProperty]
    private string powerType1 = string.Empty;

    [ObservableProperty]
    private string localBoxNumber2 = string.Empty;

    [ObservableProperty]
    private string powerType2 = string.Empty;

    [ObservableProperty]
    private string localBoxNumber3 = string.Empty;

    [ObservableProperty]
    private string powerType3 = string.Empty;

    [ObservableProperty]
    private ObservableCollection<IoFullData>? displayBoardPoints;

    [ObservableProperty]
    private ObservableCollection<IoFullData>? displayUnsetPoints;

    [ObservableProperty]
    private int unsetPointCount;

    [ObservableProperty]
    private int boardPointCount;

    private void Filter()
    {
        // 支持FF板卡：使用GetAllChannels()获取所有通道
        DisplayBoardPoints = [.. ViewBoard?.GetAllChannels().Where(x => x.Point != null).Select(x => x.Point)
            .WhereIf(x => x!.LocalBoxNumber == LocalBoxNumber2, LocalBoxNumber2 != "全部")
            .WhereIf(x => x!.PowerType == PowerType2, PowerType2 != "全部") ?? []];

        DisplayUnsetPoints = [.. Cabinet?.UnsetPoints?
            .WhereIf(x => x!.LocalBoxNumber == LocalBoxNumber3, LocalBoxNumber3 != "全部")
            .WhereIf(x => x!.PowerType == PowerType3, PowerType3 != "全部") ?? []];
    }

    partial void OnDisplayBoardPointsChanged(ObservableCollection<IoFullData>? value)
    {
        BoardPointCount = DisplayBoardPoints?.Count ?? 0;
    }

    partial void OnDisplayUnsetPointsChanged(ObservableCollection<IoFullData>? value)
    {
        UnsetPointCount = DisplayUnsetPoints?.Count ?? 0;
    }

    partial void OnPowerType2Changed(string value)
    {
        Filter();
    }

    partial void OnLocalBoxNumber2Changed(string value)
    {
        Filter();
    }

    partial void OnPowerType3Changed(string value)
    {
        Filter();
    }

    partial void OnLocalBoxNumber3Changed(string value)
    {
        Filter();
    }

    partial void OnCabinetChanged(StdCabinet? value)
    {
        ViewBoard = null;
        DisplayBoardPoints = null;
        DisplayUnsetPoints = null;

        if (Cabinet is null) { return; }

        var allPoints = Cabinet.ToPoint();
        LocalBoxNumberOptions = ["全部", .. allPoints.Select(x => x.LocalBoxNumber).Distinct()];
        PowerTypeOptions = ["全部", .. allPoints.Select(x => x.PowerType).Distinct()];

        LocalBoxNumber1 = "全部";
        LocalBoxNumber2 = "全部";
        LocalBoxNumber3 = "全部";

        PowerType1 = "全部";
        PowerType2 = "全部";
        PowerType3 = "全部";
        Filter();
    }
}
