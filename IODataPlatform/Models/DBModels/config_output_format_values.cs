using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.DBModels;

///<summary>
///
///</summary>
[SugarTable("config_output_format_values")]
public partial class config_output_format_values
{
    [Display(Name = "ID", AutoGenerateField = false)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [Display(Name = "量程下限")]
    public double RangeLow { get; set; } 

    [Display(Name = "量程上限")]
    public double RangeHigh { get; set; } 

    [Display(Name = "量程单位")]
    public string RangeUnit { get; set; } = string.Empty;

    [Display(Name = "小数点位数")]
    public string DecimalPlaces { get; set; } = string.Empty;

}
