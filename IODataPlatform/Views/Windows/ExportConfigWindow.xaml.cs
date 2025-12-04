using System.Data;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.ComponentModel.DataAnnotations;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Models.ExportModels;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models;
using IODataPlatform.Utilities;
using LYSoft.Libs.ServiceInterfaces;
using System.Windows.Controls.Primitives;
using System.Text.Json;
using SqlSugar;
using IODataPlatform.Services;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// IO清单导出配置窗口
/// 提供实时预览的导出配置功能，支持列选择、排序和字段管理
/// </summary>
public partial class ExportConfigWindow : Window
{
    #region 私有字段

    private readonly IPickerService _picker;
    private readonly ExcelService _excel;
    private readonly IMessageService _message;
    private readonly SqlSugarContext _context;
    private readonly CloudExportConfigService _configService;
    private readonly int? _currentSubProjectId;
    private readonly int? _currentProjectId;
    private readonly IList<IoFullData> _sourceData;
    private readonly ObservableCollection<ColumnInfo> _allColumns = new();
    private readonly ObservableCollection<object> _previewData = new();
    private readonly Dictionary<DataGridColumn, ColumnInfo> _columnMapping = new();
    private readonly ObservableCollection<ExportConfig> _savedConfigs = new();
    private ExportConfig? _currentConfig;
    private ExportType _currentExportType = ExportType.CurrentSystemList;
    private int _currentStep = 1;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化导出配置窗口
    /// </summary>
    /// <param name="sourceData">源数据</param>
    /// <param name="picker">文件选择服务</param>
    /// <param name="excel">Excel服务</param>
    /// <param name="message">消息服务</param>
    /// <param name="context">数据库上下文</param>
    /// <param name="configService">云端导出配置服务</param>
    /// <param name="currentSubProjectId">当前子项目ID</param>
    public ExportConfigWindow(IList<IoFullData> sourceData, IPickerService picker, ExcelService excel, IMessageService message, SqlSugarContext context, CloudExportConfigService configService, int? currentSubProjectId = null)
    {
        InitializeComponent();
        
        _sourceData = sourceData ?? throw new ArgumentNullException(nameof(sourceData));
        _picker = picker ?? throw new ArgumentNullException(nameof(picker));
        _excel = excel ?? throw new ArgumentNullException(nameof(excel));
        _message = message ?? throw new ArgumentNullException(nameof(message));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _currentSubProjectId = currentSubProjectId;
        
        // 获取当前项目ID
        _currentProjectId = GetCurrentProjectId();

        InitializeColumnsForCurrentType();
        
        // 在Loaded事件中异步加载配置
        Loaded += async (s, e) => {
            await LoadSavedConfigs();
            
            // 检查并同步字段变更
            await CheckAndSyncFieldChanges();
            
            LoadDefaultConfigForCurrentType(); // 先加载默认配置
            UpdateConfigComboBox(); // 再更新下拉框，保持选中默认配置
            UpdateStepUI();
            UpdateSelectedFieldsCount();
        };
    }

    /// <summary>
    /// 刷新预览数据
    /// </summary>
    private void RefreshPreview()
    {
        var filteredData = GetFilteredData();
        
        _previewData.Clear();
        
        // 取前100条数据用于预览
        var previewCount = Math.Min(100, filteredData.Count);
        for (int i = 0; i < previewCount; i++)
        {
            _previewData.Add(filteredData[i]);
        }
        
        UpdateDataGridColumns();
        UpdatePreviewInfo();
    }
    
    /// <summary>
    /// 更新数据表格列
    /// </summary>
    private void UpdateDataGridColumns()
    {
        // 该方法已废弃，预览更新由UpdatePreview方法统一处理
        UpdatePreview();
    }

    #endregion

    #region 事件处理

    /// <summary>
    /// 根据当前导出类型初始化列配置
    /// </summary>
    private void InitializeColumnsForCurrentType()
    {
        if (_currentExportType == ExportType.CurrentSystemList || _currentExportType == ExportType.PublishedList)
        {
            InitializeCurrentSystemColumns();
        }
        else
        {
            InitializeColumns();
        }
    }

    /// <summary>
    /// 初始化当前控制系统的列配置
    /// 根据当前子项目的控制系统，从 config_controlSystem_mapping 表中获取对应的字段
    /// </summary>
    private void InitializeCurrentSystemColumns()
    {
        _allColumns.Clear();
        
        try
        {
            // 获取当前子项目的控制系统
            var currentControlSystem = GetCurrentControlSystem();
            if (currentControlSystem == null)
            {
                // 如果获取不到控制系统，使用默认初始化
                InitializeColumns();
                return;
            }
            
            // 根据控制系统类型查询映射表
            var mappings = GetControlSystemMappings(currentControlSystem.Value);
            
            if (mappings == null || mappings.Count == 0)
            {
                // 如果没有找到映射，使用默认初始化
                InitializeColumns();
                return;
            }
            
            // 获取 IoFullData 类型的所有属性
            var properties = typeof(IoFullData).GetProperties()
                .Where(p => p.CanRead && IsDisplayableProperty(p))
                .ToArray();
            
            var order = 0;
            foreach (var mapping in mappings)
            {
                // 查找对应的属性
                var property = properties.FirstOrDefault(p => 
                {
                    var displayAttribute = p.GetCustomAttribute<DisplayAttribute>();
                    var displayName = displayAttribute?.Name ?? p.Name;
                    return displayName == mapping.StdField;
                });
                
                if (property != null)
                {
                    var displayName = GetDisplayName(property.Name);
                    var customFieldName = GetControlSystemFieldName(mapping, currentControlSystem.Value);
                    
                    _allColumns.Add(new ColumnInfo
                    {
                        FieldName = property.Name,
                        DisplayName = !string.IsNullOrEmpty(customFieldName) ? customFieldName : displayName,
                        Order = order++,
                        IsVisible = true, // 默认全选
                        IsRequired = false,
                        Type = GetColumnType(property.PropertyType)
                    });
                }
            }
            
            FieldsItemsControl.ItemsSource = _allColumns;
        }
        catch (Exception)
        {
            // 如果发生错误，使用默认初始化
            InitializeColumns();
        }
    }
    
    /// <summary>
    /// 获取当前子项目的控制系统
    /// </summary>
    private ControlSystem? GetCurrentControlSystem()
    {
        if (_currentSubProjectId == null) return null;
        
        try
        {
            var subProject = _context.Db.Queryable<config_project_subProject>()
                .Where(x => x.Id == _currentSubProjectId)
                .First();
                
            var major = _context.Db.Queryable<config_project_major>()
                .Where(x => x.Id == subProject.MajorId)
                .First();
                
            return major.ControlSystem;
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// 获取当前项目ID
    /// </summary>
    private int? GetCurrentProjectId()
    {
        if (_currentSubProjectId == null) return null;
        
        try
        {
            var subProject = _context.Db.Queryable<config_project_subProject>()
                .Where(x => x.Id == _currentSubProjectId)
                .First();
                
            var major = _context.Db.Queryable<config_project_major>()
                .Where(x => x.Id == subProject.MajorId)
                .First();
                
            return major.ProjectId;
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// 获取控制系统对应的映射配置
    /// </summary>
    private List<config_controlSystem_mapping> GetControlSystemMappings(ControlSystem controlSystem)
    {
        return controlSystem switch
        {
            ControlSystem.龙鳍 => _context.Db.Queryable<config_controlSystem_mapping>()
                .Where(it => !string.IsNullOrEmpty(it.LqOld) && !string.IsNullOrEmpty(it.StdField))
                .ToList(),
            ControlSystem.中控 => _context.Db.Queryable<config_controlSystem_mapping>()
                .Where(it => !string.IsNullOrEmpty(it.ZkOld) && !string.IsNullOrEmpty(it.StdField))
                .ToList(),
            ControlSystem.龙核 => _context.Db.Queryable<config_controlSystem_mapping>()
                .Where(it => !string.IsNullOrEmpty(it.LhOld) && !string.IsNullOrEmpty(it.StdField))
                .ToList(),
            ControlSystem.一室 => _context.Db.Queryable<config_controlSystem_mapping>()
                .Where(it => !string.IsNullOrEmpty(it.Xt1Old) && !string.IsNullOrEmpty(it.StdField))
                .ToList(),
            ControlSystem.安全级模拟系统 => _context.Db.Queryable<config_controlSystem_mapping>()
                .Where(it => !string.IsNullOrEmpty(it.AQJMNOld) && !string.IsNullOrEmpty(it.StdField))
                .ToList(),
            _ => new List<config_controlSystem_mapping>()
        };
    }
    
    /// <summary>
    /// 获取控制系统对应的字段名称
    /// </summary>
    private static string GetControlSystemFieldName(config_controlSystem_mapping mapping, ControlSystem controlSystem)
    {
        return controlSystem switch
        {
            ControlSystem.龙鳍 => mapping.LqOld ?? string.Empty,
            ControlSystem.中控 => mapping.ZkOld ?? string.Empty,
            ControlSystem.龙核 => mapping.LhOld ?? string.Empty,
            ControlSystem.一室 => mapping.Xt1Old ?? string.Empty,
            ControlSystem.安全级模拟系统 => mapping.AQJMNOld ?? string.Empty,
            _ => string.Empty
        };
    }

    /// <summary>
    /// 初始化列配置
    /// </summary>
    private void InitializeColumns()
    {
        _allColumns.Clear();
        
        // 如果没有数据，使用默认列配置
        if (_sourceData == null || _sourceData.Count == 0)
        {
            InitializeDefaultColumns();
            return;
        }

        // 从实际数据动态获取可用字段
        var properties = typeof(IoFullData).GetProperties()
            .Where(p => p.CanRead && IsDisplayableProperty(p))
            .ToArray();

        for (int i = 0; i < properties.Length; i++)
        {
            var prop = properties[i];
            var displayName = GetDisplayName(prop.Name);
            
            _allColumns.Add(new ColumnInfo
            {
                FieldName = prop.Name,
                DisplayName = displayName,
                Order = i,
                IsVisible = true, // 默认全选
                IsRequired = false, // 移除必填字段概念
                Type = GetColumnType(prop.PropertyType)
            });
        }
        
        FieldsItemsControl.ItemsSource = _allColumns;
    }

    /// <summary>
    /// 更新步骤界面
    /// </summary>
    private void UpdateStepUI()
    {
        // 如果控件还未初始化，直接返回
        if (Step1Panel == null || Step2Panel == null || Step3Panel == null ||
            PreviousButton == null || NextButton == null || ExportButton == null)
            return;
            
        // 更新步骤指示器
        UpdateStepIndicator();
        
        // 显示/隐藏对应的面板
        Step1Panel.Visibility = _currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
        Step2Panel.Visibility = _currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
        Step3Panel.Visibility = _currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;
        
        // 更新按钮状态
        PreviousButton.Visibility = _currentStep > 1 ? Visibility.Visible : Visibility.Collapsed;
        NextButton.Visibility = _currentStep < 3 ? Visibility.Visible : Visibility.Collapsed;
        ExportButton.Visibility = _currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;
        
        // 更新按钮文本
        if (_currentStep == 1)
        {
            NextButton.Content = "下一步";
        }
        else if (_currentStep == 2)
        {
            NextButton.Content = "预览";
            // 更新拖拽面板的数据
            UpdateColumnPanel();
        }
        else if (_currentStep == 3)
        {
            // 更新预览数据
            UpdatePreview();
        }
    }

    /// <summary>
    /// 更新步骤指示器
    /// </summary>
    private void UpdateStepIndicator()
    {
        // 如果控件还未初始化，直接返回
        if (Step1Border == null || Step2Border == null || Step3Border == null || 
            Step2Text == null || Step3Text == null)
            return;
            
        var activeColor = Application.Current.Resources["AccentFillColorDefaultBrush"] as Brush;
        var inactiveColor = Application.Current.Resources["ControlFillColorDisabledBrush"] as Brush;
        var activeTextColor = Brushes.Black; // 改为黑色，提高可读性
        var inactiveTextColor = Application.Current.Resources["TextFillColorDisabledBrush"] as Brush;
        
        // 步骤1
        Step1Border.Background = _currentStep >= 1 ? activeColor : inactiveColor;
        
        // 步骤2
        Step2Border.Background = _currentStep >= 2 ? activeColor : inactiveColor;
        Step2Text.Foreground = _currentStep >= 2 ? activeTextColor : inactiveTextColor;
        
        // 步骤3
        Step3Border.Background = _currentStep >= 3 ? activeColor : inactiveColor;
        Step3Text.Foreground = _currentStep >= 3 ? activeTextColor : inactiveTextColor;
    }

    /// <summary>
    /// 更新选中字段数量
    /// </summary>
    private void UpdateSelectedFieldsCount()
    {
        if (SelectedFieldsCountText == null) return;
        
        var selectedCount = _allColumns.Count(c => c.IsVisible);
        SelectedFieldsCountText.Text = $"已选中：{selectedCount}个字段";
        
        if (SelectedFieldsCountText2 != null)
        {
            SelectedFieldsCountText2.Text = $"已选中：{selectedCount}个字段";
        }
    }

    /// <summary>
    /// 更新拖拽面板
    /// </summary>
    private void UpdateColumnPanel()
    {
        if (ColumnPanel == null) return;
        
        var selectedColumns = _allColumns.Where(c => c.IsVisible).OrderBy(c => c.Order).ToList();
        ColumnPanel.Columns = new ObservableCollection<ColumnInfo>(selectedColumns);
    }

    /// <summary>
    /// 更新预览数据
    /// 使用与导出完全相同的数据准备逻辑
    /// </summary>
    private async void UpdatePreview()
    {
        try
        {
            _previewData.Clear();
            
            // 使用统一的数据准备方法（与导出保持一致）
            using var preparedDataTable = GetPreparedExportData();
            
            // 将DataTable转换为预览数据
            var previewCount = Math.Min(100, preparedDataTable.Rows.Count);
            for (int i = 0; i < previewCount; i++)
            {
                var row = preparedDataTable.Rows[i];
                var previewItem = new Dictionary<string, object>();
                
                foreach (DataColumn column in preparedDataTable.Columns)
                {
                    previewItem[column.ColumnName] = row[column] ?? string.Empty;
                }
                
                _previewData.Add(previewItem);
            }
            
            // 更新预览表格列
            UpdatePreviewDataGridColumnsFromDataTable(preparedDataTable);
        }
        catch (InvalidOperationException ex)
        {
            // 显示业务逻辑错误给用户
            await _message.ErrorAsync($"预览数据失败\n{ex.Message}");
            
            // 清空预览数据
            _previewData.Clear();
            PreviewDataGrid.ItemsSource = _previewData;
            PreviewDataGrid.Columns.Clear();
        }
        catch (Exception ex)
        {
            // 其他未预期错误
            await _message.ErrorAsync($"预览数据时发生未知错误\n错误信息：{ex.Message}\n\n请联系管理员或重新尝试。");
            
            // 清空预览数据
            _previewData.Clear();
            PreviewDataGrid.ItemsSource = _previewData;
            PreviewDataGrid.Columns.Clear();
        }
        
        UpdatePreviewInfo();
    }
    
    /// <summary>
    /// 从数据表更新预览表格列
    /// </summary>
    private void UpdatePreviewDataGridColumnsFromDataTable(DataTable dataTable)
    {
        PreviewDataGrid.Columns.Clear();
        _columnMapping.Clear();
        
        // 使用数据表的列名创建表格列
        foreach (DataColumn column in dataTable.Columns)
        {
            var dataGridColumn = new DataGridTextColumn
            {
                Header = column.ColumnName,
                Binding = new Binding($"[{column.ColumnName}]"), // 使用索引器绑定
                Width = new DataGridLength(120, DataGridLengthUnitType.Pixel)
            };
            
            PreviewDataGrid.Columns.Add(dataGridColumn);
        }
        
        // 设置数据源
        PreviewDataGrid.ItemsSource = _previewData;
    }

    /// <summary>
    /// 更新预览信息
    /// </summary>
    private void UpdatePreviewInfo()
    {
        if (RecordCountText == null || FieldCountText == null) return;
        
        var filteredData = GetFilteredData();
        var visibleColumns = _allColumns.Count(c => c.IsVisible);

        RecordCountText.Text = $"共计：{filteredData.Count}条记录";
        FieldCountText.Text = $"导出字段：{visibleColumns}个";
    }

    /// <summary>
    /// 判断属性是否应该显示
    /// </summary>
    private static bool IsDisplayableProperty(PropertyInfo property)
    {
        // 排除一些不需要显示的属性
        var excludedProperties = new[] { "Id", "IsDeleted", "CreateUser", "ModifyUser" };
        return !excludedProperties.Contains(property.Name) && 
               property.PropertyType != typeof(object) &&
               !property.PropertyType.IsClass || property.PropertyType == typeof(string);
    }

    /// <summary>
    /// 获取字段显示名称
    /// 优先从属性的Display特性中获取Name值，如果没有则使用属性名
    /// </summary>
    private static string GetDisplayName(string fieldName)
    {
        var property = typeof(IoFullData).GetProperty(fieldName);
        if (property != null)
        {
            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute?.Name != null)
            {
                return displayAttribute.Name;
            }
        }
        
        // 如果没有Display特性，则返回字段名
        return fieldName;
    }

    /// <summary>
    /// 获取列类型
    /// </summary>
    private static ColumnType GetColumnType(Type propertyType)
    {
        if (propertyType == typeof(int) || propertyType == typeof(int?) ||
            propertyType == typeof(double) || propertyType == typeof(double?) ||
            propertyType == typeof(decimal) || propertyType == typeof(decimal?))
            return ColumnType.Number;
            
        if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            return ColumnType.Date;
            
        return ColumnType.Text;
    }

    /// <summary>
    /// 初始化默认列配置（当没有数据时使用）
    /// </summary>
    private void InitializeDefaultColumns()
    {
        var defaultColumns = new[]
        {
            new { Field = "TagName", Display = "信号名称", Type = ColumnType.Text },
            new { Field = "IoType", Display = "IO类型", Type = ColumnType.Text },
            new { Field = "StationName", Display = "控制系统", Type = ColumnType.Text },
            new { Field = "SignalPositionNumber", Display = "位号", Type = ColumnType.Text },
            new { Field = "Description", Display = "信号描述", Type = ColumnType.Text },
            new { Field = "CabinetNumber", Display = "机柜编号", Type = ColumnType.Text }
        };

        for (int i = 0; i < defaultColumns.Length; i++)
        {
            var def = defaultColumns[i];
            _allColumns.Add(new ColumnInfo
            {
                FieldName = def.Field,
                DisplayName = def.Display,
                Order = i,
                IsVisible = true,
                IsRequired = false, // 移除必填字段概念
                Type = def.Type
            });
        }
    }



    #endregion

    #region 事件处理

    /// <summary>
    /// 导出类型选择变化
    /// </summary>
    private void ExportTypeRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (sender == CompleteListRadio)
            _currentExportType = ExportType.CompleteList;
        else if (sender == CurrentSystemRadio)
            _currentExportType = ExportType.CurrentSystemList;
        else if (sender == PublishedListRadio)
            _currentExportType = ExportType.PublishedList;

        // 重置到步骤1并重新初始化列配置
        _currentStep = 1;
        _currentConfig = null; // 清除当前配置
        InitializeColumnsForCurrentType();
        LoadDefaultConfigForCurrentType(); // 加载默认配置
        UpdateConfigComboBox(); // 更新配置下拉框
        UpdateStepUI();
        UpdateSelectedFieldsCount(); // 更新选中字段数量显示
    }

    /// <summary>
    /// 下一步按钮点击
    /// </summary>
    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStep == 1)
        {
            // 检查是否选中了字段
            var selectedCount = _allColumns.Count(c => c.IsVisible);
            if (selectedCount == 0)
            {
                _message.WarnAsync("请至少选择一个字段");
                return;
            }
            
            _currentStep = 2;
        }
        else if (_currentStep == 2)
        {
            // 从第2步到第3步时，同步拖拽面板的列顺序
            SyncColumnOrderFromPanel();
            _currentStep = 3;
        }
        
        UpdateStepUI();
    }

    /// <summary>
    /// 上一步按钮点击
    /// </summary>
    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStep > 1)
        {
            _currentStep--;
            UpdateStepUI();
        }
    }

    /// <summary>
    /// 字段复选框选中
    /// </summary>
    private void FieldCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        UpdateSelectedFieldsCount();
    }

    /// <summary>
    /// 字段复选框取消选中
    /// </summary>
    private void FieldCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        UpdateSelectedFieldsCount();
    }

    /// <summary>
    /// 全选字段按钮点击
    /// </summary>
    private void SelectAllFieldsButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var column in _allColumns)
        {
            column.IsVisible = true;
        }
        UpdateSelectedFieldsCount();
    }

    /// <summary>
    /// 取消全选字段按钮点击
    /// </summary>
    private void UnselectAllFieldsButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var column in _allColumns)
        {
            column.IsVisible = false;
        }
        UpdateSelectedFieldsCount();
    }

    /// <summary>
    /// 获取过滤后的数据
    /// 不进行任何排序，直接返回原始数据
    /// </summary>
    private List<IoFullData> GetFilteredData()
    {
        if (_sourceData == null) return new List<IoFullData>();

        // 直接返回原始数据，不进行排序
        return _sourceData.ToList();
    }
    
    // 移除排序相关方法
    // 排序功能已独立为单独的排序工具
    // 导出功能只负责按原始顺序导出数据
    
  
    
 


    /// <summary>
    /// 导出按钮点击
    /// </summary>
    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_picker.SaveFile("Excel 文件(*.xlsx)|*.xlsx", "IO清单导出") is not string filePath)
                return;

            // 使用统一的数据准备方法（与预览保持一致）
            using var dataTable = GetPreparedExportData();
            
            if (dataTable.Rows.Count == 0)
            {
                await _message.WarnAsync("没有数据可导出，请检查数据源和筛选条件。");
                return;
            }
            
            await _excel.FastExportAsync(dataTable, filePath);
            
            await _message.MessageAsync($"导出成功：{filePath}\n\n共导出 {dataTable.Rows.Count} 条记录，{dataTable.Columns.Count} 个字段。");
            
            DialogResult = true;
            Close();
        }
        catch (InvalidOperationException ex)
        {
            // 业务逻辑错误，显示给用户
            await _message.ErrorAsync($"导出失败\n{ex.Message}");
        }
        catch (UnauthorizedAccessException)
        {
            await _message.ErrorAsync("导出失败\n文件被占用或没有写入权限，请检查：\n1. 文件是否在Excel中打开\n2. 是否有文件夹的写入权限");
        }
        catch (DirectoryNotFoundException)
        {
            await _message.ErrorAsync("导出失败\n指定的目录不存在，请选择有效的保存路径。");
        }
        catch (IOException ex)
        {
            await _message.ErrorAsync($"导出失败\n文件操作错误：{ex.Message}\n\n请检查：\n1. 磁盘空间是否足够\n2. 文件路径是否正确");
        }
        catch (Exception ex)
        {
            await _message.ErrorAsync($"导出失败\n发生未知错误：{ex.Message}\n\n请联系管理员或重新尝试。");
        }
    }
    
    /// <summary>
    /// 默认配置导出按钮点击
    /// </summary>
    private async void DefaultExportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var exportTypeName = _currentExportType.GetDescription();
            var fileName = $"{exportTypeName}_默认配置";
            
            if (_picker.SaveFile("Excel 文件(*.xlsx)|*.xlsx", fileName) is not string filePath)
                return;

            // 根据当前导出类型获取数据
            var filteredData = GetFilteredData();
            if (filteredData == null || filteredData.Count == 0)
            {
                await _message.WarnAsync($"没有{exportTypeName}数据可导出。");
                return;
            }

            DataTable dataTable;
            string configDescription;
            
            // 根据导出类型使用不同的默认配置
            if (_currentExportType == ExportType.CurrentSystemList || _currentExportType == ExportType.PublishedList)
            {
                // 当前控制系统和发布清单：使用映射配置
                var currentControlSystem = GetCurrentControlSystem();
                if (!currentControlSystem.HasValue)
                {
                    await _message.WarnAsync("请先选择控制系统。");
                    return;
                }
                
                dataTable = filteredData.ToCustomDataTable(_context.Db, currentControlSystem.Value);
                configDescription = "系统默认映射配置";
            }
            else
            {
                // 完整清单：使用Display特性配置
                dataTable = await filteredData.ToTableByDisplayAttributeAsync();
                configDescription = "系统默认Display特性配置";
            }
            
            if (dataTable.Rows.Count == 0)
            {
                await _message.WarnAsync("没有数据可导出。");
                return;
            }
            
            await _excel.FastExportAsync(dataTable, filePath);
            
            await _message.MessageAsync($"默认配置导出成功：{filePath}\n\n" +
                                      $"导出类型：{exportTypeName}\n" +
                                      $"使用配置：{configDescription}\n" +
                                      $"共导出 {dataTable.Rows.Count} 条记录，{dataTable.Columns.Count} 个字段。");
            
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            await _message.ErrorAsync($"默认配置导出失败：{ex.Message}");
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

    /// <summary>
    /// 全选列按钮点击
    /// </summary>
    private void SelectAllColumnsButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var column in _allColumns)
        {
            column.IsVisible = true;
        }
        RefreshPreview();
    }

    /// <summary>
    /// 取消全选列按钮点击
    /// </summary>
    private void UnselectAllColumnsButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var column in _allColumns)
        {
            column.IsVisible = false;
        }
        RefreshPreview();
    }

    /// <summary>
    /// 列可见性复选框选中
    /// </summary>
    private void ColumnVisibilityCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox)
        {
            // 通过查找父级DataGridColumnHeader来获取对应的列
            var header = FindParent<DataGridColumnHeader>(checkBox);
            if (header?.Column != null && _columnMapping.TryGetValue(header.Column, out var columnInfo))
            {
                columnInfo.IsVisible = true;
                RefreshPreview();
            }
        }
    }

    /// <summary>
    /// 列可见性复选框取消选中
    /// </summary>
    private void ColumnVisibilityCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox)
        {
            var header = FindParent<DataGridColumnHeader>(checkBox);
            if (header?.Column != null && _columnMapping.TryGetValue(header.Column, out var columnInfo))
            {
                columnInfo.IsVisible = false;
                RefreshPreview();
            }
        }
    }

    /// <summary>
    /// 查找父级元素
    /// </summary>
    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent != null && parent is not T)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }
        return parent as T;
    }

    /// <summary>
    /// 数据表格列重新排序
    /// </summary>
    private void PreviewDataGrid_ColumnReordered(object sender, DataGridColumnEventArgs e)
    {
        // 更新列顺序
        for (int i = 0; i < PreviewDataGrid.Columns.Count; i++)
        {
            if (_columnMapping.TryGetValue(PreviewDataGrid.Columns[i], out var columnInfo))
            {
                columnInfo.Order = i;
            }
        }
        
        // 排序所有列
        var sortedColumns = _allColumns.OrderBy(c => c.Order).ToList();
        _allColumns.Clear();
        foreach (var col in sortedColumns)
        {
            _allColumns.Add(col);
        }
        
        UpdatePreviewInfo();
    }

    /// <summary>
    /// 从拖拽面板同步列顺序到_allColumns
    /// </summary>
    private void SyncColumnOrderFromPanel()
    {
        if (ColumnPanel?.Columns == null) return;
        
        // 获取拖拽面板中的列顺序
        var panelColumns = ColumnPanel.Columns.ToList();
        
        // 更新_allColumns中对应列的Order属性
        for (int i = 0; i < panelColumns.Count; i++)
        {
            var panelColumn = panelColumns[i];
            var allColumn = _allColumns.FirstOrDefault(c => c.FieldName == panelColumn.FieldName);
            if (allColumn != null)
            {
                allColumn.Order = i;
            }
        }
        
        // 重新排序_allColumns
        var sortedColumns = _allColumns.OrderBy(c => c.Order).ToList();
        _allColumns.Clear();
        foreach (var column in sortedColumns)
        {
            _allColumns.Add(column);
        }
    }
    
    /// <summary>
    /// 创建按用户选择的列和顺序的自定义DataTable
    /// </summary>
    private DataTable CreateCustomDataTable(List<IoFullData> data, List<ColumnInfo> visibleColumns)
    {
        var dataTable = new DataTable();
        
        // 按顺序添加列，保持原始数据类型
        foreach (var columnInfo in visibleColumns.OrderBy(c => c.Order))
        {
            var property = typeof(IoFullData).GetProperty(columnInfo.FieldName);
            var columnType = property != null ? (Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType) : typeof(string);
            dataTable.Columns.Add(columnInfo.DisplayName, columnType);
        }
        
        // 添加数据行
        foreach (var item in data)
        {
            var row = dataTable.NewRow();
            
            foreach (var columnInfo in visibleColumns.OrderBy(c => c.Order))
            {
                try
                {
                    var property = typeof(IoFullData).GetProperty(columnInfo.FieldName);
                    if (property != null)
                    {
                        var value = property.GetValue(item);
                        // 保持原始数据类型，对于可空类型的null值使用DBNull.Value
                        if (value == null)
                        {
                            row[columnInfo.DisplayName] = DBNull.Value;
                        }
                        else
                        {
                            row[columnInfo.DisplayName] = value;
                        }
                    }
                    else
                    {
                        row[columnInfo.DisplayName] = DBNull.Value;
                    }
                }
                catch
                {
                    row[columnInfo.DisplayName] = DBNull.Value;
                }
            }
            
            dataTable.Rows.Add(row);
        }
        
        return dataTable;
    }
    
    /// <summary>
    /// 创建按用户选择的列和顺序，但使用映射字段名的自定义DataTable
    /// </summary>
    private DataTable CreateCustomDataTableWithMapping(List<IoFullData> data, List<ColumnInfo> visibleColumns, Dictionary<string, string> mappingDict)
    {
        var dataTable = new DataTable();
        
        // 按用户设定的顺序添加列，但使用映射后的字段名
        foreach (var columnInfo in visibleColumns.OrderBy(c => c.Order))
        {
            // 获取标准字段名（Display特性值或属性名）
            var property = typeof(IoFullData).GetProperty(columnInfo.FieldName);
            var stdFieldName = property?.GetCustomAttribute<DisplayAttribute>()?.Name ?? columnInfo.FieldName;
            
            // 尝试获取映射后的字段名
            if (mappingDict.TryGetValue(stdFieldName, out var mappedColumnName) && !string.IsNullOrEmpty(mappedColumnName))
            {
                // 使用映射后的字段名作为列名，保持原始数据类型
                var columnType = property != null ? (Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType) : typeof(string);
                dataTable.Columns.Add(mappedColumnName, columnType);
            }
            else
            {
                // 如果没有映射，使用原始显示名，保持原始数据类型
                var columnType = property != null ? (Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType) : typeof(string);
                dataTable.Columns.Add(columnInfo.DisplayName, columnType);
            }
        }
        
        // 添加数据行
        foreach (var item in data)
        {
            var row = dataTable.NewRow();
            
            foreach (var columnInfo in visibleColumns.OrderBy(c => c.Order))
            {
                try
                {
                    var property = typeof(IoFullData).GetProperty(columnInfo.FieldName);
                    if (property != null)
                    {
                        var stdFieldName = property.GetCustomAttribute<DisplayAttribute>()?.Name ?? columnInfo.FieldName;
                        var value = property.GetValue(item);
                        
                        // 获取对应的列名（映射后的或原始的）
                        string columnName;
                        if (mappingDict.TryGetValue(stdFieldName, out var mappedColumnName) && !string.IsNullOrEmpty(mappedColumnName))
                        {
                            columnName = mappedColumnName;
                        }
                        else
                        {
                            columnName = columnInfo.DisplayName;
                        }
                        
                        // 只有当列存在时才赋值
                        if (dataTable.Columns.Contains(columnName))
                        {
                            // 保持原始数据类型，对于可空类型的null值使用DBNull.Value
                            if (value == null)
                            {
                                row[columnName] = DBNull.Value;
                            }
                            else
                            {
                                row[columnName] = value;
                            }
                        }
                    }
                }
                catch
                {
                    // 忽略错误，继续处理下一个字段
                }
            }
            
            dataTable.Rows.Add(row);
        }
        
        return dataTable;
    }
        
    /// <summary>
    /// 获取准备好的导出数据（预览和导出使用统一的数据源）
    /// </summary>
    private DataTable GetPreparedExportData()
    {
        try
        {
            var filteredData = GetFilteredData();
            if (filteredData == null || filteredData.Count == 0)
            {
                throw new InvalidOperationException("没有可导出的数据，请检查数据源或筛选条件。");
            }
            
            var visibleColumns = _allColumns.Where(c => c.IsVisible).OrderBy(c => c.Order).ToList();
            if (visibleColumns.Count == 0)
            {
                throw new InvalidOperationException("请至少选择一个字段进行导出。");
            }

            DataTable dataTable;
            
            // 对于所有导出类型，都使用用户自定义的列顺序
            // 只是在字段名映射时根据导出类型进行不同处理
            if (_currentExportType == ExportType.CurrentSystemList || _currentExportType == ExportType.PublishedList)
            {
                // 当前控制系统模式和发布的IO清单：使用映射字段名
                var currentControlSystem = GetCurrentControlSystem();
                if (currentControlSystem.HasValue)
                {
                    try
                    {
                        // 获取映射字典
                        var mappingDict = _context.Db.Queryable<config_controlSystem_mapping>()
                            .Where(it => it.StdField != null).ToList()
                            .ToDictionary(
                                it => it.StdField,
                                it => currentControlSystem.Value switch
                                {
                                    ControlSystem.龙鳍 => it.LqOld,
                                    ControlSystem.中控 => it.ZkOld,
                                    ControlSystem.龙核 => it.LhOld,
                                    ControlSystem.一室 => it.Xt1Old,
                                    ControlSystem.安全级模拟系统 => it.AQJMNOld,
                                    _ => null
                                });
                        
                        // 创建按用户顺序排列且使用映射字段名的DataTable
                        dataTable = CreateCustomDataTableWithMapping(filteredData, visibleColumns, mappingDict);
                        
                        if (dataTable.Columns.Count == 0)
                        {
                            throw new InvalidOperationException($"当前控制系统（{currentControlSystem.Value}）的字段映射配置不完整，请检查配置表 config_controlSystem_mapping。");
                        }
                    }
                    catch (ArgumentException ex) when (ex.Message.Contains("same key"))
                    {
                        throw new InvalidOperationException($"当前控制系统（{currentControlSystem.Value}）的映射配置存在重复字段，请联系管理员检查数据库配置。\n错误详情：{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"处理当前控制系统数据时发生错误：{ex.Message}");
                    }
                }
                else
                {
                    throw new InvalidOperationException("无法获取当前子项目的控制系统信息，请检查项目配置。");
                }
            }
            else
            {
                // 完整清单模式：按用户选择的列和顺序导出
                dataTable = CreateCustomDataTable(filteredData, visibleColumns);
            }
            
            return dataTable;
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            // 其他未预期的异常
            throw new InvalidOperationException($"准备导出数据时发生错误：{ex.Message}");
        }
    }

    #endregion

    #region 配置管理

    /// <summary>
    /// 加载已保存的配置
    /// </summary>
    private async Task LoadSavedConfigs()
    {
        try
        {
            _savedConfigs.Clear();
            
            // 从云端加载配置
            var configs = await _configService.GetUserConfigsAsync(_currentExportType, _currentProjectId);
            if (configs != null)
            {
                foreach (var config in configs)
                {
                    _savedConfigs.Add(config);
                }
            }
            
            // 加载默认配置
            LoadDefaultConfigForCurrentType();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载配置失败：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 保存配置到云端
    /// </summary>
    private async Task SaveConfigToCloud(ExportConfig config)
    {
        try
        {
            await _configService.SaveUserConfigAsync(_currentExportType, config);
        }
        catch (Exception ex)
        {
            await _message.ErrorAsync($"保存配置失败：{ex.Message}");
        }
    }

    
    /// <summary>
    /// 加载当前导出类型的默认配置
    /// 根据项目规范，同类型导出中只有一个默认配置
    /// </summary>
    private void LoadDefaultConfigForCurrentType()
    {
        var defaultConfig = _savedConfigs.FirstOrDefault(c => c.Type == _currentExportType && c.IsSystemDefault);
        if (defaultConfig != null)
        {
            ApplyConfig(defaultConfig);
        }
        else
        {
            // 如果没有默认配置，保持当前的初始化状态
            // 这保证了字段选择默认为全选状态
        }
    }
    
    /// <summary>
    /// 应用配置
    /// </summary>
    private void ApplyConfig(ExportConfig config)
    {
        try
        {
            _currentConfig = config;
            
            // 恢复列配置
            if (config.ColumnOrder?.Count > 0)
            {
                _allColumns.Clear();
                
                foreach (var columnConfig in config.ColumnOrder.OrderBy(c => c.Order))
                {
                    _allColumns.Add(new ColumnInfo
                    {
                        FieldName = columnConfig.FieldName,
                        DisplayName = columnConfig.DisplayName,
                        IsVisible = columnConfig.IsVisible,
                        Order = columnConfig.Order,
                        Type = columnConfig.Type,
                        IsRequired = columnConfig.IsRequired
                    });
                }
                
                if (FieldsItemsControl != null)
                {
                    FieldsItemsControl.ItemsSource = _allColumns;
                }
                UpdateSelectedFieldsCount();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"应用配置失败：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 保存当前配置
    /// </summary>
    private async Task SaveCurrentConfig(string configName, bool setAsDefault = false)
    {
        try
        {
            // 在保存配置前，先同步拖拽面板的列顺序到_allColumns
            SyncColumnOrderFromPanel();
            
            if (setAsDefault)
            {
                foreach (var config in _savedConfigs.Where(c => c.Type == _currentExportType))
                {
                    config.IsSystemDefault = false;
                }
            }
            
            var newConfig = new ExportConfig
            {
                ConfigName = configName,
                Type = _currentExportType,
                ColumnOrder = _allColumns.ToList(),
                CreatedTime = DateTime.Now,
                LastModified = DateTime.Now,
                IsSystemDefault = setAsDefault
            };
            
            var existingConfig = _savedConfigs.FirstOrDefault(c => c.ConfigName == configName && c.Type == _currentExportType);
            if (existingConfig != null)
            {
                _savedConfigs.Remove(existingConfig);
            }
            
            _savedConfigs.Add(newConfig);
            _currentConfig = newConfig;
            
            await SaveConfigToCloud(newConfig);
            UpdateConfigComboBox(); // 更新配置下拉框
            await _message.MessageAsync($"配置“{configName}”保存成功！");
        }
        catch (Exception ex)
        {
            await _message.ErrorAsync($"保存配置失败：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 保存配置按钮点击
    /// </summary>
    private async void SaveConfigButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // 检查是否有选中的字段
            var selectedCount = _allColumns.Count(c => c.IsVisible);
            if (selectedCount == 0)
            {
                await _message.WarnAsync("请至少选择一个字段后再保存配置！");
                return;
            }
            
            var result = await ShowConfigNameDialog();
            if (!string.IsNullOrEmpty(result.ConfigName))
            {
                await SaveCurrentConfig(result.ConfigName, result.SetAsDefault);
            }
        }
        catch (Exception ex)
        {
            await _message.ErrorAsync($"保存配置失败：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 显示配置名称输入对话框
    /// </summary>
    private async Task<(string ConfigName, bool SetAsDefault)> ShowConfigNameDialog()
    {
        // 创建一个简单的输入对话框
        var inputWindow = new Window
        {
            Title = "保存配置",
            Width = 350,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize
        };
        
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        
        // 标题
        var titleText = new Wpf.Ui.Controls.TextBlock
        {
            Text = "请输入配置名称：",
            Margin = new Thickness(20, 20, 20, 10),
            FontWeight = FontWeights.Medium
        };
        Grid.SetRow(titleText, 0);
        grid.Children.Add(titleText);
        
        // 输入框 - 根据导出类型添加前缀
        var typePrefix = _currentExportType switch
        {
            ExportType.CompleteList => "完整",
            ExportType.CurrentSystemList => "控制系统", 
            ExportType.PublishedList => "发布",
            _ => "配置"
        };
        
        var nameTextBox = new Wpf.Ui.Controls.TextBox
        {
            Text = $"配置_{typePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}",
            Margin = new Thickness(20, 0, 20, 15)
        };
        Grid.SetRow(nameTextBox, 1);
        grid.Children.Add(nameTextBox);
        
        // 默认配置复选框
        var defaultCheckBox = new CheckBox
        {
            Content = "设为默认配置",
            Margin = new Thickness(20, 0, 20, 15),
            IsChecked = false
        };
        Grid.SetRow(defaultCheckBox, 2);
        grid.Children.Add(defaultCheckBox);
        
        // 空白区域
        Grid.SetRow(new Border(), 3);
        
        // 按钮区域
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(20, 10, 20, 20)
        };
        
        var okButton = new Wpf.Ui.Controls.Button
        {
            Content = "确定",
            Margin = new Thickness(0, 0, 10, 0),
            MinWidth = 80,
            Appearance = Wpf.Ui.Controls.ControlAppearance.Primary
        };
        
        var cancelButton = new Wpf.Ui.Controls.Button
        {
            Content = "取消",
            MinWidth = 80
        };
        
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        Grid.SetRow(buttonPanel, 4);
        grid.Children.Add(buttonPanel);
        
        inputWindow.Content = grid;
        
        // 设置事件
        bool? dialogResult = null;
        okButton.Click += (s, e) => {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                _message.WarnAsync("请输入配置名称！");
                return;
            }
            dialogResult = true;
            inputWindow.Close();
        };
        
        cancelButton.Click += (s, e) => {
            dialogResult = false;
            inputWindow.Close();
        };
        
        // 显示对话框
        inputWindow.ShowDialog();
        
        if (dialogResult == true && !string.IsNullOrWhiteSpace(nameTextBox.Text))
        {
            return (nameTextBox.Text.Trim(), defaultCheckBox.IsChecked == true);
        }
        
        return (string.Empty, false);
    }
    
    /// <summary>
    /// 更新配置下拉框
    /// </summary>
    private void UpdateConfigComboBox()
    {
        if (SavedConfigsComboBox == null) return;
        
        SavedConfigsComboBox.Items.Clear();
        
        // 添加默认提示项
        SavedConfigsComboBox.Items.Add(new { Config = (ExportConfig?)null, Display = "选择已保存的配置..." });
        
        var configs = _savedConfigs.Where(c => c.Type == _currentExportType).ToList();
        int selectedIndex = 0; // 默认选中提示项
        
        for (int i = 0; i < configs.Count; i++)
        {
            var config = configs[i];
            var displayText = config.ConfigName;
            if (config.IsSystemDefault)
            {
                displayText += " (默认)";
            }
            
            SavedConfigsComboBox.Items.Add(new { Config = config, Display = displayText });
            
            // 如果这是当前配置，记录索引
            if (_currentConfig != null && config.ConfigName == _currentConfig.ConfigName)
            {
                selectedIndex = i + 1; // +1因为有提示项
            }
        }
        
        // 选中对应的配置
        if (SavedConfigsComboBox.Items.Count > selectedIndex)
        {
            SavedConfigsComboBox.SelectedIndex = selectedIndex;
        }
    }
    
    /// <summary>
    /// 配置下拉框选择变化 - 立即应用配置
    /// </summary>
    private async void SavedConfigsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SavedConfigsComboBox.SelectedItem == null || SavedConfigsComboBox.SelectedIndex == 0)
            return;
            
        try
        {
            var selectedItem = (dynamic)SavedConfigsComboBox.SelectedItem;
            var config = (ExportConfig?)selectedItem.Config;
            
            if (config != null)
            {
                ApplyConfig(config);
                // 不显示提示消息，保持操作流畅
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"应用配置失败：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 删除配置按钮点击
    /// </summary>
    private async void DeleteConfigButton_Click(object sender, RoutedEventArgs e)
    {
        if (SavedConfigsComboBox.SelectedItem == null || SavedConfigsComboBox.SelectedIndex == 0)
        {
            await _message.WarnAsync("请先选择一个配置！");
            return;
        }
        
        var selectedItem = (dynamic)SavedConfigsComboBox.SelectedItem;
        var config = (ExportConfig?)selectedItem.Config;
        
        if (config == null)
        {
            await _message.WarnAsync("请选择一个有效的配置！");
            return;
        }
        
        // 使用自定义对话框来解决Owner问题
        var confirmed = ShowCustomConfirmDialog($"确定要删除配置“{config.ConfigName}”吗？");
        if (confirmed)
        {
            try
            {
                // 移除前缀以获取真实的配置名称
                var realConfigName = config.ConfigName;
                if (realConfigName.StartsWith("[我的] "))
                {
                    realConfigName = realConfigName.Replace("[我的] ", "");
                }
                else if (!realConfigName.StartsWith("[我的] "))
                {
                    await _message.WarnAsync("只能删除您自己创建的配置！");
                    return;
                }
                
                _savedConfigs.Remove(config);
                await _configService.DeleteUserConfigAsync(_currentExportType, realConfigName);
                UpdateConfigComboBox();
                await _message.MessageAsync($"配置已删除！");
            }
            catch (Exception ex)
            {
                await _message.ErrorAsync($"删除配置失败：{ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// 显示自定义确认对话框，确保正确的Owner设置
    /// </summary>
    /// <param name="message">确认消息</param>
    /// <returns>用户点击确认返回true，否则返回false</returns>
    private bool ShowCustomConfirmDialog(string message)
    {
        var confirmWindow = new Window
        {
            Title = "系统提示",
            Width = 350,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this, // 设置为当前窗口的子窗口
            ResizeMode = ResizeMode.NoResize
        };
        
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        
        // 消息内容
        var messageText = new Wpf.Ui.Controls.TextBlock
        {
            Text = message,
            Margin = new Thickness(20),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(messageText, 0);
        grid.Children.Add(messageText);
        
        // 按钮区域
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(20, 10, 20, 20)
        };
        
        var confirmButton = new Wpf.Ui.Controls.Button
        {
            Content = "确认",
            Margin = new Thickness(0, 0, 10, 0),
            MinWidth = 80,
            Appearance = Wpf.Ui.Controls.ControlAppearance.Primary
        };
        
        var cancelButton = new Wpf.Ui.Controls.Button
        {
            Content = "取消",
            MinWidth = 80
        };
        
        buttonPanel.Children.Add(confirmButton);
        buttonPanel.Children.Add(cancelButton);
        Grid.SetRow(buttonPanel, 1);
        grid.Children.Add(buttonPanel);
        
        confirmWindow.Content = grid;
        
        // 设置事件
        bool? dialogResult = null;
        confirmButton.Click += (s, e) => {
            dialogResult = true;
            confirmWindow.Close();
        };
        
        cancelButton.Click += (s, e) => {
            dialogResult = false;
            confirmWindow.Close();
        };
        
        // 显示模态对话框
        confirmWindow.ShowDialog();
        
        return dialogResult == true;
    }
}

#region 字段兼容性处理

/// <summary>
/// 导出配置窗口的字段兼容性处理扩展
/// </summary>
public partial class ExportConfigWindow
{
    /// <summary>
    /// 检查并同步字段变更
    /// 在加载配置时自动检查字段变化，确保配置的有效性
    /// </summary>
    private async Task CheckAndSyncFieldChanges()
    {
        try
        {
            // 获取当前可用字段
            var availableFields = GetCurrentAvailableFields();
            
            // 同步用户配置中的字段变更
            await _configService.SyncFieldChangesForUser(_currentExportType, availableFields, _currentProjectId);
            
            // 检查当前加载的配置是否需要同步
            await CheckLoadedConfigsFieldSync(availableFields);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"检查字段变更失败：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 检查已加载配置的字段同步
    /// </summary>
    /// <param name="availableFields">可用字段列表</param>
    private async Task CheckLoadedConfigsFieldSync(List<string> availableFields)
    {
        try
        {
            var configsNeedSync = new List<ExportConfig>();
            
            foreach (var config in _savedConfigs.Where(c => c.Type == _currentExportType))
            {
                if (_configService.NeedFieldSync(config, availableFields))
                {
                    configsNeedSync.Add(config);
                }
            }
            
            if (configsNeedSync.Count > 0)
            {
                // 可以选择显示提示或自动同步
                System.Diagnostics.Debug.WriteLine($"发现{configsNeedSync.Count}个配置需要字段同步");
                
                // 自动同步（静默处理）
                await AutoSyncConfigFields(configsNeedSync, availableFields);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"检查加载配置同步失败：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 自动同步配置字段
    /// </summary>
    /// <param name="configs">需要同步的配置列表</param>
    /// <param name="availableFields">可用字段列表</param>
    private async Task AutoSyncConfigFields(List<ExportConfig> configs, List<string> availableFields)
    {
        try
        {
            foreach (var config in configs)
            {
                // 备份原始配置名称（用于日志）
                var originalName = config.ConfigName;
                
                // 同步SelectedFields
                if (config.SelectedFields != null)
                {
                    var originalCount = config.SelectedFields.Count;
                    config.SelectedFields.RemoveAll(field => !availableFields.Contains(field));
                    
                    // 添加核心字段（如果缺失）
                    AddCoreFieldsIfMissing(config.SelectedFields, availableFields);
                    
                    if (config.SelectedFields.Count != originalCount)
                    {
                        System.Diagnostics.Debug.WriteLine($"配置{originalName}的选中字段从{originalCount}个调整为{config.SelectedFields.Count}个");
                    }
                }
                
                // 同步ColumnOrder
                if (config.ColumnOrder != null)
                {
                    var originalCount = config.ColumnOrder.Count;
                    config.ColumnOrder.RemoveAll(col => !availableFields.Contains(col.FieldName));
                    
                    // 为新字段添加列配置
                    AddNewFieldColumns(config.ColumnOrder, availableFields);
                    
                    if (config.ColumnOrder.Count != originalCount)
                    {
                        System.Diagnostics.Debug.WriteLine($"配置{originalName}的列配置从{originalCount}个调整为{config.ColumnOrder.Count}个");
                    }
                }
                
                // 更新最后修改时间
                config.LastModified = DateTime.Now;
                
                // 保存更新后的配置
                await _configService.SaveUserConfigAsync(_currentExportType, config);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"自动同步配置字段失败：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 获取当前可用字段
    /// </summary>
    /// <returns>可用字段列表</returns>
    private List<string> GetCurrentAvailableFields()
    {
        return _allColumns.Select(c => c.FieldName).ToList();
    }
    
    /// <summary>
    /// 添加核心字段（如果缺失）
    /// 注意：根据用户自由选择原则，不再自动添加任何字段
    /// </summary>
    /// <param name="selectedFields">已选择的字段列表</param>
    /// <param name="availableFields">可用字段列表</param>
    private void AddCoreFieldsIfMissing(List<string> selectedFields, List<string> availableFields)
    {
        // 不再自动添加任何字段，完全由用户控制
        // 这是为了遵循“导出字段选择自由原则”
    }
    
    /// <summary>
    /// 为新字段添加列配置
    /// 新字段默认不选中，排序放在最后
    /// </summary>
    /// <param name="columnOrder">列配置列表</param>
    /// <param name="availableFields">可用字段列表</param>
    private void AddNewFieldColumns(List<ColumnInfo> columnOrder, List<string> availableFields)
    {
        var existingFields = columnOrder.Select(c => c.FieldName).ToHashSet();
        
        // 获取新增的字段
        var newFields = availableFields.Where(field => !existingFields.Contains(field)).ToList();
        if (newFields.Count == 0) return;
        
        // 将新字段按字母顺序排列，放在最后
        newFields.Sort();
        
        var maxOrder = columnOrder.Count > 0 ? columnOrder.Max(c => c.Order) : -1;
        
        foreach (var field in newFields)
        {
            var displayName = GetDisplayName(field);
            columnOrder.Add(new ColumnInfo
            {
                FieldName = field,
                DisplayName = displayName,
                Order = ++maxOrder, // 放在最后
                IsVisible = false, // 新字段默认不显示，由用户自由选择
                Type = GetColumnType(typeof(string))
            });
            
            System.Diagnostics.Debug.WriteLine($"新增字段：{field} ({displayName}) - 默认不选中，放在位置 {maxOrder}");
        }
    }
}

#endregion

/// <summary>
/// 扩展方法
/// </summary>
public static class ExportTypeExtensions
{
    /// <summary>
    /// 获取枚举描述
    /// </summary>
    public static string GetDescription(this ExportType exportType)
    {
        return exportType switch
        {
            ExportType.CompleteList => "完整IO清单",
            ExportType.CurrentSystemList => "当前控制系统IO清单",
            ExportType.PublishedList => "发布的IO清单",
            _ => exportType.ToString()
        };
    }
}
#endregion


