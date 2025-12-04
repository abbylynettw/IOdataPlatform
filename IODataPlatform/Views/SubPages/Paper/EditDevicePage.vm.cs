using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;

using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;

namespace IODataPlatform.Views.SubPages.Paper;

public partial class EditDeviceViewModel(PaperViewModel paper, GlobalModel model, IPickerService picker, ExcelService excel, IMessageService message, SqlSugarContext context) : ObservableObject, INavigationAware {

    [ObservableProperty]
    private config_project? project;

    [ObservableProperty]
    private ObservableCollection<盘箱柜>? _data;

    [RelayCommand]
    private async Task Import() {
        if (picker.OpenFile("Excel 文件(*.xlsx; *.xls)|*.xlsx; *.xls") is not string file) { return; }
        model.Status.Busy("正在导入……");
        var projectId = Project!.Id;
        using var data = await excel.GetDataTableAsStringAsync(file, true);
        var list = data.StringTableToIEnumerableByDiplay<盘箱柜>().ToList().AllDo(x => x.项目Id = projectId).ToList();
        await context.Db.Insertable(list).ExecuteCommandIdentityIntoEntityAsync();
        Data = [.. Data, .. list];
        model.Status.Reset();
    }

    [RelayCommand]
    private void DownloadTemplate() {
        if (picker.SaveFile("Excel 文件(*.xlsx; *.xls)|*.xlsx; *.xls", "盘箱柜批量导入模板") is not string file) { return; }
        using var wb = excel.GetWorkbook();
        using var ws = wb.Worksheets[0];
        ws.Cells[0, 0].Value = "名称";
        ws.Cells[0, 1].Value = "类别";
        ws.Cells[0, 2].Value = "子类别";
        ws.Cells[0, 3].Value = "房间号";
        ws.Cells[0, 4].Value = "内部编码";
        ws.Cells[0, 5].Value = "外部编码";
        ws.Cells[0, 6].Value = "子项";
        ws.Cells[0, 7].Value = "LOT";
        ws.Cells[0, 8].Value = "Batch";
        
        wb.Save(file);
    }

    [RelayCommand]
    private async Task Add() {
        var obj = new 盘箱柜() { 项目Id = Project!.Id };
        if (!EditWithEditor(obj, "添加")) { return; }
        await context.Db.Insertable(obj).ExecuteCommandIdentityIntoEntityAsync();
        Data!.Add(obj);
    }

    [RelayCommand]
    private async Task Edit(盘箱柜 data) {
        var obj = new 盘箱柜().CopyPropertiesFrom(data);
        if (!EditWithEditor(obj, "编辑")) { return; }
        await context.Db.Updateable(obj).ExecuteCommandAsync();
        data.CopyPropertiesFrom(obj);
        Data = [.. Data];
    }

    [RelayCommand]
    private async Task Delete(盘箱柜 data) {
        if (!await message.ConfirmAsync("确认删除")) { return; }
        await context.Db.Deleteable(data).ExecuteCommandAsync();
        Data!.Remove(data);
    }

    public void OnNavigatedFrom() { }

    public async void OnNavigatedTo() {
        Project = paper.Project ?? throw new();
        Data = [.. await context.Db.Queryable<盘箱柜>().Where(x => x.项目Id == Project.Id).ToListAsync()];
    }

    private static bool EditWithEditor(盘箱柜 data, string title) {

        var builder = data.CreateEditorBuilder();

        builder.WithTitle(title).WithObject(data).WithEditorHeight(660).WithValidator(x => { 
            if (string.IsNullOrEmpty(x.名称)) { return "请输入盘箱柜名称"; }
            if (string.IsNullOrEmpty(x.房间号)) { return "请输入房间号"; }
            if (string.IsNullOrEmpty(x.内部编码)) { return "请输入内部编码"; }
            if (string.IsNullOrEmpty(x.外部编码)) { return "请输入外部编码"; }
            if (string.IsNullOrEmpty(x.LOT)) { return "请输入LOT"; }
            if (string.IsNullOrEmpty(x.Batch)) { return "请输入Batch"; }
            return string.Empty;
        });

        builder.AddProperty<string>(nameof(盘箱柜.名称)).WithHeader(nameof(盘箱柜.名称)).EditAsText().ShorterThan(50);
        builder.AddProperty<盘箱柜类别>(nameof(盘箱柜.类别)).WithHeader(nameof(盘箱柜.类别)).EditAsCombo<盘箱柜类别>().WithOptions<盘箱柜类别>();
        builder.AddProperty<string>(nameof(盘箱柜.子类别)).WithHeader(nameof(盘箱柜.子类别)).EditAsText().ShorterThan(50);
        builder.AddProperty<string>(nameof(盘箱柜.房间号)).WithHeader(nameof(盘箱柜.房间号)).EditAsText().ShorterThan(20);
        builder.AddProperty<string>(nameof(盘箱柜.内部编码)).WithHeader(nameof(盘箱柜.内部编码)).EditAsText().ShorterThan(50);
        builder.AddProperty<string>(nameof(盘箱柜.外部编码)).WithHeader(nameof(盘箱柜.外部编码)).EditAsText().ShorterThan(50);
        builder.AddProperty<string>(nameof(盘箱柜.子项)).WithHeader(nameof(盘箱柜.子项)).EditAsText().ShorterThan(50);
        builder.AddProperty<string>(nameof(盘箱柜.LOT)).WithHeader(nameof(盘箱柜.LOT)).EditAsText().ShorterThan(50);
        builder.AddProperty<string>(nameof(盘箱柜.Batch)).WithHeader(nameof(盘箱柜.Batch)).EditAsText().ShorterThan(50);

        return builder.Build().EditWithWpfUI();

    }

}