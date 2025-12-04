﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System.Data;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Data;
using System.Globalization;
using System.Collections;
using IODataPlatform.Models;
using IODataPlatform.Views.Windows;


namespace IODataPlatform.Views.Pages;

public partial class DataComparePage : INavigableView<DataCompareViewModel> {
    private readonly ExcelService excel;
    public readonly GlobalModel model;

    // 差异定位状态
    private int currentDiffRowIndex = -1; // 当前定位的行索引
    private int currentDiffColumnIndex = -1; // 当前定位的列索引（在差异列列表中的索引）
    private List<string> currentRowDiffColumns = new(); // 当前行的差异列列表

    public DataCompareViewModel ViewModel { get; }

    public DataComparePage(DataCompareViewModel viewModel, ExcelService excel, GlobalModel model) {
        ViewModel = viewModel;
        this.excel = excel;
        this.model = model;
        DataContext = this;

        InitializeComponent();
        
        // 设置 DataGrid 的单元格选中模式
        if (OldDataGrid != null)
        {
            OldDataGrid.SelectionUnit = DataGridSelectionUnit.Cell;
            OldDataGrid.SelectionMode = DataGridSelectionMode.Single;
        }
        if (NewDataGrid != null)
        {
            NewDataGrid.SelectionUnit = DataGridSelectionUnit.Cell;
            NewDataGrid.SelectionMode = DataGridSelectionMode.Single;
        }
        if (VerticalComparisonDataGrid != null)
        {
            VerticalComparisonDataGrid.SelectionUnit = DataGridSelectionUnit.Cell;
            VerticalComparisonDataGrid.SelectionMode = DataGridSelectionMode.Single;
        }
        
        // 监听属性变化
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        // 页面加载后最大化窗口
        Loaded += (s, e) =>
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = WindowState.Maximized;
                
                // 添加键盘快捷键支持
                window.PreviewKeyDown += Window_PreviewKeyDown;
            }
        };
        
        Unloaded += (s, e) =>
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.PreviewKeyDown -= Window_PreviewKeyDown;
            }
        };
    }

    // 键盘快捷键处理
    private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        // Ctrl + Up: 上一处差异
        if (e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Control && 
            e.Key == System.Windows.Input.Key.Up)
        {
            if (ViewModel.HasComparisonResults)
            {
                GotoPreviousDiff(null, null);
                e.Handled = true;
            }
        }
        // Ctrl + Down: 下一处差异
        else if (e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Control && 
                 e.Key == System.Windows.Input.Key.Down)
        {
            if (ViewModel.HasComparisonResults)
            {
                GotoNextDiff(null, null);
                e.Handled = true;
            }
        }
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.DisplayColumns))
        {
            UpdateDataGridColumns();
        }
        else if (e.PropertyName == nameof(ViewModel.IsSideBySideMode))
        {
            UpdateDataGridColumns();
        }
        else if (e.PropertyName == nameof(ViewModel.ShowDiffColumnsOnly))
        {
            UpdateDataGridColumns();
        }
        else if (e.PropertyName == nameof(ViewModel.ShowKeyColumns))
        {
            // 主键列显示/隐藏状态变化，重新生成列
            UpdateDataGridColumns();
        }
    }

    // 创建斜线阴影画刷
    private DrawingBrush CreateHatchBrush()
    {
        var geometryGroup = new GeometryGroup();
        
        // 创建多条斜线
        for (int i = -10; i <= 10; i++)
        {
            geometryGroup.Children.Add(new LineGeometry(
                new Point(i * 10, 0),
                new Point(i * 10 + 10, 10)
            ));
        }

        var geometryDrawing = new GeometryDrawing
        {
            Geometry = geometryGroup,
            Pen = new Pen(new SolidColorBrush(Color.FromRgb(200, 200, 200)), 1)
        };

        var drawingBrush = new DrawingBrush
        {
            Drawing = geometryDrawing,
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, 10, 10),
            ViewportUnits = BrushMappingMode.Absolute,
            Viewbox = new Rect(0, 0, 10, 10),
            ViewboxUnits = BrushMappingMode.Absolute
        };

        return drawingBrush;
    }

    // 主键按钮点击：打开主键选择窗口
    private void KeyFieldsButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.AvailableKeyFields.Count == 0)
        {
            model.Status.Error("请先选择Excel文件和工作表");
            return;
        }

        var dialog = new KeyFieldSelectionWindow(
            ViewModel.AvailableKeyFields.ToList(),
            ViewModel.SelectedKeyFields.ToList())
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            ViewModel.SelectedKeyFields.Clear();

            if (dialog.IsOrderMode)
            {
                // 按顺序对比
                ViewModel.KeyFieldsDisplay = "按顺序对比";
            }
            else
            {
                // 收集选中的字段
                foreach (var field in dialog.SelectedFields)
                {
                    ViewModel.SelectedKeyFields.Add(field);
                }

                if (ViewModel.SelectedKeyFields.Count == 0)
                {
                    ViewModel.KeyFieldsDisplay = "按顺序对比";
                }
                else if (ViewModel.SelectedKeyFields.Count == 1)
                {
                    ViewModel.KeyFieldsDisplay = ViewModel.SelectedKeyFields[0];
                }
                else
                {
                    ViewModel.KeyFieldsDisplay = string.Join("+", ViewModel.SelectedKeyFields);
                }
            }
        }
    }

    // 差异定位：上一处差异
    private void GotoPreviousDiff(object sender, RoutedEventArgs e)
    {
        NavigateToDiff(false);
    }

    // 差异定位：下一处差异
    private void GotoNextDiff(object sender, RoutedEventArgs e)
    {
        NavigateToDiff(true);
    }

    // 差异定位核心逻辑：从当前位置开始，向下/向上查找下一个差异单元格
    private void NavigateToDiff(bool isNext)
    {
        if (ViewModel.FilteredResults == null)
        {
            model.Status.Error("没有对比结果");
            return;
        }

        var results = ViewModel.FilteredResults.SourceCollection.Cast<ComparisonRow>().ToList();
        
        if (results.Count == 0)
        {
            model.Status.Error("没有对比结果");
            return;
        }
        
        // 查找所有有差异的行
        var diffRows = results
            .Select((row, index) => new { Row = row, Index = index })
            .Where(x => x.Row.Type != ComparisonType.Unchanged && 
                       x.Row.ChangedFields != null && 
                       x.Row.ChangedFields.Any(kv => kv.Value))
            .ToList();

        if (diffRows.Count == 0)
        {
            model.Status.Error("没有找到差异记录");
            DiffPositionText.Text = "无差异";
            return;
        }

        // 获取当前选中的行索引
        int currentSelectedIndex = GetCurrentSelectedRowIndex(results);

        int targetRowIndex = -1;
        string targetColumn = null;

        if (isNext)
        {
            // 下一处：先在当前行查找下一个差异列
            if (currentDiffRowIndex == currentSelectedIndex && currentRowDiffColumns.Count > 0)
            {
                // 在同一行内查找下一个差异列
                currentDiffColumnIndex++;
                if (currentDiffColumnIndex < currentRowDiffColumns.Count)
                {
                    targetRowIndex = currentDiffRowIndex;
                    targetColumn = currentRowDiffColumns[currentDiffColumnIndex];
                }
                else
                {
                    // 当前行的差异列已经遍历完，跳转到下一个差异行
                    var nextRow = diffRows.FirstOrDefault(x => x.Index > currentDiffRowIndex);
                    if (nextRow != null)
                    {
                        targetRowIndex = nextRow.Index;
                        currentRowDiffColumns = nextRow.Row.ChangedFields
                            .Where(kv => kv.Value)
                            .Select(kv => kv.Key)
                            .ToList();
                        currentDiffColumnIndex = 0;
                        targetColumn = currentRowDiffColumns.FirstOrDefault();
                    }
                    else
                    {
                        // 循环到第一个差异行
                        targetRowIndex = diffRows.First().Index;
                        currentRowDiffColumns = diffRows.First().Row.ChangedFields
                            .Where(kv => kv.Value)
                            .Select(kv => kv.Key)
                            .ToList();
                        currentDiffColumnIndex = 0;
                        targetColumn = currentRowDiffColumns.FirstOrDefault();
                        model.Status.Message("已到最后一处差异，跳转到第一处");
                    }
                }
            }
            else
            {
                // 查找第一个大于当前选中行的差异行
                var nextRow = diffRows.FirstOrDefault(x => x.Index > currentSelectedIndex);
                if (nextRow != null)
                {
                    targetRowIndex = nextRow.Index;
                }
                else
                {
                    // 循环到第一个
                    targetRowIndex = diffRows.First().Index;
                    model.Status.Message("已到最后一处差异，跳转到第一处");
                }
                
                // 重新设置当前行的差异列
                var targetRow = results[targetRowIndex];
                currentRowDiffColumns = targetRow.ChangedFields
                    .Where(kv => kv.Value)
                    .Select(kv => kv.Key)
                    .ToList();
                currentDiffColumnIndex = 0;
                targetColumn = currentRowDiffColumns.FirstOrDefault();
            }
        }
        else
        {
            // 上一处：先在当前行查找上一个差异列
            if (currentDiffRowIndex == currentSelectedIndex && currentRowDiffColumns.Count > 0)
            {
                currentDiffColumnIndex--;
                if (currentDiffColumnIndex >= 0)
                {
                    targetRowIndex = currentDiffRowIndex;
                    targetColumn = currentRowDiffColumns[currentDiffColumnIndex];
                }
                else
                {
                    // 当前行的第一个差异列，跳转到上一个差异行的最后一个差异列
                    var prevRow = diffRows.LastOrDefault(x => x.Index < currentDiffRowIndex);
                    if (prevRow != null)
                    {
                        targetRowIndex = prevRow.Index;
                        currentRowDiffColumns = prevRow.Row.ChangedFields
                            .Where(kv => kv.Value)
                            .Select(kv => kv.Key)
                            .ToList();
                        currentDiffColumnIndex = currentRowDiffColumns.Count - 1;
                        targetColumn = currentRowDiffColumns.LastOrDefault();
                    }
                    else
                    {
                        // 循环到最后一个差异行的最后一个差异列
                        targetRowIndex = diffRows.Last().Index;
                        currentRowDiffColumns = diffRows.Last().Row.ChangedFields
                            .Where(kv => kv.Value)
                            .Select(kv => kv.Key)
                            .ToList();
                        currentDiffColumnIndex = currentRowDiffColumns.Count - 1;
                        targetColumn = currentRowDiffColumns.LastOrDefault();
                        model.Status.Message("已到第一处差异，跳转到最后一处");
                    }
                }
            }
            else
            {
                // 查找最后一个小于当前选中行的差异行
                var prevRow = diffRows.LastOrDefault(x => x.Index < currentSelectedIndex);
                if (prevRow != null)
                {
                    targetRowIndex = prevRow.Index;
                }
                else
                {
                    // 循环到最后一个
                    targetRowIndex = diffRows.Last().Index;
                    model.Status.Message("已到第一处差异，跳转到最后一处");
                }
                
                // 设置为该行的最后一个差异列
                var targetRow = results[targetRowIndex];
                currentRowDiffColumns = targetRow.ChangedFields
                    .Where(kv => kv.Value)
                    .Select(kv => kv.Key)
                    .ToList();
                currentDiffColumnIndex = currentRowDiffColumns.Count - 1;
                targetColumn = currentRowDiffColumns.LastOrDefault();
            }
        }

        if (targetRowIndex >= 0 && targetColumn != null)
        {
            // 更新当前定位状态
            currentDiffRowIndex = targetRowIndex;
            
            // 滚动到目标行并选中
            ScrollToRow(targetRowIndex);
            
            // 滚动到目标列（单元格级别）
            ScrollToCellColumn(targetColumn);
            
            // 显示当前位置信息
            var targetRow = results[targetRowIndex];
            var diffType = targetRow.Type == ComparisonType.Added ? "新增" : 
                          targetRow.Type == ComparisonType.Deleted ? "删除" : "修改";
            var totalDiffCells = diffRows.Sum(x => x.Row.ChangedFields.Count(kv => kv.Value));
            DiffPositionText.Text = $"{diffType} - 列: {targetColumn} ({currentDiffColumnIndex + 1}/{currentRowDiffColumns.Count})";
            
            model.Status.Success($"已定位到: 行{targetRow.RowIndex}, 列'{targetColumn}'");
        }
    }

    // 获取当前选中的行索引
    private int GetCurrentSelectedRowIndex(List<ComparisonRow> results)
    {
        if (ViewModel.IsSideBySideMode)
        {
            // 左右对比模式：从 OldDataGrid 或 NewDataGrid 获取
            return OldDataGrid.SelectedIndex >= 0 ? OldDataGrid.SelectedIndex : NewDataGrid.SelectedIndex;
        }
        else
        {
            // 上下对比模式：从 VerticalComparisonDataGrid 获取
            if (VerticalComparisonDataGrid.SelectedItem is VerticalComparisonRow verticalRow)
            {
                // 找到对应的 ComparisonRow 索引
                return results.FindIndex(r => r.RowIndex == verticalRow.RowIndex);
            }
        }
        return -1;
    }

    // 滚动到指定行并选中
    private void ScrollToRow(int index)
    {
        if (ViewModel.IsSideBySideMode)
        {
            // 左右对比模式：同时选中和滚动两个 DataGrid
            if (index >= 0 && index < OldDataGrid.Items.Count)
            {
                OldDataGrid.SelectedIndex = index;
                OldDataGrid.UpdateLayout();
                OldDataGrid.ScrollIntoView(OldDataGrid.Items[index]);
                OldDataGrid.Focus();
            }
            if (index >= 0 && index < NewDataGrid.Items.Count)
            {
                NewDataGrid.SelectedIndex = index;
                NewDataGrid.UpdateLayout();
                NewDataGrid.ScrollIntoView(NewDataGrid.Items[index]);
            }
        }
        else
        {
            // 上下对比模式：需要找到对应的 VerticalComparisonRow
            var verticalData = VerticalComparisonDataGrid.ItemsSource as IEnumerable;
            if (verticalData != null)
            {
                var results = ViewModel.FilteredResults.SourceCollection.Cast<ComparisonRow>().ToList();
                if (index < 0 || index >= results.Count) return;
                
                var targetRowIndex = results[index].RowIndex;
                var verticalRows = verticalData.Cast<VerticalComparisonRow>().ToList();
                
                // 找到对应的新数据行（版本="新"）
                var targetRow = verticalRows.FirstOrDefault(r => r.RowIndex == targetRowIndex && r.RowVersion == "新");
                if (targetRow != null)
                {
                    var verticalIndex = verticalRows.IndexOf(targetRow);
                    VerticalComparisonDataGrid.SelectedIndex = verticalIndex;
                    VerticalComparisonDataGrid.UpdateLayout();
                    VerticalComparisonDataGrid.ScrollIntoView(targetRow);
                    VerticalComparisonDataGrid.Focus();
                }
                else
                {
                    // 如果没有新数据行（删除记录），尝试查找旧数据行
                    var oldRow = verticalRows.FirstOrDefault(r => r.RowIndex == targetRowIndex && r.RowVersion == "旧");
                    if (oldRow != null)
                    {
                        var verticalIndex = verticalRows.IndexOf(oldRow);
                        VerticalComparisonDataGrid.SelectedIndex = verticalIndex;
                        VerticalComparisonDataGrid.UpdateLayout();
                        VerticalComparisonDataGrid.ScrollIntoView(oldRow);
                        VerticalComparisonDataGrid.Focus();
                    }
                }
            }
        }
    }

    // 滚动到指定行的指定列（单元格级别）
    private void ScrollToCellColumn(string columnName)
    {
        if (string.IsNullOrEmpty(columnName)) return;

        if (ViewModel.IsSideBySideMode)
        {
            // 左右对比模式：滚动两个 DataGrid 到指定列
            ScrollDataGridToColumn(OldDataGrid, columnName);
            ScrollDataGridToColumn(NewDataGrid, columnName);
        }
        else
        {
            // 上下对比模式：滚动到指定列
            ScrollDataGridToColumn(VerticalComparisonDataGrid, columnName);
        }
    }

    // 滚动 DataGrid 到指定列
    private void ScrollDataGridToColumn(System.Windows.Controls.DataGrid dataGrid, string columnName)
    {
        if (dataGrid == null || dataGrid.Columns.Count == 0) return;

        // 查找列索引
        var column = dataGrid.Columns.FirstOrDefault(c => 
            c.Header?.ToString()?.Replace(" [主键]", "") == columnName);
        
        if (column != null)
        {
            var columnIndex = dataGrid.Columns.IndexOf(column);
            
            // 使用 Dispatcher 确保在 UI 更新后执行
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // 滚动到指定列
                    dataGrid.ScrollIntoView(dataGrid.SelectedItem, column);
                    
                    // 设置当前单元格（高亮显示）
                    if (dataGrid.SelectedItem != null)
                    {
                        dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.SelectedItem, column);
                        
                        // 强制进入编辑模式然后退出，以触发选中效果
                        dataGrid.Focus();
                        dataGrid.BeginEdit();
                        dataGrid.CommitEdit();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"滚动到列失败: {ex.Message}");
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private void UpdateDataGridColumns()
    {
        if (ViewModel.ComparisonResults == null || ViewModel.ComparisonResults.Count == 0) return;

        var columns = ViewModel.DisplayColumns;

        if (ViewModel.IsSideBySideMode)
        {
            // 左右对比模式：更新两个DataGrid
            UpdateSideBySideGrids(columns);
        }
        else
        {
            // 上下对比模式：更新单个DataGrid
            UpdateVerticalGrid(columns);
        }
    }

    // 左右对比：两个DataGrid显示相同的列，一个显示旧数据，一个显示新数据
    private void UpdateSideBySideGrids(List<string> columns)
    {
        OldDataGrid.Columns.Clear();
        NewDataGrid.Columns.Clear();

        // 检查数据源是否有序号列（支持多种名称）
        var hasNumberColumn = columns.Any(c => 
            c.Equals("序号", StringComparison.OrdinalIgnoreCase) ||
            c.Equals("#", StringComparison.OrdinalIgnoreCase) ||
            c.Equals("NO", StringComparison.OrdinalIgnoreCase) ||
            c.Equals("Number", StringComparison.OrdinalIgnoreCase));

        int frozenCount = 0;

        // 第一列：序号列（总是显示）
        if (!hasNumberColumn)
        {
            // 数据源没有序号列，自动生成
            var oldIndexColumn = new DataGridTextColumn
            {
                Header = "#",
                Binding = new Binding("RowIndex"),
                Width = new DataGridLength(50),
                IsReadOnly = true,
                CanUserReorder = false
            };
            OldDataGrid.Columns.Add(oldIndexColumn);

            var newIndexColumn = new DataGridTextColumn
            {
                Header = "#",
                Binding = new Binding("RowIndex"),
                Width = new DataGridLength(50),
                IsReadOnly = true,
                CanUserReorder = false
            };
            NewDataGrid.Columns.Add(newIndexColumn);
            frozenCount++;
            System.Diagnostics.Debug.WriteLine("左右对比：添加自动生成的序号列");
        }
        else
        {
            // 数据源有序号列，但也要显示（不跳过）
            frozenCount++;
        }

        // 然后添加主键列（固定在序号后面）
        var keyFields = ViewModel.SelectedKeyFields.ToList();
        System.Diagnostics.Debug.WriteLine($"左右对比：选中的主键字段数量 = {keyFields.Count}, ShowKeyColumns = {ViewModel.ShowKeyColumns}");
        
        // 只有在 ShowKeyColumns = true 时才添加主键列
        if (ViewModel.ShowKeyColumns)
        {
            foreach (var keyField in keyFields)
            {
                System.Diagnostics.Debug.WriteLine($"左右对比：检查主键字段 '{keyField}'，是否在columns中：{columns.Contains(keyField)}");
                if (columns.Contains(keyField))
                {
                    // 主键列使用专用模板：始终显示黑色文字
                    var oldKeyColumn = new DataGridTemplateColumn
                    {
                        Header = keyField + " [主键]",
                        Width = DataGridLength.Auto,
                        CellTemplate = CreateSideBySideKeyFieldCellTemplate(keyField, true), // true = 旧数据
                        CanUserReorder = false
                    };
                    OldDataGrid.Columns.Add(oldKeyColumn);

                    var newKeyColumn = new DataGridTemplateColumn
                    {
                        Header = keyField + " [主键]",
                        Width = DataGridLength.Auto,
                        CellTemplate = CreateSideBySideKeyFieldCellTemplate(keyField, false), // false = 新数据
                        CanUserReorder = false
                    };
                    NewDataGrid.Columns.Add(newKeyColumn);
                    frozenCount++;
                    System.Diagnostics.Debug.WriteLine($"左右对比：已添加主键列 '{keyField}'");
                }
            }
        }

        // 添加其他数据列（跳过主键列和序号列）
        foreach (var colName in columns)
        {
            // 跳过已经添加的主键列和序号列
            if (keyFields.Contains(colName) || IsNumberColumn(colName))
                continue;

            var oldColumn = new DataGridTemplateColumn
            {
                Header = colName,
                Width = DataGridLength.Auto,
                CellTemplate = CreateOldValueCellTemplate(colName)
            };
            OldDataGrid.Columns.Add(oldColumn);

            var newColumn = new DataGridTemplateColumn
            {
                Header = colName,
                Width = DataGridLength.Auto,
                CellTemplate = CreateNewValueCellTemplate(colName)
            };
            NewDataGrid.Columns.Add(newColumn);
        }

        System.Diagnostics.Debug.WriteLine($"左右对比：总列数 = {OldDataGrid.Columns.Count}，冻结列数 = {frozenCount}");

        // 绑定数据源
        OldDataGrid.ItemsSource = ViewModel.FilteredResults.SourceCollection;
        NewDataGrid.ItemsSource = ViewModel.FilteredResults.SourceCollection;
        
        // 使用 Dispatcher 延迟设置冻结列，确保列已经完全生成
        Dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                OldDataGrid.FrozenColumnCount = frozenCount;
                NewDataGrid.FrozenColumnCount = frozenCount;
                
                System.Diagnostics.Debug.WriteLine($"左右对比设置冻结列数: {frozenCount}, 总列数: {OldDataGrid.Columns.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"左右对比设置冻结列失败: {ex.Message}");
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);
        
        // 添加滚动同步
        SyncScrollViewers(OldDataGrid, NewDataGrid);
        
        // 添加选中同步
        SyncDataGridSelection(OldDataGrid, NewDataGrid);
    }

    // 上下对比：单个DataGrid，每条记录显示为两行
    private void UpdateVerticalGrid(List<string> columns)
    {
        VerticalComparisonDataGrid.Columns.Clear();

        int frozenColumnCount = 0;

        // 检查数据源是否有序号列（支持多种名称）
        var hasNumberColumn = columns.Any(c => 
            c.Equals("序号", StringComparison.OrdinalIgnoreCase) ||
            c.Equals("#", StringComparison.OrdinalIgnoreCase) ||
            c.Equals("NO", StringComparison.OrdinalIgnoreCase) ||
            c.Equals("Number", StringComparison.OrdinalIgnoreCase));

        // 第一列：序号列（固定）
        if (!hasNumberColumn)
        {
            // 数据源没有序号列，自动生成
            var indexColumn = new DataGridTextColumn
            {
                Header = "#",
                Binding = new Binding("RowIndex"),
                Width = new DataGridLength(50),
                IsReadOnly = true,
                CanUserReorder = false
            };
            VerticalComparisonDataGrid.Columns.Add(indexColumn);
            frozenColumnCount++;
            System.Diagnostics.Debug.WriteLine("上下对比：添加自动生成的序号列");
        }

        // 添加版本列（固定）
        var versionColumn = new DataGridTextColumn
        {
            Header = "版本",
            Binding = new Binding("RowVersion"),
            Width = new DataGridLength(60),
            CanUserReorder = false
        };
        VerticalComparisonDataGrid.Columns.Add(versionColumn);
        frozenColumnCount++;

        // 添加主键列（固定在左侧）
        var keyFields = ViewModel.SelectedKeyFields.ToList();
        System.Diagnostics.Debug.WriteLine($"上下对比：选中的主键字段数量 = {keyFields.Count}, ShowKeyColumns = {ViewModel.ShowKeyColumns}");
        
        // 只有在 ShowKeyColumns = true 时才添加主键列
        if (ViewModel.ShowKeyColumns)
        {
            foreach (var keyField in keyFields)
            {
                System.Diagnostics.Debug.WriteLine($"上下对比：检查主键字段 '{keyField}'，是否在columns中：{columns.Contains(keyField)}");
                if (columns.Contains(keyField))
                {
                    var keyColumn = new DataGridTemplateColumn
                    {
                        Header = keyField + " [主键]",
                        Width = DataGridLength.Auto,
                        CellTemplate = CreateVerticalKeyFieldCellTemplate(keyField), // 使用主键专用模板
                        CanUserReorder = false
                    };
                    VerticalComparisonDataGrid.Columns.Add(keyColumn);
                    frozenColumnCount++;
                    System.Diagnostics.Debug.WriteLine($"上下对比：已添加主键列 '{keyField}'");
                }
            }
        }

        // 添加其他数据列（非主键列且非序号列）
        foreach (var colName in columns)
        {
            // 跳过主键列和序号列（已经添加）
            if (!keyFields.Contains(colName) && !IsNumberColumn(colName))
            {
                var column = new DataGridTemplateColumn
                {
                    Header = colName,
                    Width = DataGridLength.Auto,
                    CellTemplate = CreateVerticalCellTemplate(colName)
                };
                VerticalComparisonDataGrid.Columns.Add(column);
            }
        }

        System.Diagnostics.Debug.WriteLine($"上下对比：总列数 = {VerticalComparisonDataGrid.Columns.Count}，冻结列数 = {frozenColumnCount}");

        // 设置冻结列数
        Dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                VerticalComparisonDataGrid.FrozenColumnCount = frozenColumnCount;
                System.Diagnostics.Debug.WriteLine($"上下对比设置冻结列数: {frozenColumnCount}, 总列数: {VerticalComparisonDataGrid.Columns.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"上下对比设置冻结列失败: {ex.Message}");
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);

        // 转换数据：每个ComparisonRow转为两行（旧数据+新数据）
        var verticalData = ConvertToVerticalData(ViewModel.FilteredResults.SourceCollection);
        VerticalComparisonDataGrid.ItemsSource = verticalData;
    }

    // 判断是否为序号列
    private bool IsNumberColumn(string columnName)
    {
        return columnName.Equals("序号", StringComparison.OrdinalIgnoreCase) ||
               columnName.Equals("#", StringComparison.OrdinalIgnoreCase) ||
               columnName.Equals("NO", StringComparison.OrdinalIgnoreCase) ||
               columnName.Equals("Number", StringComparison.OrdinalIgnoreCase);
    }

    // 创建旧数据单元格模板
    private DataTemplate CreateOldValueCellTemplate(string columnName)
    {
        var template = new DataTemplate();
        var factory = new FrameworkElementFactory(typeof(Border));
        factory.SetValue(Border.PaddingProperty, new Thickness(5));

        var textFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.TextBlock));
        textFactory.SetValue(System.Windows.Controls.TextBlock.TextWrappingProperty, TextWrapping.Wrap);

        // 获取旧数据的值
        var valueBinding = new MultiBinding();
        valueBinding.Bindings.Add(new Binding("OldRow"));
        valueBinding.Converter = new DataRowColumnValueConverter { ColumnName = columnName };
        textFactory.SetBinding(System.Windows.Controls.TextBlock.TextProperty, valueBinding);

        // 样式：根据是否有差异设置颜色
        var textStyle = new Style(typeof(System.Windows.Controls.TextBlock));
        
        // 默认浅灰色（无差异）
        textStyle.Setters.Add(new Setter(System.Windows.Controls.TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(150, 150, 150))));
        
        // 如果有差异，文字标红
        var changedTrigger = new DataTrigger
        {
            Binding = new MultiBinding
            {
                Bindings = { new Binding("ChangedFields") },
                Converter = new ChangedFieldsContainsKeyConverter { ColumnName = columnName }
            },
            Value = true
        };
        changedTrigger.Setters.Add(new Setter(System.Windows.Controls.TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(244, 67, 54))));
        textStyle.Triggers.Add(changedTrigger);
        
        textFactory.SetValue(System.Windows.Controls.TextBlock.StyleProperty, textStyle);
        factory.AppendChild(textFactory);

        // 背景样式：右有左无时显示阴影
        var bgStyle = new Style(typeof(Border));
        var missingTrigger = new DataTrigger
        {
            Binding = new MultiBinding
            {
                Bindings = 
                { 
                    new Binding("OldRow"),
                    new Binding("CurrentRow")
                },
                Converter = new IsColumnMissingConverter { ColumnName = columnName, CheckOldMissing = true }
            },
            Value = true
        };
        missingTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CreateHatchBrush()));
        bgStyle.Triggers.Add(missingTrigger);
        factory.SetValue(Border.StyleProperty, bgStyle);

        template.VisualTree = factory;
        return template;
    }

    // 创建新数据单元格模板
    private DataTemplate CreateNewValueCellTemplate(string columnName)
    {
        var template = new DataTemplate();
        var factory = new FrameworkElementFactory(typeof(Border));
        factory.SetValue(Border.PaddingProperty, new Thickness(5));

        var textFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.TextBlock));
        textFactory.SetValue(System.Windows.Controls.TextBlock.TextWrappingProperty, TextWrapping.Wrap);

        // 获取新数据的值
        var valueBinding = new MultiBinding();
        valueBinding.Bindings.Add(new Binding("CurrentRow"));
        valueBinding.Converter = new DataRowColumnValueConverter { ColumnName = columnName };
        textFactory.SetBinding(System.Windows.Controls.TextBlock.TextProperty, valueBinding);

        // 样式：根据是否有差异设置颜色
        var textStyle = new Style(typeof(System.Windows.Controls.TextBlock));
        
        // 默认浅灰色（无差异）
        textStyle.Setters.Add(new Setter(System.Windows.Controls.TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(150, 150, 150))));
        
        // 如果有差异，文字标红
        var changedTrigger = new DataTrigger
        {
            Binding = new MultiBinding
            {
                Bindings = { new Binding("ChangedFields") },
                Converter = new ChangedFieldsContainsKeyConverter { ColumnName = columnName }
            },
            Value = true
        };
        changedTrigger.Setters.Add(new Setter(System.Windows.Controls.TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(244, 67, 54))));
        textStyle.Triggers.Add(changedTrigger);
        
        textFactory.SetValue(System.Windows.Controls.TextBlock.StyleProperty, textStyle);
        factory.AppendChild(textFactory);

        // 背景样式：左有右无时显示阴影
        var bgStyle = new Style(typeof(Border));
        var missingTrigger = new DataTrigger
        {
            Binding = new MultiBinding
            {
                Bindings = 
                { 
                    new Binding("OldRow"),
                    new Binding("CurrentRow")
                },
                Converter = new IsColumnMissingConverter { ColumnName = columnName, CheckOldMissing = false }
            },
            Value = true
        };
        missingTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CreateHatchBrush()));
        bgStyle.Triggers.Add(missingTrigger);
        factory.SetValue(Border.StyleProperty, bgStyle);

        template.VisualTree = factory;
        return template;
    }

    // 创建上下对比模式的单元格模板
    private DataTemplate CreateVerticalCellTemplate(string columnName)
    {
        var template = new DataTemplate();
        var factory = new FrameworkElementFactory(typeof(Border));
        factory.SetValue(Border.PaddingProperty, new Thickness(5));

        var textFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.TextBlock));
        textFactory.SetValue(System.Windows.Controls.TextBlock.TextWrappingProperty, TextWrapping.Wrap);

        // 获取值
        var valueBinding = new MultiBinding();
        valueBinding.Bindings.Add(new Binding("DataRow"));
        valueBinding.Converter = new DataRowColumnValueConverter { ColumnName = columnName };
        textFactory.SetBinding(System.Windows.Controls.TextBlock.TextProperty, valueBinding);

        // 样式：根据是否有差异设置颜色
        var textStyle = new Style(typeof(System.Windows.Controls.TextBlock));
        
        // 默认浅灰色（无差异）
        textStyle.Setters.Add(new Setter(System.Windows.Controls.TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(150, 150, 150))));
        
        // 如果有差异，文字标红
        var changedTrigger = new DataTrigger
        {
            Binding = new MultiBinding
            {
                Bindings = { new Binding("ChangedFields") },
                Converter = new ChangedFieldsContainsKeyConverter { ColumnName = columnName }
            },
            Value = true
        };
        changedTrigger.Setters.Add(new Setter(System.Windows.Controls.TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(244, 67, 54))));
        textStyle.Triggers.Add(changedTrigger);
        
        textFactory.SetValue(System.Windows.Controls.TextBlock.StyleProperty, textStyle);
        factory.AppendChild(textFactory);

        // 背景样式：一边有值一边没有时显示阴影
        var bgStyle = new Style(typeof(Border));
        
        // 旧数据行：如果新数据有这列但旧数据没有，显示阴影
        var oldMissingTrigger = new MultiDataTrigger();
        oldMissingTrigger.Conditions.Add(new Condition
        {
            Binding = new Binding("RowVersion"),
            Value = "旧"
        });
        oldMissingTrigger.Conditions.Add(new Condition
        {
            Binding = new MultiBinding
            {
                Bindings = 
                { 
                    new Binding("OldRow"),
                    new Binding("NewRow")
                },
                Converter = new IsColumnMissingConverter { ColumnName = columnName, CheckOldMissing = true }
            },
            Value = true
        });
        oldMissingTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CreateHatchBrush()));
        bgStyle.Triggers.Add(oldMissingTrigger);
        
        // 新数据行：如果旧数据有这列但新数据没有，显示阴影
        var newMissingTrigger = new MultiDataTrigger();
        newMissingTrigger.Conditions.Add(new Condition
        {
            Binding = new Binding("RowVersion"),
            Value = "新"
        });
        newMissingTrigger.Conditions.Add(new Condition
        {
            Binding = new MultiBinding
            {
                Bindings = 
                { 
                    new Binding("OldRow"),
                    new Binding("NewRow")
                },
                Converter = new IsColumnMissingConverter { ColumnName = columnName, CheckOldMissing = false }
            },
            Value = true
        });
        newMissingTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CreateHatchBrush()));
        bgStyle.Triggers.Add(newMissingTrigger);
        
        factory.SetValue(Border.StyleProperty, bgStyle);

        template.VisualTree = factory;
        return template;
    }

    // 创建上下对比模式的主键字段单元格模板（始终显示值，黑色文字）
    private DataTemplate CreateVerticalKeyFieldCellTemplate(string columnName)
    {
        var template = new DataTemplate();
        var factory = new FrameworkElementFactory(typeof(Border));
        factory.SetValue(Border.PaddingProperty, new Thickness(5));

        var textFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.TextBlock));
        textFactory.SetValue(System.Windows.Controls.TextBlock.TextWrappingProperty, TextWrapping.Wrap);

        // 获取值
        var valueBinding = new MultiBinding();
        valueBinding.Bindings.Add(new Binding("DataRow"));
        valueBinding.Converter = new DataRowColumnValueConverter { ColumnName = columnName };
        textFactory.SetBinding(System.Windows.Controls.TextBlock.TextProperty, valueBinding);

        // 主键字段始终使用黑色文字，不管是否有差异
        textFactory.SetValue(System.Windows.Controls.TextBlock.ForegroundProperty, 
            new SolidColorBrush(Colors.Black));
        
        factory.AppendChild(textFactory);

        template.VisualTree = factory;
        return template;
    }

    // 创建左右对比模式的主键字段单元格模板（始终显示值，黑色文字）
    private DataTemplate CreateSideBySideKeyFieldCellTemplate(string columnName, bool isOldData)
    {
        var template = new DataTemplate();
        var factory = new FrameworkElementFactory(typeof(Border));
        factory.SetValue(Border.PaddingProperty, new Thickness(5));

        var textFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.TextBlock));
        textFactory.SetValue(System.Windows.Controls.TextBlock.TextWrappingProperty, TextWrapping.Wrap);

        // 获取值：根据是旧数据还是新数据选择对应的字段
        var valueBinding = new MultiBinding();
        valueBinding.Bindings.Add(new Binding(isOldData ? "OldRow" : "CurrentRow"));
        valueBinding.Converter = new DataRowColumnValueConverter { ColumnName = columnName };
        textFactory.SetBinding(System.Windows.Controls.TextBlock.TextProperty, valueBinding);

        // 主键字段始终使用黑色文字，不管是否有差异
        textFactory.SetValue(System.Windows.Controls.TextBlock.ForegroundProperty, 
            new SolidColorBrush(Colors.Black));
        textFactory.SetValue(System.Windows.Controls.TextBlock.FontWeightProperty, FontWeights.Bold);
        
        factory.AppendChild(textFactory);

        template.VisualTree = factory;
        return template;
    }

    // 转换为上下对比数据
    private IEnumerable ConvertToVerticalData(IEnumerable source)
    {
        var result = new List<VerticalComparisonRow>();
        
        foreach (var item in source)
        {
            if (item is ComparisonRow row)
            {
                // 只处理差异行（新增、修改、删除），跳过未变更的记录
                if (row.Type == ComparisonType.Unchanged)
                    continue;

                // 旧数据行
                if (row.OldRow != null)
                {
                    result.Add(new VerticalComparisonRow
                    {
                        RowIndex = row.RowIndex,
                        RowVersion = "旧",
                        Type = row.Type,
                        DataRow = row.OldRow,
                        OldRow = row.OldRow,
                        NewRow = row.CurrentRow,
                        ChangedFields = row.ChangedFields,
                        IsColumnChanged = false
                    });
                }

                // 新数据行
                if (row.CurrentRow != null && row.Type != ComparisonType.Deleted)
                {
                    result.Add(new VerticalComparisonRow
                    {
                        RowIndex = row.RowIndex,
                        RowVersion = "新",
                        Type = row.Type,
                        DataRow = row.CurrentRow,
                        OldRow = row.OldRow,
                        NewRow = row.CurrentRow,
                        ChangedFields = row.ChangedFields,
                        IsColumnChanged = false
                    });
                }
            }
        }
        
        return result;
    }

    // 同步两个DataGrid的滚动
    private void SyncScrollViewers(System.Windows.Controls.DataGrid grid1, System.Windows.Controls.DataGrid grid2)
    {
        // 获取ScrollViewer
        var sv1 = FindVisualChild<ScrollViewer>(grid1);
        var sv2 = FindVisualChild<ScrollViewer>(grid2);

        if (sv1 != null && sv2 != null)
        {
            bool isSyncing = false;

            sv1.ScrollChanged += (s, e) =>
            {
                if (!isSyncing)
                {
                    isSyncing = true;
                    sv2.ScrollToVerticalOffset(sv1.VerticalOffset);
                    sv2.ScrollToHorizontalOffset(sv1.HorizontalOffset);
                    isSyncing = false;
                }
            };

            sv2.ScrollChanged += (s, e) =>
            {
                if (!isSyncing)
                {
                    isSyncing = true;
                    sv1.ScrollToVerticalOffset(sv2.VerticalOffset);
                    sv1.ScrollToHorizontalOffset(sv2.HorizontalOffset);
                    isSyncing = false;
                }
            };
        }
    }

    // 同步两个DataGrid的选中状态
    private void SyncDataGridSelection(System.Windows.Controls.DataGrid grid1, System.Windows.Controls.DataGrid grid2)
    {
        bool isSyncing = false;

        grid1.SelectionChanged += (s, e) =>
        {
            if (!isSyncing && grid1.SelectedIndex >= 0)
            {
                isSyncing = true;
                grid2.SelectedIndex = grid1.SelectedIndex;
                isSyncing = false;
            }
        };

        grid2.SelectionChanged += (s, e) =>
        {
            if (!isSyncing && grid2.SelectedIndex >= 0)
            {
                isSyncing = true;
                grid1.SelectedIndex = grid2.SelectedIndex;
                isSyncing = false;
            }
        };
    }

    // 查找可视化树中的子元素
    private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
                return result;

            var childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
                return childOfChild;
        }
        return null;
    }

    private async void Compare(object sender, RoutedEventArgs e) {
        if (!File.Exists(ViewModel.FilePath1) || ViewModel.FilePath1 == "请选择Excel文件1") { 
            model.Status.Error("请选择Excel文件1"); 
            return; 
        }
        if (!File.Exists(ViewModel.FilePath2) || ViewModel.FilePath2 == "请选择Excel文件2") { 
            model.Status.Error("请选择Excel文件2"); 
            return; 
        }
        // 不再强制要求主键,允许按顺序对比
        if (string.IsNullOrEmpty(ViewModel.SelectedSheet1)) { 
            model.Status.Error("请选择文件1的工作表"); 
            return; 
        }
        if (string.IsNullOrEmpty(ViewModel.SelectedSheet2)) { 
            model.Status.Error("请选择文件2的工作表"); 
            return; 
        }

        // 开始对比，初始化进度
        ViewModel.IsComparing = true;
        ViewModel.ProgressValue = 0;
        ViewModel.ProgressMessage = "正在加载数据...";
        model.Status.Busy("正在比对数据");
        ViewModel.StatusMessage = "正在加载数据...";

        try {
            // 加载第一个文件数据
            ViewModel.ProgressMessage = "正在加载文件1数据...";
            using var oldData1 = await excel.GetDataTableAsStringAsync(ViewModel.FilePath1, ViewModel.SelectedSheet1, true);
            
            // 加载第二个文件数据
            ViewModel.ProgressValue = 20;
            ViewModel.ProgressMessage = "正在加载文件2数据...";
            using var oldData2 = await excel.GetDataTableAsStringAsync(ViewModel.FilePath2, ViewModel.SelectedSheet2, true);
            
            // 如果使用主键对比,检查主键唯一性
            if (ViewModel.SelectedKeyFields.Count > 0)
            {
                var duplicateKeys = CheckKeyUniqueness(oldData1, oldData2);
                if (duplicateKeys.Count > 0)
                {
                    // 显示重复主键警告窗口
                    var warningWindow = new DuplicateKeyWarningWindow(duplicateKeys)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    
                    var result = warningWindow.ShowDialog();
                    
                    // 如果用户选择取消,则中止对比
                    if (result != true || !warningWindow.ContinueComparison)
                    {
                        model.Status.Error("已取消对比");
                        return;
                    }
                    
                    // 用户选择继续,显示警告信息
                    model.Status.Error("检测到主键重复,对比结果可能不准确");
                }
            }
            
            // 开始数据对比
            ViewModel.ProgressValue = 40;
            ViewModel.ProgressMessage = "正在对比数据...";
            
            // 执行对比
            var comparisonResults = await PerformComparison(oldData1, oldData2);
            
            // 更新UI
            ViewModel.ComparisonResults.Clear();
            foreach (var result in comparisonResults) {
                ViewModel.ComparisonResults.Add(result);
            }
            
            // 对比完成，更新进度到100%
            ViewModel.ProgressValue = 100;
            ViewModel.ProgressMessage = "对比完成！";
            
            // 更新统计信息
            ViewModel.UpdateStatistics();
            
            model.Status.Success($"对比完成，共找到 {comparisonResults.Count} 条记录");
            
            // 重置进度属性
            ViewModel.IsComparing = false;
            ViewModel.ProgressValue = 0;
            ViewModel.ProgressMessage = string.Empty;
        }
        catch (Exception ex) {
            model.Status.Error($"对比失败：{ex.Message}");
            ViewModel.StatusMessage = $"错误：{ex.Message}";
            
            // 重置进度属性
            ViewModel.IsComparing = false;
            ViewModel.ProgressValue = 0;
            ViewModel.ProgressMessage = string.Empty;
        }
    }

    // 检查主键唯一性
    private List<DuplicateKeyInfo> CheckKeyUniqueness(DataTable oldData, DataTable newData)
    {
        var duplicateKeys = new List<DuplicateKeyInfo>();
        
        // 检查旧数据文件
        var oldDuplicates = FindDuplicateKeys(oldData, ViewModel.FileName1);
        duplicateKeys.AddRange(oldDuplicates);
        
        // 检查新数据文件
        var newDuplicates = FindDuplicateKeys(newData, ViewModel.FileName2);
        duplicateKeys.AddRange(newDuplicates);
        
        return duplicateKeys;
    }
    
    // 在单个 DataTable 中查找重复的主键
    private List<DuplicateKeyInfo> FindDuplicateKeys(DataTable data, string fileName)
    {
        var duplicates = new List<DuplicateKeyInfo>();
        var keyCount = new Dictionary<string, int>();
        
        // 统计每个主键值出现的次数
        foreach (DataRow row in data.Rows)
        {
            var keyParts = new List<string>();
            foreach (var fieldName in ViewModel.SelectedKeyFields)
            {
                if (data.Columns.Contains(fieldName))
                {
                    keyParts.Add(row[fieldName]?.ToString() ?? "");
                }
            }
            var key = string.Join("|", keyParts);
            
            if (!string.IsNullOrEmpty(key))
            {
                if (keyCount.ContainsKey(key))
                    keyCount[key]++;
                else
                    keyCount[key] = 1;
            }
        }
        
        // 找出重复的主键（出现次数 > 1）
        foreach (var kvp in keyCount)
        {
            if (kvp.Value > 1)
            {
                duplicates.Add(new DuplicateKeyInfo
                {
                    FileName = fileName,
                    KeyValue = kvp.Key.Replace("|", " + "), // 显示为更友好的格式
                    Count = kvp.Value
                });
            }
        }
        
        return duplicates;
    }

    private async Task<List<ComparisonRow>> PerformComparison(DataTable oldData, DataTable newData) {
        return await Task.Run(() => {
            var results = new List<ComparisonRow>();
            
            // 判断是否按顺序对比
            if (ViewModel.SelectedKeyFields.Count == 0)
            {
                // 按顺序对比：直接按行号对应
                int maxRows = Math.Max(oldData.Rows.Count, newData.Rows.Count);
                
                for (int i = 0; i < maxRows; i++)
                {
                    // 更新进度
                    int progress = 40 + (int)((i + 1) / (double)maxRows * 50);
                    if (progress != ViewModel.ProgressValue)
                    {
                        ViewModel.ProgressValue = progress;
                        ViewModel.ProgressMessage = $"正在对比第 {i + 1}/{maxRows} 行...";
                    }
                    
                    DataRow oldRow = i < oldData.Rows.Count ? oldData.Rows[i] : null;
                    DataRow newRow = i < newData.Rows.Count ? newData.Rows[i] : null;
                    
                    if (oldRow == null && newRow != null)
                    {
                        // 新增：旧数据没有这一行
                        results.Add(new ComparisonRow
                        {
                            Key = $"行{i + 1}",
                            CurrentRow = newRow,
                            Type = ComparisonType.Added
                        });
                    }
                    else if (oldRow != null && newRow == null)
                    {
                        // 删除：新数据没有这一行
                        results.Add(new ComparisonRow
                        {
                            Key = $"行{i + 1}",
                            OldRow = oldRow,
                            Type = ComparisonType.Deleted
                        });
                    }
                    else if (oldRow != null && newRow != null)
                    {
                        // 比较两行是否相同
                        var changedFields = new Dictionary<string, bool>();
                        bool hasChanges = false;
                        
                        foreach (DataColumn col in newData.Columns)
                        {
                            var oldValue = oldData.Columns.Contains(col.ColumnName) 
                                ? (oldRow[col.ColumnName]?.ToString() ?? "") 
                                : "";
                            var newValue = newRow[col.ColumnName]?.ToString() ?? "";
                            bool isChanged = oldValue != newValue;
                            changedFields[col.ColumnName] = isChanged;
                            if (isChanged) hasChanges = true;
                        }
                        
                        results.Add(new ComparisonRow
                        {
                            Key = $"行{i + 1}",
                            CurrentRow = newRow,
                            OldRow = oldRow,
                            Type = hasChanges ? ComparisonType.Modified : ComparisonType.Unchanged,
                            ChangedFields = changedFields
                        });
                    }
                }
            }
            else
            {
                // 使用主键对比：支持多主键
                var oldKeyIndex = new Dictionary<string, DataRow>();
                var newKeyIndex = new Dictionary<string, DataRow>();
                var processedKeys = new HashSet<string>();
                
                // 构建索引：将多个主键字段的值拼接为一个键
                foreach (DataRow row in oldData.Rows) {
                    var keyParts = new List<string>();
                    foreach (var fieldName in ViewModel.SelectedKeyFields)
                    {
                        if (oldData.Columns.Contains(fieldName))
                        {
                            keyParts.Add(row[fieldName]?.ToString() ?? "");
                        }
                    }
                    var key = string.Join("|", keyParts);
                    if (!string.IsNullOrEmpty(key)) {
                        oldKeyIndex[key] = row;
                    }
                }
                
                foreach (DataRow row in newData.Rows) {
                    var keyParts = new List<string>();
                    foreach (var fieldName in ViewModel.SelectedKeyFields)
                    {
                        if (newData.Columns.Contains(fieldName))
                        {
                            keyParts.Add(row[fieldName]?.ToString() ?? "");
                        }
                    }
                    var key = string.Join("|", keyParts);
                    if (!string.IsNullOrEmpty(key)) {
                        newKeyIndex[key] = row;
                    }
                }
                
                // 先处理新数据中的记录（保持新数据的原始顺序）
                int totalRows = newData.Rows.Count + oldData.Rows.Count;
                int processedRows = 0;
                
                foreach (DataRow newRow in newData.Rows) {
                    processedRows++;
                    // 更新进度
                    int progress = 40 + (int)(processedRows / (double)totalRows * 50);
                    if (progress != ViewModel.ProgressValue)
                    {
                        ViewModel.ProgressValue = progress;
                        ViewModel.ProgressMessage = $"正在对比第 {processedRows}/{totalRows} 条记录...";
                    }
                    var keyParts = new List<string>();
                    foreach (var fieldName in ViewModel.SelectedKeyFields)
                    {
                        if (newData.Columns.Contains(fieldName))
                        {
                            keyParts.Add(newRow[fieldName]?.ToString() ?? "");
                        }
                    }
                    var key = string.Join("|", keyParts);
                    if (string.IsNullOrEmpty(key)) continue;
                    
                    processedKeys.Add(key);
                    
                    if (oldKeyIndex.ContainsKey(key)) {
                        // 都存在，检查是否有变化
                        var oldRow = oldKeyIndex[key];
                        
                        var changedFields = new Dictionary<string, bool>();
                        bool hasChanges = false;
                        
                        foreach (DataColumn col in newData.Columns) {
                            var oldValue = oldData.Columns.Contains(col.ColumnName)
                                ? (oldRow[col.ColumnName]?.ToString() ?? "")
                                : "";
                            var newValue = newRow[col.ColumnName]?.ToString() ?? "";
                            bool isChanged = oldValue != newValue;
                            changedFields[col.ColumnName] = isChanged;
                            if (isChanged) hasChanges = true;
                        }
                        
                        var comparisonRow = new ComparisonRow {
                            Key = key,
                            CurrentRow = newRow,
                            OldRow = oldRow,
                            Type = hasChanges ? ComparisonType.Modified : ComparisonType.Unchanged,
                            ChangedFields = changedFields
                        };
                        results.Add(comparisonRow);
                    }
                    else {
                        // 只在新数据中存在，表示新增
                        var comparisonRow = new ComparisonRow {
                            Key = key,
                            CurrentRow = newRow,
                            Type = ComparisonType.Added
                        };
                        results.Add(comparisonRow);
                    }
                }
                
                // 处理旧数据中剩余的记录（删除的情况）
                foreach (DataRow oldRow in oldData.Rows) {
                    processedRows++;
                    // 更新进度
                    int progress = 40 + (int)(processedRows / (double)totalRows * 50);
                    if (progress != ViewModel.ProgressValue)
                    {
                        ViewModel.ProgressValue = progress;
                        ViewModel.ProgressMessage = $"正在对比第 {processedRows}/{totalRows} 条记录...";
                    }
                    var keyParts = new List<string>();
                    foreach (var fieldName in ViewModel.SelectedKeyFields)
                    {
                        if (oldData.Columns.Contains(fieldName))
                        {
                            keyParts.Add(oldRow[fieldName]?.ToString() ?? "");
                        }
                    }
                    var key = string.Join("|", keyParts);
                    if (string.IsNullOrEmpty(key)) continue;
                    
                    if (!processedKeys.Contains(key)) {
                        // 只在旧数据中存在，表示删除
                        var comparisonRow = new ComparisonRow {
                            Key = key,
                            OldRow = oldRow,
                            Type = ComparisonType.Deleted
                        };
                        results.Add(comparisonRow);
                    }
                }
            }
            
            // 为所有结果设置行序号
            for (int i = 0; i < results.Count; i++)
            {
                results[i].RowIndex = i + 1;
            }
            
            return results;
        });
    }

    // 拖拽处理
    private async void Page_Drop(object sender, DragEventArgs e) {
        if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            
            // 过滤出Excel文件
            var excelFiles = files.Where(f => File.Exists(f) && (f.EndsWith(".xlsx") || f.EndsWith(".xls"))).ToArray();
            
            if (excelFiles.Length == 0)
            {
                model.Status.Error("请拖拽Excel文件");
                return;
            }

            if (excelFiles.Length == 1)
            {
                // 拖拽单个文件：智能分配到空位或替换
                string file = excelFiles[0];
                if (ViewModel.FilePath1 == "请选择Excel文件1" || string.IsNullOrEmpty(ViewModel.FilePath1))
                {
                    await LoadFileAsync(file, 1);
                }
                else if (ViewModel.FilePath2 == "请选择Excel文件2" || string.IsNullOrEmpty(ViewModel.FilePath2))
                {
                    await LoadFileAsync(file, 2);
                }
                else
                {
                    // 两个文件都已选择，默认替换文件2（新版本）
                    await LoadFileAsync(file, 2);
                }
            }
            else if (excelFiles.Length >= 2)
            {
                // 拖拽多个文件：第一个为旧版本，第二个为新版本
                await LoadFileAsync(excelFiles[0], 1);
                await LoadFileAsync(excelFiles[1], 2);
                model.Status.Success($"已加载 {excelFiles.Length} 个文件，按顺序分配为旧/新版本");
            }
        }
    }

    // 加载文件的通用方法
    private async Task LoadFileAsync(string file, int fileNumber)
    {
        try
        {
            var sheetNames = await excel.GetSheetNamesAsync(file);
            
            if (fileNumber == 1)
            {
                ViewModel.FilePath1 = file;
                ViewModel.FileName1 = Path.GetFileName(file);
                ViewModel.SheetNames1.Clear();
                foreach (var sheetName in sheetNames)
                {
                    ViewModel.SheetNames1.Add(sheetName);
                }
                if (ViewModel.SheetNames1.Count > 0)
                    ViewModel.SelectedSheet1 = ViewModel.SheetNames1[0];
            }
            else
            {
                ViewModel.FilePath2 = file;
                ViewModel.FileName2 = Path.GetFileName(file);
                ViewModel.SheetNames2.Clear();
                foreach (var sheetName in sheetNames)
                {
                    ViewModel.SheetNames2.Add(sheetName);
                }
                if (ViewModel.SheetNames2.Count > 0)
                    ViewModel.SelectedSheet2 = ViewModel.SheetNames2[0];
            }
        }
        catch (Exception ex)
        {
            model.Status.Error($"加载文件失败：{ex.Message}");
        }
    }

    // 互换新旧文件
    private void SwapFiles(object sender, RoutedEventArgs e)
    {
        // 交换文件路径
        var tempPath = ViewModel.FilePath1;
        ViewModel.FilePath1 = ViewModel.FilePath2;
        ViewModel.FilePath2 = tempPath;

        // 交换文件名
        var tempName = ViewModel.FileName1;
        ViewModel.FileName1 = ViewModel.FileName2;
        ViewModel.FileName2 = tempName;

        // 交换工作表列表
        var tempSheets = new ObservableCollection<string>(ViewModel.SheetNames1);
        ViewModel.SheetNames1.Clear();
        foreach (var sheet in ViewModel.SheetNames2)
        {
            ViewModel.SheetNames1.Add(sheet);
        }
        ViewModel.SheetNames2.Clear();
        foreach (var sheet in tempSheets)
        {
            ViewModel.SheetNames2.Add(sheet);
        }

        // 交换选中的工作表
        var tempSelected = ViewModel.SelectedSheet1;
        ViewModel.SelectedSheet1 = ViewModel.SelectedSheet2;
        ViewModel.SelectedSheet2 = tempSelected;

        // 确保有默认选中的sheet
        if (string.IsNullOrEmpty(ViewModel.SelectedSheet1) && ViewModel.SheetNames1.Count > 0)
        {
            ViewModel.SelectedSheet1 = ViewModel.SheetNames1[0];
        }
        if (string.IsNullOrEmpty(ViewModel.SelectedSheet2) && ViewModel.SheetNames2.Count > 0)
        {
            ViewModel.SelectedSheet2 = ViewModel.SheetNames2[0];
        }

        model.Status.Success("已互换新旧文件");
    }

    // 筛选按钮点击事件
    private void ToggleAdded(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowAdded = !ViewModel.ShowAdded;
    }

    private void ToggleModified(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowModified = !ViewModel.ShowModified;
    }

    private void ToggleDeleted(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowDeleted = !ViewModel.ShowDeleted;
    }

    private void ToggleUnchanged(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowUnchanged = !ViewModel.ShowUnchanged;
    }

    // 列显示模式按钮点击事件
    private void ShowDiffColumnsOnly(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowDiffColumnsOnly = true;
    }

    private void ShowAllColumns(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowDiffColumnsOnly = false;
    }

    // 视图模式按钮点击事件
    private void ShowSideBySideMode(object sender, RoutedEventArgs e)
    {
        ViewModel.IsSideBySideMode = true;
    }

    private void ShowVerticalMode(object sender, RoutedEventArgs e)
    {
        ViewModel.IsSideBySideMode = false;
    }

    // 主键列显示/隐藏按钮点击事件
    private void ToggleKeyColumns(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowKeyColumns = !ViewModel.ShowKeyColumns;
    }

    // 导出按钮鼠标按下事件（支持点击和拖拽）
    private Point exportButtonMouseDownPos;
    private void ExportButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (!ViewModel.HasComparisonResults)
            return;

        exportButtonMouseDownPos = e.GetPosition(null);

        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
            {
                button.PreviewMouseMove += ExportButton_PreviewMouseMove;
                button.PreviewMouseUp += ExportButton_PreviewMouseUp;
            }
        }
    }

    private async void ExportButton_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            Point currentPos = e.GetPosition(null);
            Vector diff = exportButtonMouseDownPos - currentPos;

            // 判断是否开始拖拽（移动距离超过阈值）
            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.PreviewMouseMove -= ExportButton_PreviewMouseMove;
                    button.PreviewMouseUp -= ExportButton_PreviewMouseUp;
                }

                // 开始拖拽操作
                await PerformDragExport();
            }
        }
    }

    private async void ExportButton_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var button = sender as System.Windows.Controls.Button;
        if (button != null)
        {
            button.PreviewMouseMove -= ExportButton_PreviewMouseMove;
            button.PreviewMouseUp -= ExportButton_PreviewMouseUp;
        }

        // 如果没有发生拖拽，则执行点击导出到桌面
        Point currentPos = e.GetPosition(null);
        Vector diff = exportButtonMouseDownPos - currentPos;
        if (Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance)
        {
            await ViewModel.ExportResultsToDesktopCommand.ExecuteAsync(null);
        }
    }

    private async Task PerformDragExport()
    {
        try
        {
            // 生成临时文件
            var tempPath = Path.Combine(Path.GetTempPath(), $"数据对比结果_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            
            model.Status.Busy("正在准备导出文件...");

            await Task.Run(() =>
            {
                var workbook = excel.GetWorkbook();
                var worksheet = workbook.Worksheets[0];
                worksheet.Name = "对比结果";

                // 获取所有列名
                var columnNames = new List<string>();
                if (ViewModel.ComparisonResults.FirstOrDefault()?.CurrentRow != null)
                {
                    var table = ViewModel.ComparisonResults.First().CurrentRow.Table;
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        columnNames.Add(table.Columns[i].ColumnName);
                    }
                }
                else if (ViewModel.ComparisonResults.FirstOrDefault()?.OldRow != null)
                {
                    var table = ViewModel.ComparisonResults.First().OldRow.Table;
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        columnNames.Add(table.Columns[i].ColumnName);
                    }
                }

                // 写入表头
                worksheet.Cells[0, 0].PutValue("变更类型");
                worksheet.Cells[0, 1].PutValue("主键");
                for (int i = 0; i < columnNames.Count; i++)
                {
                    worksheet.Cells[0, i + 2].PutValue(columnNames[i]);
                }

                // 写入数据
                int row = 1;
                foreach (var result in ViewModel.ComparisonResults)
                {
                    string typeText = result.Type switch
                    {
                        ComparisonType.Added => "新增",
                        ComparisonType.Modified => "修改",
                        ComparisonType.Deleted => "删除",
                        ComparisonType.Unchanged => "未变更",
                        _ => result.Type.ToString()
                    };
                    worksheet.Cells[row, 0].PutValue(typeText);
                    worksheet.Cells[row, 1].PutValue(result.Key);

                    var rowStyle = workbook.CreateStyle();
                    if (result.Type == ComparisonType.Added)
                    {
                        rowStyle.BackgroundColor = System.Drawing.Color.FromArgb(255, 249, 196);
                        rowStyle.Pattern = Aspose.Cells.BackgroundType.Solid;
                    }
                    else if (result.Type == ComparisonType.Deleted)
                    {
                        rowStyle.BackgroundColor = System.Drawing.Color.FromArgb(245, 245, 245);
                        rowStyle.Pattern = Aspose.Cells.BackgroundType.Solid;
                        rowStyle.Font.IsStrikeout = true;
                    }
                    else if (result.Type == ComparisonType.Modified)
                    {
                        rowStyle.BackgroundColor = System.Drawing.Color.FromArgb(255, 205, 210);
                        rowStyle.Pattern = Aspose.Cells.BackgroundType.Solid;
                    }

                    worksheet.Cells[row, 0].SetStyle(rowStyle);
                    worksheet.Cells[row, 1].SetStyle(rowStyle);

                    var dataRow = result.CurrentRow ?? result.OldRow;
                    if (dataRow != null)
                    {
                        for (int i = 0; i < columnNames.Count; i++)
                        {
                            var columnName = columnNames[i];
                            var value = dataRow.Table.Columns.Contains(columnName)
                                ? dataRow[columnName]?.ToString() ?? ""
                                : "";
                            var cell = worksheet.Cells[row, i + 2];
                            cell.PutValue(value);

                            if (result.Type == ComparisonType.Modified && result.ChangedFields.ContainsKey(columnName) && result.ChangedFields[columnName])
                            {
                                var diffStyle = workbook.CreateStyle();
                                diffStyle.BackgroundColor = System.Drawing.Color.FromArgb(255, 224, 178);
                                diffStyle.Pattern = Aspose.Cells.BackgroundType.Solid;
                                diffStyle.Font.IsBold = true;
                                cell.SetStyle(diffStyle);
                            }
                            else
                            {
                                cell.SetStyle(rowStyle);
                            }
                        }
                    }

                    row++;
                }

                worksheet.AutoFitColumns();
                workbook.Save(tempPath);
            });

            // 执行拖拽操作
            var dataObject = new DataObject(DataFormats.FileDrop, new string[] { tempPath });
            DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);

            model.Status.Success("拖拽导出完成");
        }
        catch (Exception ex)
        {
            model.Status.Error($"拖拽导出失败：{ex.Message}");
        }
    }
}

// 上下对比模式的数据行
public class VerticalComparisonRow
{
    public int RowIndex { get; set; } // 行序号
    public string RowVersion { get; set; } = string.Empty; // "旧" 或 "新"
    public ComparisonType Type { get; set; }
    public DataRow DataRow { get; set; }
    public DataRow OldRow { get; set; } // 旧数据行（用于阴影判断）
    public DataRow NewRow { get; set; } // 新数据行（用于阴影判断）
    public Dictionary<string, bool> ChangedFields { get; set; } = new();
    public bool IsColumnChanged { get; set; }
}

// DataRow列值转换器
public class DataRowColumnValueConverter : IMultiValueConverter
{
    public string ColumnName { get; set; } = string.Empty;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length > 0 && values[0] is DataRow row && row != null)
        {
            try
            {
                if (row.Table.Columns.Contains(ColumnName))
                {
                    return row[ColumnName]?.ToString() ?? "";
                }
            }
            catch
            {
                return "";
            }
        }
        return "";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// ChangedFields字典包含Key检查转换器
public class ChangedFieldsContainsKeyConverter : IMultiValueConverter
{
    public string ColumnName { get; set; } = string.Empty;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length > 0 && values[0] is Dictionary<string, bool> changedFields)
        {
            return changedFields.TryGetValue(ColumnName, out var changed) && changed;
        }
        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// 转换器：根据展开状态返回按钮内容
public class ExpandButtonContentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "-" : "+";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// 转换器：Bool 到 Appearance（选中时Primary，未选中时Secondary）
public class BoolToAppearanceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isTrue && isTrue)
        {
            return Wpf.Ui.Controls.ControlAppearance.Primary;
        }
        return Wpf.Ui.Controls.ControlAppearance.Secondary;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// 转换器：反向Bool 到 Appearance（选中时Secondary，未选中时Primary）
public class InverseBoolToAppearanceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isTrue && isTrue)
        {
            return Wpf.Ui.Controls.ControlAppearance.Secondary;
        }
        return Wpf.Ui.Controls.ControlAppearance.Primary;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// 转换器：Bool 到主键列按钮文字
public class BoolToKeyColumnTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isTrue && isTrue)
        {
            return "隐藏主键";
        }
        return "显示主键";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// 转换器：判断列是否缺失（左有右无/右有左无）
public class IsColumnMissingConverter : IMultiValueConverter
{
    public string ColumnName { get; set; } = string.Empty;
    public bool CheckOldMissing { get; set; } // true=检查OldRow是否缺失此列，false=检查CurrentRow是否缺失此列

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return false;

        var oldRow = values[0] as DataRow;
        var currentRow = values[1] as DataRow;

        if (CheckOldMissing)
        {
            // 检查右有左无：CurrentRow有值但OldRow没有此列或值为空
            if (currentRow != null && currentRow.Table.Columns.Contains(ColumnName))
            {
                var currentValue = currentRow[ColumnName]?.ToString() ?? "";
                if (oldRow == null || !oldRow.Table.Columns.Contains(ColumnName))
                {
                    return !string.IsNullOrWhiteSpace(currentValue);
                }
                var oldValue = oldRow[ColumnName]?.ToString() ?? "";
                return string.IsNullOrWhiteSpace(oldValue) && !string.IsNullOrWhiteSpace(currentValue);
            }
        }
        else
        {
            // 检查左有右无：OldRow有值但CurrentRow没有此列或值为空
            if (oldRow != null && oldRow.Table.Columns.Contains(ColumnName))
            {
                var oldValue = oldRow[ColumnName]?.ToString() ?? "";
                if (currentRow == null || !currentRow.Table.Columns.Contains(ColumnName))
                {
                    return !string.IsNullOrWhiteSpace(oldValue);
                }
                var currentValue = currentRow[ColumnName]?.ToString() ?? "";
                return string.IsNullOrWhiteSpace(currentValue) && !string.IsNullOrWhiteSpace(oldValue);
            }
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// 智能数据行列值转换器：主键列始终显示，非主键列仅在有差异时显示
public class SmartDataRowColumnValueConverter : IMultiValueConverter
{
    public string ColumnName { get; set; } = string.Empty;
    public bool IsKeyField { get; set; } = false; // 是否是主键字段

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 1 || values[0] is not DataRow row || row == null)
            return "";

        try
        {
            if (!row.Table.Columns.Contains(ColumnName))
                return "";

            var cellValue = row[ColumnName]?.ToString() ?? "";

            // 主键字段：始终显示内容
            if (IsKeyField)
                return cellValue;

            // 非主键字段：只在有差异时显示内容
            if (values.Length >= 2 && values[1] is Dictionary<string, bool> changedFields)
            {
                // 如果该列有差异，显示内容
                if (changedFields.TryGetValue(ColumnName, out var changed) && changed)
                {
                    return cellValue;
                }
                // 该列无差异，返回空
                return "";
            }

            // 默认显示内容（兜底）
            return cellValue;
        }
        catch
        {
            return "";
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}