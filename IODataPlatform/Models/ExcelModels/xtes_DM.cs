using System.ComponentModel.DataAnnotations;

public class xtes_DM
{
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

    [Display(Name = "SN - 站号")]
    public string SN { get; set; }

    [Display(Name = "QFM - 输出信号替代方式")]
    public string QFM { get; set; }

    [Display(Name = "QFID - 输出替代值")]
    public string QFID { get; set; }

    [Display(Name = "IC - 是否产生仪控故障")]
    public string IC { get; set; }

    [Display(Name = "E1 - 置1说明")]
    public string E1 { get; set; }

    [Display(Name = "E0 - 置0说明")]
    public string E0 { get; set; }

    [Display(Name = "HIGH - 是否高速上传")]
    public string HIGH { get; set; }

    [Display(Name = "GROUPS - 变量分组")]
    public string GROUPS { get; set; }

    [Display(Name = "ALLOCATION - 报警分组")]
    public string ALLOCATION { get; set; }

    [Display(Name = "ACUT - 一层是否报警切除")]
    public string ACUT { get; set; }

    [Display(Name = "AP - 是否判断报警")]
    public string AP { get; set; }
}
