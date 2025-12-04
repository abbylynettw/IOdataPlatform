using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Aspose.Cells;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Utilities;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.SubPages.Common;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class GenericDataComparisonViewModel(GlobalModel model, IMessageService message, IPickerService picker, ExcelService excel) : ObservableObject()
{
	[ObservableProperty]
	private string oldDataSource = "请选择旧版数据文件";

	[ObservableProperty]
	private string newDataSource = "请选择新版数据文件";

	[ObservableProperty]
	private int addedCount;

	[ObservableProperty]
	private int deletedCount;

	[ObservableProperty]
	private int modifiedCount;

	[ObservableProperty]
	private bool hasComparisonResults;

	[ObservableProperty]
	private bool hasDataLoaded;

	[ObservableProperty]
	private bool showAdded = true;

	[ObservableProperty]
	private bool showDeleted = true;

	[ObservableProperty]
	private bool showModified = true;

	[ObservableProperty]
	private bool showUnchanged;

	[ObservableProperty]
	private ObservableCollection<string> availableFields = new ObservableCollection<string>();

	[ObservableProperty]
	private string selectedField = string.Empty;

	private ICollectionView? filteredResultsView;

	private DataTable oldDataTable;

	private DataTable newDataTable;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.SelectOldFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? selectOldFileCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.SelectNewFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? selectNewFileCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.ExecuteComparisonCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? executeComparisonCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.ExportReportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? exportReportCommand;

	public ObservableCollection<GenericComparisonRow> ComparisonResults { get; } = new ObservableCollection<GenericComparisonRow>();

	public ICollectionView? FilteredResults
	{
		get
		{
			if (filteredResultsView == null && ComparisonResults.Count > 0)
			{
				filteredResultsView = CollectionViewSource.GetDefaultView(ComparisonResults);
				filteredResultsView.Filter = FilterResult;
			}
			return filteredResultsView;
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.oldDataSource" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string OldDataSource
	{
		get
		{
			return oldDataSource;
		}
		[MemberNotNull("oldDataSource")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(oldDataSource, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.OldDataSource);
				oldDataSource = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.OldDataSource);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.newDataSource" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string NewDataSource
	{
		get
		{
			return newDataSource;
		}
		[MemberNotNull("newDataSource")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(newDataSource, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.NewDataSource);
				newDataSource = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.NewDataSource);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.addedCount" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int AddedCount
	{
		get
		{
			return addedCount;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(addedCount, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.AddedCount);
				addedCount = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.AddedCount);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.deletedCount" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int DeletedCount
	{
		get
		{
			return deletedCount;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(deletedCount, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DeletedCount);
				deletedCount = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DeletedCount);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.modifiedCount" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int ModifiedCount
	{
		get
		{
			return modifiedCount;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(modifiedCount, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ModifiedCount);
				modifiedCount = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ModifiedCount);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.hasComparisonResults" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool HasComparisonResults
	{
		get
		{
			return hasComparisonResults;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(hasComparisonResults, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.HasComparisonResults);
				hasComparisonResults = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.HasComparisonResults);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.hasDataLoaded" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool HasDataLoaded
	{
		get
		{
			return hasDataLoaded;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(hasDataLoaded, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.HasDataLoaded);
				hasDataLoaded = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.HasDataLoaded);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.showAdded" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool ShowAdded
	{
		get
		{
			return showAdded;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(showAdded, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ShowAdded);
				showAdded = value;
				OnShowAddedChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ShowAdded);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.showDeleted" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool ShowDeleted
	{
		get
		{
			return showDeleted;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(showDeleted, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ShowDeleted);
				showDeleted = value;
				OnShowDeletedChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ShowDeleted);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.showModified" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool ShowModified
	{
		get
		{
			return showModified;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(showModified, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ShowModified);
				showModified = value;
				OnShowModifiedChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ShowModified);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.showUnchanged" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool ShowUnchanged
	{
		get
		{
			return showUnchanged;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(showUnchanged, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ShowUnchanged);
				showUnchanged = value;
				OnShowUnchangedChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ShowUnchanged);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.availableFields" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<string> AvailableFields
	{
		get
		{
			return availableFields;
		}
		[MemberNotNull("availableFields")]
		set
		{
			if (!EqualityComparer<ObservableCollection<string>>.Default.Equals(availableFields, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.AvailableFields);
				availableFields = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.AvailableFields);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.selectedField" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SelectedField
	{
		get
		{
			return selectedField;
		}
		[MemberNotNull("selectedField")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(selectedField, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SelectedField);
				selectedField = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SelectedField);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.SelectOldFile" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand SelectOldFileCommand => selectOldFileCommand ?? (selectOldFileCommand = new AsyncRelayCommand(SelectOldFile));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.SelectNewFile" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand SelectNewFileCommand => selectNewFileCommand ?? (selectNewFileCommand = new AsyncRelayCommand(SelectNewFile));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.ExecuteComparison" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ExecuteComparisonCommand => executeComparisonCommand ?? (executeComparisonCommand = new AsyncRelayCommand(ExecuteComparison));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.ExportReport" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ExportReportCommand => exportReportCommand ?? (exportReportCommand = new AsyncRelayCommand(ExportReport));

	private void RefreshFilter()
	{
		ICollectionView? filteredResults = FilteredResults;
		if (filteredResults != null)
		{
			filteredResults.Refresh();
		}
	}

	private bool FilterResult(object obj)
	{
		if (!(obj is GenericComparisonRow genericComparisonRow))
		{
			return false;
		}
		return genericComparisonRow.Type switch
		{
			GenericDataChangeType.新增 => ShowAdded, 
			GenericDataChangeType.删除 => ShowDeleted, 
			GenericDataChangeType.修改 => ShowModified, 
			GenericDataChangeType.无变化 => ShowUnchanged, 
			_ => false, 
		};
	}

	[RelayCommand]
	private async Task SelectOldFile()
	{
		string file = picker.OpenFile("Excel文件|*.xlsx;*.xls");
		if (file != null)
		{
			try
			{
				model.Status.Busy("正在加载旧版数据...");
				oldDataTable = await excel.GetDataTableAsStringAsync(file, hasHeader: true);
				OldDataSource = $"{Path.GetFileName(file)} ({oldDataTable.Rows.Count}条记录)";
				model.Status.Success($"已加载 {oldDataTable.Rows.Count} 条旧版数据");
				LoadAvailableFields();
			}
			catch (Exception ex)
			{
				model.Status.Error("加载失败：" + ex.Message);
			}
		}
	}

	[RelayCommand]
	private async Task SelectNewFile()
	{
		string file = picker.OpenFile("Excel文件|*.xlsx;*.xls");
		if (file != null)
		{
			try
			{
				model.Status.Busy("正在加载新版数据...");
				newDataTable = await excel.GetDataTableAsStringAsync(file, hasHeader: true);
				NewDataSource = $"{Path.GetFileName(file)} ({newDataTable.Rows.Count}条记录)";
				model.Status.Success($"已加载 {newDataTable.Rows.Count} 条新版数据");
				LoadAvailableFields();
			}
			catch (Exception ex)
			{
				model.Status.Error("加载失败：" + ex.Message);
			}
		}
	}

	private void LoadAvailableFields()
	{
		if (oldDataTable == null || newDataTable == null)
		{
			return;
		}
		List<string> list = (from f in (from DataColumn c in oldDataTable.Columns
				select c.ColumnName).Intersect(from DataColumn c in newDataTable.Columns
				select c.ColumnName)
			orderby f
			select f).ToList();
		AvailableFields.Clear();
		foreach (string item in list)
		{
			AvailableFields.Add(item);
		}
		HasDataLoaded = true;
		if (AvailableFields.Count > 0 && string.IsNullOrEmpty(SelectedField))
		{
			SelectedField = AvailableFields[0];
		}
	}

	[RelayCommand]
	private async Task ExecuteComparison()
	{
		if (oldDataTable == null || newDataTable == null)
		{
			await message.MessageAsync("提示", "请先选择新旧两个数据文件！");
			return;
		}
		if (!string.IsNullOrEmpty(SelectedField))
		{
			try
			{
				model.Status.Busy("正在执行对比分析...");
				List<DynamicObject> oldDataList = oldDataTable.StringTableToDynamicObjects(SelectedField);
				List<DynamicObject> newDataList = newDataTable.StringTableToDynamicObjects(SelectedField);
				List<DifferentObject<string>> list = await DataComparator.ComparerAsync(newDataList, oldDataList, (DynamicObject x) => x.Key);
				ComparisonResults.Clear();
				foreach (DifferentObject<string> diffObj in list)
				{
					GenericComparisonRow genericComparisonRow = new GenericComparisonRow();
					genericComparisonRow.Key = diffObj.Key;
					GenericComparisonRow genericComparisonRow2 = genericComparisonRow;
					genericComparisonRow2.Type = diffObj.Type switch
					{
						DifferentType.新增 => GenericDataChangeType.新增, 
						DifferentType.移除 => GenericDataChangeType.删除, 
						DifferentType.覆盖 => GenericDataChangeType.修改, 
						_ => GenericDataChangeType.无变化, 
					};
					genericComparisonRow.Data = ((diffObj.Type == DifferentType.新增) ? newDataList.FirstOrDefault((DynamicObject d) => d.Key == diffObj.Key) : oldDataList.FirstOrDefault((DynamicObject d) => d.Key == diffObj.Key));
					genericComparisonRow.DiffProps = diffObj.DiffProps;
					GenericComparisonRow item = genericComparisonRow;
					ComparisonResults.Add(item);
				}
				AddedCount = ComparisonResults.Count((GenericComparisonRow x) => x.Type == GenericDataChangeType.新增);
				DeletedCount = ComparisonResults.Count((GenericComparisonRow x) => x.Type == GenericDataChangeType.删除);
				ModifiedCount = ComparisonResults.Count((GenericComparisonRow x) => x.Type == GenericDataChangeType.修改);
				filteredResultsView = null;
				OnPropertyChanged("FilteredResults");
				HasComparisonResults = ComparisonResults.Count > 0;
				model.Status.Success($"对比完成！新增{AddedCount}条，删除{DeletedCount}条，修改{ModifiedCount}条");
				return;
			}
			catch (Exception ex)
			{
				model.Status.Error("对比失败：" + ex.Message);
				return;
			}
		}
		await message.MessageAsync("提示", "请选择主键字段！");
	}

	[RelayCommand]
	private async Task ExportReport()
	{
		if (!HasComparisonResults)
		{
			await message.MessageAsync("提示", "没有可导出的对比结果！");
			return;
		}
		try
		{
			string filename = $"数据对比报告_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
			string savePath = picker.SaveFile("Excel文件|*.xlsx", filename);
			if (savePath == null)
			{
				return;
			}
			model.Status.Busy("正在生成对比报告...");
			Workbook workbook = excel.GetWorkbook();
			Worksheet worksheet = workbook.Worksheets[0];
			worksheet.Name = "对比结果";
			string[] first = new string[2] { "变更类型", "主键值" };
			List<string> list = (from f in (from DataColumn c in oldDataTable.Columns
					select c.ColumnName).Union(from DataColumn c in newDataTable.Columns
					select c.ColumnName).Distinct()
				orderby f
				select f).ToList();
			string[] array = first.Concat(list).ToArray();
			for (int num = 0; num < array.Length; num++)
			{
				Cell cell = worksheet.Cells[0, num];
				cell.PutValue(array[num]);
				cell.SetStyle(GetHeaderStyle());
			}
			int num2 = 1;
			foreach (GenericComparisonRow comparisonResult in ComparisonResults)
			{
				worksheet.Cells[num2, 0].PutValue(comparisonResult.Type.ToString());
				worksheet.Cells[num2, 1].PutValue(comparisonResult.Key);
				for (int num3 = 0; num3 < list.Count; num3++)
				{
					string propertyName = list[num3];
					object obj = comparisonResult.Type switch
					{
						GenericDataChangeType.新增 => comparisonResult.Data?.GetPropertyValue(propertyName), 
						GenericDataChangeType.删除 => comparisonResult.Data?.GetPropertyValue(propertyName), 
						GenericDataChangeType.修改 => comparisonResult.Data?.GetPropertyValue(propertyName), 
						GenericDataChangeType.无变化 => comparisonResult.Data?.GetPropertyValue(propertyName), 
						_ => null, 
					};
					worksheet.Cells[num2, num3 + 2].PutValue(obj?.ToString());
				}
				Style style = workbook.CreateStyle();
				if (comparisonResult.Type == GenericDataChangeType.新增)
				{
					style.ForegroundColor = Color.FromArgb(255, 249, 196);
					style.Pattern = BackgroundType.Solid;
				}
				else if (comparisonResult.Type == GenericDataChangeType.修改)
				{
					style.ForegroundColor = Color.FromArgb(255, 205, 210);
					style.Pattern = BackgroundType.Solid;
				}
				else if (comparisonResult.Type == GenericDataChangeType.删除)
				{
					style.Font.IsStrikeout = true;
					style.ForegroundColor = Color.FromArgb(245, 245, 245);
					style.Pattern = BackgroundType.Solid;
				}
				for (int num4 = 0; num4 < array.Length; num4++)
				{
					worksheet.Cells[num2, num4].SetStyle(style);
				}
				num2++;
			}
			worksheet.AutoFitColumns();
			workbook.Save(savePath);
			model.Status.Success("对比报告已导出：" + Path.GetFileName(savePath));
			if (await message.ConfirmAsync("导出成功！是否立即打开文件？", "导出成功"))
			{
				Process.Start("explorer.exe", savePath);
			}
		}
		catch (Exception ex)
		{
			model.Status.Error("导出失败：" + ex.Message);
		}
	}

	private Style GetHeaderStyle()
	{
		Style style = new Style();
		style.Font.IsBold = true;
		style.Font.Size = 11;
		style.ForegroundColor = Color.LightGray;
		style.Pattern = BackgroundType.Solid;
		style.HorizontalAlignment = TextAlignmentType.Center;
		return style;
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.ShowAdded" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.ShowAdded" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnShowAddedChanged(bool value)
	{
		RefreshFilter();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.ShowDeleted" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.ShowDeleted" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnShowDeletedChanged(bool value)
	{
		RefreshFilter();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.ShowModified" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.ShowModified" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnShowModifiedChanged(bool value)
	{
		RefreshFilter();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.ShowUnchanged" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.GenericDataComparisonViewModel.ShowUnchanged" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnShowUnchangedChanged(bool value)
	{
		RefreshFilter();
	}
}
