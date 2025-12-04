using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using IODataPlatform.Models.ExcelModels;
using LYSoft.Libs;
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// 导入阀门顺序窗口
/// 功能：从 Excel 导入阀门编号和气口顺序，匹配到 IO 数据的 DP阀位顺序字段
/// 注意：本功能只负责数据导入和匹配，不进行排序操作
/// 排序功能由导出功能或单独的排序工具负责
/// </summary>
/// <summary>
/// ValveSortingWindow
/// </summary>
public class ValveSortingWindow : Window, IComponentConnector
{
	private List<IoFullData> _originalData;

	private List<ValveData> _valveData = new List<ValveData>();

	private List<MatchResult> _matchResults = new List<MatchResult>();

	private readonly ExcelService _excelService;

	internal Wpf.Ui.Controls.Button DownloadTemplateButton;

	internal Wpf.Ui.Controls.Button SelectFileButton;

	internal Wpf.Ui.Controls.Button ProcessButton;

	internal Wpf.Ui.Controls.TextBlock StatusText;

	internal TabControl DataTabControl;

	internal TabItem ValveDataTab;

	internal Wpf.Ui.Controls.TextBlock ValveDataCountText;

	internal System.Windows.Controls.DataGrid ValveDataGrid;

	internal TabItem MatchResultTab;

	internal Wpf.Ui.Controls.TextBlock MatchedCountText;

	internal Wpf.Ui.Controls.TextBlock UnmatchedCountText;

	internal System.Windows.Controls.DataGrid MatchResultGrid;

	internal Wpf.Ui.Controls.Button CancelButton;

	internal Wpf.Ui.Controls.Button ConfirmButton;

	private bool _contentLoaded;

	public ValveSortingWindow(ExcelService excelService)
	{
		InitializeComponent();
		_excelService = excelService;
		ValveDataGrid.ItemsSource = _valveData;
		MatchResultGrid.ItemsSource = _matchResults;
	}

	/// <summary>
	/// 初始化窗口数据
	/// </summary>
	/// <param name="data">IO数据列表</param>
	public void InitializeData(List<IoFullData> data)
	{
		_originalData = data.ToList();
	}

	/// <summary>
	/// 获取更新后的数据
	/// </summary>
	/// <returns>已填充 DP阀位顺序字段的 IO 数据列表</returns>
	public List<IoFullData> GetSortedData()
	{
		return _originalData;
	}

	/// <summary>
	/// 下载模板按钮点击事件
	/// </summary>
	private async void DownloadTemplateButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = "Excel文件|*.xlsx",
				FileName = "阀门排序模板.xlsx",
				Title = "保存阀门排序模板"
			};
			if (saveFileDialog.ShowDialog() == true)
			{
				await CreateTemplate(saveFileDialog.FileName);
				StatusText.Text = "模板已保存到：" + saveFileDialog.FileName + "（包含3个示例Sheet）";
				System.Windows.MessageBox.Show("多Sheet模板下载成功！\n\n模板包含：\n- 阀门数据1：基础格式\n- 阀门数据2：多列格式\n- 混合格式数据：支持英文列名\n\n您可以在任意Sheet中填入阀门编号和气口顺序数据。", "提示", System.Windows.MessageBoxButton.OK, MessageBoxImage.Asterisk);
			}
		}
		catch (Exception ex)
		{
			System.Windows.MessageBox.Show("下载模板失败：" + ex.Message, "错误", System.Windows.MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	/// <summary>
	/// 选择文件按钮点击事件
	/// </summary>
	private async void SelectFileButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "Excel文件|*.xlsx;*.xls",
				Title = "选择阀门数据文件"
			};
			if (openFileDialog.ShowDialog() == true)
			{
				await LoadValveData(openFileDialog.FileName);
				StatusText.Text = $"已加载 {_valveData.Count} 条阀门数据（来自多个Sheet）";
				ProcessButton.IsEnabled = _valveData.Count > 0;
				ValveDataCountText.Text = $"阀门数据：{_valveData.Count}条";
			}
		}
		catch (Exception ex)
		{
			System.Windows.MessageBox.Show("加载多Sheet文件失败：" + ex.Message + "\n\n支持的格式：\n- 可以包含多个工作表\n- 每个工作表可以有不同的列结构\n- 系统会自动识别包含'阀门编号'和'气口顺序'的列", "错误", System.Windows.MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	/// <summary>
	/// 处理数据按钮点击事件
	/// </summary>
	private void ProcessButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			ProcessValveSorting();
			StatusText.Text = $"处理完成，匹配 {_matchResults.Count((MatchResult r) => r.MatchStatus == "匹配成功")} 条数据";
			ConfirmButton.IsEnabled = _matchResults.Any((MatchResult r) => r.MatchStatus == "匹配成功");
			DataTabControl.SelectedItem = MatchResultTab;
			int value = _matchResults.Count((MatchResult r) => r.MatchStatus == "匹配成功");
			int value2 = _matchResults.Count((MatchResult r) => r.MatchStatus != "匹配成功");
			MatchedCountText.Text = $"匹配成功：{value}条";
			UnmatchedCountText.Text = $"未匹配：{value2}条";
		}
		catch (Exception ex)
		{
			System.Windows.MessageBox.Show("处理数据失败：" + ex.Message, "错误", System.Windows.MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	/// <summary>
	/// 确认排序按钮点击事件
	/// </summary>
	private void ConfirmButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			int value = _matchResults.Count((MatchResult r) => r.MatchStatus == "匹配成功");
			int count = _originalData.Count;
			System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"确认要导入阀门顺序数据吗？\n\n匹配成功：{value} 条\n总信号数：{count} 条\n\n注意：将清空所有IO数据的DP阀位顺序字段，然后填入新数据！", "确认导入", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
			if (messageBoxResult == System.Windows.MessageBoxResult.Yes)
			{
				ApplySortingResults();
				base.DialogResult = true;
				Close();
			}
		}
		catch (Exception ex)
		{
			System.Windows.MessageBox.Show("应用排序失败：" + ex.Message, "错误", System.Windows.MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	/// <summary>
	/// 取消按钮点击事件
	/// </summary>
	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
		Close();
	}

	/// <summary>
	/// 创建Excel模板（支持多Sheet）
	/// </summary>
	private async Task CreateTemplate(string filePath)
	{
		_ = 2;
		try
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("阀门编号", typeof(string));
			dataTable.Columns.Add("气口顺序", typeof(string));
			dataTable.Rows.Add("XV001", "F1");
			dataTable.Rows.Add("XV-002", "F2");
			dataTable.Rows.Add("XV 003", "F3/F4");
			dataTable.Rows.Add("XV_004", "F9");
			await _excelService.FastExportSheetAsync(dataTable, filePath, "阀门数据1");
			DataTable dataTable2 = new DataTable();
			dataTable2.Columns.Add("阀门编号", typeof(string));
			dataTable2.Columns.Add("气口顺序", typeof(string));
			dataTable2.Columns.Add("备注", typeof(string));
			dataTable2.Rows.Add("XV 005", "F5", "包含空格的阀门编号");
			dataTable2.Rows.Add("XV-006", "F6", "包含短横线的阀门编号");
			dataTable2.Rows.Add("XV_007", "F7/F8", "包含下划线的阀门编号");
			await _excelService.FastExportSheetAsync(dataTable2, filePath, "阀门数据2");
			DataTable dataTable3 = new DataTable();
			dataTable3.Columns.Add("Valve Number", typeof(string));
			dataTable3.Columns.Add("Port Order", typeof(string));
			dataTable3.Columns.Add("其他信息", typeof(string));
			dataTable3.Rows.Add("XV-008", "F10", "英文列名+短横线");
			dataTable3.Rows.Add("XV 009", "F11", "英文列名+空格");
			await _excelService.FastExportSheetAsync(dataTable3, filePath, "混合格式数据");
		}
		catch (Exception ex)
		{
			throw new Exception("创建模板失败：" + ex.Message);
		}
	}

	/// <summary>
	/// 加载阀门数据（支持多个Sheet）
	/// </summary>
	private async Task LoadValveData(string filePath)
	{
		_valveData.Clear();
		try
		{
			List<string> list = await _excelService.GetSheetNames(filePath);
			if (list == null || list.Count == 0)
			{
				throw new Exception("Excel文件中没有找到工作表");
			}
			StatusText.Text = $"正在读取 {list.Count} 个工作表...";
			foreach (string item in list)
			{
				try
				{
					DataTable dataTable = await _excelService.GetDataTableAsStringAsync(filePath, item, hasHeader: true);
					if (dataTable == null || dataTable.Rows.Count == 0)
					{
						continue;
					}
					int num = -1;
					int num2 = -1;
					for (int i = 0; i < dataTable.Columns.Count; i++)
					{
						string text = dataTable.Columns[i].ColumnName?.Trim() ?? "";
						if (text.Contains("阀门编号") || text.Contains("阀门号") || text.ToLower().Contains("valve"))
						{
							num = i;
						}
						else if (text.Contains("气口顺序") || text.Contains("气口") || text.Contains("顺序") || text.ToLower().Contains("order"))
						{
							num2 = i;
						}
					}
					if (num < 0 || num2 < 0)
					{
						continue;
					}
					foreach (DataRow row in dataTable.Rows)
					{
						string valveNumber = row[num]?.ToString()?.Trim();
						string text2 = row[num2]?.ToString()?.Trim();
						if (!string.IsNullOrEmpty(valveNumber) && !string.IsNullOrEmpty(text2) && !_valveData.Any((ValveData v) => v.ValveNumber.Equals(valveNumber, StringComparison.OrdinalIgnoreCase)))
						{
							_valveData.Add(new ValveData
							{
								ValveNumber = valveNumber,
								AirPortOrder = text2
							});
						}
					}
				}
				catch (Exception)
				{
				}
			}
			if (_valveData.Count == 0)
			{
				throw new Exception("所有工作表中都没有找到有效的阀门编号和气口顺序数据。\n\n请确保Excel文件中至少有一个工作表包含以下列名：\n- 阀门编号（或包含'阀门'、'valve'关键字）\n- 气口顺序（或包含'气口'、'顺序'、'order'关键字）");
			}
			ValveDataGrid.Items.Refresh();
		}
		catch (Exception ex2)
		{
			throw new Exception("读取Excel文件失败：" + ex2.Message);
		}
	}

	/// <summary>
	/// 标准化信号位号，移除空格和短横线用于匹配
	/// </summary>
	private string NormalizeSignalNumber(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return string.Empty;
		}
		return input.Replace(" ", "").Replace("-", "").Replace("_", "")
			.ToUpperInvariant();
	}

	/// <summary>
	/// 处理导入阀门顺序逻辑
	/// 步骤：
	/// 1. 标准化阀门编号（移除空格、短横线、下划线）
	/// 2. 匹配 IO 数据中的信号位号
	/// 3. 生成匹配结果供预览
	/// 注意：此方法只进行匹配预览，不修改原始数据
	/// </summary>
	private void ProcessValveSorting()
	{
		_matchResults.Clear();
		foreach (ValveData valveDatum in _valveData)
		{
			string normalizedValveNumber = NormalizeSignalNumber(valveDatum.ValveNumber);
			IoFullData ioFullData = _originalData.FirstOrDefault(delegate(IoFullData io)
			{
				string text = NormalizeSignalNumber(io.SignalPositionNumber);
				return text.Equals(normalizedValveNumber, StringComparison.OrdinalIgnoreCase);
			});
			MatchResult item = new MatchResult
			{
				ValveNumber = valveDatum.ValveNumber,
				AirPortOrder = valveDatum.AirPortOrder,
				SignalPositionNumber = (ioFullData?.SignalPositionNumber ?? ""),
				Description = (ioFullData?.Description ?? ""),
				MatchStatus = ((ioFullData != null) ? "匹配成功" : "未找到匹配"),
				DPTerminalChannel = ((ioFullData != null) ? valveDatum.AirPortOrder : "")
			};
			_matchResults.Add(item);
		}
		MatchResultGrid.Items.Refresh();
	}

	/// <summary>
	/// 从字符串中提取数字值用于DP阀位顺序
	/// </summary>
	private int? ExtractNumericValue(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return null;
		}
		Match match = Regex.Match(input, "\\d+");
		if (match.Success && int.TryParse(match.Value, out var result))
		{
			return result;
		}
		return null;
	}

	/// <summary>
	/// 应用导入结果
	/// 步骤：
	/// 1. 清空所有 IO 数据的 DP阀位顺序字段
	/// 2. 将匹配成功的气口顺序写入对应的 IO 数据
	/// </summary>
	private void ApplySortingResults()
	{
		foreach (IoFullData originalDatum in _originalData)
		{
			originalDatum.DPTerminalChannel = string.Empty;
		}
		foreach (MatchResult item in _matchResults.Where((MatchResult r) => r.MatchStatus == "匹配成功"))
		{
			string normalizedValveNumber = NormalizeSignalNumber(item.ValveNumber);
			IoFullData ioFullData = _originalData.FirstOrDefault(delegate(IoFullData io)
			{
				string text = NormalizeSignalNumber(io.SignalPositionNumber);
				return text.Equals(normalizedValveNumber, StringComparison.OrdinalIgnoreCase);
			});
			if (ioFullData != null)
			{
				ioFullData.DPTerminalChannel = item.AirPortOrder;
			}
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/windows/valvesortingwindow.xaml", UriKind.Relative);
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
			DownloadTemplateButton = (Wpf.Ui.Controls.Button)target;
			DownloadTemplateButton.Click += DownloadTemplateButton_Click;
			break;
		case 2:
			SelectFileButton = (Wpf.Ui.Controls.Button)target;
			SelectFileButton.Click += SelectFileButton_Click;
			break;
		case 3:
			ProcessButton = (Wpf.Ui.Controls.Button)target;
			ProcessButton.Click += ProcessButton_Click;
			break;
		case 4:
			StatusText = (Wpf.Ui.Controls.TextBlock)target;
			break;
		case 5:
			DataTabControl = (TabControl)target;
			break;
		case 6:
			ValveDataTab = (TabItem)target;
			break;
		case 7:
			ValveDataCountText = (Wpf.Ui.Controls.TextBlock)target;
			break;
		case 8:
			ValveDataGrid = (System.Windows.Controls.DataGrid)target;
			break;
		case 9:
			MatchResultTab = (TabItem)target;
			break;
		case 10:
			MatchedCountText = (Wpf.Ui.Controls.TextBlock)target;
			break;
		case 11:
			UnmatchedCountText = (Wpf.Ui.Controls.TextBlock)target;
			break;
		case 12:
			MatchResultGrid = (System.Windows.Controls.DataGrid)target;
			break;
		case 13:
			CancelButton = (Wpf.Ui.Controls.Button)target;
			CancelButton.Click += CancelButton_Click;
			break;
		case 14:
			ConfirmButton = (Wpf.Ui.Controls.Button)target;
			ConfirmButton.Click += ConfirmButton_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
