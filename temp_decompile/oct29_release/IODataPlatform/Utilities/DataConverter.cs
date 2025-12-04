using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using LYSoft.Libs.ServiceInterfaces;
using SqlSugar;

namespace IODataPlatform.Utilities;

/// <summary>
/// 数据转换器工具类
/// 提供不同数据模型之间的转换功能，支持多种控制系统和数据格式
/// 主要用于IO数据的标准化处理和不同系统间的数据映射转换
/// 支持龙鲭、中控、龙核、一室、安全级等多种控制系统的数据转换
/// </summary>
/// <summary>
/// 数据转换器的IoFullData处理部分
/// 专门处理旧版本数据向IoFullData模型的转换
/// 支持多种控制系统的字段映射和数据类型转换
/// 包含完整的错误处理和缺失字段检测机制
/// </summary>
public static class DataConverter
{
	/// <summary>
	/// 将IoFullData列表转换为指定控制系统的自定义DataTable
	/// 根据不同控制系统的字段映射配置，将标准化的IO数据转换为对应系统的数据格式
	/// 支持龙鲭、中控、龙核、一室、安全级模拟系统等多种控制系统
	/// </summary>
	/// <param name="data">要转换的IoFullData列表</param>
	/// <param name="db">SqlSugar数据库上下文，用于查询字段映射配置</param>
	/// <param name="controlSystem">目标控制系统类型</param>
	/// <returns>返回转换后的DataTable，包含对应控制系统的字段名和数据</returns>
	public static DataTable ToCustomDataTable(this List<IoFullData> data, SqlSugarClient db, ControlSystem controlSystem)
	{
		Dictionary<string, string> dictionary = (from it in db.Queryable<config_controlSystem_mapping>()
			where it.StdField != null
			select it).ToList().ToDictionary((config_controlSystem_mapping it) => it.StdField, (config_controlSystem_mapping it) => controlSystem switch
		{
			ControlSystem.龙鳍 => it.LqOld, 
			ControlSystem.中控 => it.ZkOld, 
			ControlSystem.龙核 => it.LhOld, 
			ControlSystem.一室 => it.Xt1Old, 
			ControlSystem.安全级模拟系统 => it.AQJMNOld, 
			_ => null, 
		});
		DataTable dataTable = new DataTable();
		PropertyInfo[] properties = typeof(IoFullData).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			string key = propertyInfo.GetCustomAttribute<DisplayAttribute>()?.Name ?? propertyInfo.Name;
			if (dictionary.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
			{
				dataTable.Columns.Add(value, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
			}
		}
		foreach (IoFullData datum in data)
		{
			DataRow dataRow = dataTable.NewRow();
			PropertyInfo[] properties2 = typeof(IoFullData).GetProperties();
			foreach (PropertyInfo propertyInfo2 in properties2)
			{
				string key2 = propertyInfo2.GetCustomAttribute<DisplayAttribute>()?.Name ?? propertyInfo2.Name;
				if (dictionary.TryGetValue(key2, out var value2) && !string.IsNullOrEmpty(value2))
				{
					dataRow[value2] = propertyInfo2.GetValue(datum) ?? DBNull.Value;
				}
			}
			dataTable.Rows.Add(dataRow);
		}
		return dataTable;
	}

	/// <summary>
	/// 将IoFullData对象转换为IoData对象
	/// 实现不同IO数据模型之间的映射转换，保持数据完整性
	/// 将完整的IO数据模型转换为简化的IO数据模型，适用于不同业务场景
	/// </summary>
	/// <param name="data">要转换的IoFullData源对象</param>
	/// <returns>返回转换后的IoData对象</returns>
	public static IoData ToIoData(this IoFullData data)
	{
		return new IoData
		{
			Id = data.SerialNumber,
			TagName = data.TagName,
			OldVarName = data.SignalPositionNumber,
			OldExtCode = data.ExtensionCode,
			Designation = data.Description,
			Unit = "",
			Lot = "",
			SubItem = "",
			FcuName = "",
			Room = "",
			Cabinet = data.CabinetNumber,
			IoType = data.IoType,
			Type = data.CardType,
			IoCardNumber = data.CardNumber,
			Pin = data.Channel,
			TAPartNumber = data.TerminalBoardModel,
			Component = data.TerminalBoardNumber,
			SignalStart = data.SignalPositionNumber,
			Destination = data.Destination,
			PowerType = data.PowerType,
			LoopVoltage = "",
			ContactCapacity = "",
			SignalSpec = "",
			Isolation = data.Isolation,
			Ees = "",
			SensorType = data.SensorType,
			TypicalLoopDrawing = "",
			Sa = "",
			SoeTra = "",
			IntExt = "",
			SafetyClassDivision = data.SafetyClassificationGroup,
			FunctionalClass = "",
			Seismic = data.SeismicCategory,
			System = data.SystemCode,
			List = "",
			DiagramNumber = data.PIDDrawingNumber,
			TransNo = "",
			TerminalBlock = "",
			Remarks = data.Remarks,
			SerialNumber = data.SerialNumber.ToString(),
			PIDDrawingNumber = data.PIDDrawingNumber,
			LDADDrawingNumber = data.LDADDrawingNumber,
			CardNumber = data.CardNumber,
			Cage = data.Cage.ToString(),
			Slot = data.Slot,
			CardAddress = data.CardAddress,
			FfdpTerminalchannel = data.FFTerminalChannel,
			ElectricalCharacteristics = data.ElectricalCharacteristics,
			PowerSupplyMethod = data.PowerSupplyMethod,
			EngineeringUnit = data.EngineeringUnit,
			RangeLowerLimit = data.RangeLowerLimit,
			RangeUpperLimit = data.RangeUpperLimit,
			RemoteIO = data.RemoteIO,
			Trend = data.Trend,
			StatusWhenZero = data.StatusWhenZero,
			StatusWhenOne = data.StatusWhenOne,
			SignalEffectiveMode = data.SignalEffectiveMode,
			LocalBoxNumber = data.LocalBoxNumber,
			Version = data.Version,
			ModificationDate = data.ModificationDate,
			RgRelatedscreen = data.RGRelatedScreen,
			High4LimitAlarmValue = data.High4LimitAlarmValue,
			High4LimitAlarmLevel = data.High4LimitAlarmLevel,
			High4LimitAlarmTag = data.High4LimitAlarmTag,
			High4AlarmDescription = data.High4AlarmDescription,
			High3LimitAlarmValue = data.High3LimitAlarmValue,
			High3LimitAlarmLevel = data.High3LimitAlarmLevel,
			High3LimitAlarmTag = data.High3LimitAlarmTag,
			High3AlarmDescription = data.High3AlarmDescription,
			High2LimitAlarmValue = data.High2LimitAlarmValue,
			High2LimitAlarmLevel = data.High2LimitAlarmLevel,
			High2LimitAlarmTag = data.High2LimitAlarmTag,
			High2AlarmDescription = data.High2AlarmDescription,
			High1LimitAlarmValue = data.High1LimitAlarmValue,
			High1LimitAlarmLevel = data.High1LimitAlarmLevel,
			High1LimitAlarmTag = data.High1LimitAlarmTag,
			High1AlarmDescription = data.High1AlarmDescription,
			Low1LimitAlarmValue = data.Low1LimitAlarmValue,
			Low1LimitAlarmLevel = data.Low1LimitAlarmLevel,
			Low1LimitAlarmTag = data.Low1LimitAlarmTag,
			Low1AlarmDescription = data.Low1AlarmDescription,
			Low2LimitAlarmValue = data.Low2LimitAlarmValue,
			Low2LimitAlarmLevel = data.Low2LimitAlarmLevel,
			Low2LimitAlarmTag = data.Low2LimitAlarmTag,
			Low2AlarmDescription = data.Low2AlarmDescription,
			Low3LimitAlarmValue = data.Low3LimitAlarmValue,
			Low3LimitAlarmLevel = data.Low3LimitAlarmLevel,
			Low3LimitAlarmTag = data.Low3LimitAlarmTag,
			Low3AlarmDescription = data.Low3AlarmDescription,
			Low4LimitAlarmValue = data.Low4LimitAlarmValue,
			Low4LimitAlarmLevel = data.Low4LimitAlarmLevel,
			Low4LimitAlarmTag = data.Low4LimitAlarmTag,
			Low4AlarmDescription = data.Low4AlarmDescription,
			AlarmLevel = data.AlarmLevel,
			SwitchQuantityAlarmTag = data.SwitchQuantityAlarmTag,
			AlarmDescription = data.AlarmDescription,
			AlarmAttribute = data.AlarmAttribute,
			SnStationnumber = data.StationNumber,
			SubnetSubnet = data.SubNet,
			KUPointName = data.KUPointName,
			OfDisplayformat = data.OFDisplayFormat,
			PVPoint = data.PVPoint,
			Connection1 = data.SignalPlus,
			SigPotential1 = "",
			Connection2 = data.SignalMinus,
			SigPotential2 = "",
			Connection3 = data.RTDCompensationC,
			SigPotential3 = "",
			Connection4 = data.RTDCompensationE,
			SigPotential4 = "",
			BackCardType = "",
			ScaleType = "",
			DefaultNumber = "",
			AllocateInfo = ""
		};
	}

	public static IoData ToIoData(this AQJIoData data)
	{
		return new IoData
		{
			TagName = data.信号名称,
			OldVarName = data.原变量名,
			Designation = data.信号说明,
			Room = data.房间号,
			Cabinet = data.机柜号,
			IoType = data.IO类型,
			Type = data.前卡件类型,
			IoCardNumber = data.后卡件类型,
			Pin = data.通道号,
			TAPartNumber = data.端子板类型,
			Component = data.端子板编号,
			PowerType = data.电压等级,
			SensorType = data.传感器类型,
			Connection1 = data.端子号1,
			Connection2 = data.端子号2,
			TypicalLoopDrawing = data.典型回路图,
			IntExt = data.内部外部,
			SafetyClassDivision = data.安全分级,
			Remarks = data.注释,
			SerialNumber = data.序号,
			Cage = data.机箱号.ToString(),
			Slot = data.槽位号,
			Version = data.版本,
			System = data.系统名,
			ModificationDate = null,
			MajorDest = data.去向专业
		};
	}

	public static int ToInt(this string number)
	{
		int.TryParse(number, out var result);
		return result;
	}

	public static TerminationData ToTerminationData(this IoData data)
	{
		return new TerminationData
		{
			IOPointName = data.TagName,
			OriginalVariableName = data.OldVarName,
			OriginalExtensionCode = data.OldExtCode,
			SignalDescription = data.Designation,
			UnitNumber = data.Unit,
			LOT = data.Lot,
			SubItem = data.SubItem,
			ControllerStationName = data.FcuName,
			RoomNumber = data.Room,
			CabinetNumber = data.Cabinet,
			IOType = data.IoType,
			IOCardModel = data.Type,
			IOCardNumber = data.IoCardNumber,
			ChannelNumber = data.Pin,
			TerminalBoardModel = data.TAPartNumber,
			TerminalBoardNumber = data.Component,
			SignalStart = data.SignalStart,
			SignalEnd = data.Destination,
			PowerType_Supplier_ = data.PowerType,
			CircuitVoltage = data.LoopVoltage,
			ContactCapacity = data.ContactCapacity,
			SignalCharacteristics = data.SignalSpec,
			Isolation = data.Isolation,
			EESPowerSupply = data.Ees,
			SensorType = data.SensorType,
			TypicalCircuitDiagram = data.TypicalLoopDrawing,
			SAPowerSupply = data.Sa,
			AccidentRecall_TransientRecord = data.SoeTra,
			Internal_ExternalPoints = data.IntExt,
			SafetyClassification_Grouping = data.SafetyClassDivision,
			FunctionalClassification = data.FunctionalClass,
			SeismicCategory = data.Seismic,
			SystemName = data.System,
			IOListCategory = data.List,
			DrawingNumber = data.DiagramNumber,
			TransferOrderNumber = data.TransNo,
			TerminalStripNumber = data.TerminalBlock,
			ConnectionPoint1 = data.Connection1,
			SignalDescription1 = data.SigPotential1,
			ConnectionPoint2 = data.Connection2,
			SignalDescription2 = data.SigPotential2,
			ConnectionPoint3 = data.Connection3,
			SignalDescription3 = data.SigPotential3,
			ConnectionPoint4 = data.Connection4,
			SignalDescription4 = data.SigPotential4,
			MajorDest = data.MajorDest
		};
	}

	public static xtes_duanjie toduanjieData(this IoFullData data)
	{
		return new xtes_duanjie
		{
			I_O点名 = data.TagName,
			信号说明 = data.Description,
			房间号 = "",
			机柜名_起点 = data.CabinetNumber,
			设备编号_起点 = data.TerminalBoardNumber,
			信号特性 = data.ElectricalCharacteristics,
			接线点1 = data.SignalPlus,
			接线点说明1 = "+",
			接线点2 = data.SignalMinus,
			接线点说明2 = "-",
			接线点3 = data.RTDCompensationC,
			接线点说明3 = "",
			接线点4 = data.RTDCompensationE,
			接线点说明4 = "",
			供电方式 = data.PowerSupplyMethod,
			房间号_终点 = "",
			设备编号_终点 = data.LocalBoxNumber,
			电缆编码 = "",
			电缆型号及规格 = "",
			电缆长度 = "",
			供货方 = "",
			版本 = ""
		};
	}

	public static CableData MatchCableData(TerminationData t1, TerminationData t2)
	{
		return new CableData
		{
			起点信号位号 = t1.IOPointName,
			起点房间号 = t1.RoomNumber,
			起点盘柜名称 = t1.CabinetNumber,
			起点设备名称 = t1.TerminalBoardModel,
			起点接线点1 = t1.ConnectionPoint1,
			起点接线点2 = t1.ConnectionPoint2,
			起点接线点3 = t1.ConnectionPoint3,
			起点接线点4 = t1.ConnectionPoint4,
			起点IO类型 = t1.IOType,
			起点安全分级分组 = t1.SafetyClassification_Grouping,
			起点系统号 = t1.SystemName,
			起点专业 = t1.MajorDest,
			终点信号位号 = t2.IOPointName,
			终点房间号 = t2.RoomNumber,
			终点盘柜名称 = t2.CabinetNumber,
			终点设备名称 = t2.TerminalBoardModel,
			终点接线点1 = t2.ConnectionPoint1,
			终点接线点2 = t2.ConnectionPoint2,
			终点接线点3 = t2.ConnectionPoint3,
			终点接线点4 = t2.ConnectionPoint4,
			终点IO类型 = t2.IOType,
			终点安全分级分组 = t2.SafetyClassification_Grouping,
			终点系统号 = t2.SystemName,
			终点专业 = t2.MajorDest
		};
	}

	public static async Task<MatchCableDataResult> MatchCableData(IEnumerable<TerminationData> duanjieList1, IEnumerable<TerminationData> duanjieList2, string major1, string major2)
	{
		MatchCableDataResult matchCableDataResult = new MatchCableDataResult();
		matchCableDataResult.SuccessList = new List<CableData>();
		List<TerminationData> list = duanjieList1.Where((TerminationData d) => d.MajorDest == major2).ToList();
		List<TerminationData> list2 = duanjieList2.Where((TerminationData d) => d.MajorDest == major1).ToList();
		foreach (TerminationData item3 in list)
		{
			foreach (TerminationData item4 in list2)
			{
				if (item3.IOPointName.Contains(item4.IOPointName) || item4.IOPointName.Contains(item3.IOPointName))
				{
					matchCableDataResult.SuccessList.Add(new CableData
					{
						起点IO类型 = item3.IOType,
						起点信号位号 = item3.IOPointName,
						起点房间号 = item3.RoomNumber,
						起点盘柜名称 = item3.CabinetNumber,
						起点设备名称 = item3.TerminalBoardNumber,
						起点安全分级分组 = item3.SafetyClassification_Grouping,
						起点系统号 = item3.SystemName,
						起点接线点1 = item3.ConnectionPoint1,
						起点接线点2 = item3.ConnectionPoint2,
						起点接线点3 = item3.ConnectionPoint3,
						起点接线点4 = item3.ConnectionPoint4,
						起点专业 = major1,
						供货方 = "CNCS",
						终点IO类型 = item4.IOType,
						终点信号位号 = item4.IOPointName,
						终点房间号 = item4.RoomNumber,
						终点盘柜名称 = item4.CabinetNumber,
						终点设备名称 = item4.TerminalBoardNumber,
						终点安全分级分组 = item4.SafetyClassification_Grouping,
						终点系统号 = item4.SystemName,
						终点接线点1 = item4.ConnectionPoint1,
						终点接线点2 = item4.ConnectionPoint2,
						终点接线点3 = item4.ConnectionPoint3,
						终点接线点4 = item4.ConnectionPoint4,
						终点专业 = major2
					});
					item3.IsMatched = true;
					item4.IsMatched = true;
				}
			}
		}
		matchCableDataResult.FailList1 = new List<MatchCableDataFail>();
		matchCableDataResult.FailList2 = new List<MatchCableDataFail>();
		foreach (TerminationData item5 in list.Where((TerminationData l) => !l.IsMatched))
		{
			MatchCableDataFail item = new MatchCableDataFail
			{
				Data = item5,
				Reason = "找不到IO点"
			};
			matchCableDataResult.FailList1.Add(item);
		}
		foreach (TerminationData item6 in list2.Where((TerminationData l) => !l.IsMatched))
		{
			MatchCableDataFail item2 = new MatchCableDataFail
			{
				Data = item6,
				Reason = "找不到IO点"
			};
			matchCableDataResult.FailList2.Add(item2);
		}
		return matchCableDataResult;
	}

	public static async Task<Tuple<TerminationData, TerminationData>> UnMatchCableData(CableData cableData)
	{
		TerminationData item = new TerminationData
		{
			IOPointName = cableData.起点信号位号,
			RoomNumber = cableData.起点房间号,
			CabinetNumber = cableData.起点盘柜名称,
			TerminalBoardModel = cableData.起点设备名称,
			ConnectionPoint1 = cableData.起点接线点1,
			ConnectionPoint2 = cableData.起点接线点2,
			ConnectionPoint3 = cableData.起点接线点3,
			ConnectionPoint4 = cableData.起点接线点4,
			IOType = cableData.起点IO类型,
			SafetyClassification_Grouping = cableData.起点安全分级分组,
			SystemName = cableData.起点系统号,
			MajorDest = cableData.起点专业
		};
		TerminationData item2 = new TerminationData
		{
			IOPointName = cableData.终点信号位号,
			RoomNumber = cableData.终点房间号,
			CabinetNumber = cableData.终点盘柜名称,
			TerminalBoardModel = cableData.终点设备名称,
			ConnectionPoint1 = cableData.终点接线点1,
			ConnectionPoint2 = cableData.终点接线点2,
			ConnectionPoint3 = cableData.终点接线点3,
			ConnectionPoint4 = cableData.终点接线点4,
			IOType = cableData.终点IO类型,
			SafetyClassification_Grouping = cableData.终点安全分级分组,
			SystemName = cableData.终点系统号,
			MajorDest = cableData.终点专业
		};
		return Tuple.Create(item, item2);
	}

	public static async Task<List<CableData>> NumberMatchCable(List<CableData> excelData, SqlSugarContext context)
	{
		string constant = "";
		int serialNumber = 1;
		List<config_cable_categoryAndColor> cable_CategoryAndColors = await context.Db.Queryable<config_cable_categoryAndColor>().ToListAsync();
		List<config_cable_function> cableFunctionDtos = await context.Db.Queryable<config_cable_function>().ToListAsync();
		List<config_cable_spec> cable_Specs = await context.Db.Queryable<config_cable_spec>().ToListAsync();
		List<config_cable_systemNumber> cable_SystemNumbers = await context.Db.Queryable<config_cable_systemNumber>().ToListAsync();
		List<config_cable_startNumber> startNumbers = await context.Db.Queryable<config_cable_startNumber>().ToListAsync();
		foreach (CableData cd in excelData)
		{
			config_cable_function config_cable_function = cableFunctionDtos.FirstOrDefault((config_cable_function c) => c.FirstIOType == cd.起点IO类型 && c.SecondIOType == cd.终点IO类型);
			cd.IO类型 = ((config_cable_function == null) ? cd.起点IO类型 : config_cable_function.CableIOType);
			if (cd.起点系统号 != null && cd.终点系统号 != null)
			{
				cd.SystemNo = cable_SystemNumbers.FirstOrDefault((config_cable_systemNumber t) => cd.起点系统号.Contains(t.StartSystem) && cd.终点系统号.Contains(t.EndSystem)) ?? new config_cable_systemNumber();
			}
		}
		List<每根电缆> list = new List<每根电缆>();
		IEnumerable<IGrouping<string, CableData>> enumerable = from record in excelData
			group record by record.IO类型.Substring(0, 1);
		foreach (IGrouping<string, CableData> item in enumerable)
		{
			string cableType = ((item.Key == "D") ? "控制电缆" : "测量电缆");
			IEnumerable<IGrouping<string, CableData>> enumerable2 = from record in item
				group record by record.起点设备名称;
			foreach (IGrouping<string, CableData> item2 in enumerable2)
			{
				item2.ToList();
				IEnumerable<IGrouping<string, CableData>> enumerable3 = from record in item2
					group record by record.终点盘柜名称;
				foreach (IGrouping<string, CableData> item3 in enumerable3)
				{
					item3.ToList();
					IOrderedEnumerable<IGrouping<string, CableData>> orderedEnumerable = from record in item3
						group record by record.起点信号位号.Split('_').FirstOrDefault() into @group
						orderby @group.Count() descending
						select @group;
					foreach (IGrouping<string, CableData> item4 in orderedEnumerable)
					{
						List<CableData> list2 = item4.Where((CableData s) => s.起点系统号 != null && s.终点系统号 != null).ToList();
						int remainingPoints;
						int num;
						for (remainingPoints = list2.Sum((CableData signal) => CountConnectionPoints(signal)); remainingPoints > 0; remainingPoints -= num)
						{
							config_cable_spec config_cable_spec = (from c in cable_Specs
								where c.CableType == cableType
								orderby c.ConnectCount
								select c).FirstOrDefault((config_cable_spec c) => c.ConnectCount >= remainingPoints);
							if (config_cable_spec == null)
							{
								config_cable_spec = (from c in cable_Specs
									where c.CableType == cableType
									orderby c.ConnectCount descending
									select c).FirstOrDefault();
							}
							每根电缆 每根电缆 = new 每根电缆();
							每根电缆.控制电缆 = (电缆类型)Enum.Parse(typeof(电缆类型), cableType);
							每根电缆.Signals = new List<CableData>();
							每根电缆 每根电缆2 = 每根电缆;
							num = 0;
							int coreNumber = 1;
							foreach (CableData item5 in list2.ToList())
							{
								int num2 = CountConnectionPoints(item5);
								if (num + num2 <= config_cable_spec.ConnectCount)
								{
									每根电缆2.Signals.Add(item5);
									num += num2;
									AssignSignalAttributes(item5, serialNumber++, cable_CategoryAndColors, config_cable_spec);
									AssignCoreNumbers(item5, cableType, ref coreNumber);
									list2.Remove(item5);
									continue;
								}
								break;
							}
							GenerateCableNumber(每根电缆2, cableType, constant, cable_SystemNumbers, startNumbers);
							list.Add(每根电缆2);
						}
					}
				}
			}
		}
		List<CableData> list3 = new List<CableData>();
		foreach (每根电缆 item6 in list)
		{
			list3.AddRange(item6.Signals);
		}
		return list3;
	}

	public static async Task<List<CableData>> NumberMatchCable1(List<CableData> excelData, SqlSugarContext context)
	{
		string constant = "";
		int serialNumber = 1;
		List<config_cable_categoryAndColor> cable_CategoryAndColors = await context.Db.Queryable<config_cable_categoryAndColor>().ToListAsync();
		List<config_cable_function> cableFunctionDtos = await context.Db.Queryable<config_cable_function>().ToListAsync();
		List<config_cable_spec> cable_Specs = await context.Db.Queryable<config_cable_spec>().ToListAsync();
		List<config_cable_systemNumber> cable_SystemNumbers = await context.Db.Queryable<config_cable_systemNumber>().ToListAsync();
		List<config_cable_startNumber> startNumbers = await context.Db.Queryable<config_cable_startNumber>().ToListAsync();
		foreach (CableData cd in excelData)
		{
			config_cable_function config_cable_function = cableFunctionDtos.FirstOrDefault((config_cable_function c) => c.FirstIOType == cd.起点IO类型 && c.SecondIOType == cd.终点IO类型);
			cd.IO类型 = ((config_cable_function == null) ? cd.起点IO类型 : config_cable_function.CableIOType);
			if (cd.起点系统号 != null && cd.终点系统号 != null)
			{
				cd.SystemNo = cable_SystemNumbers.FirstOrDefault((config_cable_systemNumber t) => cd.起点系统号.Contains(t.StartSystem) && cd.终点系统号.Contains(t.EndSystem)) ?? new config_cable_systemNumber();
			}
		}
		List<每根电缆> list = new List<每根电缆>();
		IEnumerable<IGrouping<string, CableData>> enumerable = from record in excelData
			group record by record.IO类型.Substring(0, 1);
		foreach (IGrouping<string, CableData> item in enumerable)
		{
			string cableType = ((item.Key == "D") ? "控制电缆" : "测量电缆");
			IEnumerable<IGrouping<string, CableData>> enumerable2 = from record in item
				group record by record.终点盘柜名称;
			foreach (IGrouping<string, CableData> item2 in enumerable2)
			{
				List<CableData> list2 = (from s in item2
					where s.起点系统号 != null && s.终点系统号 != null
					orderby s.起点IO类型
					select s).ToList();
				int num = list2.Sum((CableData signal) => CountConnectionPoints(signal));
				while (num > 0)
				{
					config_cable_spec config_cable_spec = (from c in cable_Specs
						where c.CableType == cableType
						orderby c.ConnectCount descending
						select c).FirstOrDefault();
					每根电缆 每根电缆 = new 每根电缆();
					每根电缆.控制电缆 = (电缆类型)Enum.Parse(typeof(电缆类型), cableType);
					每根电缆.Signals = new List<CableData>();
					每根电缆 每根电缆2 = 每根电缆;
					int num2 = 0;
					int coreNumber = 1;
					foreach (CableData item3 in list2.ToList())
					{
						int num3 = CountConnectionPoints(item3);
						if (num2 + num3 <= config_cable_spec.ConnectCount)
						{
							每根电缆2.Signals.Add(item3);
							num2 += num3;
							AssignSignalAttributes(item3, serialNumber++, cable_CategoryAndColors, config_cable_spec);
							AssignCoreNumbers(item3, cableType, ref coreNumber);
							list2.Remove(item3);
							continue;
						}
						break;
					}
					GenerateCableNumber(每根电缆2, cableType, constant, cable_SystemNumbers, startNumbers);
					list.Add(每根电缆2);
					num -= num2;
				}
			}
		}
		List<CableData> list3 = new List<CableData>();
		foreach (每根电缆 item4 in list)
		{
			list3.AddRange(item4.Signals);
		}
		return list3;
	}

	private static int CountConnectionPoints(CableData signal)
	{
		int num = 0;
		if (!string.IsNullOrEmpty(signal.起点接线点1))
		{
			num++;
		}
		if (!string.IsNullOrEmpty(signal.起点接线点2))
		{
			num++;
		}
		return num;
	}

	private static void AssignSignalAttributes(CableData signal, int serialNumber, List<config_cable_categoryAndColor> cable_CategoryAndColors, config_cable_spec matchingCableSpec)
	{
		config_cable_categoryAndColor config_cable_categoryAndColor = cable_CategoryAndColors.FirstOrDefault((config_cable_categoryAndColor c) => c.FirstSafetyGroup == signal.起点安全分级分组 && c.SecondSafetyGroup == signal.终点安全分级分组);
		signal.序号 = serialNumber.ToString();
		signal.线缆列别 = config_cable_categoryAndColor?.CableCategory ?? "未查到";
		signal.色标 = config_cable_categoryAndColor?.Color ?? "未查到";
		signal.特性代码 = matchingCableSpec.FeatureCode;
	}

	private static void AssignCoreNumbers(CableData signal, string cableType, ref int coreNumber)
	{
		if (cableType == "控制电缆")
		{
			List<string> list = new List<string>();
			if (!string.IsNullOrEmpty(signal.起点接线点1))
			{
				list.Add(coreNumber++.ToString());
			}
			if (!string.IsNullOrEmpty(signal.起点接线点2))
			{
				list.Add(coreNumber++.ToString());
			}
			signal.芯线号 = string.Join("&", list);
		}
		else if (cableType == "测量电缆")
		{
			signal.芯线号 = "R&IY";
		}
	}

	private static void GenerateCableNumber(每根电缆 cable, string cableType, string constant, List<config_cable_systemNumber> cable_SystemNumbers, List<config_cable_startNumber> startNumbers)
	{
		string text = ((cableType == "控制电缆") ? "C" : "M");
		if (cable.Signals.Count == 0)
		{
			return;
		}
		CableData signal = cable.Signals.FirstOrDefault();
		if (signal.起点系统号.Contains("盘台") || signal.终点系统号.Contains("盘台"))
		{
			config_cable_startNumber config_cable_startNumber = startNumbers.FirstOrDefault((config_cable_startNumber n) => n.StartCabinetNo == signal.起点盘柜名称 && n.Sequence == signal.线缆列别);
			if (config_cable_startNumber == null)
			{
				throw new Exception("没有找到盘台匹配的流水号");
			}
			foreach (CableData signal2 in cable.Signals)
			{
				string text2 = config_cable_startNumber.StartNumber.ToString().PadLeft(4, '0');
				signal.线缆编号 = constant + signal.SystemNo.CableSystem + text + text2;
			}
			config_cable_startNumber.StartNumber++;
			return;
		}
		config_cable_systemNumber config_cable_systemNumber = cable_SystemNumbers.FirstOrDefault((config_cable_systemNumber c) => c.StartMajor != null && c.EndMajor != null && signal.起点专业.Contains(c.StartMajor) && signal.终点专业.Contains(c.EndMajor) && signal.起点系统号.Contains(c.StartSystem) && signal.终点系统号.Contains(c.EndSystem));
		if (config_cable_systemNumber == null)
		{
			throw new Exception($"没有找到机柜起点专业：{signal.起点专业} 终点专业：{signal.终点专业} 起点系统号：{signal.起点系统号} 终点系统号：{signal.终点系统号}匹配的流水号");
		}
		if (config_cable_systemNumber.MinNo == config_cable_systemNumber.MaxNo)
		{
			throw new Exception($"机柜起点专业：{signal.起点专业} 终点专业：{signal.终点专业} 起点系统号：{signal.起点系统号} 终点系统号：{signal.终点系统号} 超过最大流水号");
		}
		foreach (CableData signal3 in cable.Signals)
		{
			signal3.线缆编号 = $"{constant}{signal3.SystemNo.CableSystem}{text}{config_cable_systemNumber.MinNo}";
		}
		config_cable_systemNumber.MinNo++;
	}

	private static int GetMaxSignalSupport(bool DorA, List<config_cable_spec> cableSpecifications)
	{
		if (!DorA)
		{
			return cableSpecifications.Where((config_cable_spec c) => c.CableType == "测量电缆").Max((config_cable_spec c) => c.ConnectCount);
		}
		return cableSpecifications.Where((config_cable_spec c) => c.CableType == "控制电缆").Max((config_cable_spec c) => c.ConnectCount);
	}

	private static void AssignSerialNumbers(List<CableData> cables)
	{
		for (int i = 0; i < cables.Count; i++)
		{
			cables[i].序号 = (i + 1).ToString();
		}
	}

	private static void SwapCablePropertiesForTypeA(List<CableData> cables)
	{
		foreach (CableData item in cables.Where((CableData r) => r.IO类型.StartsWith("A")))
		{
			CableData cableData = item;
			string 芯线对数号 = item.芯线对数号;
			string 芯线号 = item.芯线号;
			item.芯线号 = 芯线对数号;
			cableData.芯线对数号 = 芯线号;
		}
	}

	public static Tuple<int, int> GetCableCountUsed(List<CableData> allSignals, int maxSignalSupport)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < allSignals.Count; i++)
		{
			if (num2 == maxSignalSupport)
			{
				num++;
				num2 = 0;
			}
			if (num2 > maxSignalSupport)
			{
				num++;
				if (i > 0)
				{
					num2 = 0;
					num2 += ((allSignals[i - 1].起点接线点1 != "/") ? 1 : 0);
					num2 += ((allSignals[i - 1].起点接线点2 != "/") ? 1 : 0);
				}
			}
			num2 += ((allSignals[i].起点接线点1 != "/") ? 1 : 0);
			num2 += ((allSignals[i].起点接线点2 != "/") ? 1 : 0);
		}
		return new Tuple<int, int>(num, num2);
	}

	public static int GetSkipRowCount(List<CableData> allSignals, int maxSignalSupport, int i)
	{
		int num = 0;
		int num2 = 0;
		for (int j = 0; j < allSignals.Count; j++)
		{
			if (num == maxSignalSupport * i)
			{
				break;
			}
			if (num > maxSignalSupport * i)
			{
				num2--;
				break;
			}
			num += ((allSignals[j].起点接线点1 != "/") ? 1 : 0);
			num += ((allSignals[j].起点接线点2 != "/") ? 1 : 0);
			num2++;
		}
		return num2;
	}

	public static int GetTakeRowCount(List<CableData> allSignals, int maxSignalSupport, int i, int rowCount)
	{
		int num = 0;
		int num2 = 0;
		for (int j = 0; j < allSignals.Skip(rowCount).ToList().Count; j++)
		{
			if (num == maxSignalSupport)
			{
				break;
			}
			if (num > maxSignalSupport)
			{
				num2--;
				break;
			}
			num += ((allSignals.Skip(rowCount).ToList()[j].起点接线点1 != "/") ? 1 : 0);
			num += ((allSignals.Skip(rowCount).ToList()[j].起点接线点2 != "/") ? 1 : 0);
			num2++;
		}
		return num2;
	}

	/// <summary>
	/// 将旧版本数据表转换为IoFullData对象集合
	/// 根据不同控制系统的字段映射配置，智能转换数据类型和值
	/// 支持缺失字段检测和额外字段提示，确保数据转换的完整性
	/// 使用并发集合提高大数据量处理性能
	/// </summary>
	/// <param name="dataTable">源数据表，包含旧版本格式的IO数据</param>
	/// <param name="msg">消息服务，用于显示转换过程中的提示信息</param>
	/// <param name="db">SqlSugar数据库上下文，用于查询字段映射配置</param>
	/// <param name="controlSystem">目标控制系统类型</param>
	/// <returns>返回转换后的IoFullData对象集合</returns>
	public static IEnumerable<IoFullData> ConvertOldDataTableToIoFullData(this DataTable dataTable, IMessageService msg, SqlSugarClient db, ControlSystem controlSystem)
	{
		List<config_controlSystem_mapping> list = new List<config_controlSystem_mapping>();
		list = controlSystem switch
		{
			ControlSystem.龙鳍 => (from it in db.Queryable<config_controlSystem_mapping>()
				where !string.IsNullOrEmpty(it.LqOld) && !string.IsNullOrEmpty(it.StdField)
				select it).ToList(), 
			ControlSystem.中控 => (from it in db.Queryable<config_controlSystem_mapping>()
				where !string.IsNullOrEmpty(it.ZkOld) && !string.IsNullOrEmpty(it.StdField)
				select it).ToList(), 
			ControlSystem.龙核 => (from it in db.Queryable<config_controlSystem_mapping>()
				where !string.IsNullOrEmpty(it.LhOld) && !string.IsNullOrEmpty(it.StdField)
				select it).ToList(), 
			ControlSystem.一室 => (from it in db.Queryable<config_controlSystem_mapping>()
				where !string.IsNullOrEmpty(it.Xt1Old) && !string.IsNullOrEmpty(it.StdField)
				select it).ToList(), 
			ControlSystem.安全级模拟系统 => (from it in db.Queryable<config_controlSystem_mapping>()
				where !string.IsNullOrEmpty(it.AQJMNOld) && !string.IsNullOrEmpty(it.StdField)
				select it).ToList(), 
			_ => (from it in db.Queryable<config_controlSystem_mapping>()
				where !string.IsNullOrEmpty(it.StdField)
				select it).ToList(), 
		};
		Dictionary<string, PropertyInfo> propertyInfos = (from p in typeof(IoFullData).GetProperties()
			where p.CanWrite
			select new
			{
				Property = p,
				DisplayName = (p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name)
			}).ToDictionary(p => p.DisplayName, p => p.Property);
		Dictionary<string, string> dictionary = (from mapping in list
			where !string.IsNullOrEmpty(mapping.StdField)
			group mapping by mapping.StdField).ToDictionary((IGrouping<string, config_controlSystem_mapping> group) => group.Key, (IGrouping<string, config_controlSystem_mapping> group) => group.Select(delegate(config_controlSystem_mapping mapping)
		{
			switch (controlSystem)
			{
			case ControlSystem.龙鳍:
				if (!string.IsNullOrEmpty(mapping.LqOld))
				{
					return mapping.LqOld;
				}
				break;
			case ControlSystem.中控:
				if (!string.IsNullOrEmpty(mapping.ZkOld))
				{
					return mapping.ZkOld;
				}
				break;
			case ControlSystem.龙核:
				if (!string.IsNullOrEmpty(mapping.LhOld))
				{
					return mapping.LhOld;
				}
				break;
			case ControlSystem.一室:
				if (!string.IsNullOrEmpty(mapping.Xt1Old))
				{
					return mapping.Xt1Old;
				}
				break;
			case ControlSystem.安全级模拟系统:
				if (!string.IsNullOrEmpty(mapping.AQJMNOld))
				{
					return mapping.AQJMNOld;
				}
				break;
			}
			return mapping.StdField;
		}).First());
		var list2 = (from kvp in dictionary
			select new
			{
				StdField = kvp.Key,
				ColumnName = kvp.Value,
				ColumnIndex = (dataTable.Columns.Contains(kvp.Value) ? dataTable.Columns[kvp.Value].Ordinal : (-1)),
				Property = (propertyInfos.ContainsKey(kvp.Key) ? propertyInfos[kvp.Key] : null)
			} into x
			where x.Property != null
			select x).ToList();
		ConcurrentBag<IoFullData> concurrentBag = new ConcurrentBag<IoFullData>();
		List<string> list3 = new List<string>();
		List<string> list4 = new List<string>();
		foreach (DataColumn column in dataTable.Columns)
		{
			if (!dictionary.ContainsValue(column.ColumnName))
			{
				list4.Add(column.ColumnName);
			}
		}
		foreach (DataRow item in dataTable.AsEnumerable())
		{
			IoFullData ioFullData = new IoFullData();
			bool flag = false;
			foreach (var item2 in list2)
			{
				if (item2.ColumnIndex >= 0)
				{
					string value = item[item2.ColumnIndex]?.ToString();
					if (!string.IsNullOrEmpty(value))
					{
						SetIoFullDataValue(ioFullData, item2.Property, value);
						flag = true;
					}
				}
				else
				{
					list3.Add("当前平台列名： " + item2.ColumnName);
				}
			}
			if (flag)
			{
				concurrentBag.Add(ioFullData);
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (list3.Any())
		{
			stringBuilder.AppendLine("以下配置表中的列未在Excel文件中找到：");
			foreach (string item3 in list3.Distinct())
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 1, stringBuilder2);
				handler.AppendFormatted(item3);
				stringBuilder3.AppendLine(ref handler);
			}
		}
		if (list4.Any())
		{
			stringBuilder.AppendLine("以下Excel中的列未在配置表中映射，请到数据资产中心配置表配置：");
			foreach (string item4 in list4.Distinct())
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
				handler.AppendLiteral("- ");
				handler.AppendFormatted(item4);
				stringBuilder4.AppendLine(ref handler);
			}
		}
		if (stringBuilder.Length > 0)
		{
			msg.AlertAsync(stringBuilder.ToString());
		}
		return concurrentBag;
	}

	private static void SetIoFullDataValue(IoFullData result, PropertyInfo property, string value)
	{
		Type propertyType = property.PropertyType;
		if (propertyType == typeof(string))
		{
			property.SetValue(result, value);
			return;
		}
		if (propertyType == typeof(bool))
		{
			property.SetValue(result, bool.TryParse(value, out var result2) && result2);
			return;
		}
		if (propertyType == typeof(int))
		{
			property.SetValue(result, int.TryParse(value, out var result3) ? result3 : 0);
			return;
		}
		if (propertyType == typeof(int?))
		{
			property.SetValue(result, int.TryParse(value, out var result4) ? new int?(result4) : ((int?)null));
			return;
		}
		if (propertyType == typeof(float))
		{
			property.SetValue(result, float.TryParse(value, out var result5) ? result5 : 0f);
			return;
		}
		if (propertyType == typeof(float?))
		{
			property.SetValue(result, float.TryParse(value, out var result6) ? new float?(result6) : ((float?)null));
			return;
		}
		if (propertyType == typeof(double))
		{
			property.SetValue(result, double.TryParse(value, out var result7) ? result7 : 0.0);
			return;
		}
		if (propertyType == typeof(double?))
		{
			property.SetValue(result, double.TryParse(value, out var result8) ? new double?(result8) : ((double?)null));
			return;
		}
		if (propertyType == typeof(DateTime?))
		{
			property.SetValue(result, DateTime.TryParse(value, out var result9) ? new DateTime?(result9) : ((DateTime?)null));
			return;
		}
		if (propertyType == typeof(TagType))
		{
			if (int.TryParse(value, out var result10))
			{
				property.SetValue(result, (TagType)result10);
			}
			else
			{
				property.SetValue(result, TagType.Normal);
			}
			return;
		}
		if (propertyType == typeof(Xt2NetType))
		{
			Xt2NetType xt2NetType = ((!(value == "Net1") && value == "Net2") ? Xt2NetType.Net2 : Xt2NetType.Net1);
			property.SetValue(result, xt2NetType);
			return;
		}
		if (propertyType == typeof(盘箱柜类别))
		{
			property.SetValue(result, value switch
			{
				"盘台" => 盘箱柜类别.盘台, 
				"阀箱" => 盘箱柜类别.阀箱, 
				"机柜" => 盘箱柜类别.机柜, 
				_ => throw new NotImplementedException(), 
			});
			return;
		}
		throw new NotImplementedException("No converter implemented for type '" + propertyType.Name + "'.");
	}

	public static string GetControlSystemField(SqlSugarClient db, ControlSystem controlSystem, string stdDisplayName)
	{
		config_controlSystem_mapping config_controlSystem_mapping = (from m in db.Queryable<config_controlSystem_mapping>()
			where m.StdField == stdDisplayName
			select m).ToList().FirstOrDefault();
		if (config_controlSystem_mapping == null)
		{
			throw new Exception("未在config_controlSystem_mapping表中找到" + stdDisplayName + " 标准字段");
		}
		string text = controlSystem switch
		{
			ControlSystem.龙鳍 => config_controlSystem_mapping.LqOld, 
			ControlSystem.中控 => config_controlSystem_mapping.ZkOld, 
			ControlSystem.龙核 => config_controlSystem_mapping.LhOld, 
			ControlSystem.一室 => config_controlSystem_mapping.Xt1Old, 
			_ => null, 
		};
		if (string.IsNullOrEmpty(text))
		{
			return config_controlSystem_mapping.StdField;
		}
		return text;
	}
}
