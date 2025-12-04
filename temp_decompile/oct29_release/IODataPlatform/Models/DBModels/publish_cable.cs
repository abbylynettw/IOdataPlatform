using System;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

public class publish_cable
{
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	[Display(Name = "Id")]
	public int Id { get; set; }

	[Display(Name = "子项目Id1")]
	public int SubProjectId1 { get; set; }

	[Display(Name = "子项目Id2")]
	public int SubProjectId2 { get; set; }

	[Display(Name = "发布版本")]
	public string PublishedVersion { get; set; } = string.Empty;

	[Display(Name = "发布原因")]
	public string PublishedReason { get; set; } = string.Empty;

	[Display(Name = "发布人")]
	public string Publisher { get; set; } = string.Empty;

	[Display(Name = "发布时间")]
	public DateTime PublishedTime { get; set; } = DateTime.Now;
}
