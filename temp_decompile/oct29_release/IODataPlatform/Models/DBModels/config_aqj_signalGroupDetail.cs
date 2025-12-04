using System.ComponentModel.DataAnnotations;
using IODataPlatform.Models.ExcelModels;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

[SugarTable("config_aqj_signalGroupDetail")]
public class config_aqj_signalGroupDetail
{
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	[Display(Name = "Id", AutoGenerateField = false)]
	public int Id { get; set; }

	[Display(Name = "Signal Group Id")]
	public int signalGroupId { get; set; }

	[Display(Name = "序号")]
	public string 序号 { get; set; } = string.Empty;

	[Display(Name = "信号名称")]
	public string 信号名称 { get; set; } = string.Empty;

	[Display(Name = "信号说明")]
	public string 信号说明 { get; set; } = string.Empty;

	[Display(Name = "原变量名")]
	public string 原变量名 { get; set; } = string.Empty;

	[Display(Name = "控制站")]
	public string 控制站 { get; set; } = string.Empty;

	[Display(Name = "机柜号")]
	public string 机柜号 { get; set; } = string.Empty;

	[Display(Name = "机箱号")]
	public string 机箱号 { get; set; } = string.Empty;

	[Display(Name = "槽位号")]
	public string 槽位号 { get; set; } = string.Empty;

	[Display(Name = "通道号")]
	public string 通道号 { get; set; } = string.Empty;

	[Display(Name = "安全分级")]
	public string 安全分级 { get; set; } = string.Empty;

	[Display(Name = "I/O类型")]
	public string IO类型 { get; set; } = string.Empty;

	[Display(Name = "前卡件类型")]
	public string 前卡件类型 { get; set; } = string.Empty;

	[Display(Name = "后卡件类型")]
	public string 后卡件类型 { get; set; } = string.Empty;

	[Display(Name = "端子板类型")]
	public string 端子板类型 { get; set; } = string.Empty;

	[Display(Name = "端子板编号")]
	public string 端子板编号 { get; set; } = string.Empty;

	[Display(Name = "端子号1")]
	public string 端子号1 { get; set; } = string.Empty;

	[Display(Name = "端子号2")]
	public string 端子号2 { get; set; } = string.Empty;

	[Display(Name = "刻度类型")]
	public string 刻度类型 { get; set; } = string.Empty;

	[Display(Name = "信号特性")]
	public string 信号特性 { get; set; } = string.Empty;

	[Display(Name = "量程下限")]
	public string 量程下限 { get; set; } = string.Empty;

	[Display(Name = "量程上限")]
	public string 量程上限 { get; set; } = string.Empty;

	[Display(Name = "超量程下限")]
	public string 超量程下限 { get; set; } = string.Empty;

	[Display(Name = "超量程上限")]
	public string 超量程上限 { get; set; } = string.Empty;

	[Display(Name = "单位")]
	public string 单位 { get; set; } = string.Empty;

	[Display(Name = "缺省值")]
	public string 缺省值 { get; set; } = string.Empty;

	[Display(Name = "回路供电")]
	public string 回路供电 { get; set; } = string.Empty;

	[Display(Name = "电压等级")]
	public string 电压等级 { get; set; } = string.Empty;

	[Display(Name = "内部/外部")]
	public string 内部外部 { get; set; } = string.Empty;

	[Display(Name = "典型回路图")]
	public string 典型回路图 { get; set; } = string.Empty;

	[Display(Name = "源头/目的")]
	public string 源头目的 { get; set; } = string.Empty;

	[Display(Name = "是否隔离")]
	public string 是否隔离 { get; set; } = string.Empty;

	[Display(Name = "FD/SAMA页码")]
	public string FDSAMA页码 { get; set; } = string.Empty;

	[Display(Name = "版本")]
	public string 版本 { get; set; } = string.Empty;

	[Display(Name = "传感器类型")]
	public string 传感器类型 { get; set; } = string.Empty;

	[Display(Name = "分配信息")]
	public string 分配信息 { get; set; } = string.Empty;

	[Display(Name = "备注")]
	public string 备注 { get; set; } = string.Empty;

	[Display(Name = "点类型")]
	[SugarColumn(IsIgnore = true)]
	public TagType TagType { get; set; }
}
