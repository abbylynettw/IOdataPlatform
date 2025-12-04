using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Windows;
using LYSoft.Libs.Wpf.WpfUI.Custom;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// DepXT2Page
/// </summary>
public class DepXT2Page : Page, INavigableView<DepXT2ViewModel>, IComponentConnector
{
	private List<Wpf.Ui.Controls.DataGrid> dataGrids = new List<Wpf.Ui.Controls.DataGrid>();

	private readonly Dictionary<string, DataGridColumn> columnMapping = new Dictionary<string, DataGridColumn>();

	private readonly Dictionary<string, bool> userColumnVisibility = new Dictionary<string, bool>();

	internal Slider SliderRedundancy;

	internal ToggleSwitch ShowPoints;

	internal CustomDataGrid MainDataGrid;

	private bool _contentLoaded;

	public DepXT2ViewModel ViewModel { get; }

	public DepXT2Page(DepXT2ViewModel viewModel)
	{
		ViewModel = viewModel;
		base.DataContext = this;
		InitializeComponent();
	}

	/// <summary>
	/// 切换筛选模式（显示/隐藏列头筛选图标）
	/// </summary>
	private void ToggleFilterMode_Click(object sender, RoutedEventArgs e)
	{
		ViewModel.IsFilterModeEnabled = !ViewModel.IsFilterModeEnabled;
		foreach (KeyValuePair<string, DataGridColumn> item in columnMapping)
		{
			if (item.Value.Visibility == Visibility.Visible)
			{
				if (ViewModel.IsFilterModeEnabled)
				{
					ApplyFilterButtonToColumn(item.Value);
				}
				else
				{
					RemoveFilterButtonFromColumn(item.Value);
				}
			}
		}
	}

	/// <summary>
	/// 重置所有筛选
	/// </summary>
	private void ResetFiltersMenuItem_Click(object sender, RoutedEventArgs e)
	{
		ViewModel.ClearAllFilterOptionsCommand.Execute(null);
	}

	private void UpdateColumnVisibility(string columnName, bool isVisible)
	{
		if (columnMapping.TryGetValue(columnName, out DataGridColumn value))
		{
			value.Visibility = ((!isVisible) ? Visibility.Collapsed : Visibility.Visible);
			if (isVisible && ViewModel.IsFilterModeEnabled)
			{
				ApplyFilterButtonToColumn(value);
			}
			else if (!isVisible)
			{
				RemoveFilterButtonFromColumn(value);
			}
		}
	}

	/// <summary>
	/// 为指定列添加筛选按钮
	/// </summary>
	private void ApplyFilterButtonToColumn(DataGridColumn column)
	{
		string columnHeader = column.Header?.ToString();
		if (!string.IsNullOrEmpty(columnHeader))
		{
			ExcelFilter excelFilter = ViewModel.Filters.FirstOrDefault((ExcelFilter f) => f.Title == columnHeader);
			if (excelFilter != null)
			{
				DataTemplate dataTemplate = new DataTemplate();
				FrameworkElementFactory frameworkElementFactory = new FrameworkElementFactory(typeof(StackPanel));
				frameworkElementFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
				FrameworkElementFactory frameworkElementFactory2 = new FrameworkElementFactory(typeof(Wpf.Ui.Controls.TextBlock));
				frameworkElementFactory2.SetValue(System.Windows.Controls.TextBlock.TextProperty, columnHeader);
				frameworkElementFactory2.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
				frameworkElementFactory2.SetValue(FrameworkElement.MarginProperty, new Thickness(0.0, 0.0, 5.0, 0.0));
				frameworkElementFactory.AppendChild(frameworkElementFactory2);
				FrameworkElementFactory frameworkElementFactory3 = new FrameworkElementFactory(typeof(Wpf.Ui.Controls.Button));
				frameworkElementFactory3.SetValue(ContentControl.ContentProperty, "\ud83d\udd3d");
				frameworkElementFactory3.SetValue(FrameworkElement.WidthProperty, 20.0);
				frameworkElementFactory3.SetValue(FrameworkElement.HeightProperty, 20.0);
				frameworkElementFactory3.SetValue(Control.PaddingProperty, new Thickness(0.0));
				frameworkElementFactory3.SetValue(Control.FontSizeProperty, 10.0);
				frameworkElementFactory3.SetValue(Wpf.Ui.Controls.Button.AppearanceProperty, ControlAppearance.Secondary);
				frameworkElementFactory3.SetValue(FrameworkElement.TagProperty, excelFilter);
				frameworkElementFactory3.SetValue(FrameworkElement.ToolTipProperty, "筛选");
				frameworkElementFactory3.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(FilterButton_Click));
				frameworkElementFactory.AppendChild(frameworkElementFactory3);
				dataTemplate.VisualTree = frameworkElementFactory;
				column.HeaderTemplate = dataTemplate;
			}
		}
	}

	/// <summary>
	/// 从指定列移除筛选按钮（恢复原始列头）
	/// </summary>
	private void RemoveFilterButtonFromColumn(DataGridColumn column)
	{
		column.HeaderTemplate = null;
	}

	/// <summary>
	/// 筛选按钮点击事件处理
	/// </summary>
	private void FilterButton_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Wpf.Ui.Controls.Button { Tag: ExcelFilter tag })
		{
			ExcelFilterWindow excelFilterWindow = new ExcelFilterWindow(tag)
			{
				Owner = Window.GetWindow((DependencyObject)(object)this)
			};
			if (excelFilterWindow.ShowDialog() == true)
			{
				ViewModel.FilterAndSortCommand.Execute(null);
			}
		}
	}

	private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
	{
		string text = e.Column.Header.ToString();
		List<string> defaultField = ViewModel.GetDefaultField();
		bool flag = text.Contains("报警") || text.Contains("限") || text.Contains("告警") || text.Contains("Alarm") || text.Contains("Limit");
		bool flag2 = false;
		List<PropertyInfo> list = typeof(IoFullData).GetProperties().ToList();
		bool flag3 = false;
		foreach (PropertyInfo item in list)
		{
			string text2 = item.GetCustomAttribute<DisplayAttribute>()?.Name;
			if (item.Name == "eletroValueBox")
			{
				flag3 = true;
			}
			else if (flag3 && text2 == text)
			{
				flag2 = true;
				break;
			}
		}
		bool flag4 = ((!userColumnVisibility.ContainsKey(text)) ? (defaultField.Contains(text) && !flag && !flag2) : userColumnVisibility[text]);
		e.Column.Visibility = ((!flag4) ? Visibility.Collapsed : Visibility.Visible);
		columnMapping[text] = e.Column;
		if (ViewModel.IsFilterModeEnabled && flag4)
		{
			ApplyFilterButtonToColumn(e.Column);
		}
	}

	private void DataGrid_Loaded(object sender, RoutedEventArgs e)
	{
		if (sender is Wpf.Ui.Controls.DataGrid dataGrid)
		{
			dataGrid.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
		}
	}

	private void DataGrid_Unloaded(object sender, RoutedEventArgs e)
	{
		if (sender is Wpf.Ui.Controls.DataGrid dataGrid)
		{
			dataGrid.AutoGeneratingColumn -= DataGrid_AutoGeneratingColumn;
		}
	}

	/// <summary>
	/// 显示列选择对话框
	/// </summary>
	private void ShowColumnsDialog_Click(object sender, RoutedEventArgs e)
	{
		List<string> defaultField = ViewModel.GetDefaultField();
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		foreach (KeyValuePair<string, DataGridColumn> item in columnMapping)
		{
			dictionary[item.Key] = item.Value.Visibility == Visibility.Visible;
		}
		ColumnSelectionWindow columnSelectionWindow = new ColumnSelectionWindow(defaultField, dictionary)
		{
			Owner = Window.GetWindow((DependencyObject)(object)this)
		};
		if (columnSelectionWindow.ShowDialog() != true)
		{
			return;
		}
		foreach (KeyValuePair<string, bool> item2 in columnSelectionWindow.ColumnVisibility)
		{
			userColumnVisibility[item2.Key] = item2.Value;
			UpdateColumnVisibility(item2.Key, item2.Value);
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/pages/depxt2page.xaml", UriKind.Relative);
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
			SliderRedundancy = (Slider)target;
			break;
		case 2:
			ShowPoints = (ToggleSwitch)target;
			break;
		case 3:
			((Wpf.Ui.Controls.MenuItem)target).Click += ShowColumnsDialog_Click;
			break;
		case 4:
			((Wpf.Ui.Controls.MenuItem)target).Click += ToggleFilterMode_Click;
			break;
		case 5:
			((Wpf.Ui.Controls.MenuItem)target).Click += ResetFiltersMenuItem_Click;
			break;
		case 6:
			MainDataGrid = (CustomDataGrid)target;
			MainDataGrid.Loaded += DataGrid_Loaded;
			MainDataGrid.Unloaded += DataGrid_Unloaded;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
