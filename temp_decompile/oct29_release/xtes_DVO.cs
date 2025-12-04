using System.ComponentModel.DataAnnotations;

public class xtes_DVO
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

	[Display(Name = "FAVTYPE - 执行方式")]
	public string FAVTYPE { get; set; }

	[Display(Name = "FAV - 故障安全输出值")]
	public string FAV { get; set; }
}
