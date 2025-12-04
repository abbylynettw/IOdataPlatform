#pragma warning disable CA1822 // 将成员标记为 static

using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reactive.Linq;
using System.Reflection;

using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;

using LYSoft.Libs.ServiceInterfaces;

using SqlSugar;

namespace IODataPlatform.Views.SubPages.XT2;

public partial class UploadExcelDataViewModel(
    SqlSugarContext context, DepXT2ViewModel xt2,DepXT1ViewModel xt1,  GlobalModel model, IPickerService picker,
    ExcelService excel, INavigationService navigation,IMessageService msg, NavigationParameterService parameterService) : ObservableObject
{

    private ControlSystem controlSystem;

    [ObservableProperty]
    private string subNet = string.Empty;

    [ObservableProperty]
    private ObservableCollection<pdf_control_io>? ioData;

    public int IoDataCount { get => IoData?.Count ?? 0; } //IO数据数量

  

    [RelayCommand]
    private async Task ImportData(string param) {
        if (picker.OpenFiles("Excel 文件(*.xls; *.xlsx)|*.xls; *.xlsx") is not string[] files) { return; }
        await ImportIoData(files);
    }

    private List<T> GetExcelData<T>(string file) where T : new() {
        using var wb = excel.GetWorkbook(file);
        var result = new List<T>();
        foreach (var ws in wb.Worksheets) {
            var cells = ws.Cells;
            var data = cells.ExportDataTableAsString(0, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, true);
            var colNames = data.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            var propNames = typeof(T).GetProperties().Select(x => x.GetCustomAttribute<DisplayAttribute>()?.Name ?? x.Name).ToArray();
            if (colNames.Except(propNames).ToArray() is { Length: > 0 } cols) { throw new($"以下不是标准列名称：{string.Join("、", cols)}".Replace("\r", "").Replace("\n", "")); }
            result.AddRange(data.StringTableToIEnumerableByDiplay<T>());
        }
        return result;
    }

    private async Task ImportIoData(string[] files) {
        model.Status.Busy("正在导入输入输出表……");
        // 确保参数不为 null，并检查参数类型是否正确
        var value = parameterService.GetParameter<ControlSystem>("controlSystem");
        this.controlSystem = value;
        switch (controlSystem)
        {
            case ControlSystem.龙鳍:
                IoData = [.. await Task.Run(() => files.SelectMany(GetExcelData<xtes_pdf_control_io>))];
                break;
            case ControlSystem.中控:               
                break;
            case ControlSystem.龙核:
                break;
            case ControlSystem.一室:
                IoData = [.. await Task.Run(() => files.SelectMany(GetExcelData<xtys_pdf_control_io>))];
                break;
            default:
                break;
        }
        
        model.Status.Success("已导入输入输出表");
    }

    [RelayCommand]
    private async Task Confirm() {

        if (string.IsNullOrEmpty(SubNet)) { throw new("请输入子网"); }
        if (IoData is null) { throw new("请先导入输入输出表"); }

        var configsTask = context.Db.Queryable<config_card_type_judge>().ToListAsync();
        model.Status.Busy("正在生成数据……");
        switch (controlSystem)
        {
            case ControlSystem.龙鳍:
                _ = xt2.SubProject ?? throw new("开发人员注意");
                _ = xt2.AllData ?? throw new("开发人员注意");
                var newPointsTask = Task.Run(() => GetIoDataSystem2([.. IoData.OfType<xtes_pdf_control_io>().ToList()], SubNet));
                var oldPoints = xt2.AllData;
                var newPoints = await newPointsTask;
                //判断主键是否唯一，如果不唯一，给出提示
                var duplicates = newPoints
                .Where(item => oldPoints.Select(o => $"{o.SignalPositionNumber}_{o.ExtensionCode}").Contains($"{item.SignalPositionNumber}_{item.ExtensionCode}"))
                .Select(item => $"{item.SignalPositionNumber}_{item.ExtensionCode}");

                string result = duplicates.Any() ? $"以下主键重复，共{duplicates.Count()}个：\n{string.Join(";\n", duplicates)};\n" : "";
                if (result != "")
                {
                    await msg.AlertAsync(result); throw new("主键重复，无法导入");
                }
                xt2.AllData = [.. newPoints.OrderBy(n => n.CabinetNumber)];
                model.Status.Busy("正在保存……");
                await xt2.SaveAndUploadFileAsync();
                break;
         
            case ControlSystem.一室:
                _ = xt1.SubProject ?? throw new("开发人员注意");
                _ = xt1.AllData ?? throw new("开发人员注意");
                newPointsTask = Task.Run(() => GetIoDataSystem1([.. IoData.OfType<xtys_pdf_control_io>().ToList()], SubNet));
                oldPoints = xt2.AllData;
                newPoints = await newPointsTask;
                //判断主键是否唯一，如果不唯一，给出提示
                duplicates = newPoints
                .Where(item => oldPoints.Select(o => $"{o.SignalPositionNumber}_{o.ExtensionCode}").Contains($"{item.SignalPositionNumber}_{item.ExtensionCode}"))
                .Select(item => $"{item.SignalPositionNumber}_{item.ExtensionCode}");

                 result = duplicates.Any() ? $"以下主键重复，共{duplicates.Count()}个：\n{string.Join(";\n", duplicates)};\n" : "";
                if (result != "")
                {
                    await msg.AlertAsync(result); throw new("主键重复，无法导入");
                }
                xt1.AllData = [.. newPoints.OrderBy(n => n.CabinetNumber)];
                model.Status.Busy("正在保存……");
                await xt1.SaveAndUploadFileAsync();
                break;
            case ControlSystem.中控:              
            case ControlSystem.龙核:              
            default:
                break;
        }
        model.Status.Success("导入成功");
        navigation.GoBack();
    }


    private async Task<List<IoFullData>> GetIoDataSystem2(List<xtes_pdf_control_io> IOs, string subNet)
    {
        try
        {
            List<IoFullData> result = [];
            var config_card_type_judge = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
            var config_terminalboard_type_judge = await context.Db.Queryable<config_terminalboard_type_judge>().ToListAsync();
            var config_output_format_values = await context.Db.Queryable<config_output_format_values>().ToListAsync();
            var config_power_supply_method = await context.Db.Queryable<config_power_supply_method>().ToListAsync();


            var formularHelper = new FormularHelper();
            for (int i = 0; i < IOs.Count; i++)
            {
                var tag = IOs[i];
                var data = new IoFullData();

                _ = int.TryParse(tag.序号, out int temp);
                //硬赋值
                data.SerialNumber = temp; ; // 序号
                #region
                data.PIDDrawingNumber = ""; // P&ID图号
                data.SystemCode = ""; // 系统代码
                data.LDADDrawingNumber = ""; // LD/AD图号               
                data.Isolation = ""; // 隔离
                data.RemoteIO = ""; // 远程IO
                data.Trend = ""; // 趋势
                data.StatusWhenZero = ""; // 为0时状态
                data.StatusWhenOne = ""; // 为1时状态
                data.SignalEffectiveMode = ""; // 信号有效方式
                data.Destination = ""; // 信号终点                               
                data.RGRelatedScreen = ""; // RG-关联画面
                data.High4LimitAlarmValue = 0; // 高4限报警限值
                data.High4LimitAlarmLevel = ""; // 高4限报警等级
                data.High4LimitAlarmTag = ""; // 高4限报警标签
                data.High4AlarmDescription = ""; // 高4报警描述
                data.High3LimitAlarmValue = 0; // 高3限报警限值
                data.High3LimitAlarmLevel = ""; // 高3限报警等级
                data.High3LimitAlarmTag = ""; // 高3限报警标签
                data.High3AlarmDescription = ""; // 高3报警描述
                data.High2LimitAlarmValue = 0; // 高2限报警限值
                data.High2LimitAlarmLevel = ""; // 高2限报警等级
                data.High2LimitAlarmTag = ""; // 高2限报警标签
                data.High2AlarmDescription = ""; // 高2报警描述
                data.High1LimitAlarmValue = 0; // 高1限报警限值
                data.High1LimitAlarmLevel = ""; // 高1限报警等级
                data.High1LimitAlarmTag = ""; // 高1限报警标签
                data.High1AlarmDescription = ""; // 高1报警描述
                data.Low1LimitAlarmValue = 0; // 低1限报警限值
                data.Low1LimitAlarmLevel = ""; // 低1限报警等级
                data.Low1LimitAlarmTag = ""; // 低1限报警标签
                data.Low1AlarmDescription = ""; // 低1报警描述
                data.Low2LimitAlarmValue = 0; // 低2限报警限值
                data.Low2LimitAlarmLevel = ""; // 低2限报警等级
                data.Low2LimitAlarmTag = ""; // 低2限报警标签
                data.Low2AlarmDescription = ""; // 低2报警描述
                data.Low3LimitAlarmValue = 0; // 低3限报警限值
                data.Low3LimitAlarmLevel = ""; // 低3限报警等级
                data.Low3LimitAlarmTag = ""; // 低3限报警标签
                data.Low3AlarmDescription = ""; // 低3报警描述
                data.Low4LimitAlarmValue = 0; // 低4限报警限值
                data.Low4LimitAlarmLevel = ""; // 低4限报警等级
                data.Low4LimitAlarmTag = ""; // 低4限报警标签
                data.Low4AlarmDescription = ""; // 低4报警描述
                data.AlarmLevel = ""; // 报警等级
                data.SwitchQuantityAlarmTag = ""; // 开关量报警标签
                data.AlarmDescription = ""; // 报警描述
                data.AlarmAttribute = ""; // 报警属性
                data.StationNumber = ""; // SN-站号         
                data.KUPointName = ""; // KU点名
                #endregion
                data.SubNet = subNet; // SUBNET-子网                
                data.ModificationDate = DateTime.Now; // 修改日期
                data.LocalBoxNumber = tag.就地箱号; // 就地箱号           

                //设计输入读取
                data.SignalPositionNumber = xt2.GetSignalPositionNumber(tag.信号位号); // 信号位号
                data.ExtensionCode = tag.扩展码; // 扩展码
                data.TagName = xt2.GetTagName(data.SignalPositionNumber, data.ExtensionCode); //标签名称
                data.Description = tag.信号功能;// 描述           
                data.CabinetNumber = tag.机柜号.Replace("-",""); // 机柜号
                data.SafetyClassificationGroup = tag.安全分级; // 安全分级/分组
                data.SeismicCategory = tag.抗震类别; // 抗震类别
                data.IoType = tag.IO类型; // I/O类型
                data.ElectricalCharacteristics = tag.信号特性; // 电气特性           
                data.PowerType = tag.供电类型; // 供电类型
                data.SensorType = tag.传感器类型; // 传感器类型
                data.EngineeringUnit = tag.单位; ; // 工程单位
                data.VoltageLevel = tag.电压等级; // 电压等级
                data.InstrumentFunctionNumber = tag.仪表功能号; // 仪表功能号
                data.FFSlaveModuleModel = tag.FF从站模块型号;
                data.RangeLowerLimit = tag.最小测量范围; ; // 量程范围下限
                data.RangeUpperLimit = tag.最大测量范围; ; // 量程范围上限
                data.OFDisplayFormat = xt2.GetOfDisplayFormat(config_output_format_values, data); // OF-显示格式 从数据库读
                data.CardType = xt2.UseFormula ? xt2.GetIoCardTypeFormula(data) : xt2.GetIoCardType(config_card_type_judge, data);// 板卡类型从数据库读
                data.PowerSupplyMethod = xt2.UseFormula ? xt2.GetPowerSupplyMethodFormula(data) : xt2.GetPowerSupplyMethod(config_power_supply_method, data); // 供电方式从数据库读
                data.TerminalBoardModel = xt2.UseFormula ? xt2.GetTerminalBoxTypeFormula(data.CardType,data.PowerSupplyMethod,data.ElectricalCharacteristics) : xt2.GetTerminalBoxType(config_terminalboard_type_judge, data);//端子板型号从数据库读            
                data.PVPoint = ""; // PV点
                data.Version = tag.版本;
                data.Remarks = tag.备注; // 备注
                data.ModificationDate = DateTime.Now;
                result.Add(data);
            }

            //遍历result，为每个机柜赋值站号
            var startAddress = 2;           
            result.GroupBy(r => r.CabinetNumber)
                  .ToList()
                  .ForEach(cabinetGroup =>
                  {
                      // 为每个cabinet分配一个站号
                      var stationNumber = (startAddress++).ToString();
                      // 将相同cabinet的所有IO分配相同的站号
                      cabinetGroup.ToList().ForEach(io => io.StationNumber = stationNumber);
                  });


            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message.ToString());
        }
        
    }


    private async Task<List<IoFullData>> GetIoDataSystem1(List<xtys_pdf_control_io> IOs, string subNet)
    {
        try
        {
            List<IoFullData> result = [];
            var config_card_type_judge = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
            var config_terminalboard_type_judge = await context.Db.Queryable<config_terminalboard_type_judge>().ToListAsync();
            var config_output_format_values = await context.Db.Queryable<config_output_format_values>().ToListAsync();
            var config_power_supply_method = await context.Db.Queryable<config_power_supply_method>().ToListAsync();


            var formularHelper = new FormularHelper();
            for (int i = 0; i < IOs.Count; i++)
            {
                var tag = IOs[i];
                var data = new IoFullData();

                _ = int.TryParse(tag.序号, out int temp);
                //硬赋值
                data.SerialNumber = temp; ; // 序号
                #region
                data.PIDDrawingNumber = ""; // P&ID图号
                data.SystemCode = ""; // 系统代码
                data.LDADDrawingNumber = ""; // LD/AD图号             
                data.Isolation = ""; // 隔离
                data.RemoteIO = ""; // 远程IO
                data.Trend = ""; // 趋势
                data.StatusWhenZero = ""; // 为0时状态
                data.StatusWhenOne = ""; // 为1时状态
                data.SignalEffectiveMode = ""; // 信号有效方式
                data.Destination = ""; // 信号终点                               
                data.RGRelatedScreen = ""; // RG-关联画面
                data.High4LimitAlarmValue = 0; // 高4限报警限值
                data.High4LimitAlarmLevel = ""; // 高4限报警等级
                data.High4LimitAlarmTag = ""; // 高4限报警标签
                data.High4AlarmDescription = ""; // 高4报警描述
                data.High3LimitAlarmValue = 0; // 高3限报警限值
                data.High3LimitAlarmLevel = ""; // 高3限报警等级
                data.High3LimitAlarmTag = ""; // 高3限报警标签
                data.High3AlarmDescription = ""; // 高3报警描述
                data.High2LimitAlarmValue = 0; // 高2限报警限值
                data.High2LimitAlarmLevel = ""; // 高2限报警等级
                data.High2LimitAlarmTag = ""; // 高2限报警标签
                data.High2AlarmDescription = ""; // 高2报警描述
                data.High1LimitAlarmValue = 0; // 高1限报警限值
                data.High1LimitAlarmLevel = ""; // 高1限报警等级
                data.High1LimitAlarmTag = ""; // 高1限报警标签
                data.High1AlarmDescription = ""; // 高1报警描述
                data.Low1LimitAlarmValue = 0; // 低1限报警限值
                data.Low1LimitAlarmLevel = ""; // 低1限报警等级
                data.Low1LimitAlarmTag = ""; // 低1限报警标签
                data.Low1AlarmDescription = ""; // 低1报警描述
                data.Low2LimitAlarmValue = 0; // 低2限报警限值
                data.Low2LimitAlarmLevel = ""; // 低2限报警等级
                data.Low2LimitAlarmTag = ""; // 低2限报警标签
                data.Low2AlarmDescription = ""; // 低2报警描述
                data.Low3LimitAlarmValue = 0; // 低3限报警限值
                data.Low3LimitAlarmLevel = ""; // 低3限报警等级
                data.Low3LimitAlarmTag = ""; // 低3限报警标签
                data.Low3AlarmDescription = ""; // 低3报警描述
                data.Low4LimitAlarmValue = 0; // 低4限报警限值
                data.Low4LimitAlarmLevel = ""; // 低4限报警等级
                data.Low4LimitAlarmTag = ""; // 低4限报警标签
                data.Low4AlarmDescription = ""; // 低4报警描述
                data.AlarmLevel = ""; // 报警等级
                data.SwitchQuantityAlarmTag = ""; // 开关量报警标签
                data.AlarmDescription = ""; // 报警描述
                data.AlarmAttribute = ""; // 报警属性
                data.StationNumber = ""; // SN-站号         
                data.KUPointName = ""; // KU点名
                #endregion
                data.SubNet = subNet; // SUBNET-子网                
                data.ModificationDate = DateTime.Now; // 修改日期
                //data.LocalBoxNumber = tag.就地箱号; // 就地箱号           

                //设计输入读取
                data.SignalPositionNumber = xt2.GetSignalPositionNumber(tag.信号位号); // 信号位号
                data.ExtensionCode = tag.扩展码; // 扩展码
                data.TagName = xt2.GetTagName(data.SignalPositionNumber, data.ExtensionCode); //标签名称
                data.Description = tag.信号说明;// 描述           
                //data.CabinetNumber = tag.机柜号.Replace("-", ""); // 机柜号
                data.SafetyClassificationGroup = tag.安全分级分组; // 安全分级/分组
                data.SeismicCategory = tag.抗震类别; // 抗震类别
                data.IoType = tag.IO类型; // I/O类型
                data.ElectricalCharacteristics = tag.信号特性; // 电气特性           
                //data.PowerType = tag.机柜供电; // 供电类型
                //data.SensorType = tag.传感器类型; // 传感器类型
                data.EngineeringUnit = tag.测量单位; ; // 工程单位
                data.RangeLowerLimit = tag.量程下限; ; // 量程范围下限
                data.RangeUpperLimit = tag.量程上限; ; // 量程范围上限
                data.OFDisplayFormat = xt2.GetOfDisplayFormat(config_output_format_values, data); // OF-显示格式 从数据库读
                data.CardType = xt2.UseFormula ? xt2.GetIoCardTypeFormula(data) : xt2.GetIoCardType(config_card_type_judge, data);// 板卡类型从数据库读
                data.PowerSupplyMethod = xt2.UseFormula ? xt2.GetPowerSupplyMethodFormula(data) : xt2.GetPowerSupplyMethod(config_power_supply_method, data); // 供电方式从数据库读
                data.TerminalBoardModel = xt2.UseFormula ? xt2.GetTerminalBoxTypeFormula(data.CardType, data.PowerSupplyMethod, data.ElectricalCharacteristics) : xt2.GetTerminalBoxType(config_terminalboard_type_judge, data);//端子板型号从数据库读            
                data.PVPoint = ""; // PV点
                data.Version = tag.版本;
                data.Remarks = tag.备注; // 备注
                data.ModificationDate = DateTime.Now;
                result.Add(data);
            }

            //遍历result，为每个机柜赋值站号
            var startAddress = 2;
            result.GroupBy(r => r.CabinetNumber)
                  .ToList()
                  .ForEach(cabinetGroup =>
                  {
                      // 为每个cabinet分配一个站号
                      var stationNumber = (startAddress++).ToString();
                      // 将相同cabinet的所有IO分配相同的站号
                      cabinetGroup.ToList().ForEach(io => io.StationNumber = stationNumber);
                  });


            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message.ToString());
        }

    }

    
}
