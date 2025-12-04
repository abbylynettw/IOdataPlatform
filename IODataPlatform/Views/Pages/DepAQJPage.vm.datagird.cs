using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.AQJ;
using LYSoft.Libs.Editor;
using LYSoft.Libs.Wpf.WpfUI;
using SqlSugar;

namespace IODataPlatform.Views.Pages;

// 表格部分

partial class DepAQJViewModel {

    [RelayCommand]
    private async Task Add() {
        if (SubProject is null) { return; }
        if (AllData is null) { return; }

        var data = new IoFullData();
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
    private async Task Edit(IoFullData data) {
        var objToEdit = new IoFullData().CopyPropertiesFrom(data);
        if (!EditData(objToEdit, "编辑")) { return; }
        data.CopyPropertiesFrom(objToEdit);
        await SaveAndUploadFileAsync();
        await ReloadAllData();
    }

    [RelayCommand]
    private async Task Delete(IoFullData data) {
        _ = AllData ?? throw new("开发人员注意");
        if (!await message.ConfirmAsync("是否删除")) { return; }

        AllData.Remove(data);
        await SaveAndUploadFileAsync();
    }

    [RelayCommand]
    private async Task Import() {
        navigation.NavigateWithHierarchy(typeof(ImportPage));
    }

    [RelayCommand]
    private async Task Refresh() {
        model.Status.Busy("正在刷新……");
        await ReloadAllData();
        model.Status.Reset();
    }

    private bool EditData(IoFullData data, string title) {

        var builder = data.CreateEditorBuilder();
        builder.WithTitle(title);
        foreach (var property in typeof(IoFullData).GetProperties()) {
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

    public List<string> GetDefaultField()
    {
        if (SubProject is null) { throw new Exception("子项目为空，找不到控制系统"); }
        var controsystem = context.Db.Queryable<config_project_major>()
                                  .Where(it => it.Id == SubProject.MajorId).First().ControlSystem;

        return controsystem switch
        {
            Models.ControlSystem.龙核 => database.GetLongHeFields(context),
            Models.ControlSystem.安全级模拟系统 => database.GetAQJMNFields(context),
            _ => throw new Exception($"安全级室不存在{controsystem},控制系统错误"),
        };
    }
}