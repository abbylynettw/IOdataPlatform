using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels.OtherFunction;

public class Control_生成位号表
{
	[Display(Name = "域地址")]
	public string 域地址 { get; set; }

	[Display(Name = "域名称")]
	public string 域名称 { get; set; }

	[Display(Name = "域描述")]
	public string 域描述 { get; set; }

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
