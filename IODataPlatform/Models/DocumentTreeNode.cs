using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using IODataPlatform.Models.DBModels;
using Wpf.Ui.Controls;

namespace IODataPlatform.Models;

/// <summary>
/// 文档树节点（文件夹或文件）
/// </summary>
/// <inheritdoc />
public partial class DocumentTreeNode : ObservableObject
{
	/// <summary>
	/// 显示名称
	/// </summary>
	[ObservableProperty]
	private string _name = string.Empty;

	/// <summary>
	/// 节点类型
	/// </summary>
	public DocumentNodeType NodeType { get; set; }

	/// <summary>
	/// 图标
	/// </summary>
	public SymbolRegular Icon
	{
		get
		{
			if (NodeType != DocumentNodeType.Folder)
			{
				return SymbolRegular.Document16;
			}
			return SymbolRegular.Folder16;
		}
	}

	/// <summary>
	/// 文件夹数据（如果是文件夹）
	/// </summary>
	public DocCategory? Category { get; set; }

	/// <summary>
	/// 文件数据（如果是文件）
	/// </summary>
	public DocFile? File { get; set; }

	/// <summary>
	/// 子节点
	/// </summary>
	public ObservableCollection<DocumentTreeNode> Children { get; set; } = new ObservableCollection<DocumentTreeNode>();

	/// <summary>
	/// 是否展开
	/// </summary>
	public bool IsExpanded { get; set; } = true;
}
