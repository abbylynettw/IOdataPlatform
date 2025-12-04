using System.ComponentModel.DataAnnotations;

using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>IO数据发布</summary>
public class publish_io {

    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    [Display(Name = "Id")]
    public int Id { get; set; }

    [Display(Name = "子项目Id")]
    public int SubProjectId { get; set; }

    [Display(Name = "发布版本")]
    public string PublishedVersion { get; set; } = string.Empty;

    [Display(Name = "发布原因")]
    public string PublishedReason { get; set; } = string.Empty;

    [Display(Name = "发布人")]
    public string Publisher { get; set; } = string.Empty;

    [Display(Name = "发布时间")]
    public DateTime PublishedTime { get; set; } = DateTime.Now;

}