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
/// CablePage
/// </summary>
public class CablePage : Page, INavigableView<CableViewModel>, IComponentConnector
{
	internal StackPanel ColumnsVisibilityPanel;

	internal CustomDataGrid Data;

	private bool _contentLoaded;

	public CableViewModel ViewModel { get; }

	public CablePage(CableViewModel viewModel)
	{
		ViewModel = viewModel;
		base.DataContext = this;
		InitializeComponent();
		base.Loaded += DepXT2Page_Loaded;
	}

	private void DepXT2Page_Loaded(object sender, RoutedEventArgs e)
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
			"序号", "线缆编号", "线缆列别", "色标", "特性代码", "芯线对数号", "芯线号", "起点房间号", "起点盘柜名称", "起点安全分级分组",
			"起点系统号", "起点IO类型", "起点设备名称", "起点接线点1", "起点接线点2", "起点信号位号", "终点房间号", "终点盘柜名称", "终点设备名称", "终点安全分级分组",
			"终点系统号", "终点IO类型", "终点接线点1", "终点接线点2", "终点信号位号", "IO类型", "信号说明", "供货方", "版本", "备注",
			"匹配情况", "StartNumber", "SystemNo", "起点专业", "终点专业"
		};
		Data.Columns.ToObservable().Subscribe(delegate(DataGridColumn x)
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/pages/cablepage.xaml", UriKind.Relative);
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
			ColumnsVisibilityPanel = (StackPanel)target;
			break;
		case 2:
			Data = (CustomDataGrid)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
