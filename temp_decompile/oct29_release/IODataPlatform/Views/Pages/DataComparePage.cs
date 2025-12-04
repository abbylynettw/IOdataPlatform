using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using IODataPlatform.Models;
using LYSoft.Libs;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// DataComparePage
/// </summary>
public class DataComparePage : Page, INavigableView<DataCompareViewModel>, IComponentConnector, IStyleConnector
{
	private readonly ExcelService excel;

	public readonly GlobalModel model;

	private bool _contentLoaded;

	public DataCompareViewModel ViewModel { get; }

	public DataComparePage(DataCompareViewModel viewModel, ExcelService excel, GlobalModel model)
	{
		ViewModel = viewModel;
		this.excel = excel;
		this.model = model;
		base.DataContext = this;
		InitializeComponent();
	}

	private async void Compare(object sender, RoutedEventArgs e)
	{
		if (!File.Exists(ViewModel.FilePath1) || ViewModel.FilePath1 == "请选择Excel文件1")
		{
			model.Status.Error("请选择Excel文件1");
			return;
		}
		if (!File.Exists(ViewModel.FilePath2) || ViewModel.FilePath2 == "请选择Excel文件2")
		{
			model.Status.Error("请选择Excel文件2");
			return;
		}
		if (string.IsNullOrEmpty(ViewModel.FieldName))
		{
			model.Status.Error("请输入主键字段名");
			return;
		}
		if (string.IsNullOrEmpty(ViewModel.SelectedSheet1))
		{
			model.Status.Error("请选择文件1的工作表");
			return;
		}
		if (string.IsNullOrEmpty(ViewModel.SelectedSheet2))
		{
			model.Status.Error("请选择文件2的工作表");
			return;
		}
		model.Status.Busy("正在比对数据");
		try
		{
			using DataTable oldData1 = await excel.GetDataTableAsStringAsync(ViewModel.FilePath1, ViewModel.SelectedSheet1, hasHeader: true);
			using DataTable oldData2 = await excel.GetDataTableAsStringAsync(ViewModel.FilePath2, ViewModel.SelectedSheet2, hasHeader: true);
			List<ComparisonRow> list = await PerformComparison(oldData1, oldData2);
			ViewModel.ComparisonResults.Clear();
			foreach (ComparisonRow item in list)
			{
				ViewModel.ComparisonResults.Add(item);
			}
			model.Status.Success($"对比完成，共找到 {list.Count} 条记录");
		}
		catch (Exception ex)
		{
			model.Status.Error("对比失败：" + ex.Message);
		}
	}

	private async Task<List<ComparisonRow>> PerformComparison(DataTable oldData, DataTable newData)
	{
		return await Task.Run(delegate
		{
			List<ComparisonRow> list = new List<ComparisonRow>();
			Dictionary<string, DataRow> dictionary = new Dictionary<string, DataRow>();
			Dictionary<string, DataRow> dictionary2 = new Dictionary<string, DataRow>();
			foreach (DataRow row in oldData.Rows)
			{
				string text = row[ViewModel.FieldName]?.ToString() ?? "";
				if (!string.IsNullOrEmpty(text))
				{
					dictionary[text] = row;
				}
			}
			foreach (DataRow row2 in newData.Rows)
			{
				string text2 = row2[ViewModel.FieldName]?.ToString() ?? "";
				if (!string.IsNullOrEmpty(text2))
				{
					dictionary2[text2] = row2;
				}
			}
			IOrderedEnumerable<string> orderedEnumerable = from k in dictionary.Keys.Union(dictionary2.Keys).Distinct()
				orderby k
				select k;
			foreach (string item4 in orderedEnumerable)
			{
				bool flag = dictionary.ContainsKey(item4);
				bool flag2 = dictionary2.ContainsKey(item4);
				if (flag && flag2)
				{
					DataRow dataRow3 = dictionary[item4];
					DataRow dataRow4 = dictionary2[item4];
					Dictionary<string, bool> dictionary3 = new Dictionary<string, bool>();
					bool flag3 = false;
					foreach (DataColumn column in newData.Columns)
					{
						string text3 = dataRow3[column.ColumnName]?.ToString() ?? "";
						string text4 = dataRow4[column.ColumnName]?.ToString() ?? "";
						bool flag4 = text3 != text4;
						dictionary3[column.ColumnName] = flag4;
						if (flag4)
						{
							flag3 = true;
						}
					}
					ComparisonRow item = new ComparisonRow
					{
						Key = item4,
						CurrentRow = dataRow4,
						OldRow = dataRow3,
						Type = (flag3 ? ComparisonType.Modified : ComparisonType.Unchanged),
						ChangedFields = dictionary3
					};
					list.Add(item);
				}
				else if (flag2)
				{
					DataRow currentRow = dictionary2[item4];
					ComparisonRow item2 = new ComparisonRow
					{
						Key = item4,
						CurrentRow = currentRow,
						Type = ComparisonType.Added
					};
					list.Add(item2);
				}
				else if (flag)
				{
					DataRow currentRow2 = dictionary[item4];
					ComparisonRow item3 = new ComparisonRow
					{
						Key = item4,
						CurrentRow = currentRow2,
						Type = ComparisonType.Deleted
					};
					list.Add(item3);
				}
			}
			return list;
		});
	}

	private void ShowOldRow(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement { DataContext: ComparisonRow dataContext })
		{
			dataContext.ShowOldRow = !dataContext.ShowOldRow;
		}
	}

	private async void Page_Drop(object sender, DragEventArgs e)
	{
		if (!e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			return;
		}
		string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
		if (array.Length == 0)
		{
			return;
		}
		string text = array[0];
		if (!File.Exists(text) || (!text.EndsWith(".xlsx") && !text.EndsWith(".xls")))
		{
			return;
		}
		if (ViewModel.FilePath1 == "请选择Excel文件1" || ViewModel.FilePath1 == text)
		{
			ViewModel.FilePath1 = text;
			List<string> list = await excel.GetSheetNamesAsync(text);
			ViewModel.SheetNames1.Clear();
			foreach (string item in list)
			{
				ViewModel.SheetNames1.Add(item);
			}
			if (ViewModel.SheetNames1.Count > 0)
			{
				ViewModel.SelectedSheet1 = ViewModel.SheetNames1[0];
			}
		}
		else
		{
			if (!(ViewModel.FilePath2 == "请选择Excel文件2") && !(ViewModel.FilePath2 == text))
			{
				return;
			}
			ViewModel.FilePath2 = text;
			List<string> list2 = await excel.GetSheetNamesAsync(text);
			ViewModel.SheetNames2.Clear();
			foreach (string item2 in list2)
			{
				ViewModel.SheetNames2.Add(item2);
			}
			if (ViewModel.SheetNames2.Count > 0)
			{
				ViewModel.SelectedSheet2 = ViewModel.SheetNames2[0];
			}
		}
	}

	/// <summary>
	/// InitializeComponent
	/// </summary>
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/pages/datacomparepage.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			((DataComparePage)target).Drop += Page_Drop;
			break;
		case 2:
			((Wpf.Ui.Controls.Button)target).Click += Compare;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IStyleConnector.Connect(int connectionId, object target)
	{
		if (connectionId == 3)
		{
			((System.Windows.Controls.Button)target).Click += ShowOldRow;
		}
	}
}
