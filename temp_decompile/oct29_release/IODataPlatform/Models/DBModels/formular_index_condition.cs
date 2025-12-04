using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <inheritdoc />
public class formular_index_condition : ObservableObject
{
	[ObservableProperty]
	private int index;

	[ObservableProperty]
	private string fieldName = string.Empty;

	[ObservableProperty]
	private string propertyName = string.Empty;

	[ObservableProperty]
	private string fieldValue = string.Empty;

	[ObservableProperty]
	private FieldOperator fieldOperator;

	[ObservableProperty]
	private ConditionOperator conditionOperator;

	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	public int Id { get; set; }

	public int IndexId { get; set; }

	public string Description { get; set; } = string.Empty;

	/// <inheritdoc cref="F:IODataPlatform.Models.DBModels.formular_index_condition.index" />
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

	/// <inheritdoc cref="F:IODataPlatform.Models.DBModels.formular_index_condition.fieldName" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string FieldName
	{
		get
		{
			return fieldName;
		}
		[MemberNotNull("fieldName")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(fieldName, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.FieldName);
				fieldName = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.FieldName);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DBModels.formular_index_condition.propertyName" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string PropertyName
	{
		get
		{
			return propertyName;
		}
		[MemberNotNull("propertyName")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(propertyName, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PropertyName);
				propertyName = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PropertyName);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DBModels.formular_index_condition.fieldValue" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string FieldValue
	{
		get
		{
			return fieldValue;
		}
		[MemberNotNull("fieldValue")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(fieldValue, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.FieldValue);
				fieldValue = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.FieldValue);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DBModels.formular_index_condition.fieldOperator" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public FieldOperator FieldOperator
	{
		get
		{
			return fieldOperator;
		}
		set
		{
			if (!EqualityComparer<FieldOperator>.Default.Equals(fieldOperator, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.FieldOperator);
				fieldOperator = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.FieldOperator);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.DBModels.formular_index_condition.conditionOperator" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ConditionOperator ConditionOperator
	{
		get
		{
			return conditionOperator;
		}
		set
		{
			if (!EqualityComparer<ConditionOperator>.Default.Equals(conditionOperator, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ConditionOperator);
				conditionOperator = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ConditionOperator);
			}
		}
	}
}
