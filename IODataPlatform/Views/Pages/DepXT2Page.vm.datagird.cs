﻿﻿﻿using Aspose.Cells;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Views.SubPages.XT2;

using LYSoft.Libs.Editor;
using LYSoft.Libs.Wpf.WpfUI;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reflection;

namespace IODataPlatform.Views.Pages;

// 表格部分

partial class DepXT2ViewModel {

    [RelayCommand]
    private async Task Add() {
        if (SubProject is null) { return; }
        if (AllData is null) { return; }

        var data = new IoFullData();
        if (!EditData(data, "添加")) { return; }
        AllData = [.. AllData.Append(data)];
        //AllData = CabinetCalc.BuildCabinetStructureLH([.. AllPoints], await context.Db.Queryable<config_card_type_judge>().ToListAsync()));
        await SaveAndUploadFileAsync();
        RefreshFilterOptions();
        FilterAndSort();
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
        
        // 检查是否修改了影响计算结果的字段（机笼、插槽、通道等）
        bool needRecalc = objToEdit.Cage != data.Cage || 
                          objToEdit.Slot != data.Slot || 
                          objToEdit.Channel != data.Channel ||
                          objToEdit.PowerType != data.PowerType ||
                          objToEdit.IoType != data.IoType ||
                          objToEdit.ElectricalCharacteristics != data.ElectricalCharacteristics;
        
        data.CopyPropertiesFrom(objToEdit);
        DisplayPoints = [.. DisplayPoints];
        
        // 如果修改了关键字段，自动重新计算该点所在机柜（不弹确认框）
        if (needRecalc && SubProject != null)
        {
            var controlSystem = context.Db.Queryable<config_project_major>()
                                      .Where(it => it.Id == SubProject.MajorId).First().ControlSystem;
            await RecalcMethodInternal(controlSystem, data.CabinetNumber, showStatus: false); // 调用内部方法，不显示状态，不弹确认框
        }
        
        await SaveAndUploadFileAsync();
    }

    [RelayCommand]
    private async Task Delete(IoFullData data) {
        _ = AllData ?? throw new("开发人员注意");
        if (!await message.ConfirmAsync("是否删除")) { return; }        
        AllData.Remove(data);     
        await SaveAndUploadFileAsync();
    }

    [RelayCommand]
    private void Import() {
        navigation.NavigateWithHierarchy(typeof(ImportPage));
    }

    [RelayCommand]
    public async Task Refresh() {
        model.Status.Busy("正在刷新……");
        await ReloadAllData();
        model.Status.Reset();
    }

    private bool EditData(IoFullData data, string title) {

        var builder = data.CreateEditorBuilder();
        builder.WithTitle(title);
        foreach (var property in typeof(IoFullData).GetProperties()) {
            var porpDisplay = property.GetCustomAttribute<DisplayAttribute>()!;
            if (!porpDisplay.GetAutoGenerateField().GetValueOrDefault(true)) { continue; }
            var propDisplayName = porpDisplay.Name!;

            // Excel筛选器不再用于编辑默认值，此功能已移除
            // if (Filters.SingleOrDefault(x => x.Title == propDisplayName) is { Option: not "全部" } filter) {
            //     property.SetValue(data, filter.Option);
            // }

            if (property.PropertyType == typeof(string)) {
                builder.AddProperty<string>(property.Name).WithHeader(propDisplayName).WithPlaceHoplder($"请输入{propDisplayName}");
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
        return database.GetLongLqFields(context);
    }

}