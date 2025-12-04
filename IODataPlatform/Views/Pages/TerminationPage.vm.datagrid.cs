using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Views.SubPages.XT2;

using LYSoft.Libs.Editor;
using LYSoft.Libs.Wpf.WpfUI;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace IODataPlatform.Views.Pages
{
    partial class TerminationViewModel
    {

        [RelayCommand]
        private async Task Add()
        {
            if (SubProject is null) { return; }
            if (AllData is null) { return; }

            var data = new TerminationData();
            if (!EditData(data, "添加")) { return; }
            AllData.Add(data);
            await SaveAndUploadRealtimeFileAsync();
            RefreshFilterOptions();
            Filter();
        }

        [RelayCommand]
        private async Task Clear()
        {
            if (!await message.ConfirmAsync("确认清空")) { return; }
            AllData = [];
            await SaveAndUploadRealtimeFileAsync();
        }

        [RelayCommand]
        private async Task Edit(TerminationData data)
        {
            var objToEdit = new TerminationData().CopyPropertiesFrom(data);
            if (!EditData(objToEdit, "编辑")) { return; }
            data.CopyPropertiesFrom(objToEdit);
            DisplayData = [.. DisplayData];
            await SaveAndUploadRealtimeFileAsync();
        }

        [RelayCommand]
        private async Task Delete(TerminationData data)
        {
            _ = AllData ?? throw new("开发人员注意");
            if (!await message.ConfirmAsync("是否删除")) { return; }            
            AllData.Remove(data);
            await SaveAndUploadRealtimeFileAsync();
        }       

        [RelayCommand]
        private async Task Refresh()
        {
            model.Status.Busy("正在刷新……");
            await ReloadAllData();
            model.Status.Reset();
        }

        private bool EditData(TerminationData data, string title)
        {

            var builder = data.CreateEditorBuilder();
            builder.WithTitle(title);
            foreach (var property in typeof(TerminationData).GetProperties())
            {
                var porpDisplay = property.GetCustomAttribute<DisplayAttribute>()!;
                if (!porpDisplay.GetAutoGenerateField().GetValueOrDefault(true)) { continue; }
                var propDisplayName = porpDisplay.Name!;

                if (Filters.SingleOrDefault(x => x.Title == propDisplayName) is { Option: not "全部" } filter)
                {
                    property.SetValue(data, filter.Option);
                }

                if (property.PropertyType == typeof(string))
                {
                    builder.AddProperty<string>(property.Name).WithHeader(propDisplayName).WithPlaceHoplder($"请输入{propDisplayName}");
                }
                else if (property.PropertyType == typeof(float))
                {
                    builder.AddProperty<float>(property.Name).WithHeader(propDisplayName).EditAsDouble().ConvertFromProperty(x => (double)x).ConvertToProperty(x => (float)x);
                }
                else if (property.PropertyType == typeof(int))
                {
                    builder.AddProperty<int>(property.Name).WithHeader(propDisplayName).EditAsInt();
                }
                else if (property.PropertyType == typeof(int?))
                {
                    builder.AddProperty<int?>(property.Name).WithHeader(propDisplayName).EditAsInt().ConvertFromProperty(x => x ?? default);
                }
                else if (property.PropertyType == typeof(DateTime?))
                {
                    builder.AddProperty<DateTime?>(property.Name).WithHeader(propDisplayName).EditAsDateTime().ConvertFromProperty(x => x ?? new()).ConvertToProperty(x => x == default ? null : x);
                }
                else
                {
                    Debugger.Break();
                }
            }

            return builder.Build().EditWithWpfUI();
        }

    }
}
