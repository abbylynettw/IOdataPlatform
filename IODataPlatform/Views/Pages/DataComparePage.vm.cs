using System.IO;
using System.Data;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media;
using IODataPlatform.Models;
using IODataPlatform.Services;
using Wpf.Ui.Controls;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using System.Collections.Generic;
using Aspose.Cells;
using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.Pages;

public partial class DataCompareViewModel : ObservableObject, INavigationAware
{
    private readonly IPickerService picker;
    private readonly ExcelService excel;
    private readonly GlobalModel model;

    public DataCompareViewModel(IPickerService picker, ExcelService excel, GlobalModel model)
    {
        this.picker = picker;
        this.excel = excel;
        this.model = model;
    }

    [ObservableProperty]
    private string filePath1 = "请选择Excel文件1";

    [ObservableProperty]
    private string filePath2 = "请选择Excel文件2";

    [ObservableProperty]
    private string fileName1 = "请选择Excel文件1";

    [ObservableProperty]
    private string fileName2 = "请选择Excel文件2";

    [ObservableProperty]
    private string fieldName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> selectedKeyFields = new(); // 多主键选择

    [ObservableProperty]
    private string keyFieldsDisplay = "按顺序对比"; // 显示在界面上的主键文本

    [ObservableProperty]
    private ObservableCollection<string> availableKeyFields = new();

    [ObservableProperty]
    private ObservableCollection<string> sheetNames1 = new();

    [ObservableProperty]
    private ObservableCollection<string> sheetNames2 = new();

    [ObservableProperty]
    private string selectedSheet1 = string.Empty;

    [ObservableProperty]
    private string selectedSheet2 = string.Empty;

    partial void OnSelectedSheet1Changed(string value)
    {
        if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(FilePath1))
        {
            _ = LoadAvailableKeyFieldsAsync();
        }
    }

    partial void OnSelectedSheet2Changed(string value)
    {
        if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(FilePath2))
        {
            _ = LoadAvailableKeyFieldsAsync();
        }
    }

    private async Task LoadAvailableKeyFieldsAsync()
    {
        try
        {
            // 优先使用文件1的列名，如果不存在则使用文件2
            string filePath = !string.IsNullOrEmpty(FilePath1) && !string.IsNullOrEmpty(SelectedSheet1) 
                ? FilePath1 
                : FilePath2;
            string sheetName = !string.IsNullOrEmpty(FilePath1) && !string.IsNullOrEmpty(SelectedSheet1) 
                ? SelectedSheet1 
                : SelectedSheet2;

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(sheetName))
                return;

            using var dataTable = await excel.GetDataTableAsStringAsync(filePath, sheetName, true);
            
            AvailableKeyFields.Clear();
            foreach (DataColumn column in dataTable.Columns)
            {
                AvailableKeyFields.Add(column.ColumnName);
            }

            // 如果当前选中的字段不在列表中，清空选择
            if (!string.IsNullOrEmpty(FieldName) && !AvailableKeyFields.Contains(FieldName))
            {
                FieldName = string.Empty;
            }

            // 如果没有选择主键，且存在常见的主键字段，自动选择
            if (string.IsNullOrEmpty(FieldName) && AvailableKeyFields.Count > 0)
            {
                var commonKeyFields = new[] { "序号", "ID", "Id", "id", "位号", "TagName", "名称" };
                foreach (var commonKey in commonKeyFields)
                {
                    if (AvailableKeyFields.Contains(commonKey))
                    {
                        FieldName = commonKey;
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            model.Status.Error($"读取列名失败：{ex.Message}");
        }
    }

    [ObservableProperty]
    private ObservableCollection<ComparisonRow> comparisonResults = new();

    [ObservableProperty]
    private bool hasComparisonResults = false;

    [ObservableProperty]
    private int totalCount;

    [ObservableProperty]
    private int addedCount;

    [ObservableProperty]
    private int modifiedCount;

    [ObservableProperty]
    private int deletedCount;

    [ObservableProperty]
    private int unchangedCount;

    [ObservableProperty]
    private bool showAdded = true;

    [ObservableProperty]
    private bool showModified = true;

    [ObservableProperty]
    private bool showDeleted = true;

    [ObservableProperty]
    private bool showUnchanged = false;

    [ObservableProperty]
    private bool showDiffColumnsOnly = true;

    partial void OnShowDiffColumnsOnlyChanged(bool value)
    {
        // 刷新显示列
        OnPropertyChanged(nameof(DisplayColumns));
    }

    [ObservableProperty]
    private bool isSideBySideMode = true;

    partial void OnIsSideBySideModeChanged(bool value)
    {
        // 切换视图模式
        OnPropertyChanged(nameof(FilteredResults));
    }

    [ObservableProperty]
    private bool showKeyColumns = true; // 是否显示主键列

    partial void OnShowKeyColumnsChanged(bool value)
    {
        // 显示/隐藏主键列，需要重新更新列
        OnPropertyChanged(nameof(DisplayColumns));
    }

    [ObservableProperty]
    private string statusMessage = "就绪";

    // 分页相关属性
    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int pageSize = 100;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private bool canGoPreviousPage = false;

    [ObservableProperty]
    private bool canGoNextPage = false;

    private List<ComparisonRow> allFilteredResults = new();

    partial void OnCurrentPageChanged(int value)
    {
        UpdatePagedResults();
        canGoPreviousPage = value > 1;
        canGoNextPage = value < totalPages;
        OnPropertyChanged(nameof(CanGoPreviousPage));
        OnPropertyChanged(nameof(CanGoNextPage));
    }

    partial void OnPageSizeChanged(int value)
    {
        currentPage = 1;
        OnPropertyChanged(nameof(CurrentPage));
        UpdatePagination();
    }

    private ICollectionView? filteredResultsView;

    public ICollectionView FilteredResults
    {
        get
        {
            if (filteredResultsView == null && ComparisonResults != null)
            {
                filteredResultsView = CollectionViewSource.GetDefaultView(ComparisonResults);
                filteredResultsView.Filter = FilterComparisonResults;
            }
            return filteredResultsView;
        }
    }

    // 获取应显示的列名列表
    public List<string> DisplayColumns
    {
        get
        {
            if (ComparisonResults == null || ComparisonResults.Count == 0)
                return new List<string>();

            var firstRow = ComparisonResults.FirstOrDefault();
            if (firstRow == null)
                return new List<string>();

            if (ShowDiffColumnsOnly)
            {
                // 仅显示差异列：收集所有有变化的列
                var changedColumns = new HashSet<string>();
                foreach (var row in ComparisonResults)
                {
                    foreach (var col in row.GetChangedColumnNames())
                    {
                        changedColumns.Add(col);
                    }
                }
                
                // 确保主键列总是包含在显示列中
                foreach (var keyField in SelectedKeyFields)
                {
                    changedColumns.Add(keyField);
                }
                
                // 确保序号列总是包含在显示列中（如果数据源有序号列）
                var allColumns = firstRow.GetAllColumnNames().ToList();
                var numberColumn = allColumns.FirstOrDefault(c => 
                    c.Equals("序号", StringComparison.OrdinalIgnoreCase) ||
                    c.Equals("#", StringComparison.OrdinalIgnoreCase) ||
                    c.Equals("NO", StringComparison.OrdinalIgnoreCase) ||
                    c.Equals("Number", StringComparison.OrdinalIgnoreCase));
                if (numberColumn != null)
                {
                    changedColumns.Add(numberColumn);
                }
                
                return changedColumns.OrderBy(c => c).ToList();
            }
            else
            {
                // 显示所有列
                return firstRow.GetAllColumnNames().ToList();
            }
        }
    }

    partial void OnShowAddedChanged(bool value) => RefreshFilter();
    partial void OnShowModifiedChanged(bool value) => RefreshFilter();
    partial void OnShowDeletedChanged(bool value) => RefreshFilter();
    partial void OnShowUnchangedChanged(bool value) => RefreshFilter();

    private bool FilterComparisonResults(object obj)
    {
        if (obj is ComparisonRow row)
        {
            return row.Type switch
            {
                ComparisonType.Added => ShowAdded,
                ComparisonType.Modified => ShowModified,
                ComparisonType.Deleted => ShowDeleted,
                ComparisonType.Unchanged => ShowUnchanged,
                _ => true
            };
        }
        return false;
    }

    private void RefreshFilter()
    {
        filteredResultsView?.Refresh();
        UpdatePagination();
    }

    private void UpdatePagination()
    {
        // 获取筛选后的所有数据
        allFilteredResults = ComparisonResults
            .Where(r => r.Type switch
            {
                ComparisonType.Added => ShowAdded,
                ComparisonType.Modified => ShowModified,
                ComparisonType.Deleted => ShowDeleted,
                ComparisonType.Unchanged => ShowUnchanged,
                _ => true
            })
            .ToList();

        // 计算总页数
        totalPages = allFilteredResults.Count > 0 
            ? (int)Math.Ceiling((double)allFilteredResults.Count / pageSize) 
            : 1;
        OnPropertyChanged(nameof(TotalPages));

        // 确保当前页码合法
        if (currentPage > totalPages)
        {
            currentPage = totalPages;
            OnPropertyChanged(nameof(CurrentPage));
        }
        if (currentPage < 1)
        {
            currentPage = 1;
            OnPropertyChanged(nameof(CurrentPage));
        }

        UpdatePagedResults();
    }

    private void UpdatePagedResults()
    {
        // 获取当前页的数据
        var pagedData = allFilteredResults
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // 更新视图
        if (filteredResultsView == null)
        {
            filteredResultsView = CollectionViewSource.GetDefaultView(pagedData);
        }
        else
        {
            filteredResultsView = CollectionViewSource.GetDefaultView(pagedData);
        }

        OnPropertyChanged(nameof(FilteredResults));

        // 更新分页按钮状态
        canGoPreviousPage = currentPage > 1;
        canGoNextPage = currentPage < totalPages;
        OnPropertyChanged(nameof(CanGoPreviousPage));
        OnPropertyChanged(nameof(CanGoNextPage));
    }

    [RelayCommand]
    private void FirstPage()
    {
        currentPage = 1;
        OnPropertyChanged(nameof(CurrentPage));
        UpdatePagedResults();
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            OnPropertyChanged(nameof(CurrentPage));
            UpdatePagedResults();
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            OnPropertyChanged(nameof(CurrentPage));
            UpdatePagedResults();
        }
    }

    [RelayCommand]
    private void LastPage()
    {
        currentPage = totalPages;
        OnPropertyChanged(nameof(CurrentPage));
        UpdatePagedResults();
    }

    [RelayCommand]
    private void ClearResults()
    {
        ComparisonResults.Clear();
        HasComparisonResults = false;
        TotalCount = 0;
        AddedCount = 0;
        ModifiedCount = 0;
        DeletedCount = 0;
        UnchangedCount = 0;
        StatusMessage = "就绪";
        filteredResultsView = null;
    }

    [RelayCommand]
    private void NextDifference()
    {
        // TODO: 实现跳转到下一个差异行
        model.Status.Success("跳转到下一个差异");
    }

    [RelayCommand]
    private void PreviousDifference()
    {
        // TODO: 实现跳转到上一个差异行
        model.Status.Success("跳转到上一个差异");
    }

    public void UpdateStatistics()
    {
        TotalCount = ComparisonResults.Count;
        AddedCount = ComparisonResults.Count(r => r.Type == ComparisonType.Added);
        ModifiedCount = ComparisonResults.Count(r => r.Type == ComparisonType.Modified);
        DeletedCount = ComparisonResults.Count(r => r.Type == ComparisonType.Deleted);
        UnchangedCount = ComparisonResults.Count(r => r.Type == ComparisonType.Unchanged);
        HasComparisonResults = TotalCount > 0;
        StatusMessage = $"对比完成：总记录 {TotalCount}，新增 {AddedCount}，修改 {ModifiedCount}，删除 {DeletedCount}，未变更 {UnchangedCount}";
        OnPropertyChanged(nameof(DisplayColumns));
    }

    [RelayCommand]
    private async Task PickFile(string parameter)
    {
        if (picker.OpenFile("Excel文件|*.xlsx;*.xls") is string file)
        {
            try
            {
                if (parameter == "1")
                {
                    FilePath1 = file;
                    FileName1 = Path.GetFileName(file);
                    // 获取工作表名称
                    var sheetNames = await excel.GetSheetNamesAsync(file);
                    SheetNames1.Clear();
                    foreach (var sheetName in sheetNames)
                    {
                        SheetNames1.Add(sheetName);
                    }
                    if (SheetNames1.Count > 0)
                        SelectedSheet1 = SheetNames1[0];
                }
                else if (parameter == "2")
                {
                    FilePath2 = file;
                    FileName2 = Path.GetFileName(file);
                    // 获取工作表名称
                    var sheetNames = await excel.GetSheetNamesAsync(file);
                    SheetNames2.Clear();
                    foreach (var sheetName in sheetNames)
                    {
                        SheetNames2.Add(sheetName);
                    }
                    if (SheetNames2.Count > 0)
                        SelectedSheet2 = SheetNames2[0];
                }
            }
            catch (Exception ex)
            {
                model.Status.Error($"读取文件失败：{ex.Message}");
            }
        }
    }

    [RelayCommand]
    private async Task ExportResults()
    {
        await ExportResultsToDesktop();
    }

    [RelayCommand]
    private async Task ExportResultsToDesktop()
    {
        if (ComparisonResults.Count == 0)
        {
            model.Status.Error("没有可导出的数据");
            return;
        }

        try
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileName = $"数据对比结果_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var savePath = Path.Combine(desktopPath, fileName);

            model.Status.Busy("正在导出数据...");

            await Task.Run(() =>
            {
                var workbook = excel.GetWorkbook();
                var worksheet = workbook.Worksheets[0];
                worksheet.Name = "对比结果";

                // 获取所有列名
                var columnNames = new List<string>();
                if (ComparisonResults.FirstOrDefault()?.CurrentRow != null)
                {
                    var table = ComparisonResults.First().CurrentRow.Table;
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        columnNames.Add(table.Columns[i].ColumnName);
                    }
                }
                else if (ComparisonResults.FirstOrDefault()?.OldRow != null)
                {
                    var table = ComparisonResults.First().OldRow.Table;
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
                foreach (var result in ComparisonResults)
                {
                    // 设置类型列
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

                    // 根据类型设置行样式
                    var rowStyle = workbook.CreateStyle();
                    if (result.Type == ComparisonType.Added)
                    {
                        // 新增：黄色背景
                        rowStyle.BackgroundColor = System.Drawing.Color.FromArgb(255, 249, 196);
                        rowStyle.Pattern = BackgroundType.Solid;
                    }
                    else if (result.Type == ComparisonType.Deleted)
                    {
                        // 删除：灰色背景 + 删除线
                        rowStyle.BackgroundColor = System.Drawing.Color.FromArgb(245, 245, 245);
                        rowStyle.Pattern = BackgroundType.Solid;
                        rowStyle.Font.IsStrikeout = true;
                    }
                    else if (result.Type == ComparisonType.Modified)
                    {
                        // 修改：红色背景
                        rowStyle.BackgroundColor = System.Drawing.Color.FromArgb(255, 205, 210);
                        rowStyle.Pattern = BackgroundType.Solid;
                    }

                    // 应用行样式到类型和主键列
                    worksheet.Cells[row, 0].SetStyle(rowStyle);
                    worksheet.Cells[row, 1].SetStyle(rowStyle);

                    // 写入当前行数据（优先使用CurrentRow，如果不存在则使用OldRow）
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
                            
                            // 如果是修改的字段，设置差异列高亮（橙色背景）
                            if (result.Type == ComparisonType.Modified && result.ChangedFields.ContainsKey(columnName) && result.ChangedFields[columnName])
                            {
                                var diffStyle = workbook.CreateStyle();
                                diffStyle.BackgroundColor = System.Drawing.Color.FromArgb(255, 224, 178);
                                diffStyle.Pattern = BackgroundType.Solid;
                                diffStyle.Font.IsBold = true;
                                cell.SetStyle(diffStyle);
                            }
                            else
                            {
                                // 应用行样式
                                cell.SetStyle(rowStyle);
                            }
                        }
                    }

                    row++;
                }

                // 自动调整列宽
                worksheet.AutoFitColumns();
                workbook.Save(savePath);
            });

            model.Status.Success($"导出完成：{fileName}");
            
            // 自动打开文件
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = savePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            model.Status.Error($"导出失败：{ex.Message}");
        }
    }

    public void OnNavigatedTo()
    {
    }

    public void OnNavigatedFrom()
    {
    }
}

public partial class ComparisonRow : ObservableObject
{
    public int RowIndex { get; set; } // 行序号
    public string Key { get; set; } = string.Empty;
    public DataRow CurrentRow { get; set; }
    public DataRow OldRow { get; set; }
    public ComparisonType Type { get; set; }
    public Dictionary<string, bool> ChangedFields { get; set; } = new();
    
    [ObservableProperty]
    private bool showOldRow = true;
    
    public string BackgroundColor => Type switch
    {
        ComparisonType.Added => "#FFF9C4",      // 黄色 - 新增
        ComparisonType.Deleted => "#F5F5F5",    // 灰色 - 删除
        ComparisonType.Modified => "#FFCDD2",   // 红色 - 修改
        ComparisonType.Unchanged => "Transparent", // 透明 - 无变化
        _ => "Transparent"
    };
    
    public string ForegroundColor => Type switch
    {
        ComparisonType.Deleted => "#9E9E9E",    // 灰色文字 - 删除
        _ => "#000000"                          // 黑色文字 - 其他
    };
    
    public bool IsDeleted => Type == ComparisonType.Deleted;
    
    public bool HasOldData => Type == ComparisonType.Modified || Type == ComparisonType.Deleted;
    
    public bool HasNewData => Type == ComparisonType.Modified || Type == ComparisonType.Added;

    // 获取指定列的新值
    public string GetNewValue(string columnName)
    {
        if (CurrentRow == null || !CurrentRow.Table.Columns.Contains(columnName))
            return "";
        return CurrentRow[columnName]?.ToString() ?? "";
    }

    // 获取指定列的旧值
    public string GetOldValue(string columnName)
    {
        if (OldRow == null || !OldRow.Table.Columns.Contains(columnName))
            return "";
        return OldRow[columnName]?.ToString() ?? "";
    }

    // 判断指定列是否有变化
    public bool IsColumnChanged(string columnName)
    {
        return ChangedFields.TryGetValue(columnName, out var changed) && changed;
    }

    // 获取所有列名（用于动态生成表格列）
    public IEnumerable<string> GetAllColumnNames()
    {
        if (CurrentRow != null)
        {
            return CurrentRow.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
        }
        if (OldRow != null)
        {
            return OldRow.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
        }
        return Enumerable.Empty<string>();
    }

    // 获取差异列名（仅变化的列）
    public IEnumerable<string> GetChangedColumnNames()
    {
        return ChangedFields.Where(kv => kv.Value).Select(kv => kv.Key);
    }
    
    // 获取旧行的列集合（用于展示修改前的数据）
    public IList<KeyValuePair<string, string>> OldRowColumns
    {
        get
        {
            var result = new List<KeyValuePair<string, string>>();
            if (OldRow != null)
            {
                foreach (DataColumn column in OldRow.Table.Columns)
                {
                    var value = OldRow[column.ColumnName]?.ToString() ?? "";
                    result.Add(new KeyValuePair<string, string>(column.ColumnName, value));
                }
            }
            return result;
        }
    }

    // 新数据行：按类型显示列（修改：仅差异列；新增：非空列；删除：不显示）
    public IList<ColumnValueDisplay> NewRowColumns
    {
        get
        {
            var result = new List<ColumnValueDisplay>();
            if (CurrentRow != null)
            {
                foreach (DataColumn column in CurrentRow.Table.Columns)
                {
                    var name = column.ColumnName;
                    var value = CurrentRow[name]?.ToString() ?? "";
                    var isChanged = ChangedFields.TryGetValue(name, out var changed) && changed;
                    
                    if (Type == ComparisonType.Modified)
                    {
                        if (isChanged)
                            result.Add(new ColumnValueDisplay 
                            { 
                                Key = name, 
                                Value = value,
                                IsChanged = true,
                                BackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 224, 178)),
                                FontWeight = FontWeights.Bold
                            });
                    }
                    else if (Type == ComparisonType.Added)
                    {
                        if (!string.IsNullOrEmpty(value))
                            result.Add(new ColumnValueDisplay 
                            { 
                                Key = name, 
                                Value = value,
                                IsChanged = false,
                                BackgroundBrush = Brushes.Transparent,
                                FontWeight = FontWeights.Normal
                            });
                    }
                    // Deleted 和 Unchanged 不显示新值列表
                }
            }
            return result;
        }
    }

    // 旧数据行：按类型显示列（修改：仅差异列；删除：非空列；新增：不显示）
    public IList<ColumnValueDisplay> ChangedOldRowColumns
    {
        get
        {
            var result = new List<ColumnValueDisplay>();
            if (OldRow != null)
            {
                foreach (DataColumn column in OldRow.Table.Columns)
                {
                    var name = column.ColumnName;
                    var value = OldRow[name]?.ToString() ?? "";
                    var isChanged = ChangedFields.TryGetValue(name, out var changed) && changed;
                    
                    if (Type == ComparisonType.Modified)
                    {
                        if (isChanged)
                            result.Add(new ColumnValueDisplay 
                            { 
                                Key = name, 
                                Value = value,
                                IsChanged = true,
                                BackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 224, 178)),
                                FontWeight = FontWeights.Bold
                            });
                    }
                    else if (Type == ComparisonType.Deleted)
                    {
                        if (!string.IsNullOrEmpty(value))
                            result.Add(new ColumnValueDisplay 
                            { 
                                Key = name, 
                                Value = value,
                                IsChanged = false,
                                BackgroundBrush = Brushes.Transparent,
                                FontWeight = FontWeights.Normal
                            });
                    }
                    // Added 和 Unchanged 不显示旧值列表
                }
            }
            return result;
        }
    }
}

public class ColumnValueDisplay
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsChanged { get; set; }
    public Brush BackgroundBrush { get; set; } = Brushes.Transparent;
    public FontWeight FontWeight { get; set; } = FontWeights.Normal;
}

public enum ComparisonType
{
    Unchanged,
    Added,
    Deleted,
    Modified
}