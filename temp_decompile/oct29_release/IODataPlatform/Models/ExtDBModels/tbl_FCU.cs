using SqlSugar;

namespace IODataPlatform.Models.ExtDBModels;

/// <summary>
///
/// </summary>
public class tbl_FCU
{
	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	[SugarColumn(ColumnName = "Cabinet")]
	public string Cabinet { get; set; }

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	[SugarColumn(ColumnName = "Description")]
	public string Description { get; set; }

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:False
	/// </summary>           
	[SugarColumn(IsPrimaryKey = true, ColumnName = "DF")]
	public string DF { get; set; }

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	[SugarColumn(ColumnName = "Domain")]
	public string Domain { get; set; }

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:False
	/// </summary>           
	[SugarColumn(ColumnName = "FCU")]
	public string FCU { get; set; }

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	[SugarColumn(ColumnName = "FCU_Name")]
	public string FCU_Name { get; set; }

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	[SugarColumn(ColumnName = "IO_Connect_Address")]
	public string IO_Connect_Address { get; set; }
}
