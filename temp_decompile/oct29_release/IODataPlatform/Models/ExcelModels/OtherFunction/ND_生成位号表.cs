using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels.OtherFunction;

public class ND_生成位号表
{
	[Display(Name = "序号")]
	public string SequenceNumber { get; set; }

	[Display(Name = "名称")]
	public string Name { get; set; }

	[Display(Name = "描述")]
	public string Description { get; set; }

	[Display(Name = "控制站地址")]
	public string ControlStationAddress { get; set; }

	[Display(Name = "量程下限")]
	public double RangeLowerLimit { get; set; }

	[Display(Name = "量程上限")]
	public double RangeUpperLimit { get; set; }

	[Display(Name = "单位")]
	public string Unit { get; set; }

	[Display(Name = "整型类型")]
	public string IntegerType { get; set; }

	[Display(Name = "初始值")]
	public double InitialValue { get; set; }

	[Display(Name = "位号分组")]
	public string TagGroup { get; set; }

	[Display(Name = "位号等级")]
	public string TagLevel { get; set; }
}
