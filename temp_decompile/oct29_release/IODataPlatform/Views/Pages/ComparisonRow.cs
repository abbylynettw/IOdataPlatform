using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;

namespace IODataPlatform.Views.Pages;

/// <inheritdoc />
public class ComparisonRow : ObservableObject
{
	[ObservableProperty]
	private bool showOldRow;

	public string Key { get; set; } = string.Empty;

	public DataRow CurrentRow { get; set; }

	public DataRow OldRow { get; set; }

	public ComparisonType Type { get; set; }

	public Dictionary<string, bool> ChangedFields { get; set; } = new Dictionary<string, bool>();

	public string BackgroundColor => Type switch
	{
		ComparisonType.Added => "#FFF9C4", 
		ComparisonType.Deleted => "#F5F5F5", 
		ComparisonType.Modified => "#FFCDD2", 
		ComparisonType.Unchanged => "Transparent", 
		_ => "Transparent", 
	};

	public string ForegroundColor
	{
		get
		{
			ComparisonType type = Type;
			if (type == ComparisonType.Deleted)
			{
				return "#9E9E9E";
			}
			return "#000000";
		}
	}

	public bool IsDeleted => Type == ComparisonType.Deleted;

	public IList<KeyValuePair<string, string>> OldRowColumns
	{
		get
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			if (OldRow != null)
			{
				foreach (DataColumn column in OldRow.Table.Columns)
				{
					string value = OldRow[column.ColumnName]?.ToString() ?? "";
					list.Add(new KeyValuePair<string, string>(column.ColumnName, value));
				}
			}
			return list;
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.ComparisonRow.showOldRow" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool ShowOldRow
	{
		get
		{
			return showOldRow;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(showOldRow, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ShowOldRow);
				showOldRow = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ShowOldRow);
			}
		}
	}
}
