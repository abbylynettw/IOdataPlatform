using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Cells;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class DataCompareViewModel : ObservableObject, INavigationAware
{
	private readonly IPickerService picker;

	private readonly ExcelService excel;

	private readonly GlobalModel model;

	[ObservableProperty]
	private string filePath1 = "请选择Excel文件1";

	[ObservableProperty]
	private string filePath2 = "请选择Excel文件2";

	[ObservableProperty]
	private string fieldName = string.Empty;

	[ObservableProperty]
	private ObservableCollection<string> sheetNames1 = new ObservableCollection<string>();

	[ObservableProperty]
	private ObservableCollection<string> sheetNames2 = new ObservableCollection<string>();

	[ObservableProperty]
	private string selectedSheet1 = string.Empty;

	[ObservableProperty]
	private string selectedSheet2 = string.Empty;

	[ObservableProperty]
	private ObservableCollection<ComparisonRow> comparisonResults = new ObservableCollection<ComparisonRow>();

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DataCompareViewModel.PickFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<string>? pickFileCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DataCompareViewModel.ExportResultsCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? exportResultsCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataCompareViewModel.filePath1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string FilePath1
	{
		get
		{
			return filePath1;
		}
		[MemberNotNull("filePath1")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(filePath1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.FilePath1);
				filePath1 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.FilePath1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataCompareViewModel.filePath2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string FilePath2
	{
		get
		{
			return filePath2;
		}
		[MemberNotNull("filePath2")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(filePath2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.FilePath2);
				filePath2 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.FilePath2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataCompareViewModel.fieldName" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataCompareViewModel.sheetNames1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<string> SheetNames1
	{
		get
		{
			return sheetNames1;
		}
		[MemberNotNull("sheetNames1")]
		set
		{
			if (!EqualityComparer<ObservableCollection<string>>.Default.Equals(sheetNames1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SheetNames1);
				sheetNames1 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SheetNames1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataCompareViewModel.sheetNames2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<string> SheetNames2
	{
		get
		{
			return sheetNames2;
		}
		[MemberNotNull("sheetNames2")]
		set
		{
			if (!EqualityComparer<ObservableCollection<string>>.Default.Equals(sheetNames2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SheetNames2);
				sheetNames2 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SheetNames2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataCompareViewModel.selectedSheet1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SelectedSheet1
	{
		get
		{
			return selectedSheet1;
		}
		[MemberNotNull("selectedSheet1")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(selectedSheet1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SelectedSheet1);
				selectedSheet1 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SelectedSheet1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataCompareViewModel.selectedSheet2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SelectedSheet2
	{
		get
		{
			return selectedSheet2;
		}
		[MemberNotNull("selectedSheet2")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(selectedSheet2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SelectedSheet2);
				selectedSheet2 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SelectedSheet2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataCompareViewModel.comparisonResults" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<ComparisonRow> ComparisonResults
	{
		get
		{
			return comparisonResults;
		}
		[MemberNotNull("comparisonResults")]
		set
		{
			if (!EqualityComparer<ObservableCollection<ComparisonRow>>.Default.Equals(comparisonResults, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ComparisonResults);
				comparisonResults = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ComparisonResults);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DataCompareViewModel.PickFile(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<string> PickFileCommand => pickFileCommand ?? (pickFileCommand = new AsyncRelayCommand<string>(PickFile));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DataCompareViewModel.ExportResults" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ExportResultsCommand => exportResultsCommand ?? (exportResultsCommand = new AsyncRelayCommand(ExportResults));

	public DataCompareViewModel(IPickerService picker, ExcelService excel, GlobalModel model)
	{
		this.picker = picker;
		this.excel = excel;
		this.model = model;
	}

	[RelayCommand]
	private async Task PickFile(string parameter)
	{
		string text = picker.OpenFile("Excel文件|*.xlsx;*.xls");
		if (text == null)
		{
			return;
		}
		try
		{
			if (parameter == "1")
			{
				FilePath1 = text;
				List<string> list = await excel.GetSheetNamesAsync(text);
				SheetNames1.Clear();
				foreach (string item in list)
				{
					SheetNames1.Add(item);
				}
				if (SheetNames1.Count > 0)
				{
					SelectedSheet1 = SheetNames1[0];
				}
			}
			else
			{
				if (!(parameter == "2"))
				{
					return;
				}
				FilePath2 = text;
				List<string> list2 = await excel.GetSheetNamesAsync(text);
				SheetNames2.Clear();
				foreach (string item2 in list2)
				{
					SheetNames2.Add(item2);
				}
				if (SheetNames2.Count > 0)
				{
					SelectedSheet2 = SheetNames2[0];
				}
			}
		}
		catch (Exception ex)
		{
			model.Status.Error("读取文件失败：" + ex.Message);
		}
	}

	[RelayCommand]
	private async Task ExportResults()
	{
		if (ComparisonResults.Count == 0)
		{
			model.Status.Error("没有可导出的数据");
			return;
		}
		try
		{
			string savePath = picker.SaveFile("Excel文件|*.xlsx", $"数据对比结果_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
			if (string.IsNullOrEmpty(savePath))
			{
				return;
			}
			model.Status.Busy("正在导出数据...");
			await Task.Run(delegate
			{
				Workbook workbook = excel.GetWorkbook();
				Worksheet worksheet = workbook.Worksheets[0];
				worksheet.Name = "对比结果";
				List<string> list = new List<string>();
				if (ComparisonResults.FirstOrDefault()?.CurrentRow != null)
				{
					DataTable table = ComparisonResults.First().CurrentRow.Table;
					for (int i = 0; i < table.Columns.Count; i++)
					{
						list.Add(table.Columns[i].ColumnName);
					}
				}
				worksheet.Cells[0, 0].PutValue("变更类型");
				worksheet.Cells[0, 1].PutValue("主键");
				for (int j = 0; j < list.Count; j++)
				{
					worksheet.Cells[0, j + 2].PutValue(list[j]);
				}
				int num = 1;
				foreach (ComparisonRow comparisonResult in ComparisonResults)
				{
					worksheet.Cells[num, 0].PutValue(comparisonResult.Type.ToString());
					worksheet.Cells[num, 1].PutValue(comparisonResult.Key);
					for (int k = 0; k < list.Count; k++)
					{
						string text = list[k];
						string stringValue = comparisonResult.CurrentRow[text]?.ToString() ?? "";
						worksheet.Cells[num, k + 2].PutValue(stringValue);
						if (comparisonResult.Type == ComparisonType.Modified && comparisonResult.ChangedFields.ContainsKey(text) && comparisonResult.ChangedFields[text])
						{
							Cell cell = worksheet.Cells[num, k + 2];
							Style style = cell.GetStyle();
							style.BackgroundColor = Color.LightCoral;
							cell.SetStyle(style);
						}
					}
					num++;
				}
				worksheet.AutoFitColumns();
				workbook.Save(savePath);
			});
			model.Status.Success("导出完成：" + savePath);
		}
		catch (Exception ex)
		{
			model.Status.Error("导出失败：" + ex.Message);
		}
	}

	public void OnNavigatedTo()
	{
	}

	public void OnNavigatedFrom()
	{
	}
}
