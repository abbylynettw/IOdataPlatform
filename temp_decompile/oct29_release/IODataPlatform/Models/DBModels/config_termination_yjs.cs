using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>
/// 硬件室典回资源库表
/// </summary>
[SugarTable("config_termination_yjs")]
public class config_termination_yjs
{
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	[Display(Name = "ID", AutoGenerateField = false)]
	public int Id { get; set; }

	/// <summary>
	/// 典回类型 
	/// </summary>
	[Display(Name = "典回类型")]
	[SugarColumn(ColumnName = "LoopType")]
	public string LoopType { get; set; }

	/// <summary>
	/// 版本 
	/// </summary>
	[Display(Name = "版本")]
	[SugarColumn(ColumnName = "Version")]
	public string Version { get; set; }

	/// <summary>
	/// 机组号 
	/// </summary>
	[Display(Name = "机组号")]
	[SugarColumn(ColumnName = "UnitNumber")]
	public string UnitNumber { get; set; }

	/// <summary>
	/// 系统号 
	/// </summary>
	[Display(Name = "系统号")]
	[SugarColumn(ColumnName = "SystemNumber")]
	public string SystemNumber { get; set; }

	/// <summary>
	/// 信号名称 
	/// </summary>
	[Display(Name = "信号名称")]
	[SugarColumn(ColumnName = "SignalName")]
	public string SignalName { get; set; }

	/// <summary>
	/// 扩展码 
	/// </summary>
	[Display(Name = "扩展码")]
	[SugarColumn(ColumnName = "ExtensionCode")]
	public string ExtensionCode { get; set; }

	/// <summary>
	/// 信号位置 
	/// </summary>
	[Display(Name = "信号位置")]
	[SugarColumn(ColumnName = "SignalLocation")]
	public string SignalLocation { get; set; }

	/// <summary>
	/// 设备类型 
	/// </summary>
	[Display(Name = "设备类型")]
	[SugarColumn(ColumnName = "DeviceType")]
	public string DeviceType { get; set; }

	/// <summary>
	/// 功能描述 
	/// </summary>
	[Display(Name = "功能描述")]
	[SugarColumn(ColumnName = "FunctionDescription")]
	public string FunctionDescription { get; set; }

	/// <summary>
	/// 安全等级 
	/// </summary>
	[Display(Name = "安全等级")]
	[SugarColumn(ColumnName = "SafetyLevel")]
	public string SafetyLevel { get; set; }

	/// <summary>
	/// 安全列 
	/// </summary>
	[Display(Name = "安全列")]
	[SugarColumn(ColumnName = "SafetyColumn")]
	public string SafetyColumn { get; set; }

	/// <summary>
	/// 应急供电 
	/// </summary>
	[Display(Name = "应急供电")]
	[SugarColumn(ColumnName = "EmergencyPowerSupply")]
	public string EmergencyPowerSupply { get; set; }

	/// <summary>
	/// PAMS 
	/// </summary>
	[Display(Name = "PAMS")]
	[SugarColumn(ColumnName = "PAMS")]
	public string PAMS { get; set; }

	/// <summary>
	/// 端子组名 
	/// </summary>
	[Display(Name = "端子组名")]
	[SugarColumn(ColumnName = "TerminalGroupName")]
	public string TerminalGroupName { get; set; }

	/// <summary>
	/// 端子号P 
	/// </summary>
	[Display(Name = "端子号(+)")]
	[SugarColumn(ColumnName = "TerminalNumberP")]
	public string TerminalNumberP { get; set; }

	/// <summary>
	/// 端子号N 
	/// </summary>
	[Display(Name = "端子号(-)")]
	[SugarColumn(ColumnName = "TerminalNumberN")]
	public string TerminalNumberN { get; set; }

	/// <summary>
	/// 终端 
	/// </summary>
	[Display(Name = "终端")]
	[SugarColumn(ColumnName = "Terminal")]
	public string Terminal { get; set; }

	/// <summary>
	/// IO类型 
	/// </summary>
	[Display(Name = "IO类型")]
	[SugarColumn(ColumnName = "IOType")]
	public string IOType { get; set; }

	/// <summary>
	/// 供电 
	/// </summary>
	[Display(Name = "供电")]
	[SugarColumn(ColumnName = "PowerSupply")]
	public string PowerSupply { get; set; }

	/// <summary>
	/// 回路电压 
	/// </summary>
	[Display(Name = "回路电压")]
	[SugarColumn(ColumnName = "LoopVoltage")]
	public string LoopVoltage { get; set; }

	/// <summary>
	/// 盘号 
	/// </summary>
	[Display(Name = "盘号")]
	[SugarColumn(ColumnName = "PanelNumber")]
	public string PanelNumber { get; set; }

	/// <summary>
	/// 横坐标 
	/// </summary>
	[Display(Name = "横坐标")]
	[SugarColumn(ColumnName = "XCoordinate")]
	public string XCoordinate { get; set; }

	/// <summary>
	/// 纵坐标 
	/// </summary>
	[Display(Name = "纵坐标")]
	[SugarColumn(ColumnName = "YCoordinate")]
	public string YCoordinate { get; set; }
}
