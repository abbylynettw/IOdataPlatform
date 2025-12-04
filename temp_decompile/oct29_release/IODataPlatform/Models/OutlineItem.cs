using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;

namespace IODataPlatform.Models;

/// <summary>
/// 文档大纲项（标题）
/// </summary>
/// <inheritdoc />
public class OutlineItem : ObservableObject
{
	/// <summary>
	/// 锤点ID（用于跳转）
	/// </summary>
	[ObservableProperty]
	private string id = string.Empty;

	/// <summary>
	/// 标题文本
	/// </summary>
	[ObservableProperty]
	private string text = string.Empty;

	/// <summary>
	/// 标题层级（1-6）
	/// </summary>
	[ObservableProperty]
	private int level = 1;

	/// <summary>
	/// 字体大小（根据层级动态计算）
	/// </summary>
	public double FontSize => Level switch
	{
		1 => 13.0, 
		2 => 12.5, 
		3 => 12.0, 
		_ => 11.5, 
	};

	/// <summary>
	/// 缩进（根据层级动态计算）
	/// </summary>
	public Thickness Indent => new Thickness((Level - 1) * 12, 2.0, 2.0, 2.0);

	/// <inheritdoc cref="F:IODataPlatform.Models.OutlineItem.id" />
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

	/// <inheritdoc cref="F:IODataPlatform.Models.OutlineItem.text" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Text
	{
		get
		{
			return text;
		}
		[MemberNotNull("text")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(text, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Text);
				text = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Text);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.OutlineItem.level" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int Level
	{
		get
		{
			return level;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(level, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Level);
				level = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Level);
			}
		}
	}
}
