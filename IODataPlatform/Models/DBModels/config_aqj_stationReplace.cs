using System.ComponentModel.DataAnnotations;
using SqlSugar;

[SugarTable("config_aqj_stationReplace")]
public class config_aqj_stationReplace
{
    /// <summary>
    /// 自增Id
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    [Display(Name = "Id", AutoGenerateField = false)]
    public int Id { get; set; } = 0;

    /// <summary>
    /// 控制站
    /// </summary>
    [Display(Name = "控制站")]
    public string ControlStation { get; set; }

    /// <summary>
    /// 替换前（控制站）
    /// </summary>
    [Display(Name = "控制站替换前")]
    public string ControlStationBefore { get; set; }

    /// <summary>
    /// 替换后（控制站）
    /// </summary>
    [Display(Name = "控制站替换后")]
    public string ControlStationAfter { get; set; }

}
