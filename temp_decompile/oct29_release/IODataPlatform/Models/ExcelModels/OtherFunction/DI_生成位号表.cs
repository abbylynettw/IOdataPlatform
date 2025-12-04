using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels.OtherFunction;

public class DI_生成位号表
{
	[Display(Name = "序号")]
	public string SequenceNumber { get; set; }

	[Display(Name = "名称")]
	public string Name { get; set; }

	[Display(Name = "描述")]
	public string Description { get; set; }

	[Display(Name = "控制站地址")]
	public string ControlStationAddress { get; set; }

	[Display(Name = "地址")]
	public string Address { get; set; }

	[Display(Name = "ON描述")]
	public string OnDescription { get; set; }

	[Display(Name = "OFF描述")]
	public string OffDescription { get; set; }

	[Display(Name = "位号类型")]
	public string TagType { get; set; }

	[Display(Name = "模块类型")]
	public string ModuleType { get; set; }

	[Display(Name = "位号运行周期")]
	public string TagOperatingCycle { get; set; }

	[Display(Name = "输入取反")]
	public string InputReverse { get; set; }

	[Display(Name = "ON状态报警")]
	public string OnStateAlarm { get; set; }

	[Display(Name = "ON状态报警等级")]
	public string OnStateAlarmLevel { get; set; }

	[Display(Name = "OFF状态报警")]
	public string OffStateAlarm { get; set; }

	[Display(Name = "OFF状态报警等级")]
	public string OffStateAlarmLevel { get; set; }

	[Display(Name = "正跳变报警")]
	public string PositiveJumpAlarm { get; set; }

	[Display(Name = "正跳变报警等级")]
	public string PositiveJumpAlarmLevel { get; set; }

	[Display(Name = "负跳变报警")]
	public string NegativeJumpAlarm { get; set; }

	[Display(Name = "负跳变报警等级")]
	public string NegativeJumpAlarmLevel { get; set; }

	[Display(Name = "故障报警")]
	public string FaultAlarm { get; set; }

	[Display(Name = "故障报警等级")]
	public string FaultAlarmLevel { get; set; }

	[Display(Name = "故障处理")]
	public string FaultHandling { get; set; }

	[Display(Name = "位号分组")]
	public string TagGroup { get; set; }

	[Display(Name = "位号等级")]
	public string TagLevel { get; set; }

	[Display(Name = "SOE硬点标记")]
	public string SoeHardPointMarker { get; set; }

	[Display(Name = "SOE标志")]
	public string SoeFlag { get; set; }

	[Display(Name = "SOE描述")]
	public string SoeDescription { get; set; }

	[Display(Name = "SOE设备组")]
	public string SoeDeviceGroup { get; set; }
}
