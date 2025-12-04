using System;
using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels;

[Display(Name = "IO完整数据")]
public class IoFullData
{
	[Display(Name = "序号")]
	public int SerialNumber { get; set; }

	[Display(Name = "信号位号")]
	public string SignalPositionNumber { get; set; } = string.Empty;

	[Display(Name = "扩展码")]
	public string ExtensionCode { get; set; } = string.Empty;

	[Display(Name = "原变量名")]
	public string OriginalVariableName { get; set; } = string.Empty;

	[Display(Name = "信号名称")]
	public string TagName { get; set; } = string.Empty;

	[Display(Name = "信号功能")]
	public string Description { get; set; } = string.Empty;

	[Display(Name = "I/O类型")]
	public string IoType { get; set; } = string.Empty;

	[Display(Name = "信号特性")]
	public string ElectricalCharacteristics { get; set; } = string.Empty;

	[Display(Name = "供电方式")]
	public string PowerSupplyMethod { get; set; } = string.Empty;

	[Display(Name = "供电类型")]
	public string PowerType { get; set; } = string.Empty;

	[Display(Name = "是否隔离")]
	public string Isolation { get; set; } = string.Empty;

	[Display(Name = "传感器类型")]
	public string SensorType { get; set; } = string.Empty;

	[Display(Name = "工程单位")]
	public string EngineeringUnit { get; set; } = string.Empty;

	[Display(Name = "量程下限")]
	public string RangeLowerLimit { get; set; } = string.Empty;

	[Display(Name = "量程上限")]
	public string RangeUpperLimit { get; set; } = string.Empty;

	[Display(Name = "超量程下限")]
	public string SuperRangeLowerLimit { get; set; } = string.Empty;

	[Display(Name = "超量程上限")]
	public string SuperRangeUpperLimit { get; set; } = string.Empty;

	[Display(Name = "缺省值")]
	public string DefaultValue { get; set; } = string.Empty;

	[Display(Name = "就地箱号")]
	public string LocalBoxNumber { get; set; } = string.Empty;

	[Display(Name = "OF显示格式")]
	public string OFDisplayFormat { get; set; } = string.Empty;

	[Display(Name = "SN站号")]
	public string StationNumber { get; set; } = string.Empty;

	[Display(Name = "控制站")]
	public string StationName { get; set; } = string.Empty;

	[Display(Name = "SUBNET子网")]
	public string SubNet { get; set; } = string.Empty;

	[Display(Name = "机柜号")]
	public string CabinetNumber { get; set; } = string.Empty;

	[Display(Name = "所属机柜")]
	public string CabinetController { get; set; } = string.Empty;

	[Display(Name = "是否是扩展柜")]
	public string CabinetType { get; set; } = string.Empty;

	[Display(Name = "机箱号")]
	public int Cage { get; set; }

	[Display(Name = "槽位号")]
	public int Slot { get; set; }

	[Display(Name = "板卡类型")]
	public string CardType { get; set; } = string.Empty;

	[Display(Name = "后板卡类型")]
	public string BackCardType { get; set; } = string.Empty;

	[Display(Name = "板卡柜内铭牌号")]
	public string CardNumberInCabinet { get; set; } = string.Empty;

	[Display(Name = "板卡编号")]
	public string CardNumber { get; set; } = string.Empty;

	[Display(Name = "板卡地址")]
	public string CardAddress { get; set; }

	[Display(Name = "通道号")]
	public int Channel { get; set; }

	[Display(Name = "通道地址")]
	public string ChannelAddress { get; set; }

	[Display(Name = "IO基座")]
	public string IOBase { get; set; }

	[Display(Name = "端子板型号")]
	public string TerminalBoardModel { get; set; } = string.Empty;

	[Display(Name = "端子板编号")]
	public string TerminalBoardNumber { get; set; } = string.Empty;

	[Display(Name = "信号+")]
	public string SignalPlus { get; set; } = string.Empty;

	[Display(Name = "信号-")]
	public string SignalMinus { get; set; } = string.Empty;

	[Display(Name = "RTD补偿端+")]
	public string RTDCompensationC { get; set; } = string.Empty;

	[Display(Name = "RTD补偿端-")]
	public string RTDCompensationE { get; set; } = string.Empty;

	[Display(Name = "接线点1类型")]
	public string Signal1Type { get; set; } = string.Empty;

	[Display(Name = "接线点2类型")]
	public string Signal2Type { get; set; } = string.Empty;

	[Display(Name = "接线点3类型")]
	public string Signal3Type { get; set; } = string.Empty;

	[Display(Name = "接线点4类型")]
	public string Signal4Type { get; set; } = string.Empty;

	[Display(Name = "FF网段")]
	public string NetType { get; set; } = "Net1";

	[Display(Name = "FF/DP从站号")]
	public string FFDPStaionNumber { get; set; }

	[Display(Name = "FF从站通道")]
	public int? FFTerminalChannel { get; set; }

	[Display(Name = "DP阀位顺序")]
	public string DPTerminalChannel { get; set; } = string.Empty;

	[Display(Name = "FF从站模块型号")]
	public string FFSlaveModuleModel { get; set; }

	[Display(Name = "FF从站模块编号")]
	public string FFSlaveModuleID { get; set; }

	[Display(Name = "FF从站模块信号+（S+）")]
	public string FFSlaveModuleSignalPositive { get; set; }

	[Display(Name = "FF从站模块信号-（S-）")]
	public string FFSlaveModuleSignalNegative { get; set; }

	[Display(Name = "版本")]
	public string Version { get; set; } = string.Empty;

	[Display(Name = "备注")]
	public string Remarks { get; set; } = string.Empty;

	[Display(Name = "修改日期")]
	public DateTime? ModificationDate { get; set; }

	[Display(Name = "修改说明")]
	public DateTime? ModificationReason { get; set; }

	[Display(Name = "电磁阀箱类型")]
	public string? eletroValueBox { get; set; }

	[Display(Name = "系统代码")]
	public string SystemCode { get; set; } = string.Empty;

	[Display(Name = "功能码")]
	public string functionCode { get; set; } = string.Empty;

	[Display(Name = "LD/AD图号")]
	public string LDADDrawingNumber { get; set; } = string.Empty;

	[Display(Name = "P&ID图号")]
	public string PIDDrawingNumber { get; set; } = string.Empty;

	[Display(Name = "安全分级/分组")]
	public string SafetyClassificationGroup { get; set; } = string.Empty;

	[Display(Name = "抗震类别")]
	public string SeismicCategory { get; set; } = string.Empty;

	[Display(Name = "信号有效方式")]
	public string SignalEffectiveMode { get; set; } = string.Empty;

	[Display(Name = "KU点名")]
	public string KUPointName { get; set; } = string.Empty;

	[Display(Name = "PV点")]
	public string PVPoint { get; set; } = string.Empty;

	[Display(Name = "RG - 关联画面")]
	public string RGRelatedScreen { get; set; } = string.Empty;

	[Display(Name = "PD/SAMA页码")]
	public string PDSAMAPage { get; set; } = string.Empty;

	[Display(Name = "远程IO")]
	public string RemoteIO { get; set; } = string.Empty;

	[Display(Name = "趋势")]
	public string Trend { get; set; } = string.Empty;

	[Display(Name = "为0时状态")]
	public string StatusWhenZero { get; set; } = string.Empty;

	[Display(Name = "为1时状态")]
	public string StatusWhenOne { get; set; } = string.Empty;

	[Display(Name = "信号终点")]
	public string Destination { get; set; } = string.Empty;

	[Display(Name = "电压等级")]
	public string VoltageLevel { get; set; } = string.Empty;

	[Display(Name = "仪表功能号")]
	public string InstrumentFunctionNumber { get; set; } = string.Empty;

	[Display(Name = "内部/外部")]
	public string INOrOut { get; set; } = string.Empty;

	[Display(Name = "典型回路图")]
	public string TypicalLoopDrawing { get; set; } = string.Empty;

	[Display(Name = "分配信息")]
	public string AllocatedInfo { get; set; } = string.Empty;

	[Display(Name = "报警等级")]
	public string AlarmLevel { get; set; } = string.Empty;

	[Display(Name = "开关量报警标签")]
	public string SwitchQuantityAlarmTag { get; set; } = string.Empty;

	[Display(Name = "报警描述")]
	public string AlarmDescription { get; set; } = string.Empty;

	[Display(Name = "报警属性")]
	public string AlarmAttribute { get; set; } = string.Empty;

	[Display(Name = "高4限报警限值")]
	public float High4LimitAlarmValue { get; set; }

	[Display(Name = "高4限报警等级")]
	public string High4LimitAlarmLevel { get; set; } = string.Empty;

	[Display(Name = "高4限报警标签")]
	public string High4LimitAlarmTag { get; set; } = string.Empty;

	[Display(Name = "高4报警描述")]
	public string High4AlarmDescription { get; set; } = string.Empty;

	[Display(Name = "高3限报警限值")]
	public float High3LimitAlarmValue { get; set; }

	[Display(Name = "高3限报警等级")]
	public string High3LimitAlarmLevel { get; set; } = string.Empty;

	[Display(Name = "高3限报警标签")]
	public string High3LimitAlarmTag { get; set; } = string.Empty;

	[Display(Name = "高3报警描述")]
	public string High3AlarmDescription { get; set; } = string.Empty;

	[Display(Name = "高2限报警限值")]
	public float High2LimitAlarmValue { get; set; }

	[Display(Name = "高2限报警等级")]
	public string High2LimitAlarmLevel { get; set; } = string.Empty;

	[Display(Name = "高2限报警标签")]
	public string High2LimitAlarmTag { get; set; } = string.Empty;

	[Display(Name = "高2报警描述")]
	public string High2AlarmDescription { get; set; } = string.Empty;

	[Display(Name = "高1限报警限值")]
	public float High1LimitAlarmValue { get; set; }

	[Display(Name = "高1限报警等级")]
	public string High1LimitAlarmLevel { get; set; } = string.Empty;

	[Display(Name = "高1限报警标签")]
	public string High1LimitAlarmTag { get; set; } = string.Empty;

	[Display(Name = "高1报警描述")]
	public string High1AlarmDescription { get; set; } = string.Empty;

	[Display(Name = "低1限报警限值")]
	public float Low1LimitAlarmValue { get; set; }

	[Display(Name = "低1限报警等级")]
	public string Low1LimitAlarmLevel { get; set; } = string.Empty;

	[Display(Name = "低1限报警标签")]
	public string Low1LimitAlarmTag { get; set; } = string.Empty;

	[Display(Name = "低1报警描述")]
	public string Low1AlarmDescription { get; set; } = string.Empty;

	[Display(Name = "低2限报警限值")]
	public float Low2LimitAlarmValue { get; set; }

	[Display(Name = "低2限报警等级")]
	public string Low2LimitAlarmLevel { get; set; } = string.Empty;

	[Display(Name = "低2限报警标签")]
	public string Low2LimitAlarmTag { get; set; } = string.Empty;

	[Display(Name = "低2报警描述")]
	public string Low2AlarmDescription { get; set; } = string.Empty;

	[Display(Name = "低3限报警限值")]
	public float Low3LimitAlarmValue { get; set; }

	[Display(Name = "低3限报警等级")]
	public string Low3LimitAlarmLevel { get; set; } = string.Empty;

	[Display(Name = "低3限报警标签")]
	public string Low3LimitAlarmTag { get; set; } = string.Empty;

	[Display(Name = "低3报警描述")]
	public string Low3AlarmDescription { get; set; } = string.Empty;

	[Display(Name = "低4限报警限值")]
	public float Low4LimitAlarmValue { get; set; }

	[Display(Name = "低4限报警等级")]
	public string Low4LimitAlarmLevel { get; set; } = string.Empty;

	[Display(Name = "低4限报警标签")]
	public string Low4LimitAlarmTag { get; set; } = string.Empty;

	[Display(Name = "低4报警描述")]
	public string Low4AlarmDescription { get; set; } = string.Empty;

	[Display(Name = "刻度类型")]
	public string ScaleType { get; set; } = string.Empty;

	[Display(Name = "FD/SAMA页码")]
	public string FDSAMAPage { get; set; } = string.Empty;

	[Display(Name = "点类型")]
	public TagType PointType { get; set; }

	[Display(Name = "未分配原因")]
	public string UnsetReason { get; set; } = string.Empty;
}
