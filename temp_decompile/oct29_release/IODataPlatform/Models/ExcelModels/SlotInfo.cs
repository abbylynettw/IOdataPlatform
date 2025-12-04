using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;

namespace IODataPlatform.Models.ExcelModels;

/// <summary>插槽</summary>
/// <inheritdoc />
public class SlotInfo : ObservableObject
{
	/// <summary>端子板，如插槽中没有端子板，则为null</summary>
	[ObservableProperty]
	private Board? board;

	/// <summary>序号</summary>
	public required int Index { get; set; }

	/// <summary>是否为虚拟插槽（用于存放未分配的板卡）</summary>
	public bool IsVirtual { get; set; }

	/// <inheritdoc cref="F:IODataPlatform.Models.ExcelModels.SlotInfo.board" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public Board? Board
	{
		get
		{
			return board;
		}
		set
		{
			if (!EqualityComparer<IODataPlatform.Models.ExcelModels.Board>.Default.Equals(board, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Board);
				board = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Board);
			}
		}
	}
}
