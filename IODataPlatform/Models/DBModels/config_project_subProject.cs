using System.ComponentModel.DataAnnotations;

using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>Io子项目，除系统二室外，其他科室每个项目生成一个默认子项目</summary>
public class config_project_subProject
{

    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    [Display(Name = "Id")]
    public int Id { get; set; } 

    [Display(Name = "专业Id")]
    public int MajorId { get; set; }

    [Display(Name = "子项目名称")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "创建者Id")]
    public int CreatorUserId { get; set; }

    [SugarColumn(IsIgnore = true)]
    [Display(Name = "创建者")]
    public string CreatorName { get; private set; } = string.Empty; // 只读属性

    // 更新创建者名称的方法
    public void UpdateCreatorName(string? name)
    {
        // 如果 name 是 null 或者是空白字符串，赋值为 "未知"
        CreatorName = !string.IsNullOrWhiteSpace(name) ? name : "未知";
    }

}