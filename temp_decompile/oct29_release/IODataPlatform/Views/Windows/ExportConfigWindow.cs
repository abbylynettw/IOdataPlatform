using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Models.ExportModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.Export;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// IO清单导出配置窗口
/// 提供实时预览的导出配置功能，支持列选择、排序和字段管理
/// </summary>
/// <summary>
/// 导出配置窗口的字段兼容性处理扩展
/// </summary>
/// <summary>
/// ExportConfigWindow
/// </summary>
public class ExportConfigWindow : Window, IComponentConnector, IStyleConnector
{
	private readonly IPickerService _picker;

	private readonly ExcelService _excel;

	private readonly IMessageService _message;

	private readonly SqlSugarContext _context;

	private readonly CloudExportConfigService _configService;

	private readonly int? _currentSubProjectId;

	private readonly int? _currentProjectId;

	private readonly IList<IoFullData> _sourceData;

	private readonly ObservableCollection<ColumnInfo> _allColumns = new ObservableCollection<ColumnInfo>();

	private readonly ObservableCollection<object> _previewData = new ObservableCollection<object>();

	private readonly Dictionary<DataGridColumn, ColumnInfo> _columnMapping = new Dictionary<DataGridColumn, ColumnInfo>();

	private readonly ObservableCollection<ExportConfig> _savedConfigs = new ObservableCollection<ExportConfig>();

	private ExportConfig? _currentConfig;

	private ExportType _currentExportType = ExportType.CurrentSystemList;

	private int _currentStep = 1;

	internal RadioButton CompleteListRadio;

	internal RadioButton CurrentSystemRadio;

	internal RadioButton PublishedListRadio;

	internal Wpf.Ui.Controls.Button DefaultExportButton;

	internal Border Step1Border;

	internal Border Step2Border;

	internal Wpf.Ui.Controls.TextBlock Step2Text;

	internal Border Step3Border;

	internal Wpf.Ui.Controls.TextBlock Step3Text;

	internal Grid Step1Panel;

	internal Wpf.Ui.Controls.Button SelectAllFieldsButton;

	internal Wpf.Ui.Controls.Button UnselectAllFieldsButton;

	internal ComboBox SavedConfigsComboBox;

	internal Wpf.Ui.Controls.Button DeleteConfigButton;

	internal Wpf.Ui.Controls.TextBlock SelectedFieldsCountText;

	internal ItemsControl FieldsItemsControl;

	internal Grid Step2Panel;

	internal Wpf.Ui.Controls.TextBlock SelectedFieldsCountText2;

	internal DraggableColumnPanel ColumnPanel;

	internal Grid Step3Panel;

	internal Wpf.Ui.Controls.TextBlock RecordCountText;

	internal Wpf.Ui.Controls.TextBlock FieldCountText;

	internal System.Windows.Controls.DataGrid PreviewDataGrid;

	internal Wpf.Ui.Controls.Button SaveConfigButton;

	internal Wpf.Ui.Controls.Button PreviousButton;

	internal Wpf.Ui.Controls.Button NextButton;

	internal Wpf.Ui.Controls.Button CancelButton;

	internal Wpf.Ui.Controls.Button ExportButton;

	private bool _contentLoaded;

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
		_sourceData = sourceData ?? throw new ArgumentNullException("sourceData");
		_picker = picker ?? throw new ArgumentNullException("picker");
		_excel = excel ?? throw new ArgumentNullException("excel");
		_message = message ?? throw new ArgumentNullException("message");
		_context = context ?? throw new ArgumentNullException("context");
		_configService = configService ?? throw new ArgumentNullException("configService");
		_currentSubProjectId = currentSubProjectId;
		_currentProjectId = GetCurrentProjectId();
		InitializeColumnsForCurrentType();
		base.Loaded += async delegate
		{
			await LoadSavedConfigs();
			await CheckAndSyncFieldChanges();
			LoadDefaultConfigForCurrentType();
			UpdateConfigComboBox();
			UpdateStepUI();
			UpdateSelectedFieldsCount();
		};
	}

	/// <summary>
	/// 刷新预览数据
	/// </summary>
	private void RefreshPreview()
	{
		List<IoFullData> filteredData = GetFilteredData();
		_previewData.Clear();
		int num = Math.Min(100, filteredData.Count);
		for (int i = 0; i < num; i++)
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
		UpdatePreview();
	}

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
			ControlSystem? currentControlSystem = GetCurrentControlSystem();
			if (!currentControlSystem.HasValue)
			{
				InitializeColumns();
				return;
			}
			List<config_controlSystem_mapping> controlSystemMappings = GetControlSystemMappings(currentControlSystem.Value);
			if (controlSystemMappings == null || controlSystemMappings.Count == 0)
			{
				InitializeColumns();
				return;
			}
			PropertyInfo[] source = (from p in typeof(IoFullData).GetProperties()
				where p.CanRead && IsDisplayableProperty(p)
				select p).ToArray();
			int num = 0;
			foreach (config_controlSystem_mapping mapping in controlSystemMappings)
			{
				PropertyInfo propertyInfo = source.FirstOrDefault(delegate(PropertyInfo p)
				{
					string text = p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name;
					return text == mapping.StdField;
				});
				if (propertyInfo != null)
				{
					string displayName = GetDisplayName(propertyInfo.Name);
					string controlSystemFieldName = GetControlSystemFieldName(mapping, currentControlSystem.Value);
					_allColumns.Add(new ColumnInfo
					{
						FieldName = propertyInfo.Name,
						DisplayName = ((!string.IsNullOrEmpty(controlSystemFieldName)) ? controlSystemFieldName : displayName),
						Order = num++,
						IsVisible = true,
						IsRequired = false,
						Type = GetColumnType(propertyInfo.PropertyType)
					});
				}
			}
			FieldsItemsControl.ItemsSource = _allColumns;
		}
		catch (Exception)
		{
			InitializeColumns();
		}
	}

	/// <summary>
	/// 获取当前子项目的控制系统
	/// </summary>
	private ControlSystem? GetCurrentControlSystem()
	{
		if (!_currentSubProjectId.HasValue)
		{
			return null;
		}
		try
		{
			config_project_subProject subProject = (from x in _context.Db.Queryable<config_project_subProject>()
				where (int?)x.Id == _currentSubProjectId
				select x).First();
			config_project_major config_project_major = (from x in _context.Db.Queryable<config_project_major>()
				where x.Id == subProject.MajorId
				select x).First();
			return config_project_major.ControlSystem;
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
		if (!_currentSubProjectId.HasValue)
		{
			return null;
		}
		try
		{
			config_project_subProject subProject = (from x in _context.Db.Queryable<config_project_subProject>()
				where (int?)x.Id == _currentSubProjectId
				select x).First();
			config_project_major config_project_major = (from x in _context.Db.Queryable<config_project_major>()
				where x.Id == subProject.MajorId
				select x).First();
			return config_project_major.ProjectId;
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
			ControlSystem.龙鳍 => (from it in _context.Db.Queryable<config_controlSystem_mapping>()
				where !string.IsNullOrEmpty(it.LqOld) && !string.IsNullOrEmpty(it.StdField)
				select it).ToList(), 
			ControlSystem.中控 => (from it in _context.Db.Queryable<config_controlSystem_mapping>()
				where !string.IsNullOrEmpty(it.ZkOld) && !string.IsNullOrEmpty(it.StdField)
				select it).ToList(), 
			ControlSystem.龙核 => (from it in _context.Db.Queryable<config_controlSystem_mapping>()
				where !string.IsNullOrEmpty(it.LhOld) && !string.IsNullOrEmpty(it.StdField)
				select it).ToList(), 
			ControlSystem.一室 => (from it in _context.Db.Queryable<config_controlSystem_mapping>()
				where !string.IsNullOrEmpty(it.Xt1Old) && !string.IsNullOrEmpty(it.StdField)
				select it).ToList(), 
			ControlSystem.安全级模拟系统 => (from it in _context.Db.Queryable<config_controlSystem_mapping>()
				where !string.IsNullOrEmpty(it.AQJMNOld) && !string.IsNullOrEmpty(it.StdField)
				select it).ToList(), 
			_ => new List<config_controlSystem_mapping>(), 
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
			_ => string.Empty, 
		};
	}

	/// <summary>
	/// 初始化列配置
	/// </summary>
	private void InitializeColumns()
	{
		_allColumns.Clear();
		if (_sourceData == null || _sourceData.Count == 0)
		{
			InitializeDefaultColumns();
			return;
		}
		PropertyInfo[] array = (from p in typeof(IoFullData).GetProperties()
			where p.CanRead && IsDisplayableProperty(p)
			select p).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			PropertyInfo propertyInfo = array[num];
			string displayName = GetDisplayName(propertyInfo.Name);
			_allColumns.Add(new ColumnInfo
			{
				FieldName = propertyInfo.Name,
				DisplayName = displayName,
				Order = num,
				IsVisible = true,
				IsRequired = false,
				Type = GetColumnType(propertyInfo.PropertyType)
			});
		}
		FieldsItemsControl.ItemsSource = _allColumns;
	}

	/// <summary>
	/// 更新步骤界面
	/// </summary>
	private void UpdateStepUI()
	{
		if (Step1Panel != null && Step2Panel != null && Step3Panel != null && PreviousButton != null && NextButton != null && ExportButton != null)
		{
			UpdateStepIndicator();
			Step1Panel.Visibility = ((_currentStep != 1) ? Visibility.Collapsed : Visibility.Visible);
			Step2Panel.Visibility = ((_currentStep != 2) ? Visibility.Collapsed : Visibility.Visible);
			Step3Panel.Visibility = ((_currentStep != 3) ? Visibility.Collapsed : Visibility.Visible);
			PreviousButton.Visibility = ((_currentStep <= 1) ? Visibility.Collapsed : Visibility.Visible);
			NextButton.Visibility = ((_currentStep >= 3) ? Visibility.Collapsed : Visibility.Visible);
			ExportButton.Visibility = ((_currentStep != 3) ? Visibility.Collapsed : Visibility.Visible);
			if (_currentStep == 1)
			{
				NextButton.Content = "下一步";
			}
			else if (_currentStep == 2)
			{
				NextButton.Content = "预览";
				UpdateColumnPanel();
			}
			else if (_currentStep == 3)
			{
				UpdatePreview();
			}
		}
	}

	/// <summary>
	/// 更新步骤指示器
	/// </summary>
	private void UpdateStepIndicator()
	{
		if (Step1Border != null && Step2Border != null && Step3Border != null && Step2Text != null && Step3Text != null)
		{
			Brush brush = Application.Current.Resources["AccentFillColorDefaultBrush"] as Brush;
			Brush brush2 = Application.Current.Resources["ControlFillColorDisabledBrush"] as Brush;
			SolidColorBrush black = Brushes.Black;
			Brush brush3 = Application.Current.Resources["TextFillColorDisabledBrush"] as Brush;
			Step1Border.Background = ((_currentStep >= 1) ? brush : brush2);
			Step2Border.Background = ((_currentStep >= 2) ? brush : brush2);
			Step2Text.Foreground = ((_currentStep >= 2) ? black : brush3);
			Step3Border.Background = ((_currentStep >= 3) ? brush : brush2);
			Step3Text.Foreground = ((_currentStep >= 3) ? black : brush3);
		}
	}

	/// <summary>
	/// 更新选中字段数量
	/// </summary>
	private void UpdateSelectedFieldsCount()
	{
		if (SelectedFieldsCountText != null)
		{
			int value = _allColumns.Count((ColumnInfo c) => c.IsVisible);
			SelectedFieldsCountText.Text = $"已选中：{value}个字段";
			if (SelectedFieldsCountText2 != null)
			{
				SelectedFieldsCountText2.Text = $"已选中：{value}个字段";
			}
		}
	}

	/// <summary>
	/// 更新拖拽面板
	/// </summary>
	private void UpdateColumnPanel()
	{
		if (ColumnPanel != null)
		{
			List<ColumnInfo> list = (from c in _allColumns
				where c.IsVisible
				orderby c.Order
				select c).ToList();
			ColumnPanel.Columns = new ObservableCollection<ColumnInfo>(list);
		}
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
			using DataTable dataTable = GetPreparedExportData();
			int num = Math.Min(100, dataTable.Rows.Count);
			for (int i = 0; i < num; i++)
			{
				DataRow dataRow = dataTable.Rows[i];
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				foreach (DataColumn column in dataTable.Columns)
				{
					dictionary[column.ColumnName] = dataRow[column] ?? string.Empty;
				}
				_previewData.Add(dictionary);
			}
			UpdatePreviewDataGridColumnsFromDataTable(dataTable);
		}
		catch (InvalidOperationException ex)
		{
			await _message.ErrorAsync("预览数据失败\n" + ex.Message);
			_previewData.Clear();
			PreviewDataGrid.ItemsSource = _previewData;
			PreviewDataGrid.Columns.Clear();
		}
		catch (Exception ex2)
		{
			await _message.ErrorAsync("预览数据时发生未知错误\n错误信息：" + ex2.Message + "\n\n请联系管理员或重新尝试。");
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
		foreach (DataColumn column in dataTable.Columns)
		{
			DataGridTextColumn item = new DataGridTextColumn
			{
				Header = column.ColumnName,
				Binding = new Binding("[" + column.ColumnName + "]"),
				Width = new DataGridLength(120.0, DataGridLengthUnitType.Pixel)
			};
			PreviewDataGrid.Columns.Add(item);
		}
		PreviewDataGrid.ItemsSource = _previewData;
	}

	/// <summary>
	/// 更新预览信息
	/// </summary>
	private void UpdatePreviewInfo()
	{
		if (RecordCountText != null && FieldCountText != null)
		{
			List<IoFullData> filteredData = GetFilteredData();
			int value = _allColumns.Count((ColumnInfo c) => c.IsVisible);
			RecordCountText.Text = $"共计：{filteredData.Count}条记录";
			FieldCountText.Text = $"导出字段：{value}个";
		}
	}

	/// <summary>
	/// 判断属性是否应该显示
	/// </summary>
	private static bool IsDisplayableProperty(PropertyInfo property)
	{
		string[] array = new string[4] { "Id", "IsDeleted", "CreateUser", "ModifyUser" };
		if (((ReadOnlySpan<string>)array).Contains(property.Name) || !(property.PropertyType != typeof(object)) || property.PropertyType.IsClass)
		{
			return property.PropertyType == typeof(string);
		}
		return true;
	}

	/// <summary>
	/// 获取字段显示名称
	/// 优先从属性的Display特性中获取Name值，如果没有则使用属性名
	/// </summary>
	private static string GetDisplayName(string fieldName)
	{
		PropertyInfo property = typeof(IoFullData).GetProperty(fieldName);
		if (property != null)
		{
			DisplayAttribute customAttribute = property.GetCustomAttribute<DisplayAttribute>();
			if (customAttribute != null && customAttribute.Name != null)
			{
				return customAttribute.Name;
			}
		}
		return fieldName;
	}

	/// <summary>
	/// 获取列类型
	/// </summary>
	private static ColumnType GetColumnType(Type propertyType)
	{
		if (propertyType == typeof(int) || propertyType == typeof(int?) || propertyType == typeof(double) || propertyType == typeof(double?) || propertyType == typeof(decimal) || propertyType == typeof(decimal?))
		{
			return ColumnType.Number;
		}
		if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
		{
			return ColumnType.Date;
		}
		return ColumnType.Text;
	}

	/// <summary>
	/// 初始化默认列配置（当没有数据时使用）
	/// </summary>
	private void InitializeDefaultColumns()
	{
		var array = new[]
		{
			new
			{
				Field = "TagName",
				Display = "信号名称",
				Type = ColumnType.Text
			},
			new
			{
				Field = "IoType",
				Display = "IO类型",
				Type = ColumnType.Text
			},
			new
			{
				Field = "StationName",
				Display = "控制系统",
				Type = ColumnType.Text
			},
			new
			{
				Field = "SignalPositionNumber",
				Display = "位号",
				Type = ColumnType.Text
			},
			new
			{
				Field = "Description",
				Display = "信号描述",
				Type = ColumnType.Text
			},
			new
			{
				Field = "CabinetNumber",
				Display = "机柜编号",
				Type = ColumnType.Text
			}
		};
		for (int i = 0; i < array.Length; i++)
		{
			var anon = array[i];
			_allColumns.Add(new ColumnInfo
			{
				FieldName = anon.Field,
				DisplayName = anon.Display,
				Order = i,
				IsVisible = true,
				IsRequired = false,
				Type = anon.Type
			});
		}
	}

	/// <summary>
	/// 导出类型选择变化
	/// </summary>
	private void ExportTypeRadio_Checked(object sender, RoutedEventArgs e)
	{
		if (sender == CompleteListRadio)
		{
			_currentExportType = ExportType.CompleteList;
		}
		else if (sender == CurrentSystemRadio)
		{
			_currentExportType = ExportType.CurrentSystemList;
		}
		else if (sender == PublishedListRadio)
		{
			_currentExportType = ExportType.PublishedList;
		}
		_currentStep = 1;
		_currentConfig = null;
		InitializeColumnsForCurrentType();
		LoadDefaultConfigForCurrentType();
		UpdateConfigComboBox();
		UpdateStepUI();
		UpdateSelectedFieldsCount();
	}

	/// <summary>
	/// 下一步按钮点击
	/// </summary>
	private void NextButton_Click(object sender, RoutedEventArgs e)
	{
		if (_currentStep == 1)
		{
			if (_allColumns.Count((ColumnInfo c) => c.IsVisible) == 0)
			{
				_message.WarnAsync("请至少选择一个字段");
				return;
			}
			_currentStep = 2;
		}
		else if (_currentStep == 2)
		{
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
		foreach (ColumnInfo allColumn in _allColumns)
		{
			allColumn.IsVisible = true;
		}
		UpdateSelectedFieldsCount();
	}

	/// <summary>
	/// 取消全选字段按钮点击
	/// </summary>
	private void UnselectAllFieldsButton_Click(object sender, RoutedEventArgs e)
	{
		foreach (ColumnInfo allColumn in _allColumns)
		{
			allColumn.IsVisible = false;
		}
		UpdateSelectedFieldsCount();
	}

	/// <summary>
	/// 获取过滤后的数据
	/// 不进行任何排序，直接返回原始数据
	/// </summary>
	private List<IoFullData> GetFilteredData()
	{
		if (_sourceData == null)
		{
			return new List<IoFullData>();
		}
		return _sourceData.ToList();
	}

	/// <summary>
	/// 导出按钮点击
	/// </summary>
	private async void ExportButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			string filePath = _picker.SaveFile("Excel 文件(*.xlsx)|*.xlsx", "IO清单导出");
			if (filePath == null)
			{
				return;
			}
			using DataTable dataTable = GetPreparedExportData();
			if (dataTable.Rows.Count == 0)
			{
				await _message.WarnAsync("没有数据可导出，请检查数据源和筛选条件。");
				return;
			}
			await _excel.FastExportAsync(dataTable, filePath);
			await _message.MessageAsync($"导出成功：{filePath}\n\n共导出 {dataTable.Rows.Count} 条记录，{dataTable.Columns.Count} 个字段。");
			base.DialogResult = true;
			Close();
		}
		catch (InvalidOperationException ex)
		{
			await _message.ErrorAsync("导出失败\n" + ex.Message);
		}
		catch (UnauthorizedAccessException)
		{
			await _message.ErrorAsync("导出失败\n文件被占用或没有写入权限，请检查：\n1. 文件是否在Excel中打开\n2. 是否有文件夹的写入权限");
		}
		catch (DirectoryNotFoundException)
		{
			await _message.ErrorAsync("导出失败\n指定的目录不存在，请选择有效的保存路径。");
		}
		catch (IOException ex4)
		{
			await _message.ErrorAsync("导出失败\n文件操作错误：" + ex4.Message + "\n\n请检查：\n1. 磁盘空间是否足够\n2. 文件路径是否正确");
		}
		catch (Exception ex5)
		{
			await _message.ErrorAsync("导出失败\n发生未知错误：" + ex5.Message + "\n\n请联系管理员或重新尝试。");
		}
	}

	/// <summary>
	/// 默认配置导出按钮点击
	/// </summary>
	private async void DefaultExportButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			string exportTypeName = _currentExportType.GetDescription();
			string filename = exportTypeName + "_默认配置";
			string filePath = _picker.SaveFile("Excel 文件(*.xlsx)|*.xlsx", filename);
			if (filePath == null)
			{
				return;
			}
			List<IoFullData> filteredData = GetFilteredData();
			if (filteredData == null || filteredData.Count == 0)
			{
				await _message.WarnAsync("没有" + exportTypeName + "数据可导出。");
				return;
			}
			DataTable dataTable;
			string configDescription;
			if (_currentExportType == ExportType.CurrentSystemList || _currentExportType == ExportType.PublishedList)
			{
				ControlSystem? currentControlSystem = GetCurrentControlSystem();
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
				dataTable = await filteredData.ToTableByDisplayAttributeAsync();
				configDescription = "系统默认Display特性配置";
			}
			if (dataTable.Rows.Count == 0)
			{
				await _message.WarnAsync("没有数据可导出。");
				return;
			}
			await _excel.FastExportAsync(dataTable, filePath);
			await _message.MessageAsync($"默认配置导出成功：{filePath}\n\n导出类型：{exportTypeName}\n使用配置：{configDescription}\n共导出 {dataTable.Rows.Count} 条记录，{dataTable.Columns.Count} 个字段。");
			base.DialogResult = true;
			Close();
		}
		catch (Exception ex)
		{
			await _message.ErrorAsync("默认配置导出失败：" + ex.Message);
		}
	}

	/// <summary>
	/// 取消按钮点击
	/// </summary>
	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
		Close();
	}

	/// <summary>
	/// 全选列按钮点击
	/// </summary>
	private void SelectAllColumnsButton_Click(object sender, RoutedEventArgs e)
	{
		foreach (ColumnInfo allColumn in _allColumns)
		{
			allColumn.IsVisible = true;
		}
		RefreshPreview();
	}

	/// <summary>
	/// 取消全选列按钮点击
	/// </summary>
	private void UnselectAllColumnsButton_Click(object sender, RoutedEventArgs e)
	{
		foreach (ColumnInfo allColumn in _allColumns)
		{
			allColumn.IsVisible = false;
		}
		RefreshPreview();
	}

	/// <summary>
	/// 列可见性复选框选中
	/// </summary>
	private void ColumnVisibilityCheckBox_Checked(object sender, RoutedEventArgs e)
	{
		if (sender is CheckBox child)
		{
			DataGridColumnHeader dataGridColumnHeader = FindParent<DataGridColumnHeader>((DependencyObject)(object)child);
			if (dataGridColumnHeader?.Column != null && _columnMapping.TryGetValue(dataGridColumnHeader.Column, out ColumnInfo value))
			{
				value.IsVisible = true;
				RefreshPreview();
			}
		}
	}

	/// <summary>
	/// 列可见性复选框取消选中
	/// </summary>
	private void ColumnVisibilityCheckBox_Unchecked(object sender, RoutedEventArgs e)
	{
		if (sender is CheckBox child)
		{
			DataGridColumnHeader dataGridColumnHeader = FindParent<DataGridColumnHeader>((DependencyObject)(object)child);
			if (dataGridColumnHeader?.Column != null && _columnMapping.TryGetValue(dataGridColumnHeader.Column, out ColumnInfo value))
			{
				value.IsVisible = false;
				RefreshPreview();
			}
		}
	}

	/// <summary>
	/// 查找父级元素
	/// </summary>
	private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
	{
		DependencyObject parent = VisualTreeHelper.GetParent(child);
		while (parent != null && !(parent is T))
		{
			parent = VisualTreeHelper.GetParent(parent);
		}
		return (T)(object)((parent is T) ? parent : null);
	}

	/// <summary>
	/// 数据表格列重新排序
	/// </summary>
	private void PreviewDataGrid_ColumnReordered(object sender, DataGridColumnEventArgs e)
	{
		for (int i = 0; i < PreviewDataGrid.Columns.Count; i++)
		{
			if (_columnMapping.TryGetValue(PreviewDataGrid.Columns[i], out ColumnInfo value))
			{
				value.Order = i;
			}
		}
		List<ColumnInfo> list = _allColumns.OrderBy((ColumnInfo c) => c.Order).ToList();
		_allColumns.Clear();
		foreach (ColumnInfo item in list)
		{
			_allColumns.Add(item);
		}
		UpdatePreviewInfo();
	}

	/// <summary>
	/// 从拖拽面板同步列顺序到_allColumns
	/// </summary>
	private void SyncColumnOrderFromPanel()
	{
		if (ColumnPanel?.Columns == null)
		{
			return;
		}
		List<ColumnInfo> list = ColumnPanel.Columns.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			ColumnInfo panelColumn = list[i];
			ColumnInfo columnInfo = _allColumns.FirstOrDefault((ColumnInfo c) => c.FieldName == panelColumn.FieldName);
			if (columnInfo != null)
			{
				columnInfo.Order = i;
			}
		}
		List<ColumnInfo> list2 = _allColumns.OrderBy((ColumnInfo c) => c.Order).ToList();
		_allColumns.Clear();
		foreach (ColumnInfo item in list2)
		{
			_allColumns.Add(item);
		}
	}

	/// <summary>
	/// 创建按用户选择的列和顺序的自定义DataTable
	/// </summary>
	private DataTable CreateCustomDataTable(List<IoFullData> data, List<ColumnInfo> visibleColumns)
	{
		DataTable dataTable = new DataTable();
		foreach (ColumnInfo item in visibleColumns.OrderBy((ColumnInfo c) => c.Order))
		{
			PropertyInfo property = typeof(IoFullData).GetProperty(item.FieldName);
			Type type = ((property != null) ? (Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType) : typeof(string));
			dataTable.Columns.Add(item.DisplayName, type);
		}
		foreach (IoFullData datum in data)
		{
			DataRow dataRow = dataTable.NewRow();
			foreach (ColumnInfo item2 in visibleColumns.OrderBy((ColumnInfo c) => c.Order))
			{
				try
				{
					PropertyInfo property2 = typeof(IoFullData).GetProperty(item2.FieldName);
					if (property2 != null)
					{
						object value = property2.GetValue(datum);
						if (value == null)
						{
							dataRow[item2.DisplayName] = DBNull.Value;
						}
						else
						{
							dataRow[item2.DisplayName] = value;
						}
					}
					else
					{
						dataRow[item2.DisplayName] = DBNull.Value;
					}
				}
				catch
				{
					dataRow[item2.DisplayName] = DBNull.Value;
				}
			}
			dataTable.Rows.Add(dataRow);
		}
		return dataTable;
	}

	/// <summary>
	/// 创建按用户选择的列和顺序，但使用映射字段名的自定义DataTable
	/// </summary>
	private DataTable CreateCustomDataTableWithMapping(List<IoFullData> data, List<ColumnInfo> visibleColumns, Dictionary<string, string> mappingDict)
	{
		DataTable dataTable = new DataTable();
		foreach (ColumnInfo item in visibleColumns.OrderBy((ColumnInfo c) => c.Order))
		{
			PropertyInfo property = typeof(IoFullData).GetProperty(item.FieldName);
			string key = property?.GetCustomAttribute<DisplayAttribute>()?.Name ?? item.FieldName;
			if (mappingDict.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
			{
				Type type = ((property != null) ? (Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType) : typeof(string));
				dataTable.Columns.Add(value, type);
			}
			else
			{
				Type type2 = ((property != null) ? (Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType) : typeof(string));
				dataTable.Columns.Add(item.DisplayName, type2);
			}
		}
		foreach (IoFullData datum in data)
		{
			DataRow dataRow = dataTable.NewRow();
			foreach (ColumnInfo item2 in visibleColumns.OrderBy((ColumnInfo c) => c.Order))
			{
				try
				{
					PropertyInfo property2 = typeof(IoFullData).GetProperty(item2.FieldName);
					if (!(property2 != null))
					{
						continue;
					}
					string key2 = property2.GetCustomAttribute<DisplayAttribute>()?.Name ?? item2.FieldName;
					object value2 = property2.GetValue(datum);
					string value3;
					string text = ((!mappingDict.TryGetValue(key2, out value3) || string.IsNullOrEmpty(value3)) ? item2.DisplayName : value3);
					if (dataTable.Columns.Contains(text))
					{
						if (value2 == null)
						{
							dataRow[text] = DBNull.Value;
						}
						else
						{
							dataRow[text] = value2;
						}
					}
				}
				catch
				{
				}
			}
			dataTable.Rows.Add(dataRow);
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
			List<IoFullData> filteredData = GetFilteredData();
			if (filteredData == null || filteredData.Count == 0)
			{
				throw new InvalidOperationException("没有可导出的数据，请检查数据源或筛选条件。");
			}
			List<ColumnInfo> list = (from c in _allColumns
				where c.IsVisible
				orderby c.Order
				select c).ToList();
			if (list.Count == 0)
			{
				throw new InvalidOperationException("请至少选择一个字段进行导出。");
			}
			DataTable dataTable;
			if (_currentExportType == ExportType.CurrentSystemList || _currentExportType == ExportType.PublishedList)
			{
				ControlSystem? currentControlSystem = GetCurrentControlSystem();
				if (!currentControlSystem.HasValue)
				{
					throw new InvalidOperationException("无法获取当前子项目的控制系统信息，请检查项目配置。");
				}
				try
				{
					Dictionary<string, string> mappingDict = (from it in _context.Db.Queryable<config_controlSystem_mapping>()
						where it.StdField != null
						select it).ToList().ToDictionary((config_controlSystem_mapping it) => it.StdField, (config_controlSystem_mapping it) => currentControlSystem.Value switch
					{
						ControlSystem.龙鳍 => it.LqOld, 
						ControlSystem.中控 => it.ZkOld, 
						ControlSystem.龙核 => it.LhOld, 
						ControlSystem.一室 => it.Xt1Old, 
						ControlSystem.安全级模拟系统 => it.AQJMNOld, 
						_ => null, 
					});
					dataTable = CreateCustomDataTableWithMapping(filteredData, list, mappingDict);
					if (dataTable.Columns.Count == 0)
					{
						throw new InvalidOperationException($"当前控制系统（{currentControlSystem.Value}）的字段映射配置不完整，请检查配置表 config_controlSystem_mapping。");
					}
				}
				catch (ArgumentException ex) when (ex.Message.Contains("same key"))
				{
					throw new InvalidOperationException($"当前控制系统（{currentControlSystem.Value}）的映射配置存在重复字段，请联系管理员检查数据库配置。\n错误详情：{ex.Message}");
				}
				catch (Exception ex2)
				{
					throw new InvalidOperationException("处理当前控制系统数据时发生错误：" + ex2.Message);
				}
			}
			else
			{
				dataTable = CreateCustomDataTable(filteredData, list);
			}
			return dataTable;
		}
		catch (Exception ex3) when (!(ex3 is InvalidOperationException))
		{
			throw new InvalidOperationException("准备导出数据时发生错误：" + ex3.Message);
		}
	}

	/// <summary>
	/// 加载已保存的配置
	/// </summary>
	private async Task LoadSavedConfigs()
	{
		try
		{
			_savedConfigs.Clear();
			List<ExportConfig> list = await _configService.GetUserConfigsAsync(_currentExportType, _currentProjectId);
			if (list != null)
			{
				foreach (ExportConfig item in list)
				{
					_savedConfigs.Add(item);
				}
			}
			LoadDefaultConfigForCurrentType();
		}
		catch (Exception)
		{
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
			await _message.ErrorAsync("保存配置失败：" + ex.Message);
		}
	}

	/// <summary>
	/// 加载当前导出类型的默认配置
	/// 根据项目规范，同类型导出中只有一个默认配置
	/// </summary>
	private void LoadDefaultConfigForCurrentType()
	{
		ExportConfig exportConfig = _savedConfigs.FirstOrDefault((ExportConfig c) => c.Type == _currentExportType && c.IsSystemDefault);
		if (exportConfig != null)
		{
			ApplyConfig(exportConfig);
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
			List<ColumnInfo> columnOrder = config.ColumnOrder;
			if (columnOrder == null || columnOrder.Count <= 0)
			{
				return;
			}
			_allColumns.Clear();
			foreach (ColumnInfo item in config.ColumnOrder.OrderBy((ColumnInfo c) => c.Order))
			{
				_allColumns.Add(new ColumnInfo
				{
					FieldName = item.FieldName,
					DisplayName = item.DisplayName,
					IsVisible = item.IsVisible,
					Order = item.Order,
					Type = item.Type,
					IsRequired = item.IsRequired
				});
			}
			if (FieldsItemsControl != null)
			{
				FieldsItemsControl.ItemsSource = _allColumns;
			}
			UpdateSelectedFieldsCount();
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// 保存当前配置
	/// </summary>
	private async Task SaveCurrentConfig(string configName, bool setAsDefault = false)
	{
		try
		{
			SyncColumnOrderFromPanel();
			if (setAsDefault)
			{
				foreach (ExportConfig item in _savedConfigs.Where((ExportConfig c) => c.Type == _currentExportType))
				{
					item.IsSystemDefault = false;
				}
			}
			ExportConfig exportConfig = new ExportConfig
			{
				ConfigName = configName,
				Type = _currentExportType,
				ColumnOrder = _allColumns.ToList(),
				CreatedTime = DateTime.Now,
				LastModified = DateTime.Now,
				IsSystemDefault = setAsDefault
			};
			ExportConfig exportConfig2 = _savedConfigs.FirstOrDefault((ExportConfig c) => c.ConfigName == configName && c.Type == _currentExportType);
			if (exportConfig2 != null)
			{
				_savedConfigs.Remove(exportConfig2);
			}
			_savedConfigs.Add(exportConfig);
			_currentConfig = exportConfig;
			await SaveConfigToCloud(exportConfig);
			UpdateConfigComboBox();
			await _message.MessageAsync("配置“" + configName + "”保存成功！");
		}
		catch (Exception ex)
		{
			await _message.ErrorAsync("保存配置失败：" + ex.Message);
		}
	}

	/// <summary>
	/// 保存配置按钮点击
	/// </summary>
	private async void SaveConfigButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (_allColumns.Count((ColumnInfo c) => c.IsVisible) == 0)
			{
				await _message.WarnAsync("请至少选择一个字段后再保存配置！");
				return;
			}
			(string, bool) tuple = await ShowConfigNameDialog();
			if (!string.IsNullOrEmpty(tuple.Item1))
			{
				await SaveCurrentConfig(tuple.Item1, tuple.Item2);
			}
		}
		catch (Exception ex)
		{
			await _message.ErrorAsync("保存配置失败：" + ex.Message);
		}
	}

	/// <summary>
	/// 显示配置名称输入对话框
	/// </summary>
	private async Task<(string ConfigName, bool SetAsDefault)> ShowConfigNameDialog()
	{
		Window inputWindow = new Window
		{
			Title = "保存配置",
			Width = 350.0,
			Height = 180.0,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			Owner = this,
			ResizeMode = ResizeMode.NoResize
		};
		Grid grid = new Grid();
		grid.RowDefinitions.Add(new RowDefinition
		{
			Height = GridLength.Auto
		});
		grid.RowDefinitions.Add(new RowDefinition
		{
			Height = GridLength.Auto
		});
		grid.RowDefinitions.Add(new RowDefinition
		{
			Height = GridLength.Auto
		});
		grid.RowDefinitions.Add(new RowDefinition
		{
			Height = new GridLength(1.0, GridUnitType.Star)
		});
		grid.RowDefinitions.Add(new RowDefinition
		{
			Height = GridLength.Auto
		});
		Wpf.Ui.Controls.TextBlock element = new Wpf.Ui.Controls.TextBlock
		{
			Text = "请输入配置名称：",
			Margin = new Thickness(20.0, 20.0, 20.0, 10.0),
			FontWeight = FontWeights.Medium
		};
		Grid.SetRow(element, 0);
		grid.Children.Add(element);
		string value = _currentExportType switch
		{
			ExportType.CompleteList => "完整", 
			ExportType.CurrentSystemList => "控制系统", 
			ExportType.PublishedList => "发布", 
			_ => "配置", 
		};
		Wpf.Ui.Controls.TextBox nameTextBox = new Wpf.Ui.Controls.TextBox
		{
			Text = $"配置_{value}_{DateTime.Now:yyyyMMdd_HHmmss}",
			Margin = new Thickness(20.0, 0.0, 20.0, 15.0)
		};
		Grid.SetRow(nameTextBox, 1);
		grid.Children.Add(nameTextBox);
		CheckBox checkBox = new CheckBox
		{
			Content = "设为默认配置",
			Margin = new Thickness(20.0, 0.0, 20.0, 15.0),
			IsChecked = false
		};
		Grid.SetRow(checkBox, 2);
		grid.Children.Add(checkBox);
		Grid.SetRow(new Border(), 3);
		StackPanel stackPanel = new StackPanel
		{
			Orientation = Orientation.Horizontal,
			HorizontalAlignment = HorizontalAlignment.Right,
			Margin = new Thickness(20.0, 10.0, 20.0, 20.0)
		};
		Wpf.Ui.Controls.Button button = new Wpf.Ui.Controls.Button
		{
			Content = "确定",
			Margin = new Thickness(0.0, 0.0, 10.0, 0.0),
			MinWidth = 80.0,
			Appearance = ControlAppearance.Primary
		};
		Wpf.Ui.Controls.Button button2 = new Wpf.Ui.Controls.Button
		{
			Content = "取消",
			MinWidth = 80.0
		};
		stackPanel.Children.Add(button);
		stackPanel.Children.Add(button2);
		Grid.SetRow(stackPanel, 4);
		grid.Children.Add(stackPanel);
		inputWindow.Content = grid;
		bool? dialogResult = null;
		button.Click += delegate
		{
			if (string.IsNullOrWhiteSpace(nameTextBox.Text))
			{
				_message.WarnAsync("请输入配置名称！");
			}
			else
			{
				dialogResult = true;
				inputWindow.Close();
			}
		};
		button2.Click += delegate
		{
			dialogResult = false;
			inputWindow.Close();
		};
		inputWindow.ShowDialog();
		if (dialogResult == true && !string.IsNullOrWhiteSpace(nameTextBox.Text))
		{
			return (ConfigName: nameTextBox.Text.Trim(), SetAsDefault: checkBox.IsChecked == true);
		}
		return (ConfigName: string.Empty, SetAsDefault: false);
	}

	/// <summary>
	/// 更新配置下拉框
	/// </summary>
	private void UpdateConfigComboBox()
	{
		if (SavedConfigsComboBox == null)
		{
			return;
		}
		SavedConfigsComboBox.Items.Clear();
		SavedConfigsComboBox.Items.Add(new
		{
			Config = (ExportConfig)null,
			Display = "选择已保存的配置..."
		});
		List<ExportConfig> list = _savedConfigs.Where((ExportConfig c) => c.Type == _currentExportType).ToList();
		int num = 0;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			ExportConfig exportConfig = list[num2];
			string text = exportConfig.ConfigName;
			if (exportConfig.IsSystemDefault)
			{
				text += " (默认)";
			}
			SavedConfigsComboBox.Items.Add(new
			{
				Config = exportConfig,
				Display = text
			});
			if (_currentConfig != null && exportConfig.ConfigName == _currentConfig.ConfigName)
			{
				num = num2 + 1;
			}
		}
		if (SavedConfigsComboBox.Items.Count > num)
		{
			SavedConfigsComboBox.SelectedIndex = num;
		}
	}

	/// <summary>
	/// 配置下拉框选择变化 - 立即应用配置
	/// </summary>
	private async void SavedConfigsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (SavedConfigsComboBox.SelectedItem == null || SavedConfigsComboBox.SelectedIndex == 0)
		{
			return;
		}
		try
		{
			dynamic selectedItem = SavedConfigsComboBox.SelectedItem;
			ExportConfig exportConfig = (ExportConfig)selectedItem.Config;
			if (exportConfig != null)
			{
				ApplyConfig(exportConfig);
			}
		}
		catch (Exception)
		{
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
		dynamic selectedItem = SavedConfigsComboBox.SelectedItem;
		ExportConfig exportConfig = (ExportConfig)selectedItem.Config;
		if (exportConfig == null)
		{
			await _message.WarnAsync("请选择一个有效的配置！");
		}
		else
		{
			if (!ShowCustomConfirmDialog("确定要删除配置“" + exportConfig.ConfigName + "”吗？"))
			{
				return;
			}
			try
			{
				string text = exportConfig.ConfigName;
				if (text.StartsWith("[我的] "))
				{
					text = text.Replace("[我的] ", "");
				}
				else if (!text.StartsWith("[我的] "))
				{
					await _message.WarnAsync("只能删除您自己创建的配置！");
					return;
				}
				_savedConfigs.Remove(exportConfig);
				await _configService.DeleteUserConfigAsync(_currentExportType, text);
				UpdateConfigComboBox();
				await _message.MessageAsync("配置已删除！");
			}
			catch (Exception ex)
			{
				await _message.ErrorAsync("删除配置失败：" + ex.Message);
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
		Window confirmWindow = new Window
		{
			Title = "系统提示",
			Width = 350.0,
			Height = 150.0,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			Owner = this,
			ResizeMode = ResizeMode.NoResize
		};
		Grid grid = new Grid();
		grid.RowDefinitions.Add(new RowDefinition
		{
			Height = new GridLength(1.0, GridUnitType.Star)
		});
		grid.RowDefinitions.Add(new RowDefinition
		{
			Height = GridLength.Auto
		});
		Wpf.Ui.Controls.TextBlock element = new Wpf.Ui.Controls.TextBlock
		{
			Text = message,
			Margin = new Thickness(20.0),
			TextWrapping = TextWrapping.Wrap,
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Center
		};
		Grid.SetRow(element, 0);
		grid.Children.Add(element);
		StackPanel stackPanel = new StackPanel
		{
			Orientation = Orientation.Horizontal,
			HorizontalAlignment = HorizontalAlignment.Center,
			Margin = new Thickness(20.0, 10.0, 20.0, 20.0)
		};
		Wpf.Ui.Controls.Button button = new Wpf.Ui.Controls.Button
		{
			Content = "确认",
			Margin = new Thickness(0.0, 0.0, 10.0, 0.0),
			MinWidth = 80.0,
			Appearance = ControlAppearance.Primary
		};
		Wpf.Ui.Controls.Button button2 = new Wpf.Ui.Controls.Button
		{
			Content = "取消",
			MinWidth = 80.0
		};
		stackPanel.Children.Add(button);
		stackPanel.Children.Add(button2);
		Grid.SetRow(stackPanel, 1);
		grid.Children.Add(stackPanel);
		confirmWindow.Content = grid;
		bool? dialogResult = null;
		button.Click += delegate
		{
			dialogResult = true;
			confirmWindow.Close();
		};
		button2.Click += delegate
		{
			dialogResult = false;
			confirmWindow.Close();
		};
		confirmWindow.ShowDialog();
		return dialogResult == true;
	}

	/// <summary>
	/// 检查并同步字段变更
	/// 在加载配置时自动检查字段变化，确保配置的有效性
	/// </summary>
	private async Task CheckAndSyncFieldChanges()
	{
		_ = 1;
		try
		{
			List<string> availableFields = GetCurrentAvailableFields();
			await _configService.SyncFieldChangesForUser(_currentExportType, availableFields, _currentProjectId);
			await CheckLoadedConfigsFieldSync(availableFields);
		}
		catch (Exception)
		{
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
			List<ExportConfig> list = new List<ExportConfig>();
			foreach (ExportConfig item in _savedConfigs.Where((ExportConfig c) => c.Type == _currentExportType))
			{
				if (_configService.NeedFieldSync(item, availableFields))
				{
					list.Add(item);
				}
			}
			if (list.Count > 0)
			{
				await AutoSyncConfigFields(list, availableFields);
			}
		}
		catch (Exception)
		{
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
			foreach (ExportConfig config in configs)
			{
				_ = config.ConfigName;
				if (config.SelectedFields != null)
				{
					_ = config.SelectedFields.Count;
					config.SelectedFields.RemoveAll((string field) => !availableFields.Contains(field));
					AddCoreFieldsIfMissing(config.SelectedFields, availableFields);
					_ = config.SelectedFields.Count;
				}
				if (config.ColumnOrder != null)
				{
					_ = config.ColumnOrder.Count;
					config.ColumnOrder.RemoveAll((ColumnInfo col) => !availableFields.Contains(col.FieldName));
					AddNewFieldColumns(config.ColumnOrder, availableFields);
					_ = config.ColumnOrder.Count;
				}
				config.LastModified = DateTime.Now;
				await _configService.SaveUserConfigAsync(_currentExportType, config);
			}
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// 获取当前可用字段
	/// </summary>
	/// <returns>可用字段列表</returns>
	private List<string> GetCurrentAvailableFields()
	{
		return _allColumns.Select((ColumnInfo c) => c.FieldName).ToList();
	}

	/// <summary>
	/// 添加核心字段（如果缺失）
	/// 注意：根据用户自由选择原则，不再自动添加任何字段
	/// </summary>
	/// <param name="selectedFields">已选择的字段列表</param>
	/// <param name="availableFields">可用字段列表</param>
	private void AddCoreFieldsIfMissing(List<string> selectedFields, List<string> availableFields)
	{
	}

	/// <summary>
	/// 为新字段添加列配置
	/// 新字段默认不选中，排序放在最后
	/// </summary>
	/// <param name="columnOrder">列配置列表</param>
	/// <param name="availableFields">可用字段列表</param>
	private void AddNewFieldColumns(List<ColumnInfo> columnOrder, List<string> availableFields)
	{
		HashSet<string> existingFields = columnOrder.Select((ColumnInfo c) => c.FieldName).ToHashSet();
		List<string> list = availableFields.Where((string field) => !existingFields.Contains(field)).ToList();
		if (list.Count == 0)
		{
			return;
		}
		list.Sort();
		int num = ((columnOrder.Count > 0) ? columnOrder.Max((ColumnInfo c) => c.Order) : (-1));
		foreach (string item in list)
		{
			string displayName = GetDisplayName(item);
			ColumnInfo columnInfo = new ColumnInfo
			{
				FieldName = item,
				DisplayName = displayName
			};
			num = (columnInfo.Order = num + 1);
			columnInfo.IsVisible = false;
			columnInfo.Type = GetColumnType(typeof(string));
			columnOrder.Add(columnInfo);
		}
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/windows/exportconfigwindow.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			CompleteListRadio = (RadioButton)target;
			CompleteListRadio.Checked += ExportTypeRadio_Checked;
			break;
		case 2:
			CurrentSystemRadio = (RadioButton)target;
			CurrentSystemRadio.Checked += ExportTypeRadio_Checked;
			break;
		case 3:
			PublishedListRadio = (RadioButton)target;
			PublishedListRadio.Checked += ExportTypeRadio_Checked;
			break;
		case 4:
			DefaultExportButton = (Wpf.Ui.Controls.Button)target;
			DefaultExportButton.Click += DefaultExportButton_Click;
			break;
		case 5:
			Step1Border = (Border)target;
			break;
		case 6:
			Step2Border = (Border)target;
			break;
		case 7:
			Step2Text = (Wpf.Ui.Controls.TextBlock)target;
			break;
		case 8:
			Step3Border = (Border)target;
			break;
		case 9:
			Step3Text = (Wpf.Ui.Controls.TextBlock)target;
			break;
		case 10:
			Step1Panel = (Grid)target;
			break;
		case 11:
			SelectAllFieldsButton = (Wpf.Ui.Controls.Button)target;
			SelectAllFieldsButton.Click += SelectAllFieldsButton_Click;
			break;
		case 12:
			UnselectAllFieldsButton = (Wpf.Ui.Controls.Button)target;
			UnselectAllFieldsButton.Click += UnselectAllFieldsButton_Click;
			break;
		case 13:
			SavedConfigsComboBox = (ComboBox)target;
			SavedConfigsComboBox.SelectionChanged += SavedConfigsComboBox_SelectionChanged;
			break;
		case 14:
			DeleteConfigButton = (Wpf.Ui.Controls.Button)target;
			DeleteConfigButton.Click += DeleteConfigButton_Click;
			break;
		case 15:
			SelectedFieldsCountText = (Wpf.Ui.Controls.TextBlock)target;
			break;
		case 16:
			FieldsItemsControl = (ItemsControl)target;
			break;
		case 18:
			Step2Panel = (Grid)target;
			break;
		case 19:
			SelectedFieldsCountText2 = (Wpf.Ui.Controls.TextBlock)target;
			break;
		case 20:
			ColumnPanel = (DraggableColumnPanel)target;
			break;
		case 21:
			Step3Panel = (Grid)target;
			break;
		case 22:
			RecordCountText = (Wpf.Ui.Controls.TextBlock)target;
			break;
		case 23:
			FieldCountText = (Wpf.Ui.Controls.TextBlock)target;
			break;
		case 24:
			PreviewDataGrid = (System.Windows.Controls.DataGrid)target;
			break;
		case 25:
			SaveConfigButton = (Wpf.Ui.Controls.Button)target;
			SaveConfigButton.Click += SaveConfigButton_Click;
			break;
		case 26:
			PreviousButton = (Wpf.Ui.Controls.Button)target;
			PreviousButton.Click += PreviousButton_Click;
			break;
		case 27:
			NextButton = (Wpf.Ui.Controls.Button)target;
			NextButton.Click += NextButton_Click;
			break;
		case 28:
			CancelButton = (Wpf.Ui.Controls.Button)target;
			CancelButton.Click += CancelButton_Click;
			break;
		case 29:
			ExportButton = (Wpf.Ui.Controls.Button)target;
			ExportButton.Click += ExportButton_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IStyleConnector.Connect(int connectionId, object target)
	{
		if (connectionId == 17)
		{
			((CheckBox)target).Checked += FieldCheckBox_Checked;
			((CheckBox)target).Unchecked += FieldCheckBox_Unchecked;
		}
	}
}
