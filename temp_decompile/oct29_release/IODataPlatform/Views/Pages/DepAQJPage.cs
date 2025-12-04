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
using System.Windows.Markup;
using IODataPlatform.Models.ExcelModels;
using LYSoft.Libs.Wpf.WpfUI.Custom;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// DepAQJPage
/// </summary>
public class DepAQJPage : Page, INavigableView<DepAQJViewModel>, IComponentConnector
{
	private readonly Dictionary<string, DataGridColumn> columnMapping = new Dictionary<string, DataGridColumn>();

	internal Slider SliderRedundancy;

	internal StackPanel ColumnsVisibilityPanel;

	internal CustomDataGrid Data;

	private bool _contentLoaded;

	public DepAQJViewModel ViewModel { get; }

	public DepAQJPage(DepAQJViewModel viewModel)
	{
		ViewModel = viewModel;
		base.DataContext = this;
		InitializeComponent();
	}

	private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		InitializeColumnsVisibilityPanel();
	}

	private void InitializeColumnsVisibilityPanel()
	{
		List<string> defaultField = ViewModel.GetDefaultField();
		ColumnsVisibilityPanel.Children.Clear();
		CheckBox checkBox = new CheckBox
		{
			Content = "全选",
			IsChecked = false
		};
		checkBox.Checked += delegate
		{
			foreach (CheckBox item in ColumnsVisibilityPanel.Children.OfType<CheckBox>())
			{
				item.IsChecked = true;
				UpdateColumnVisibility(item.Content.ToString(), isVisible: true);
			}
		};
		checkBox.Unchecked += delegate
		{
			foreach (CheckBox item2 in ColumnsVisibilityPanel.Children.OfType<CheckBox>())
			{
				item2.IsChecked = false;
				UpdateColumnVisibility(item2.Content.ToString(), isVisible: false);
			}
		};
		ColumnsVisibilityPanel.Children.Add(checkBox);
		PropertyInfo[] properties = typeof(IoFullData).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			string displayName = propertyInfo.GetCustomAttribute<DisplayAttribute>()?.Name ?? propertyInfo.Name;
			CheckBox checkBox2 = new CheckBox
			{
				Content = displayName,
				IsChecked = defaultField.Contains(displayName)
			};
			checkBox2.Checked += delegate
			{
				UpdateColumnVisibility(displayName, isVisible: true);
			};
			checkBox2.Unchecked += delegate
			{
				UpdateColumnVisibility(displayName, isVisible: false);
			};
			ColumnsVisibilityPanel.Children.Add(checkBox2);
		}
	}

	private void UpdateColumnVisibility(string columnName, bool isVisible)
	{
		if (columnMapping.TryGetValue(columnName, out DataGridColumn value))
		{
			value.Visibility = ((!isVisible) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
	{
		string header = e.Column.Header.ToString();
		bool flag = ColumnsVisibilityPanel.Children.OfType<CheckBox>().FirstOrDefault((CheckBox cb) => cb.Content.ToString() == header)?.IsChecked ?? true;
		e.Column.Visibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
		columnMapping[header] = e.Column;
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
	/// InitializeComponent
	/// </summary>
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/pages/depaqjpage.xaml", UriKind.Relative);
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
			((ComboBox)target).SelectionChanged += ComboBox_SelectionChanged;
			break;
		case 2:
			SliderRedundancy = (Slider)target;
			break;
		case 3:
			ColumnsVisibilityPanel = (StackPanel)target;
			break;
		case 4:
			Data = (CustomDataGrid)target;
			Data.Loaded += DataGrid_Loaded;
			Data.Unloaded += DataGrid_Unloaded;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
