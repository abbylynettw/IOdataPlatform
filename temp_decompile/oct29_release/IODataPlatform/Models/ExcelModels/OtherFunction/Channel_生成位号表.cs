using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels.OtherFunction;

public class Channel_生成位号表
{
	[Display(Name = "控制器地址")]
	public string 控制器地址 { get; set; }

	[Display(Name = "io连接模块地址")]
	public string io连接模块地址 { get; set; }

	[Display(Name = "机架地址")]
	public string 机架地址 { get; set; }

	[Display(Name = "io模块地址")]
	public string io模块地址 { get; set; }

	[Display(Name = "地址")]
	public string 地址 { get; set; }

	[Display(Name = "信号模式")]
	public string 信号模式 { get; set; }

	[Display(Name = "信号类型")]
	public string 信号类型 { get; set; }

	[Display(Name = "配电状况")]
	public string 配电状况 { get; set; }

	[Display(Name = "通道开关")]
	public string 通道开关 { get; set; }

	[Display(Name = "有效性检测")]
	public string 有效性检测 { get; set; }

	[Display(Name = "采样模式")]
	public string 采样模式 { get; set; }

	[Display(Name = "量程上限")]
	public string 量程上限 { get; set; }

	[Display(Name = "量程下限")]
	public string 量程下限 { get; set; }

	[Display(Name = "脉冲接线类型")]
	public string 脉冲接线类型 { get; set; }

	[Display(Name = "故障安全模式")]
	public string 故障安全模式 { get; set; }

	[Display(Name = "滤波时间")]
	public string 滤波时间 { get; set; }

	[Display(Name = "锁存功能")]
	public string 锁存功能 { get; set; }

	[Display(Name = "抖动参数")]
	public string 抖动参数 { get; set; }

	[Display(Name = "接近开关信号")]
	public string 接近开关信号 { get; set; }

	[Display(Name = "防抖时间")]
	public string 防抖时间 { get; set; }

	[Display(Name = "输出类型")]
	public string 输出类型 { get; set; }

	[Display(Name = "继电器类型")]
	public string 继电器类型 { get; set; }
}
