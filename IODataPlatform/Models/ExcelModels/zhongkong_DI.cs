using System.ComponentModel.DataAnnotations;

/// <summary>
/// 中控系统DI点（开关量输入）
/// </summary>
public class xtes_zhongkong_DI
{
    [Display(Name = "名称")]
    public string 名称 { get; set; }

    [Display(Name = "描述")]
    public string 描述 { get; set; }

    [Display(Name = "控制站地址")]
    public string 控制站地址 { get; set; }

    [Display(Name = "地址")]
    public string 地址 { get; set; }

    [Display(Name = "ON描述")]
    public string ON描述 { get; set; } = "ON";

    [Display(Name = "OFF描述")]
    public string OFF描述 { get; set; } = "OFF";

    [Display(Name = "位号类型")]
    public string 位号类型 { get; set; } = "常规DI位号";

    [Display(Name = "模块类型")]
    public string 模块类型 { get; set; } = "DI711-S 数字信号输入模块（16路，24V）";

    [Display(Name = "位号运行周期")]
    public string 位号运行周期 { get; set; } = "基本扫描周期";

    [Display(Name = "输入取反")]
    public string 输入取反 { get; set; } = "禁止";

    [Display(Name = "ON状态报警")]
    public string ON状态报警 { get; set; } = "禁止";

    [Display(Name = "ON状态报警等级")]
    public string ON状态报警等级 { get; set; } = "低";

    [Display(Name = "OFF状态报警")]
    public string OFF状态报警 { get; set; } = "禁止";

    [Display(Name = "OFF状态报警等级")]
    public string OFF状态报警等级 { get; set; } = "低";

    [Display(Name = "正跳变报警")]
    public string 正跳变报警 { get; set; } = "禁止";

    [Display(Name = "正跳变报警等级")]
    public string 正跳变报警等级 { get; set; } = "低";

    [Display(Name = "负跳变报警")]
    public string 负跳变报警 { get; set; } = "禁止";

    [Display(Name = "负跳变报警等级")]
    public string 负跳变报警等级 { get; set; } = "低";

    [Display(Name = "故障报警")]
    public string 故障报警 { get; set; } = "使能";

    [Display(Name = "故障报警等级")]
    public string 故障报警等级 { get; set; } = "低";

    [Display(Name = "故障处理")]
    public string 故障处理 { get; set; } = "保持";

    [Display(Name = "位号分组")]
    public string 位号分组 { get; set; } = "位号分组 0";

    [Display(Name = "位号等级")]
    public string 位号等级 { get; set; } = "0级";

    [Display(Name = "面板")]
    public string 面板 { get; set; }

    [Display(Name = "SOE硬点标记")]
    public string SOE硬点标记 { get; set; } = "0";

    [Display(Name = "SOE标志")]
    public string SOE标志 { get; set; } = "否";

    [Display(Name = "SOE描述")]
    public string SOE描述 { get; set; } = "";

    [Display(Name = "SOE设备组")]
    public string SOE设备组 { get; set; } = "";
}
