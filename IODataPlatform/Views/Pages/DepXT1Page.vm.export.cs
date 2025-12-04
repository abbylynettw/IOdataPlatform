using System.Data;
using System.IO;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using Microsoft.Xaml.Behaviors.Layout;

namespace IODataPlatform.Views.Pages
{
    partial class DepXT1ViewModel
    {

        [RelayCommand]
        private async Task Export(string param)
        {
            if (AllData == null) { throw new("没有可导出的数据"); }

            if (picker.SaveFile("Excel 文件(*.xlsx)|*.xlsx", param) is not string file) { return; }
            if (File.Exists(file)) { File.Delete(file); }

            model.Status.Busy($"正在{param}数据……");
            if (param == "导出IO点表")
            {
                var data = AllData.Reset(AllData).ToList();
                await ExportIOSubStation(data, file);
            }           
            model.Status.Success($"已成功{param}数据：{file}");
        }

        private async Task ExportIOSubStation(List<IoFullData> data, string filePath)
        {          
            using var dataTable = await data.ToTableByDisplayAttributeAsync();
            await excel.FastExportSheetAsync(dataTable, filePath, "系统一室IO表");
        }

       
       



       
      

    }
}
