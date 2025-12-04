using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>
/// 电缆类别和颜色
///             </summary>
[SugarTable("config_cable_categoryAndColor")]
public class config_cable_categoryAndColor
{
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	[Display(Name = "Id", AutoGenerateField = false)]
	public int Id { get; set; }

	/// <summary>
	/// 第一方安全分级分组 
	///             </summary>
	[SugarColumn(ColumnName = "FirstSafetyGroup")]
	[Display(Name = "起点安全分级")]
	public string FirstSafetyGroup { get; set; }

	/// <summary>
	/// 第二方安全分级/分组 
	///             </summary>
	[SugarColumn(ColumnName = "SecondSafetyGroup")]
	[Display(Name = "终点安全分级")]
	public string SecondSafetyGroup { get; set; }

	/// <summary>
	/// 线缆列别 
	///             </summary>
	[SugarColumn(ColumnName = "CableCategory")]
	[Display(Name = "线缆列别")]
	public string CableCategory { get; set; }

	/// <summary>
	/// 色标 
	///             </summary>
	[SugarColumn(ColumnName = "Color")]
	[Display(Name = "色标")]
	public string Color { get; set; }
}
