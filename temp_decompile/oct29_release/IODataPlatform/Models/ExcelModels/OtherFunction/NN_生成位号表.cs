using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels.OtherFunction;

public class NN_生成位号表
{
	[Display(Name = "序号")]
	public string SequenceNumber { get; set; }

	[Display(Name = "名称")]
	public string Name { get; set; }

	[Display(Name = "描述")]
	public string Description { get; set; }

	[Display(Name = "控制站地址")]
	public string ControlStationAddress { get; set; }

	[Display(Name = "初始值")]
	public double InitialValue { get; set; }
}
