using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>
/// 接线点配置表
/// </summary>
[SugarTable("config_connection_points")]
public class config_connection_points
{
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	[Display(Name = "ID", AutoGenerateField = false)]
	public int Id { get; set; }

	[Display(Name = "端子板型号")]
	public string TerminalBoardModel { get; set; } = string.Empty;

	[Display(Name = "IO类型")]
	public string IoType { get; set; } = string.Empty;

	[Display(Name = "端子板编号")]
	public string TerminalBoardNumber { get; set; } = string.Empty;

	[Display(Name = "信号有效方式")]
	public string SignalEffectiveMode { get; set; } = string.Empty;

	[Display(Name = "供电方式")]
	public string PowerSupply { get; set; } = string.Empty;

	[Display(Name = "信号+（S+）")]
	public string SignalPlus { get; set; } = string.Empty;

	[Display(Name = "信号-（S-）")]
	public string SignalMinus { get; set; } = string.Empty;

	[Display(Name = "优先级")]
	public string Priority { get; set; } = string.Empty;
}
