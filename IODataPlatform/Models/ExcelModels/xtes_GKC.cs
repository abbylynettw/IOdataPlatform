﻿using System.ComponentModel.DataAnnotations;

public class xtes_GKC
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
    public string IH { get; set; }

    [Display(Name = "SYS - 系统名")]
    public string SYS { get; set; }

    [Display(Name = "POT - 设备类型")]
    public string POT { get; set; }

    [Display(Name = "SN - 站号")]
    public string SN { get; set; }

    [Display(Name = "SUBNET - 子网")]
    public string SUBNET { get; set; }

    [Display(Name = "GROUPS - 变量分组")]
    public string GROUPS { get; set; }
}
