using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels.OtherFunction;

public class PN_生成位号表
{
	[Display(Name = "序号")]
	public string SequenceNumber { get; set; }

	[Display(Name = "名称")]
	public string Name { get; set; }

	[Display(Name = "描述")]
	public string Description { get; set; }

	[Display(Name = "控制站地址")]
	public string ControlStationAddress { get; set; }

	[Display(Name = "功能块地址")]
	public string FunctionBlockAddress { get; set; }

	[Display(Name = "功能块类型")]
	public string FunctionBlockType { get; set; }

	[Display(Name = "功能块库ID")]
	public string FunctionBlockLibraryId { get; set; }

	[Display(Name = "模块ID")]
	public string ModuleId { get; set; }

	[Display(Name = "所属程序名")]
	public string ProgramName { get; set; }

	[Display(Name = "位号分组")]
	public string TagGroup { get; set; }

	[Display(Name = "位号等级")]
	public string TagLevel { get; set; }
}
