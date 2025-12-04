using System;
using System.Collections.Generic;

namespace IODataPlatform.Models.ExportModels;

/// <summary>
/// 导出配置数据模型
/// </summary>
public class ExportConfig
{
	/// <summary>
	/// 配置名称
	/// </summary>
	public string ConfigName { get; set; } = string.Empty;

	/// <summary>
	/// 导出类型
	/// </summary>
	public ExportType Type { get; set; }

	/// <summary>
	/// 列顺序配置
	/// </summary>
	public List<ColumnInfo> ColumnOrder { get; set; } = new List<ColumnInfo>();

	/// <summary>
	/// 选中的字段（仅发布清单时有效）
	/// </summary>
	public List<string> SelectedFields { get; set; } = new List<string>();

	/// <summary>
	/// 创建时间
	/// </summary>
	public DateTime CreatedTime { get; set; } = DateTime.Now;

	/// <summary>
	/// 最后修改时间
	/// </summary>
	public DateTime LastModified { get; set; } = DateTime.Now;

	/// <summary>
	/// 是否为系统预设配置
	/// </summary>
	public bool IsSystemDefault { get; set; }
}
