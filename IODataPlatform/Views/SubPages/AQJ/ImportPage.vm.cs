using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;

using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.SubPages.AQJ;

public partial class ImportViewModel(SqlSugarContext context, IMessageService message, GlobalModel model, DepAQJViewModel aqj, ExcelService excel, IPickerService picker, INavigationService navigation) : ObservableObject, INavigationAware
{

    public void OnNavigatedFrom() { }

    public void OnNavigatedTo()
    {
        _ = aqj.Project ?? throw new("开发人员注意");
        _ = aqj.SubProject ?? throw new("开发人员注意");
        _ = aqj.AllData ?? throw new("开发人员注意");
        oldData =[.. aqj.AllData] ;
    }

    public ObservableCollection<DifferentObject<string>> DiffObjects { get; } = [];
    public ObservableCollection<DifferentProperty> DiffProps { get; } = [];

    private List<IoFullData>? oldData;
    private readonly List<IoFullData> newData = [];

    [RelayCommand]
    private async Task ImportFile()
    {

        if (picker.OpenFile("Excel 文件(*.xls; *.xlsx)|*.xls; *.xlsx") is not string file) { return; }
        _ = oldData ?? throw new();
        _ = aqj.SubProject ?? throw new();
        model.Status.Busy("正在获取数据……");
        DiffObjects.Clear();
        DiffProps?.Clear();

        using var dataTable = await excel.GetDataTableAsStringAsync(file, true);
     
        var controsystem = context.Db.Queryable<config_project_major>()
                                  .Where(it => it.Id == aqj.SubProject.MajorId).First().ControlSystem;
        newData.Reset(dataTable.ConvertOldDataTableToIoFullData(message, context.Db, controsystem));

        model.Status.Busy("正在比对数据……");
        DiffObjects.Reset(await DataComparator.ComparerAsync(newData, oldData, x => $"{x.TagName}"));
        model.Status.Reset();
    }

    [RelayCommand]
    private void ViewData(DifferentObject<string> obj)
    {
        DiffProps.Reset(obj.DiffProps);
    }

    [RelayCommand]
    private async Task Confirm()
    {
        if (!await message.ConfirmAsync("确认操作\r\n将会覆盖之前子项的全部内容")) { return; }
        model.Status.Success("正在提交数据……");
        aqj.AllData = [.. newData];
        await aqj.SaveAndUploadFileAsync();

        model.Status.Success("导入成功");
        navigation.GoBack();
    }

    [RelayCommand]
    private void ConvertBack()
    {
        navigation.GoBack();
    }

}
