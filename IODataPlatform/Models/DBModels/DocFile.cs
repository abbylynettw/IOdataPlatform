using System;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>
/// 文档文件表
/// </summary>
[SugarTable("doc_file")]
public class DocFile
{
	/// <summary>
	/// 主键ID
	/// </summary>
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	public long Id { get; set; }

	/// <summary>
	/// 文档标题
	/// </summary>
	public string Title { get; set; } = string.Empty;

	/// <summary>
	/// 所属目录ID
	/// </summary>
	public long CategoryId { get; set; }

	/// <summary>
	/// 文件类型：markdown, word, excel, pdf
	/// </summary>
	public string FileType { get; set; } = "markdown";

	/// <summary>
	/// 文档内容（Markdown格式）
	/// </summary>
	[SugarColumn(ColumnDataType = "NVARCHAR(MAX)")]
	public string Content { get; set; } = string.Empty;

	/// <summary>
	/// 文件在服务器上的相对路径
	/// </summary>
	public string FilePath { get; set; } = string.Empty;

	/// <summary>
	/// 排序序号
	/// </summary>
	public int SortOrder { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// 更新时间
	/// </summary>
	public DateTime UpdatedAt { get; set; }
}
