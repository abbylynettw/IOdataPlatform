using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;

namespace IODataPlatform.Models;

/// <summary>
/// 文档树节点模型（支持多级目录）
/// </summary>
/// <inheritdoc />
public class DocumentNode : ObservableObject
{
	/// <summary>
	/// 节点ID
	/// </summary>
	[ObservableProperty]
	private string id = Guid.NewGuid().ToString();

	/// <summary>
	/// 节点名称
	/// </summary>
	[ObservableProperty]
	private string name = string.Empty;

	/// <summary>
	/// 节点类型
	/// </summary>
	[ObservableProperty]
	private DocumentNodeType nodeType;

	/// <summary>
	/// Markdown内容（仅文档类型有效）
	/// </summary>
	[ObservableProperty]
	private string content = string.Empty;

	/// <summary>
	/// 父节点ID
	/// </summary>
	[ObservableProperty]
	private string? parentId;

	/// <summary>
	/// 排序序号
	/// </summary>
	[ObservableProperty]
	private int order;

	/// <summary>
	/// 是否展开
	/// </summary>
	[ObservableProperty]
	private bool isExpanded = true;

	/// <summary>
	/// 子节点列表
	/// </summary>
	[ObservableProperty]
	private ObservableCollection<DocumentNode> children = new ObservableCollection<DocumentNode>();

	/// <summary>
	/// 创建时间
	/// </summary>
	[ObservableProperty]
	private DateTime createTime = DateTime.Now;

	/// <summary>
	/// 最后修改时间
	/// </summary>
	[ObservableProperty]
	private DateTime lastModifyTime = DateTime.Now;

	/// <inheritdoc cref="F:IODataPlatform.Models.DocumentNode.id" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Id
	{
		get
		{
			return id;
		}
		[MemberNotNull("id")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(id, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Id);
				id = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Id);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DocumentNode.name" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Name
	{
		get
		{
			return name;
		}
		[MemberNotNull("name")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(name, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Name);
				name = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Name);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DocumentNode.nodeType" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public DocumentNodeType NodeType
	{
		get
		{
			return nodeType;
		}
		set
		{
			if (!EqualityComparer<DocumentNodeType>.Default.Equals(nodeType, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.NodeType);
				nodeType = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.NodeType);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DocumentNode.content" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Content
	{
		get
		{
			return content;
		}
		[MemberNotNull("content")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(content, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Content);
				content = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Content);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DocumentNode.parentId" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string? ParentId
	{
		get
		{
			return parentId;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(parentId, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ParentId);
				parentId = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ParentId);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DocumentNode.order" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int Order
	{
		get
		{
			return order;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(order, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Order);
				order = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Order);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DocumentNode.isExpanded" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsExpanded
	{
		get
		{
			return isExpanded;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(isExpanded, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsExpanded);
				isExpanded = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsExpanded);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DocumentNode.children" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<DocumentNode> Children
	{
		get
		{
			return children;
		}
		[MemberNotNull("children")]
		set
		{
			if (!EqualityComparer<ObservableCollection<DocumentNode>>.Default.Equals(children, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Children);
				children = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Children);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DocumentNode.createTime" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public DateTime CreateTime
	{
		get
		{
			return createTime;
		}
		set
		{
			if (!EqualityComparer<DateTime>.Default.Equals(createTime, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.CreateTime);
				createTime = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.CreateTime);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DocumentNode.lastModifyTime" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public DateTime LastModifyTime
	{
		get
		{
			return lastModifyTime;
		}
		set
		{
			if (!EqualityComparer<DateTime>.Default.Equals(lastModifyTime, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.LastModifyTime);
				lastModifyTime = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.LastModifyTime);
			}
		}
	}
}
