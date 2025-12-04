using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

[SugarTable("config_cable_function")]
public class config_cable_function
{
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	[Display(Name = "Id", AutoGenerateField = false)]
	public int Id { get; set; }

	/// <summary>
	/// 第二方IO类型 
	///             </summary>
	[SugarColumn(ColumnName = "FirstIOType")]
	[Display(Name = "起点IO类型")]
	public string FirstIOType { get; set; }

	/// <summary>
	/// 第二方IO类型 
	///             </summary>
	[SugarColumn(ColumnName = "SecondIOType")]
	[Display(Name = "终点IO类型")]
	public string SecondIOType { get; set; }

	/// <summary>
	/// 电缆IO类型 
	///             </summary>
	[SugarColumn(ColumnName = "CableIOType")]
	[Display(Name = "电缆IO类型")]
	public string CableIOType { get; set; }
}
