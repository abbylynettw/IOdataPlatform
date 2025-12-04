using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace IODataPlatform.Models.ExcelModels;

[SugarTable("duanjie")]

public class TerminationData
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    [Display(Name = "ID")]
    public int Id { get; set; }

    [Display(Name = "I/O点名")]
    public string IOPointName { get; set; }

    [Display(Name = "原变量名")]
    public string OriginalVariableName { get; set; }

    [Display(Name = "原扩展码")]
    public string OriginalExtensionCode { get; set; }

    [Display(Name = "信号说明")]
    public string SignalDescription { get; set; }

    [Display(Name = "机组号")]
    public string UnitNumber { get; set; }

    [Display(Name = "LOT")]
    public string LOT { get; set; }

    [Display(Name = "子项")]
    public string SubItem { get; set; }

    [Display(Name = "控制器站名")]
    public string ControllerStationName { get; set; }

    [Display(Name = "房间号")]
    public string RoomNumber { get; set; }

    [Display(Name = "盘箱柜号")]
    public string CabinetNumber { get; set; }

    [Display(Name = "I_O类型")]
    public string IOType { get; set; }

    [Display(Name = "I_O卡型号")]
    public string IOCardModel { get; set; }

    [Display(Name = "I_O卡编号")]
    public string IOCardNumber { get; set; }

    [Display(Name = "通道号")]
    public int ChannelNumber { get; set; }

    [Display(Name = "接线板型号")]
    public string TerminalBoardModel { get; set; }

    [Display(Name = "接线板编号")]
    public string TerminalBoardNumber { get; set; }

    [Display(Name = "信号起点")]
    public string SignalStart { get; set; }

    [Display(Name = "信号终点")]
    public string SignalEnd { get; set; }

    [Display(Name = "供电类型_供电方_")]
    public string PowerType_Supplier_ { get; set; }

    [Display(Name = "回路电压")]
    public string CircuitVoltage { get; set; }

    [Display(Name = "触点容量")]
    public string ContactCapacity { get; set; }

    [Display(Name = "信号特性")]
    public string SignalCharacteristics { get; set; }

    [Display(Name = "隔离")]
    public string Isolation { get; set; }

    [Display(Name = "EES供电")]
    public string EESPowerSupply { get; set; }

    [Display(Name = "传感器类型")]
    public string SensorType { get; set; }

    [Display(Name = "典型回路图")]
    public string TypicalCircuitDiagram { get; set; }

    [Display(Name = "SA供电")]
    public string SAPowerSupply { get; set; }

    [Display(Name = "事故追忆_瞬态记录")]
    public string AccidentRecall_TransientRecord { get; set; }

    [Display(Name = "内_外部点")]
    public string Internal_ExternalPoints { get; set; }

    [Display(Name = "安全分级_分组")]
    public string SafetyClassification_Grouping { get; set; }

    [Display(Name = "功能分级")]
    public string FunctionalClassification { get; set; }

    [Display(Name = "抗震类别")]
    public string SeismicCategory { get; set; }

    [Display(Name = "系统名")]
    public string SystemName { get; set; }

    [Display(Name = "I_O清单类别")]
    public string IOListCategory { get; set; }

    [Display(Name = "图号")]
    public string DrawingNumber { get; set; }

    [Display(Name = "传递单号")]
    public string TransferOrderNumber { get; set; }

    [Display(Name = "端子排编号")]
    public string TerminalStripNumber { get; set; }

    [Display(Name = "接线点1")]
    public string ConnectionPoint1 { get; set; }

    [Display(Name = "信号说明1")]
    public string SignalDescription1 { get; set; }

    [Display(Name = "接线点2")]
    public string ConnectionPoint2 { get; set; }

    [Display(Name = "信号说明2")]
    public string SignalDescription2 { get; set; }

    [Display(Name = "接线点3")]
    public string ConnectionPoint3 { get; set; }

    [Display(Name = "信号说明3")]
    public string SignalDescription3 { get; set; }

    [Display(Name = "接线点4")]
    public string ConnectionPoint4 { get; set; }

    [Display(Name = "信号说明4")]
    public string SignalDescription4 { get; set; }


    [Display(Name = "行版本")]
    public string RowVersion { get; set; }

    [Display(Name = "去向专业")]
    public string MajorDest { get; set; }


    [Display(Name = "IsMatched")]
    [SugarColumn(IsIgnore = true)]
    public bool IsMatched { get; set; }

}


