using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels;

/// <summary>
/// 机柜报警信息导入模型
/// </summary>
public class AlarmImportModel
{
    [Display(Name = "机柜名称")]
    public string 机柜名称 { get; set; } = string.Empty;

    [Display(Name = "机柜类型")]
    public string 机柜类型 { get; set; } = string.Empty;

    [Display(Name = "房间号")]
    public string 房间号 { get; set; } = string.Empty;

    [Display(Name = "温度（AI点）")]
    public int 温度 { get; set; }

    [Display(Name = "综合报警（DI点）")]
    public int 综合报警 { get; set; }

    [Display(Name = "机柜电源A故障")]
    public int 机柜电源A故障 { get; set; }

    [Display(Name = "机柜电源报警B故障")]
    public int 机柜电源B故障 { get; set; }

    [Display(Name = "机柜门开")]
    public int 机柜门开 { get; set; }

    [Display(Name = "机柜温度高报警")]
    public int 机柜温度高报警 { get; set; }

    [Display(Name = "风扇故障")]
    public int 风扇故障 { get; set; }

    [Display(Name = "网络故障")]
    public int 网络故障 { get; set; }

    [Display(Name = "RTU板卡位置")]
    public string RTU板卡位置 { get; set; } = string.Empty;

    [Display(Name = "RTU板卡所在机柜号")]
    public string RTU板卡所在机柜号 { get; set; } = string.Empty;
}
