using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

[SugarTable("config_cable_systemNumber")]
public class config_cable_systemNumber
{
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	[Display(Name = "Id", AutoGenerateField = false)]
	public int Id { get; set; }

	/// <summary>
	/// 第一方系统号 
	///             </summary>
	[SugarColumn(ColumnName = "StartSystem")]
	[Display(Name = "起点系统号")]
	public string StartSystem { get; set; }

	/// <summary>
	/// 起点专业 
	///             </summary>
	[SugarColumn(ColumnName = "StartMajor")]
	[Display(Name = "起点专业")]
	public string StartMajor { get; set; }

	/// <summary>
	/// 第二方系统号 
	///             </summary>
	[SugarColumn(ColumnName = "EndSystem")]
	[Display(Name = "终点系统号")]
	public string EndSystem { get; set; }

	/// <summary>
	/// 终点专业 
	///             </summary>
	[SugarColumn(ColumnName = "EndMajor")]
	[Display(Name = "终点专业")]
	public string EndMajor { get; set; }

	/// <summary>
	/// 电缆系统号 
	///             </summary>
	[SugarColumn(ColumnName = "CableSystem")]
	[Display(Name = "电缆系统号")]
	public string CableSystem { get; set; }

	/// <summary>
	/// 起始编号 
	///             </summary>
	[Display(Name = "起始编号")]
	[SugarColumn(ColumnName = "MinNo")]
	public int MinNo { get; set; }

	/// <summary>
	/// 终止编号 
	///             </summary>
	[SugarColumn(ColumnName = "MaxNo")]
	[Display(Name = "终止编号")]
	public int MaxNo { get; set; }
}
