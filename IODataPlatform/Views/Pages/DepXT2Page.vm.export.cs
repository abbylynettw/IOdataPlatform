﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿#pragma warning disable CA1822 // 将成员标记为 static

using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Windows;
using Microsoft.Xaml.Behaviors.Layout;

namespace IODataPlatform.Views.Pages;

// 导出部分

partial class DepXT2ViewModel {

    /// <summary>
    /// 动态导出命令 - 打开导出配置窗口
    /// </summary>
    [RelayCommand]
    private async Task DynamicExport()
    {
        if (AllData == null) 
        { 
            model.Status.Warn("没有可导出的数据");
            return; 
        }

        try
        {
            var exportWindow = new ExportConfigWindow(
                AllData.ToList(), 
                picker, 
                excel, 
                message,
                context,
                cloudExportConfigService,
                SubProject?.Id
            );
            
            exportWindow.Owner = Application.Current.MainWindow;
            var result = exportWindow.ShowDialog();
            
            if (result == true)
            {
                model.Status.Success("IO清单导出完成");
            }
        }
        catch (Exception ex)
        {
            model.Status.Error($"打开导出配置窗口失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出到桌面并打开命令 - 快速导出当前控制系统IO清单到桌面并自动打开
    /// </summary>
    [RelayCommand]
    private async Task ExportToDesktopAndOpen()
    {
        if (AllData == null || AllData.Count == 0)
        {
            model.Status.Warn("没有可导出的数据");
            return;
        }

        try
        {
            model.Status.Busy("正在导出IO清单到桌面...");

            // 获取桌面路径
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            
            // 生成文件名：项目名_专业_子项_IO清单.xlsx（不加时间戳，同一子项可覆盖）
            string projectName = Project?.Name ?? "未命名项目";
            string majorName = Major?.CableSystem ?? "未命名专业";
            string subProjectName = SubProject?.Name ?? "未命名子项";
            string fileName = $"{projectName}_{majorName}_{subProjectName}_IO清单.xlsx";
            string filePath = Path.Combine(desktopPath, fileName);

            // 获取当前控制系统
            var currentControlSystem = SubProject != null 
                ? context.Db.Queryable<config_project_major>()
                    .Where(it => it.Id == SubProject.MajorId).First().ControlSystem
                : (ControlSystem?)null;
                
            if (currentControlSystem.HasValue)
            {
                // 使用映射字段名导出（复用ExportConfigWindow的逻辑）
                var dataTable = AllData.ToList().ToCustomDataTable(context.Db, currentControlSystem.Value);
                await excel.FastExportAsync(dataTable, filePath);
            }
            else
            {
                // 如果没有控制系统，使用标准字段名导出
                using var dataTable = await AllData.ToTableByDisplayAttributeAsync();
                await excel.FastExportAsync(dataTable, filePath);
            }

            model.Status.Success($"IO清单已导出到桌面：{fileName}");

            // 打开文件
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            model.Status.Error($"导出到桌面失败：{ex.Message}");
        }
    }



    [RelayCommand]
    private async Task Export(string param)
    {
        if (AllData == null) 
        { 
            model.Status.Warn("没有可导出的数据");
            return;
        }

        // 龙鳍数据库导出使用配置窗口，不需要文件选择对话框
        if (param == "导出龙鳍数据库")
        {
            await OpenDatabaseExportConfigWindow();
            return;
        }

        // 其他导出功能需要先选择文件路径
        if (picker.SaveFile("Excel 文件(*.xlsx)|*.xlsx", param) is not string file) { return; }
        if (File.Exists(file)) { File.Delete(file); }

        model.Status.Busy($"正在{param}数据……");
        
        try
        {
            if (param == "导出主系统板卡清单")
            {
                await ExportMainSystemBoardList([.. AllData], file);
            }
            else if (param == "导出板卡统计信息")
            {
                await ExportBoardStatisticsInfo([.. AllData], file);
            }
            else if (param == "导出端接清单")
            {
                await ExportDuanjie([.. AllData], file);
            }
            else if (param == "导出FF从站箱端接清单")
            {
                await ExportFFDuanjie([.. AllData], file);
            }
            else if (param == "导出FF总线箱端接清单")
            {
                await ExportFFZXDuanjie([.. AllData], file);
            }
            else if (param == "导出机柜报警表")
            {
                await ExportCabinetAlarmTable([.. AllData], file);
            }
            
            model.Status.Success($"已成功{param}数据：{file}");
        }
        catch (Exception ex)
        {
            model.Status.Error($"{param}失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 打开数据库导出配置窗口
    /// </summary>
    [RelayCommand]
    private async Task OpenDatabaseExportConfigWindow()
    {
        if (AllData == null || AllData.Count == 0)
        {
            model.Status.Warn("没有可导出的数据");
            return;
        }
        
        // 获取当前子项目的控制系统
        if (SubProject is null) 
        { 
            model.Status.Error("子项目为空，找不到控制系统");
            return;
        }
        
        var controlSystem = context.Db.Queryable<config_project_major>()
                                  .Where(it => it.Id == SubProject.MajorId)
                                  .First()
                                  .ControlSystem;

        try
        {
            var exportWindow = new DatabaseExportConfigWindow(
                AllData.ToList(),
                picker,
                excel,
                controlSystem  // 传入控制系统
            );

            exportWindow.Owner = Application.Current.MainWindow;
            var result = exportWindow.ShowDialog();

            if (result == true)
            {
                model.Status.Success("数据库导出完成");
            }
        }
        catch (Exception ex)
        {
            model.Status.Error($"打开数据库导出配置窗口失败：{ex.Message}");
        }
    }

    private async Task ExportMainSystemBoardList(List<IoFullData> data, string filePath)
    {       
        var groupedData = Getxtes_mainsystemboard_list(data);
        using var dataTable = await groupedData.ToTableByPropertyNameAsync();
        await excel.FastExportSheetAsync(dataTable, filePath, "主系统板卡");
    } 

    public List<xtes_mainsystemboard_list> Getxtes_mainsystemboard_list(List<IoFullData> data)
    {
        var groupedData = new List<xtes_mainsystemboard_list>();
        var cabinets = data.BuildCabinetStructureOther(this.config_Card_Types);
        foreach (var cabinet in cabinets)
        {
            AddControllerAndPowerBoards(groupedData, cabinet);

            foreach (var cage in cabinet.Cages)
            {
                foreach (var slot in cage.Slots)
                {
                    if (slot.Board != null) // 确保插槽中有板卡
                    {
                        var tagFirst = slot.Board.Channels.Select(c => c.Point).FirstOrDefault(c => c != null);
                        groupedData.Add(CreateBoardListItem(cabinet.Name, cage.Index, slot.Index, slot.Board, tagFirst));
                    }
                }
            }

            // 补全未使用的插槽
            CompleteUnusedSlots(groupedData, cabinet);
        }

        // 最终按机柜编号、机架号和插槽号排序
        return groupedData.OrderBy(g => g.机柜编号).ThenBy(g => g.机架号).ThenBy(g => g.插槽).ToList();
    }

    private void AddControllerAndPowerBoards(List<xtes_mainsystemboard_list> list, StdCabinet cabinet)
    {
        int startAddress = cabinet.Cages
        .SelectMany(cage => cage.Slots)
        .Where(slot => slot.Board != null)
        .SelectMany(slot => slot.Board.Channels)
        .Select(channel => channel.Point)
        .FirstOrDefault(point => point != null && !string.IsNullOrEmpty(point.StationNumber)) is var point && int.TryParse(point?.StationNumber, out int address) ? address : 0;


        // 添加控制器板卡
        list.AddRange(new[]
        {
        new xtes_mainsystemboard_list
        {
            机柜编号 = cabinet.Name,
            机架号 = 1,
            插槽 = 1,
            地址 = startAddress.ToString(),
            板卡类型 = "NP204",
            板卡编号 = $"{cabinet.Name}101UC",
            描述 = "控制器",
        },
        new xtes_mainsystemboard_list
        {
            机柜编号 = cabinet.Name,
            机架号 = 1,
            插槽 = 2,
            地址 = (startAddress + 128).ToString(),
            板卡类型 = "NP204",
            板卡编号 = $"{cabinet.Name}102UC",
            描述 = "控制器",
        }
        });

        // 添加电源板卡
        for (int i = 0; i < 3; i++)
        {
            list.Add(new xtes_mainsystemboard_list
            {
                机柜编号 = cabinet.Name,
                机架号 = i + 1,
                插槽 = 11,
                地址 = i == 0 ? "B" : i + "B",
                板卡类型 = "PM211",
                描述 = "DC24V电源板卡",
            });
            list.Add(new xtes_mainsystemboard_list
            {
                机柜编号 = cabinet.Name,
                机架号 = i + 1,
                插槽 = 12,
                地址 = i == 0 ? "C" : i + "C",
                板卡类型 = "PM211",
                描述 = "DC24V电源板卡",
            });
        }
    }

    private xtes_mainsystemboard_list CreateBoardListItem(string cabinetName, int cageIndex, int slotIndex, Board board,IoFullData? tag)
    {
        return new xtes_mainsystemboard_list
        {
            机柜编号 = cabinetName,
            机架号 = cageIndex,
            插槽 = slotIndex,
            地址 = GetAddress(cageIndex, slotIndex),
            板卡类型 = GetCardType(cageIndex, slotIndex, board.Type),
            板卡编号 = GetCardNumber(cabinetName, cageIndex, slotIndex, board.Type),
            端子板类型 = GetTerminalBoardType(board.Type, slotIndex, tag),
            端子板编号 = GetTerminalBoardNumber(cageIndex, slotIndex, tag),
            描述 = GetDescription(board.Type),
        };
    }

    private void CompleteUnusedSlots(List<xtes_mainsystemboard_list> list, StdCabinet cabinet)
    {
        var slotsUsed = list.Where(g => g.机柜编号 == cabinet.Name).Select(g => (g.机架号, g.插槽)).ToList();
        for (int rack = 1; rack <= 3; rack++)
        {
            for (int slot = 1; slot <= 12; slot++)
            {
                if (!slotsUsed.Contains((rack, slot)))
                {
                    list.Add(new xtes_mainsystemboard_list
                    {
                        机柜编号 = cabinet.Name,
                        机架号 = rack,
                        插槽 = slot,
                        地址 = GetAddress(rack, slot),
                        板卡类型 = "备用",
                        描述 = "空插槽",
                    });
                }
            }
        }
    }

    private async Task ExportBoardStatisticsInfo(List<IoFullData> data, string filePath)
    {     
        var list = Getxtes_mainsystemboard_list(data);
        var aggregatedStatistics = list.GroupBy(board => new { board.板卡类型, board.端子板类型 })
                                          .Select((group, index) => new xtes_board_statistics
                                          {
                                              序号 = index + 1,
                                              板卡 = group.Key.板卡类型,
                                              板卡数量 = group.Count(),
                                              端子板 = group.Key.端子板类型,
                                              端子板数量 = group.Count()
                                          }).ToList();
        using var dataTable = await aggregatedStatistics.ToTableByPropertyNameAsync();
        await excel.FastExportSheetAsync(dataTable, filePath, "板卡统计");
    }
    private string GetAddress(int cage, int slot) {
        return (cage, slot) switch {
            (1, < 3) => "", 
            (_, < 11) => ((cage - 1) * 10 + slot).ToString(),
            _ => "--",
        };
    }

    private string GetCardType(int cage, int slot, string cardType) {
        return (cage, slot) switch {
            (1, 1 or 2) => "NP204",  
            (_, 11 or 12) => "PM211",  
            _ => cardType, 
        };
    }

    private string GetCardNumber(string cabinetNumber, int cage, int slot, string cardType) {
        string cardNumber = cabinetNumber + GetCardInCabinetNumber(cage, slot, cardType);
        return (cage, slot) switch {
            (1, 1 or 2) => $"{cabinetNumber}{cage}{slot:00}UC",
            (_, 11 or 12) => "--",
            (_, 13 or 14) => $"{cabinetNumber}{cage}{slot:00}PM",
            _ => cardNumber,
        };
    }

    private string GetTerminalBoardType(string cardType, int slot, IoFullData? tag) =>
    cardType switch
    {
        "MD211" => "TB244",
        "NP204" or "MD212" or "MD216" => "--",
        _ when slot is 11 or 12 or 13 or 14 => "--",
        _ => tag?.TerminalBoardModel ?? "--"
    };


    private string GetTerminalBoardNumber(int cage, int slot, IoFullData? tag) =>
      (cage, slot) switch
      {
          (1, 1 or 2) => "--",                  
          (_, 11 or 12 or 13 or 14) => "--",    
          _ => tag?.TerminalBoardNumber ?? "--" 
      };


    private string GetDescription(string cardType) {
        return cardType switch {
            "AI212" => "8通道模拟量输入模块",
            "FF211" => "FF H1网关通信模块",
            "DP211" => "PROFIBUS-DP",
            "AI216" => "8通道带HART功能模拟量输入模块",
            "AI221" => "8通道热电偶模块",
            "AI231" => "8通道热电阻模块",
            "AO211" => "8通道模拟量输出模块",
            "AO212" => "8通道模拟量输出模块",
            "DI211" => "16通道开关量输入模块",
            "DO211" => "16通道开关量输出模块",
            "NP204" => "控制器",
            "PM211" => "DC24V电源板卡",
            "备用" => "",
            "--" => "总线扩展连接模块",
            "MD212" => "MODBUS TCP通讯板卡",
            "MD211" => "Modbus RTU通讯板卡",
            "PI211" => "脉冲量输入模块",
            _ => "Unknown"
        };
    }

    private async Task ExportDuanjie(List<IoFullData> data, string filePath)
    {       
        var terminations = data.Select(x => x.toduanjieData()).ToList();
        using var dataTable = await terminations.ToTableByDisplayAttributeAsync();
        await excel.FastExportSheetAsync(dataTable, filePath, "系统二室端接清单");
    }

    private async Task ExportDatabase(IList<IoFullData> data, string filePath) {       

        var dataAI = data.Where(d => d.IoType.Contains("AI"));
        var dataPI = data.Where(d => d.CardType.Contains("PI"));
        var dataAO = data.Where(d => d.CardType.Contains("AO"));
        var dataDI = data.Where(d => d.CardType.Contains("DI"));
        var dataDO = data.Where(d => d.CardType.Contains("DO"));

        var dataGBP = data.Where(d => d.CardType.Contains("DO"));
        var dataGCP = data.Where(d => d.CardType.Contains("AO"));
        var dataGST = new List<xtes_GST>();
        var dataGKC = new List<xtes_GKC>();


        var (aviTask, pviTask, avoTask, dviTask, dvoTask,gbpTask,gcpTask,gstTask,gkcTask ) = (          
            new FormularHelper().ConvertToAviList(dataAI).ToTableByDisplayAttributeAsync(),
            new FormularHelper().ConvertToPviList(dataPI).ToTableByDisplayAttributeAsync(),         
            new FormularHelper().ConvertToAvoList(dataAO).ToTableByDisplayAttributeAsync(),
            new FormularHelper().ConvertToDviList(dataDI).ToTableByDisplayAttributeAsync(),
            new FormularHelper().ConvertToDvoList(dataDO).ToTableByDisplayAttributeAsync(),

            new FormularHelper().ConvertToGBPList(dataGBP).ToTableByDisplayAttributeAsync(),
            new FormularHelper().ConvertToGCPList(dataGCP).ToTableByDisplayAttributeAsync(),
            new FormularHelper().ConvertToGSTList(data).ToTableByDisplayAttributeAsync(),
            new FormularHelper().ConvertToGKCList(data).ToTableByDisplayAttributeAsync()
        );

        var (avi, pvi, avo, dvi, dvo, gbp, gcp, gst, gkc) = (await aviTask, await pviTask, await avoTask, await dviTask, await dvoTask, await gbpTask, await gcpTask, await gstTask, await gkcTask);

        await Task.Run(() => {
            var wb = excel.GetWorkbook();
            // Delete the default Sheet1 if it exists
            var sheet1 = wb.Worksheets.FirstOrDefault(ws => ws.Name == "Sheet1");
            if (sheet1 != null)
            {
                wb.Worksheets.RemoveAt("Sheet1");
            }
            void writeData(DataTable dataTable, string name) {
                var ws = wb.Worksheets.Add(name);
                var cells = ws.Cells;
                cells.ImportData(dataTable, 0, 0, new() { IsFieldNameShown = true });
                dataTable.Dispose();
            }
         
            writeData(avi, nameof(avi).ToUpper());
            writeData(pvi, nameof(pvi).ToUpper());
            writeData(avo, nameof(avo).ToUpper());
            writeData(dvi, nameof(dvi).ToUpper());
            writeData(dvo, nameof(dvo).ToUpper());
            writeData(gbp, nameof(gbp).ToUpper());
            writeData(gcp, nameof(gcp).ToUpper());
            writeData(gst, nameof(gst).ToUpper());
            writeData(gkc, nameof(gkc).ToUpper());

            wb.Save(filePath);
        });

    }
   
    


}
