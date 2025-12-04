using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;

namespace IODataPlatform.Models.ExcelModels;

/// <summary>通道</summary>
/// <inheritdoc />
public class Xt2Channel : ObservableObject
{
	/// <summary>点，如通道上没有点，则为null</summary>
	[ObservableProperty]
	private IoFullData? point;

	/// <summary>序号</summary>
	public required int Index { get; set; }

	/// <inheritdoc cref="F:IODataPlatform.Models.ExcelModels.Xt2Channel.point" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IoFullData? Point
	{
		get
		{
			return point;
		}
		set
		{
			if (!EqualityComparer<IoFullData>.Default.Equals(point, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Point);
				point = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Point);
			}
		}
	}
}
