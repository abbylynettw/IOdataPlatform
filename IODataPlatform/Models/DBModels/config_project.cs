using System.ComponentModel.DataAnnotations;

using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>项目</summary>
public class config_project {

    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    [Display(Name = "Id")]
    public int Id { get; set; }

    [Display(Name = "名称")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "备注")]
    public string Note { get; set; } = string.Empty;
    
    [Display(Name = "创建日期")]
    public DateTime Creation { get; set; } = DateTime.Now;

}