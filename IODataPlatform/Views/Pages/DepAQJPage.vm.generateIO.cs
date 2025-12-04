using Aspose.Cells;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Cells;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.Common;

using LYSoft.Libs.ServiceInterfaces;
using SqlSugar;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Drawing;

namespace IODataPlatform.Views.Pages
{
    partial class DepAQJViewModel
    {
        [RelayCommand]
        private async void GenerateIO()
        {
            _ = Project?.Name ?? throw new("未选择子项！");
            string log = "";
            if (picker.OpenFile("请选择仪表信号基础表(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx") is not string selectedFilePathA || string.IsNullOrEmpty(selectedFilePathA)) return;

            if (picker.OpenFile("请选择控制信号基础表(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx") is not string selectedFilePathD || string.IsNullOrEmpty(selectedFilePathD)) return;

            model.Status.Busy("正在读取模拟量配置表……");
            var configs = await context.Db.Queryable<config_aqj_analog>().ToListAsync();
            model.Status.Busy("正在读取仪表信号基础表并更新所属类型……");
            List<aqjs_InstrumentSignl_input>  deviceInfosA = await GetInstrumentSignalListAsync(selectedFilePathA, configs);
            model.Status.Success($"已成功写入所属类型到{selectedFilePathA}。");


            model.Status.Busy("正在读取数字量配置表……");
            List<config_aqj_control> configsD = await context.Db.Queryable<config_aqj_control>().ToListAsync();
            model.Status.Busy("正在读取控制信号基础表并更新所属类型……");
            List<aqjs_controlSignal_input> deviceInfosD = await GetControlSignalListAsync(selectedFilePathD, configsD);
            model.Status.Success($"已成功写入所属类型到{selectedFilePathD}。");

            // 根据配置生成和所属类型生成IO清册
            var configSignalGroups = await context.Db.Queryable<config_aqj_signalGroup>().ToListAsync();
            var configSignals = await context.Db.Queryable<config_aqj_signalGroupDetail>().ToListAsync();

            var configReplaceStations = await context.Db.Queryable<config_aqj_stationReplace>().ToListAsync();
            var configReplaceTagNames = await context.Db.Queryable<config_aqj_tagReplace>().ToListAsync();
            var configReplaceCabinets = await context.Db.Queryable<config_aqj_stationCabinet>().ToListAsync();

            var result = new List<config_aqj_signalGroupDetail>();

            foreach (var group in deviceInfosA.GroupBy(l => l.设备编号))
            {
                var device = group.FirstOrDefault();
                if (device == null) continue;

                var signalGroup = configSignalGroups.FirstOrDefault(s => s.signalGroupName == device.所属类型);

                if (signalGroup == null) { log += $"未从配置表中找到{device.所属类型}\n"; continue; }

                var signalDetailsConfig = configSignals.Where(c => c.signalGroupId == signalGroup.Id).ToList();
                if (signalDetailsConfig.Count == 0) { log += $"未从配置表中找到{device.所属类型}的信号列表\n"; continue; }

                foreach (var signal in signalDetailsConfig)//遍历每一个信号，开始替换
                {
                    var model = signal.AsTrue<config_aqj_signalGroupDetail>().JsonClone();
                    var configStationReplaces = configReplaceStations.Where(c => c.ControlStation == device.站号);
                    var configTagReplace = configReplaceTagNames.FirstOrDefault(c => c.ControlStation == device.站号 && model.信号名称.Contains(c.IoPointNameBefore));


                    //先替换TagName
                    model.信号名称 = model.信号名称.Replace("TagName", device.设备编号.Replace("-", ""));
                    model.信号说明 = model.信号说明.Replace("TagName", device.设备编号.Replace("-", ""));
                    model.原变量名 = model.原变量名.Replace("TagName", device.设备编号.Replace("-", ""));
                    model.信号说明 = model.信号说明.Replace("FunctionDesc", device.功能描述);
                    model.原变量名 = model.原变量名.Replace("设备编号", device.设备编号);
                    model.安全分级 = ComputeSafetyClass(signal.源头目的, device.安全等级);

                    if (configStationReplaces != null && configStationReplaces.Count() > 0)
                    {
                        foreach (var item in configStationReplaces)
                        {
                            model.分配信息 = model.分配信息.Replace(item.ControlStationBefore, item.ControlStationAfter);
                            model.控制站 = model.控制站.Replace(item.ControlStationBefore, item.ControlStationAfter);
                        }                      
                    }
                    
                    var cabinet = configReplaceCabinets.FirstOrDefault(c => c.ControlStationNumber == model.控制站);
                    model.信号名称 = configTagReplace != null ? model.信号名称.Replace(configTagReplace.IoPointNameBefore, configTagReplace.IoPointNameAfter) : model.信号名称;
                    model.机柜号 = cabinet != null ? cabinet.CabinetNumber : model.机柜号;

                    if (model.IO类型 == "AI")
                    {
                        model.量程上限 = device.仪表量程_最大.ToString();
                        model.量程下限 = device.仪表量程_最小.ToString(); ;
                        model.单位 = device.仪表量程_单位;
                    }
                    else if (model.IO类型 == "AO")
                    {
                        model.量程上限 = device.二次表量程_最大.ToString();
                        model.量程下限 = device.二次表量程_最小.ToString();
                        model.单位 = device.二次表量程_单位;
                    }
                    result.Add(model);
                }
            }

            foreach (var group in deviceInfosD.GroupBy(l => l.设备编号))
            {
                var device = group.FirstOrDefault();
                if (device == null) continue;

                var signalGroup = configSignalGroups.FirstOrDefault(s => s.signalGroupName == device.所属类型);

                if (signalGroup == null) { log += $"未从配置表中找到{device.所属类型}\n"; continue; }

                var signalDetailsConfig = configSignals.Where(c => c.signalGroupId == signalGroup.Id).ToList();
                if (signalDetailsConfig.Count == 0) { log += $"未从配置表中找到{device.所属类型}的信号列表\n"; continue; }

                foreach (var signal in signalDetailsConfig)//遍历每一个信号，开始替换
                {
                    var model = signal.AsTrue<config_aqj_signalGroupDetail>().JsonClone();

                    var configStationReplaces = configReplaceStations.Where(c => c.ControlStation == device.站号);
                    var configTagReplace = configReplaceTagNames.FirstOrDefault(c => c.ControlStation == device.站号 && model.信号名称.Contains(c.IoPointNameBefore));

                    //先替换TagName
                    model.信号名称 = model.信号名称.Replace("TagName", device.设备编号.Replace("-", ""));
                    model.信号说明 = model.信号说明.Replace("TagName", device.设备编号.Replace("-", ""));
                    model.原变量名 = model.原变量名.Replace("TagName", device.设备编号.Replace("-", ""));
                    model.信号说明 = model.信号说明.Replace("FunctionDesc", device.功能描述);
                    model.原变量名 = model.原变量名.Replace("设备编号", device.设备编号);

                    model.安全分级 = ComputeSafetyClass(signal.源头目的, device.安全等级);

                    if (configStationReplaces != null && configStationReplaces.Count() > 0)
                    {
                        foreach (var item in configStationReplaces)
                        {
                            model.分配信息 = model.分配信息.Replace(item.ControlStationBefore, item.ControlStationAfter);
                            model.控制站 = model.控制站.Replace(item.ControlStationBefore, item.ControlStationAfter);
                        }                        
                    }                                                         
                    var cabinet = configReplaceCabinets.FirstOrDefault(c => c.ControlStationNumber == model.控制站);

                    model.信号名称 = configTagReplace != null ? model.信号名称.Replace(configTagReplace.IoPointNameBefore, configTagReplace.IoPointNameAfter) : model.信号名称;
                    model.机柜号 = cabinet != null ? cabinet.CabinetNumber : model.机柜号;
                    //最后再替换机柜号
                    model.信号名称 = model.信号名称.Replace("CabinetNumber", model.机柜号);
                    model.信号说明 = model.信号说明.Replace("CabinetNumber", model.机柜号);
                    result.Add(model);
                }
            }

            //按照机柜号分组，每个机柜增加一个报警列表
            var groupByCabinet = result.GroupBy(r => r.机柜号);
            foreach (var item in groupByCabinet)
            {
                var alarm = configSignalGroups.FirstOrDefault(c => c.signalGroupName.Contains("ALARM"));
                if (alarm == null) continue;
                var alarmSignals = configSignals.Where(c => c.signalGroupId == alarm.Id);
                if (alarmSignals == null || alarmSignals.Count() == 0) continue;
                foreach (var alarmSignal in alarmSignals)
                {
                    //最后再替换机柜号
                    var model = alarmSignal.AsTrue<config_aqj_signalGroupDetail>().JsonClone();
                    //最后再替换机柜号
                    model.信号名称 = alarmSignal.信号名称.Replace("CabinetNumber", item.Key);
                    model.信号说明 = alarmSignal.信号说明.Replace("CabinetNumber", item.Key);
                    model.机柜号 = item.Key;
                    model.控制站 = item.ToList().FirstOrDefault().控制站;
                    model.TagType = TagType.Alarm;
                    result.Add(model);
                }

            }

            //result加序号
            for (int i = 0; i < result.Count; i++)
            {
                result[i].序号 = (i + 1).ToString();
            }
            model.Status.Busy("开始生成安全级室IO清册...");
            var dataTable = await result.ToTableByDisplayAttributeAsync();
            string filePath = Path.Combine(new FileInfo(selectedFilePathA).DirectoryName!, $"{SubProject!.Name}生成的安全级室IO清册.xlsx");
            await excel.FastExportToDesktopAsync(dataTable, filePath);
            model.Status.Success($"已生成IO清册到{filePath}");            
        }

        private string ComputeSafetyClass(string destination,string inputSafetyClass)
        {
            if (string.IsNullOrEmpty(destination)) return "RS";
            if (destination.ToUpper()== ("LOCAL")) return inputSafetyClass;
            if (destination.ToUpper().Contains("NR-DCS")) return "NR";
            return "RS";

        }

        public async Task<List<aqjs_InstrumentSignl_input>> GetInstrumentSignalListAsync(string fileName, List<config_aqj_analog> configs)
        {
            var result = new List<aqjs_InstrumentSignl_input>();
            using var wb = new Workbook(fileName);
            var cells = wb.Worksheets[0].Cells;
            model.Status.Busy("正在检验信号表表头是否符合规范……");
            await VerifyTableAHeadersAsync(cells);

            // 创建一个共享的样式对象
            Aspose.Cells.Style style = wb.CreateStyle();

            // 设置单元格背景颜色
            style.ForegroundColor = Color.Yellow;  // 设置前景颜色
            style.Pattern = BackgroundType.Solid;
            style.HorizontalAlignment = TextAlignmentType.Center;// 设置填充模式为实心
                                                   // 创建一个StyleFlag对象，指定应用的样式部分
            StyleFlag flag = new StyleFlag();
            flag.All = true;  // 确保应用单元格背景颜色
            return await Task.Run(() =>
            {               
                aqjs_InstrumentSignl_input previousItem = null;

                for (int i = 2; i <= cells.MaxDataRow; i++)
                {
                    var item = new aqjs_InstrumentSignl_input
                    {
                        行号 = i,
                        序号 = int.TryParse(cells[i, 0].StringValue, out var sequence) ? sequence : 0,
                        设备编号 = cells[i, 1].StringValue,
                        功能1 = cells[i, 2].StringValue,
                        安全等级 = cells[i, 3].StringValue,
                        序列 = cells[i, 4].StringValue,
                        安全级机柜 = cells[i, 5].StringValue,
                        功能描述 = cells[i, 6].StringValue,
                        IO类型_AI = cells[i, 7].StringValue,
                        IO类型_AO = cells[i, 8].StringValue,
                        IO类型_DI = cells[i, 9].StringValue,
                        IO类型_DO = cells[i, 10].StringValue,
                        信号类型 = cells[i, 11].StringValue,
                        显示控制_DCS = cells[i, 12].StringValue,
                        显示控制_中央控制室 = cells[i, 13].StringValue,
                        显示控制_应急监控室 = cells[i, 14].StringValue,
                        仪表量程_最小 = double.TryParse(cells[i, 15].StringValue, out var yblc_min) ? yblc_min : 0,
                        仪表量程_最大 = double.TryParse(cells[i, 16].StringValue, out var yblc_max) ? yblc_max : 0,
                        仪表量程_单位 = cells[i, 17].StringValue,
                        运算 = cells[i, 18].StringValue,
                        二次表量程_最小 = double.TryParse(cells[i, 19].StringValue, out var eclc_min) ? eclc_min : 0,
                        二次表量程_最大 = double.TryParse(cells[i, 20].StringValue, out var eclc_max) ? eclc_max : 0,
                        二次表量程_单位 = cells[i, 21].StringValue,
                        功能2 = cells[i, 22].StringValue,
                        阈值 = double.TryParse(cells[i, 23].StringValue, out var threshold) ? threshold : 0,
                        阈值_单位 = cells[i, 24].StringValue,
                        转入DCS信号_AI = cells[i, 25].StringValue,
                        转入DCS信号_AO = cells[i, 26].StringValue,
                        转入DCS信号_DI = cells[i, 27].StringValue,
                        转入DCS信号_DO = cells[i, 28].StringValue,
                        DCS机柜 = cells[i, 29].StringValue,
                        附注 = cells[i, 30].StringValue,
                        原理图号 = cells[i, 31].StringValue,
                        所属类型 = cells[i, 32].StringValue,
                        站号 = cells[i, 33].StringValue
                    };//上来先new一个

                    if (string.IsNullOrEmpty(item.设备编号) && previousItem != null) //如果本行数据为空，就复制上一行数据
                    {
                        item.行号 = i;
                        item.序号 = previousItem.序号;
                        cells[i, 0].Value = item.序号;
                        cells[i, 0].SetStyle(style,flag);

                        item.设备编号 = previousItem.设备编号;
                        cells[i, 1].Value = item.设备编号;
                        cells[i, 1].SetStyle(style,flag);

                        if (string.IsNullOrEmpty(item.功能1))
                        {
                            item.功能1 = previousItem.功能1;
                            cells[i, 2].Value = item.功能1;
                            cells[i, 2].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.安全等级))
                        {
                            item.安全等级 = previousItem.安全等级;
                            cells[i, 3].Value = item.安全等级;
                            cells[i, 3].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.序列))
                        {
                            item.序列 = previousItem.序列;
                            cells[i, 4].Value = item.序列;
                            cells[i, 4].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.安全级机柜))
                        {
                            item.安全级机柜 = previousItem.安全级机柜;
                            cells[i, 5].Value = item.安全级机柜;
                            cells[i, 5].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.功能描述))
                        {
                            item.功能描述 = previousItem.功能描述;
                            cells[i, 6].Value = item.功能描述;
                            cells[i, 6].SetStyle(style,flag);  // 应用样式
                        }

                  
                       

                        if (string.IsNullOrEmpty(item.信号类型))
                        {
                            item.信号类型 = previousItem.信号类型;
                            cells[i, 11].Value = item.信号类型;
                            cells[i, 11].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.显示控制_DCS))
                        {
                            item.显示控制_DCS = previousItem.显示控制_DCS;
                            cells[i, 12].Value = item.显示控制_DCS;
                            cells[i, 12].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.显示控制_中央控制室))
                        {
                            item.显示控制_中央控制室 = previousItem.显示控制_中央控制室;
                            cells[i, 13].Value = item.显示控制_中央控制室;
                            cells[i, 13].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.显示控制_应急监控室))
                        {
                            item.显示控制_应急监控室 = previousItem.显示控制_应急监控室;
                            cells[i, 14].Value = item.显示控制_应急监控室;
                            cells[i, 14].SetStyle(style,flag);  // 应用样式
                        }

                        if (item.仪表量程_最小 == 0)
                        {
                            item.仪表量程_最小 = previousItem.仪表量程_最小;
                            cells[i, 15].Value = item.仪表量程_最小;
                            cells[i, 15].SetStyle(style,flag);  // 应用样式
                        }

                        if (item.仪表量程_最大 == 0)
                        {
                            item.仪表量程_最大 = previousItem.仪表量程_最大;
                            cells[i, 16].Value = item.仪表量程_最大;
                            cells[i, 16].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.仪表量程_单位))
                        {
                            item.仪表量程_单位 = previousItem.仪表量程_单位;
                            cells[i, 17].Value = item.仪表量程_单位;
                            cells[i, 17].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.运算))
                        {
                            item.运算 = previousItem.运算;
                            cells[i, 18].Value = item.运算;
                            cells[i, 18].SetStyle(style,flag);  // 应用样式
                        }

                        if (item.二次表量程_最小 == 0)
                        {
                            item.二次表量程_最小 = previousItem.二次表量程_最小;
                            cells[i, 19].Value = item.二次表量程_最小;
                            cells[i, 19].SetStyle(style,flag);  // 应用样式
                        }

                        if (item.二次表量程_最大 == 0)
                        {
                            item.二次表量程_最大 = previousItem.二次表量程_最大;
                            cells[i, 20].Value = item.二次表量程_最大;
                            cells[i, 20].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.二次表量程_单位))
                        {
                            item.二次表量程_单位 = previousItem.二次表量程_单位;
                            cells[i, 21].Value = item.二次表量程_单位;
                            cells[i, 21].SetStyle(style,flag);  // 应用样式
                        }
                                             
                        if (string.IsNullOrEmpty(item.DCS机柜))
                        {
                            item.DCS机柜 = previousItem.DCS机柜;
                            cells[i, 29].Value = item.DCS机柜;
                            cells[i, 29].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.附注))
                        {
                            item.附注 = previousItem.附注;
                            cells[i, 30].Value = item.附注;
                            cells[i, 30].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.原理图号))
                        {
                            item.原理图号 = previousItem.原理图号;
                            cells[i, 31].Value = item.原理图号;
                            cells[i, 31].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.所属类型))
                        {
                            item.所属类型 = previousItem.所属类型;
                            cells[i, 32].Value = item.所属类型;
                            cells[i, 32].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.站号))
                        {
                            item.站号 = previousItem.站号;
                            cells[i, 33].Value = item.站号;
                            cells[i, 33].SetStyle(style,flag);  // 应用样式
                        }
                    }
                    else previousItem = item;
                    result.Add(item);
                }

                var group = result.GroupBy(r => r.设备编号);//按照设备编号分组
                foreach (var device in group)
                {
                    var first = device.FirstOrDefault();
                    var found = configs.FirstOrDefault(c => c.SchematicType == first!.原理图号.Trim() && c.IsContainsR == first.功能1.Contains("R") && IsValidFunction2(device.Select(i => i.功能2.Trim()), c));
                    foreach (var item in device)
                    {
                        cells[item.行号, 32].Value = found == null ? "未找到" : found.DeviceType;
                        cells[item.行号, 32].SetStyle(style,flag);  // 应用样式
                    }
                }
                wb.Save(fileName);
                return result;
            });
        }
        public async Task<List<aqjs_controlSignal_input>> GetControlSignalListAsync(string fileName, List<config_aqj_control> configs)
        {
            var result = new List<aqjs_controlSignal_input>();
            using var wb = new Workbook(fileName);
            var cells = wb.Worksheets[0].Cells;
            // 创建一个共享的样式对象
            Aspose.Cells.Style style = wb.CreateStyle();

            // 设置单元格背景颜色
            style.ForegroundColor = Color.Yellow;  // 设置前景颜色
            style.Pattern = BackgroundType.Solid;  // 设置填充模式为实心
                                                   // style.HorizontalAlignment = TextAlignmentType.Center;
            StyleFlag flag = new StyleFlag();
            flag.All = true;  // 确保应用单元格背景颜色
            model.Status.Busy("正在检验信号表表头是否符合规范……");
            // 创建一个StyleFlag对象，指定应用的样式部分
            await VerifyTableDHeadersAsync(cells);
            return await Task.Run(() =>
            {
                aqjs_controlSignal_input previousItem = null;

                for (int i = 2; i <= cells.MaxDataRow; i++)
                {
                    var item = new aqjs_controlSignal_input
                    {
                        行号 = i,
                        序号 = int.TryParse(cells[i, 0].StringValue, out var sequence) ? sequence : 0,
                        设备编号 = cells[i, 1].StringValue,
                        功能 = cells[i, 2].StringValue,
                        安全等级 = cells[i, 3].StringValue,
                        序列 = cells[i, 4].StringValue,
                        安全级机柜 = cells[i, 5].StringValue,
                        功能描述 = cells[i, 6].StringValue,
                        IO类型_AI = int.TryParse(cells[i, 7].StringValue, out var ioAi) ? ioAi : 0,
                        IO类型_AO = int.TryParse(cells[i, 8].StringValue, out var ioAo) ? ioAo : 0,
                        IO类型_DI = int.TryParse(cells[i, 9].StringValue, out var ioDi) ? ioDi : 0,
                        IO类型_DO = int.TryParse(cells[i, 10].StringValue, out var ioDo) ? ioDo : 0,
                        信号类型 = cells[i, 11].StringValue,
                        供电电压 = cells[i, 12].StringValue,
                        显示控制_DCS = cells[i, 13].StringValue,
                        显示控制_中央控制室 = cells[i, 14].StringValue,
                        显示控制_应急监控室 = cells[i, 15].StringValue,
                        转入DCS信号_AI = cells[i, 16].StringValue,
                        转入DCS信号_AO = cells[i, 17].StringValue,
                        转入DCS信号_DI = cells[i, 18].StringValue,
                        转入DCS信号_DO = cells[i, 19].StringValue,
                        DCS机柜 = cells[i, 20].StringValue,
                        附注 = cells[i, 21].StringValue,
                        原理图号 = cells[i, 22].StringValue,
                        所属类型 = cells[i, 23].StringValue,
                        站号 = cells[i, 24].StringValue
                    };//上来先new一个

                    if (string.IsNullOrEmpty(item.设备编号) && previousItem != null) // 如果本行数据为空，就复制上一行数据
                    {
                        item.行号 = i;

                        item.序号 = previousItem.序号;
                        cells[i, 0].Value = item.序号;
                        cells[i, 0].SetStyle(style,flag);  // 应用样式

                        item.设备编号 = previousItem.设备编号;
                        cells[i, 1].Value = item.设备编号;
                        cells[i, 1].SetStyle(style,flag);  // 应用样式

                        if (string.IsNullOrEmpty(item.功能))
                        {
                            item.功能 = previousItem.功能;
                            cells[i, 2].Value = item.功能;
                            cells[i, 2].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.安全等级))
                        {
                            item.安全等级 = previousItem.安全等级;
                            cells[i, 3].Value = item.安全等级;
                            cells[i, 3].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.序列))
                        {
                            item.序列 = previousItem.序列;
                            cells[i, 4].Value = item.序列;
                            cells[i, 4].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.安全级机柜))
                        {
                            item.安全级机柜 = previousItem.安全级机柜;
                            cells[i, 5].Value = item.安全级机柜;
                            cells[i, 5].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.功能描述))
                        {
                            item.功能描述 = previousItem.功能描述;
                            cells[i, 6].Value = item.功能描述;
                            cells[i, 6].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.信号类型))
                        {
                            item.信号类型 = previousItem.信号类型;
                            cells[i, 11].Value = item.信号类型;
                            cells[i, 11].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.供电电压))
                        {
                            item.供电电压 = previousItem.供电电压;
                            cells[i, 12].Value = item.供电电压;
                            cells[i, 12].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.显示控制_DCS))
                        {
                            item.显示控制_DCS = previousItem.显示控制_DCS;
                            cells[i, 13].Value = item.显示控制_DCS;
                            cells[i, 13].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.显示控制_中央控制室))
                        {
                            item.显示控制_中央控制室 = previousItem.显示控制_中央控制室;
                            cells[i, 14].Value = item.显示控制_中央控制室;
                            cells[i, 14].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.显示控制_应急监控室))
                        {
                            item.显示控制_应急监控室 = previousItem.显示控制_应急监控室;
                            cells[i, 15].Value = item.显示控制_应急监控室;
                            cells[i, 15].SetStyle(style,flag);  // 应用样式
                        }
                        if (string.IsNullOrEmpty(item.DCS机柜))
                        {
                            item.DCS机柜 = previousItem.DCS机柜;
                            cells[i, 20].Value = item.DCS机柜;
                            cells[i, 20].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.附注))
                        {
                            item.附注 = previousItem.附注;
                            cells[i, 21].Value = item.附注;
                            cells[i, 21].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.原理图号))
                        {
                            item.原理图号 = previousItem.原理图号;
                            cells[i, 22].Value = item.原理图号;
                            cells[i, 22].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.所属类型))
                        {
                            item.所属类型 = previousItem.所属类型;
                            cells[i, 23].Value = item.所属类型;
                            cells[i, 23].SetStyle(style,flag);  // 应用样式
                        }

                        if (string.IsNullOrEmpty(item.站号))
                        {
                            item.站号 = previousItem.站号;
                            cells[i, 24].Value = item.站号;
                            cells[i, 24].SetStyle(style,flag);  // 应用样式
                        }
                    }

                    else previousItem = item;
                    result.Add(item);
                }

                var group = result.GroupBy(r => r.设备编号);
                foreach (var device in group)
                {
                    var first = device.FirstOrDefault();
                    var totalDOCount = device.Sum(d => d.IO类型_DO);
                    var totalDICount = device.Sum(d => d.IO类型_DI);
                    var found = configs.FirstOrDefault(c =>
                                                       c.SchematicType == first.原理图号 &&
                                                       c.IOTypeDONumber == totalDOCount &&
                                                       c.IOTypeDINumber == totalDICount &&
                                                       (string.IsNullOrEmpty(c.FunctionDescription) ||
                                                        c.FunctionDescription.Split(';').Any(f => first.功能描述.Contains(f))) );

                    foreach (var item in device)
                    {
                        cells[item.行号, 23].Value = found == null ? "未找到" : found.DeviceType;
                        cells[item.行号, 23].SetStyle(style,flag);
                    }
                }
                wb.Save(fileName);
                return result;
            });
        }


        /// <summary> 验证模拟量输入信息表头  /// </summary> 
        public async Task<bool> VerifyTableAHeadersAsync(Cells cells)
        {
            return await Task.Run(() =>
            {
                // 预期的表头名称列表
                var expectedHeaders = new List<string>
                {
                    "序号", "设备编号", "功能1", "安全等级", "序列", "安全级机柜", "功能描述",
                    "IO类型", "", "", "", "信号类型", "显示/控制", "",
                    "", "仪表量程", "", "", "运算", "二次表量程", "", "",
                    "阈值", "", "", "转入DCS信号","","","","DCS机柜","附注", "原理图号", "所属类型", "站号"
                };

                // 检查表头是否正确
                for (int j = 0; j < expectedHeaders.Count; j++)
                {
                    var a = cells[0, j];
                    var b = cells[1, j];
                    if (cells[0, j].StringValue != expectedHeaders[j])
                    {
                        throw new Exception($"表头不匹配，期望值: {expectedHeaders[j]}, 但找到: {cells[0, j].StringValue}");
                    }
                }
                return true;
            });
        }
        /// <summary> 验证数字量输入信息表头  /// </summary> 
        public async Task<bool> VerifyTableDHeadersAsync(Cells cells)
        {
            return await Task.Run(() =>
            {
                // 预期的表头名称列表
                var expectedHeaders = new List<string>
                {
                    "序号", "设备编号", "功能", "安全等级", "序列", "安全级机柜", "功能描述",
                    "IO类型", "", "", "", "信号类型", "供电电压","显示/控制", "","",
                    "转入DCS信号","","","","DCS机柜","附注", "原理图号", "所属类型", "站号"
                };

                // 检查表头是否正确
                for (int j = 0; j < expectedHeaders.Count; j++)
                {
                    var a = cells[0, j];
                    var b = cells[1, j];
                    if (cells[0, j].StringValue != expectedHeaders[j])
                    {
                        throw new Exception($"表头不匹配，期望值: {expectedHeaders[j]}, 但找到: {cells[0, j].StringValue}");
                    }
                }
                return true;
            });
        }
        public bool IsValidFunction2(IEnumerable<string> function2, config_aqj_analog config)
        {
            if (function2 == null || function2.Count() == 0) return false;

            var relevantProperties = new List<string> { "SH", "AH", "AHH", "AL", "ALL", "SAH", "SAL" };

            return relevantProperties.All(propertyName =>
            {
                var property = typeof(config_aqj_analog).GetProperty(propertyName);
                if (property == null) return false;
                var propValue = (bool)property.GetValue(config);
                return function2.Contains(propertyName) ? propValue : !propValue;
            });
        }

        
    }
}
