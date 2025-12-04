using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using System.Runtime.InteropServices;
using Aspose.Words.Lists;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Data;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows.Documents;
using IODataPlatform.Models;

namespace IODataPlatform.Utilities;

/// <summary>
/// 数据转换器工具类
/// 提供不同数据模型之间的转换功能，支持多种控制系统和数据格式
/// 主要用于IO数据的标准化处理和不同系统间的数据映射转换
/// 支持龙鲭、中控、龙核、一室、安全级等多种控制系统的数据转换
/// </summary>
public  static partial class DataConverter {

    /// <summary>
    /// 将IoFullData列表转换为指定控制系统的自定义DataTable
    /// 根据不同控制系统的字段映射配置，将标准化的IO数据转换为对应系统的数据格式
    /// 支持龙鲭、中控、龙核、一室、安全级模拟系统等多种控制系统
    /// </summary>
    /// <param name="data">要转换的IoFullData列表</param>
    /// <param name="db">SqlSugar数据库上下文，用于查询字段映射配置</param>
    /// <param name="controlSystem">目标控制系统类型</param>
    /// <returns>返回转换后的DataTable，包含对应控制系统的字段名和数据</returns>
    public static DataTable ToCustomDataTable(this List<IoFullData> data, SqlSugarClient db, ControlSystem controlSystem)
    {
        // 根据控制系统选择对应的字段
        var mappingDict = db.Queryable<config_controlSystem_mapping>()
            .Where(it => it.StdField != null).ToList()
            .ToDictionary(
                it => it.StdField,
                it => controlSystem switch
                {
                    ControlSystem.龙鳍 => it.LqOld,
                    ControlSystem.中控 => it.ZkOld,
                    ControlSystem.龙核 => it.LhOld,
                    ControlSystem.一室 => it.Xt1Old,
                    ControlSystem.安全级模拟系统 => it.AQJMNOld,
                    _ => null
                });

        DataTable dataTable = new DataTable();

        // 创建列
        foreach (var prop in typeof(IoFullData).GetProperties())
        {
            var stdField = prop.GetCustomAttribute<DisplayAttribute>()?.Name ?? prop.Name;
            if (mappingDict.TryGetValue(stdField, out var mappedColumn) && !string.IsNullOrEmpty(mappedColumn))
            {
                dataTable.Columns.Add(mappedColumn, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
        }

        // 填充数据
        foreach (var item in data)
        {
            var row = dataTable.NewRow();
            foreach (var prop in typeof(IoFullData).GetProperties())
            {
                var stdField = prop.GetCustomAttribute<DisplayAttribute>()?.Name ?? prop.Name;
                if (mappingDict.TryGetValue(stdField, out var mappedColumn) && !string.IsNullOrEmpty(mappedColumn))
                {
                    row[mappedColumn] = prop.GetValue(item) ?? DBNull.Value;
                }
            }
            dataTable.Rows.Add(row);
        }

        return dataTable;
    }

    /// <summary>
    /// 将IoFullData对象转换为IoData对象
    /// 实现不同IO数据模型之间的映射转换，保持数据完整性
    /// 将完整的IO数据模型转换为简化的IO数据模型，适用于不同业务场景
    /// </summary>
    /// <param name="data">要转换的IoFullData源对象</param>
    /// <returns>返回转换后的IoData对象</returns>
    public static IoData ToIoData(this IoFullData data) {
        return new() {
            Id = data.SerialNumber,
            TagName = data.TagName,
          
            OldVarName = data.SignalPositionNumber, // Xt2IoSubstation 中没有对应字段
            OldExtCode = data.ExtensionCode,
           
            Designation = data.Description,
            Unit = "", // Xt2IoSubstation 中没有对应字段
            Lot = "", // Xt2IoSubstation 中没有对应字段
            SubItem = "", // Xt2IoSubstation 中没有对应字段
            FcuName = "", // Xt2IoSubstation 中没有对应字段
            Room = "", // Xt2IoSubstation 中没有对应字段
            Cabinet = data.CabinetNumber,
            IoType = data.IoType,
            Type = data.CardType,
            IoCardNumber = data.CardNumber,
            Pin = data.Channel,
            TAPartNumber = data.TerminalBoardModel,
            Component = data.TerminalBoardNumber,
            SignalStart = data.SignalPositionNumber,
            Destination = data.Destination,
            PowerType = data.PowerType,
            LoopVoltage = "", // Xt2IoSubstation 中没有对应字段
            ContactCapacity = "", // Xt2IoSubstation 中没有对应字段
            SignalSpec = "", // Xt2IoSubstation 中没有对应字段
            Isolation = data.Isolation,
            Ees = "", // Xt2IoSubstation 中没有对应字段
            SensorType = data.SensorType,
            TypicalLoopDrawing = "", // Xt2IoSubstation 中没有对应字段
            Sa = "", // Xt2IoSubstation 中没有对应字段
            SoeTra = "", // Xt2IoSubstation 中没有对应字段
            IntExt = "", // Xt2IoSubstation 中没有对应字段
            SafetyClassDivision = data.SafetyClassificationGroup,
            FunctionalClass = "", // Xt2IoSubstation 中没有对应字段
            Seismic = data.SeismicCategory,
            System = data.SystemCode,
            List = "", // Xt2IoSubstation 中没有对应字段
            DiagramNumber = data.PIDDrawingNumber,
            TransNo = "", // Xt2IoSubstation 中没有对应字段
            TerminalBlock = "", // Xt2IoSubstation 中没有对应字段
            Remarks = data.Remarks,
            SerialNumber = data.SerialNumber.ToString(),
            PIDDrawingNumber = data.PIDDrawingNumber,
            LDADDrawingNumber = data.LDADDrawingNumber,
            CardNumber = data.CardNumber,
            Cage = data.Cage.ToString(),
            Slot = data.Slot,
            CardAddress = data.CardAddress,
            FfdpTerminalchannel = data.FFTerminalChannel,
            ElectricalCharacteristics = data.ElectricalCharacteristics,
            PowerSupplyMethod = data.PowerSupplyMethod,
            EngineeringUnit = data.EngineeringUnit,
            RangeLowerLimit = data.RangeLowerLimit,
            RangeUpperLimit = data.RangeUpperLimit,
            RemoteIO = data.RemoteIO,
            Trend = data.Trend,
            StatusWhenZero = data.StatusWhenZero,
            StatusWhenOne = data.StatusWhenOne,
            SignalEffectiveMode = data.SignalEffectiveMode,
            LocalBoxNumber = data.LocalBoxNumber,
            Version = data.Version,
            ModificationDate = data.ModificationDate,
            RgRelatedscreen = data.RGRelatedScreen,
            High4LimitAlarmValue = data.High4LimitAlarmValue,
            High4LimitAlarmLevel = data.High4LimitAlarmLevel,
            High4LimitAlarmTag = data.High4LimitAlarmTag,
            High4AlarmDescription = data.High4AlarmDescription,
            High3LimitAlarmValue = data.High3LimitAlarmValue,
            High3LimitAlarmLevel = data.High3LimitAlarmLevel,
            High3LimitAlarmTag = data.High3LimitAlarmTag,
            High3AlarmDescription = data.High3AlarmDescription,
            High2LimitAlarmValue = data.High2LimitAlarmValue,
            High2LimitAlarmLevel = data.High2LimitAlarmLevel,
            High2LimitAlarmTag = data.High2LimitAlarmTag,
            High2AlarmDescription = data.High2AlarmDescription,
            High1LimitAlarmValue = data.High1LimitAlarmValue,
            High1LimitAlarmLevel = data.High1LimitAlarmLevel,
            High1LimitAlarmTag = data.High1LimitAlarmTag,
            High1AlarmDescription = data.High1AlarmDescription,
            Low1LimitAlarmValue = data.Low1LimitAlarmValue,
            Low1LimitAlarmLevel = data.Low1LimitAlarmLevel,
            Low1LimitAlarmTag = data.Low1LimitAlarmTag,
            Low1AlarmDescription = data.Low1AlarmDescription,
            Low2LimitAlarmValue = data.Low2LimitAlarmValue,
            Low2LimitAlarmLevel = data.Low2LimitAlarmLevel,
            Low2LimitAlarmTag = data.Low2LimitAlarmTag,
            Low2AlarmDescription = data.Low2AlarmDescription,
            Low3LimitAlarmValue = data.Low3LimitAlarmValue,
            Low3LimitAlarmLevel = data.Low3LimitAlarmLevel,
            Low3LimitAlarmTag = data.Low3LimitAlarmTag,
            Low3AlarmDescription = data.Low3AlarmDescription,
            Low4LimitAlarmValue = data.Low4LimitAlarmValue,
            Low4LimitAlarmLevel = data.Low4LimitAlarmLevel,
            Low4LimitAlarmTag = data.Low4LimitAlarmTag,
            Low4AlarmDescription = data.Low4AlarmDescription,
            AlarmLevel = data.AlarmLevel,
            SwitchQuantityAlarmTag = data.SwitchQuantityAlarmTag,
            AlarmDescription = data.AlarmDescription,
            AlarmAttribute = data.AlarmAttribute,
            SnStationnumber = data.StationNumber,
            SubnetSubnet = data.SubNet,
            KUPointName = data.KUPointName,
            OfDisplayformat = data.OFDisplayFormat,
            PVPoint = data.PVPoint,
            Connection1 = data.SignalPlus,
            SigPotential1 = "", // Xt2IoSubstation 中没有对应字段
            Connection2 = data.SignalMinus,
            SigPotential2 = "", // Xt2IoSubstation 中没有对应字段
            Connection3 = data.RTDCompensationC, // Xt2IoSubstation 中没有对应字段
            SigPotential3 = "", // Xt2IoSubstation 中没有对应字段
            Connection4 = data.RTDCompensationE, // Xt2IoSubstation 中没有对应字段
            SigPotential4 = "", // Xt2IoSubstation 中没有对应字段
            BackCardType = "", // Xt2IoSubstation 中没有对应字段
            ScaleType = "", // Xt2IoSubstation 中没有对应字段
            DefaultNumber = "", // Xt2IoSubstation 中没有对应字段
            AllocateInfo = "", // Xt2IoSubstation 中没有对应字段                          
        };

    }
    public static IoData ToIoData(this AQJIoData data)
    {
        return new IoData()
        {
            TagName = data.信号名称,
            OldVarName = data.原变量名,
            Designation = data.信号说明,
            Room = data.房间号,
            Cabinet = data.机柜号,
            IoType = data.IO类型,
            Type = data.前卡件类型,
            IoCardNumber = data.后卡件类型,
            Pin = data.通道号,
            TAPartNumber = data.端子板类型,
            Component = data.端子板编号,
            PowerType = data.电压等级,
            SensorType = data.传感器类型,
            Connection1 = data.端子号1,
            Connection2 = data.端子号2,
            TypicalLoopDrawing = data.典型回路图,
            IntExt = data.内部外部,
            SafetyClassDivision = data.安全分级,
            Remarks = data.注释,
            SerialNumber = data.序号,
            Cage = data.机箱号.ToString(),
            Slot = data.槽位号,
            Version = data.版本,
            System=data.系统名,
            ModificationDate = null, // 没有对应字段            
            MajorDest = data.去向专业
        };
    }


    public static int ToInt(this string number)
    {
        _ = int.TryParse(number, out int pin);
        return pin;
    }
    public static TerminationData ToTerminationData(this IoData data) {
        return new()
        {
            IOPointName = data.TagName,
         
            OriginalVariableName = data.OldVarName,
            OriginalExtensionCode = data.OldExtCode,
            
            SignalDescription = data.Designation,
            UnitNumber = data.Unit,
            LOT = data.Lot,
            SubItem = data.SubItem,
            ControllerStationName = data.FcuName,
            RoomNumber = data.Room,
            CabinetNumber = data.Cabinet,
            IOType = data.IoType,
            IOCardModel = data.Type,
            IOCardNumber = data.IoCardNumber,
            ChannelNumber = data.Pin,
            TerminalBoardModel = data.TAPartNumber,
            TerminalBoardNumber = data.Component,
            SignalStart = data.SignalStart,
            SignalEnd = data.Destination,
            PowerType_Supplier_ = data.PowerType,
            CircuitVoltage = data.LoopVoltage,
            ContactCapacity = data.ContactCapacity,
            SignalCharacteristics = data.SignalSpec,
            Isolation = data.Isolation,
            EESPowerSupply = data.Ees,
            SensorType = data.SensorType,
            TypicalCircuitDiagram = data.TypicalLoopDrawing,
            SAPowerSupply = data.Sa,
            AccidentRecall_TransientRecord = data.SoeTra,
            Internal_ExternalPoints = data.IntExt,
            SafetyClassification_Grouping = data.SafetyClassDivision,
            FunctionalClassification = data.FunctionalClass,
            SeismicCategory = data.Seismic,
            SystemName = data.System,
            IOListCategory = data.List,
            DrawingNumber = data.DiagramNumber,
            TransferOrderNumber = data.TransNo,
            TerminalStripNumber = data.TerminalBlock,
            ConnectionPoint1 = data.Connection1,
            SignalDescription1 = data.SigPotential1,
            ConnectionPoint2 = data.Connection2,
            SignalDescription2 = data.SigPotential2,
            ConnectionPoint3 = data.Connection3,
            SignalDescription3 = data.SigPotential3,
            ConnectionPoint4 = data.Connection4,
            SignalDescription4 = data.SigPotential4,                               
            MajorDest = data.MajorDest
        };
    }

    public static xtes_duanjie toduanjieData(this IoFullData data)
    {
        return new xtes_duanjie()
        {
            I_O点名 = data.TagName,
            信号说明 = data.Description,
            房间号 = "",
            机柜名_起点 = data.CabinetNumber,
            设备编号_起点 = data.TerminalBoardNumber,
            信号特性 = data.ElectricalCharacteristics,
            接线点1 = data.SignalPlus,
            接线点说明1 = "+",
            接线点2 = data.SignalMinus,
            接线点说明2 = "-",
            接线点3 = data.RTDCompensationC,
            接线点说明3 = "",
            接线点4 = data.RTDCompensationE,
            接线点说明4 = "",
            供电方式 = data.PowerSupplyMethod,
            房间号_终点 = "",
            设备编号_终点 = data.LocalBoxNumber,
            电缆编码 = "",
            电缆型号及规格 = "",
            电缆长度 = "",
            供货方 = "",
            版本 = ""
        };
    }
    public static CableData MatchCableData(TerminationData t1, TerminationData t2) {
      
        CableData cableData = new CableData
        {
            起点信号位号 = t1.IOPointName,
            起点房间号 = t1.RoomNumber,
            起点盘柜名称 = t1.CabinetNumber,
            起点设备名称 = t1.TerminalBoardModel,
            起点接线点1 = t1.ConnectionPoint1,
            起点接线点2 = t1.ConnectionPoint2,
            起点接线点3 = t1.ConnectionPoint3,
            起点接线点4 = t1.ConnectionPoint4,
            起点IO类型=t1.IOType,
            起点安全分级分组=t1.SafetyClassification_Grouping,
            起点系统号=t1.SystemName,
            起点专业=t1.MajorDest,

            终点信号位号 = t2.IOPointName,
            终点房间号 = t2.RoomNumber,
            终点盘柜名称 = t2.CabinetNumber,
            终点设备名称 = t2.TerminalBoardModel,
            终点接线点1 = t2.ConnectionPoint1,
            终点接线点2 = t2.ConnectionPoint2,
            终点接线点3 = t2.ConnectionPoint3,
            终点接线点4 = t2.ConnectionPoint4,
            终点IO类型 = t2.IOType,
            终点安全分级分组 = t2.SafetyClassification_Grouping,
            终点系统号 = t2.SystemName,
            终点专业=t2.MajorDest
        };

        return cableData;
    }
    public static async Task<MatchCableDataResult> MatchCableData(IEnumerable<TerminationData> duanjieList1, IEnumerable<TerminationData> duanjieList2, string major1, string major2)
    {        
        MatchCableDataResult result = new MatchCableDataResult();
        result.SuccessList = new List<CableData>();
        var l1 = duanjieList1.Where(d => d.MajorDest == major2).ToList();
        var l2 = duanjieList2.Where(d => d.MajorDest == major1).ToList();
        foreach (var d1 in l1)
        {
            foreach (var d2 in l2)
            {
                if (d1.IOPointName.Contains(d2.IOPointName) || d2.IOPointName.Contains(d1.IOPointName))
                {
                    result.SuccessList.Add(new CableData
                    {
                        起点IO类型 = d1.IOType,

                        起点信号位号 = d1.IOPointName,
                        起点房间号 = d1.RoomNumber,
                        起点盘柜名称 = d1.CabinetNumber,
                        起点设备名称 = d1.TerminalBoardNumber,
                        起点安全分级分组 = d1.SafetyClassification_Grouping,
                        起点系统号 = d1.SystemName,
                        起点接线点1 = d1.ConnectionPoint1,
                        起点接线点2 = d1.ConnectionPoint2,
                        起点接线点3 = d1.ConnectionPoint3,
                        起点接线点4 = d1.ConnectionPoint4,
                        起点专业 = major1,

                        供货方 = "CNCS",
                        终点IO类型 = d2.IOType,
                        终点信号位号 = d2.IOPointName,
                        终点房间号 = d2.RoomNumber,
                        终点盘柜名称 = d2.CabinetNumber,
                        终点设备名称 = d2.TerminalBoardNumber,
                        终点安全分级分组 = d2.SafetyClassification_Grouping,
                        终点系统号 = d2.SystemName,
                        终点接线点1 = d2.ConnectionPoint1,
                        终点接线点2 = d2.ConnectionPoint2,
                        终点接线点3 = d2.ConnectionPoint3,
                        终点接线点4 = d2.ConnectionPoint4,
                        终点专业 = major2,
                    });
                    d1.IsMatched = true;
                    d2.IsMatched = true;
                }
            }
        }
        result.FailList1 = new List<MatchCableDataFail>();
        result.FailList2 = new List<MatchCableDataFail>();
        foreach (var item in l1.Where(l => !l.IsMatched))
        {
            MatchCableDataFail match = new MatchCableDataFail { Data = item, Reason = "找不到IO点" };
            result.FailList1.Add(match);
        }
        foreach (var item in l2.Where(l => !l.IsMatched))
        {
            MatchCableDataFail match = new MatchCableDataFail { Data = item, Reason = "找不到IO点" };
            result.FailList2.Add(match);
        }
        return result;
    }

    public static async Task<Tuple<TerminationData, TerminationData>> UnMatchCableData(CableData cableData)
    {
        // 创建起点的 TerminationData
        TerminationData t1 = new TerminationData
        {
            IOPointName = cableData.起点信号位号,
            RoomNumber = cableData.起点房间号,
            CabinetNumber = cableData.起点盘柜名称,
            TerminalBoardModel = cableData.起点设备名称,
            ConnectionPoint1 = cableData.起点接线点1,
            ConnectionPoint2 = cableData.起点接线点2,
            ConnectionPoint3 = cableData.起点接线点3,
            ConnectionPoint4 = cableData.起点接线点4,
            IOType = cableData.起点IO类型,
            SafetyClassification_Grouping = cableData.起点安全分级分组,
            SystemName = cableData.起点系统号,
            MajorDest = cableData.起点专业,
        };

        // 创建终点的 TerminationData
        TerminationData t2 = new TerminationData
        {
            IOPointName = cableData.终点信号位号,
            RoomNumber = cableData.终点房间号,
            CabinetNumber = cableData.终点盘柜名称,
            TerminalBoardModel = cableData.终点设备名称,
            ConnectionPoint1 = cableData.终点接线点1,
            ConnectionPoint2 = cableData.终点接线点2,
            ConnectionPoint3 = cableData.终点接线点3,
            ConnectionPoint4 = cableData.终点接线点4,
            IOType = cableData.终点IO类型,
            SafetyClassification_Grouping = cableData.终点安全分级分组,
            SystemName = cableData.终点系统号,
            MajorDest = cableData.终点专业,
        };

        // 返回一个包含两个 TerminationData 对象的元组
        return Tuple.Create(t1, t2);
    }





    public static async Task<List<CableData>> NumberMatchCable(List<CableData> excelData, SqlSugarContext context)
    { 
        string constant = "";
        int serialNumber = 1;
        var cable_CategoryAndColors = await context.Db.Queryable<config_cable_categoryAndColor>().ToListAsync();
        var cableFunctionDtos = await context.Db.Queryable<config_cable_function>().ToListAsync();
        var cable_Specs = await context.Db.Queryable<config_cable_spec>().ToListAsync();
        var cable_SystemNumbers = await context.Db.Queryable<config_cable_systemNumber>().ToListAsync();
        var startNumbers = await context.Db.Queryable<config_cable_startNumber>().ToListAsync();
        //计算每根电缆的信息
        foreach (var cd in excelData)
        {
            //IO类型
            var match = cableFunctionDtos.FirstOrDefault(c =>
                    c.FirstIOType == cd.起点IO类型 && c.SecondIOType == cd.终点IO类型);
            cd.IO类型 = match == null ? cd.起点IO类型 : match.CableIOType;
            if (cd.起点系统号 == null || cd.终点系统号 == null) continue;
            cd.SystemNo = cable_SystemNumbers.FirstOrDefault(t =>
                            cd.起点系统号.Contains(t.StartSystem) && cd.终点系统号.Contains(t.EndSystem)) ?? new();

        }
        var cables = new List<每根电缆>();
        var ioTypeGroups = excelData.GroupBy(record => record.IO类型[..1]);
        foreach (var ioTypeGroup in ioTypeGroups)
        {
            string cableType = ioTypeGroup.Key == "D" ? "控制电缆" : "测量电缆";
            var equipmentGroups = ioTypeGroup.GroupBy(record => record.起点设备名称);
            foreach (var equipmentGroup in equipmentGroups)
            {
                var l1 = equipmentGroup.ToList();
                var cabinetGroups = equipmentGroup.GroupBy(record => record.终点盘柜名称);
                foreach (var cabinetGroup in cabinetGroups)
                {
                    var l2 = cabinetGroup.ToList();
                    var signalGroups = cabinetGroup
                        .GroupBy(record => record.起点信号位号.Split('_').FirstOrDefault())
                        .OrderByDescending(group => group.Count());

                    foreach (var signalGroup in signalGroups)
                    {
                        var signals = signalGroup.Where(s => s.起点系统号 != null && s.终点系统号 != null).ToList();//这一组信号列表
                        int remainingPoints = signals.Sum(signal => CountConnectionPoints(signal));//总点数

                        while (remainingPoints > 0)
                        {
                            var matchingCableSpec = cable_Specs
                                .Where(c => c.CableType == cableType)
                                .OrderBy(c => c.ConnectCount)
                                .FirstOrDefault(c => c.ConnectCount >= remainingPoints);

                            if (matchingCableSpec == null)
                            {
                                //如果没有匹配的电缆，就选择最大的
                                matchingCableSpec = cable_Specs.Where(c => c.CableType == cableType).OrderByDescending(c => c.ConnectCount).FirstOrDefault();
                            }

                            var cable = new 每根电缆
                            {
                                控制电缆 = (电缆类型)Enum.Parse(typeof(电缆类型), cableType),
                                Signals = new List<CableData>()
                            };

                            int allocatedPoints = 0;//已分配的点
                            int coreNumber = 1; // 芯数从1开始编号

                            foreach (var signal in signals.ToList())
                            {
                                int signalConnectionPoints = CountConnectionPoints(signal);

                                if (allocatedPoints + signalConnectionPoints > matchingCableSpec.ConnectCount)
                                {                                   
                                    break;
                                }
                                cable.Signals.Add(signal);
                                allocatedPoints += signalConnectionPoints;                              
                                AssignSignalAttributes(signal, serialNumber++, cable_CategoryAndColors, matchingCableSpec);
                                AssignCoreNumbers(signal, cableType, ref coreNumber);                               
                                signals.Remove(signal);
                            }
                            GenerateCableNumber(cable, cableType, constant, cable_SystemNumbers, startNumbers);

                            cables.Add(cable);
                            remainingPoints -= allocatedPoints;
                        }
                    }
                }
            }
        }
        List<CableData> cableDatas = new List<CableData>();
        foreach(var item in cables)
        {
            cableDatas.AddRange(item.Signals);
        }
        return cableDatas;
    }

    public static async Task<List<CableData>> NumberMatchCable1(List<CableData> excelData, SqlSugarContext context)
    {
        string constant = "";
        int serialNumber = 1;
        var cable_CategoryAndColors = await context.Db.Queryable<config_cable_categoryAndColor>().ToListAsync();
        var cableFunctionDtos = await context.Db.Queryable<config_cable_function>().ToListAsync();
        var cable_Specs = await context.Db.Queryable<config_cable_spec>().ToListAsync();
        var cable_SystemNumbers = await context.Db.Queryable<config_cable_systemNumber>().ToListAsync();
        var startNumbers = await context.Db.Queryable<config_cable_startNumber>().ToListAsync();
        //计算每根电缆的信息
        foreach (var cd in excelData)
        {
            //IO类型
            var match = cableFunctionDtos.FirstOrDefault(c =>
                    c.FirstIOType == cd.起点IO类型 && c.SecondIOType == cd.终点IO类型);
            cd.IO类型 = match == null ? cd.起点IO类型 : match.CableIOType;
            if (cd.起点系统号 == null || cd.终点系统号 == null) continue;
            cd.SystemNo = cable_SystemNumbers.FirstOrDefault(t =>
                            cd.起点系统号.Contains(t.StartSystem) && cd.终点系统号.Contains(t.EndSystem)) ?? new();

        }
        var cables = new List<每根电缆>();
        var ioTypeGroups = excelData.GroupBy(record => record.IO类型[..1]);
        foreach (var ioTypeGroup in ioTypeGroups)
        {
            string cableType = ioTypeGroup.Key == "D" ? "控制电缆" : "测量电缆";
            var cabinetGroups = ioTypeGroup.GroupBy(record => record.终点盘柜名称);
            foreach (var cabinetGroup in cabinetGroups)
            {
                var signals = cabinetGroup.Where(s => s.起点系统号 != null && s.终点系统号 != null).OrderBy(s=>s.起点IO类型).ToList();//这一组信号列表
                int remainingPoints = signals.Sum(signal => CountConnectionPoints(signal));//总点数

                while (remainingPoints > 0)
                {
                    var matchingCableSpec = cable_Specs.Where(c => c.CableType == cableType).OrderByDescending(c => c.ConnectCount).FirstOrDefault();                   

                    var cable = new 每根电缆
                    {
                        控制电缆 = (电缆类型)Enum.Parse(typeof(电缆类型), cableType),
                        Signals = new List<CableData>()
                    };

                    int allocatedPoints = 0;//已分配的点
                    int coreNumber = 1; // 芯数从1开始编号

                    foreach (var signal in signals.ToList())
                    {
                        int signalConnectionPoints = CountConnectionPoints(signal);

                        if (allocatedPoints + signalConnectionPoints > matchingCableSpec.ConnectCount)
                        {
                            break;
                        }
                        cable.Signals.Add(signal);
                        allocatedPoints += signalConnectionPoints;
                        AssignSignalAttributes(signal, serialNumber++, cable_CategoryAndColors, matchingCableSpec);
                        AssignCoreNumbers(signal, cableType, ref coreNumber);
                        signals.Remove(signal);
                    }
                    GenerateCableNumber(cable, cableType, constant, cable_SystemNumbers, startNumbers);

                    cables.Add(cable);
                    remainingPoints -= allocatedPoints;
                }
            }
        }
        List<CableData> cableDatas = new List<CableData>();
        foreach (var item in cables)
        {
            cableDatas.AddRange(item.Signals);
        }
        return cableDatas;
    }

    private static int CountConnectionPoints(CableData signal)
    {
        int count = 0;
        if (!string.IsNullOrEmpty(signal.起点接线点1)) count++;
        if (!string.IsNullOrEmpty(signal.起点接线点2)) count++;
        //if (!string.IsNullOrEmpty(signalCabinet.起点接线点3)) count++;
        //if (!string.IsNullOrEmpty(signalCabinet.起点接线点4)) count++;
        return count;
    }

    private static void AssignSignalAttributes(CableData signal, int serialNumber, List<config_cable_categoryAndColor> cable_CategoryAndColors, config_cable_spec matchingCableSpec)
    {
        var matchingCategoryAndColor = cable_CategoryAndColors
            .FirstOrDefault(c => c.FirstSafetyGroup == signal.起点安全分级分组 && c.SecondSafetyGroup == signal.终点安全分级分组);

        signal.序号 = serialNumber.ToString();
        signal.线缆列别 = matchingCategoryAndColor?.CableCategory ?? "未查到";
        signal.色标 = matchingCategoryAndColor?.Color ?? "未查到";
        signal.特性代码 = matchingCableSpec.FeatureCode;
    }

    private static void AssignCoreNumbers(CableData signal, string cableType, ref int coreNumber)
    {
        if (cableType == "控制电缆")
        {
            var coreNumbers = new List<string>();
            if (!string.IsNullOrEmpty(signal.起点接线点1)) coreNumbers.Add(coreNumber++.ToString());
            if (!string.IsNullOrEmpty(signal.起点接线点2)) coreNumbers.Add(coreNumber++.ToString());
            //if (!string.IsNullOrEmpty(signalCabinet.起点接线点3)) coreNumbers.Add(coreNumber++.ToString());
            //if (!string.IsNullOrEmpty(signalCabinet.起点接线点4)) coreNumbers.Add(coreNumber++.ToString());
            signal.芯线号 = string.Join("&", coreNumbers);
        }
        else if (cableType == "测量电缆")
        {
            signal.芯线号 = "R&IY";
        }
    }

    private static void GenerateCableNumber(每根电缆 cable, string cableType, string constant, List<config_cable_systemNumber> cable_SystemNumbers, List<config_cable_startNumber> startNumbers)
    {        
        string cableTypeCode = cableType == "控制电缆" ? "C" : "M";
        if (cable.Signals.Count == 0) return;
        var signal = cable.Signals.FirstOrDefault();
        if (signal.起点系统号.Contains("盘台") || signal.终点系统号.Contains("盘台"))
        {
            var startNumber = startNumbers.FirstOrDefault(n => n.StartCabinetNo == signal.起点盘柜名称 && n.Sequence == signal.线缆列别);
            if (startNumber == null)
            {
                throw new Exception("没有找到盘台匹配的流水号");
            }
            foreach (var item in cable.Signals)
            {
                string sequenceNumber = startNumber.StartNumber.ToString().PadLeft(4, '0');
                signal.线缆编号 = $"{constant}{signal.SystemNo.CableSystem}{cableTypeCode}{sequenceNumber}";
               
            }
            startNumber.StartNumber++;
        }
        else
        {

            var config = cable_SystemNumbers.FirstOrDefault(c => (c.StartMajor != null && c.EndMajor != null) && signal.起点专业.Contains(c.StartMajor) && signal.终点专业.Contains(c.EndMajor) && signal.起点系统号.Contains(c.StartSystem) && signal.终点系统号.Contains(c.EndSystem));
            if (config == null)
            {
                throw new Exception($"没有找到机柜起点专业：{signal.起点专业} 终点专业：{signal.终点专业} 起点系统号：{signal.起点系统号} 终点系统号：{signal.终点系统号}匹配的流水号");
            }
            if (config.MinNo == config.MaxNo)
            {
                throw new Exception($"机柜起点专业：{signal.起点专业} 终点专业：{signal.终点专业} 起点系统号：{signal.起点系统号} 终点系统号：{signal.终点系统号} 超过最大流水号");
            }
            foreach (var signalCabinet in cable.Signals)
            {               
                signalCabinet.线缆编号 = $"{constant}{signalCabinet.SystemNo.CableSystem}{cableTypeCode}{config.MinNo}";
               
            }
            config.MinNo++;
        }
        

       

     
    }
    private static int GetMaxSignalSupport(bool DorA, List<config_cable_spec> cableSpecifications) {
        return DorA
            ? cableSpecifications.Where(c => c.CableType == "控制电缆").Max(c => c.ConnectCount)
            : cableSpecifications.Where(c => c.CableType == "测量电缆").Max(c => c.ConnectCount);
    }
    private static void AssignSerialNumbers(List<CableData> cables) {
        for (int i = 0; i < cables.Count; i++) {
            cables[i].序号 = (i + 1).ToString();
        }
    }
    private static void SwapCablePropertiesForTypeA(List<CableData> cables) {
        foreach (var item in cables.Where(r => r.IO类型.StartsWith("A"))) {
            (item.芯线号, item.芯线对数号) = (item.芯线对数号, item.芯线号);
        }
    }
    public static Tuple<int, int> GetCableCountUsed(List<CableData> allSignals, int maxSignalSupport) {
        int needCableCount = 0;
        int tempNum = 0;
        for (int i = 0; i < allSignals.Count; i++) {
            if (tempNum == maxSignalSupport) {
                needCableCount += 1;
                tempNum = 0;
            }
            if (tempNum > maxSignalSupport) {
                needCableCount += 1;
                if (i > 0) {
                    tempNum = 0;
                    tempNum += allSignals[i - 1].起点接线点1 != @"/" ? 1 : 0;
                    tempNum += allSignals[i - 1].起点接线点2 != @"/" ? 1 : 0;
                }

            }
            tempNum += allSignals[i].起点接线点1 != @"/" ? 1 : 0;
            tempNum += allSignals[i].起点接线点2 != @"/" ? 1 : 0;
        }
        return new Tuple<int, int>(needCableCount, tempNum);
    }
    public static int GetSkipRowCount(List<CableData> allSignals, int maxSignalSupport, int i) {
        int skipNum = 0;
        int rowCount = 0;
        for (int c = 0; c < allSignals.Count; c++) {
            if (skipNum == maxSignalSupport * i) {
                break;
            }
            if (skipNum > maxSignalSupport * i) {
                rowCount -= 1;
                break;
            }
            skipNum += allSignals[c].起点接线点1 != @"/" ? 1 : 0;
            skipNum += allSignals[c].起点接线点2 != @"/" ? 1 : 0;
            rowCount += 1;
        }
        return rowCount;
    }
    public static int GetTakeRowCount(List<CableData> allSignals, int maxSignalSupport, int i, int rowCount) {
        int skipNum1 = 0;
        int rowTakeCount = 0;
        for (int c = 0; c < allSignals.Skip(rowCount).ToList().Count; c++) {
            if (skipNum1 == maxSignalSupport) {
                break;
            }
            if (skipNum1 > maxSignalSupport) {
                rowTakeCount -= 1;
                break;
            }
            skipNum1 += allSignals.Skip(rowCount).ToList()[c].起点接线点1 != @"/" ? 1 : 0;
            skipNum1 += allSignals.Skip(rowCount).ToList()[c].起点接线点2 != @"/" ? 1 : 0;
            rowTakeCount += 1;
        }
        return rowTakeCount;
    }

}

/// <summary>匹配电缆数据结果</summary>
public class MatchCableDataResult {

    /// <summary>匹配成功的电缆数据列表</summary>
    public List<CableData> SuccessList { get; set; }

    /// <summary>数据源1中匹配失败的数据列表</summary>
    public List<MatchCableDataFail> FailList1 { get; set; }

    /// <summary>数据源2中匹配失败的数据列表</summary>
    public List<MatchCableDataFail> FailList2 { get; set; }
}

/// <summary>匹配电缆失败数据</summary>
public class MatchCableDataFail {

    /// <summary>端接数据</summary>
    public required TerminationData Data { get; set; }

    /// <summary>失败原因</summary>
    public required string Reason { get; set; }
    
    /// <summary>被选中</summary>
    public bool IsChecked { get; set; } = false;

}

