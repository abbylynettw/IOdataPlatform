using SqlSugar;

using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.DBModels;

///<summary>
///
///</summary>
[SugarTable("config_card_type_judge")]
public partial class config_card_type_judge {

    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    [Display(Name = "Id", AutoGenerateField = false)]
    public int Id { get; set; } = 0;

    [Display(Name = "IO类型")]
    public string IoType { get; set; } = string.Empty;

    [Display(Name = "信号特性")]
    public string SignalSpec { get; set; } = string.Empty;

    [Display(Name = "供电类型")]
    public string PowerType { get; set; } = string.Empty;

    [Display(Name = "IO卡型号")]
    public string IoCardType { get; set; } = string.Empty;

    [Display(Name = "通道数量")]
    public int PinsCount { get; set; } = 0;

}