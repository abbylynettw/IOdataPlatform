using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using IODataPlatform.Models.ExcelModels;

namespace IODataPlatform.Views.SubPages.Common;

/// <summary>
/// 网络接线清单对比结果（升版后的完整数据）
/// </summary>
/// <inheritdoc />
public class CableComparisonRow : ObservableObject
{
	[ObservableProperty]
	private bool showOldData;

	public CableData Data { get; set; } = new CableData();

	public CableData? OldData { get; set; }

	public ChangeType Type { get; set; }

	public List<string> ChangedFields { get; set; } = new List<string>();

	/// <summary>
	/// 是否被删除（用于显示删除线）
	/// </summary>
	public bool IsDeleted => Type == ChangeType.删除;

	/// <summary>
	/// 行背景色（仅新增和删除有背景色）
	/// </summary>
	public string BackgroundColor => Type switch
	{
		ChangeType.新增 => "#FFF9C4", 
		ChangeType.删除 => "#F5F5F5", 
		_ => "Transparent", 
	};

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.CableComparisonRow.showOldData" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool ShowOldData
	{
		get
		{
			return showOldData;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(showOldData, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ShowOldData);
				showOldData = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ShowOldData);
			}
		}
	}

	/// <summary>
	/// 判断某个字段是否发生变化
	/// </summary>
	public bool IsFieldChanged(string fieldName)
	{
		return ChangedFields.Contains(fieldName);
	}
}
