using System.ComponentModel.DataAnnotations;

public class xtes_GST
{
	[Display(Name = "PN - 点名")]
	public string PN { get; set; }

	[Display(Name = "DESC - 点描述")]
	public string DESC { get; set; }

	[Display(Name = "TRAIN - 安全列")]
	public string TRAIN { get; set; }

	[Display(Name = "RG - 关联FD/SAMA图")]
	public string RG { get; set; }

	[Display(Name = "IH - 是否进历史库")]
	public bool IH { get; set; }

	[Display(Name = "SYS - 系统名")]
	public string SYS { get; set; }

	[Display(Name = "UNIT - 单位")]
	public string UNIT { get; set; }

	[Display(Name = "PVH - 设定值高限")]
	public double PVH { get; set; }

	[Display(Name = "PVL - 设定值低限")]
	public double PVL { get; set; }

	[Display(Name = "SPH - 设定值允许设置高限")]
	public double SPH { get; set; }

	[Display(Name = "SPL - 设定值允许设置低限")]
	public double SPL { get; set; }

	[Display(Name = "SN - 站号")]
	public int SN { get; set; }

	[Display(Name = "SUBNET - 子网")]
	public string SUBNET { get; set; }

	[Display(Name = "FRATE - 快增快减变化率%")]
	public double FRATE { get; set; }

	[Display(Name = "SRATE - 慢增慢减变化率%")]
	public double SRATE { get; set; }
}
