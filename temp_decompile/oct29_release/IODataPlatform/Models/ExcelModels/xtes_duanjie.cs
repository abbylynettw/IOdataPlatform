using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels;

public class xtes_duanjie
{
	[Display(Name = "序号")]
	public int 序号 { get; set; }

	[Display(Name = "I/O点名")]
	public string I_O点名 { get; set; }

	[Display(Name = "信号说明")]
	public string 信号说明 { get; set; }

	[Display(Name = "房间号")]
	public string 房间号 { get; set; }

	[Display(Name = "机柜名（起点）")]
	public string 机柜名_起点 { get; set; }

	[Display(Name = "设备编号（起点）")]
	public string 设备编号_起点 { get; set; }

	[Display(Name = "信号特性")]
	public string 信号特性 { get; set; }

	[Display(Name = "接线点1")]
	public string 接线点1 { get; set; }

	[Display(Name = "接线点说明1")]
	public string 接线点说明1 { get; set; }

	[Display(Name = "接线点2")]
	public string 接线点2 { get; set; }

	[Display(Name = "接线点说明2")]
	public string 接线点说明2 { get; set; }

	[Display(Name = "接线点3")]
	public string 接线点3 { get; set; }

	[Display(Name = "接线点说明3")]
	public string 接线点说明3 { get; set; }

	[Display(Name = "接线点4")]
	public string 接线点4 { get; set; }

	[Display(Name = "接线点说明4")]
	public string 接线点说明4 { get; set; }

	[Display(Name = "供电方式")]
	public string 供电方式 { get; set; }

	[Display(Name = "房间号（终点）")]
	public string 房间号_终点 { get; set; }

	[Display(Name = "设备编号（终点）")]
	public string 设备编号_终点 { get; set; }

	[Display(Name = "电缆编码")]
	public string 电缆编码 { get; set; }

	[Display(Name = "电缆型号及规格")]
	public string 电缆型号及规格 { get; set; }

	[Display(Name = "电缆长度")]
	public string 电缆长度 { get; set; }

	[Display(Name = "供货方")]
	public string 供货方 { get; set; }

	[Display(Name = "版本")]
	public string 版本 { get; set; }
}
