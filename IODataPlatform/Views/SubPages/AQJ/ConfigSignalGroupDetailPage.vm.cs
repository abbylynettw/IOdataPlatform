using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;

using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace IODataPlatform.Views.SubPages.AQJ;

public partial class ConfigSignalGroupDetailViewModel(SqlSugarContext context, ConfigSignalGroupViewModel signalGroupVm, IMessageService message,ExcelService excel, GlobalModel model,IPickerService picker) : ObservableObject, INavigationAware
{

    [ObservableProperty]
    private config_aqj_signalGroup? signalGroup;

    [ObservableProperty]
    private ObservableCollection<config_aqj_signalGroupDetail>? signalGroupDetails;


    [RelayCommand]
    private async Task DownLoad()
    {
        _ = signalGroup ?? throw new Exception("没有数据，无法下载");
        model.Status.Busy("开始生成安全级室IO清册...");
        var list = SignalGroupDetails.ToList();
        var dataTable = await list.ToTableByDisplayAttributeAsync();
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string filePath = Path.Combine(desktopPath, $"{signalGroup.signalGroupName}.xlsx");
        await excel.FastExportToDesktopAsync(dataTable, filePath);
        model.Status.Success($"已下载到{filePath}");
    }


    [RelayCommand]
    private async Task Import()
    {
        //读取Excel，批量插入
        //选择文件
        if (picker.OpenFile("请选择导入的信号(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx") is not string selectedFilePathA || string.IsNullOrEmpty(selectedFilePathA)) return;
        using var dataTable = await excel.GetDataTableAsStringAsync(selectedFilePathA, true);
        var data = await Task.Run(dataTable.StringTableToIEnumerableByDiplay<config_aqj_signalGroupDetail>);
        var list = data.ToList();
        foreach (var item in list)
        {
            item.signalGroupId = SignalGroup?.Id ?? throw new InvalidOperationException();
        }
        //todo 先删除 后覆盖
        await Delete();
        await context.Db.Insertable(list).ExecuteCommandAsync();
        await Refresh();

    }

    [RelayCommand]
    private async Task Delete()
    {
        await context.Db.Deleteable<config_aqj_signalGroupDetail>().Where(t => t.signalGroupId == signalGroup.Id).ExecuteCommandAsync();
        await Refresh();
    }
   

    [RelayCommand]
    private async Task Edit(config_aqj_signalGroupDetail data)
    {
        var dataToEdit = new config_aqj_signalGroupDetail().CopyPropertiesFrom(data);
        if (!EditSignalGroupDetail(dataToEdit, "编辑")) { return; }
        await context.Db.Updateable(dataToEdit).ExecuteCommandAsync();
        await Refresh();
    }
   

    private bool EditSignalGroupDetail(config_aqj_signalGroupDetail data, string title)
    {
        var builder = data.CreateEditorBuilder();
        builder.WithTitle(title).WithEditorHeight(600); // Increase height to accommodate more fields

        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.序号)).WithHeader("序号").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.信号名称)).WithHeader("信号名称").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.信号说明)).WithHeader("信号说明").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.原变量名)).WithHeader("原变量名").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.控制站)).WithHeader("控制站").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.机柜号)).WithHeader("机柜号").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.机箱号)).WithHeader("机箱号").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.槽位号)).WithHeader("槽位号").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.通道号)).WithHeader("通道号").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.安全分级)).WithHeader("安全分级").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.IO类型)).WithHeader("I/O类型").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.前卡件类型)).WithHeader("前卡件类型").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.后卡件类型)).WithHeader("后卡件类型").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.端子板类型)).WithHeader("端子板类型").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.端子板编号)).WithHeader("端子板编号").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.端子号1)).WithHeader("端子号1").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.端子号2)).WithHeader("端子号2").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.刻度类型)).WithHeader("刻度类型").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.信号特性)).WithHeader("信号特性").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.量程下限)).WithHeader("量程下限").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.超量程下限)).WithHeader("超量程下限").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.量程上限)).WithHeader("量程上限").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.超量程上限)).WithHeader("超量程上限").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.单位)).WithHeader("单位").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.缺省值)).WithHeader("缺省值").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.回路供电)).WithHeader("回路供电").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.电压等级)).WithHeader("电压等级").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.内部外部)).WithHeader("内部/外部").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.典型回路图)).WithHeader("典型回路图").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.源头目的)).WithHeader("源头/目的").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.是否隔离)).WithHeader("是否隔离").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.FDSAMA页码)).WithHeader("FD/SAMA页码").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.版本)).WithHeader("版本").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.传感器类型)).WithHeader("传感器类型").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.分配信息)).WithHeader("分配信息").EditAsText();
        builder.AddProperty<string>(nameof(config_aqj_signalGroupDetail.备注)).WithHeader("备注").EditAsText();

        return builder.Build().EditWithWpfUI();
    }

    private async Task Refresh()
    {
        SignalGroupDetails = null;
        if (SignalGroup is null) { return; }
        SignalGroupDetails = new ObservableCollection<config_aqj_signalGroupDetail>(await context.Db.Queryable<config_aqj_signalGroupDetail>().Where(x => x.signalGroupId == SignalGroup.Id).ToListAsync());
    }

    public void OnNavigatedFrom() { }

    public async void OnNavigatedTo()
    {
        SignalGroup = signalGroupVm.SignalGroup ?? throw new InvalidOperationException();
        await Refresh();
    }
}
