using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using LYSoft.Libs;
using LYSoft.Libs.Wpf.WpfUI.Custom;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// DepXT1Page
/// </summary>
public class DepXT1Page : Page, INavigableView<DepXT1ViewModel>, IComponentConnector
{
	internal Slider SliderRedundancy;

	internal StackPanel ColumnsVisibilityPanel;

	internal CustomDataGrid grid;

	private bool _contentLoaded;

	public DepXT1ViewModel ViewModel { get; set; }

	public DepXT1Page(DepXT1ViewModel viewModel)
	{
		ViewModel = viewModel;
		base.DataContext = this;
		InitializeComponent();
		base.Loaded += DepXT1Page_Loaded;
	}

	private void DepXT1Page_Loaded(object sender, RoutedEventArgs e)
	{
		ColumnsVisibilityPanel.Children.Clear();
		CheckBox checkBox = new CheckBox
		{
			Content = "全选",
			IsChecked = false
		};
		checkBox.Checked += delegate
		{
			ColumnsVisibilityPanel.Children.Cast<CheckBox>().AllDo(delegate(CheckBox x)
			{
				x.IsChecked = true;
			});
		};
		checkBox.Unchecked += delegate
		{
			ColumnsVisibilityPanel.Children.Cast<CheckBox>().AllDo(delegate(CheckBox x)
			{
				x.IsChecked = false;
			});
		};
		ColumnsVisibilityPanel.Children.Add(checkBox);
		List<string> columnsInit = new List<string>
		{
			"房间号", "标签名", "原变量名", "原扩展码", "信号说明", "房间号", "盘箱柜号", "IO类型", "IO卡型号", "IO卡编号",
			"安全分级/分组", "端子排编号", "接线点1", "接线点2"
		};
		grid.Columns.ToObservable().Subscribe(delegate(DataGridColumn x)
		{
			CheckBox checkBox2 = new CheckBox
			{
				Content = x.Header,
				IsChecked = columnsInit.Contains($"{x.Header}")
			};
			x.Visibility = ((!(checkBox2.IsChecked ?? false)) ? Visibility.Collapsed : Visibility.Visible);
			checkBox2.Checked += delegate
			{
				x.Visibility = Visibility.Visible;
			};
			checkBox2.Unchecked += delegate
			{
				x.Visibility = Visibility.Collapsed;
			};
			ColumnsVisibilityPanel.Children.Add(checkBox2);
		});
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/pages/depxt1page.xaml", UriKind.Relative);
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
			ColumnsVisibilityPanel = (StackPanel)target;
			break;
		case 3:
			grid = (CustomDataGrid)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
