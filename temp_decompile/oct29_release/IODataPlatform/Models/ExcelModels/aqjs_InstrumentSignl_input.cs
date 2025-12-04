namespace IODataPlatform.Models.ExcelModels;

/// <summary>
/// 安全级室仪表信号设计输入，用于存储从Excel文件中提取的数据和新数据
/// </summary>
public class aqjs_InstrumentSignl_input
{
	public int 行号 { get; set; }

	public int 序号 { get; set; }

	public string 设备编号 { get; set; }

	public string 功能1 { get; set; }

	public string 安全等级 { get; set; }

	public string 序列 { get; set; }

	public string 安全级机柜 { get; set; }

	public string 功能描述 { get; set; }

	public string IO类型_AI { get; set; }

	public string IO类型_AO { get; set; }

	public string IO类型_DI { get; set; }

	public string IO类型_DO { get; set; }

	public string 信号类型 { get; set; }

	public string 显示控制_DCS { get; set; }

	public string 显示控制_中央控制室 { get; set; }

	public string 显示控制_应急监控室 { get; set; }

	public double 仪表量程_最小 { get; set; }

	public double 仪表量程_最大 { get; set; }

	public string 仪表量程_单位 { get; set; }

	public string 运算 { get; set; }

	public double 二次表量程_最小 { get; set; }

	public double 二次表量程_最大 { get; set; }

	public string 二次表量程_单位 { get; set; }

	public string 功能2 { get; set; }

	public double 阈值 { get; set; }

	public string 阈值_单位 { get; set; }

	public string 转入DCS信号_AI { get; set; }

	public string 转入DCS信号_AO { get; set; }

	public string 转入DCS信号_DI { get; set; }

	public string 转入DCS信号_DO { get; set; }

	public string DCS机柜 { get; set; }

	public string 附注 { get; set; }

	public string 原理图号 { get; set; }

	public string 所属类型 { get; set; }

	public string 站号 { get; set; }
}
