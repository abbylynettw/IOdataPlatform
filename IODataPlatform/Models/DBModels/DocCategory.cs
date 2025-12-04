using System;
using System.Collections.ObjectModel;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>
/// 文档目录表
/// </summary>
[SugarTable("doc_category")]
public class DocCategory
{
	/// <summary>
	/// 主键ID
	/// </summary>
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	public long Id { get; set; }

	/// <summary>
	/// 文件夹名称
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// 父目录ID（null表示根目录）
	/// </summary>
	[SugarColumn(IsNullable = true)]
	public long? ParentId { get; set; }

	/// <summary>
	/// 排序序号
	/// </summary>
	public int SortOrder { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// 子节点列表（非数据库字段）
	/// </summary>
	[SugarColumn(IsIgnore = true)]
	public ObservableCollection<DocCategory> Children { get; set; } = new ObservableCollection<DocCategory>();

	/// <summary>
	/// 是否展开（非数据库字段）
	/// </summary>
	[SugarColumn(IsIgnore = true)]
	public bool IsExpanded { get; set; } = true;
}
