using Aspose.Cells;
using Aspose.Pdf;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Models.ExcelModels.OtherFunction;
using IODataPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Views.Pages
{
    public partial class OtherFunctionViewModel
    {
        [RelayCommand]
        public async void AddBackUpTag()
        {
            if (picker.OpenFile("中控IO分站清单(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx") is not string selectedFilePath || string.IsNullOrEmpty(selectedFilePath))
                return;

            // 打开Excel文件
            model.Status.Busy($"正在识别IO分站清单……");                       
            using var workbook = await Task.Run(() => excel.GetWorkbook(selectedFilePath));

            var worksheet = workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("IO分站清单"));
            _ = worksheet ?? throw new($"{selectedFilePath}中不存在IO分站清单Sheet");
            model.Status.Busy($"正在生成备用点……");
            ExportTableOptions exportOptions = new ExportTableOptions
            {
                ExportColumnName = true,
                CheckMixedValueType = true
            };
            // 导出工作表数据到DataTable
            DataTable dt = await Task.Run(() => worksheet.Cells.ExportDataTable(0, 0, worksheet.Cells.MaxDataRow + 1, worksheet.Cells.MaxDataColumn + 1, exportOptions));
            List<IO分站清单_中控> ioStations = await Task.Run(() => dt.StringTableToIEnumerableByDiplay<IO分站清单_中控>().ToList());            
            ioStations = ioStations.Where(i => !i.CardType.Contains("COM") && !i.CardType.Contains("AM")).ToList();
                dt.Clear();
            var groupedByCabinet = ioStations.GroupBy(io => io.CabinetNumber);
            foreach (var cabinetTag in groupedByCabinet)
            {
                string cabinetNumber = cabinetTag.Key;
                var groupedByRack = cabinetTag.GroupBy(c => c.Rack);
                foreach (var rackTag in groupedByRack)
                {
                    int rack = rackTag.Key;
                    var groupedBySlot = rackTag.GroupBy(r => r.Slot);
                    foreach (var slotTags in groupedBySlot)
                    {
                        int slot = slotTags.Key;
                        var firstTag = slotTags.First();
                        string ioType = firstTag.IOType;
                        if (ioType == null) throw new Exception($"{firstTag.TagName} IO类型为空，请增加后再重新生成！");
                        string systemCode = firstTag.SystemCode;
                        string cardNumber = firstTag.CardNumber;
                        string signalCharacteristic = firstTag.SignalCharacteristic;
                        string powerSupplyType = firstTag.PowerSupplyType;

                        int requiredChannels = ioType[0] switch
                        {
                            'A' => 8, // AI or AO
                            'D' => 16, // DI or DO
                            'P' => 6, // PI
                            _ => 0
                        };
                        string measurementUnit = ioType[0] switch
                        {
                            'A' => "%", // AI or AO
                            'P' => "Hz", // PI
                            _ => ""
                        };
                        string rangeLowerLimit = ioType[0] switch
                        {
                            'A' or 'P' => "0.00", // AI, AO, PI
                            _ => ""
                        };
                        double rangeUpperLimit = ioType[0] switch
                        {
                            'A' => 100.00, // AI or AO
                            'P' => 1000.00, // PI
                            _ => 0
                        };
                        var currentChannels = slotTags.Select(st => st.ChannelOrFFSegment).ToList();
                        for (int i = 1; i <= requiredChannels; i++)
                        {
                            if (!currentChannels.Contains(i))
                            {
                                var newTag = new IO分站清单_中控
                                {
                                    SequenceNumber = slotTags.Max(s => s.SequenceNumber) + 1,                                  
                                    SignalTag = $"{cardNumber}CH{i:D2}",
                                    SignalFunction = "备用",
                                    CardType = slotTags.FirstOrDefault(s => s.CardType != null)?.CardType,
                                    SystemCode = systemCode,
                                    CabinetNumber = cabinetNumber,
                                    Rack = rack,
                                    Slot = slot,
                                    ChannelOrFFSegment = i,
                                    IOType = ioType,
                                    SignalCharacteristic = signalCharacteristic,
                                    PowerSupplyType = powerSupplyType,
                                    MeasurementUnit = measurementUnit,
                                    RangeLowerLimit = rangeLowerLimit,
                                    RangeUpperLimit = rangeUpperLimit,
                                    ChannelAddress = $"000-{rack - 1:D3}-{slot - 1:D3}-{i-1:D3}"

                                };
                                ioStations.Add(newTag);

                                // 在DataTable中添加新的行

                                var newRow = dt.NewRow();
                                newRow["序号"] = newTag.SequenceNumber;                              
                                newRow["信号位号"] = newTag.SignalTag;
                                newRow["信号功能"] = newTag.SignalFunction;
                                newRow["板卡类型"] = newTag.CardType;
                                newRow["系统代码"] = newTag.SystemCode;
                                newRow["机柜号"] = newTag.CabinetNumber;
                                newRow["机架"] = newTag.Rack;
                                newRow["插槽"] = newTag.Slot;
                                newRow["通道/FF网段"] = newTag.ChannelOrFFSegment;
                                newRow["IO类型"] = newTag.IOType;
                                newRow["信号特性"] = newTag.SignalCharacteristic;
                                newRow["供电类型"] = newTag.PowerSupplyType;
                                newRow["测量单位"] = newTag.MeasurementUnit;
                                newRow["量程下限"] = newTag.RangeLowerLimit;
                                newRow["量程上限"] = newTag.RangeUpperLimit;
                                newRow["通道地址"] = newTag.ChannelAddress;
                                dt.Rows.Add(newRow);
                            }
                        }
                    }
                }
            }


            model.Status.Busy($"正在将备用点添加至表格……");
            // 获取当前工作表的最后一行
            int lastRow = worksheet.Cells.MaxDataRow;
            int newLastRow = lastRow + dt.Rows.Count;

            // 使用ImportDataTable一次性插入新数据，从最后一行开始
            worksheet.Cells.ImportDataTable(dt, false, lastRow + 1, 0, true);

            //// 复制最后一行的公式到新行
            //for (int row = lastRow + 1; row <= newLastRow; row++)
            //{
            //    for (int col = 0; col <= worksheet.Cells.MaxDataColumn; col++)
            //    {
            //        var sourceCell = worksheet.Cells[lastRow, col];
            //        var targetCell = worksheet.Cells[row, col];

            //        if (sourceCell.IsFormula)
            //        {
            //            targetCell.Formula = sourceCell.Formula.Replace((lastRow + 1).ToString(), (row + 1).ToString());
            //        }
            //    }
            //}          
            //// 重新计算公式
            //workbook.CalculateFormula();
            // 保存工作簿
            workbook.Save(selectedFilePath);
            model.Status.Success($"添加完毕,共添加{dt.Rows.Count}条，从{lastRow + 2}开始。");
        }



        [RelayCommand]

        public async void GenerateBitList()
        {
            if (picker.OpenFile("请选择要提取的IO分站清单(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx") is not string selectedFilePath || string.IsNullOrEmpty(selectedFilePath)) return;
            if (picker.PickFolder() is not string folder) { return; }

            var dtCards = new List<板卡清单_生成位号表>();
            var dtIOStations = new List<IO分站清单_中控>();
            model.Status.Busy("正在读取数据...");
            await Task.Run(() =>
            {
                var workbook = excel.GetWorkbook(selectedFilePath);
                ExportTableOptions exportOptions = new ExportTableOptions
                {
                    ExportColumnName = true,
                    CheckMixedValueType = true
                };
                workbook.Worksheets.ToList().ForEach(ws =>
                {
                    var dt = ws.Cells.ExportDataTable(0, 0, ws.Cells.MaxDataRow + 1, ws.Cells.MaxDataColumn + 1, exportOptions);
                    if (ws.Name.Contains("板卡清单"))
                        dtCards.AddRange(dt.StringTableToIEnumerableByDiplay<板卡清单_生成位号表>());
                    else if (ws.Name.Contains("IO分站清单"))
                        dtIOStations.AddRange(dt.StringTableToIEnumerableByDiplay<IO分站清单_中控>());
                });
            });

            model.Status.Busy("正在生成清单...");
            //todo 板卡清单只去掉COM
           
            dtCards = dtCards.Where(d => !d.CardType.StartsWith("COM")).ToList();
            // 根据机柜号分组
            if (dtIOStations.Where(c => c.CardType == null).Count() > 0) throw new Exception("Io分站清单每一行的板卡类型都不能为空");
            var groupedByCabinet = dtIOStations.Where(d => !(d.CardType.StartsWith("AM") || d.CardType.StartsWith("COM"))).GroupBy(io => io.CabinetNumber);
           
            // 遍历每个机柜号的分组
            foreach (var group in groupedByCabinet)
            {
                var cabinetNumber = group.Key;
                var ioStations = group.ToList();
                var cabinetCards = dtCards.Where(d => d.CabinetNumber == cabinetNumber).ToList();
                // 使用机柜号创建文件名
                string destIO = Path.Combine(folder, $"{cabinetNumber}io.xls");
                string destHardWare = Path.Combine(folder, $"{cabinetNumber}hardware.xls");

                #region io.xls
                // 生成 AI 和 AO 的 DataTable
                var dataTableAI = await GenerateAI(ioStations.Where(i => i.IOType == "AI" || i.IOType == "PI" || i.IOType == "P").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
                var dataTableAO = await GenerateAO(ioStations.Where(i => i.IOType == "AO").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
                var dataTableDI = await GenerateDI(ioStations.Where(i => i.IOType == "DI").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
                var dataTableDO = await GenerateDO(ioStations.Where(i => i.IOType == "DO").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();

                var dataTableNA = await GenerateNA(ioStations.Where(i => i.IOType == "NA").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
                var dataTableND = await GenerateND(ioStations.Where(i => i.IOType == "ND").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
                var dataTableNN = await GenerateNN(ioStations.Where(i => i.IOType == "NN").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
                var dataTablePA = await GeneratePA(ioStations.Where(i => i.IOType == "PA").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
                var dataTablePD = await GeneratePD(ioStations.Where(i => i.IOType == "PD").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
                var dataTablePN = await GeneratePN(ioStations.Where(i => i.IOType == "PN").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
                var dataTablePB = await GenerateFB(ioStations.Where(i => i.IOType == "PB").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();

                // 异步导出数据表到 Excel 文件
                if (File.Exists(destIO)) File.Delete(destIO);
                await excel.FastExportSheetAsync(dataTableAI, destIO, "AI",3);
                await excel.FastExportSheetAsync(dataTableAO, destIO, "AO",3);
                await excel.FastExportSheetAsync(dataTableDI, destIO, "DI",3);
                await excel.FastExportSheetAsync(dataTableDO, destIO, "DO",3);
                                                                          
                await excel.FastExportSheetAsync(dataTableNA, destIO, "NA",3);
                await excel.FastExportSheetAsync(dataTableND, destIO, "ND",3);
                await excel.FastExportSheetAsync(dataTableNN, destIO, "NN",3);
                await excel.FastExportSheetAsync(dataTablePA, destIO, "PA",3);
                await excel.FastExportSheetAsync(dataTablePD, destIO, "PD",3);
                await excel.FastExportSheetAsync(dataTablePN, destIO, "PN",3);
                await excel.FastExportSheetAsync(dataTablePB, destIO, "PB",3);

                #endregion

                #region hardware.xls
                if (File.Exists(destHardWare)) File.Delete(destHardWare);
                // 生成 AI 和 AO 的 DataTable
                var dataTableControl = await GenerateControl(cabinetCards).ToTableByDisplayAttributeAsync();
                var datatableIOLinkModel = await GenerateIOLinkModel(cabinetCards).ToTableByDisplayAttributeAsync();
                var datatableRack = await GenerateIORack(cabinetCards).ToTableByDisplayAttributeAsync();
                var datatableIO = await GenerateIO(ioStations, cabinetCards).ToTableByDisplayAttributeAsync();
                var datatableChannel = await GenerateChannel(ioStations, cabinetCards).ToTableByDisplayAttributeAsync();


                // 异步导出数据表到 Excel 文件
                await excel.FastExportSheetAsync(dataTableControl, destHardWare, "Control");
                await excel.FastExportSheetAsync(datatableIOLinkModel, destHardWare, "IO Link Module");
                await excel.FastExportSheetAsync(datatableRack, destHardWare, "IO Rack");
                await excel.FastExportSheetAsync(datatableIO, destHardWare, "IO");
                await excel.FastExportSheetAsync(datatableChannel, destHardWare, "Channel");
                #endregion
            }
            model.Status.Success($"生成成功到{folder}！");
        }

        // 生成 AI 位号表的方法
        private List<AI_生成位号表> GenerateAI(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
        {
            // 初始化结果列表
            var aiList = new List<AI_生成位号表>();
            // 序号从0开始
            int sequenceNumber = 1;
            // 遍历每个 IO 分站清单项
            ioStations.ForEach(io =>
            {
                // 获取匹配的板卡清单项
                // 将 Slot 转换为两位数格式
                var normalizedSlot = io.Slot.ToString().PadLeft(2, '0');

                // 查找匹配的板卡项，比较时将 SlotNumber 也转换为两位数格式
                var controlStationAddress = cardList.FirstOrDefault(c => c.SlotNumber.ToString().PadLeft(2, '0') == normalizedSlot)?.ControllerAddress;
                // 创建新的 AI 位号表对象
                double.TryParse(io.RangeLowerLimit, out double lowerLimit);
                var ai = new AI_生成位号表
                {
                    SequenceNumber = sequenceNumber++.ToString(), // 从0开始往后排
                    Name = io.TagName, // IO分站清单sheet“标签名称” 映射
                    Description = io.SignalFunction, // IO分站清单sheet“信号功能” 映射
                    ControlStationAddress = controlStationAddress, // 板卡清单sheet“控制器地址” 映射
                    Address = io.ChannelAddress, // IO分站清单sheet“通道地址” 映射
                    RangeLowerLimit = Math.Round(lowerLimit, 3), // IO分站清单sheet“量程下限” 映射
                    RangeUpperLimit = Math.Round(io.RangeUpperLimit, 3), // IO分站清单sheet“量程上限” 映射
                    Unit = io.MeasurementUnit, // IO分站清单sheet“测量单位” 映射
                    TagType = "常规AI位号", // 固定
                    ModuleType = GetModuleType(io.CardType), // IO分站清单sheet“板卡类型” + 固定 组合
                    SignalType = GetSignalType(io.CardType),
                    TagOperatingCycle = "基本扫描周期", // 固定
                    DataType = "2字节整数(有符号)", // 固定
                    SignalNature = "工程量", // 固定
                    StatusCodePosition = "状态码在前", // 固定
                    DataFormat = "不转换", // 固定
                    ConversionType = GetDataFormat(io.CardType), // 固定
                    LinearSquareRoot = "不开方", // 固定
                    SmallSignal = "不切除", // 固定
                    SmallSignalCutoffValue = 0.500, // 固定
                    FilterTimeConstant = 0.0000, // 固定
                    ExtendedRangeUpperPercentage = 10.000, // 固定
                    ExtendedRangeLowerPercentage = 10.000, // 固定
                    OverrangeUpperAlarm = "使能", // 固定
                    OverrangeLowerAlarm = "使能", // 固定
                    InputRawCodeUpperLimit = 100.000, // 固定
                    InputRawCodeLowerLimit = 0.000, // 固定
                    HighTripLimitAlarm = "禁止", // 固定
                    HighTripLimitAlarmLevel = "低", // 固定
                    HighTripLimitAlarmValue = 100.000, // 固定
                    HighHighLimitAlarm = "禁止", // 固定
                    HighHighLimitAlarmLevel = "低", // 固定
                    HighHighLimitAlarmValue = 95.000, // 固定
                    HighLimitAlarm = "禁止", // 固定
                    HighLimitAlarmLevel = "低", // 固定
                    HighLimitAlarmValue = 90.000, // 固定
                    LowLimitAlarm = "禁止", // 固定
                    LowLimitAlarmLevel = "低", // 固定
                    LowLimitAlarmValue = 10.000, // 固定
                    LowLowLimitAlarm = "禁止", // 固定
                    LowLowLimitAlarmLevel = "低", // 固定
                    LowLowLimitAlarmValue = 5.000, // 固定
                    LowTripLimitAlarm = "禁止", // 固定
                    LowTripLimitAlarmLevel = "低", // 固定
                    LowTripLimitAlarmValue = 0.000, // 固定
                    HighLowLimitAlarmHysteresisValue = 0.500, // 固定
                    RateOfChangeAlarm = "禁止", // 固定
                    RateOfChangeAlarmLevel = "低", // 固定
                    RateOfChangeAlarmValue = 100.000, // 固定
                    FaultAlarm = "使能", // 固定
                    FaultAlarmLevel = "低", // 固定
                    FaultHandling = "保持", // 固定
                    TagGroup = "位号分组 0", // 固定
                    TagLevel = "0级", // 固定
                    DecimalPlaces ="3" // 固定
                };
                // 添加到列表
                aiList.Add(ai);
            });

            // 返回生成的列表
            return aiList;
        }
        private List<AO_生成位号表> GenerateAO(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
        {
            var aoList = new List<AO_生成位号表>();
            int sequenceNumber = 1;

            foreach (var io in ioStations)
            {
                // 获取控制站地址
                var controlStationAddress = cardList.FirstOrDefault(c => c.CardNumber == io.CardNumber)?.ControllerAddress;

                // 组合模块类型
                var moduleType = io.CardType switch
                {
                    "AO711-S" => "电流信号输出模块（8路）",
                    "AO711-H" => "电流信号输出模块（8路 HART）",
                    _ => "未知模块类型"
                };
                moduleType = $"{io.CardType} {moduleType}";
                double.TryParse(io.RangeLowerLimit, out double lowerLimit);
                var ao = new AO_生成位号表
                {
                    SequenceNumber = sequenceNumber++.ToString(), // 从0开始往后排
                    Name = io.TagName, // IO分站清单sheet“标签名称” 映射
                    Description = io.SignalFunction, // IO分站清单sheet“信号功能” 映射
                    ControlStationAddress = controlStationAddress, // 板卡清单sheet“控制器地址” 映射
                    Address = io.ChannelAddress, // IO分站清单sheet“通道地址” 映射
                    RangeLowerLimit = lowerLimit, // IO分站清单sheet“量程下限” 映射
                    RangeUpperLimit = io.RangeUpperLimit, // IO分站清单sheet“量程上限” 映射
                    Unit = io.MeasurementUnit, // IO分站清单sheet“测量单位” 映射
                    TagType = "常规AO位号", // 固定
                    ModuleType = moduleType, // IO分站清单sheet“板卡类型”+ “固定值” 组合
                    SignalType = "电流(4mA～20mA)", // 固定
                    FaultSafetyMode = "输出保持", // 固定
                    FaultStateSetValue = 0.000, // 固定
                    TagOperatingCycle = "基本扫描周期", // 固定
                    DataType = "2字节整数(无符号)", // 固定
                    SignalNature = "工程量", // 固定
                    StatusCodePosition = "状态码在前", // 固定
                    DataFormat = "不转换", // 固定
                    ConversionType = "线性转换", // 固定
                    PositiveNegativeOutput = "正输出", // 固定
                    ExtendedRangeUpperPercentage = 0.000, // 固定
                    ExtendedRangeLowerPercentage = 0.000, // 固定
                    OverrangeUpperAlarm = "禁止", // 固定
                    OverrangeLowerAlarm = "禁止", // 固定
                    OutputRawCodeUpperLimit = 100.000, // 固定
                    OutputRawCodeLowerLimit = 0.000, // 固定
                    OutputHighLimitClippingAlarm = "禁止", // 固定
                    OutputHighLimitClippingAlarmLevel = "低", // 固定
                    OutputHighLimitClippingValue = 50.000, // 固定
                    OutputLowLimitClippingAlarm = "禁止", // 固定
                    OutputLowLimitClippingAlarmLevel = "低", // 固定
                    OutputLowLimitClippingValue = 0.000, // 固定
                    FaultAlarm = "使能", // 固定
                    FaultAlarmLevel = "低", // 固定
                    ConfigurationErrorAlarm = "使能", // 固定
                    ConfigurationErrorAlarmLevel = "低", // 固定
                    TagGroup = "位号分组 0", // 固定
                    TagLevel = "0级", // 固定
                    DecimalPlaces = "3" // 固定
                };

                aoList.Add(ao);
            }

            return aoList;
        }
        private List<DI_生成位号表> GenerateDI(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
        {
            var diList = new List<DI_生成位号表>();
            int sequenceNumber = 1;

            foreach (var io in ioStations)
            {
                var controlStationAddress = cardList
                    .FirstOrDefault(c => c.CardNumber == io.CardNumber)?.ControllerAddress;

                var di = new DI_生成位号表
                {
                    SequenceNumber = sequenceNumber++.ToString(), // 从0开始往后排
                    Name = io.TagName, // IO分站清单sheet“标签名称” 映射
                    Description = io.SignalFunction, // IO分站清单sheet“信号功能” 映射
                    ControlStationAddress = controlStationAddress, // 板卡清单sheet“控制器地址” 映射
                    Address = io.ChannelAddress, // IO分站清单sheet“通道地址” 映射
                    OnDescription = "ON", // 固定
                    OffDescription = "OFF", // 固定
                    TagType = "常规DI位号", // 固定
                    ModuleType = "DI711-S 数字信号输入模块（16路，24V）", // 固定
                    TagOperatingCycle = "基本扫描周期", // 固定
                    InputReverse = "禁止", // 固定
                    OnStateAlarm = "禁止", // 固定
                    OnStateAlarmLevel = "低", // 固定
                    OffStateAlarm = "禁止", // 固定
                    OffStateAlarmLevel = "低", // 固定
                    PositiveJumpAlarm = "禁止", // 固定
                    PositiveJumpAlarmLevel = "低", // 固定
                    NegativeJumpAlarm = "禁止", // 固定
                    NegativeJumpAlarmLevel = "低", // 固定
                    FaultAlarm = "使能", // 固定
                    FaultAlarmLevel = "低", // 固定
                    FaultHandling = "保持", // 固定
                    TagGroup = "位号分组 0", // 固定
                    TagLevel = "0级", // 固定
                    SoeHardPointMarker = "0", // 固定
                    SoeFlag = "否", // 固定
                    SoeDescription = "", // 可选，空字符串
                    SoeDeviceGroup = "" // 可选，空字符串
                };

                diList.Add(di);
            }

            return diList;
        }
        private List<DO_生成位号表> GenerateDO(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
        {
            var doList = new List<DO_生成位号表>();
            int sequenceNumber = 1;

            foreach (var io in ioStations)
            {
                var controlStationAddress = cardList
                    .FirstOrDefault(c => c.CardNumber == io.CardNumber)?.ControllerAddress;

                var doItem = new DO_生成位号表
                {
                    SequenceNumber = sequenceNumber++.ToString(), // 从0开始往后排
                    Name = io.TagName, // IO分站清单sheet“标签名称” 映射
                    Description = io.SignalFunction, // IO分站清单sheet“信号功能” 映射
                    ControlStationAddress = controlStationAddress, // 板卡清单sheet“控制器地址” 映射
                    Address = io.ChannelAddress, // IO分站清单sheet“通道地址” 映射
                    OnDescription = "ON", // 固定
                    OffDescription = "OFF", // 固定
                    TagType = "常规DO位号", // 固定
                    ModuleType = "DO711-S 数字信号输出模块（16路）", // 固定
                    FaultSafetyMode = "输出保持", // 固定
                    FaultStateSetting = "OFF", // 固定
                    TagOperatingCycle = "基本扫描周期", // 固定
                    OutputReverse = "禁止", // 固定
                    OnStateAlarm = "禁止", // 固定
                    OnStateAlarmLevel = "低", // 固定
                    OffStateAlarm = "禁止", // 固定
                    OffStateAlarmLevel = "低", // 固定
                    FaultAlarm = "使能", // 固定
                    FaultAlarmLevel = "低", // 固定
                    TagGroup = "位号分组 0", // 固定
                    TagLevel = "0级", // 固定
                    SoeFlag = "否", // 固定
                    SoeDescription = string.Empty, // 可选，空字符串
                    SoeDeviceGroup = string.Empty // 可选，空字符串
                };

                doList.Add(doItem);
            }

            return doList;
        }
        private List<NA_生成位号表> GenerateNA(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
        {
            var naList = new List<NA_生成位号表>();           

            return naList;
        }
        private List<ND_生成位号表> GenerateND(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
        {
            var ndList = new List<ND_生成位号表>();           

            return ndList;
        }
        private List<NN_生成位号表> GenerateNN(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
        {
            var nnList = new List<NN_生成位号表>();          

            return nnList;
        }
        private List<PA_生成位号表> GeneratePA(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
        {
            var paList = new List<PA_生成位号表>();          

            return paList;
        }
        private List<PD_生成位号表> GeneratePD(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
        {
            var pdList = new List<PD_生成位号表>();            

            return pdList;
        }
        private List<PN_生成位号表> GeneratePN(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
        {
            var pnList = new List<PN_生成位号表>();            

            return pnList;
        }
        private List<FB_生成位号表> GenerateFB(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
        {
            var fbList = new List<FB_生成位号表>();            

            return fbList;
        }
        private List<Control_生成位号表> GenerateControl(List<板卡清单_生成位号表> cardList)
        {
            if (cardList.Count == 0) return new List<Control_生成位号表>();
            return
            [
                new Control_生成位号表
                {
                    地址=cardList.FirstOrDefault().ControllerAddress,
                    型号="FCU712-S",//可选
                    冗余="1"
                   
                }
            ];
        }
        private List<IO_Link_Module_生成位号表> GenerateIOLinkModel(List<板卡清单_生成位号表> cardList)
        {
            if (cardList.Count == 0) return new List<IO_Link_Module_生成位号表>();
            return
            [
                new IO_Link_Module_生成位号表
                {
                  控制器地址=cardList.FirstOrDefault().ControllerAddress,
                  地址= "0",
                  型号="COM701-S",
                  冗余="0",
                }
            ];
        }
        private List<IO_Rack_生成位号表> GenerateIORack(List<板卡清单_生成位号表> cardList)
        {
            if (cardList.Count == 0) return new List<IO_Rack_生成位号表>();
            List<IO_Rack_生成位号表> result = new List<IO_Rack_生成位号表>();
            var groupbyRack = cardList.GroupBy(c => c.RackNumber);
            foreach (var rack in groupbyRack)
            {
                var slots = rack.ToList();
                if (slots.All(c => c.CardType == "备用")) continue;
                string address = (int.Parse(slots.FirstOrDefault()?.RackNumber.TrimStart('0') ?? "0") - 1).ToString();
                result.Add(new IO_Rack_生成位号表
                {
                    控制器地址 = slots.FirstOrDefault().ControllerAddress,
                    io连接模块地址 = "0",
                    地址 = address,
                    型号 = address == "0" || address == "1" ? "CN721" : "CN722"
                });
            }
            return result;
        }
        private List<IO_生成位号表> GenerateIO(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
        {
            if (cardList.Count == 0) return new List<IO_生成位号表>();
            List<IO_生成位号表> result = new List<IO_生成位号表>();
            var groupbyRack = cardList.GroupBy(c => c.RackNumber);

            foreach (var rack in groupbyRack)
            {
                var slots = rack.ToList();
                foreach (var slot in slots)
                {
                    if (slot.CardType == "备用" || slot.CardType == "空" || string.IsNullOrEmpty(slot.CardType)) continue;
                    int rackAddress = (int.Parse(slot.RackNumber.TrimStart('0')));
                    int slotAddress = (int.Parse(slot.SlotNumber.TrimStart('0')));
                    if (slot.CardType.StartsWith("AM") && (slotAddress-1) % 2 != 0) continue;

                    var tag = ioStations.FirstOrDefault(i => i.Rack == rackAddress && i.Slot == slotAddress);

                    result.Add(new IO_生成位号表
                    {
                        控制器地址 = slot.ControllerAddress,              // 映射到板卡清单的“控制器地址”
                        io连接模块地址 = "0",                            // 固定为0
                        机架地址 = (rackAddress - 1).ToString(),                         // 机架号减1
                        地址 = (slotAddress - 1).ToString(),                           // 插槽号减1
                        型号 = slot.CardType,                           // 映射到板卡清单的“板卡类型”
                        描述 = tag?.CardCabinetNumber ?? "",            // 映射到IO分站清单的“板卡柜内编号”
                        备注 = "",                                       // 留空
                        冗余 = slot.CardType.Contains("AM712") ? "1" : "0",                                     // 固定为0
                        采样周期 = "0",                                 // 固定为0
                        信号类型配置 = "0",                             // 固定为0
                        冷端补偿 = "0",                                 // 固定为0
                        抖动参数 = slot.CardType == "DI711-S" ? "1" : "0" // 型号为DI711-S时填1，其余填0
                    });
                }
            }

            return result;
        }
        private List<Channel_生成位号表> GenerateChannel(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
        {
            if (cardList.Count == 0) return new List<Channel_生成位号表>();

            List<Channel_生成位号表> result = new List<Channel_生成位号表>();
          
            return result;
        }

        // 获取模块类型
        private string GetModuleType(string cardType)
        {
            return cardType switch
            {
                "AI711-S" => "AI711-S 模拟信号输入模块（8路）",
                "AI711-H" => "AI711-H 模拟信号输入模块（8路，HART）",
                "PI711-S" => "PI711-S 脉冲信号输入模块（6路）",
                "AI731-S" => "AI731-S 热电阻信号输入模块（8路）",
                "AI722-S" => "AI722-S 热电偶信号输入模块（8路）",
                _ => "未知模块类型"
            };
        }
        private string GetSignalType(string cardType)
        {
            return cardType switch
            {
                "AI711-S" or "AI711-H" => "电流(4mA～20mA)",
                "PI711-S" => "0Hz～1000Hz",
                "AI731-S" => "Pt100（-200℃~850℃）",
                "AI722-S" => "K型(-200℃～1300℃)",
                _ => "Err"
            };
        }
        private string GetDataFormat(string cardType)
        {
            return cardType switch
            {
                "AI711-S" or "AI711-H" => "线性转换",
                "PI711-S" => "无转换",
                _ => "线性转换"
            };
        }
    }
}
