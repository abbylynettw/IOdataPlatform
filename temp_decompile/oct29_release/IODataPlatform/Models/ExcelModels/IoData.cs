using System;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace IODataPlatform.Models.ExcelModels;

[SugarTable("iodata")]
public class IoData
{
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	[Display(Name = "ID")]
	public int Id { get; set; }

	[Display(Name = "标签名")]
	public string TagName { get; set; }

	[Display(Name = "原变量名")]
	public string OldVarName { get; set; }

	[Display(Name = "原扩展码")]
	public string OldExtCode { get; set; }

	[Display(Name = "信号说明")]
	public string Designation { get; set; }

	[Display(Name = "机组号")]
	public string Unit { get; set; }

	[Display(Name = "LOT号")]
	public string Lot { get; set; }

	[Display(Name = "子项")]
	public string SubItem { get; set; }

	[Display(Name = "控制器站名")]
	public string FcuName { get; set; }

	[Display(Name = "房间号")]
	public string Room { get; set; }

	[Display(Name = "盘箱柜号")]
	public string Cabinet { get; set; }

	[Display(Name = "IO类型")]
	public string IoType { get; set; }

	[Display(Name = "IO卡型号")]
	public string Type { get; set; }

	[Display(Name = "IO卡编号")]
	public string IoCardNumber { get; set; }

	[Display(Name = "通道号")]
	public int Pin { get; set; }

	[Display(Name = "接线板型号")]
	public string TAPartNumber { get; set; }

	[Display(Name = "接线板编号")]
	public string Component { get; set; }

	[Display(Name = "信号起点")]
	public string SignalStart { get; set; }

	[Display(Name = "信号终点")]
	public string Destination { get; set; }

	[Display(Name = "供电类型")]
	public string PowerType { get; set; }

	[Display(Name = "回路电压")]
	public string LoopVoltage { get; set; }

	[Display(Name = "触点容量")]
	public string ContactCapacity { get; set; }

	[Display(Name = "信号特性")]
	public string SignalSpec { get; set; }

	[Display(Name = "隔离")]
	public string Isolation { get; set; }

	[Display(Name = "EES供电")]
	public string Ees { get; set; }

	[Display(Name = "传感器类型")]
	public string SensorType { get; set; }

	[Display(Name = "典型回路图")]
	public string TypicalLoopDrawing { get; set; }

	[Display(Name = "SA供电")]
	public string Sa { get; set; }

	[Display(Name = "事故追忆/瞬态记录")]
	public string SoeTra { get; set; }

	[Display(Name = "内/外部点")]
	public string IntExt { get; set; }

	[Display(Name = "安全分级/分组")]
	public string SafetyClassDivision { get; set; }

	[Display(Name = "功能分级")]
	public string FunctionalClass { get; set; }

	[Display(Name = "抗震类别")]
	public string Seismic { get; set; }

	[Display(Name = "系统名")]
	public string System { get; set; }

	[Display(Name = "IO清单类别")]
	public string List { get; set; }

	[Display(Name = "图号")]
	public string DiagramNumber { get; set; }

	[Display(Name = "传递单号")]
	public string TransNo { get; set; }

	[Display(Name = "端子排编号")]
	public string TerminalBlock { get; set; }

	[Display(Name = "备注")]
	public string Remarks { get; set; }

	[Display(Name = "序号")]
	public string SerialNumber { get; set; }

	[Display(Name = "P&ID图号")]
	public string PIDDrawingNumber { get; set; }

	[Display(Name = "LD/AD图号")]
	public string LDADDrawingNumber { get; set; }

	[Display(Name = "板卡编号")]
	public string CardNumber { get; set; }

	[Display(Name = "机笼")]
	public string Cage { get; set; }

	[Display(Name = "通道")]
	public int Slot { get; set; }

	[Display(Name = "板卡地址")]
	public string CardAddress { get; set; }

	[Display(Name = "FF/DP端子通道")]
	public int? FfdpTerminalchannel { get; set; }

	[Display(Name = "电气特性")]
	public string ElectricalCharacteristics { get; set; }

	[Display(Name = "供电方式")]
	public string PowerSupplyMethod { get; set; }

	[Display(Name = "工程单位")]
	public string EngineeringUnit { get; set; }

	[Display(Name = "量程范围下限")]
	public string RangeLowerLimit { get; set; }

	[Display(Name = "量程范围上限")]
	public string RangeUpperLimit { get; set; }

	[Display(Name = "远程IO")]
	public string RemoteIO { get; set; }

	[Display(Name = "趋势")]
	public string Trend { get; set; }

	[Display(Name = "为0时状态")]
	public string StatusWhenZero { get; set; }

	[Display(Name = "为1时状态")]
	public string StatusWhenOne { get; set; }

	[Display(Name = "信号有效方式")]
	public string SignalEffectiveMode { get; set; }

	[Display(Name = "就地箱号")]
	public string LocalBoxNumber { get; set; }

	[Display(Name = "RG-关联画面")]
	public string RgRelatedscreen { get; set; }

	[Display(Name = "高4限报警限值")]
	public float High4LimitAlarmValue { get; set; }

	[Display(Name = "高4限报警等级")]
	public string High4LimitAlarmLevel { get; set; }

	[Display(Name = "高4限报警标签")]
	public string High4LimitAlarmTag { get; set; }

	[Display(Name = "高4报警描述")]
	public string High4AlarmDescription { get; set; }

	[Display(Name = "高3限报警限值")]
	public float High3LimitAlarmValue { get; set; }

	[Display(Name = "高3限报警等级")]
	public string High3LimitAlarmLevel { get; set; }

	[Display(Name = "高3限报警标签")]
	public string High3LimitAlarmTag { get; set; }

	[Display(Name = "高3报警描述")]
	public string High3AlarmDescription { get; set; }

	[Display(Name = "高2限报警限值")]
	public float High2LimitAlarmValue { get; set; }

	[Display(Name = "高2限报警等级")]
	public string High2LimitAlarmLevel { get; set; }

	[Display(Name = "高2限报警标签")]
	public string High2LimitAlarmTag { get; set; }

	[Display(Name = "高2报警描述")]
	public string High2AlarmDescription { get; set; }

	[Display(Name = "高1限报警限值")]
	public float High1LimitAlarmValue { get; set; }

	[Display(Name = "高1限报警等级")]
	public string High1LimitAlarmLevel { get; set; }

	[Display(Name = "高1限报警标签")]
	public string High1LimitAlarmTag { get; set; }

	[Display(Name = "高1报警描述")]
	public string High1AlarmDescription { get; set; }

	[Display(Name = "低1限报警限值")]
	public float Low1LimitAlarmValue { get; set; }

	[Display(Name = "低1限报警等级")]
	public string Low1LimitAlarmLevel { get; set; }

	[Display(Name = "低1限报警标签")]
	public string Low1LimitAlarmTag { get; set; }

	[Display(Name = "低1报警描述")]
	public string Low1AlarmDescription { get; set; }

	[Display(Name = "低2限报警限值")]
	public float Low2LimitAlarmValue { get; set; }

	[Display(Name = "低2限报警等级")]
	public string Low2LimitAlarmLevel { get; set; }

	[Display(Name = "低2限报警标签")]
	public string Low2LimitAlarmTag { get; set; }

	[Display(Name = "低2报警描述")]
	public string Low2AlarmDescription { get; set; }

	[Display(Name = "低3限报警限值")]
	public float Low3LimitAlarmValue { get; set; }

	[Display(Name = "低3限报警等级")]
	public string Low3LimitAlarmLevel { get; set; }

	[Display(Name = "低3限报警标签")]
	public string Low3LimitAlarmTag { get; set; }

	[Display(Name = "低3报警描述")]
	public string Low3AlarmDescription { get; set; }

	[Display(Name = "低4限报警限值")]
	public float Low4LimitAlarmValue { get; set; }

	[Display(Name = "低4限报警等级")]
	public string Low4LimitAlarmLevel { get; set; }

	[Display(Name = "低4限报警标签")]
	public string Low4LimitAlarmTag { get; set; }

	[Display(Name = "低4报警描述")]
	public string Low4AlarmDescription { get; set; }

	[Display(Name = "报警等级")]
	public string AlarmLevel { get; set; }

	[Display(Name = "开关量报警标签")]
	public string SwitchQuantityAlarmTag { get; set; }

	[Display(Name = "报警描述")]
	public string AlarmDescription { get; set; }

	[Display(Name = "报警属性")]
	public string AlarmAttribute { get; set; }

	[Display(Name = "站号")]
	public string SnStationnumber { get; set; }

	[Display(Name = "子网")]
	public string SubnetSubnet { get; set; }

	[Display(Name = "KU点名")]
	public string KUPointName { get; set; }

	[Display(Name = "显示格式")]
	public string OfDisplayformat { get; set; }

	[Display(Name = "PV点")]
	public string PVPoint { get; set; }

	[Display(Name = "接线点1")]
	public string Connection1 { get; set; }

	[Display(Name = "信号说明1")]
	public string SigPotential1 { get; set; }

	[Display(Name = "接线点2")]
	public string Connection2 { get; set; }

	[Display(Name = "信号说明2")]
	public string SigPotential2 { get; set; }

	[Display(Name = "接线点3")]
	public string Connection3 { get; set; }

	[Display(Name = "信号说明3")]
	public string SigPotential3 { get; set; }

	[Display(Name = "接线点4")]
	public string Connection4 { get; set; }

	[Display(Name = "信号说明4")]
	public string SigPotential4 { get; set; }

	[Display(Name = "后卡件类型")]
	public string BackCardType { get; set; }

	[Display(Name = "刻度类型")]
	public string ScaleType { get; set; }

	[Display(Name = "缺省值")]
	public string DefaultNumber { get; set; }

	[Display(Name = "分配信息")]
	public string AllocateInfo { get; set; }

	[Display(Name = "版本")]
	public string Version { get; set; }

	[Display(Name = "修改日期")]
	public DateTime? ModificationDate { get; set; }

	[Display(Name = "去向专业")]
	public string MajorDest { get; set; }
}
