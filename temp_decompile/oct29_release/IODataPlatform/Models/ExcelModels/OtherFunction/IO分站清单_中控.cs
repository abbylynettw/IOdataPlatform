using System;
using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels.OtherFunction;

public class IO分站清单_中控
{
	[Display(Name = "序号")]
	public int SequenceNumber { get; set; }

	[Display(Name = "LD/AD图号")]
	public string DiagramNumber { get; set; }

	[Display(Name = "信号位号")]
	public string SignalTag { get; set; }

	[Display(Name = "扩展码")]
	public string ExtensionCode { get; set; }

	[Display(Name = "标签名称")]
	public string TagName { get; set; }

	[Display(Name = "信号功能")]
	public string SignalFunction { get; set; }

	[Display(Name = "功能码")]
	public string FunctionCode { get; set; }

	[Display(Name = "系统代码")]
	public string SystemCode { get; set; }

	[Display(Name = "就地箱号")]
	public string LocalBoxNumber { get; set; }

	[Display(Name = "机柜号")]
	public string CabinetNumber { get; set; }

	[Display(Name = "板卡类型")]
	public string CardType { get; set; }

	[Display(Name = "板卡柜内编号")]
	public string CardCabinetNumber { get; set; }

	[Display(Name = "板卡编号")]
	public string CardNumber { get; set; }

	[Display(Name = "机架")]
	public int Rack { get; set; }

	[Display(Name = "插槽")]
	public int Slot { get; set; }

	[Display(Name = "通道/FF网段")]
	public int ChannelOrFFSegment { get; set; }

	[Display(Name = "FF/DP从站号")]
	public string FFOrDPSlaveNumber { get; set; }

	[Display(Name = "FF/DP端子通道")]
	public string FFOrDPTerminalChannel { get; set; }

	[Display(Name = "通道地址")]
	public string ChannelAddress { get; set; }

	[Display(Name = "IO基座")]
	public string IoBase { get; set; }

	[Display(Name = "端子板型号")]
	public string TerminalBoardModel { get; set; }

	[Display(Name = "端子板编号")]
	public string TerminalBoardNumber { get; set; }

	[Display(Name = "信号+（V+")]
	public string SignalPositive { get; set; }

	[Display(Name = "信号-（V-")]
	public string SignalNegative { get; set; }

	[Display(Name = "补偿端（I+")]
	public string CompensationPositive { get; set; }

	[Display(Name = "补偿端（I-")]
	public string CompensationNegative { get; set; }

	[Display(Name = "FF从站模块型号")]
	public string FFSlaveModuleModel { get; set; }

	[Display(Name = "FF从站模块编号")]
	public string FFSlaveModuleNumber { get; set; }

	[Display(Name = "FF从站模块信号+（S+）")]
	public string FFSlaveModuleSignalPositive { get; set; }

	[Display(Name = "FF从站模块信号-（S-）")]
	public string FFSlaveModuleSignalNegative { get; set; }

	[Display(Name = "IO类型")]
	public string IOType { get; set; }

	[Display(Name = "信号特性")]
	public string SignalCharacteristic { get; set; }

	[Display(Name = "供电方式")]
	public string PowerSupplyMode { get; set; }

	[Display(Name = "供电类型")]
	public string PowerSupplyType { get; set; }

	[Display(Name = "传感器类型")]
	public string SensorType { get; set; }

	[Display(Name = "测量单位")]
	public string MeasurementUnit { get; set; }

	[Display(Name = "量程下限")]
	public string RangeLowerLimit { get; set; }

	[Display(Name = "量程上限")]
	public double RangeUpperLimit { get; set; }

	[Display(Name = "信号有效方式")]
	public string SignalEffectiveMode { get; set; }

	[Display(Name = "电磁阀箱地址")]
	public string SolenoidValveBoxAddress { get; set; }

	[Display(Name = "电磁阀箱类型")]
	public string SolenoidValveBoxType { get; set; }

	[Display(Name = "板块地址")]
	public string PlateAddress { get; set; }

	[Display(Name = "虚拟板块通道")]
	public string VirtualPlateChannel { get; set; }

	[Display(Name = "通道")]
	public string Channel { get; set; }

	[Display(Name = "板卡位号")]
	public string CardSlotNumber { get; set; }

	[Display(Name = "版本")]
	public string Version { get; set; }

	[Display(Name = "备注")]
	public string Remarks { get; set; }

	[Display(Name = "修改日期")]
	public DateTime ModificationDate { get; set; }

	[Display(Name = "修改说明")]
	public string ModificationDescription { get; set; }

	[Display(Name = "阀箱共负端子号")]
	public string ValveBoxCommonNegativeTerminalNumber { get; set; }
}
