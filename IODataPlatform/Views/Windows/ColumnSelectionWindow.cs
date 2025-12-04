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
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// ColumnSelectionWindow
/// </summary>
public partial class ColumnSelectionWindow : FluentWindow
{
	private readonly List<CheckBox> allColumnCheckBoxes = new List<CheckBox>();

	private readonly Dictionary<string, bool> columnVisibility = new Dictionary<string, bool>();

	private readonly List<string> defaultVisibleColumns;

	public Dictionary<string, bool> ColumnVisibility => columnVisibility;

	public ColumnSelectionWindow(List<string> defaultVisibleColumns, Dictionary<string, bool>? currentColumnVisibility = null)
	{
		this.defaultVisibleColumns = defaultVisibleColumns;
		InitializeComponent();
		InitializeColumns(currentColumnVisibility);
	}

	private void InitializeColumns(Dictionary<string, bool>? currentColumnVisibility)
	{
		allColumnCheckBoxes.Clear();
		ColumnsVisibilityPanel.Children.Clear();
		List<PropertyInfo> list = typeof(IoFullData).GetProperties().ToList();
		bool flag = false;
		foreach (PropertyInfo item in list)
		{
			string displayName = item.GetCustomAttribute<DisplayAttribute>()?.Name ?? item.Name;
			bool flag2 = false;
			if (item.Name == "eletroValueBox")
			{
				flag = true;
			}
			else if (flag)
			{
				flag2 = true;
			}
			bool value;
			if (currentColumnVisibility != null && currentColumnVisibility.ContainsKey(displayName))
			{
				value = currentColumnVisibility[displayName];
			}
			else
			{
				bool flag3 = displayName.Contains("报警") || displayName.Contains("限") || displayName.Contains("告警") || displayName.Contains("Alarm") || displayName.Contains("Limit") || item.Name.Contains("Alarm") || item.Name.Contains("Limit");
				value = defaultVisibleColumns.Contains(displayName) && !flag3 && !flag2;
			}
			CheckBox checkBox = new CheckBox
			{
				Content = displayName,
				IsChecked = value,
				Margin = new Thickness(5.0),
				Width = 200.0
			};
			allColumnCheckBoxes.Add(checkBox);
			ColumnsVisibilityPanel.Children.Add(checkBox);
			columnVisibility[displayName] = value;
			checkBox.Checked += delegate
			{
				columnVisibility[displayName] = true;
			};
			checkBox.Unchecked += delegate
			{
				columnVisibility[displayName] = false;
			};
		}
	}

	/// <summary>
	/// 搜索框文本变化，过滤列
	/// </summary>
	private void ColumnSearchBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		string value = ColumnSearchBox.Text?.ToLower() ?? string.Empty;
		ColumnsVisibilityPanel.Children.Clear();
		foreach (CheckBox allColumnCheckBox in allColumnCheckBoxes)
		{
			string text = allColumnCheckBox.Content.ToString()?.ToLower() ?? string.Empty;
			if (string.IsNullOrEmpty(value) || text.Contains(value))
			{
				ColumnsVisibilityPanel.Children.Add(allColumnCheckBox);
			}
		}
	}

	/// <summary>
	/// 全选/取消全选切换按钮（智能切换）
	/// </summary>
	private void ToggleAllColumns_Click(object sender, RoutedEventArgs e)
	{
		// 检查当前显示的列中是否全部选中
		var visibleCheckBoxes = allColumnCheckBoxes.Where(cb => ColumnsVisibilityPanel.Children.Contains(cb)).ToList();
		var allChecked = visibleCheckBoxes.All(cb => cb.IsChecked == true);
		
		if (allChecked)
		{
			// 如果全部选中，则取消全选
			foreach (CheckBox allColumnCheckBox in visibleCheckBoxes)
			{
				allColumnCheckBox.IsChecked = false;
			}
			if (sender is Wpf.Ui.Controls.Button button)
			{
				button.Content = "全选";
				button.Icon = new SymbolIcon { Symbol = SymbolRegular.CheckboxChecked24 };
			}
		}
		else
		{
			// 如果有未选中的，则全选
			foreach (CheckBox allColumnCheckBox in visibleCheckBoxes)
			{
				allColumnCheckBox.IsChecked = true;
			}
			if (sender is Wpf.Ui.Controls.Button button)
			{
				button.Content = "取消全选";
				button.Icon = new SymbolIcon { Symbol = SymbolRegular.CheckboxUnchecked24 };
			}
		}
	}

	/// <summary>
	/// 全选按钮
	/// </summary>
	private void SelectAllColumns_Click(object sender, RoutedEventArgs e)
	{
		foreach (CheckBox allColumnCheckBox in allColumnCheckBoxes)
		{
			if (ColumnsVisibilityPanel.Children.Contains(allColumnCheckBox))
			{
				allColumnCheckBox.IsChecked = true;
			}
		}
	}

	/// <summary>
	/// 取消全选按钮
	/// </summary>
	private void UnselectAllColumns_Click(object sender, RoutedEventArgs e)
	{
		foreach (CheckBox allColumnCheckBox in allColumnCheckBoxes)
		{
			if (ColumnsVisibilityPanel.Children.Contains(allColumnCheckBox))
			{
				allColumnCheckBox.IsChecked = false;
			}
		}
	}

	/// <summary>
	/// 反选按钮
	/// </summary>
	private void InverseSelectColumns_Click(object sender, RoutedEventArgs e)
	{
		foreach (CheckBox allColumnCheckBox in allColumnCheckBoxes)
		{
			if (ColumnsVisibilityPanel.Children.Contains(allColumnCheckBox))
			{
				allColumnCheckBox.IsChecked = !allColumnCheckBox.IsChecked;
			}
		}
	}

	/// <summary>
	/// 恢复默认按钮
	/// </summary>
	private void RestoreDefaultColumns_Click(object sender, RoutedEventArgs e)
	{
		List<PropertyInfo> list = typeof(IoFullData).GetProperties().ToList();
		bool flag = false;
		HashSet<string> hashSet = new HashSet<string>();
		foreach (PropertyInfo item2 in list)
		{
			string item = item2.GetCustomAttribute<DisplayAttribute>()?.Name ?? item2.Name;
			if (item2.Name == "eletroValueBox")
			{
				flag = true;
			}
			else if (flag)
			{
				hashSet.Add(item);
			}
		}
		foreach (CheckBox allColumnCheckBox in allColumnCheckBoxes)
		{
			string text = allColumnCheckBox.Content.ToString() ?? string.Empty;
			bool flag2 = text.Contains("报警") || text.Contains("限") || text.Contains("告警") || text.Contains("Alarm") || text.Contains("Limit");
			bool flag3 = hashSet.Contains(text);
			allColumnCheckBox.IsChecked = defaultVisibleColumns.Contains(text) && !flag2 && !flag3;
		}
	}

	/// <summary>
	/// 确定按钮
	/// </summary>
	private void ConfirmButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = true;
		Close();
	}

	/// <summary>
	/// 取消按钮
	/// </summary>
	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
		Close();
	}
}
