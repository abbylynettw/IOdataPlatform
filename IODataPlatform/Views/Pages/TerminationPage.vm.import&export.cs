using System.IO;
using System.Text.Json;
using System.Windows.Media;

using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;

using Wpf.Ui.Extensions;

namespace IODataPlatform.Views.Pages;

// 导出部分

partial class TerminationViewModel {

    [RelayCommand]
    private async Task Import(string param) {
        if (param == "导入发布IO数据") {
            await ImportIoData();
        } else if (param == "导入数据") {
            ImportData();
        } else {
            throw new("开发人员注意");
        }
    }

    private async Task ImportIoData() {
        var projectId = Project?.Id ?? throw new("开发人员注意");
        _ = Major ?? throw new();
        var subProjectId = SubProject?.Id ?? throw new("开发人员注意");

        var versions = await context.Db.Queryable<publish_io>().Where(x => x.SubProjectId == subProjectId).Select(x => x.PublishedVersion).ToListAsync();

        var textblock = new TextBlock() { Text = "发布版本", HorizontalAlignment = HorizontalAlignment.Left };
        var combobox = new System.Windows.Controls.ComboBox() { HorizontalAlignment = HorizontalAlignment.Stretch, ItemsSource = versions, Margin = new(0, 5, 0, 5) };
        var textblock2 = new TextBlock() { Text = "注意：此操作会覆盖当前实时（未发布）数据", Foreground = new SolidColorBrush(Colors.Red), HorizontalAlignment = HorizontalAlignment.Left };
        var stackpanel = new System.Windows.Controls.StackPanel() { VerticalAlignment = VerticalAlignment.Center, Width = 300 };
        if (versions.Count == 1) { combobox.Text = versions[0]; }

        stackpanel.Children.Add(textblock);
        stackpanel.Children.Add(combobox);
        stackpanel.Children.Add(textblock2);

        var result = await dialog.ShowSimpleDialogAsync(
            new SimpleContentDialogCreateOptions() {
                Title = "导入发布IO数据",
                Content = stackpanel,
                PrimaryButtonText = "导入",
                CloseButtonText = "取消",
            }
        );

        if (result != ContentDialogResult.Primary) { return; }
        if (string.IsNullOrWhiteSpace(combobox.Text)) { throw new("请选择发布版本"); }
        var versionText = combobox.Text;

        if (string.IsNullOrWhiteSpace(versionText)) { }
        model.Status.Busy($"正在导入IO数据……");
        var publish = await Task.Run(() => context.Db.Queryable<publish_io>().Where(x => x.SubProjectId == subProjectId).Where(x => x.PublishedVersion == versionText).Single());

        var file = await storage.DownloadPublishIoFileAsync(subProjectId, publish.Id);
        if (Major.Department == Models.Department.系统二室) {
            var cabinets = JsonSerializer.Deserialize<List<StdCabinet>>(await File.ReadAllTextAsync(file));
            _ = cabinets ?? throw new("开发人员注意");
            var list = CabinetCalc.CabinetStructureToPoint(cabinets).Select(x => x.ToIoData().ToTerminationData());
            AllData = [.. list];
        }
        else if (Major.Department == Models.Department.安全级室){
            var data = await excel.GetDataTableAsStringAsync(file, true);
            var list = data.StringTableToIEnumerableByDiplay<AQJIoData>().Select(x => x.ToIoData().ToTerminationData());           
            AllData = [.. list];
        } else if (Major.Department == Models.Department.系统一室){
            var data = await excel.GetDataTableAsStringAsync(file, true);
            var list = data.StringTableToIEnumerableByDiplay<IoData>().Select(x => x.ToTerminationData());
            AllData = [.. list];
        } else
        {
            throw new NotImplementedException();
        }

        // 多科室判断

        model.Status.Success("导入成功");
        await SaveAndUploadRealtimeFileAsync();
    }

    private void ImportData() {
        navigation.NavigateWithHierarchy(typeof(SubPages.Termination.ImportPage));
    }

    [RelayCommand]
    private async Task Export(string param) {
        _ = AllData ?? throw new("没有可导出的数据");

        if (picker.SaveFile("Excel 文件(*.xlsx)|*.xlsx", param) is not string file) { return; }
        if (File.Exists(file)) { File.Delete(file); }

        model.Status.Busy($"正在{param}……");
        if (param == "导出全部端接数据") {
            await ExportAllData(file);
        } else if (param == "导出筛选端接数据") {
            await ExportFilterData(file);
        } else {
            throw new("开发人员注意");
        }

        model.Status.Success($"已成功{param}：{file}");
    }
    
    private async Task ExportAllData(string file) {
        if (AllData is null) { throw new("开发人员注意"); }
        await excel.FastExportAsync(await AllData.ToTableByDisplayAttributeAsync(), file);
    }

    private async Task ExportFilterData(string file) {
        if (DisplayData is null) { throw new("开发人员注意"); }
        await excel.FastExportAsync(await DisplayData.ToTableByDisplayAttributeAsync(), file);
    }

}