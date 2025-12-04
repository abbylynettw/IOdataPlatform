using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;

namespace IODataPlatform.Utilities;

/// <summary>
/// 筛选选项
/// </summary>
/// <inheritdoc />
public class FilterOption : ObservableObject
{
	/// <summary>选项值</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor("DisplayText")]
	private string value = string.Empty;

	/// <summary>该选项的数量</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor("DisplayText")]
	private int count;

	/// <summary>是否选中</summary>
	[ObservableProperty]
	private bool isSelected = true;

	/// <summary>显示文本（值 + 数量）</summary>
	public string DisplayText => $"{Value} ({Count})";

	/// <inheritdoc cref="F:IODataPlatform.Utilities.FilterOption.value" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Value
	{
		get
		{
			return value;
		}
		[MemberNotNull("value")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(this.value, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Value);
				this.value = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DisplayText);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Utilities.FilterOption.count" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int Count
	{
		get
		{
			return count;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(count, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Count);
				count = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Count);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DisplayText);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Utilities.FilterOption.isSelected" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsSelected
	{
		get
		{
			return isSelected;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(isSelected, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsSelected);
				isSelected = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsSelected);
			}
		}
	}
}
