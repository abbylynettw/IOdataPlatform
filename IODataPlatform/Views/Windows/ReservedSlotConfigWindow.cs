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
public partial class ReservedSlotConfigWindow : FluentWindow
{
	/// <summary>
	/// 预留插槽配置集合
	/// </summary>
	public ObservableCollection<CabinetReservedSlotConfig> ReservedSlotConfigs { get; set; }

	/// <summary>
	/// 构造函数
	/// </summary>
	/// <param name="cabinets">机柜列表</param>
	public ReservedSlotConfigWindow(IEnumerable<StdCabinet> cabinets) : this(cabinets, null)
	{
	}

	/// <summary>
	/// 构造函数（带现有配置）
	/// </summary>
	/// <param name="cabinets">机柜列表</param>
	/// <param name="existingConfigs">现有的预留配置列表</param>
	public ReservedSlotConfigWindow(IEnumerable<StdCabinet> cabinets, List<CabinetReservedSlotConfig>? existingConfigs)
	{
		InitializeComponent();
		ReservedSlotConfigs = new ObservableCollection<CabinetReservedSlotConfig>();
		
		foreach (StdCabinet cabinet in cabinets)
		{
			// 🔑 查找该机柜是否有现有配置
			var existingConfig = existingConfigs?.FirstOrDefault(c => c.CabinetName == cabinet.Name);
			
			if (existingConfig != null)
			{
				// 如果有现有配置，使用现有配置
				ReservedSlotConfigs.Add(existingConfig);
			}
			else
			{
				// 否则创建新的配置
				ReservedSlotConfigs.Add(new CabinetReservedSlotConfig
				{
					CabinetName = cabinet.Name,
					IsSelected = false
					// ReservedCount 是只读属性，自动等于 SlotConfigs.Count，不需要赋值
				});
			}
		}
		
		CabinetDataGrid.ItemsSource = ReservedSlotConfigs;
	}

	// 🗑️ 删除插槽按钮事件（不再需要，因为现在只配置数量）

	/// <summary>
	/// 添加插槽按钮点击事件
	/// </summary>
	private void AddSlotButton_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Wpf.Ui.Controls.Button button && button.Tag is CabinetReservedSlotConfig cabinetConfig)
		{
			// 添加新的插槽配置
			cabinetConfig.SlotConfigs.Add(new SlotCardTypeConfig
			{
				SelectedCardType = "MD211",
				AvailableCardTypes = cabinetConfig.AvailableCardTypes
			});
		}
	}

	/// <summary>
	/// 删除插槽按钮点击事件
	/// </summary>
	private void DeleteSlotButton_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Wpf.Ui.Controls.Button button && button.Tag is SlotCardTypeConfig slotConfig)
		{
			// 找到该插槽配置所属的机柜配置
			foreach (var cabinetConfig in ReservedSlotConfigs)
			{
				if (cabinetConfig.SlotConfigs.Contains(slotConfig))
				{
					cabinetConfig.SlotConfigs.Remove(slotConfig);
					break;
				}
			}
		}
	}

	/// <summary>
	/// 切换预留目的按钮点击事件
	/// </summary>
	private void TogglePurposeButton_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Wpf.Ui.Controls.Button button && button.Tag is SlotCardTypeConfig slotConfig)
		{
			// 在通讯预留和报警预留之间切换
			slotConfig.ReservedPurpose = slotConfig.ReservedPurpose == ReservedPurpose.通讯预留 
				? ReservedPurpose.报警预留 
				: ReservedPurpose.通讯预留;
		}
	}

	/// <summary>
	/// 确认按钮点击事件
	/// </summary>
	private void ConfirmButton_Click(object sender, RoutedEventArgs e)
	{
		foreach (CabinetReservedSlotConfig reservedSlotConfig in ReservedSlotConfigs)
		{
			if (reservedSlotConfig.IsSelected)
			{
				if (reservedSlotConfig.ReservedCount <= 0)
				{
					System.Windows.MessageBox.Show("机柜 " + reservedSlotConfig.CabinetName + " 已选择预留插槽，但没有添加任何预留插槽配置，请点击'添加预留插槽'按钮。", "输入验证失败", System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
					return;
				}
				
				// 验证每个插槽的板卡类型是否已选择
				foreach (var slotConfig in reservedSlotConfig.SlotConfigs)
				{
					if (string.IsNullOrEmpty(slotConfig.SelectedCardType))
					{
						System.Windows.MessageBox.Show($"机柜 {reservedSlotConfig.CabinetName} 有插槽未选择板卡类型，请选择。", "输入验证失败", System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
						return;
					}
				}
			}
		}
		// 验证通过，关闭窗口并返回成功结果
		base.DialogResult = true;
	}

	/// <summary>
	/// 取消按钮点击事件
	/// </summary>
	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
	}
}
