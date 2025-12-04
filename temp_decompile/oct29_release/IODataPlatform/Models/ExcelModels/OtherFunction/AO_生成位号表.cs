using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels.OtherFunction;

public class AO_生成位号表
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

	[Display(Name = "量程下限")]
	public double RangeLowerLimit { get; set; }

	[Display(Name = "量程上限")]
	public double RangeUpperLimit { get; set; }

	[Display(Name = "单位")]
	public string Unit { get; set; }

	[Display(Name = "位号类型")]
	public string TagType { get; set; }

	[Display(Name = "模块类型")]
	public string ModuleType { get; set; }

	[Display(Name = "信号类型")]
	public string SignalType { get; set; }

	[Display(Name = "故障安全模式")]
	public string FaultSafetyMode { get; set; }

	[Display(Name = "故障状态设定值")]
	public double FaultStateSetValue { get; set; }

	[Display(Name = "位号运行周期")]
	public string TagOperatingCycle { get; set; }

	[Display(Name = "数据类型")]
	public string DataType { get; set; }

	[Display(Name = "信号性质")]
	public string SignalNature { get; set; }

	[Display(Name = "状态码位置")]
	public string StatusCodePosition { get; set; }

	[Display(Name = "数据格式")]
	public string DataFormat { get; set; }

	[Display(Name = "转换类型")]
	public string ConversionType { get; set; }

	[Display(Name = "正/反输出")]
	public string PositiveNegativeOutput { get; set; }

	[Display(Name = "扩展量程上限百分量(%)")]
	public double ExtendedRangeUpperPercentage { get; set; }

	[Display(Name = "扩展量程下限百分量(%)")]
	public double ExtendedRangeLowerPercentage { get; set; }

	[Display(Name = "超量程上限报警")]
	public string OverrangeUpperAlarm { get; set; }

	[Display(Name = "超量程下限报警")]
	public string OverrangeLowerAlarm { get; set; }

	[Display(Name = "输出原始码上限")]
	public double OutputRawCodeUpperLimit { get; set; }

	[Display(Name = "输出原始码下限")]
	public double OutputRawCodeLowerLimit { get; set; }

	[Display(Name = "输出高限限幅报警")]
	public string OutputHighLimitClippingAlarm { get; set; }

	[Display(Name = "输出高限限幅报警等级")]
	public string OutputHighLimitClippingAlarmLevel { get; set; }

	[Display(Name = "输出高限限幅值")]
	public double OutputHighLimitClippingValue { get; set; }

	[Display(Name = "输出低限限幅报警")]
	public string OutputLowLimitClippingAlarm { get; set; }

	[Display(Name = "输出低限限幅报警等级")]
	public string OutputLowLimitClippingAlarmLevel { get; set; }

	[Display(Name = "输出低限限幅值")]
	public double OutputLowLimitClippingValue { get; set; }

	[Display(Name = "故障报警")]
	public string FaultAlarm { get; set; }

	[Display(Name = "故障报警等级")]
	public string FaultAlarmLevel { get; set; }

	[Display(Name = "组态错误报警")]
	public string ConfigurationErrorAlarm { get; set; }

	[Display(Name = "组态错误报警等级")]
	public string ConfigurationErrorAlarmLevel { get; set; }

	[Display(Name = "位号分组")]
	public string TagGroup { get; set; }

	[Display(Name = "位号等级")]
	public string TagLevel { get; set; }

	[Display(Name = "小数位数")]
	public string DecimalPlaces { get; set; }
}
