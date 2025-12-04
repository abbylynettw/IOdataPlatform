using IODataPlatform.Models;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;

using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.SubPages.Termination;

public partial class ImportViewModel(GlobalModel model, TerminationViewModel termination, ExcelService excel, IPickerService picker, INavigationService navigation) : ObservableObject, INavigationAware {

    public void OnNavigatedFrom() { }

    public void OnNavigatedTo() {
        _ = termination.Project ?? throw new("开发人员注意");
        _ = termination.SubProject ?? throw new("开发人员注意");
        _ = termination.AllData ?? throw new("开发人员注意");
    }

    public ObservableCollection<DifferentObject<string>> DiffObjects { get; } = [];
    public ObservableCollection<DifferentProperty> DiffProps { get; } = [];

    private readonly List<TerminationData> oldData = [.. termination.AllData ?? throw new("开发人员注意")];
    private readonly List<TerminationData> newData = [];

    [RelayCommand]
    private async Task ImportFile() {

        if (picker.OpenFile("Excel 文件(*.xls; *.xlsx)|*.xls; *.xlsx") is not string file) { return; }
        
        model.Status.Busy("正在获取数据……");
        DiffObjects.Clear();
        DiffProps?.Clear();
        
        using var dataTable = await excel.GetDataTableAsStringAsync(file, true);
        newData.Reset(dataTable.StringTableToIEnumerableByDiplay<TerminationData>());

        model.Status.Busy("正在比对数据……");
        DiffObjects.Reset(await DataComparator.ComparerAsync(newData, oldData, x => $"{x.IOPointName}"));
        model.Status.Reset();
    }

    [RelayCommand]
    private void ViewData(DifferentObject<string> obj) {
        DiffProps.Reset(obj.DiffProps);
    }

    [RelayCommand]
    private async Task Confirm() {
        model.Status.Success("正在提交数据……");

        termination.AllData = new(newData);
        await termination.SaveAndUploadRealtimeFileAsync();

        model.Status.Success("导入成功");
        navigation.GoBack();
    }

    [RelayCommand]
    private void ConvertBack() {
        navigation.GoBack();
    }

}
