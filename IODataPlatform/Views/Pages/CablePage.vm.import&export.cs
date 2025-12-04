#pragma warning disable CA1822 // 将成员标记为 static

using System.Data;
using System.IO;
using System.Security.Policy;
using System.Windows.Media;

using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.Cable;

using Microsoft.Extensions.DependencyInjection;

using Wpf.Ui.Extensions;

namespace IODataPlatform.Views.Pages;

// 导出部分

partial class CableViewModel {

    [RelayCommand]
    ///下载模板///
    private  async void DownLoadTemplate()
    {
        var dt = new DataTable();
        string[] columns = [
            "起点房间号", "起点系统号", "起点盘柜名称", "起点设备名称", "起点接线点1", "起点接线点2", "起点接线点3", "起点接线点4","起点屏蔽端", "起点IO类型", "起点安全分级分组", "起点信号位号", "起点专业",
            "终点房间号", "终点系统号", "终点盘柜名称", "终点设备名称", "终点接线点1", "终点接线点2", "终点接线点3", "终点接线点4","终点屏蔽端", "终点IO类型", "终点安全分级分组", "终点信号位号", "终点专业"];

        // 添加列
        foreach (var column in columns){dt.Columns.Add(column);}

        // 添加一行示例值
        dt.Rows.Add("房间号", "示例：IPP、IPC", "示例：3IPP001AR", "示例：205BN", "示例：1", "示例：2", "示例：3", "示例：4", "", "示例：DI", "示例：NC", "点名带扩展码", "示例：NC到DAS 即NC");       
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "待导入的电缆清单模板.xlsx");
        await excel.FastExportToDesktopAsync(dt, filePath);
        model.Status.Success($"成功下载到{filePath}");
    }


    [RelayCommand]
    private void Import(string param) {
        if (param == "导入发布端接数据") {
            ImportIoData();
        } else if (param == "导入数据") {
            ImportData();
        } else {
            throw new("开发人员注意");
        }
    }

    private void ImportIoData() {
        navigation.NavigateWithHierarchy(typeof(MatchPage));
    }

    private  async void ImportData() {
        //选择数据 导入
        if (picker.OpenFile("Excel 文件(*.xls; *.xlsx)|*.xls; *.xlsx") is not string file) { return; }
        model.Status.Busy("正在导入···");
        using var dataTable = await excel.GetDataTableAsStringAsync(file, true);
        var list = await Task.Run(dataTable.StringTableToIEnumerableByDiplay<CableData>);
        AllData = [.. list];
        await SaveAndUploadRealtimeFileAsync();
        model.Status.Reset();
       
    }

    [RelayCommand]
    private async Task Export(string param) {
        _ = AllData ?? throw new("没有可导出的数据");

        if (picker.SaveFile("Excel 文件(*.xlsx)|*.xlsx", param) is not string file) { return; }
        if (File.Exists(file)) { File.Delete(file); }

        model.Status.Busy($"正在{param}……");
        if (param == "导出全部数据"){
            await ExportAllData(file);
        }else if (param == "导出筛选数据"){
            await ExportFilterData(file);
        }else if (param == "导出可发布数据"){
            await ExportPublishData(file);
        }else{
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

    private async Task ExportPublishData(string file)
    {
        if (AllData is null) { throw new Exception("无数据可导出"); }

        // 下载模板，填入数据
        model.Status.Busy("正在下载电缆模板……");
        var cableLacalPath = await storage.DownloadtemplatesDepFileAsync("电缆清单模板.xlsx");

        // 定义需要填充的列
        string[] columns = { "序号", "线缆编号", "线缆列别", "色标", "特性代码", "对数", "芯数",
            "起点房间号","起点盘柜名称","起点设备名称", "起点接线点1","起点接线点2","起点接线点3","起点接线点4","起点屏蔽端","起点信号位号","电缆长度", 
            "终点房间号","终点盘柜名称","终点设备名称", "终点接线点1","终点接线点2","终点接线点3","终点接线点4","终点屏蔽端","终点信号位号","供货方","版本", "备注"};

        // 创建 DataTable
        var dt = excel.CreateDataTable(columns);

        // 填充数据
        foreach (var data in AllData){
            dt.Rows.Add( data.序号, data.线缆编号,data.线缆列别, data.色标,data.特性代码, data.芯线对数号, data.芯线号,
                data.起点房间号,data.起点盘柜名称, data.起点设备名称, data.起点接线点1, data.起点接线点2,data.起点接线点3,data.起点接线点4,data.起点屏蔽端,data.起点信号位号,data.电缆长度,
                data.终点房间号,data.终点盘柜名称, data.终点设备名称, data.终点接线点1,data.终点接线点2, data.终点接线点3,data.终点接线点4,data.终点屏蔽端,data.终点信号位号,data.供货方,data.版本,data.备注);
        }

        // 快速导出到指定位置
        await excel.FastExportSheetAsync(dt, cableLacalPath, 4);
      
        File.Move(cableLacalPath, file);
        model.Status.Success($"已成功导出到{file}");
    }


}