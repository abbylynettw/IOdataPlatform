using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using Aspose.Cells;
using IODataPlatform.Models;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// 数据库点类型配置
/// </summary>
public class DatabasePointType
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = true;
}

/// <summary>
/// 数据库更新配置窗口
/// </summary>
public partial class DatabaseExportConfigWindow : Window
{
    private readonly ObservableCollection<DatabasePointType> _pointTypes;
    private readonly IList<IoFullData> _sourceData;
    private readonly IPickerService _picker;
    private readonly ExcelService _excel;
    private string? _selectedFilePath;
    private Workbook? _targetWorkbook;
    
    // 龙鳍系统点类型配置
    private readonly List<DatabasePointType> _longfinPointTypes = new()
    {
        new DatabasePointType 
        { 
            Code = "AVI", 
            DisplayName = "1.1.1 AVI", 
            Description = "模拟量输入",
            IsSelected = true 
        },
        new DatabasePointType 
        { 
            Code = "PVI", 
            DisplayName = "1.1.2 PVI", 
            Description = "脉冲量输入",
            IsSelected = true 
        },
        new DatabasePointType 
        { 
            Code = "AVO", 
            DisplayName = "1.1.3 AVO", 
            Description = "模拟量输出",
            IsSelected = true 
        },
        new DatabasePointType 
        { 
            Code = "DVI", 
            DisplayName = "1.1.4 DVI", 
            Description = "开关量输入",
            IsSelected = true 
        },
        new DatabasePointType 
        { 
            Code = "DVO", 
            DisplayName = "1.1.5 DVO", 
            Description = "开关量输出",
            IsSelected = true 
        },
        new DatabasePointType 
        { 
            Code = "AM", 
            DisplayName = "1.1.6 AM", 
            Description = "Real型模拟量一层中间点",
            IsSelected = true
        },
        new DatabasePointType 
        { 
            Code = "DM_FEW", 
            DisplayName = "1.1.7 DM_FEW", 
            Description = "开关量一层中间点",
            IsSelected = true
        },
        new DatabasePointType 
        { 
            Code = "GBP", 
            DisplayName = "1.1.8 GBP", 
            Description = "开关类设备点",
            IsSelected = true 
        },
        new DatabasePointType 
        { 
            Code = "GCP", 
            DisplayName = "1.1.9 GCP", 
            Description = "调节类设备点",
            IsSelected = true 
        }
    };
    
    // 中控系统点类型配置
    private readonly List<DatabasePointType> _zhongkongPointTypes = new()
    {
        new DatabasePointType 
        { 
            Code = "AI", 
            DisplayName = "AI", 
            Description = "模拟量输入",
            IsSelected = true 
        },
        new DatabasePointType 
        { 
            Code = "AO", 
            DisplayName = "AO", 
            Description = "模拟量输出",
            IsSelected = true 
        },
        new DatabasePointType 
        { 
            Code = "DI", 
            DisplayName = "DI", 
            Description = "开关量输入",
            IsSelected = true 
        },
        new DatabasePointType 
        { 
            Code = "DO", 
            DisplayName = "DO", 
            Description = "开关量输出",
            IsSelected = true 
        }
    };

    public DatabaseExportConfigWindow(IList<IoFullData> sourceData, IPickerService picker, ExcelService excel, ControlSystem controlSystem)
    {
        InitializeComponent();

        _sourceData = sourceData ?? throw new ArgumentNullException(nameof(sourceData));
        _picker = picker ?? throw new ArgumentNullException(nameof(picker));
        _excel = excel ?? throw new ArgumentNullException(nameof(excel));

        // 初始化点类型列表（根据控制系统自动选择）
        _pointTypes = new ObservableCollection<DatabasePointType>();
        
        // 根据控制系统设置单选按钮状态和加载对应点类型
        if (controlSystem == ControlSystem.中控)
        {
            RadioZhongkong.IsChecked = true;
            LoadPointTypes(_zhongkongPointTypes);
        }
        else
        {
            RadioLongfin.IsChecked = true;
            LoadPointTypes(_longfinPointTypes);
        }

        PointTypesItemsControl.ItemsSource = _pointTypes;
        UpdateSelectedCount();
    }
    
    /// <summary>
    /// 加载点类型列表
    /// </summary>
    private void LoadPointTypes(List<DatabasePointType> pointTypes)
    {
        _pointTypes.Clear();
        foreach (var pt in pointTypes)
        {
            _pointTypes.Add(new DatabasePointType
            {
                Code = pt.Code,
                DisplayName = pt.DisplayName,
                Description = pt.Description,
                IsSelected = pt.IsSelected
            });
        }
    }
    
    /// <summary>
    /// 控制系统切换
    /// </summary>
    private void ControlSystemRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (_pointTypes == null) return; // 初始化期间跳过
        
        if (RadioLongfin.IsChecked == true)
        {
            LoadPointTypes(_longfinPointTypes);
        }
        else if (RadioZhongkong.IsChecked == true)
        {
            LoadPointTypes(_zhongkongPointTypes);
        }
        
        UpdateSelectedCount();
    }

    /// <summary>
    /// 获取选中的点类型代码列表
    /// </summary>
    public List<string> SelectedPointTypeCodes => _pointTypes
        .Where(pt => pt.IsSelected)
        .Select(pt => pt.Code)
        .ToList();

    /// <summary>
    /// 浏览文件按钮点击
    /// </summary>
    private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
    {
        if (_picker.OpenFile("Excel 文件(*.xlsx)|*.xlsx", "选择要更新的数据库文件") is not string filePath)
        {
            return;
        }

        if (!File.Exists(filePath))
        {
            MessageBox.Show("文件不存在", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            // 读取Excel文件
            _targetWorkbook = _excel.GetWorkbook(filePath);
            _selectedFilePath = filePath;
            FilePathTextBox.Text = filePath;

            // 检查文件结构
            if (!ValidateWorkbookStructure())
            {
                _targetWorkbook = null;
                _selectedFilePath = null;
                FilePathTextBox.Text = string.Empty;
                UpdateButton.IsEnabled = false;
                return;
            }

            UpdateButton.IsEnabled = true;
            MessageBox.Show("文件检查通过，可以开始更新", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"读取文件失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            _targetWorkbook = null;
            _selectedFilePath = null;
            FilePathTextBox.Text = string.Empty;
            UpdateButton.IsEnabled = false;
        }
    }

    /// <summary>
    /// 验证Excel文件结构
    /// </summary>
    private bool ValidateWorkbookStructure()
    {
        if (_targetWorkbook == null) return false;

        var selectedCodes = SelectedPointTypeCodes;
        if (selectedCodes.Count == 0)
        {
            MessageBox.Show("请至少选择一个点类型", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        var validationResults = new List<ColumnValidationResult>();
        var missingSheets = new List<string>();
        
        foreach (var code in selectedCodes)
        {
            var sheetName = code == "CNETCOM" ? "CNET_COM" : code;
            var sheet = _targetWorkbook.Worksheets[sheetName];
            
            if (sheet == null)
            {
                missingSheets.Add(sheetName);
                continue;
            }

            // 根据点类型获取对应模型的字段定义
            var expectedColumns = GetExpectedColumns(code);
            if (expectedColumns == null || expectedColumns.Count == 0)
            {
                continue; // 跳过未定义的点类型
            }

            // 读取Excel中的列名（第一行）
            var cells = sheet.Cells;
            var actualColumns = new List<string>();
            
            for (int col = 0; col <= cells.MaxDataColumn; col++)
            {
                var headerValue = cells[0, col].StringValue;
                if (!string.IsNullOrEmpty(headerValue))
                {
                    actualColumns.Add(headerValue.Trim());
                }
            }

            // 比对列名，找出缺失和多余的列
            var missingColumns = expectedColumns.Except(actualColumns).ToList();
            var extraColumns = actualColumns.Except(expectedColumns).ToList();
            var isValid = missingColumns.Count == 0 && extraColumns.Count == 0;

            validationResults.Add(new ColumnValidationResult
            {
                SheetName = sheetName,
                IsValid = isValid,
                ExpectedColumns = expectedColumns,
                ActualColumns = actualColumns,
                MissingColumnsList = missingColumns,
                ExtraColumnsList = extraColumns
            });
        }

        // 如果有缺失的工作表
        if (missingSheets.Count > 0)
        {
            MessageBox.Show($"文件中缺少以下工作表：{string.Join("、", missingSheets)}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        // 显示验证结果窗口
        var resultWindow = new ColumnValidationResultWindow(validationResults)
        {
            Owner = this
        };
        resultWindow.ShowDialog();

        // 返回是否所有验证都通过
        return resultWindow.IsAllValid;
    }

    /// <summary>
    /// 获取点类型对应的期望列名列表
    /// </summary>
    private List<string>? GetExpectedColumns(string pointTypeCode)
    {
        // 根据点类型代码获取对应的模型类型
        Type? modelType = pointTypeCode switch
        {
            "AVI" => typeof(xtes_AVI),
            "PVI" => typeof(xtes_PVI),
            "AVO" => typeof(xtes_AVO),
            "DVI" => typeof(xtes_DVI),
            "DVO" => typeof(xtes_DVO),
            "GBP" => typeof(xtes_GBP),
            "GCP" => typeof(xtes_GCP),
            "AM" => typeof(xtes_AM),
            "DM_FEW" => typeof(xtes_DM_FEW),
            _ => null
        };

        if (modelType == null) return null;

        // 通过反射获取模型的Display特性定义的列名
        var properties = modelType.GetProperties()
            .Where(p => p.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>() != null)
            .OrderBy(p => p.MetadataToken) // 保持字段声明顺序
            .ToList();

        var columns = new List<string>();
        foreach (var prop in properties)
        {
            var displayAttr = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
            if (displayAttr?.Name != null)
            {
                // 使用完整的Display.Name（如"PN - 点名"）
                columns.Add(displayAttr.Name);
            }
        }

        return columns;
    }
    private void SelectAllButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var pointType in _pointTypes)
        {
            pointType.IsSelected = true;
        }
        PointTypesItemsControl.Items.Refresh();
        UpdateSelectedCount();
    }

    /// <summary>
    /// 取消全选
    /// </summary>
    private void UnselectAllButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var pointType in _pointTypes)
        {
            pointType.IsSelected = false;
        }
        PointTypesItemsControl.Items.Refresh();
        UpdateSelectedCount();
    }

    /// <summary>
    /// 复选框状态改变
    /// </summary>
    private void PointTypeCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        UpdateSelectedCount();
    }

    /// <summary>
    /// 更新选中数量统计
    /// </summary>
    private void UpdateSelectedCount()
    {
        var selectedCount = _pointTypes.Count(pt => pt.IsSelected);
        SelectedCountText.Text = $"已选中：{selectedCount}个点类型";
    }

    /// <summary>
    /// 更新按钮点击
    /// </summary>
    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedFilePath) || _targetWorkbook == null)
        {
            MessageBox.Show("请先选择要更新的文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var selectedCodes = SelectedPointTypeCodes;
        if (selectedCodes.Count == 0)
        {
            MessageBox.Show("请至少选择一个点类型", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            UpdateButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            UpdateButton.Content = "更新中...";

            // 执行更新逻辑
            await UpdateSelectedPointTypes(selectedCodes);

            // 保存文件
            _targetWorkbook.Save(_selectedFilePath);

            MessageBox.Show($"更新成功：{_selectedFilePath}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"更新失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            UpdateButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
            UpdateButton.Content = "更新";
        }
    }

    /// <summary>
    /// 更新选中的点类型
    /// </summary>
    private async Task UpdateSelectedPointTypes(List<string> selectedCodes)
    {
        if (_targetWorkbook == null) return;

        var data = _sourceData.ToList();
        var helper = new FormularHelper();

        await Task.Run(() =>
        {
            // 处理AVI - 供电类型前两个字符 = AI
            if (selectedCodes.Contains("AVI"))
            {
                var dataAI = data.Where(d => !string.IsNullOrEmpty(d.PowerType) && 
                    d.PowerType.Length >= 2 &&
                    d.PowerType.Substring(0, 2).Equals("AI", StringComparison.OrdinalIgnoreCase));
                var aviList = helper.ConvertToAviList(dataAI);
                ClearAndFillSheet("AVI", aviList);
            }

            // 处理PVI - 供电类型前两个字符 = PI
            if (selectedCodes.Contains("PVI"))
            {
                var dataPI = data.Where(d => !string.IsNullOrEmpty(d.PowerType) && 
                    d.PowerType.Length >= 2 &&
                    d.PowerType.Substring(0, 2).Equals("PI", StringComparison.OrdinalIgnoreCase));
                var pviList = helper.ConvertToPviList(dataPI);
                ClearAndFillSheet("PVI", pviList);
            }

            // 处理AVO - 供电类型前两个字符 = AO
            if (selectedCodes.Contains("AVO"))
            {
                var dataAO = data.Where(d => !string.IsNullOrEmpty(d.PowerType) && 
                    d.PowerType.Length >= 2 &&
                    d.PowerType.Substring(0, 2).Equals("AO", StringComparison.OrdinalIgnoreCase));
                var avoList = helper.ConvertToAvoList(dataAO);
                ClearAndFillSheet("AVO", avoList);
            }

            // 处理DVI - 供电类型前两个字符 = DI
            if (selectedCodes.Contains("DVI"))
            {
                var dataDI = data.Where(d => !string.IsNullOrEmpty(d.PowerType) && 
                    d.PowerType.Length >= 2 &&
                    d.PowerType.Substring(0, 2).Equals("DI", StringComparison.OrdinalIgnoreCase));
                var dviList = helper.ConvertToDviList(dataDI);
                ClearAndFillSheet("DVI", dviList);
            }

            // 处理DVO - 供电类型前两个字符 = DO
            if (selectedCodes.Contains("DVO"))
            {
                var dataDO = data.Where(d => !string.IsNullOrEmpty(d.PowerType) && 
                    d.PowerType.Length >= 2 &&
                    d.PowerType.Substring(0, 2).Equals("DO", StringComparison.OrdinalIgnoreCase));
                var dvoList = helper.ConvertToDvoList(dataDO);
                ClearAndFillSheet("DVO", dvoList);
            }

            // 处理AM - 供电类型为FF1~FF6
            if (selectedCodes.Contains("AM"))
            {
                var amData = data.Where(d => !string.IsNullOrEmpty(d.PowerType) && 
                    (d.PowerType.StartsWith("FF1", StringComparison.OrdinalIgnoreCase) ||
                     d.PowerType.StartsWith("FF2", StringComparison.OrdinalIgnoreCase) ||
                     d.PowerType.StartsWith("FF3", StringComparison.OrdinalIgnoreCase) ||
                     d.PowerType.StartsWith("FF4", StringComparison.OrdinalIgnoreCase) ||
                     d.PowerType.StartsWith("FF5", StringComparison.OrdinalIgnoreCase) ||
                     d.PowerType.StartsWith("FF6", StringComparison.OrdinalIgnoreCase)));
                var amList = helper.ConvertToAMList(amData);
                ClearAndFillSheet("AM", amList);
            }

            // 处理DM_FEW - 供电类型为FF7~FF8、DP2
            if (selectedCodes.Contains("DM_FEW"))
            {
                var dmFewData = data.Where(d => !string.IsNullOrEmpty(d.PowerType) && 
                    (d.PowerType.StartsWith("FF7", StringComparison.OrdinalIgnoreCase) ||
                     d.PowerType.StartsWith("FF8", StringComparison.OrdinalIgnoreCase) ||
                     d.PowerType.StartsWith("DP2", StringComparison.OrdinalIgnoreCase)));
                var dmFewList = helper.ConvertToDM_FEWList(dmFewData);
                ClearAndFillSheet("DM_FEW", dmFewList);
            }

            // 处理GBP
            if (selectedCodes.Contains("GBP"))
            {
                var gbpList = helper.ConvertToGBPList(data);
                ClearAndFillSheet("GBP", gbpList);
            }

            // 处理GCP
            if (selectedCodes.Contains("GCP"))
            {
                var gcpList = helper.ConvertToGCPList(data);
                ClearAndFillSheet("GCP", gcpList);
            }
        });
    }

    /// <summary>
    /// 清空工作表并按顺序填充数据（保留标题行）
    /// </summary>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="newData">新数据列表</param>
    private void ClearAndFillSheet<T>(string sheetName, IEnumerable<T> newData)
    {
        if (_targetWorkbook == null) return;

        var sheet = _targetWorkbook.Worksheets[sheetName];
        if (sheet == null) return;

        var cells = sheet.Cells;
        
        // 获取列名映射（第一行为标题行）
        var columnMapping = new Dictionary<string, int>(); // 完整列名 -> 列索引
        
        for (int col = 0; col < cells.MaxDataColumn + 1; col++)
        {
            var headerValue = cells[0, col].StringValue;
            if (!string.IsNullOrEmpty(headerValue))
            {
                // 使用完整列名（如"PN - 点名"）
                columnMapping[headerValue.Trim()] = col;
            }
        }

        // 清空所有数据行（保留标题行）
        if (cells.MaxDataRow > 0)
        {
            cells.DeleteRows(1, cells.MaxDataRow);
        }

        // 获取新数据的属性信息
        var type = typeof(T);
        var properties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>() != null)
            .ToList();

        // 按顺序填充数据（从第2行开始，第1行是标题）
        int rowIndex = 1;
        foreach (var item in newData)
        {
            // 填充该行的所有字段
            foreach (var prop in properties)
            {
                var displayAttr = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                if (displayAttr?.Name == null) continue;

                // 使用完整的Display.Name（如"PN - 点名"）
                if (columnMapping.TryGetValue(displayAttr.Name, out int colIndex))
                {
                    var value = prop.GetValue(item);
                    cells[rowIndex, colIndex].PutValue(value);
                }
            }
            
            rowIndex++;
        }
    }

    /// <summary>
    /// 更新工作表数据（通过指定列匹配）
    /// </summary>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="newData">新数据列表</param>
    /// <param name="matchColumnName">用于匹配的列名（如CHN、PN等）</param>
    private void UpdateSheetData<T>(string sheetName, IEnumerable<T> newData, string matchColumnName = "CHN")
    {
        if (_targetWorkbook == null) return;

        var sheet = _targetWorkbook.Worksheets[sheetName];
        if (sheet == null) return;

        var cells = sheet.Cells;
        
        // 获取列名映射（第一行为标题行）
        var columnMapping = new Dictionary<string, int>(); // 完整列名 -> 列索引
        int matchColumnIndex = -1;
        
        for (int col = 0; col < cells.MaxDataColumn + 1; col++)
        {
            var headerValue = cells[0, col].StringValue;
            if (!string.IsNullOrEmpty(headerValue))
            {
                // 使用完整列名（如"CHN - 通道号"）
                columnMapping[headerValue.Trim()] = col;
                
                // 匹配列需要检查是否以matchColumnName开头
                if (headerValue.StartsWith(matchColumnName + " -") || headerValue == matchColumnName)
                {
                    matchColumnIndex = col;
                }
            }
        }

        if (matchColumnIndex == -1)
        {
            throw new Exception($"工作表 {sheetName} 中未找到匹配列 {matchColumnName}");
        }

        // 构建现有数据的匹配列索引（匹配列值 -> 行号）
        var matchRowMapping = new Dictionary<string, int>();
        for (int row = 1; row <= cells.MaxDataRow; row++)
        {
            var matchValue = cells[row, matchColumnIndex].StringValue;
            if (!string.IsNullOrEmpty(matchValue))
            {
                matchRowMapping[matchValue] = row;
            }
        }

        // 获取新数据的属性信息
        var type = typeof(T);
        var properties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>() != null)
            .ToList();

        // 遍历新数据，根据匹配列更新
        foreach (var item in newData)
        {
            // 获取匹配列的值
            var matchProperty = properties.FirstOrDefault(p => 
            {
                var displayAttr = p.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                return displayAttr?.Name?.StartsWith(matchColumnName) == true;
            });
            
            if (matchProperty == null) continue;
            
            var matchValue = matchProperty.GetValue(item)?.ToString();
            if (string.IsNullOrEmpty(matchValue)) continue;

            // 查找对应的行
            if (!matchRowMapping.TryGetValue(matchValue, out int rowIndex))
            {
                // 如果找不到匹配的值，跳过（或者可以选择新增行）
                continue;
            }

            // 更新该行的所有字段，并标记修改的单元格
            foreach (var prop in properties)
            {
                var displayAttr = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                if (displayAttr?.Name == null) continue;

                // 使用完整的Display.Name（如"PN - 点名"）
                if (columnMapping.TryGetValue(displayAttr.Name, out int colIndex))
                {
                    var newValue = prop.GetValue(item);
                    var cell = cells[rowIndex, colIndex];
                    var oldValue = cell.Value;
                    
                    // 比较新旧值，只有不同时才更新并标红
                    bool isChanged = false;
                    if (newValue != null && oldValue != null)
                    {
                        isChanged = !newValue.ToString().Equals(oldValue.ToString());
                    }
                    else if (newValue != null || oldValue != null)
                    {
                        isChanged = true;
                    }
                    
                    if (isChanged)
                    {
                        // 更新值
                        cell.PutValue(newValue);
                        
                        // 设置单元格文字为红色
                        var style = cell.GetStyle();
                        style.Font.Color = System.Drawing.Color.Red;
                        cell.SetStyle(style);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 按顺序填充工作表数据（用于没有CHN字段的表，如AM、DM_FEW、GBP、GCP等）
    /// </summary>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="newData">新数据列表</param>
    private void UpdateSheetDataBySequence<T>(string sheetName, IEnumerable<T> newData)
    {
        if (_targetWorkbook == null) return;

        var sheet = _targetWorkbook.Worksheets[sheetName];
        if (sheet == null) return;

        var cells = sheet.Cells;
        
        // 获取列名映射（第一行为标题行）
        var columnMapping = new Dictionary<string, int>(); // 完整列名 -> 列索引
        
        for (int col = 0; col < cells.MaxDataColumn + 1; col++)
        {
            var headerValue = cells[0, col].StringValue;
            if (!string.IsNullOrEmpty(headerValue))
            {
                // 使用完整列名（如"PN - 点名"）
                columnMapping[headerValue.Trim()] = col;
            }
        }

        // 获取新数据的属性信息
        var type = typeof(T);
        var properties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>() != null)
            .ToList();

        // 按顺序填充数据（从第2行开始，第1行是标题）
        int rowIndex = 1;
        foreach (var item in newData)
        {
            // 更新该行的所有字段，并标记修改的单元格
            foreach (var prop in properties)
            {
                var displayAttr = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                if (displayAttr?.Name == null) continue;

                // 使用完整的Display.Name（如"PN - 点名"）
                if (columnMapping.TryGetValue(displayAttr.Name, out int colIndex))
                {
                    var newValue = prop.GetValue(item);
                    var cell = cells[rowIndex, colIndex];
                    var oldValue = cell.Value;
                    
                    // 比较新旧值，只有不同时才更新并标红
                    bool isChanged = false;
                    if (newValue != null && oldValue != null)
                    {
                        isChanged = !newValue.ToString().Equals(oldValue.ToString());
                    }
                    else if (newValue != null || oldValue != null)
                    {
                        isChanged = true;
                    }
                    
                    if (isChanged)
                    {
                        // 更新值
                        cell.PutValue(newValue);
                        
                        // 设置单元格文字为红色
                        var style = cell.GetStyle();
                        style.Font.Color = System.Drawing.Color.Red;
                        cell.SetStyle(style);
                    }
                }
            }
            
            rowIndex++;
        }
    }

    /// <summary>
    /// 取消按钮点击
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
