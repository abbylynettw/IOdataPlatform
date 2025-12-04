using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>
///
/// </summary>
[SugarTable("config_power_supply_method")]
public class config_power_supply_method
{
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	[Display(Name = "ID", AutoGenerateField = false)]
	public int Id { get; set; }

	[Display(Name = "供电类型")]
	public string SupplyType { get; set; } = string.Empty;

	[Display(Name = "板卡类型")]
	public string CardType { get; set; } = string.Empty;

	[Display(Name = "传感器类型")]
	public string SensorType { get; set; } = string.Empty;

	[Display(Name = "供电方式")]
	public string SupplyModel { get; set; } = string.Empty;
}
