namespace IODataPlatform.Models.ExcelModels;

public abstract class pdf_control_io
{
	public abstract string 序号 { get; set; }

	public virtual string? 机柜号 { get; set; }

	public virtual string? 就地箱号 { get; set; }

	public virtual string? 信号位号 { get; set; }

	public virtual string? 扩展码 { get; set; }

	public virtual string? 信号功能 { get; set; }

	public virtual string? 安全分级 { get; set; }

	public virtual string? 抗震类别 { get; set; }

	public virtual string? 传感器类型 { get; set; }

	public virtual string? IO类型 { get; set; }

	public virtual string? 信号特性 { get; set; }

	public virtual string? 供电类型 { get; set; }

	public virtual string? 最小测量范围 { get; set; }

	public virtual string? 最大测量范围 { get; set; }

	public virtual string? 单位 { get; set; }

	public virtual string? 电压等级 { get; set; }

	public virtual string? 仪表功能号 { get; set; }

	public virtual string? FF从站模块型号 { get; set; }

	public virtual string? 版本 { get; set; }

	public virtual string? 备注 { get; set; }

	public virtual string? 信号说明 { get; set; }

	public virtual string? 安全分级分组 { get; set; }

	public virtual string? 功能分级 { get; set; }

	public virtual string? 供电方 { get; set; }

	public virtual string? 测量单位 { get; set; }

	public virtual string? 量程下限 { get; set; }

	public virtual string? 量程上限 { get; set; }

	public virtual string? 缺省值 { get; set; }

	public virtual string? SOETRA { get; set; }

	public virtual string? 负载信息 { get; set; }

	public virtual string? 图号 { get; set; }
}
