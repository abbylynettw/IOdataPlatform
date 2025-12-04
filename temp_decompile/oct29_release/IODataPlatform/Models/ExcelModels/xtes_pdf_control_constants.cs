using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels;

public class xtes_pdf_control_constants
{
	[Display(Name = "序号")]
	public string 序号 { get; set; } = string.Empty;

	[Display(Name = "仪表功能号")]
	public string 仪表功能号 { get; set; } = string.Empty;

	[Display(Name = "功能描述")]
	public string 功能描述 { get; set; } = string.Empty;

	[Display(Name = "最小值")]
	public string 最小值 { get; set; } = string.Empty;

	[Display(Name = "最大值")]
	public string 最大值 { get; set; } = string.Empty;

	[Display(Name = "单位")]
	public string 单位 { get; set; } = string.Empty;

	[Display(Name = "类型")]
	public string 类型 { get; set; } = string.Empty;

	[Display(Name = "数值")]
	public string 数值 { get; set; } = string.Empty;

	[Display(Name = "动作")]
	public string 动作 { get; set; } = string.Empty;

	[Display(Name = "备注")]
	public string 备注 { get; set; } = string.Empty;
}
