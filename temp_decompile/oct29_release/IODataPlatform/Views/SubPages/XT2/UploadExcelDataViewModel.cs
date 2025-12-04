using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Aspose.Cells;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui;

namespace IODataPlatform.Views.SubPages.XT2;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class UploadExcelDataViewModel(SqlSugarContext context, DepXT2ViewModel xt2, DepXT1ViewModel xt1, GlobalModel model, IPickerService picker, ExcelService excel, INavigationService navigation, IMessageService msg, NavigationParameterService parameterService) : ObservableObject()
{
	private ControlSystem controlSystem;

	[ObservableProperty]
	private string subNet = string.Empty;

	[ObservableProperty]
	private ObservableCollection<pdf_control_io>? ioData;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.XT2.UploadExcelDataViewModel.ImportDataCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<string>? importDataCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.XT2.UploadExcelDataViewModel.ConfirmCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? confirmCommand;

	public int IoDataCount => IoData?.Count ?? 0;

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.XT2.UploadExcelDataViewModel.subNet" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SubNet
	{
		get
		{
			return subNet;
		}
		[MemberNotNull("subNet")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(subNet, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SubNet);
				subNet = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SubNet);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.XT2.UploadExcelDataViewModel.ioData" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<pdf_control_io>? IoData
	{
		get
		{
			return ioData;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<pdf_control_io>>.Default.Equals(ioData, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IoData);
				ioData = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IoData);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.XT2.UploadExcelDataViewModel.ImportData(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<string> ImportDataCommand => importDataCommand ?? (importDataCommand = new AsyncRelayCommand<string>(ImportData));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.XT2.UploadExcelDataViewModel.Confirm" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ConfirmCommand => confirmCommand ?? (confirmCommand = new AsyncRelayCommand(Confirm));

	[RelayCommand]
	private async Task ImportData(string param)
	{
		string[] array = picker.OpenFiles("Excel 文件(*.xls; *.xlsx)|*.xls; *.xlsx");
		if (array != null)
		{
			await ImportIoData(array);
		}
	}

	private List<T> GetExcelData<T>(string file) where T : new()
	{
		using Workbook workbook = excel.GetWorkbook(file);
		List<T> list = new List<T>();
		foreach (Worksheet worksheet in workbook.Worksheets)
		{
			Cells cells = worksheet.Cells;
			DataTable dataTable = cells.ExportDataTableAsString(0, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, exportColumnName: true);
			string[] first = (from DataColumn x in dataTable.Columns
				select x.ColumnName).ToArray();
			string[] second = (from x in typeof(T).GetProperties()
				select x.GetCustomAttribute<DisplayAttribute>()?.Name ?? x.Name).ToArray();
			string[] array = first.Except(second).ToArray();
			if (array != null && array.Length > 0)
			{
				throw new Exception(("以下不是标准列名称：" + string.Join("、", array)).Replace("\r", "").Replace("\n", ""));
			}
			list.AddRange(dataTable.StringTableToIEnumerableByDiplay<T>());
		}
		return list;
	}

	private async Task ImportIoData(string[] files)
	{
		model.Status.Busy("正在导入输入输出表……");
		ControlSystem parameter = parameterService.GetParameter<ControlSystem>("controlSystem");
		controlSystem = parameter;
		switch (controlSystem)
		{
		case ControlSystem.龙鳍:
		{
			ObservableCollection<pdf_control_io> observableCollection = new ObservableCollection<pdf_control_io>();
			foreach (xtes_pdf_control_io item in await Task.Run(() => files.SelectMany(GetExcelData<xtes_pdf_control_io>)))
			{
				observableCollection.Add(item);
			}
			IoData = observableCollection;
			break;
		}
		case ControlSystem.一室:
		{
			ObservableCollection<pdf_control_io> observableCollection = new ObservableCollection<pdf_control_io>();
			foreach (xtys_pdf_control_io item2 in await Task.Run(() => files.SelectMany(GetExcelData<xtys_pdf_control_io>)))
			{
				observableCollection.Add(item2);
			}
			IoData = observableCollection;
			break;
		}
		}
		model.Status.Success("已导入输入输出表");
	}

	[RelayCommand]
	private async Task Confirm()
	{
		if (string.IsNullOrEmpty(SubNet))
		{
			throw new Exception("请输入子网");
		}
		if (IoData == null)
		{
			throw new Exception("请先导入输入输出表");
		}
		context.Db.Queryable<config_card_type_judge>().ToListAsync();
		model.Status.Busy("正在生成数据……");
		ObservableCollection<IoFullData> oldPoints;
		switch (controlSystem)
		{
		case ControlSystem.龙鳍:
		{
			if (xt2.SubProject == null)
			{
				throw new Exception("开发人员注意");
			}
			if (xt2.AllData == null)
			{
				throw new Exception("开发人员注意");
			}
			Task<List<IoFullData>> task = Task.Run(() => GetIoDataSystem2(IoData.OfType<xtes_pdf_control_io>().ToList().ToList(), SubNet));
			oldPoints = xt2.AllData;
			List<IoFullData> source = await task;
			IEnumerable<string> enumerable = from item in source
				where oldPoints.Select((IoFullData o) => o.SignalPositionNumber + "_" + o.ExtensionCode).Contains(item.SignalPositionNumber + "_" + item.ExtensionCode)
				select item.SignalPositionNumber + "_" + item.ExtensionCode;
			string text = (enumerable.Any() ? $"以下主键重复，共{enumerable.Count()}个：\n{string.Join(";\n", enumerable)};\n" : "");
			if (text != "")
			{
				await msg.AlertAsync(text);
				throw new Exception("主键重复，无法导入");
			}
			DepXT2ViewModel depXT2ViewModel = xt2;
			ObservableCollection<IoFullData> observableCollection2 = new ObservableCollection<IoFullData>();
			foreach (IoFullData item in source.OrderBy((IoFullData n) => n.CabinetNumber))
			{
				observableCollection2.Add(item);
			}
			depXT2ViewModel.AllData = observableCollection2;
			model.Status.Busy("正在保存……");
			await xt2.SaveAndUploadFileAsync();
			break;
		}
		case ControlSystem.一室:
		{
			if (xt1.SubProject == null)
			{
				throw new Exception("开发人员注意");
			}
			if (xt1.AllData == null)
			{
				throw new Exception("开发人员注意");
			}
			Task<List<IoFullData>> task = Task.Run(() => GetIoDataSystem1(IoData.OfType<xtys_pdf_control_io>().ToList().ToList(), SubNet));
			oldPoints = xt2.AllData;
			List<IoFullData> source = await task;
			IEnumerable<string> enumerable = from item in source
				where oldPoints.Select((IoFullData o) => o.SignalPositionNumber + "_" + o.ExtensionCode).Contains(item.SignalPositionNumber + "_" + item.ExtensionCode)
				select item.SignalPositionNumber + "_" + item.ExtensionCode;
			string text = (enumerable.Any() ? $"以下主键重复，共{enumerable.Count()}个：\n{string.Join(";\n", enumerable)};\n" : "");
			if (text != "")
			{
				await msg.AlertAsync(text);
				throw new Exception("主键重复，无法导入");
			}
			DepXT1ViewModel depXT1ViewModel = xt1;
			ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
			foreach (IoFullData item2 in source.OrderBy((IoFullData n) => n.CabinetNumber))
			{
				observableCollection.Add(item2);
			}
			depXT1ViewModel.AllData = observableCollection;
			model.Status.Busy("正在保存……");
			await xt1.SaveAndUploadFileAsync();
			break;
		}
		}
		model.Status.Success("导入成功");
		navigation.GoBack();
	}

	private async Task<List<IoFullData>> GetIoDataSystem2(List<xtes_pdf_control_io> IOs, string subNet)
	{
		_ = 3;
		try
		{
			List<IoFullData> result = new List<IoFullData>();
			List<config_card_type_judge> config_card_type_judge = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
			List<config_terminalboard_type_judge> config_terminalboard_type_judge = await context.Db.Queryable<config_terminalboard_type_judge>().ToListAsync();
			List<config_output_format_values> config_output_format_values = await context.Db.Queryable<config_output_format_values>().ToListAsync();
			List<config_power_supply_method> list = await context.Db.Queryable<config_power_supply_method>().ToListAsync();
			new FormularHelper();
			for (int i = 0; i < IOs.Count; i++)
			{
				xtes_pdf_control_io xtes_pdf_control_io = IOs[i];
				IoFullData ioFullData = new IoFullData();
				int.TryParse(xtes_pdf_control_io.序号, out var result2);
				ioFullData.SerialNumber = result2;
				ioFullData.PIDDrawingNumber = "";
				ioFullData.SystemCode = "";
				ioFullData.LDADDrawingNumber = "";
				ioFullData.Isolation = "";
				ioFullData.RemoteIO = "";
				ioFullData.Trend = "";
				ioFullData.StatusWhenZero = "";
				ioFullData.StatusWhenOne = "";
				ioFullData.SignalEffectiveMode = "";
				ioFullData.Destination = "";
				ioFullData.RGRelatedScreen = "";
				ioFullData.High4LimitAlarmValue = 0f;
				ioFullData.High4LimitAlarmLevel = "";
				ioFullData.High4LimitAlarmTag = "";
				ioFullData.High4AlarmDescription = "";
				ioFullData.High3LimitAlarmValue = 0f;
				ioFullData.High3LimitAlarmLevel = "";
				ioFullData.High3LimitAlarmTag = "";
				ioFullData.High3AlarmDescription = "";
				ioFullData.High2LimitAlarmValue = 0f;
				ioFullData.High2LimitAlarmLevel = "";
				ioFullData.High2LimitAlarmTag = "";
				ioFullData.High2AlarmDescription = "";
				ioFullData.High1LimitAlarmValue = 0f;
				ioFullData.High1LimitAlarmLevel = "";
				ioFullData.High1LimitAlarmTag = "";
				ioFullData.High1AlarmDescription = "";
				ioFullData.Low1LimitAlarmValue = 0f;
				ioFullData.Low1LimitAlarmLevel = "";
				ioFullData.Low1LimitAlarmTag = "";
				ioFullData.Low1AlarmDescription = "";
				ioFullData.Low2LimitAlarmValue = 0f;
				ioFullData.Low2LimitAlarmLevel = "";
				ioFullData.Low2LimitAlarmTag = "";
				ioFullData.Low2AlarmDescription = "";
				ioFullData.Low3LimitAlarmValue = 0f;
				ioFullData.Low3LimitAlarmLevel = "";
				ioFullData.Low3LimitAlarmTag = "";
				ioFullData.Low3AlarmDescription = "";
				ioFullData.Low4LimitAlarmValue = 0f;
				ioFullData.Low4LimitAlarmLevel = "";
				ioFullData.Low4LimitAlarmTag = "";
				ioFullData.Low4AlarmDescription = "";
				ioFullData.AlarmLevel = "";
				ioFullData.SwitchQuantityAlarmTag = "";
				ioFullData.AlarmDescription = "";
				ioFullData.AlarmAttribute = "";
				ioFullData.StationNumber = "";
				ioFullData.KUPointName = "";
				ioFullData.SubNet = subNet;
				ioFullData.ModificationDate = DateTime.Now;
				ioFullData.LocalBoxNumber = xtes_pdf_control_io.就地箱号;
				ioFullData.SignalPositionNumber = xt2.GetSignalPositionNumber(xtes_pdf_control_io.信号位号);
				ioFullData.ExtensionCode = xtes_pdf_control_io.扩展码;
				ioFullData.TagName = xt2.GetTagName(ioFullData.SignalPositionNumber, ioFullData.ExtensionCode);
				ioFullData.Description = xtes_pdf_control_io.信号功能;
				ioFullData.CabinetNumber = xtes_pdf_control_io.机柜号.Replace("-", "");
				ioFullData.SafetyClassificationGroup = xtes_pdf_control_io.安全分级;
				ioFullData.SeismicCategory = xtes_pdf_control_io.抗震类别;
				ioFullData.IoType = xtes_pdf_control_io.IO类型;
				ioFullData.ElectricalCharacteristics = xtes_pdf_control_io.信号特性;
				ioFullData.PowerType = xtes_pdf_control_io.供电类型;
				ioFullData.SensorType = xtes_pdf_control_io.传感器类型;
				ioFullData.EngineeringUnit = xtes_pdf_control_io.单位;
				ioFullData.VoltageLevel = xtes_pdf_control_io.电压等级;
				ioFullData.InstrumentFunctionNumber = xtes_pdf_control_io.仪表功能号;
				ioFullData.FFSlaveModuleModel = xtes_pdf_control_io.FF从站模块型号;
				ioFullData.RangeLowerLimit = xtes_pdf_control_io.最小测量范围;
				ioFullData.RangeUpperLimit = xtes_pdf_control_io.最大测量范围;
				ioFullData.OFDisplayFormat = xt2.GetOfDisplayFormat(config_output_format_values, ioFullData);
				ioFullData.CardType = (xt2.UseFormula ? xt2.GetIoCardTypeFormula(ioFullData) : xt2.GetIoCardType(config_card_type_judge, ioFullData));
				ioFullData.PowerSupplyMethod = (xt2.UseFormula ? xt2.GetPowerSupplyMethodFormula(ioFullData) : xt2.GetPowerSupplyMethod(list, ioFullData));
				ioFullData.TerminalBoardModel = (xt2.UseFormula ? xt2.GetTerminalBoxTypeFormula(ioFullData.CardType, ioFullData.PowerSupplyMethod, ioFullData.ElectricalCharacteristics) : xt2.GetTerminalBoxType(config_terminalboard_type_judge, ioFullData));
				ioFullData.PVPoint = "";
				ioFullData.Version = xtes_pdf_control_io.版本;
				ioFullData.Remarks = xtes_pdf_control_io.备注;
				ioFullData.ModificationDate = DateTime.Now;
				result.Add(ioFullData);
			}
			int startAddress = 2;
			(from r in result
				group r by r.CabinetNumber).ToList().ForEach(delegate(IGrouping<string, IoFullData> cabinetGroup)
			{
				string stationNumber = startAddress++.ToString();
				cabinetGroup.ToList().ForEach(delegate(IoFullData io)
				{
					io.StationNumber = stationNumber;
				});
			});
			return result;
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message.ToString());
		}
	}

	private async Task<List<IoFullData>> GetIoDataSystem1(List<xtys_pdf_control_io> IOs, string subNet)
	{
		_ = 3;
		try
		{
			List<IoFullData> result = new List<IoFullData>();
			List<config_card_type_judge> config_card_type_judge = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
			List<config_terminalboard_type_judge> config_terminalboard_type_judge = await context.Db.Queryable<config_terminalboard_type_judge>().ToListAsync();
			List<config_output_format_values> config_output_format_values = await context.Db.Queryable<config_output_format_values>().ToListAsync();
			List<config_power_supply_method> list = await context.Db.Queryable<config_power_supply_method>().ToListAsync();
			new FormularHelper();
			for (int i = 0; i < IOs.Count; i++)
			{
				xtys_pdf_control_io xtys_pdf_control_io = IOs[i];
				IoFullData ioFullData = new IoFullData();
				int.TryParse(xtys_pdf_control_io.序号, out var result2);
				ioFullData.SerialNumber = result2;
				ioFullData.PIDDrawingNumber = "";
				ioFullData.SystemCode = "";
				ioFullData.LDADDrawingNumber = "";
				ioFullData.Isolation = "";
				ioFullData.RemoteIO = "";
				ioFullData.Trend = "";
				ioFullData.StatusWhenZero = "";
				ioFullData.StatusWhenOne = "";
				ioFullData.SignalEffectiveMode = "";
				ioFullData.Destination = "";
				ioFullData.RGRelatedScreen = "";
				ioFullData.High4LimitAlarmValue = 0f;
				ioFullData.High4LimitAlarmLevel = "";
				ioFullData.High4LimitAlarmTag = "";
				ioFullData.High4AlarmDescription = "";
				ioFullData.High3LimitAlarmValue = 0f;
				ioFullData.High3LimitAlarmLevel = "";
				ioFullData.High3LimitAlarmTag = "";
				ioFullData.High3AlarmDescription = "";
				ioFullData.High2LimitAlarmValue = 0f;
				ioFullData.High2LimitAlarmLevel = "";
				ioFullData.High2LimitAlarmTag = "";
				ioFullData.High2AlarmDescription = "";
				ioFullData.High1LimitAlarmValue = 0f;
				ioFullData.High1LimitAlarmLevel = "";
				ioFullData.High1LimitAlarmTag = "";
				ioFullData.High1AlarmDescription = "";
				ioFullData.Low1LimitAlarmValue = 0f;
				ioFullData.Low1LimitAlarmLevel = "";
				ioFullData.Low1LimitAlarmTag = "";
				ioFullData.Low1AlarmDescription = "";
				ioFullData.Low2LimitAlarmValue = 0f;
				ioFullData.Low2LimitAlarmLevel = "";
				ioFullData.Low2LimitAlarmTag = "";
				ioFullData.Low2AlarmDescription = "";
				ioFullData.Low3LimitAlarmValue = 0f;
				ioFullData.Low3LimitAlarmLevel = "";
				ioFullData.Low3LimitAlarmTag = "";
				ioFullData.Low3AlarmDescription = "";
				ioFullData.Low4LimitAlarmValue = 0f;
				ioFullData.Low4LimitAlarmLevel = "";
				ioFullData.Low4LimitAlarmTag = "";
				ioFullData.Low4AlarmDescription = "";
				ioFullData.AlarmLevel = "";
				ioFullData.SwitchQuantityAlarmTag = "";
				ioFullData.AlarmDescription = "";
				ioFullData.AlarmAttribute = "";
				ioFullData.StationNumber = "";
				ioFullData.KUPointName = "";
				ioFullData.SubNet = subNet;
				ioFullData.ModificationDate = DateTime.Now;
				ioFullData.SignalPositionNumber = xt2.GetSignalPositionNumber(xtys_pdf_control_io.信号位号);
				ioFullData.ExtensionCode = xtys_pdf_control_io.扩展码;
				ioFullData.TagName = xt2.GetTagName(ioFullData.SignalPositionNumber, ioFullData.ExtensionCode);
				ioFullData.Description = xtys_pdf_control_io.信号说明;
				ioFullData.SafetyClassificationGroup = xtys_pdf_control_io.安全分级分组;
				ioFullData.SeismicCategory = xtys_pdf_control_io.抗震类别;
				ioFullData.IoType = xtys_pdf_control_io.IO类型;
				ioFullData.ElectricalCharacteristics = xtys_pdf_control_io.信号特性;
				ioFullData.EngineeringUnit = xtys_pdf_control_io.测量单位;
				ioFullData.RangeLowerLimit = xtys_pdf_control_io.量程下限;
				ioFullData.RangeUpperLimit = xtys_pdf_control_io.量程上限;
				ioFullData.OFDisplayFormat = xt2.GetOfDisplayFormat(config_output_format_values, ioFullData);
				ioFullData.CardType = (xt2.UseFormula ? xt2.GetIoCardTypeFormula(ioFullData) : xt2.GetIoCardType(config_card_type_judge, ioFullData));
				ioFullData.PowerSupplyMethod = (xt2.UseFormula ? xt2.GetPowerSupplyMethodFormula(ioFullData) : xt2.GetPowerSupplyMethod(list, ioFullData));
				ioFullData.TerminalBoardModel = (xt2.UseFormula ? xt2.GetTerminalBoxTypeFormula(ioFullData.CardType, ioFullData.PowerSupplyMethod, ioFullData.ElectricalCharacteristics) : xt2.GetTerminalBoxType(config_terminalboard_type_judge, ioFullData));
				ioFullData.PVPoint = "";
				ioFullData.Version = xtys_pdf_control_io.版本;
				ioFullData.Remarks = xtys_pdf_control_io.备注;
				ioFullData.ModificationDate = DateTime.Now;
				result.Add(ioFullData);
			}
			int startAddress = 2;
			(from r in result
				group r by r.CabinetNumber).ToList().ForEach(delegate(IGrouping<string, IoFullData> cabinetGroup)
			{
				string stationNumber = startAddress++.ToString();
				cabinetGroup.ToList().ForEach(delegate(IoFullData io)
				{
					io.StationNumber = stationNumber;
				});
			});
			return result;
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message.ToString());
		}
	}
}
