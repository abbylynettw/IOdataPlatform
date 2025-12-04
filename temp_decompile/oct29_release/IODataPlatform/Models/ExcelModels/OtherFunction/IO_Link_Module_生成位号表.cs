using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels.OtherFunction;

public class IO_Link_Module_生成位号表
{
	[Display(Name = "控制器地址")]
	public string 控制器地址 { get; set; }

	[Display(Name = "地址")]
	public string 地址 { get; set; }

	[Display(Name = "型号")]
	public string 型号 { get; set; }

	[Display(Name = "描述")]
	public string 描述 { get; set; }

	[Display(Name = "备注")]
	public string 备注 { get; set; }

	[Display(Name = "冗余")]
	public string 冗余 { get; set; }
}
