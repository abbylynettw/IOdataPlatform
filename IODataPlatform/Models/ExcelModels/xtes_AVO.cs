﻿﻿using System.ComponentModel.DataAnnotations;

public class xtes_AVO
{
    [Display(Name = "CHN - 通道号")]
    public string CHN { get; set; }

    [Display(Name = "PN - 点名")]
    public string PN { get; set; }

    [Display(Name = "DESC - 点描述")]
    public string DESC { get; set; }

    [Display(Name = "UNIT - 单位")]
    public string UNIT { get; set; }

    [Display(Name = "MU - 量程上限")]
    public double MU { get; set; }

    [Display(Name = "MD - 量程下限")]
    public double MD { get; set; }

    [Display(Name = "TRAIN - 安全列")]
    public string TRAIN { get; set; }

    [Display(Name = "RG - 关联画面")]
    public string RG { get; set; }

    [Display(Name = "IH - 是否进历史库")]
    public string IH { get; set; }

    [Display(Name = "SYS - 系统名")]
    public string SYS { get; set; }

    [Display(Name = "CMPDEV - 压缩偏差百分比")]
    public string CMPDEV { get; set; }

    [Display(Name = "SUBNET - 子网")]
    public string SUBNET { get; set; }

    [Display(Name = "CLN - IO卡槽位地址号")]
    public string CLN { get; set; }

    [Display(Name = "MON - IO卡名")]
    public string MON { get; set; }

    [Display(Name = "SN - 站号")]
    public string SN { get; set; }

    [Display(Name = "OF - 显示格式")]
    public string OF { get; set; }

    [Display(Name = "TP - 信号类型")]
    public string TP { get; set; }

    [Display(Name = "FAVTYPE - 执行方式")]
    public string FAVTYPE { get; set; }

    [Display(Name = "FAV - 故障安全输出值")]
    public string FAV { get; set; }

    [Display(Name = "ISOF - 供电方式")]
    public string ISOF { get; set; }

    [Display(Name = "GROUPS - 变量分组")]
    public string GROUPS { get; set; }

    [Display(Name = "BAUDRATE - 波特率")]
    public string BAUDRATE { get; set; }

    [Display(Name = "ROUT - 是否反量程输出")]
    public string ROUT { get; set; }
}
