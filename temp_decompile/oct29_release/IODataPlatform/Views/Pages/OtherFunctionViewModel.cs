using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Aspose.Cells;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels.OtherFunction;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.Common;
using IODataPlatform.Views.SubPages.OtherFunction;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 其他功能页面视图模型类
/// 提供系统的扩展功能和工具集合，包括网表生成、数据处理等专业工具
/// 作为功能扩展的统一入口，支持各种辅助性业务操作和数据处理任务
/// 实现INavigationAware接口以支持页面导航和状态管理
/// </summary>
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class OtherFunctionViewModel : ObservableObject, INavigationAware
{
	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.OtherFunctionViewModel.ComparePantaiPropertiesCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? comparePantaiPropertiesCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.OtherFunctionViewModel.GenerateNetListCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? generateNetListCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.OtherFunctionViewModel.GenericDataComparisonCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? genericDataComparisonCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.OtherFunctionViewModel.AddBackUpTagCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? addBackUpTagCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.OtherFunctionViewModel.GenerateBitListCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? generateBitListCommand;

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.OtherFunctionViewModel.ComparePantaiProperties" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ComparePantaiPropertiesCommand => comparePantaiPropertiesCommand ?? (comparePantaiPropertiesCommand = new RelayCommand(ComparePantaiProperties));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.OtherFunctionViewModel.GenerateNetList" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand GenerateNetListCommand => generateNetListCommand ?? (generateNetListCommand = new RelayCommand(GenerateNetList));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.OtherFunctionViewModel.GenericDataComparison" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand GenericDataComparisonCommand => genericDataComparisonCommand ?? (genericDataComparisonCommand = new RelayCommand(GenericDataComparison));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.OtherFunctionViewModel.AddBackUpTag" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand AddBackUpTagCommand => addBackUpTagCommand ?? (addBackUpTagCommand = new RelayCommand(AddBackUpTag));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.OtherFunctionViewModel.GenerateBitList" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand GenerateBitListCommand => generateBitListCommand ?? (generateBitListCommand = new RelayCommand(GenerateBitList));

	[RelayCommand]
	public void ComparePantaiProperties()
	{
		string text = _003Cpicker_003EP.OpenFile("请选择设计院的设备清单(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx");
		if (text == null || string.IsNullOrEmpty(text))
		{
			return;
		}
		string text2 = _003Cpicker_003EP.OpenFile("请选择我的设备清单(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx");
		if (text2 == null || string.IsNullOrEmpty(text2))
		{
			return;
		}
		List<设备清单> list = new List<设备清单>();
		Workbook workbook = new Workbook(text);
		using (IEnumerator<Worksheet> enumerator = workbook.Worksheets.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				Worksheet current = enumerator.Current;
				for (int i = 1; i <= current.Cells.MaxDataRow; i++)
				{
					list.Add(new 设备清单
					{
						PKS = current.Cells[i, 5]?.StringValue,
						Prop1 = current.Cells[i, 14]?.StringValue,
						Prop2 = current.Cells[i, 15]?.StringValue,
						Prop3 = current.Cells[i, 16]?.StringValue
					});
				}
			}
		}
		List<设备清单> list2 = new List<设备清单>();
		Workbook workbook2 = new Workbook(text2);
		using (IEnumerator<Worksheet> enumerator2 = workbook2.Worksheets.GetEnumerator())
		{
			if (enumerator2.MoveNext())
			{
				Worksheet current2 = enumerator2.Current;
				for (int j = 1; j <= current2.Cells.MaxDataRow; j++)
				{
					list2.Add(new 设备清单
					{
						PKS = current2.Cells[j, 5]?.StringValue,
						Prop1 = current2.Cells[j, 10]?.StringValue,
						Prop2 = current2.Cells[j, 11]?.StringValue,
						Prop3 = current2.Cells[j, 12]?.StringValue
					});
				}
			}
		}
		foreach (设备清单 equipment in list)
		{
			设备清单 设备清单2 = list2.FirstOrDefault((设备清单 e) => e.PKS == equipment.PKS);
			if (设备清单2 != null)
			{
				if (equipment.Prop1 != 设备清单2.Prop1 || equipment.Prop2 != 设备清单2.Prop2 || equipment.Prop3 != 设备清单2.Prop3)
				{
					HighlightDifferences(设备清单2, equipment, workbook2);
				}
				continue;
			}
			设备清单2 = list2.FirstOrDefault((设备清单 e) => e.PKS == equipment.Prop1);
			if (设备清单2 != null)
			{
				if (equipment.Prop2 != 设备清单2.Prop2 || equipment.Prop3 != 设备清单2.Prop3)
				{
					HighlightPartialMatchDifferences(设备清单2, equipment, workbook2);
				}
			}
			else
			{
				HighlightNoMatch(equipment, workbook2);
			}
		}
		workbook2.Save(text2);
	}

	private void HighlightDifferences(设备清单 user, 设备清单 design, Workbook workbook)
	{
		foreach (Worksheet worksheet in workbook.Worksheets)
		{
			for (int i = 1; i <= worksheet.Cells.MaxDataRow; i++)
			{
				if (worksheet.Cells[i, 5]?.StringValue == user.PKS)
				{
					if (user.Prop1 != design.Prop1)
					{
						SetCellColor(worksheet.Cells[i, 10], Color.Red);
					}
					if (user.Prop2 != design.Prop2)
					{
						SetCellColor(worksheet.Cells[i, 11], Color.Red);
					}
					if (user.Prop3 != design.Prop3)
					{
						SetCellColor(worksheet.Cells[i, 12], Color.Red);
					}
				}
			}
		}
	}

	private void HighlightPartialMatchDifferences(设备清单 user, 设备清单 design, Workbook workbook)
	{
		foreach (Worksheet worksheet in workbook.Worksheets)
		{
			for (int i = 1; i <= worksheet.Cells.MaxDataRow; i++)
			{
				if (worksheet.Cells[i, 5]?.StringValue == user.PKS)
				{
					if (user.Prop2 != design.Prop2)
					{
						SetCellColor(worksheet.Cells[i, 11], Color.Red);
					}
					if (user.Prop3 != design.Prop3)
					{
						SetCellColor(worksheet.Cells[i, 12], Color.Red);
					}
				}
			}
		}
	}

	private void HighlightNoMatch(设备清单 user, Workbook workbook)
	{
		foreach (Worksheet worksheet in workbook.Worksheets)
		{
			for (int i = 1; i <= worksheet.Cells.MaxDataRow; i++)
			{
				if (worksheet.Cells[i, 5]?.StringValue == user.PKS)
				{
					SetCellColor(worksheet.Cells[i, 5], Color.Red);
					SetCellColor(worksheet.Cells[i, 10], Color.Red);
					SetCellColor(worksheet.Cells[i, 11], Color.Red);
					SetCellColor(worksheet.Cells[i, 12], Color.Red);
				}
			}
		}
	}

	private void SetCellColor(Cell cell, Color color)
	{
		Style style = cell.GetStyle();
		style.ForegroundColor = color;
		style.BackgroundColor = color;
		style.Pattern = BackgroundType.Solid;
		cell.SetStyle(style);
	}

	/// <summary>
	/// 其他功能页面视图模型类
	/// 提供系统的扩展功能和工具集合，包括网表生成、数据处理等专业工具
	/// 作为功能扩展的统一入口，支持各种辅助性业务操作和数据处理任务
	/// 实现INavigationAware接口以支持页面导航和状态管理
	/// </summary>
	public OtherFunctionViewModel(SqlSugarContext context, INavigationService navigation, GlobalModel model, IPickerService picker, ExcelService excel)
	{
		_003Cnavigation_003EP = navigation;
		_003Cmodel_003EP = model;
		_003Cpicker_003EP = picker;
		_003Cexcel_003EP = excel;
		base._002Ector();
	}

	/// <summary>
	/// 页面导航离开时触发
	/// 当前实现为空，预留用于后续状态清理或数据保存操作
	/// </summary>
	public void OnNavigatedFrom()
	{
	}

	/// <summary>
	/// 页面导航到此页面时触发
	/// 当前实现为空，预留用于后续初始化或数据加载操作
	/// </summary>
	public async void OnNavigatedTo()
	{
	}

	/// <summary>
	/// 生成网表命令
	/// 导航到Excel数据提取转网表页面，用于将Excel格式的工程数据转换为标准网表格式
	/// 广泛用于电气设计和仿真软件的数据交换，支持CAD工具链的数据传输
	/// </summary>
	[RelayCommand]
	private void GenerateNetList()
	{
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(ExtractExcelToNetListPage));
	}

	/// <summary>
	/// 通用数据对比命令
	/// 导航到通用数据对比页面，用于对比任意两个Excel数据文件的差异
	/// 支持指定主键字段进行精确对比，提供新增、删除、修改三种变更类型的识别
	/// </summary>
	[RelayCommand]
	private void GenericDataComparison()
	{
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(GenericDataComparisonPage));
	}

	[RelayCommand]
	public async void AddBackUpTag()
	{
		string selectedFilePath = _003Cpicker_003EP.OpenFile("中控IO分站清单(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx");
		if (selectedFilePath == null || string.IsNullOrEmpty(selectedFilePath))
		{
			return;
		}
		_003Cmodel_003EP.Status.Busy("正在识别IO分站清单……");
		using Workbook workbook = await Task.Run(() => _003Cexcel_003EP.GetWorkbook(selectedFilePath));
		Worksheet worksheet = workbook.Worksheets.FirstOrDefault((Worksheet w) => w.Name.Contains("IO分站清单"));
		if (worksheet == null)
		{
			throw new Exception(selectedFilePath + "中不存在IO分站清单Sheet");
		}
		_003Cmodel_003EP.Status.Busy("正在生成备用点……");
		ExportTableOptions exportOptions = new ExportTableOptions
		{
			ExportColumnName = true,
			CheckMixedValueType = true
		};
		DataTable dt = await Task.Run(() => worksheet.Cells.ExportDataTable(0, 0, worksheet.Cells.MaxDataRow + 1, worksheet.Cells.MaxDataColumn + 1, exportOptions));
		List<IO分站清单_中控> list = (await Task.Run(() => dt.StringTableToIEnumerableByDiplay<IO分站清单_中控>().ToList())).Where((IO分站清单_中控 i) => !i.CardType.Contains("COM") && !i.CardType.Contains("AM")).ToList();
		dt.Clear();
		IEnumerable<IGrouping<string, IO分站清单_中控>> enumerable = from io in list
			group io by io.CabinetNumber;
		foreach (IGrouping<string, IO分站清单_中控> item in enumerable)
		{
			string key = item.Key;
			IEnumerable<IGrouping<int, IO分站清单_中控>> enumerable2 = from iO分站清单_中控3 in item
				group iO分站清单_中控3 by iO分站清单_中控3.Rack;
			foreach (IGrouping<int, IO分站清单_中控> item2 in enumerable2)
			{
				int key2 = item2.Key;
				IEnumerable<IGrouping<int, IO分站清单_中控>> enumerable3 = from r in item2
					group r by r.Slot;
				foreach (IGrouping<int, IO分站清单_中控> item3 in enumerable3)
				{
					int key3 = item3.Key;
					IO分站清单_中控 iO分站清单_中控 = item3.First();
					string iOType = iO分站清单_中控.IOType;
					if (iOType == null)
					{
						throw new Exception(iO分站清单_中控.TagName + " IO类型为空，请增加后再重新生成！");
					}
					string systemCode = iO分站清单_中控.SystemCode;
					string cardNumber = iO分站清单_中控.CardNumber;
					string signalCharacteristic = iO分站清单_中控.SignalCharacteristic;
					string powerSupplyType = iO分站清单_中控.PowerSupplyType;
					int num = iOType[0] switch
					{
						'A' => 8, 
						'D' => 16, 
						'P' => 6, 
						_ => 0, 
					};
					string measurementUnit = iOType[0] switch
					{
						'A' => "%", 
						'P' => "Hz", 
						_ => "", 
					};
					char c = iOType[0];
					string text = ((c != 'A' && c != 'P') ? "" : "0.00");
					string rangeLowerLimit = text;
					double rangeUpperLimit = iOType[0] switch
					{
						'A' => 100.0, 
						'P' => 1000.0, 
						_ => 0.0, 
					};
					List<int> list2 = item3.Select((IO分站清单_中控 st) => st.ChannelOrFFSegment).ToList();
					for (int num2 = 1; num2 <= num; num2++)
					{
						if (!list2.Contains(num2))
						{
							IO分站清单_中控 iO分站清单_中控2 = new IO分站清单_中控
							{
								SequenceNumber = item3.Max((IO分站清单_中控 s) => s.SequenceNumber) + 1,
								SignalTag = $"{cardNumber}CH{num2:D2}",
								SignalFunction = "备用",
								CardType = item3.FirstOrDefault((IO分站清单_中控 s) => s.CardType != null)?.CardType,
								SystemCode = systemCode,
								CabinetNumber = key,
								Rack = key2,
								Slot = key3,
								ChannelOrFFSegment = num2,
								IOType = iOType,
								SignalCharacteristic = signalCharacteristic,
								PowerSupplyType = powerSupplyType,
								MeasurementUnit = measurementUnit,
								RangeLowerLimit = rangeLowerLimit,
								RangeUpperLimit = rangeUpperLimit,
								ChannelAddress = $"000-{key2 - 1:D3}-{key3 - 1:D3}-{num2 - 1:D3}"
							};
							list.Add(iO分站清单_中控2);
							DataRow dataRow = dt.NewRow();
							dataRow["序号"] = iO分站清单_中控2.SequenceNumber;
							dataRow["信号位号"] = iO分站清单_中控2.SignalTag;
							dataRow["信号功能"] = iO分站清单_中控2.SignalFunction;
							dataRow["板卡类型"] = iO分站清单_中控2.CardType;
							dataRow["系统代码"] = iO分站清单_中控2.SystemCode;
							dataRow["机柜号"] = iO分站清单_中控2.CabinetNumber;
							dataRow["机架"] = iO分站清单_中控2.Rack;
							dataRow["插槽"] = iO分站清单_中控2.Slot;
							dataRow["通道/FF网段"] = iO分站清单_中控2.ChannelOrFFSegment;
							dataRow["IO类型"] = iO分站清单_中控2.IOType;
							dataRow["信号特性"] = iO分站清单_中控2.SignalCharacteristic;
							dataRow["供电类型"] = iO分站清单_中控2.PowerSupplyType;
							dataRow["测量单位"] = iO分站清单_中控2.MeasurementUnit;
							dataRow["量程下限"] = iO分站清单_中控2.RangeLowerLimit;
							dataRow["量程上限"] = iO分站清单_中控2.RangeUpperLimit;
							dataRow["通道地址"] = iO分站清单_中控2.ChannelAddress;
							dt.Rows.Add(dataRow);
						}
					}
				}
			}
		}
		_003Cmodel_003EP.Status.Busy("正在将备用点添加至表格……");
		int maxDataRow = worksheet.Cells.MaxDataRow;
		_ = maxDataRow + dt.Rows.Count;
		worksheet.Cells.ImportDataTable(dt, isFieldNameShown: false, maxDataRow + 1, 0, insertRows: true);
		workbook.Save(selectedFilePath);
		_003Cmodel_003EP.Status.Success($"添加完毕,共添加{dt.Rows.Count}条，从{maxDataRow + 2}开始。");
	}

	[RelayCommand]
	public async void GenerateBitList()
	{
		string selectedFilePath = _003Cpicker_003EP.OpenFile("请选择要提取的IO分站清单(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx");
		if (selectedFilePath == null || string.IsNullOrEmpty(selectedFilePath))
		{
			return;
		}
		string folder = _003Cpicker_003EP.PickFolder();
		if (folder == null)
		{
			return;
		}
		List<板卡清单_生成位号表> dtCards = new List<板卡清单_生成位号表>();
		List<IO分站清单_中控> dtIOStations = new List<IO分站清单_中控>();
		_003Cmodel_003EP.Status.Busy("正在读取数据...");
		await Task.Run(delegate
		{
			Workbook workbook = _003Cexcel_003EP.GetWorkbook(selectedFilePath);
			ExportTableOptions exportOptions = new ExportTableOptions
			{
				ExportColumnName = true,
				CheckMixedValueType = true
			};
			workbook.Worksheets.ToList().ForEach(delegate(Worksheet ws)
			{
				DataTable table = ws.Cells.ExportDataTable(0, 0, ws.Cells.MaxDataRow + 1, ws.Cells.MaxDataColumn + 1, exportOptions);
				if (ws.Name.Contains("板卡清单"))
				{
					dtCards.AddRange(table.StringTableToIEnumerableByDiplay<板卡清单_生成位号表>());
				}
				else if (ws.Name.Contains("IO分站清单"))
				{
					dtIOStations.AddRange(table.StringTableToIEnumerableByDiplay<IO分站清单_中控>());
				}
			});
		});
		_003Cmodel_003EP.Status.Busy("正在生成清单...");
		dtCards = dtCards.Where((板卡清单_生成位号表 d) => !d.CardType.StartsWith("COM")).ToList();
		if (dtIOStations.Where((IO分站清单_中控 c) => c.CardType == null).Count() > 0)
		{
			throw new Exception("Io分站清单每一行的板卡类型都不能为空");
		}
		IEnumerable<IGrouping<string, IO分站清单_中控>> enumerable = from io in dtIOStations
			where !io.CardType.StartsWith("AM") && !io.CardType.StartsWith("COM")
			group io by io.CabinetNumber;
		foreach (IGrouping<string, IO分站清单_中控> item in enumerable)
		{
			string cabinetNumber = item.Key;
			List<IO分站清单_中控> ioStations = item.ToList();
			List<板卡清单_生成位号表> cabinetCards = dtCards.Where((板卡清单_生成位号表 d) => d.CabinetNumber == cabinetNumber).ToList();
			string destIO = Path.Combine(folder, cabinetNumber + "io.xls");
			string destHardWare = Path.Combine(folder, cabinetNumber + "hardware.xls");
			DataTable dataTableAI = await GenerateAI(ioStations.Where((IO分站清单_中控 i) => i.IOType == "AI" || i.IOType == "PI" || i.IOType == "P").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable dataTableAO = await GenerateAO(ioStations.Where((IO分站清单_中控 i) => i.IOType == "AO").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable dataTableDI = await GenerateDI(ioStations.Where((IO分站清单_中控 i) => i.IOType == "DI").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable dataTableDO = await GenerateDO(ioStations.Where((IO分站清单_中控 i) => i.IOType == "DO").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable dataTableNA = await GenerateNA(ioStations.Where((IO分站清单_中控 i) => i.IOType == "NA").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable dataTableND = await GenerateND(ioStations.Where((IO分站清单_中控 i) => i.IOType == "ND").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable dataTableNN = await GenerateNN(ioStations.Where((IO分站清单_中控 i) => i.IOType == "NN").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable dataTablePA = await GeneratePA(ioStations.Where((IO分站清单_中控 i) => i.IOType == "PA").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable dataTablePD = await GeneratePD(ioStations.Where((IO分站清单_中控 i) => i.IOType == "PD").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable dataTablePN = await GeneratePN(ioStations.Where((IO分站清单_中控 i) => i.IOType == "PN").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable dataTablePB = await GenerateFB(ioStations.Where((IO分站清单_中控 i) => i.IOType == "PB").ToList(), cabinetCards).ToTableByDisplayAttributeAsync();
			if (File.Exists(destIO))
			{
				File.Delete(destIO);
			}
			await _003Cexcel_003EP.FastExportSheetAsync(dataTableAI, destIO, "AI", 3);
			await _003Cexcel_003EP.FastExportSheetAsync(dataTableAO, destIO, "AO", 3);
			await _003Cexcel_003EP.FastExportSheetAsync(dataTableDI, destIO, "DI", 3);
			await _003Cexcel_003EP.FastExportSheetAsync(dataTableDO, destIO, "DO", 3);
			await _003Cexcel_003EP.FastExportSheetAsync(dataTableNA, destIO, "NA", 3);
			await _003Cexcel_003EP.FastExportSheetAsync(dataTableND, destIO, "ND", 3);
			await _003Cexcel_003EP.FastExportSheetAsync(dataTableNN, destIO, "NN", 3);
			await _003Cexcel_003EP.FastExportSheetAsync(dataTablePA, destIO, "PA", 3);
			await _003Cexcel_003EP.FastExportSheetAsync(dataTablePD, destIO, "PD", 3);
			await _003Cexcel_003EP.FastExportSheetAsync(dataTablePN, destIO, "PN", 3);
			await _003Cexcel_003EP.FastExportSheetAsync(dataTablePB, destIO, "PB", 3);
			if (File.Exists(destHardWare))
			{
				File.Delete(destHardWare);
			}
			DataTable dataTableControl = await GenerateControl(cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable datatableIOLinkModel = await GenerateIOLinkModel(cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable datatableRack = await GenerateIORack(cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable datatableIO = await GenerateIO(ioStations, cabinetCards).ToTableByDisplayAttributeAsync();
			DataTable datatableChannel = await GenerateChannel(ioStations, cabinetCards).ToTableByDisplayAttributeAsync();
			await _003Cexcel_003EP.FastExportSheetAsync(dataTableControl, destHardWare, "Control");
			await _003Cexcel_003EP.FastExportSheetAsync(datatableIOLinkModel, destHardWare, "IO Link Module");
			await _003Cexcel_003EP.FastExportSheetAsync(datatableRack, destHardWare, "IO Rack");
			await _003Cexcel_003EP.FastExportSheetAsync(datatableIO, destHardWare, "IO");
			await _003Cexcel_003EP.FastExportSheetAsync(datatableChannel, destHardWare, "Channel");
		}
		_003Cmodel_003EP.Status.Success("生成成功到" + folder + "！");
	}

	private List<AI_生成位号表> GenerateAI(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
	{
		List<AI_生成位号表> aiList = new List<AI_生成位号表>();
		int sequenceNumber = 1;
		ioStations.ForEach(delegate(IO分站清单_中控 io)
		{
			string normalizedSlot = io.Slot.ToString().PadLeft(2, '0');
			string controlStationAddress = cardList.FirstOrDefault((板卡清单_生成位号表 c) => c.SlotNumber.ToString().PadLeft(2, '0') == normalizedSlot)?.ControllerAddress;
			double.TryParse(io.RangeLowerLimit, out var result);
			AI_生成位号表 item = new AI_生成位号表
			{
				SequenceNumber = sequenceNumber++.ToString(),
				Name = io.TagName,
				Description = io.SignalFunction,
				ControlStationAddress = controlStationAddress,
				Address = io.ChannelAddress,
				RangeLowerLimit = Math.Round(result, 3),
				RangeUpperLimit = Math.Round(io.RangeUpperLimit, 3),
				Unit = io.MeasurementUnit,
				TagType = "常规AI位号",
				ModuleType = GetModuleType(io.CardType),
				SignalType = GetSignalType(io.CardType),
				TagOperatingCycle = "基本扫描周期",
				DataType = "2字节整数(有符号)",
				SignalNature = "工程量",
				StatusCodePosition = "状态码在前",
				DataFormat = "不转换",
				ConversionType = GetDataFormat(io.CardType),
				LinearSquareRoot = "不开方",
				SmallSignal = "不切除",
				SmallSignalCutoffValue = 0.5,
				FilterTimeConstant = 0.0,
				ExtendedRangeUpperPercentage = 10.0,
				ExtendedRangeLowerPercentage = 10.0,
				OverrangeUpperAlarm = "使能",
				OverrangeLowerAlarm = "使能",
				InputRawCodeUpperLimit = 100.0,
				InputRawCodeLowerLimit = 0.0,
				HighTripLimitAlarm = "禁止",
				HighTripLimitAlarmLevel = "低",
				HighTripLimitAlarmValue = 100.0,
				HighHighLimitAlarm = "禁止",
				HighHighLimitAlarmLevel = "低",
				HighHighLimitAlarmValue = 95.0,
				HighLimitAlarm = "禁止",
				HighLimitAlarmLevel = "低",
				HighLimitAlarmValue = 90.0,
				LowLimitAlarm = "禁止",
				LowLimitAlarmLevel = "低",
				LowLimitAlarmValue = 10.0,
				LowLowLimitAlarm = "禁止",
				LowLowLimitAlarmLevel = "低",
				LowLowLimitAlarmValue = 5.0,
				LowTripLimitAlarm = "禁止",
				LowTripLimitAlarmLevel = "低",
				LowTripLimitAlarmValue = 0.0,
				HighLowLimitAlarmHysteresisValue = 0.5,
				RateOfChangeAlarm = "禁止",
				RateOfChangeAlarmLevel = "低",
				RateOfChangeAlarmValue = 100.0,
				FaultAlarm = "使能",
				FaultAlarmLevel = "低",
				FaultHandling = "保持",
				TagGroup = "位号分组 0",
				TagLevel = "0级",
				DecimalPlaces = "3"
			};
			aiList.Add(item);
		});
		return aiList;
	}

	private List<AO_生成位号表> GenerateAO(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
	{
		List<AO_生成位号表> list = new List<AO_生成位号表>();
		int num = 1;
		foreach (IO分站清单_中控 io in ioStations)
		{
			string controlStationAddress = cardList.FirstOrDefault((板卡清单_生成位号表 c) => c.CardNumber == io.CardNumber)?.ControllerAddress;
			string cardType = io.CardType;
			string text = ((cardType == "AO711-S") ? "电流信号输出模块（8路）" : ((!(cardType == "AO711-H")) ? "未知模块类型" : "电流信号输出模块（8路 HART）"));
			string text2 = text;
			text2 = io.CardType + " " + text2;
			double.TryParse(io.RangeLowerLimit, out var result);
			AO_生成位号表 item = new AO_生成位号表
			{
				SequenceNumber = num++.ToString(),
				Name = io.TagName,
				Description = io.SignalFunction,
				ControlStationAddress = controlStationAddress,
				Address = io.ChannelAddress,
				RangeLowerLimit = result,
				RangeUpperLimit = io.RangeUpperLimit,
				Unit = io.MeasurementUnit,
				TagType = "常规AO位号",
				ModuleType = text2,
				SignalType = "电流(4mA～20mA)",
				FaultSafetyMode = "输出保持",
				FaultStateSetValue = 0.0,
				TagOperatingCycle = "基本扫描周期",
				DataType = "2字节整数(无符号)",
				SignalNature = "工程量",
				StatusCodePosition = "状态码在前",
				DataFormat = "不转换",
				ConversionType = "线性转换",
				PositiveNegativeOutput = "正输出",
				ExtendedRangeUpperPercentage = 0.0,
				ExtendedRangeLowerPercentage = 0.0,
				OverrangeUpperAlarm = "禁止",
				OverrangeLowerAlarm = "禁止",
				OutputRawCodeUpperLimit = 100.0,
				OutputRawCodeLowerLimit = 0.0,
				OutputHighLimitClippingAlarm = "禁止",
				OutputHighLimitClippingAlarmLevel = "低",
				OutputHighLimitClippingValue = 50.0,
				OutputLowLimitClippingAlarm = "禁止",
				OutputLowLimitClippingAlarmLevel = "低",
				OutputLowLimitClippingValue = 0.0,
				FaultAlarm = "使能",
				FaultAlarmLevel = "低",
				ConfigurationErrorAlarm = "使能",
				ConfigurationErrorAlarmLevel = "低",
				TagGroup = "位号分组 0",
				TagLevel = "0级",
				DecimalPlaces = "3"
			};
			list.Add(item);
		}
		return list;
	}

	private List<DI_生成位号表> GenerateDI(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
	{
		List<DI_生成位号表> list = new List<DI_生成位号表>();
		int num = 1;
		foreach (IO分站清单_中控 io in ioStations)
		{
			string controlStationAddress = cardList.FirstOrDefault((板卡清单_生成位号表 c) => c.CardNumber == io.CardNumber)?.ControllerAddress;
			DI_生成位号表 item = new DI_生成位号表
			{
				SequenceNumber = num++.ToString(),
				Name = io.TagName,
				Description = io.SignalFunction,
				ControlStationAddress = controlStationAddress,
				Address = io.ChannelAddress,
				OnDescription = "ON",
				OffDescription = "OFF",
				TagType = "常规DI位号",
				ModuleType = "DI711-S 数字信号输入模块（16路，24V）",
				TagOperatingCycle = "基本扫描周期",
				InputReverse = "禁止",
				OnStateAlarm = "禁止",
				OnStateAlarmLevel = "低",
				OffStateAlarm = "禁止",
				OffStateAlarmLevel = "低",
				PositiveJumpAlarm = "禁止",
				PositiveJumpAlarmLevel = "低",
				NegativeJumpAlarm = "禁止",
				NegativeJumpAlarmLevel = "低",
				FaultAlarm = "使能",
				FaultAlarmLevel = "低",
				FaultHandling = "保持",
				TagGroup = "位号分组 0",
				TagLevel = "0级",
				SoeHardPointMarker = "0",
				SoeFlag = "否",
				SoeDescription = "",
				SoeDeviceGroup = ""
			};
			list.Add(item);
		}
		return list;
	}

	private List<DO_生成位号表> GenerateDO(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
	{
		List<DO_生成位号表> list = new List<DO_生成位号表>();
		int num = 1;
		foreach (IO分站清单_中控 io in ioStations)
		{
			string controlStationAddress = cardList.FirstOrDefault((板卡清单_生成位号表 c) => c.CardNumber == io.CardNumber)?.ControllerAddress;
			DO_生成位号表 item = new DO_生成位号表
			{
				SequenceNumber = num++.ToString(),
				Name = io.TagName,
				Description = io.SignalFunction,
				ControlStationAddress = controlStationAddress,
				Address = io.ChannelAddress,
				OnDescription = "ON",
				OffDescription = "OFF",
				TagType = "常规DO位号",
				ModuleType = "DO711-S 数字信号输出模块（16路）",
				FaultSafetyMode = "输出保持",
				FaultStateSetting = "OFF",
				TagOperatingCycle = "基本扫描周期",
				OutputReverse = "禁止",
				OnStateAlarm = "禁止",
				OnStateAlarmLevel = "低",
				OffStateAlarm = "禁止",
				OffStateAlarmLevel = "低",
				FaultAlarm = "使能",
				FaultAlarmLevel = "低",
				TagGroup = "位号分组 0",
				TagLevel = "0级",
				SoeFlag = "否",
				SoeDescription = string.Empty,
				SoeDeviceGroup = string.Empty
			};
			list.Add(item);
		}
		return list;
	}

	private List<NA_生成位号表> GenerateNA(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
	{
		return new List<NA_生成位号表>();
	}

	private List<ND_生成位号表> GenerateND(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
	{
		return new List<ND_生成位号表>();
	}

	private List<NN_生成位号表> GenerateNN(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
	{
		return new List<NN_生成位号表>();
	}

	private List<PA_生成位号表> GeneratePA(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
	{
		return new List<PA_生成位号表>();
	}

	private List<PD_生成位号表> GeneratePD(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
	{
		return new List<PD_生成位号表>();
	}

	private List<PN_生成位号表> GeneratePN(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
	{
		return new List<PN_生成位号表>();
	}

	private List<FB_生成位号表> GenerateFB(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
	{
		return new List<FB_生成位号表>();
	}

	private List<Control_生成位号表> GenerateControl(List<板卡清单_生成位号表> cardList)
	{
		if (cardList.Count == 0)
		{
			return new List<Control_生成位号表>();
		}
		int num = 1;
		List<Control_生成位号表> list = new List<Control_生成位号表>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<Control_生成位号表> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = new Control_生成位号表
		{
			地址 = cardList.FirstOrDefault().ControllerAddress,
			型号 = "FCU712-S",
			冗余 = "1"
		};
		return list;
	}

	private List<IO_Link_Module_生成位号表> GenerateIOLinkModel(List<板卡清单_生成位号表> cardList)
	{
		if (cardList.Count == 0)
		{
			return new List<IO_Link_Module_生成位号表>();
		}
		int num = 1;
		List<IO_Link_Module_生成位号表> list = new List<IO_Link_Module_生成位号表>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<IO_Link_Module_生成位号表> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = new IO_Link_Module_生成位号表
		{
			控制器地址 = cardList.FirstOrDefault().ControllerAddress,
			地址 = "0",
			型号 = "COM701-S",
			冗余 = "0"
		};
		return list;
	}

	private List<IO_Rack_生成位号表> GenerateIORack(List<板卡清单_生成位号表> cardList)
	{
		if (cardList.Count == 0)
		{
			return new List<IO_Rack_生成位号表>();
		}
		List<IO_Rack_生成位号表> list = new List<IO_Rack_生成位号表>();
		IEnumerable<IGrouping<string, 板卡清单_生成位号表>> enumerable = from c in cardList
			group c by c.RackNumber;
		foreach (IGrouping<string, 板卡清单_生成位号表> item in enumerable)
		{
			List<板卡清单_生成位号表> source = item.ToList();
			if (!source.All((板卡清单_生成位号表 c) => c.CardType == "备用"))
			{
				string text = (int.Parse(source.FirstOrDefault()?.RackNumber.TrimStart('0') ?? "0") - 1).ToString();
				list.Add(new IO_Rack_生成位号表
				{
					控制器地址 = source.FirstOrDefault().ControllerAddress,
					io连接模块地址 = "0",
					地址 = text,
					型号 = ((text == "0" || text == "1") ? "CN721" : "CN722")
				});
			}
		}
		return list;
	}

	private List<IO_生成位号表> GenerateIO(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
	{
		if (cardList.Count == 0)
		{
			return new List<IO_生成位号表>();
		}
		List<IO_生成位号表> list = new List<IO_生成位号表>();
		IEnumerable<IGrouping<string, 板卡清单_生成位号表>> enumerable = from c in cardList
			group c by c.RackNumber;
		foreach (IGrouping<string, 板卡清单_生成位号表> item in enumerable)
		{
			List<板卡清单_生成位号表> list2 = item.ToList();
			foreach (板卡清单_生成位号表 item2 in list2)
			{
				if (item2.CardType == "备用" || item2.CardType == "空" || string.IsNullOrEmpty(item2.CardType))
				{
					continue;
				}
				int rackAddress = int.Parse(item2.RackNumber.TrimStart('0'));
				int slotAddress = int.Parse(item2.SlotNumber.TrimStart('0'));
				if (!item2.CardType.StartsWith("AM") || (slotAddress - 1) % 2 == 0)
				{
					IO分站清单_中控 iO分站清单_中控 = ioStations.FirstOrDefault((IO分站清单_中控 i) => i.Rack == rackAddress && i.Slot == slotAddress);
					list.Add(new IO_生成位号表
					{
						控制器地址 = item2.ControllerAddress,
						io连接模块地址 = "0",
						机架地址 = (rackAddress - 1).ToString(),
						地址 = (slotAddress - 1).ToString(),
						型号 = item2.CardType,
						描述 = (iO分站清单_中控?.CardCabinetNumber ?? ""),
						备注 = "",
						冗余 = (item2.CardType.Contains("AM712") ? "1" : "0"),
						采样周期 = "0",
						信号类型配置 = "0",
						冷端补偿 = "0",
						抖动参数 = ((item2.CardType == "DI711-S") ? "1" : "0")
					});
				}
			}
		}
		return list;
	}

	private List<Channel_生成位号表> GenerateChannel(List<IO分站清单_中控> ioStations, List<板卡清单_生成位号表> cardList)
	{
		if (cardList.Count == 0)
		{
			return new List<Channel_生成位号表>();
		}
		return new List<Channel_生成位号表>();
	}

	private string GetModuleType(string cardType)
	{
		return cardType switch
		{
			"AI711-S" => "AI711-S 模拟信号输入模块（8路）", 
			"AI711-H" => "AI711-H 模拟信号输入模块（8路，HART）", 
			"PI711-S" => "PI711-S 脉冲信号输入模块（6路）", 
			"AI731-S" => "AI731-S 热电阻信号输入模块（8路）", 
			"AI722-S" => "AI722-S 热电偶信号输入模块（8路）", 
			_ => "未知模块类型", 
		};
	}

	private string GetSignalType(string cardType)
	{
		switch (cardType)
		{
		case "AI711-S":
		case "AI711-H":
			return "电流(4mA～20mA)";
		case "PI711-S":
			return "0Hz～1000Hz";
		case "AI731-S":
			return "Pt100（-200℃~850℃）";
		case "AI722-S":
			return "K型(-200℃～1300℃)";
		default:
			return "Err";
		}
	}

	private string GetDataFormat(string cardType)
	{
		switch (cardType)
		{
		case "AI711-S":
		case "AI711-H":
			return "线性转换";
		case "PI711-S":
			return "无转换";
		default:
			return "线性转换";
		}
	}
}
