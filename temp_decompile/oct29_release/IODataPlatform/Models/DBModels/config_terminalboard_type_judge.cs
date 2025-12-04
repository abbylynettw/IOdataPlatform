using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>
///
/// </summary>
[SugarTable("config_terminalboard_type_judge")]
public class config_terminalboard_type_judge
{
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	[Display(Name = "ID", AutoGenerateField = false)]
	public int Id { get; set; }

	[Display(Name = "板卡类型")]
	public string CardType { get; set; } = string.Empty;

	[Display(Name = "供电方式")]
	public string SignalType { get; set; } = string.Empty;

	[Display(Name = "信号特性")]
	public string SignalSpec { get; set; } = string.Empty;

	[Display(Name = "接线端子盒")]
	public string TerminalBlock { get; set; } = string.Empty;
}
