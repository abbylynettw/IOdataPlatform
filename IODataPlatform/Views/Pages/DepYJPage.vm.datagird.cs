using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;

using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;

using LYSoft.Libs.Editor;
using LYSoft.Libs.Wpf.WpfUI;

namespace IODataPlatform.Views.Pages;

// 表格部分

partial class DepYJViewModel {

    [RelayCommand]
    private async Task Add() {
        if (SubProject is null) { return; }
        if (AllData is null) { return; }

        var data = new IoData();
        if (!EditData(data, "添加")) { return; }
        AllData.Add(data);
        await SaveAndUploadFileAsync();
    }

    [RelayCommand]
    private async Task Clear() {
        if (!await message.ConfirmAsync("确认清空")) { return; }
        AllData = [];
        await SaveAndUploadFileAsync();
    }

    [RelayCommand]
    private async Task Edit(IoData data) {
        var objToEdit = new IoData().CopyPropertiesFrom(data);
        if (!EditData(objToEdit, "编辑")) { return; }
        data.CopyPropertiesFrom(objToEdit);
        await SaveAndUploadFileAsync();
        await ReloadAllData();
    }

    [RelayCommand]
    private async Task Delete(IoData data) {
        _ = AllData ?? throw new("开发人员注意");
        if (!await message.ConfirmAsync("是否删除")) { return; }

        AllData.Remove(data);
        await SaveAndUploadFileAsync();
    }

    [RelayCommand]
    private async Task Import() {
        if (picker.OpenFile("Excel 文件(*.xls;*.xlsx)|*.xls;*.xlsx")is not string file ) { return; }
        model.Status.Busy("正在导入IO数据……");
        AllData = null;
        using var data = await excel.GetDataTableAsStringAsync(file, true);
        var list = await Task.Run(() => data.StringTableToIEnumerableByDiplay<IoData>());
        AllData = [.. list];
        model.Status.Reset();
        await SaveAndUploadFileAsync();
    }

    [RelayCommand]
    private async Task Refresh() {
        model.Status.Busy("正在刷新……");
        await ReloadAllData();
        model.Status.Reset();
    }

    private bool EditData(IoData data, string title) {

        var builder = data.CreateEditorBuilder();
        builder.WithTitle(title);
        foreach (var property in typeof(IoData).GetProperties()) {
            var propDisplayName = property.GetCustomAttribute<DisplayAttribute>()?.Name ?? property.Name;

            if (property.PropertyType == typeof(string)) {
                builder.AddProperty<string>(property.Name).WithHeader(propDisplayName).EditAsText();
            } else if (property.PropertyType == typeof(float)) {
                builder.AddProperty<float>(property.Name).WithHeader(propDisplayName).EditAsDouble().ConvertFromProperty(x => (double)x).ConvertToProperty(x => (float)x);
            } else if (property.PropertyType == typeof(int)) {
                builder.AddProperty<int>(property.Name).WithHeader(propDisplayName).EditAsInt();
            } else if (property.PropertyType == typeof(int?)) {
                builder.AddProperty<int?>(property.Name).WithHeader(propDisplayName).EditAsInt().ConvertFromProperty(x => x ?? default);
            } else if (property.PropertyType == typeof(DateTime?)) {
                builder.AddProperty<DateTime?>(property.Name).WithHeader(propDisplayName).EditAsDateTime().ConvertFromProperty(x => x ?? new()).ConvertToProperty(x => x == default ? null : x);
            } else {
                Debugger.Break();
            }
        }

        return builder.Build().EditWithWpfUI();
    }

}