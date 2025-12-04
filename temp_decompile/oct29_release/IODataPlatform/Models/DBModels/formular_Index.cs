using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <inheritdoc />
public class formular_Index : ObservableObject
{
	[ObservableProperty]
	private int index;

	[ObservableProperty]
	private string returnValue;

	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	public int Id { get; set; }

	public int FormulaId { get; set; }

	public string Description { get; set; } = string.Empty;

	/// <inheritdoc cref="F:IODataPlatform.Models.DBModels.formular_Index.index" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int Index
	{
		get
		{
			return index;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(index, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Index);
				index = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Index);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DBModels.formular_Index.returnValue" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string ReturnValue
	{
		get
		{
			return returnValue;
		}
		[MemberNotNull("returnValue")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(returnValue, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ReturnValue);
				returnValue = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ReturnValue);
			}
		}
	}
}
