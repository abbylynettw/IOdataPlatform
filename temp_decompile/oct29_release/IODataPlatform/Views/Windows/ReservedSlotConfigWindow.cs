using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using IODataPlatform.Models;
using IODataPlatform.Models.ExcelModels;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// ReservedSlotConfigWindow.xaml 的交互逻辑
/// </summary>
/// <summary>
/// ReservedSlotConfigWindow
/// </summary>
public class ReservedSlotConfigWindow : FluentWindow, IComponentConnector
{
	internal System.Windows.Controls.DataGrid CabinetDataGrid;

	internal Wpf.Ui.Controls.Button ConfirmButton;

	internal Wpf.Ui.Controls.Button CancelButton;

	private bool _contentLoaded;

	/// <summary>
	/// 预留插槽配置集合
	/// </summary>
	public ObservableCollection<CabinetReservedSlotConfig> ReservedSlotConfigs { get; set; }

	/// <summary>
	/// 构造函数
	/// </summary>
	/// <param name="cabinets">机柜列表</param>
	public ReservedSlotConfigWindow(IEnumerable<StdCabinet> cabinets)
	{
		InitializeComponent();
		ReservedSlotConfigs = new ObservableCollection<CabinetReservedSlotConfig>();
		foreach (StdCabinet cabinet in cabinets)
		{
			ReservedSlotConfigs.Add(new CabinetReservedSlotConfig
			{
				CabinetName = cabinet.Name,
				IsSelected = false,
				ReservedCount = 0
			});
		}
		CabinetDataGrid.ItemsSource = ReservedSlotConfigs;
	}

	/// <summary>
	/// 确认按钮点击事件
	/// </summary>
	private void ConfirmButton_Click(object sender, RoutedEventArgs e)
	{
		foreach (CabinetReservedSlotConfig reservedSlotConfig in ReservedSlotConfigs)
		{
			if (reservedSlotConfig.IsSelected && reservedSlotConfig.ReservedCount <= 0)
			{
				System.Windows.MessageBox.Show("机柜 " + reservedSlotConfig.CabinetName + " 已选择预留插槽，请输入大于0的预留数量。", "输入验证失败", System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
		}
		System.Windows.MessageBox.Show("预留插槽设置已保存！", "成功", System.Windows.MessageBoxButton.OK, MessageBoxImage.Asterisk);
		base.DialogResult = true;
	}

	/// <summary>
	/// 取消按钮点击事件
	/// </summary>
	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/windows/reservedslotconfigwindow.xaml", UriKind.Relative);
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
			CabinetDataGrid = (System.Windows.Controls.DataGrid)target;
			break;
		case 2:
			ConfirmButton = (Wpf.Ui.Controls.Button)target;
			ConfirmButton.Click += ConfirmButton_Click;
			break;
		case 3:
			CancelButton = (Wpf.Ui.Controls.Button)target;
			CancelButton.Click += CancelButton_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
