﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Windows;

using SqlSugar;

namespace IODataPlatform.Views.Pages;

public partial class DepXT2Page : INavigableView<DepXT2ViewModel>
{

    public DepXT2ViewModel ViewModel { get; }

    private List<Wpf.Ui.Controls.DataGrid> dataGrids = [];
    private readonly Dictionary<string, DataGridColumn> columnMapping = new();
    private readonly Dictionary<string, bool> userColumnVisibility = new(); // 保存用户的列选择

    public DepXT2Page(DepXT2ViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    /// <summary>
    /// 切换筛选模式（显示/隐藏列头筛选图标）
    /// </summary>
    private void ToggleFilterMode_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.IsFilterModeEnabled = !ViewModel.IsFilterModeEnabled;
        
        // 为所有当前可见的列添加或移除筛选按钮
        foreach (var kvp in columnMapping)
        {
            if (kvp.Value.Visibility == Visibility.Visible)
            {
                if (ViewModel.IsFilterModeEnabled)
                {
                    ApplyFilterButtonToColumn(kvp.Value);
                }
                else
                {
                    RemoveFilterButtonFromColumn(kvp.Value);
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
        if (columnMapping.TryGetValue(columnName, out var column))
        {
            column.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            
            // 如果筛选模式已开启，且列变为可见，需要为该列添加筛选按钮
            if (isVisible && ViewModel.IsFilterModeEnabled)
            {
                ApplyFilterButtonToColumn(column);
            }
            // 如果列变为隐藏，移除筛选按钮（恢复原始列头）
            else if (!isVisible)
            {
                RemoveFilterButtonFromColumn(column);
            }
        }
    }

    /// <summary>
    /// 刷新DataGrid以重新生成列
    /// </summary>
    private void RefreshDataGrid()
    {
        if (MainDataGrid != null)
        {
            var itemsSource = MainDataGrid.ItemsSource;
            MainDataGrid.ItemsSource = null;
            MainDataGrid.ItemsSource = itemsSource;
        }
    }

    /// <summary>
    /// 为指定列添加筛选按钮
    /// </summary>
    private void ApplyFilterButtonToColumn(DataGridColumn column)
    {
        var columnHeader = column.Header?.ToString();
        if (string.IsNullOrEmpty(columnHeader)) return;

        // 查找对应的筛选器
        var filter = ViewModel.Filters.FirstOrDefault(f => f.Title == columnHeader);
        if (filter == null) return;

        // 创建列头模板
        var headerTemplate = new DataTemplate();
        var factory = new FrameworkElementFactory(typeof(StackPanel));
        factory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

        // 列名文本
        var textFactory = new FrameworkElementFactory(typeof(Wpf.Ui.Controls.TextBlock));
        textFactory.SetValue(Wpf.Ui.Controls.TextBlock.TextProperty, columnHeader);
        textFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        textFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 5, 0));
        factory.AppendChild(textFactory);

        // 筛选按钮
        var buttonFactory = new FrameworkElementFactory(typeof(Wpf.Ui.Controls.Button));
        buttonFactory.SetValue(ContentControl.ContentProperty, "🔽");
        buttonFactory.SetValue(FrameworkElement.WidthProperty, 20.0);
        buttonFactory.SetValue(FrameworkElement.HeightProperty, 20.0);
        buttonFactory.SetValue(Control.PaddingProperty, new Thickness(0));
        buttonFactory.SetValue(Control.FontSizeProperty, 10.0);
        buttonFactory.SetValue(Wpf.Ui.Controls.Button.AppearanceProperty, Wpf.Ui.Controls.ControlAppearance.Secondary);
        buttonFactory.SetValue(FrameworkElement.TagProperty, filter);
        buttonFactory.SetValue(Control.ToolTipProperty, "筛选");
        buttonFactory.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, 
            new RoutedEventHandler(FilterButton_Click));
        
        factory.AppendChild(buttonFactory);
        headerTemplate.VisualTree = factory;
        column.HeaderTemplate = headerTemplate;
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
        if (sender is not Wpf.Ui.Controls.Button button) return;
        if (button.Tag is not ExcelFilter filter) return;

        // 弹出筛选窗口
        var dialog = new ExcelFilterWindow(filter)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            // 执行筛选命令
            ViewModel.FilterAndSortCommand.Execute(null);
        }
    }

    private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        var header = e.Column.Header.ToString();
        var defaultFields = ViewModel.GetDefaultField();

        // 默认不显示报警相关字段
        bool isAlarmRelated = header.Contains("报警") ||
                             header.Contains("限") ||
                             header.Contains("告警") ||
                             header.Contains("Alarm") ||
                             header.Contains("Limit");

        // 检查是否在"电磁阀箱类型"之后，如果是则默认不显示
        bool isAfterEletroValueBox = false;
        var properties = typeof(IoFullData).GetProperties().ToList();
        
        bool foundEletroValueBox = false;
        foreach (var prop in properties)
        {
            var displayName = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>()?.Name;
            
            // 找到"电磁阀箱类型"字段（属性名是eletroValueBox）
            if (prop.Name == "eletroValueBox")
            {
                foundEletroValueBox = true;
            }
            else if (foundEletroValueBox && displayName == header)
            {
                isAfterEletroValueBox = true;
                break;
            }
        }

        // 如果用户已经设置过，使用用户的选择；否则使用默认值
        bool isVisible;
        if (userColumnVisibility.ContainsKey(header))
        {
            isVisible = userColumnVisibility[header];
        }
        else
        {
            isVisible = defaultFields.Contains(header) && !isAlarmRelated && !isAfterEletroValueBox;
        }
        
        e.Column.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

        // 将生成的列添加到映射字典中
        columnMapping[header] = e.Column;
        
        // 如果筛选模式已开启且列是可见的，添加筛选按钮
        if (ViewModel.IsFilterModeEnabled && isVisible)
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
        var defaultFields = ViewModel.GetDefaultField();
        
        // 构建当前列的可见性状态
        var currentVisibility = new Dictionary<string, bool>();
        foreach (var kvp in columnMapping)
        {
            currentVisibility[kvp.Key] = kvp.Value.Visibility == Visibility.Visible;
        }
        
        var dialog = new ColumnSelectionWindow(defaultFields, currentVisibility)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            // 保存用户的选择
            foreach (var kvp in dialog.ColumnVisibility)
            {
                userColumnVisibility[kvp.Key] = kvp.Value; // 保存用户选择
            }
            
            // 清空列映射，强制重新生成列
            columnMapping.Clear();
            
            // 刷新DataGrid以重新生成列
            RefreshDataGrid();
        }
    }
}
