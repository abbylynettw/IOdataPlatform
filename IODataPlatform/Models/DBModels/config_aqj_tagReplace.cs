
using System.ComponentModel.DataAnnotations;
using SqlSugar;
[SugarTable("config_aqj_tagReplace")]
public class config_aqj_tagReplace
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
    /// 替换前（IO点名）
    /// </summary>
    [Display(Name = "IO点名替换前")]
    public string IoPointNameBefore { get; set; }

    /// <summary>
    /// 替换后（IO点名）
    /// </summary>
    [Display(Name = "IO点名替换后")]
    public string IoPointNameAfter { get; set; }
}