using System.ComponentModel.DataAnnotations;

public class xtes_DVI
{
	[Display(Name = "CHN - 通道号")]
	public string CHN { get; set; }

	[Display(Name = "PN - 点名")]
	public string PN { get; set; }

	[Display(Name = "DESC - 点描述")]
	public string DESC { get; set; }

	[Display(Name = "TRAIN - 安全列")]
	public string TRAIN { get; set; }

	[Display(Name = "RG - 关联画面")]
	public string RG { get; set; }

	[Display(Name = "IH - 是否进历史库")]
	public string IH { get; set; }

	[Display(Name = "SYS - 系统名")]
	public string SYS { get; set; }

	[Display(Name = "INLOG - 是否进日志")]
	public string INLOG { get; set; }

	[Display(Name = "SUBNET - 子网")]
	public string SUBNET { get; set; }

	[Display(Name = "CLN - IO卡槽位地址号")]
	public string CLN { get; set; }

	[Display(Name = "MON - IO卡名")]
	public string MON { get; set; }

	[Display(Name = "SN - 站号")]
	public string SN { get; set; }

	[Display(Name = "TOC - 发生抖动时间限(s)")]
	public string TOC { get; set; }

	[Display(Name = "TVA - 抖动消失时间限(s)")]
	public string TVA { get; set; }

	[Display(Name = "TOCT - 抖动次数")]
	public string TOCT { get; set; }

	[Display(Name = "BCT - 是否判断抖动")]
	public string BCT { get; set; }

	[Display(Name = "DBT - 硬件消抖时间(ms)")]
	public string DBT { get; set; }

	[Display(Name = "QFM - 输出信号替代方式")]
	public string QFM { get; set; }

	[Display(Name = "QFID - 输出替代值")]
	public string QFID { get; set; }

	[Display(Name = "SOE - 是否为SOE点")]
	public string SOE { get; set; }

	[Display(Name = "QUICK - 是否高速上传")]
	public string QUICK { get; set; }

	[Display(Name = "IC - 是否产生仪控故障")]
	public string IC { get; set; }

	[Display(Name = "ALLOCATION - 报警分组")]
	public string ALLOCATION { get; set; }

	[Display(Name = "ACUT - 是否报警切除")]
	public string ACUT { get; set; }

	[Display(Name = "AP - 是否判断报警")]
	public int AP { get; set; }

	[Display(Name = "ALMLEVEL - 报警级别")]
	public string ALMLEVEL { get; set; }

	[Display(Name = "KA - 开关量报警标签")]
	public string KA { get; set; }

	[Display(Name = "AL_DESC - 报警描述")]
	public string AL_DESC { get; set; }

	[Display(Name = "AF - 报警属性")]
	public string AF { get; set; }

	[Display(Name = "DEC - 是否为DEC大修报警")]
	public string DEC { get; set; }

	[Display(Name = "SI - 是否为安注报警")]
	public string SI { get; set; }

	[Display(Name = "TBTYPE - 端子板类型配置")]
	public string TBTYPE { get; set; }

	[Display(Name = "ROUT - 是否取反输出")]
	public string ROUT { get; set; }

	[Display(Name = "E1 - 置1说明")]
	public string E1 { get; set; }

	[Display(Name = "E0 - 置0说明")]
	public string E0 { get; set; }
}
