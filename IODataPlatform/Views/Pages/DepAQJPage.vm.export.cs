using System.Data;
using System.IO;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using Microsoft.Xaml.Behaviors.Layout;

namespace IODataPlatform.Views.Pages
{
    partial class DepAQJViewModel
    {

        [RelayCommand]
        private async Task Export(string param)
        {
            if (AllData == null) { throw new("没有可导出的数据"); }

            if (picker.SaveFile("Excel 文件(*.xlsx)|*.xlsx", param) is not string file) { return; }
            if (File.Exists(file)) { File.Delete(file); }

            model.Status.Busy($"正在{param}数据……");
            if (param == "导出IO清册（原格式-过渡使用）")
            {
                if (SubProject is null) { throw new Exception("子项目为空，找不到控制系统"); }       
                
                var data = AllData.Reset(AllData).ToList();
                var controsystem = context.Db.Queryable<config_project_major>()
                                  .Where(it => it.Id == SubProject.MajorId).First().ControlSystem;

                await ExportIOOriginal(data, file, controsystem);
            }
            else if (param == "导出IO清册（标准格式）") 
            {
                var data = AllData.Reset(AllData).ToList();
                await ExportIOStandord(data,file);
            }
            model.Status.Success($"已成功{param}数据：{file}");
        }

        private async Task ExportIOStandord(List<IoFullData> data, string filePath)
        {          
            using var dataTable = await data.ToTableByDisplayAttributeAsync();
            await excel.FastExportSheetAsync(dataTable, filePath, "标准IO清单");
        }

        private async Task ExportIOOriginal(List<IoFullData> data, string filePath, ControlSystem controlSystem)
        {
            using var dataTable = data.ToCustomDataTable(context.Db, controlSystem);            
            await excel.FastExportSheetAsync(dataTable, filePath, "安全级IO清册");
        }
    }
}
