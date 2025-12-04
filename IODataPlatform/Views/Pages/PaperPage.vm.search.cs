using IODataPlatform.Models;

namespace IODataPlatform.Views.Pages;

public partial class PaperViewModel {

    [ObservableProperty]
    private DateTime dateFrom = DateTime.Today - TimeSpan.FromDays(30);
    
    [ObservableProperty]
    private DateTime dateTo = DateTime.Today;
    
    [ObservableProperty]
    private ObservableCollection<盘箱柜类别> deviceTypes = [盘箱柜类别.机柜, 盘箱柜类别.盘台, 盘箱柜类别.阀箱];
    
    [ObservableProperty]
    private 盘箱柜类别 deviceType = 盘箱柜类别.机柜;
    
    [ObservableProperty]
    private string lot = string.Empty;
    
    [ObservableProperty]
    private string batch = string.Empty;
    
    [ObservableProperty]
    private string room = string.Empty;
    
    [ObservableProperty]
    private string version = string.Empty;
    
    [RelayCommand]
    private void Search() {
        DisplayData = null;
        if (AllData is null) { return; }

        DisplayData = [.. AllData
            .Where(x => x.图纸.发布日期.Date >= DateFrom.Date)
            .Where(x => x.图纸.发布日期.Date <= DateTo.Date)
            .WhereIf(x => x.盘箱柜.类别 == DeviceType, DeviceType != 0)
            .Where(x => x.盘箱柜.LOT.Contains(Lot))
            .Where(x => x.盘箱柜.Batch.Contains(Batch))
            .Where(x => x.盘箱柜.房间号.Contains(Room))
            .Where(x => x.图纸.版本.Contains(Version))];
    }

}