using Aspose.Cells;
using IODataPlatform.Models;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Views.Pages;
using IODataPlatform.Views.SubPages.Common;
using IODataPlatform.Views.SubPages.XT2;
using LYSoft.Libs.ServiceInterfaces;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Views.SubPages.OtherFunction
{
    public partial class ExtractExcelToNetListViewModel(
          GlobalModel model, IMessageService message, INavigationService navigation,
          IPickerService picker, WordService word, ExcelService excel, StorageService storage) : ObservableObject
    {

         string templateName = "网络设计清单模板.xlsx";
     
        [ObservableProperty]
        private string dest = "请选择输出目录";

        public ObservableCollection<FileToExtract> ExcelFiles { get; } = [];

        [RelayCommand]
        private void SetDest()
        {
            if (picker.PickFolder() is string folder) { Dest = folder; }
        }

        [RelayCommand]
        private async void UpLoadTemplate()
        {
            if (picker.OpenFile() is not string file) { return; }
           
            model.Status.Busy("正在上传……");                   
            var relativePath = storage.GettemplatesDepFileRelativePath(templateName);
            string localFile = storage.GetWebFileLocalAbsolutePath(relativePath);
            if (File.Exists(localFile)) { File.Delete(localFile); };
            File.Copy(file, localFile);         
            await storage.UploadtemplatesDepFileAsync(templateName);
            model.Status.Success("上传成功！");
        }
    
       

        [RelayCommand]
        private void AppendFiles()
        {
            if (picker.OpenFiles("文件(*.xls; *.xlsx)| *.xls; *.xlsx") is not string[] files) { return; }
            ExcelFiles.Reset(ExcelFiles.AppendRange(files.Select(x => new FileToExtract(x))).Distinct().ToList());
            if (ExcelFiles.GroupBy(x => x.FileName.ToLower()).Any(x => x.Count() > 1))
            {
                throw new("列表中有两个或多个文件名相同，如果开始提取，一部分结果文件会被覆盖");
            }
        }

        [RelayCommand]
        private async Task RemoveAllFiles()
        {
            if (!await message.ConfirmAsync("是否清空文件列表")) { return; }
            ExcelFiles.Clear();
        }

        [RelayCommand]
        private void RemovePdfFile(FileToExtract file)
        {
            ExcelFiles.Remove(file);
        }

        [RelayCommand]
        private void OpenPdfDestFile(FileToExtract file)
        {
            var path = Path.Combine(Dest, $"{file.FileName}.xlsx");
            if (!File.Exists(path)) { throw new("结果文件不存在，请先处理"); }
            Process.Start("explorer.exe", path);
        }

        [RelayCommand]
        private void GoBack()
        {
            navigation.GoBack();
        }

        [RelayCommand]
        private void OpenNetListComparison()
        {
            navigation.NavigateWithHierarchy(typeof(NetListComparisonPage));
        }

        [RelayCommand]
        private async Task CopyToClipboard(string param)
        {
            Clipboard.SetText(param);
            await message.SuccessAsync($"已复制到剪贴板：{param}");
        }

        [RelayCommand]
        private async Task Confirm()
        {
            if (ExcelFiles.Count <2) { throw new Exception("请添加至少IP表和配置信息统计表两个文件"); }
            if (Dest == "请选择输出目录") { throw new("请选择输出目录"); }           
            if (ExcelFiles.Where(f => f.FileName.Contains("配置信息统计表")).Count() != 1) throw new Exception("未识别到[配置信息统计表]关键字，或[配置信息统计表]大于1个");
            if (ExcelFiles.Where(f => f.FileName.Contains("IP表")).Count() == 0) throw new Exception("未识别到IP表，请添加包含[IP表]关键字的文件");

            FileToExtract cabinetRoomFile = ExcelFiles!.FirstOrDefault(f => f.FileName.Contains("配置信息统计表"));//机柜信息表
            IEnumerable<FileToExtract> ipFiles = [.. ExcelFiles.Where(e => e.FileName.Contains("IP"))];//IP表

            if (!Directory.Exists(Dest)) { Directory.CreateDirectory(Dest); }
            model.Status.Busy("正在检查数据格式……");
            await Task.WhenAll(ipFiles.Select(CheckExcel));
            
            if (ipFiles.Where(x => x.Result == "处理完成").Count() > 0)
            {
                model.Status.Busy("正在处理数据……");
                var templatePath = await DownLoadTemplate();
                
                // 执行端口重复检测
                var shouldContinue = await CheckAndPromptDuplication(ipFiles.Where(x => x.Result == "处理完成").Select(x => x.FullName));
                if (!shouldContinue)
                {
                    model.Status.Reset();
                    return;
                }
                
                // 让用户选择保存路径和文件名
                var defaultFileName = $"网络接线清单_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                if (picker.SaveFile("Excel文件|*.xlsx", defaultFileName) is not string savePath)
                {
                    model.Status.Reset();
                    return;
                }
                
                MergeAllExcelData(ipFiles.Where(x => x.Result == "处理完成").Select(x => x.FullName), templatePath, cabinetRoomFile!.FullName, savePath);
                model.Status.Success($"已成功生成网络接线清单：{Path.GetFileName(savePath)}");
                
                // 询问是否打开文件
                if (await message.ConfirmAsync("生成成功！是否立即打开文件？", "生成成功"))
                {
                    System.Diagnostics.Process.Start("explorer.exe", savePath);
                }
            }
            else model.Status.Error("生成失败！");
        }

        private async Task<string> DownLoadTemplate()
        {
            model.Status.Busy("正在下载……");
            var file = await storage.DownloadtemplatesDepFileAsync(templateName);
            model.Status.Reset();
            return file; // 返回下载的文件路径
        }
        public async Task CheckExcel(FileToExtract fileData)
        {
            fileData.Result = "处理中";
            var fullname = fileData.FullName;
            await Task.Run(() => {
                if (new[] { ".xlsx", ".xls" }.Contains(Path.GetExtension(fullname), StringComparer.CurrentCultureIgnoreCase))
                {
                    if (!CheckExcelStandard(fullname))
                    {
                        fileData.Result = "文件不标准";
                        return;
                    }
                    //var dest = Path.Combine(Dest, $"{Path.GetFileNameWithoutExtension(fullname)}.xlsx");
                    //if (!File.Exists(dest))
                    //{
                    //    var workbook = excel.GetWorkbook(fullname);
                    //    workbook.Save(dest);
                    //}                   
                    fileData.Result = "处理完成";
                }
            });
            GC.Collect();
        }

        private bool CheckExcelStandard(string filePath)
        {
            var workbook = excel.GetWorkbook(filePath);

            // 定义需要检查的单元格及其预期值
            var expectedValues = new Dictionary<(int Row, int Column), string>
            {
                { (1, 21), "接口类型" },
                { (3, 2), "机柜或盘台" },
                { (3, 3), "位号" },
                { (3, 4), "端口" },
                { (3, 6), "机柜或盘台" },
                { (3, 7), "位号" },
                { (3, 8), "端口" },
                { (3, 11), "机柜或盘台" },
                { (3, 12), "位号" },
                { (3, 13), "端口" },
                { (3, 15), "机柜或盘台" },
                { (3, 16), "位号" },
                { (3, 17), "端口" }
            };

            return workbook.Worksheets.All(worksheet =>
                expectedValues.All(cell => worksheet.Cells[cell.Key.Row, cell.Key.Column]?.Value?.ToString() == cell.Value));
        }



        /// <summary>
        /// 检测端口重复并提示用户
        /// </summary>
        private async Task<bool> CheckAndPromptDuplication(IEnumerable<string> files)
        {
            List<系统二室网络设备IP表> ipDatas = ExtractData(files);
            var duplicateResult = CheckPortDuplication(ipDatas);
            
            if (duplicateResult.HasDuplicates)
            {
                var warningMessage = BuildDuplicateWarningMessage(duplicateResult);
                model.Status.Reset();
                
                // 弹出确认对话框，允许用户复制信息
                return await message.ConfirmAsync(
                    $"⚠️ 检测到端口重复使用\n\n{warningMessage}\n是否仍要继续生成接线清单？",
                    "端口重复检测");
            }
            
            return true; // 无重复，继续执行
        }

        private void MergeAllExcelData(IEnumerable<string> files, string templatePath, string cabinetRoomPath, string savePath)
        {           
            Dictionary<string, int> cabConnectDic = new Dictionary<string, int>();               
            using var workbook = excel.GetWorkbook(templatePath);
            var finalWorksheet = workbook.Worksheets[0];           
            List<系统二室机柜信息表> cabinetRooms = ExtractData(cabinetRoomPath);
            List<系统二室网络设备IP表> ipDatas = ExtractData(files);
            int startRow = 4;
            for (int i = 0; i < ipDatas.Count; i++)
            {
                系统二室网络设备IP表 rowData = ipDatas[i];              
                switch (rowData.接口类型)
                {
                    case "电口":
                        WriteRows(finalWorksheet, cabinetRooms, ref startRow, rowData, "网线", rowData.A_Start_名称, rowData.A_End_名称, rowData.B_Start_名称, rowData.B_End_名称);
                        break;
                    case "光口":
                        WriteOpticalConnections(finalWorksheet, cabinetRooms,ipDatas, ref startRow, rowData, cabConnectDic);
                        break;
                }
            }     
            workbook.Save(savePath);
        }
     
        private List<系统二室网络设备IP表> ExtractData(IEnumerable<string> files)
        {
            List<系统二室网络设备IP表> cabinetInfo = new List<系统二室网络设备IP表>();
            foreach (var file in files)
            {
                var fileWorkbook = excel.GetWorkbook(file);
                foreach (var worksheet in fileWorkbook.Worksheets)
                {
                    for (int row = 4; row <= worksheet.Cells.MaxDataRow; row++)
                    {
                        cabinetInfo.Add(new 系统二室网络设备IP表
                        {
                            接口类型 = worksheet.Cells[row, 21]?.Value?.ToString(),
                            序号 = worksheet.Cells[row, 0]?.Value?.ToString(),
                            A_Start_名称= worksheet.Cells[row, 1]?.Value?.ToString(),
                            A_Start_机柜或盘台 = worksheet.Cells[row, 2]?.Value?.ToString(),
                            A_Start_位号 = worksheet.Cells[row, 3]?.Value?.ToString(),
                            A_Start_端口 = worksheet.Cells[row, 4]?.Value?.ToString(),
                            A_End_名称 = worksheet.Cells[row, 5]?.Value?.ToString(),
                            A_End_机柜或盘台 = worksheet.Cells[row, 6]?.Value?.ToString(),
                            A_End_位号 = worksheet.Cells[row, 7]?.Value?.ToString(),
                            A_End_端口 = worksheet.Cells[row, 8]?.Value?.ToString(),
                            B_Start_名称 = worksheet.Cells[row, 10]?.Value?.ToString(),
                            B_Start_机柜或盘台 = worksheet.Cells[row, 11]?.Value?.ToString(),
                            B_Start_位号 = worksheet.Cells[row, 12]?.Value?.ToString(),
                            B_Start_端口 = worksheet.Cells[row, 13]?.Value?.ToString(),
                            B_End_名称 = worksheet.Cells[row, 14]?.Value?.ToString(),
                            B_End_机柜或盘台 = worksheet.Cells[row, 15]?.Value?.ToString(),
                            B_End_位号 = worksheet.Cells[row, 16]?.Value?.ToString(),
                            B_End_端口 = worksheet.Cells[row, 17]?.Value?.ToString(),
                            IsDeletedRow = CheckRowStrikeout(worksheet, row)
                        });
                    }
                }
            }                      
            return cabinetInfo;
        }
        private List<系统二室机柜信息表> ExtractData(string path)
        {
            List<系统二室机柜信息表> cabinetInfo = new List<系统二室机柜信息表>();
            var wb = excel.GetWorkbook(path);
            foreach (var worksheet in wb.Worksheets)
            {
                for (int row = 1; row <= worksheet.Cells.MaxDataRow; row++)
                {
                    cabinetInfo.Add(new 系统二室机柜信息表
                    {
                        盘柜位号 = worksheet.Cells[row, 2]?.Value?.ToString(),
                        房间号 = worksheet.Cells[row, 3]?.Value?.ToString(),
                    });
                }
            }
            return cabinetInfo;
        }
        private void WriteRows(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, ref int nextRow, 系统二室网络设备IP表 rowData, string cableType, string aStart,string aEnd,string bStart,string bEnd)
        {
           if(!string.IsNullOrEmpty(rowData.A_Start_机柜或盘台)&&!string.IsNullOrEmpty(rowData.A_End_机柜或盘台) && rowData.A_Start_机柜或盘台 != "/" && rowData.A_End_机柜或盘台 != "/")
            {
                WriteRow(worksheet,  cabinetRooms,ref nextRow,  rowData.序号, rowData.A_Start_机柜或盘台, rowData.A_End_机柜或盘台, rowData.A_Start_位号, rowData.A_Start_端口, rowData.A_End_位号, rowData.A_End_端口, cableType, $"{aEnd}连接{aStart}");
                if (rowData.IsDeletedRow) { ApplyStrikeoutToRow(worksheet, nextRow - 1); }
            }
            if (!string.IsNullOrEmpty(rowData.B_Start_机柜或盘台) && !string.IsNullOrEmpty(rowData.B_End_机柜或盘台) && rowData.B_Start_机柜或盘台 != "/" && rowData.B_End_机柜或盘台 != "/")
            {
                WriteRow(worksheet,cabinetRooms, ref nextRow, rowData.序号, rowData.B_Start_机柜或盘台, rowData.B_End_机柜或盘台, rowData.B_Start_位号, rowData.B_Start_端口, rowData.B_End_位号, rowData.B_End_端口, cableType, $"{bEnd}连接{bStart}");
                if (rowData.IsDeletedRow) { ApplyStrikeoutToRow(worksheet, nextRow - 1); }
            }
        }
        private void WriteRow(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, ref int nextRow, string 序号, string startCabinet, string endCabinet, string startPos, string startPort, string endPos, string endPort, string cableType, string leftConnectRight)
        {
            string? roomStart = cabinetRooms.FirstOrDefault(c => c.盘柜位号 == startCabinet)?.房间号;
            string? roomEnd = cabinetRooms.FirstOrDefault(c => c.盘柜位号 == endCabinet)?.房间号;
            worksheet.Cells[nextRow, 0].Value = 序号;
            worksheet.Cells[nextRow, 1].Value = startCabinet == endCabinet ? "/" : "设计院确定";
            worksheet.Cells[nextRow, 4].Value = cableType;
            worksheet.Cells[nextRow, 5].Value = roomStart ?? "";
            worksheet.Cells[nextRow, 6].Value = startCabinet;
            worksheet.Cells[nextRow, 7].Value = startPos;
            worksheet.Cells[nextRow, 8].Value = startPort;
            worksheet.Cells[nextRow, 9].Value = "/";
            worksheet.Cells[nextRow, 10].Value = startCabinet == endCabinet ? "/" : "设计院确定";
            worksheet.Cells[nextRow, 11].Value = roomEnd ?? "";
            worksheet.Cells[nextRow, 12].Value = endCabinet;
            worksheet.Cells[nextRow, 13].Value = endPos;
            worksheet.Cells[nextRow, 14].Value = endPort;
            worksheet.Cells[nextRow, 15].Value = "/";
            worksheet.Cells[nextRow, 18].Value = leftConnectRight;

            nextRow++;
        }
        private void WriteOpticalConnections(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, List<系统二室网络设备IP表> list, ref int nextRow, 系统二室网络设备IP表 rowData, Dictionary<string, int> cabinetConnectionCount)
        {          
            bool isStartControl = list.Any(l =>
                (l.A_Start_机柜或盘台 == rowData.A_Start_机柜或盘台 && l.A_Start_名称?.Contains("控制器") == true) ||
                (l.A_End_机柜或盘台 == rowData.A_Start_机柜或盘台 && l.A_End_名称?.Contains("控制器") == true));

            bool isEndControl = list.Any(l =>
                (l.A_Start_机柜或盘台 == rowData.A_End_机柜或盘台 && l.A_Start_名称?.Contains("控制器") == true) ||
                (l.A_End_机柜或盘台 == rowData.A_End_机柜或盘台 && l.A_End_名称?.Contains("控制器") == true));
            if (!string.IsNullOrEmpty(rowData.A_Start_机柜或盘台) && !string.IsNullOrEmpty(rowData.A_End_机柜或盘台) && rowData.A_Start_机柜或盘台 != "/" && rowData.A_End_机柜或盘台 != "/")
                WriteComplexOpticalRow(worksheet, cabinetRooms, ref nextRow, rowData, cabinetConnectionCount, rowData.A_Start_机柜或盘台, rowData.A_End_机柜或盘台, isStartControl, isEndControl, rowData.A_Start_名称, rowData.A_End_名称);

            bool isStartControlB = list.Any(l =>
                (l.B_Start_机柜或盘台 == rowData.B_Start_机柜或盘台 && l.B_Start_名称?.Contains("控制器") == true) ||
                (l.B_End_机柜或盘台 == rowData.B_Start_机柜或盘台 && l.B_End_名称?.Contains("控制器") == true));

            bool isEndControlB = list.Any(l =>
                (l.B_Start_机柜或盘台 == rowData.B_End_机柜或盘台 && l.B_Start_名称?.Contains("控制器") == true) ||
                (l.B_End_机柜或盘台 == rowData.B_End_机柜或盘台 && l.B_End_名称?.Contains("控制器") == true));
            if (!string.IsNullOrEmpty(rowData.B_Start_机柜或盘台) && !string.IsNullOrEmpty(rowData.B_End_机柜或盘台) && rowData.B_Start_机柜或盘台 != "/" && rowData.B_End_机柜或盘台 != "/")
                WriteComplexOpticalRowB(worksheet, cabinetRooms, ref nextRow, rowData, cabinetConnectionCount, rowData.B_Start_机柜或盘台, rowData.B_End_机柜或盘台, isStartControlB, isEndControlB, rowData.B_Start_名称, rowData.B_End_名称);
        }


        private void WriteComplexOpticalRow(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, ref int nextRow, 系统二室网络设备IP表 rowData, Dictionary<string, int> cabinetConnectionCount, string startCabinet, string endCabinet,bool isStartD,bool isEndD,string startName,string EndName)
        {
            if (startCabinet == endCabinet)
            {
                WriteRow(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, rowData.A_Start_位号, rowData.A_Start_端口, rowData.A_End_位号, rowData.A_End_端口, "单模双芯LC光纤跳线", $"{EndName}连接{startName}");
                if (rowData.IsDeletedRow) { ApplyStrikeoutToRow(worksheet, nextRow - 1); }
            }
            else
            {
                int startCabinetConnections = IncrementCabinetConnection(cabinetConnectionCount, startCabinet);
                int endCabinetConnections = IncrementCabinetConnection(cabinetConnectionCount, endCabinet);
                WriteOpticalRow1(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, rowData.A_Start_位号, rowData.A_Start_端口, isStartD, startCabinetConnections, $"{EndName}连接{startName}");
                if (rowData.IsDeletedRow) { ApplyStrikeoutToRow(worksheet, nextRow - 1); }
                WriteOpticalCableRow(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, startCabinetConnections, endCabinetConnections, isStartD, isEndD, $"{EndName}连接{startName}");
                if (rowData.IsDeletedRow) { ApplyStrikeoutToRow(worksheet, nextRow - 1); }
                WriteOpticalRow2(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, rowData.A_End_位号, rowData.A_End_端口, isEndD, endCabinetConnections, $"{EndName}连接{startName}");
                if (rowData.IsDeletedRow) { ApplyStrikeoutToRow(worksheet, nextRow - 1); }
            }
        }
        private void WriteComplexOpticalRowB(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, ref int nextRow, 系统二室网络设备IP表 rowData, Dictionary<string, int> cabinetConnectionCount, string startCabinet, string endCabinet, bool isStartD, bool isEndD, string startName, string EndName)
        {
            if (startCabinet == endCabinet)
            {
                WriteRow(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, rowData.B_Start_位号, rowData.B_Start_端口, rowData.B_End_位号, rowData.B_End_端口, "单模双芯LC光纤跳线", $"{EndName}连接{startName}");
                if (rowData.IsDeletedRow) { ApplyStrikeoutToRow(worksheet, nextRow - 1); }
            }
            else
            {
                int startCabinetConnections = IncrementCabinetConnection(cabinetConnectionCount, startCabinet);
                int endCabinetConnections = IncrementCabinetConnection(cabinetConnectionCount, endCabinet);
                WriteOpticalRow1(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, rowData.B_Start_位号, rowData.B_Start_端口, isStartD, startCabinetConnections, $"{EndName}连接{startName}");
                if (rowData.IsDeletedRow) { ApplyStrikeoutToRow(worksheet, nextRow - 1); }
                WriteOpticalCableRow(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, startCabinetConnections, endCabinetConnections, isStartD, isEndD, $"{EndName}连接{startName}");
                if (rowData.IsDeletedRow) { ApplyStrikeoutToRow(worksheet, nextRow - 1); }
                WriteOpticalRow2(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, rowData.B_End_位号, rowData.B_End_端口, isEndD, endCabinetConnections, $"{EndName}连接{startName}");
                if (rowData.IsDeletedRow) { ApplyStrikeoutToRow(worksheet, nextRow - 1); }
            }
        }
        //第一行
        private void WriteOpticalRow1(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms,  ref int nextRow, string 序号, string startCabinet,string endCabinet, string startPos, string startPort, bool isStartControl, int startConnections,string connectName) { 
            string? roomStart = cabinetRooms.FirstOrDefault(c => c.盘柜位号 == startCabinet)?.房间号;         
            worksheet.Cells[nextRow, 0].Value = 序号;
            worksheet.Cells[nextRow, 1].Value = "/";
            worksheet.Cells[nextRow, 4].Value = "单模双芯LC光纤跳线";
            worksheet.Cells[nextRow, 5].Value = roomStart ?? "";
            worksheet.Cells[nextRow, 6].Value = startCabinet;
            worksheet.Cells[nextRow, 7].Value = $"{startPos}";
            worksheet.Cells[nextRow, 8].Value = $"{startPort}-TX";
            worksheet.Cells[nextRow, 9].Value = $"{startPort}-RX";
            worksheet.Cells[nextRow, 10].Value = "/";
            worksheet.Cells[nextRow, 11].Value = roomStart ?? "";
            worksheet.Cells[nextRow, 12].Value = startCabinet;
            worksheet.Cells[nextRow, 13].Value = $"{CalculateBpNumber(isStartControl, startConnections)}BP";
            worksheet.Cells[nextRow, 14].Value = $"{CalculateCurrentConnection(isStartControl, startConnections)}-TX";
            worksheet.Cells[nextRow, 15].Value = $"{CalculateCurrentConnection(isStartControl, startConnections)}-RX";
            worksheet.Cells[nextRow, 18].Value = connectName;
            nextRow++;
        }
        //第二行
        private void WriteOpticalCableRow(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, ref int nextRow, string 序号, string startCabinet, string endCabinet, int startConnections, int endConnections,bool isStartControl,bool isEndControl,string connectName)
        {
            string? roomStart = cabinetRooms.FirstOrDefault(c => c.盘柜位号 == startCabinet)?.房间号;
            string? roomEnd = cabinetRooms.FirstOrDefault(c => c.盘柜位号 == endCabinet)?.房间号;
            worksheet.Cells[nextRow, 0].Value = 序号;
            worksheet.Cells[nextRow, 1].Value = startCabinet == endCabinet ? "/" : "设计院确定";
            worksheet.Cells[nextRow, 4].Value = "四芯单模光缆";
            worksheet.Cells[nextRow, 5].Value = roomEnd ?? "";
            worksheet.Cells[nextRow, 6].Value = endCabinet;
            worksheet.Cells[nextRow, 7].Value = $"{CalculateBpNumber(isEndControl, endConnections)}BP";
            worksheet.Cells[nextRow, 8].Value = $"{CalculateCurrentConnection(isEndControl, endConnections)}-RX";
            worksheet.Cells[nextRow, 9].Value = $"{CalculateCurrentConnection(isEndControl, endConnections)}-TX";
            worksheet.Cells[nextRow, 10].Value = startCabinet == endCabinet ? "/" : "设计院确定";
            worksheet.Cells[nextRow, 11].Value = roomStart ?? "";
            worksheet.Cells[nextRow, 12].Value = startCabinet;
            worksheet.Cells[nextRow, 13].Value = $"{CalculateBpNumber(isStartControl, startConnections)}BP";
            worksheet.Cells[nextRow, 14].Value = $"{CalculateCurrentConnection(isStartControl, startConnections)}-TX";
            worksheet.Cells[nextRow, 15].Value = $"{CalculateCurrentConnection(isStartControl, startConnections)}-RX";
            worksheet.Cells[nextRow, 18].Value = connectName;
            nextRow++;
        }
        //第三行
        private void WriteOpticalRow2(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, ref int nextRow, string 序号, string startCabinet, string endCabinet, string endPos, string endPort, bool isEndControl, int endConnections,string connectName)
        {
            string? roomEnd = cabinetRooms.FirstOrDefault(c => c.盘柜位号 == endCabinet)?.房间号;
            worksheet.Cells[nextRow, 0].Value = 序号;
            worksheet.Cells[nextRow, 1].Value = "/";
            worksheet.Cells[nextRow, 4].Value = "单模双芯LC光纤跳线";
            worksheet.Cells[nextRow, 5].Value = roomEnd ?? "";
            worksheet.Cells[nextRow, 6].Value = endCabinet;
            worksheet.Cells[nextRow, 7].Value = $"{CalculateBpNumber(isEndControl, endConnections)}BP";
            worksheet.Cells[nextRow, 8].Value = $"{CalculateCurrentConnection(isEndControl, endConnections)}-RX";
            worksheet.Cells[nextRow, 9].Value = $"{CalculateCurrentConnection(isEndControl, endConnections)}-TX";
            worksheet.Cells[nextRow, 10].Value = "/";
            worksheet.Cells[nextRow, 11].Value = roomEnd ?? "";
            worksheet.Cells[nextRow, 12].Value = endCabinet;
            worksheet.Cells[nextRow, 13].Value = $"{endPos}";
            worksheet.Cells[nextRow, 14].Value = $"{endPort}-RX" ;
            worksheet.Cells[nextRow, 15].Value = $"{endPort}-TX";
            worksheet.Cells[nextRow, 18].Value = connectName;
            nextRow++;
        }
        private int IncrementCabinetConnection(Dictionary<string, int> cabinetConnectionCount, string cabinet)
        {
            if (!cabinetConnectionCount.ContainsKey(cabinet))
            {
                cabinetConnectionCount[cabinet] = 1;
            }
            else
            {
                cabinetConnectionCount[cabinet]++;
            }
            return cabinetConnectionCount[cabinet];
        }
        private int CalculateBpNumber(bool  isControlCabinet, int totalConnections)
        {
            int connectionNumber = isControlCabinet ? 3 : 10;
            return 100 + (totalConnections % connectionNumber == 0 ? totalConnections / connectionNumber : 1 + totalConnections / connectionNumber);
        }
        private int CalculateCurrentConnection(bool isControlCabinet, int totalConnections)
        {
            int connectionNumber = isControlCabinet ? 3 : 10;
            return totalConnections % connectionNumber == 0 ? connectionNumber : totalConnections % connectionNumber;
        }

        // 检查某一行是否存在删除线（任意单元格）
        private bool CheckRowStrikeout(Worksheet worksheet, int row)
        {
            int maxCol = worksheet.Cells.MaxDataColumn;
            for (int col = 0; col <= maxCol; col++)
            {
                var cell = worksheet.Cells[row, col];
                if (cell != null)
                {
                    var style = cell.GetStyle();
                    if (style != null && style.Font != null && style.Font.IsStrikeout)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // 将整行设置为删除线样式
        private void ApplyStrikeoutToRow(Worksheet worksheet, int row, int startCol = 0, int? endCol = null)
        {
            int lastCol = endCol ?? worksheet.Cells.MaxDataColumn;
            var wb = worksheet.Workbook;
            var strikeStyle = wb.CreateStyle();
            strikeStyle.Font.IsStrikeout = true;

            for (int col = startCol; col <= lastCol; col++)
            {
                worksheet.Cells[row, col].SetStyle(strikeStyle);
            }
        }

        /// <summary>
        /// 检测端口重复使用
        /// </summary>
        private PortDuplicationResult CheckPortDuplication(List<系统二室网络设备IP表> ipDatas)
        {
            var result = new PortDuplicationResult();
            var portUsageDict = new Dictionary<string, List<PortUsageInfo>>();

            for (int i = 0; i < ipDatas.Count; i++)
            {
                var rowData = ipDatas[i];
                var rowIndex = i + 1; // 用于提示是第几条数据

                // 检查A路起点
                if (!string.IsNullOrEmpty(rowData.A_Start_机柜或盘台) && rowData.A_Start_机柜或盘台 != "/" &&
                    !string.IsNullOrEmpty(rowData.A_Start_位号) && !string.IsNullOrEmpty(rowData.A_Start_端口))
                {
                    CheckAndRecordPort(portUsageDict, rowData.A_Start_机柜或盘台, rowData.A_Start_位号, 
                        rowData.A_Start_端口, rowIndex, "A路起点", rowData.序号);
                }

                // 检查A路终点
                if (!string.IsNullOrEmpty(rowData.A_End_机柜或盘台) && rowData.A_End_机柜或盘台 != "/" &&
                    !string.IsNullOrEmpty(rowData.A_End_位号) && !string.IsNullOrEmpty(rowData.A_End_端口))
                {
                    CheckAndRecordPort(portUsageDict, rowData.A_End_机柜或盘台, rowData.A_End_位号, 
                        rowData.A_End_端口, rowIndex, "A路终点", rowData.序号);
                }

                // 检查B路起点
                if (!string.IsNullOrEmpty(rowData.B_Start_机柜或盘台) && rowData.B_Start_机柜或盘台 != "/" &&
                    !string.IsNullOrEmpty(rowData.B_Start_位号) && !string.IsNullOrEmpty(rowData.B_Start_端口))
                {
                    CheckAndRecordPort(portUsageDict, rowData.B_Start_机柜或盘台, rowData.B_Start_位号, 
                        rowData.B_Start_端口, rowIndex, "B路起点", rowData.序号);
                }

                // 检查B路终点
                if (!string.IsNullOrEmpty(rowData.B_End_机柜或盘台) && rowData.B_End_机柜或盘台 != "/" &&
                    !string.IsNullOrEmpty(rowData.B_End_位号) && !string.IsNullOrEmpty(rowData.B_End_端口))
                {
                    CheckAndRecordPort(portUsageDict, rowData.B_End_机柜或盘台, rowData.B_End_位号, 
                        rowData.B_End_端口, rowIndex, "B路终点", rowData.序号);
                }
            }

            // 找出重复使用的端口
            foreach (var kvp in portUsageDict)
            {
                if (kvp.Value.Count > 1)
                {
                    result.DuplicatePorts.Add(kvp.Key, kvp.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// 记录端口使用情况
        /// </summary>
        private void CheckAndRecordPort(Dictionary<string, List<PortUsageInfo>> portUsageDict, 
            string cabinet, string position, string port, int dataRowIndex, string location, string 序号)
        {
            // 端口键：机柜|位号|端口
            string portKey = $"{cabinet}|{position}|{port}";

            if (!portUsageDict.ContainsKey(portKey))
            {
                portUsageDict[portKey] = new List<PortUsageInfo>();
            }

            portUsageDict[portKey].Add(new PortUsageInfo
            {
                Cabinet = cabinet,
                Position = position,
                Port = port,
                DataRowIndex = dataRowIndex,
                Location = location,
                序号 = 序号
            });
        }

        /// <summary>
        /// 构建重复端口的警告信息
        /// </summary>
        private string BuildDuplicateWarningMessage(PortDuplicationResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"共发现 {result.DuplicatePorts.Count} 个端口重复使用：");
            sb.AppendLine();

            int count = 1;
            foreach (var kvp in result.DuplicatePorts.OrderBy(x => x.Value.First().Cabinet))
            {
                var usages = kvp.Value;
                var first = usages.First();
                sb.AppendLine($"【{count}】机柜/盘台：{first.Cabinet}  |  位号：{first.Position}  |  端口：{first.Port}");
                sb.AppendLine($"     重复使用 {usages.Count} 次：");
                
                foreach (var usage in usages.OrderBy(u => u.DataRowIndex))
                {
                    sb.AppendLine($"     • 序号 {usage.序号}（第{usage.DataRowIndex}条数据，{usage.Location}）");
                }
                sb.AppendLine();
                count++;
            }
            
            sb.AppendLine("提示：您可以复制此信息进行核对。");

            return sb.ToString();
        }
    }

    /// <summary>
    /// 端口使用信息
    /// </summary>
    public class PortUsageInfo
    {
        public string Cabinet { get; set; }
        public string Position { get; set; }
        public string Port { get; set; }
        public int DataRowIndex { get; set; }
        public string Location { get; set; }
        public string 序号 { get; set; }
    }

    /// <summary>
    /// 端口重复检测结果
    /// </summary>
    public class PortDuplicationResult
    {
        public Dictionary<string, List<PortUsageInfo>> DuplicatePorts { get; set; } = new();
        public bool HasDuplicates => DuplicatePorts.Any();
    }
}
