using System.ComponentModel.DataAnnotations;

using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary> 项目配置表-电缆系统（NC,DAS这些）
public class config_project_major
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]       
    public int Id { get; set; } = 0;

    [Display(Name = "项目Id")]
    public int ProjectId { get; set; }
    [Display(Name = "科室")]
    public Department Department { get; set; }
    [Display(Name = "所属专业")]
    public string CableSystem { get; set; }

    [Display(Name = "所属平台")]
    public ControlSystem ControlSystem { get; set; }
}
