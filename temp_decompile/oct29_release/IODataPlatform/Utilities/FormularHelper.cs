using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;

namespace IODataPlatform.Utilities;

/// <summary>
/// 公式帮助类
/// 提供IO数据处理中的各种公式计算和数据转换功能
/// 包含标签名解析、电源分组、卡件编号计算等核心业务逻辑
/// 支持多种数据格式转换和复杂的字符串处理操作
/// </summary>
public class FormularHelper
{
	/// <summary>当前机笼索引，用于跟踪下一个插入板卡的机笼位置</summary>
	private int currentCageIndex;

	/// <summary>
	/// 电源供给方式分组字典
	/// 定义不同IO类型的电源分组策略，用于优化板卡布局和电源管理
	/// 支持DI、DO、AI、AO等多种信号类型的智能分组
	/// </summary>
	private Dictionary<string, string> powerSupplyGrouping = new Dictionary<string, string>
	{
		{ "DI1", "Group1" },
		{ "DI6", "Group1" },
		{ "DI2", "Group2" },
		{ "DI3", "Group2" },
		{ "DI4", "Group2" },
		{ "DI5", "Group2" },
		{ "DO1", "Group3" },
		{ "DO2", "Group3" },
		{ "DO3", "Group4" },
		{ "DO4", "Group5" },
		{ "DO5", "Group6" },
		{ "AI1", "Group7" },
		{ "AI6", "Group7" },
		{ "AI2", "Group8" },
		{ "AI3", "Group8" },
		{ "AI4", "Group8" },
		{ "AI5", "Group8" },
		{ "AI7", "Group9" },
		{ "AI8", "Group9" },
		{ "AI9", "Group9" },
		{ "P1", "Group10" },
		{ "P2", "Group10" },
		{ "P3", "Group10" },
		{ "AO1", "Group11" },
		{ "AOH", "Group12" },
		{ "AO2", "Group13" }
	};

	/// <summary>
	/// 公式帮助类
	/// 提供IO数据处理中的各种公式计算和数据转换功能
	/// 包含标签名解析、电源分组、卡件编号计算等核心业务逻辑
	/// 支持多种数据格式转换和复杂的字符串处理操作
	/// </summary>
	public FormularHelper()
	{
	}

	/// <summary>
	/// 计算IO卡件编号和板卡后缀
	/// 根据IO模块编号和指定后缀，计算生成最终的卡件编号
	/// 支持特殊的编号规则和格式化要求
	/// </summary>
	/// <param name="ioModule">IO模块编号字符串</param>
	/// <param name="lastfix">要添加的后缀字符串</param>
	/// <returns>返回计算后的完整卡件编号</returns>
	public string CalculateIoCardNumberAndBN(string ioModule, string lastfix)
	{
		if (string.IsNullOrEmpty(ioModule) || ioModule.Length < 7)
		{
			return ioModule + lastfix;
		}
		string text = ioModule.Substring(ioModule.Length - 3, 3);
		if (ioModule[6] == '1')
		{
			return (int.Parse(text) + 2).ToString("D3") + lastfix;
		}
		return text + lastfix;
	}

	public string GetTagNameSection(string tagName, int index)
	{
		if (string.IsNullOrEmpty(tagName))
		{
			return "";
		}
		if (GetEx(tagName) != "")
		{
			tagName = tagName.Remove(0, 2);
		}
		string[] array = tagName.Split('_');
		if (array != null && array.Length != 0 && array.Length <= 4)
		{
			int num = -1;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == "H" || array[i] == "HO" || array[i] == "HC" || array[i] == "SA" || array[i] == "DH")
				{
					num = i;
				}
			}
			switch (index)
			{
			case 0:
				if (num == -1)
				{
					if (array.Length > 2)
					{
						return "";
					}
					return array[0];
				}
				return array[0];
			case 1:
			{
				string text2 = "";
				if (num == -1)
				{
					if (array.Length == 2)
					{
						return array[1];
					}
					return text2;
				}
				for (int k = 1; k < num; k++)
				{
					text2 += ((k == num - 1) ? array[k] : (array[k] + "_"));
				}
				return text2;
			}
			case 2:
				if (num == -1)
				{
					return "";
				}
				return array[num];
			case 3:
			{
				string text = "";
				if (num == -1)
				{
					return text;
				}
				for (int j = num + 1; j < array.Length; j++)
				{
					text += ((j == array.Length - 1) ? array[j] : (array[j] + "_"));
				}
				return text;
			}
			}
		}
		return "";
	}

	public string GetEx(string tagName)
	{
		if (string.IsNullOrEmpty(tagName) || tagName.Length < 2)
		{
			return "";
		}
		char c = tagName[0];
		char c2 = tagName[1];
		if (RegexDao.IsEnglisCh(c.ToString()) && RegexDao.IsNumber(c2.ToString()))
		{
			return tagName.Substring(0, 2);
		}
		return "";
	}

	public int CountCharacterOccurrences(string str, char character)
	{
		int num = 0;
		foreach (char c in str)
		{
			if (c == character)
			{
				num++;
			}
		}
		return num;
	}

	public bool JudgeIsSame(string 信号位号, string 仪表功能号)
	{
		try
		{
			if (信号位号.Substring(0, 2) != 仪表功能号.Substring(0, 2))
			{
				return false;
			}
			return 信号位号.Split("-").Last() == 仪表功能号.Split("-").Last();
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static float GetDigitsAsFloat(string str)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		foreach (char c in str)
		{
			if (char.IsDigit(c))
			{
				stringBuilder.Append(c);
			}
			else if (c == '.' && !flag)
			{
				stringBuilder.Append(c);
				flag = true;
			}
		}
		return float.Parse(stringBuilder.ToString(), CultureInfo.InvariantCulture);
	}

	public List<xtes_AVI> ConvertToAviList(IEnumerable<IoFullData> substations)
	{
		return substations.Select(delegate(IoFullData s)
		{
			double result;
			bool flag = double.TryParse(s.RangeUpperLimit, out result);
			double result2;
			bool flag2 = double.TryParse(s.RangeLowerLimit, out result2);
			double num = ((!flag) ? 100.0 : result);
			double num2 = ((!flag2) ? 0.0 : result2);
			string oF = ((num < num2) ? "Err" : ((result - result2 <= 10.0) ? "3" : ((result - result2 <= 100.0) ? "2" : "1")));
			string tP = ((s.ElectricalCharacteristics.Contains("4") && s.ElectricalCharacteristics.Contains("mA")) ? "6" : (s.ElectricalCharacteristics.Contains("PT100") ? "7" : (s.ElectricalCharacteristics.Contains("TC") ? "13" : "Err")));
			return new xtes_AVI
			{
				CHN = s.Channel.ToString(),
				PN = s.TagName,
				DESC = s.Description,
				UNIT = s.EngineeringUnit,
				MU = num,
				MD = num2,
				TRAIN = "NULL",
				IH = "1",
				SYS = s.SystemCode,
				SUBNET = s.SubNet,
				SN = s.StationNumber,
				CLN = ((s.Cage - 1 == 0) ? "" : (s.Cage - 1).ToString()) + s.Slot.ToString("X"),
				MON = s.CardType,
				OF = oF,
				RG = s.RGRelatedScreen,
				TP = tP,
				SQ = "0",
				QFM = "1",
				QFIA = "0",
				LCV = "0",
				SD = "1.0",
				OLQ = "1",
				OEL = "10",
				OLT = "3",
				ALLOCATION = "0",
				ACUT = "1",
				INHIBIT = "0",
				DSEL = "0",
				DI = 1.0,
				H4AP = ((!string.IsNullOrEmpty(s.High4LimitAlarmLevel) && !string.IsNullOrEmpty(s.High4AlarmDescription) && !string.IsNullOrEmpty(s.High4LimitAlarmTag)) ? 1 : 0),
				H4 = s.High4LimitAlarmValue,
				H4LEVEL = s.High4LimitAlarmLevel,
				H4_DESC = s.High4AlarmDescription,
				H4KA = s.High4LimitAlarmTag,
				H4SI = "0",
				H4DL = "0",
				H3AP = ((!string.IsNullOrEmpty(s.High3LimitAlarmLevel) && !string.IsNullOrEmpty(s.High3AlarmDescription) && !string.IsNullOrEmpty(s.High3LimitAlarmTag)) ? 1 : 0),
				H3 = s.High3LimitAlarmValue,
				H3LEVEL = s.High3LimitAlarmLevel,
				H3_DESC = s.High3AlarmDescription,
				H3KA = s.High3LimitAlarmTag,
				H3SI = "0",
				H3DL = "0",
				H2AP = ((!string.IsNullOrEmpty(s.High2LimitAlarmLevel) && !string.IsNullOrEmpty(s.High2AlarmDescription) && !string.IsNullOrEmpty(s.High2LimitAlarmTag)) ? 1 : 0),
				H2 = s.High2LimitAlarmValue,
				H2LEVEL = s.High2LimitAlarmLevel,
				H2_DESC = s.High2AlarmDescription,
				H2KA = s.High2LimitAlarmTag,
				H2SI = "0",
				H2DL = "0",
				H1AP = ((!string.IsNullOrEmpty(s.High1LimitAlarmLevel) && !string.IsNullOrEmpty(s.High1AlarmDescription) && !string.IsNullOrEmpty(s.High1LimitAlarmTag)) ? 1 : 0),
				H1 = s.High1LimitAlarmValue,
				H1LEVEL = s.High1LimitAlarmLevel,
				H1_DESC = s.High1AlarmDescription,
				H1KA = s.High1LimitAlarmTag,
				H1SI = "0",
				H1DL = "0",
				L1AP = ((!string.IsNullOrEmpty(s.Low1LimitAlarmLevel) && !string.IsNullOrEmpty(s.Low1AlarmDescription) && !string.IsNullOrEmpty(s.Low1LimitAlarmTag)) ? 1 : 0),
				L1 = s.Low1LimitAlarmValue,
				L1LEVEL = s.Low1LimitAlarmLevel,
				L1_DESC = s.Low1AlarmDescription,
				L1KA = s.Low1LimitAlarmTag,
				L1SI = "0",
				L1DL = "0",
				L2AP = ((!string.IsNullOrEmpty(s.Low2LimitAlarmLevel) && !string.IsNullOrEmpty(s.Low2AlarmDescription) && !string.IsNullOrEmpty(s.Low2LimitAlarmTag)) ? 1 : 0),
				L2 = s.Low2LimitAlarmValue,
				L2LEVEL = s.Low2LimitAlarmLevel,
				L2_DESC = s.Low2AlarmDescription,
				L2KA = s.Low2LimitAlarmTag,
				L2SI = "0",
				L2DL = "0",
				L3AP = ((!string.IsNullOrEmpty(s.Low3LimitAlarmLevel) && !string.IsNullOrEmpty(s.Low3AlarmDescription) && !string.IsNullOrEmpty(s.Low3LimitAlarmTag)) ? 1 : 0),
				L3 = s.Low3LimitAlarmValue,
				L3LEVEL = s.Low3LimitAlarmLevel,
				L3_DESC = s.Low3AlarmDescription,
				L3KA = s.Low3LimitAlarmTag,
				L3SI = "0",
				L3DL = "0",
				L4AP = ((!string.IsNullOrEmpty(s.Low4LimitAlarmLevel) && !string.IsNullOrEmpty(s.Low4AlarmDescription) && !string.IsNullOrEmpty(s.Low4LimitAlarmTag)) ? 1 : 0),
				L4 = s.Low4LimitAlarmValue,
				L4LEVEL = s.Low4LimitAlarmLevel,
				L4_DESC = s.Low4AlarmDescription,
				L4KA = s.Low4LimitAlarmTag,
				L4SI = "0",
				L4DL = "0",
				RALM = "0",
				FILTER_TIME = "0"
			};
		}).ToList();
	}

	public List<xtes_PVI> ConvertToPviList(IEnumerable<IoFullData> substations)
	{
		return substations.Select(delegate(IoFullData s)
		{
			double result;
			bool flag = double.TryParse(s.RangeUpperLimit, out result);
			double result2;
			bool flag2 = double.TryParse(s.RangeLowerLimit, out result2);
			double num = ((!flag) ? 100.0 : result);
			double num2 = ((!flag2) ? 0.0 : result2);
			string oF = ((num < num2) ? "Err" : ((result - result2 <= 10.0) ? "3" : ((result - result2 <= 100.0) ? "2" : "1")));
			string text = ((s.ElectricalCharacteristics.Contains("4") && s.ElectricalCharacteristics.Contains("mA")) ? "6" : (s.ElectricalCharacteristics.Contains("PT100") ? "7" : (s.ElectricalCharacteristics.Contains("TC") ? "13" : "Err")));
			return new xtes_PVI
			{
				CHN = s.Channel.ToString(),
				PN = s.TagName,
				DESC = s.Description,
				UNIT = s.EngineeringUnit,
				MU = num,
				MD = num2,
				TRAIN = "NULL",
				IH = "1",
				SYS = s.SystemCode,
				SUBNET = s.SubNet,
				SN = s.StationNumber,
				CLN = ((s.Cage - 1 == 0) ? "" : (s.Cage - 1).ToString()) + s.Slot.ToString("X"),
				MON = s.CardType,
				OF = oF,
				RG = s.RGRelatedScreen,
				QFM = "1",
				QFIA = "0",
				SD = "1.0",
				OLQ = "1",
				OEL = "10",
				OLT = "3",
				PG = "1",
				ALLOCATION = "0",
				ACUT = "1",
				INHIBIT = "0",
				DSEL = "0",
				DI = 1.0,
				H4AP = "0",
				H4 = "98",
				H4LEVEL = "0",
				H4DEC = "0",
				H4SI = "0",
				H4DL = "0",
				H3AP = "0",
				H3 = "95",
				H3LEVEL = "0",
				H3DEC = "0",
				H3SI = "0",
				H3DL = "0",
				H2AP = "0",
				H2 = "90",
				H2LEVEL = "0",
				H2DEC = "0",
				H2SI = "0",
				H2DL = "0",
				H1AP = "0",
				H1 = "80",
				H1LEVEL = "0",
				H1DEC = "0",
				H1SI = "0",
				H1DL = "0",
				L1AP = "0",
				L1 = "20",
				L1LEVEL = "0",
				L1DEC = "0",
				L1SI = "0",
				L1DL = "0",
				L2AP = "0",
				L2 = "10",
				L2LEVEL = "0",
				L2DEC = "0",
				L2SI = "0",
				L2DL = "0",
				L3AP = "0",
				L3 = "5",
				L3LEVEL = "0",
				L3DEC = "0",
				L3SI = "0",
				L3DL = "0",
				L4AP = "0",
				L4 = "3",
				L4LEVEL = "0",
				L4DEC = "0",
				L4SI = "0",
				L4DL = "0",
				FREQUENCY = "33",
				MAXPW = "0",
				MINPW = "0",
				PFT = "0"
			};
		}).ToList();
	}

	public List<xtes_AVO> ConvertToAvoList(IEnumerable<IoFullData> substations)
	{
		return substations.Select(delegate(IoFullData s)
		{
			double result;
			bool flag = double.TryParse(s.RangeUpperLimit, out result);
			double result2;
			bool flag2 = double.TryParse(s.RangeLowerLimit, out result2);
			double num = ((!flag) ? 100.0 : result);
			double num2 = ((!flag2) ? 0.0 : result2);
			string oF = ((num < num2) ? "Err" : ((result - result2 <= 10.0) ? "3" : ((result - result2 <= 100.0) ? "2" : "1")));
			string text = ((s.ElectricalCharacteristics.Contains("4") && s.ElectricalCharacteristics.Contains("mA")) ? "6" : (s.ElectricalCharacteristics.Contains("PT100") ? "7" : (s.ElectricalCharacteristics.Contains("TC") ? "13" : "Err")));
			return new xtes_AVO
			{
				CHN = s.Channel.ToString(),
				PN = s.TagName,
				DESC = s.Description,
				UNIT = s.EngineeringUnit,
				MU = num,
				MD = num2,
				TRAIN = "NULL",
				IH = "1",
				SYS = s.SystemCode,
				SUBNET = s.SubNet,
				CLN = ((s.Cage - 1 == 0) ? "" : (s.Cage - 1).ToString()) + s.Slot.ToString("X"),
				MON = s.CardType,
				SN = s.StationNumber,
				OF = oF,
				TP = "1",
				FAVTYPE = "0",
				FAV = "0",
				ISOF = "17",
				RG = s.RGRelatedScreen
			};
		}).ToList();
	}

	public List<xtes_DVI> ConvertToDviList(IEnumerable<IoFullData> substations)
	{
		return substations.Select((IoFullData s) => new xtes_DVI
		{
			CHN = s.Channel.ToString(),
			PN = s.TagName,
			DESC = s.Description,
			TRAIN = "NULL",
			IH = "1",
			SYS = s.SystemCode,
			INLOG = "1",
			SUBNET = s.SubNet,
			CLN = ((s.Cage - 1 == 0) ? "" : (s.Cage - 1).ToString()) + s.Slot.ToString("X"),
			MON = s.CardType,
			SN = s.StationNumber,
			RG = s.RGRelatedScreen,
			TOC = "60",
			TVA = "60",
			TOCT = "10",
			BCT = "0",
			DBT = "5",
			QFM = "1",
			QFID = "0",
			SOE = "0",
			QUICK = "0",
			IC = "1",
			ALLOCATION = "0",
			ACUT = "1",
			AP = ((!string.IsNullOrEmpty(s.AlarmLevel) && !string.IsNullOrEmpty(s.SwitchQuantityAlarmTag) && !string.IsNullOrEmpty(s.AlarmDescription)) ? 1 : 0),
			ALMLEVEL = s.AlarmLevel,
			KA = s.SwitchQuantityAlarmTag,
			AL_DESC = s.AlarmDescription,
			AF = s.AlarmAttribute,
			DEC = "0",
			SI = "0",
			TBTYPE = "1",
			ROUT = "0",
			E1 = "1",
			E0 = "0"
		}).ToList();
	}

	public List<xtes_DVO> ConvertToDvoList(IEnumerable<IoFullData> substations)
	{
		return substations.Select((IoFullData s) => new xtes_DVO
		{
			CHN = s.Channel.ToString(),
			PN = s.TagName,
			DESC = s.Description,
			TRAIN = "NULL",
			IH = "1",
			SYS = s.SystemCode,
			INLOG = "1",
			SUBNET = s.SubNet,
			CLN = ((s.Cage - 1 == 0) ? "" : (s.Cage - 1).ToString()) + s.Slot.ToString("X"),
			MON = s.CardType,
			SN = s.StationNumber,
			FAVTYPE = "1",
			FAV = "0",
			RG = s.RGRelatedScreen
		}).ToList();
	}

	public List<xtes_GBP> ConvertToGBPList(IEnumerable<IoFullData> substations)
	{
		return substations.Select((IoFullData s) => new xtes_GBP
		{
			PN = s.TagName,
			DESC = s.Description,
			TRAIN = "NULL",
			IH = "1",
			SYS = s.SystemCode,
			POT = "0",
			SN = s.StationNumber,
			SUBNET = s.SubNet,
			RG = s.RGRelatedScreen
		}).ToList();
	}

	public List<xtes_GCP> ConvertToGCPList(IEnumerable<IoFullData> substations)
	{
		return substations.Select(delegate(IoFullData s)
		{
			double result;
			bool flag = double.TryParse(s.RangeUpperLimit, out result);
			double result2;
			bool flag2 = double.TryParse(s.RangeLowerLimit, out result2);
			double iNH = ((!flag) ? 100.0 : result);
			double iNL = ((!flag2) ? 0.0 : result2);
			return new xtes_GCP
			{
				PN = s.TagName,
				DESC = s.Description,
				TRAIN = "NULL",
				IH = "1",
				SYS = s.SystemCode,
				POT = "0",
				UNIT = s.EngineeringUnit,
				INH = iNH,
				INL = iNL,
				SN = s.StationNumber,
				SUBNET = s.SubNet,
				FRATE = "5",
				SRATE = "1",
				RG = s.RGRelatedScreen
			};
		}).ToList();
	}

	public List<xtes_GST> ConvertToGSTList(IEnumerable<IoFullData> substations)
	{
		return new List<xtes_GST>();
	}

	public List<xtes_GKC> ConvertToGKCList(IEnumerable<IoFullData> substations)
	{
		return new List<xtes_GKC>();
	}

	/// <summary>
	/// 自动IO分配
	/// </summary>
	/// <param name="iODatas"></param>
	/// <returns></returns>
	public List<IoFullData> AutoAllocateXT1IO(List<IoFullData> datas, List<config_card_type_judge> configs, double rate)
	{
		List<StdCabinet> cabinets = datas.BuildCabinetStructureXT1(configs);
		return cabinets.ToPoint();
	}

	public List<IoFullData> AutoAllocateIO(List<IoFullData> datas, List<config_card_type_judge> configs, double rate)
	{
		List<StdCabinet> list = datas.BuildCabinetStructureOther(configs);
		foreach (StdCabinet item in list)
		{
			AutoAllocateIO(item, configs, rate);
		}
		return list.ToPoint();
	}

	public StdCabinet AutoAllocateIO(StdCabinet cabinet, List<config_card_type_judge> configs, double rate)
	{
		IEnumerable<string> enumerable = (from t in cabinet.ToPoint()
			where t.PowerType != null
			select t.PowerType).Distinct();
		foreach (string item in enumerable)
		{
			if (!powerSupplyGrouping.ContainsKey(item))
			{
				powerSupplyGrouping.Add(item, item);
			}
		}
		ClearPointsAndAddToUnset(cabinet);
		List<IoFullData> list = cabinet.UnsetPoints.Where((IoFullData u) => u.PointType == TagType.Normal).ToList();
		List<IoFullData> list2 = cabinet.UnsetPoints.Where((IoFullData u) => u.PointType == TagType.Alarm).ToList();
		List<IoFullData> list3 = list.Where((IoFullData c) => c.IoType.ToUpper().Contains("FF")).ToList();
		List<IoFullData> source = list3;
		List<IoFullData> list4 = list.Where((IoFullData c) => c.IoType.ToUpper().Contains("PROFIBUS")).ToList();
		List<IoFullData> source2 = list.Except(list3).Except(list4).ToList();
		IEnumerable<IoFullData> source3 = source2.Where((IoFullData l) => string.IsNullOrEmpty(l.LocalBoxNumber));
		IEnumerable<IoFullData> source4 = source2.Where((IoFullData l) => !string.IsNullOrEmpty(l.LocalBoxNumber));
		cabinet.UnsetPoints.Clear();
		IOrderedEnumerable<IGrouping<string, IoFullData>> orderedEnumerable = from t in source3
			group t by t.IoType into g
			orderby GetIOTypeOrder(g.Key)
			select g;
		foreach (IGrouping<string, IoFullData> item2 in orderedEnumerable)
		{
			IEnumerable<IGrouping<string, IoFullData>> enumerable2 = from i in item2
				group i by i.CardType;
			foreach (IGrouping<string, IoFullData> card in enumerable2)
			{
				IEnumerable<IGrouping<string, IoFullData>> enumerable3 = from tag in card
					group tag by GetGroupName(tag.PowerType);
				config_card_type_judge config_card_type_judge = configs.FirstOrDefault((config_card_type_judge c) => c.IoCardType == card.Key);
				if (config_card_type_judge == null)
				{
					throw new Exception("IO分配遇到问题，未在IO卡型号配置表中找到IO卡型号为：" + card.Key + "的板卡，请手动添加后再进行分配");
				}
				foreach (IGrouping<string, IoFullData> item3 in enumerable3)
				{
					AllocateTagToSameTypeCard(cabinet, card.Key, item3.Key, item3.ToList(), config_card_type_judge, rate);
				}
			}
		}
		IOrderedEnumerable<IGrouping<string, IoFullData>> orderedEnumerable2 = from t in source4
			group t by t.IoType into g
			orderby GetIOTypeOrder(g.Key)
			select g;
		foreach (IGrouping<string, IoFullData> item4 in orderedEnumerable2)
		{
			IEnumerable<IGrouping<string, IoFullData>> enumerable4 = from i in item4
				group i by i.LocalBoxNumber;
			foreach (IGrouping<string, IoFullData> item5 in enumerable4)
			{
				string cardType = item5.ToList().FirstOrDefault().CardType;
				config_card_type_judge config_card_type_judge2 = configs.FirstOrDefault((config_card_type_judge c) => c.IoCardType == cardType);
				if (config_card_type_judge2 == null)
				{
					throw new Exception("IO分配遇到问题，未在IO卡型号配置表中找到IO卡型号为：" + cardType + "的板卡，请手动添加后再进行分配");
				}
				AllocateToCardForBoxA(cabinet, cardType, item5.ToList(), item5.Key, config_card_type_judge2, rate);
			}
		}
		IEnumerable<IGrouping<string, IoFullData>> enumerable5 = from f in list4
			group f by f.LocalBoxNumber;
		foreach (IGrouping<string, IoFullData> item6 in enumerable5)
		{
			string cardType2 = item6.ToList().FirstOrDefault().CardType;
			config_card_type_judge config_card_type_judge3 = configs.FirstOrDefault((config_card_type_judge c) => c.IoCardType == cardType2);
			if (config_card_type_judge3 == null)
			{
				throw new Exception("IO分配遇到问题，未在IO卡型号配置表中找到IO卡型号为：" + cardType2 + "的板卡，请手动添加后再进行分配");
			}
			AllocateToCard(cabinet, cardType2, item6.ToList(), item6.Key, config_card_type_judge3, rate);
		}
		IOrderedEnumerable<IGrouping<string, IoFullData>> orderedEnumerable3 = from f in source
			group f by f.LocalBoxNumber into f
			orderby f.Key
			select f;
		List<List<IoFullData>> list5 = new List<List<IoFullData>>();
		foreach (IGrouping<string, IoFullData> item7 in orderedEnumerable3)
		{
			string cardType3 = item7.ToList().FirstOrDefault().CardType;
			config_card_type_judge config_card_type_judge4 = configs.FirstOrDefault((config_card_type_judge c) => c.IoCardType == cardType3);
			if (config_card_type_judge4 == null)
			{
				throw new Exception("IO分配遇到问题，未在IO卡型号配置表中找到IO卡型号为：" + cardType3 + "的板卡，请手动添加后再进行分配");
			}
			List<IoFullData> list6 = item7.ToList();
			AllocateToCardFF(cabinet, cardType3, item7.ToList(), item7.Key, config_card_type_judge4, rate);
		}
		if (list2 != null && list2.Count > 0)
		{
			IEnumerable<IGrouping<string, IoFullData>> enumerable6 = from a in list2
				group a by a.CardType;
			foreach (IGrouping<string, IoFullData> card2 in enumerable6)
			{
				bool flag = cabinet.Cages.Any((ChassisInfo cage) => cage.Slots.Any((SlotInfo slot) => slot.Board == null));
				IEnumerable<Board> enumerable7 = from b in cabinet.Cages.SelectMany((ChassisInfo cage) => cage.Slots.Select((SlotInfo slot) => slot.Board))
					where b == null
					select b;
				if (flag)
				{
					config_card_type_judge type = configs.FirstOrDefault((config_card_type_judge c) => c.IoCardType == card2.Key);
					Board board = Board.Create(type);
					SetBoard(cabinet, board);
					foreach (IoFullData item8 in card2.ToList())
					{
						Xt2Channel xt2Channel = board.Channels.FirstOrDefault((Xt2Channel c) => c.Point == null);
						if (xt2Channel != null)
						{
							xt2Channel.Point = item8;
						}
						else
						{
							cabinet.UnsetPoints.Add(item8);
						}
					}
				}
				else
				{
					string groupName = GetGroupName(card2.FirstOrDefault().PowerType);
					config_card_type_judge config_card_type_judge5 = configs.FirstOrDefault((config_card_type_judge c) => c.IoCardType == card2.Key);
					if (config_card_type_judge5 == null)
					{
						throw new Exception("IO分配遇到问题，未在配置表中找到" + card2.Key + "，请手动添加后再进行分配");
					}
					AllocateTagToSameTypeCard(cabinet, card2.Key, groupName, card2.ToList(), config_card_type_judge5, rate);
				}
			}
		}
		return cabinet;
	}

	public void ReassignPoints(IEnumerable<Board> boards)
	{
		if (boards == null || boards.Count() == 0)
		{
			return;
		}
		foreach (Board board in boards)
		{
			List<IoFullData> first = (from c in board.Channels
				where c.Point != null && c.Point.NetType == Xt2NetType.Net1.ToString()
				select c.Point).ToList();
			List<IoFullData> second = (from c in board.Channels.Where((Xt2Channel c) => c.Point != null && c.Point.NetType == Xt2NetType.Net2.ToString()).Reverse()
				select c.Point).ToList();
			List<IoFullData> list = first.Concat(second).ToList();
			int num = 0;
			foreach (Xt2Channel channel in board.Channels)
			{
				if (num < list.Count)
				{
					channel.Point = list[num++];
				}
				else
				{
					channel.Point = null;
				}
			}
		}
	}

	private void AllocateTagToSameTypeCard(StdCabinet xt2structure, string cardType, string powerTypeValue, List<IoFullData> powerTypeTags, config_card_type_judge config, double rate)
	{
		foreach (IGrouping<string, IoFullData> item in from tag in powerTypeTags
			group tag by tag.SignalPositionNumber)
		{
			List<IoFullData> list = item.ToList();
			bool flag = false;
			IEnumerable<Board> enumerable = (from board2 in xt2structure.Cages.SelectMany((ChassisInfo cage) => cage.Slots.Select((SlotInfo slot) => slot.Board))
				where board2 != null && board2.Type == cardType
				select board2).Union<Board>(from vs in xt2structure.VirtualSlots
				where vs.Board != null && vs.Board.Type == cardType
				select vs.Board);
			foreach (Board item2 in enumerable)
			{
				int num = item2.Channels.Count - (int)Math.Ceiling((double)item2.Channels.Count * rate) - item2.Channels.Count((Xt2Channel c) => c.Point != null);
				List<IoFullData> source = (from c in item2.Channels
					where c.Point != null
					select c.Point).ToList();
				bool flag2 = true;
				if (source.Any() && list.Any())
				{
					string cardPowerType = source.First().PowerType;
					string value = powerSupplyGrouping.FirstOrDefault<KeyValuePair<string, string>>((KeyValuePair<string, string> x) => x.Key == cardPowerType).Value;
					flag2 = value == powerTypeValue;
				}
				if (!flag2 || num < list.Count)
				{
					continue;
				}
				foreach (IoFullData item3 in item)
				{
					Xt2Channel xt2Channel = item2.Channels.FirstOrDefault((Xt2Channel c) => c.Point == null);
					if (xt2Channel != null)
					{
						xt2Channel.Point = item3;
					}
				}
				flag = true;
				break;
			}
			if (flag)
			{
				continue;
			}
			Board board = Board.Create(config);
			SetBoard(xt2structure, board);
			foreach (IoFullData item4 in item)
			{
				Xt2Channel xt2Channel2 = board.Channels.FirstOrDefault((Xt2Channel c) => c.Point == null);
				if (xt2Channel2 != null)
				{
					xt2Channel2.Point = item4;
					continue;
				}
				PlacePointToUnset(xt2structure, item4, $"硬接点点数量超出{board.Channels.Count}");
			}
		}
	}

	/// <summary>
	/// 【已废弃】FF从站模块独立分配方法
	/// 【注释原因】现在已合并到AllocateToCardFF方法中，不再单独处理FF7/FF8从站模块
	/// 原为为FF7和FF8从站模块提供板卡分配逻辑，保持FF模块的网段分配特性
	/// FF从站模块之间可以复用板卡，但不与FF总线模块（FF1-FF6）复用同一块板卡
	/// 适用于从站系统的板卡分配、网段管理、资源优化配置
	/// </summary>
	/// <param name="xt2structure">机柜结构对象，包含机笼、插槽和板卡信息</param>
	/// <param name="cardType">板卡类型，用于确定板卡的类型和规格</param>
	/// <param name="stationTags">从站模块的IO数据列表，包含所有需要分配的信号点</param>
	/// <param name="stationNumber">从站编号，用于标识和管理从站设备</param>
	/// <param name="config">板卡配置信息，包含通道数量等参数</param>
	/// <param name="rate">冗余率，用于计算板卡可用通道数</param>
	/// <exception cref="T:System.Exception">当从站模块信号数量超出板卡网段通道数时抛出异常</exception>
	[Obsolete("该方法已废弃，现在使用AllocateToCardFF方法统一处理所有FF模块", true)]
	private void AllocateToCardForFFSlave(StdCabinet xt2structure, string cardType, List<IoFullData> stationTags, string stationNumber, config_card_type_judge config, double rate)
	{
		bool flag = false;
		IEnumerable<Board> enumerable = (from board2 in xt2structure.Cages.SelectMany((ChassisInfo cage) => cage.Slots.Select((SlotInfo slot) => slot.Board))
			where board2 != null && board2.Type == cardType && IsFFSlaveBoard(board2)
			select board2).Union<Board>(from vs in xt2structure.VirtualSlots
			where vs.Board != null && vs.Board.Type == cardType && IsFFSlaveBoard(vs.Board)
			select vs.Board);
		foreach (Board item in enumerable)
		{
			int count = item.Channels.Count / 2;
			List<Xt2Channel> list = item.Channels.Take(count).ToList();
			List<Xt2Channel> list2 = item.Channels.Skip(count).ToList();
			int num = list.Count - (int)Math.Ceiling((double)list.Count * rate) - list.Count((Xt2Channel c) => c.Point != null);
			int num2 = list2.Count - (int)Math.Ceiling((double)list2.Count * rate) - list2.Count((Xt2Channel c) => c.Point != null);
			List<IoFullData> source = (from c in list
				where c.Point != null
				select c.Point).ToList();
			List<IoFullData> source2 = (from c in list2
				where c.Point != null
				select c.Point).ToList();
			bool flag2 = source.Any((IoFullData p) => p != null && p.NetType == Xt2NetType.Net1.ToString() && !string.IsNullOrEmpty(p.Remarks) && (p.Remarks.Contains("串") || (!string.IsNullOrEmpty(stationNumber) && p.Remarks.Contains(stationNumber))));
			bool flag3 = source2.Any((IoFullData p) => p != null && p.NetType == Xt2NetType.Net2.ToString() && !string.IsNullOrEmpty(p.Remarks) && (p.Remarks.Contains("串") || (!string.IsNullOrEmpty(stationNumber) && p.Remarks.Contains(stationNumber))));
			if (flag2 && num >= stationTags.Count)
			{
				foreach (IoFullData stationTag in stationTags)
				{
					stationTag.NetType = Xt2NetType.Net1.ToString();
					Xt2Channel xt2Channel = list.FirstOrDefault((Xt2Channel c) => c.Point == null);
					if (xt2Channel != null)
					{
						xt2Channel.Point = stationTag;
					}
				}
				flag = true;
				break;
			}
			if (!(list2.All((Xt2Channel p) => p.Point == null) || flag3) || num2 < stationTags.Count)
			{
				continue;
			}
			foreach (IoFullData stationTag2 in stationTags)
			{
				stationTag2.NetType = Xt2NetType.Net2.ToString();
				Xt2Channel xt2Channel2 = list2.FirstOrDefault((Xt2Channel c) => c.Point == null);
				if (xt2Channel2 != null)
				{
					xt2Channel2.Point = stationTag2;
				}
			}
			flag = true;
			break;
		}
		if (flag)
		{
			return;
		}
		Board board = Board.Create(config);
		SetBoard(xt2structure, board);
		int num3 = board.Channels.Count / 2;
		List<Xt2Channel> source3 = board.Channels.Take(num3).ToList();
		foreach (IoFullData stationTag3 in stationTags)
		{
			stationTag3.NetType = Xt2NetType.Net1.ToString();
			Xt2Channel xt2Channel3 = source3.FirstOrDefault((Xt2Channel c) => c.Point == null);
			if (xt2Channel3 != null)
			{
				xt2Channel3.Point = stationTag3;
				continue;
			}
			PlacePointToUnset(xt2structure, stationTag3, $"FF从站模块{stationNumber}网段1数量超出{num3}");
		}
	}

	/// <summary>
	/// 【已废弃】判断板卡是否为FF从站模块创建的板卡
	/// 【注释原因】现在所有FF模块统一处理，不再区分FF从站和FF总线板卡
	/// 原通过检查板卡上的信号点的供电类型来判断是否为FF从站板卡
	/// 适用于板卡复用判断、资源管理、系统分类
	/// </summary>
	/// <param name="board">需要判断的板卡对象</param>
	/// <returns>如果是FF从站板卡返回true，否则返回false</returns>
	[Obsolete("该方法已废弃，现在所有FF模块统一处理不再区分类型", true)]
	private bool IsFFSlaveBoard(Board board)
	{
		return board.Channels.Any((Xt2Channel c) => c.Point != null && c.Point.PowerType != null && (c.Point.PowerType.Contains("FF7") || c.Point.PowerType.Contains("FF8")));
	}

	private void AllocateToCardForBoxA(StdCabinet xt2structure, string cardType, List<IoFullData> stationTags, string stationNumber, config_card_type_judge config, double rate)
	{
		bool flag = false;
		IEnumerable<Board> enumerable = (from board2 in xt2structure.Cages.SelectMany((ChassisInfo cage) => cage.Slots.Select((SlotInfo slot) => slot.Board))
			where board2 != null && board2.Type == cardType
			select board2).Union<Board>(from vs in xt2structure.VirtualSlots
			where vs.Board != null && vs.Board.Type == cardType
			select vs.Board);
		foreach (Board item in enumerable)
		{
			int num = item.Channels.Count - (int)Math.Ceiling((double)item.Channels.Count * rate) - item.Channels.Count((Xt2Channel c) => c.Point != null);
			List<IoFullData> source = (from c in item.Channels
				where c.Point != null
				select c.Point).ToList();
			bool flag2 = true;
			if (source.Any() && stationTags.Any())
			{
				string cardPowerType = source.First().PowerType;
				string cardGroup = powerSupplyGrouping.FirstOrDefault<KeyValuePair<string, string>>((KeyValuePair<string, string> x) => x.Key == cardPowerType).Value;
				flag2 = stationTags.All(delegate(IoFullData tag)
				{
					string value = powerSupplyGrouping.FirstOrDefault<KeyValuePair<string, string>>((KeyValuePair<string, string> x) => x.Key == tag.PowerType).Value;
					return value == cardGroup;
				});
			}
			if (!flag2 || num < stationTags.Count)
			{
				continue;
			}
			foreach (IoFullData stationTag in stationTags)
			{
				Xt2Channel xt2Channel = item.Channels.FirstOrDefault((Xt2Channel c) => c.Point == null);
				if (xt2Channel != null)
				{
					xt2Channel.Point = stationTag;
				}
			}
			flag = true;
			break;
		}
		if (flag)
		{
			return;
		}
		Board board = Board.Create(config);
		SetBoard(xt2structure, board);
		foreach (IoFullData stationTag2 in stationTags)
		{
			Xt2Channel xt2Channel2 = board.Channels.FirstOrDefault((Xt2Channel c) => c.Point == null);
			if (xt2Channel2 != null)
			{
				xt2Channel2.Point = stationTag2;
				continue;
			}
			PlacePointToUnset(xt2structure, stationTag2, $"A类阀箱点数量超出{board.Channels.Count}");
		}
	}

	private void AllocateToCard(StdCabinet xt2structure, string cardType, List<IoFullData> stationTags, string stationNumber, config_card_type_judge config, double rate)
	{
		bool flag = false;
		IEnumerable<Board> enumerable = (from board2 in xt2structure.Cages.SelectMany((ChassisInfo cage) => cage.Slots.Select((SlotInfo slot) => slot.Board))
			where board2 != null && board2.Type == cardType
			select board2).Union<Board>(from vs in xt2structure.VirtualSlots
			where vs.Board != null && vs.Board.Type == cardType
			select vs.Board);
		foreach (Board item in enumerable)
		{
			int num = item.Channels.Count - (int)Math.Ceiling((double)item.Channels.Count * rate) - item.Channels.Count((Xt2Channel c) => c.Point != null);
			List<IoFullData> pointsOnCard = (from c in item.Channels
				where c.Point != null
				select c.Point).ToList();
			if ((!pointsOnCard.Any((IoFullData p) => p != null && !string.IsNullOrEmpty(p.Remarks) && (p.Remarks.Contains("串联") || p.Remarks.Contains(stationNumber))) && !stationTags.Any((IoFullData tag) => !string.IsNullOrEmpty(tag.Remarks) && (tag.Remarks.Contains("串联") || pointsOnCard.Any((IoFullData p) => p != null && tag.Remarks.Contains(p.StationNumber))))) || num < stationTags.Count)
			{
				continue;
			}
			foreach (IoFullData stationTag in stationTags)
			{
				Xt2Channel xt2Channel = item.Channels.FirstOrDefault((Xt2Channel c) => c.Point == null);
				if (xt2Channel != null)
				{
					xt2Channel.Point = stationTag;
				}
			}
			flag = true;
			break;
		}
		if (flag)
		{
			return;
		}
		Board board = Board.Create(config);
		SetBoard(xt2structure, board);
		foreach (IoFullData stationTag2 in stationTags)
		{
			Xt2Channel xt2Channel2 = board.Channels.FirstOrDefault((Xt2Channel c) => c.Point == null);
			if (xt2Channel2 != null)
			{
				xt2Channel2.Point = stationTag2;
				continue;
			}
			PlacePointToUnset(xt2structure, stationTag2, $"B类阀箱点数量超出{board.Channels.Count}");
		}
	}

	/// <summary>
	/// FF模块统一分配方法
	/// 【修改】为所有FF模块提供统一的板卡分配逻辑，包括FF总线模块（FF1-FF6）和FF从站模块（FF7-FF8）
	/// 支持FF模块之间的板卡复用，一个箱子分配到一个网段上
	/// 适用于FF总线系统的板卡分配、网段管理、资源优化配置
	/// </summary>
	/// <param name="xt2structure">机柜结构对象，包含机笼、插槽和板卡信息</param>
	/// <param name="cardType">板卡类型，用于确定板卡的类型和规格</param>
	/// <param name="stationTags">FF模块的IO数据列表，包含所有需要分配的信号点</param>
	/// <param name="stationNumber">站号，用于标识和管理FF设备</param>
	/// <param name="config">板卡配置信息，包含通道数量等参数</param>
	/// <param name="rate">冗余率，用于计算板卡可用通道数</param>
	private void AllocateToCardFF(StdCabinet xt2structure, string cardType, List<IoFullData> stationTags, string stationNumber, config_card_type_judge config, double rate)
	{
		bool flag = false;
		IEnumerable<Board> enumerable = (from board2 in xt2structure.Cages.SelectMany((ChassisInfo cage) => cage.Slots.Select((SlotInfo slot) => slot.Board))
			where board2 != null && board2.Type == cardType
			select board2).Union<Board>(from vs in xt2structure.VirtualSlots
			where vs.Board != null && vs.Board.Type == cardType
			select vs.Board);
		foreach (Board item in enumerable)
		{
			int count = item.Channels.Count / 2;
			List<Xt2Channel> list = item.Channels.Take(count).ToList();
			List<Xt2Channel> list2 = item.Channels.Skip(count).ToList();
			int num = list.Count - (int)Math.Ceiling((double)list.Count * rate) - list.Count((Xt2Channel c) => c.Point != null);
			int num2 = list2.Count - (int)Math.Ceiling((double)list2.Count * rate) - list2.Count((Xt2Channel c) => c.Point != null);
			List<IoFullData> source = (from c in list
				where c.Point != null
				select c.Point).ToList();
			List<IoFullData> source2 = (from c in list2
				where c.Point != null
				select c.Point).ToList();
			bool flag2 = source.Any((IoFullData p) => p != null && p.NetType == Xt2NetType.Net1.ToString() && !string.IsNullOrEmpty(p.Remarks) && (p.Remarks.Contains("串") || (!string.IsNullOrEmpty(stationNumber) && p.Remarks.Contains(stationNumber))));
			bool flag3 = source2.Any((IoFullData p) => p != null && p.NetType == Xt2NetType.Net2.ToString() && !string.IsNullOrEmpty(p.Remarks) && (p.Remarks.Contains("串") || (!string.IsNullOrEmpty(stationNumber) && p.Remarks.Contains(stationNumber))));
			if (flag2 && num >= stationTags.Count)
			{
				foreach (IoFullData stationTag in stationTags)
				{
					if (!string.IsNullOrEmpty(stationTag.IoType) && stationTag.IoType.ToUpper().Contains("FF"))
					{
						stationTag.NetType = Xt2NetType.Net1.ToString();
					}
					Xt2Channel xt2Channel = list.FirstOrDefault((Xt2Channel c) => c.Point == null);
					if (xt2Channel != null)
					{
						xt2Channel.Point = stationTag;
					}
				}
				flag = true;
				break;
			}
			if (!(list2.All((Xt2Channel p) => p.Point == null) || flag3) || num2 < stationTags.Count)
			{
				continue;
			}
			foreach (IoFullData stationTag2 in stationTags)
			{
				if (!string.IsNullOrEmpty(stationTag2.IoType) && stationTag2.IoType.ToUpper().Contains("FF"))
				{
					stationTag2.NetType = Xt2NetType.Net2.ToString();
				}
				Xt2Channel xt2Channel2 = list2.FirstOrDefault((Xt2Channel c) => c.Point == null);
				if (xt2Channel2 != null)
				{
					xt2Channel2.Point = stationTag2;
				}
			}
			flag = true;
			break;
		}
		if (flag)
		{
			return;
		}
		Board board = Board.Create(config);
		SetBoard(xt2structure, board);
		foreach (IoFullData stationTag3 in stationTags)
		{
			int num3 = board.Channels.Count / 2;
			List<Xt2Channel> source3 = board.Channels.Take(num3).ToList();
			if (!string.IsNullOrEmpty(stationTag3.IoType) && stationTag3.IoType.ToUpper().Contains("FF"))
			{
				stationTag3.NetType = Xt2NetType.Net1.ToString();
			}
			Xt2Channel xt2Channel3 = source3.FirstOrDefault((Xt2Channel c) => c.Point == null);
			if (xt2Channel3 != null)
			{
				xt2Channel3.Point = stationTag3;
				continue;
			}
			PlacePointToUnset(xt2structure, stationTag3, $"网段1数量超出{num3}");
		}
	}

	public void ClearPointsAndAddToUnset(StdCabinet cabinet)
	{
		foreach (ChassisInfo cage in cabinet.Cages)
		{
			foreach (SlotInfo slot in cage.Slots)
			{
				if (slot.Board == null)
				{
					continue;
				}
				foreach (Xt2Channel channel in slot.Board.Channels)
				{
					if (channel.Point != null)
					{
						cabinet.UnsetPoints.Add(channel.Point);
						channel.Point = null;
					}
				}
				slot.Board = null;
			}
		}
		foreach (SlotInfo virtualSlot in cabinet.VirtualSlots)
		{
			if (virtualSlot.Board == null)
			{
				continue;
			}
			foreach (Xt2Channel channel2 in virtualSlot.Board.Channels)
			{
				if (channel2.Point != null)
				{
					cabinet.UnsetPoints.Add(channel2.Point);
					channel2.Point = null;
				}
			}
		}
		cabinet.VirtualSlots.Clear();
		foreach (IoFullData unsetPoint in cabinet.UnsetPoints)
		{
			unsetPoint.Cage = 0;
			unsetPoint.Slot = 0;
			unsetPoint.Channel = 0;
		}
	}

	public void PlacePointToUnset(StdCabinet cabinet, IoFullData tag, string reason)
	{
		tag.UnsetReason = reason;
		cabinet.UnsetPoints.Add(tag);
	}

	private void SetBoard(StdCabinet cabinetInfo, Board board)
	{
		bool flag = board.Type.Contains("FF");
		for (int i = 0; i < cabinetInfo.Cages.Count; i++)
		{
			ChassisInfo chassisInfo = cabinetInfo.Cages[(currentCageIndex + i) % cabinetInfo.Cages.Count];
			foreach (SlotInfo slot in chassisInfo.Slots)
			{
				if (slot.Board == null)
				{
					if (!flag)
					{
						slot.Board = board;
						currentCageIndex = (currentCageIndex + i + 1) % cabinetInfo.Cages.Count;
						return;
					}
					int num = slot.Index + 1;
					if (slot.Index % 2 == 1 && (chassisInfo.Slots.Count <= num || chassisInfo.Slots[num].Board == null))
					{
						slot.Board = board;
						currentCageIndex = (currentCageIndex + i + 1) % cabinetInfo.Cages.Count;
						return;
					}
				}
			}
		}
		cabinetInfo.AddBoardToVirtualSlot(board);
	}

	private int GetIOTypeOrder(string ioType)
	{
		List<string> list = new List<string> { "AI", "PI", "AO", "DI", "DO" };
		return list.IndexOf(ioType);
	}

	private string GetGroupName(string powerType)
	{
		if (string.IsNullOrEmpty(powerType))
		{
			throw new Exception("供电类型有空值，无法分配");
		}
		if (powerSupplyGrouping.ContainsKey(powerType))
		{
			return powerSupplyGrouping[powerType];
		}
		if (powerSupplyGrouping.Keys.Any((string k) => powerType.Contains(k)))
		{
			return powerSupplyGrouping[powerSupplyGrouping.Keys.FirstOrDefault((string p) => powerType.Contains(p))];
		}
		return "GROUP0";
	}

	public StdCabinet AutoAllocateLongHeIOSingle(StdCabinet acabinet, List<config_card_type_judge> configs, double rate)
	{
		List<IoFullData> source = acabinet.ToPoint();
		StdCabinet stdCabinet = StdCabinet.CreateLH(acabinet.Name);
		Dictionary<string, int> ioTypeOrder = new Dictionary<string, int>
		{
			{ "AI", 1 },
			{ "DI", 2 },
			{ "AO", 3 },
			{ "DO", 4 }
		};
		int value;
		List<IoFullData> source2 = source.OrderBy((IoFullData sg) => (!ioTypeOrder.TryGetValue(sg.IoType, out value)) ? int.MaxValue : value).ToList();
		List<string> feedbackKeywords = new List<string> { "KW", "GW", "QF", "TF", "GZ" };
		var orderedEnumerable = from s in source2
			group s by new
			{
				IoType = s.IoType,
				CardType = s.CardType,
				PowerSupplyMethod = s.PowerSupplyMethod,
				VoltageLevel = s.VoltageLevel,
				Destination = s.Destination,
				DevicePrefix = GetDevicePrefix(s.TagName)
			} into g
			orderby (!ioTypeOrder.TryGetValue(g.Key.IoType, out value)) ? int.MaxValue : value, g.Any((IoFullData s) => feedbackKeywords.Any((string keyword) => s.TagName.Contains(keyword))) descending
			select g;
		foreach (var signalGroup in orderedEnumerable)
		{
			List<IoFullData> list = signalGroup.OrderBy((IoFullData s) => s.TagName).ToList();
			config_card_type_judge config_card_type_judge = configs.SingleOrDefault((config_card_type_judge c) => c.IoCardType == signalGroup.Key.CardType);
			if (config_card_type_judge == null)
			{
				throw new Exception("未在配置表中找到" + signalGroup.Key.CardType + ",请配置后再生成");
			}
			List<IoFullData> feedbackSignals = list.Where((IoFullData s) => feedbackKeywords.Any((string keyword) => s.TagName.Contains(keyword))).ToList();
			List<IoFullData> otherSignals = list.Except(feedbackSignals).ToList();
			if (feedbackSignals.Any() && !(from sc in stdCabinet.Cages.SelectMany((ChassisInfo c) => c.Slots.Select((SlotInfo slot) => new { slot, c }))
				where sc.slot.Board?.Type == signalGroup.Key.CardType && !IsLastTwoSlots(sc.c, sc.slot)
				select sc).Any(sc => AssignGroupToSlot(sc.slot, sc.c, feedbackSignals, rate, signalGroup.Key.PowerSupplyMethod, signalGroup.Key.VoltageLevel, signalGroup.Key.Destination)))
			{
				AssignToNewSlotOrUnset(stdCabinet, config_card_type_judge, feedbackSignals);
			}
			if (otherSignals.Any() && !(from sc in stdCabinet.Cages.SelectMany((ChassisInfo c) => c.Slots.Select((SlotInfo slot) => new { slot, c }))
				where sc.slot.Board?.Type == signalGroup.Key.CardType && !IsLastTwoSlots(sc.c, sc.slot)
				select sc).Any(sc => AssignGroupToSlot(sc.slot, sc.c, otherSignals, rate, signalGroup.Key.PowerSupplyMethod, signalGroup.Key.VoltageLevel, signalGroup.Key.Destination)))
			{
				AssignToNewSlotOrUnset(stdCabinet, config_card_type_judge, otherSignals);
			}
		}
		if (stdCabinet.UnsetPoints.Any())
		{
			AssignRemainingSignals(stdCabinet, rate);
		}
		return stdCabinet;
	}

	public List<StdCabinet> AutoAllocateLongHeIO(List<IoFullData> fullDatas, List<config_card_type_judge> configs, double rate)
	{
		List<StdCabinet> list = new List<StdCabinet>();
		foreach (IGrouping<string, IoFullData> item in from f in fullDatas
			group f by f.CabinetNumber)
		{
			StdCabinet stdCabinet = StdCabinet.CreateLH(item.Key);
			list.Add(stdCabinet);
			Dictionary<string, int> ioTypeOrder = new Dictionary<string, int>
			{
				{ "AI", 1 },
				{ "DI", 2 },
				{ "AO", 3 },
				{ "DO", 4 }
			};
			int value;
			List<IoFullData> source = item.OrderBy((IoFullData sg) => (!ioTypeOrder.TryGetValue(sg.IoType, out value)) ? int.MaxValue : value).ToList();
			List<string> feedbackKeywords = new List<string> { "KW", "GW", "QF", "TF", "GZ" };
			var orderedEnumerable = from s in source
				group s by new
				{
					IoType = s.IoType,
					CardType = s.CardType,
					PowerSupplyMethod = s.PowerSupplyMethod,
					VoltageLevel = s.VoltageLevel,
					Destination = s.Destination,
					DevicePrefix = GetDevicePrefix(s.TagName)
				} into g
				orderby (!ioTypeOrder.TryGetValue(g.Key.IoType, out value)) ? int.MaxValue : value, g.Any((IoFullData s) => feedbackKeywords.Any((string keyword) => s.TagName.Contains(keyword))) descending
				select g;
			foreach (var signalGroup in orderedEnumerable)
			{
				List<IoFullData> list2 = signalGroup.OrderBy((IoFullData s) => s.TagName).ToList();
				config_card_type_judge config_card_type_judge = configs.SingleOrDefault((config_card_type_judge c) => c.IoCardType == signalGroup.Key.CardType);
				if (config_card_type_judge == null)
				{
					throw new Exception("未在配置表中找到" + signalGroup.Key.CardType + ",请配置后再生成");
				}
				List<IoFullData> feedbackSignals = list2.Where((IoFullData s) => feedbackKeywords.Any((string keyword) => s.TagName.Contains(keyword))).ToList();
				List<IoFullData> otherSignals = list2.Except(feedbackSignals).ToList();
				if (feedbackSignals.Any() && !(from sc in stdCabinet.Cages.SelectMany((ChassisInfo c) => c.Slots.Select((SlotInfo slot) => new { slot, c }))
					where sc.slot.Board?.Type == signalGroup.Key.CardType && !IsLastTwoSlots(sc.c, sc.slot)
					select sc).Any(sc => AssignGroupToSlot(sc.slot, sc.c, feedbackSignals, rate, signalGroup.Key.PowerSupplyMethod, signalGroup.Key.VoltageLevel, signalGroup.Key.Destination)))
				{
					AssignToNewSlotOrUnset(stdCabinet, config_card_type_judge, feedbackSignals);
				}
				if (otherSignals.Any() && !(from sc in stdCabinet.Cages.SelectMany((ChassisInfo c) => c.Slots.Select((SlotInfo slot) => new { slot, c }))
					where sc.slot.Board?.Type == signalGroup.Key.CardType && !IsLastTwoSlots(sc.c, sc.slot)
					select sc).Any(sc => AssignGroupToSlot(sc.slot, sc.c, otherSignals, rate, signalGroup.Key.PowerSupplyMethod, signalGroup.Key.VoltageLevel, signalGroup.Key.Destination)))
				{
					AssignToNewSlotOrUnset(stdCabinet, config_card_type_judge, otherSignals);
				}
			}
			if (stdCabinet.UnsetPoints.Any())
			{
				AssignRemainingSignals(stdCabinet, rate);
			}
		}
		return list;
	}

	private string GetDevicePrefix(string tagName)
	{
		string[] array = tagName.Split('_');
		if (array.Length == 0)
		{
			return tagName;
		}
		return array[0];
	}

	private bool IsLastTwoSlots(ChassisInfo cage, SlotInfo slot)
	{
		if (slot.Index != cage.Slots[cage.Slots.Count - 1].Index)
		{
			return slot.Index == cage.Slots[cage.Slots.Count - 2].Index;
		}
		return true;
	}

	private void AssignToNewSlotOrUnset(StdCabinet cabinetInfo, config_card_type_judge config, List<IoFullData> signals)
	{
		SlotInfo slotInfo = cabinetInfo.Cages.SelectMany((ChassisInfo c) => c.Slots.Select((SlotInfo slot) => new { slot, c })).FirstOrDefault(sc => sc.slot.Board == null && !IsLastTwoSlots(sc.c, sc.slot))?.slot;
		if (slotInfo != null)
		{
			slotInfo.Board = Board.Create(config);
			for (int num = 0; num < signals.Count; num++)
			{
				slotInfo.Board.Channels[num].Point = signals[num];
			}
		}
		else
		{
			cabinetInfo.UnsetPoints.AddRange(signals);
		}
	}

	private void AssignRemainingSignals(StdCabinet cabinetInfo, double rate)
	{
		List<IoFullData> source = cabinetInfo.UnsetPoints.ToList();
		cabinetInfo.UnsetPoints.Clear();
		foreach (var signalGroup in from s in source
			group s by new { s.CardType, s.PowerSupplyMethod, s.VoltageLevel })
		{
			List<IoFullData> signals = signalGroup.OrderBy((IoFullData s) => s.TagName).ToList();
			if ((from sc in cabinetInfo.Cages.SelectMany((ChassisInfo c) => c.Slots.Select((SlotInfo slot) => new { slot, c }))
				where sc.slot.Board?.Type == signalGroup.Key.CardType && !IsLastTwoSlots(sc.c, sc.slot)
				select sc).Any(sc => AssignGroupToSlot(sc.slot, sc.c, signals, rate, signalGroup.Key.PowerSupplyMethod, signalGroup.Key.VoltageLevel)))
			{
				continue;
			}
			SlotInfo slotInfo = cabinetInfo.Cages.SelectMany((ChassisInfo c) => c.Slots.Select((SlotInfo slot) => new { slot, c })).FirstOrDefault(sc => sc.slot.Board == null && !IsLastTwoSlots(sc.c, sc.slot))?.slot;
			if (slotInfo != null)
			{
				config_card_type_judge type = new config_card_type_judge
				{
					IoCardType = signalGroup.Key.CardType
				};
				slotInfo.Board = Board.Create(type);
				for (int num = 0; num < signals.Count; num++)
				{
					slotInfo.Board.Channels[num].Point = signals[num];
				}
			}
			else
			{
				cabinetInfo.UnsetPoints.AddRange(signals);
			}
		}
	}

	private bool AssignGroupToSlot(SlotInfo slot, ChassisInfo cage, List<IoFullData> signals, double rate, string powerSupplyMethod, string voltageLevel, string dest = null)
	{
		if (slot.Board?.Type != signals.First().CardType)
		{
			return false;
		}
		int num = slot.Board.Channels.Count((Xt2Channel c) => c.Point != null);
		int count = slot.Board.Channels.Count;
		List<string> list = (from c in slot.Board.Channels
			where c.Point != null
			select c.Point.VoltageLevel).Distinct().ToList();
		List<string> list2 = (from c in slot.Board.Channels
			where c.Point != null
			select c.Point.PowerSupplyMethod).Distinct().ToList();
		List<string> list3 = (from c in slot.Board.Channels
			where c.Point != null
			select c.Point.Destination).Distinct().ToList();
		bool flag = list.Count == 0 || (list.Count == 1 && list[0] == voltageLevel) || (list.Count == 1 && list[0] == null && voltageLevel == null);
		bool flag2 = list2.Count == 0 || (list2.Count == 1 && list2[0] == powerSupplyMethod) || (list2.Count == 1 && list2[0] == null && powerSupplyMethod == null);
		bool flag3 = dest == null || list3.Count == 0 || (list3.Count == 1 && list3[0] == dest) || (list3.Count == 1 && list3[0] == null && dest == null);
		if (flag && flag2 && flag3 && num + signals.Count <= count && (double)(num + signals.Count) / (double)count <= 1.0 - rate)
		{
			for (int num2 = 0; num2 < signals.Count; num2++)
			{
				slot.Board.Channels[num + num2].Point = signals[num2];
			}
			return true;
		}
		return false;
	}
}
