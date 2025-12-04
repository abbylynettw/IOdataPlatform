using System;
using System.Collections.Generic;
using System.Linq;
using IODataPlatform.Models.DBModels;
using LYSoft.Libs;

namespace IODataPlatform.Models.ExcelModels;

public static class CabinetCalc
{
	public static List<StdCabinet> BuildCabinetStructureLH(this List<IoFullData> points, List<config_card_type_judge> configs)
	{
		Dictionary<string, StdCabinet> dictionary = new Dictionary<string, StdCabinet>();
		foreach (IoFullData point in points)
		{
			if (!dictionary.TryGetValue(point.CabinetNumber, out var value))
			{
				value = StdCabinet.CreateLH(point.CabinetNumber);
				dictionary.Add(point.CabinetNumber, value);
			}
			ChassisInfo chassisInfo = value.Cages.FirstOrDefault((ChassisInfo c) => c.Index == point.Cage);
			SlotInfo slotInfo = chassisInfo?.Slots.FirstOrDefault((SlotInfo s) => s.Index == point.Slot);
			if (chassisInfo == null || slotInfo == null)
			{
				value.UnsetPoints.Add(point);
				continue;
			}
			if (slotInfo.Board == null)
			{
				config_card_type_judge type = configs.Single((config_card_type_judge c) => c.IoCardType == point.CardType);
				slotInfo.Board = Board.Create(type);
			}
			if (slotInfo.Board.Type != point.CardType)
			{
				value.UnsetPoints.Add(point);
				continue;
			}
			if (point.Channel <= 0 || point.Channel > slotInfo.Board.Channels.Count)
			{
				value.UnsetPoints.Add(point);
				continue;
			}
			Xt2Channel xt2Channel = slotInfo.Board.Channels[point.Channel - 1];
			if (xt2Channel.Point == null)
			{
				xt2Channel.Point = point;
			}
			else
			{
				value.UnsetPoints.Add(point);
			}
		}
		return dictionary.Values.ToList();
	}

	public static List<StdCabinet> BuildCabinetStructureXT1(this List<IoFullData> points, List<config_card_type_judge> configs)
	{
		Dictionary<string, StdCabinet> dictionary = new Dictionary<string, StdCabinet>();
		return dictionary.Values.ToList();
	}

	public static List<StdCabinet> BuildCabinetStructureOther(this List<IoFullData> points, List<config_card_type_judge> configs)
	{
		Dictionary<string, StdCabinet> dictionary = new Dictionary<string, StdCabinet>();
		foreach (IoFullData point in points)
		{
			if (!dictionary.TryGetValue(point.CabinetNumber, out var value))
			{
				value = StdCabinet.Create(point.CabinetNumber);
				dictionary.Add(point.CabinetNumber, value);
			}
			if (point.Cage == -1)
			{
				SlotInfo slotInfo = value.VirtualSlots.FirstOrDefault((SlotInfo vs) => vs.Index == point.Slot);
				if (slotInfo == null)
				{
					config_card_type_judge config_card_type_judge = configs.FirstOrDefault((config_card_type_judge c) => c.IoCardType == point.CardType);
					if (config_card_type_judge == null)
					{
						value.UnsetPoints.Add(point);
						continue;
					}
					slotInfo = new SlotInfo
					{
						Index = point.Slot,
						Board = Board.Create(config_card_type_judge),
						IsVirtual = true
					};
					value.VirtualSlots.Add(slotInfo);
				}
				if (slotInfo.Board != null && point.Channel > 0 && point.Channel <= slotInfo.Board.Channels.Count)
				{
					Xt2Channel xt2Channel = slotInfo.Board.Channels[point.Channel - 1];
					if (xt2Channel.Point == null)
					{
						xt2Channel.Point = point;
					}
					else
					{
						value.UnsetPoints.Add(point);
					}
				}
				else
				{
					value.UnsetPoints.Add(point);
				}
				continue;
			}
			ChassisInfo chassisInfo = value.Cages.FirstOrDefault((ChassisInfo c) => c.Index == point.Cage);
			SlotInfo slotInfo2 = chassisInfo?.Slots.FirstOrDefault((SlotInfo s) => s.Index == point.Slot);
			if (chassisInfo == null || slotInfo2 == null)
			{
				value.UnsetPoints.Add(point);
				continue;
			}
			if (slotInfo2.Board == null)
			{
				config_card_type_judge config_card_type_judge2 = configs.SingleOrDefault((config_card_type_judge c) => c.IoCardType == point.CardType);
				if (config_card_type_judge2 == null)
				{
					throw new Exception("IO卡型号配置表中缺少" + point.CardType + "这种板卡类型，请检查输入数据或者公式");
				}
				slotInfo2.Board = Board.Create(config_card_type_judge2);
			}
			if (slotInfo2.Board.Type != point.CardType)
			{
				value.UnsetPoints.Add(point);
				continue;
			}
			if (point.Channel <= 0 || point.Channel > slotInfo2.Board.Channels.Count)
			{
				value.UnsetPoints.Add(point);
				continue;
			}
			Xt2Channel xt2Channel2 = slotInfo2.Board.Channels[point.Channel - 1];
			if (xt2Channel2.Point == null)
			{
				xt2Channel2.Point = point;
			}
			else
			{
				value.UnsetPoints.Add(point);
			}
		}
		return dictionary.Values.ToList();
	}

	public static List<IoFullData> CabinetStructureToPoint(IEnumerable<StdCabinet> cabinets)
	{
		List<IoFullData> list = new List<IoFullData>();
		foreach (StdCabinet cabinet in cabinets)
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
							channel.Point.CabinetNumber = cabinet.Name;
							channel.Point.Cage = cage.Index;
							channel.Point.Slot = slot.Index;
							channel.Point.CardType = slot.Board.Type;
							channel.Point.Channel = channel.Index;
							list.Add(channel.Point);
						}
					}
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
						channel2.Point.CabinetNumber = cabinet.Name;
						channel2.Point.Cage = -1;
						channel2.Point.Slot = virtualSlot.Index;
						channel2.Point.CardType = virtualSlot.Board.Type;
						channel2.Point.Channel = channel2.Index;
						list.Add(channel2.Point);
					}
				}
			}
			foreach (IoFullData unsetPoint in cabinet.UnsetPoints)
			{
				unsetPoint.CabinetNumber = cabinet.Name;
				list.Add(unsetPoint);
			}
		}
		return list;
	}

	public static List<IoFullData> ToPoint(this StdCabinet cabinet)
	{
		List<IoFullData> list = new List<IoFullData>();
		foreach (SlotInfo slot in cabinet.AllSlots)
		{
			if (slot.Board == null)
			{
				continue;
			}
			foreach (Xt2Channel channel in slot.Board.Channels)
			{
				if (channel.Point == null)
				{
					continue;
				}
				channel.Point.CabinetNumber = cabinet.Name;
				channel.Point.CardType = slot.Board.Type;
				channel.Point.Channel = channel.Index;
				if (slot.IsVirtual)
				{
					channel.Point.Cage = -1;
					channel.Point.Slot = slot.Index;
				}
				else
				{
					ChassisInfo chassisInfo = cabinet.Cages.First((ChassisInfo c) => c.Slots.Contains(slot));
					channel.Point.Cage = chassisInfo.Index;
					channel.Point.Slot = slot.Index;
				}
				list.Add(channel.Point);
			}
		}
		foreach (IoFullData unsetPoint in cabinet.UnsetPoints)
		{
			unsetPoint.CabinetNumber = cabinet.Name;
			list.Add(unsetPoint);
		}
		return list;
	}

	public static List<IoFullData> Recalc(IEnumerable<IoFullData> data)
	{
		List<IoFullData> list = new List<IoFullData>();
		foreach (IGrouping<string, IoFullData> item in from d in data
			group d by d.CabinetNumber)
		{
			int num = 111;
			int num2 = 211;
			int num3 = 311;
			int num4 = 411;
			foreach (IGrouping<int, IoFullData> item2 in from d in item
				group d by d.Cage)
			{
				foreach (IGrouping<int, IoFullData> item3 in from c in item2
					group c by c.Slot)
				{
					IoFullData ioFullData = item3.FirstOrDefault();
					if (ioFullData == null || ioFullData.CardType == null)
					{
						continue;
					}
					string terminalBoardNumber = null;
					switch (ioFullData.CardType.Substring(0, 2))
					{
					case "DO":
						terminalBoardNumber = $"{num}BN";
						num++;
						break;
					case "DI":
						terminalBoardNumber = $"{num2}BN";
						num2++;
						break;
					case "AI":
						terminalBoardNumber = $"{num3}BN";
						num3++;
						break;
					case "AO":
						terminalBoardNumber = $"{num4}BN";
						num4++;
						break;
					}
					foreach (IoFullData item4 in item3)
					{
						if (item4 != null)
						{
							item4.TerminalBoardNumber = terminalBoardNumber;
							if (item4.IoType == "AO")
							{
								item4.SignalPlus = $"I{item4.Channel}+";
								item4.SignalMinus = $"I{item4.Channel}-";
							}
							else
							{
								item4.SignalPlus = $"A{item4.Channel:D2}";
								item4.SignalMinus = $"B{item4.Channel:D2}";
							}
							list.Add(item4);
						}
					}
				}
			}
		}
		return list;
	}

	public static List<IoFullData> ToPoint(this IEnumerable<StdCabinet> cabinets)
	{
		List<IoFullData> list = new List<IoFullData>();
		foreach (StdCabinet cabinet in cabinets)
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
							channel.Point.CabinetNumber = cabinet.Name;
							channel.Point.Cage = cage.Index;
							channel.Point.Slot = slot.Index;
							channel.Point.CardType = slot.Board.Type;
							channel.Point.Channel = channel.Index;
							list.Add(channel.Point);
						}
					}
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
						channel2.Point.CabinetNumber = cabinet.Name;
						channel2.Point.Cage = -1;
						channel2.Point.Slot = virtualSlot.Index;
						channel2.Point.CardType = virtualSlot.Board.Type;
						channel2.Point.Channel = channel2.Index;
						list.Add(channel2.Point);
					}
				}
			}
			foreach (IoFullData unsetPoint in cabinet.UnsetPoints)
			{
				unsetPoint.CabinetNumber = cabinet.Name;
				list.Add(unsetPoint);
			}
		}
		return list;
	}

	public static void RemovePoints(this StdCabinet cabinet, TagType type)
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
					if (channel.Point != null && channel.Point.PointType == type)
					{
						channel.Point = null;
					}
				}
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
				if (channel2.Point != null && channel2.Point.PointType == type)
				{
					channel2.Point = null;
				}
			}
		}
		cabinet.UnsetPoints.RemoveWhere((IoFullData c) => c.PointType == type);
	}

	public static NumberCheck CountTotalPoints(IEnumerable<StdCabinet> cabinets)
	{
		int num = cabinets.SelectMany((StdCabinet cabinet) => cabinet.Cages.SelectMany((ChassisInfo cage) => cage.Slots.Where((SlotInfo slot) => slot.Board != null).SelectMany((SlotInfo slot) => slot.Board.Channels.Where((Xt2Channel channel) => channel.Point != null)))).Count();
		return new NumberCheck
		{
			Number = num,
			Illegal = (num > 0)
		};
	}

	public static NumberCheck CountBackupPoints(IEnumerable<StdCabinet> cabinets)
	{
		int num = cabinets.SelectMany((StdCabinet cabinet) => cabinet.Cages.SelectMany((ChassisInfo cage) => cage.Slots.Where((SlotInfo slot) => slot.Board != null).SelectMany((SlotInfo slot) => slot.Board.Channels.Where((Xt2Channel channel) => channel.Point != null && channel.Point.PointType == TagType.BackUp)))).Count();
		return new NumberCheck
		{
			Number = num,
			Illegal = (num > 0)
		};
	}

	public static NumberCheck CountAlarmPoints(IEnumerable<StdCabinet> cabinets)
	{
		NumberCheck numberCheck = new NumberCheck
		{
			Number = cabinets.SelectMany((StdCabinet cabinet) => cabinet.Cages.SelectMany((ChassisInfo cage) => cage.Slots.Where((SlotInfo slot) => slot.Board != null).SelectMany((SlotInfo slot) => slot.Board.Channels.Where((Xt2Channel channel) => channel.Point != null && channel.Point.PointType == TagType.Alarm)))).Count()
		};
		numberCheck.Illegal = numberCheck.Number == (double)(cabinets.Count() * 7);
		return numberCheck;
	}

	public static NumberCheck CountNormalPoints(IEnumerable<StdCabinet> cabinets)
	{
		int num = cabinets.SelectMany((StdCabinet cabinet) => cabinet.Cages.SelectMany((ChassisInfo cage) => cage.Slots.Where((SlotInfo slot) => slot.Board != null).SelectMany((SlotInfo slot) => slot.Board.Channels.Where((Xt2Channel channel) => channel.Point != null && channel.Point.PointType == TagType.Normal)))).Count();
		return new NumberCheck
		{
			Number = num,
			Illegal = (num > 0)
		};
	}

	public static List<CardSpareRate> CalculateRedundancyRate(IEnumerable<StdCabinet> cabinets)
	{
		return (from x in cabinets.SelectMany((StdCabinet cabinet) => cabinet.Cages.SelectMany((ChassisInfo cage) => from slot in cage.Slots
				where slot.Board != null
				select new
				{
					BoardType = slot.Board.Type,
					TotalChannels = slot.Board.Channels.Count,
					UnusedChannels = slot.Board.Channels.Count((Xt2Channel channel) => channel.Point == null)
				}))
			group x by x.BoardType into grp
			select new CardSpareRate
			{
				CardType = grp.Key,
				Rate = Math.Round((double)grp.Sum(x => x.UnusedChannels) * 100.0 / (double)grp.Sum(x => x.TotalChannels), 1)
			}).ToList();
	}

	public static TotalSummaryInfo GetTotalSummaryInfo(List<IoFullData> data, List<config_card_type_judge> configs)
	{
		IEnumerable<StdCabinet> cabinets = data.BuildCabinetStructureOther(configs);
		return new TotalSummaryInfo
		{
			TotalPoints = CountTotalPoints(cabinets),
			BackupPoints = CountBackupPoints(cabinets),
			AlarmPoints = CountAlarmPoints(cabinets),
			NormalPoints = CountNormalPoints(cabinets),
			RedundancyRates = CalculateRedundancyRate(cabinets)
		};
	}

	public static NumberCheck CalculateTotalPoints(StdCabinet cabinet)
	{
		return new NumberCheck
		{
			Number = cabinet.Cages.SelectMany((ChassisInfo cage) => from channel in cage.Slots.SelectMany(delegate(SlotInfo slot)
				{
					IEnumerable<Xt2Channel> enumerable = slot.Board?.Channels;
					return enumerable ?? Enumerable.Empty<Xt2Channel>();
				})
				where channel.Point != null
				select channel).Count(),
			Illegal = true
		};
	}

	public static NumberCheck CalculateBackupPoints(StdCabinet cabinet)
	{
		return new NumberCheck
		{
			Number = cabinet.Cages.SelectMany((ChassisInfo cage) => cage.Slots.SelectMany(delegate(SlotInfo slot)
			{
				IEnumerable<Xt2Channel> enumerable = slot.Board?.Channels;
				return enumerable ?? Enumerable.Empty<Xt2Channel>();
			})).Count(delegate(Xt2Channel channel)
			{
				IoFullData? point = channel.Point;
				return point != null && point.PointType == TagType.BackUp;
			}),
			Illegal = true
		};
	}

	public static NumberCheck CalculateAlarmPoints(StdCabinet cabinet)
	{
		NumberCheck numberCheck = new NumberCheck
		{
			Number = cabinet.Cages.SelectMany((ChassisInfo cage) => cage.Slots.SelectMany(delegate(SlotInfo slot)
			{
				IEnumerable<Xt2Channel> enumerable = slot.Board?.Channels;
				return enumerable ?? Enumerable.Empty<Xt2Channel>();
			})).Count(delegate(Xt2Channel channel)
			{
				IoFullData? point = channel.Point;
				return point != null && point.PointType == TagType.Alarm;
			})
		};
		numberCheck.Illegal = numberCheck.Number == 7.0;
		return numberCheck;
	}

	public static NumberCheck CalculateNormalPoints(StdCabinet cabinet)
	{
		return new NumberCheck
		{
			Number = cabinet.Cages.SelectMany((ChassisInfo cage) => cage.Slots.SelectMany(delegate(SlotInfo slot)
			{
				IEnumerable<Xt2Channel> enumerable = slot.Board?.Channels;
				return enumerable ?? Enumerable.Empty<Xt2Channel>();
			})).Count(delegate(Xt2Channel channel)
			{
				IoFullData? point = channel.Point;
				return point != null && point.PointType == TagType.Normal;
			}),
			Illegal = true
		};
	}

	public static NumberCheck CalculateUnsetPoints(StdCabinet cabinet)
	{
		NumberCheck numberCheck = new NumberCheck
		{
			Number = cabinet.UnsetPoints.Count
		};
		numberCheck.Illegal = numberCheck.Number == 0.0;
		return numberCheck;
	}

	public static NumberCheck CalculateUnsetBoards(StdCabinet cabinet)
	{
		NumberCheck numberCheck = new NumberCheck
		{
			Number = cabinet.VirtualSlots.Count((SlotInfo vs) => vs.Board != null)
		};
		numberCheck.Illegal = numberCheck.Number == 0.0;
		return numberCheck;
	}

	public static List<CardSpareRate> CalculateRedundancyRates(StdCabinet cabinet)
	{
		List<CardSpareRate> list = new List<CardSpareRate>();
		Dictionary<string, int> dictionary = (from x in cabinet.Cages.SelectMany((ChassisInfo cage) => from slot in cage.Slots
				where slot.Board != null
				select new
				{
					CardType = slot.Board.Type,
					Channel = slot.Board.Channels
				}).SelectMany(x => x.Channel.Select((Xt2Channel channel) => new
			{
				CardType = x.CardType,
				Channel = channel
			}))
			group x by x.CardType).ToDictionary(group => group.Key ?? "Unknown", group => group.Count());
		Dictionary<string, int> dictionary2 = (from x in cabinet.Cages.SelectMany((ChassisInfo cage) => from slot in cage.Slots
				where slot.Board != null
				select new
				{
					CardType = slot.Board.Type,
					Channel = slot.Board.Channels
				}).SelectMany(x => from channel in x.Channel
				where channel.Point == null
				select new
				{
					CardType = x.CardType,
					Channel = channel
				})
			group x by x.CardType).ToDictionary(group => group.Key ?? "Unknown", group => group.Count());
		foreach (string key in dictionary.Keys)
		{
			int num = dictionary[key];
			int num2 = (dictionary2.ContainsKey(key) ? dictionary2[key] : 0);
			double rate = ((num > 0) ? Math.Round((double)num2 * 100.0 / (double)num, 1) : 0.0);
			list.Add(new CardSpareRate
			{
				CardType = key,
				Rate = rate
			});
		}
		return list;
	}

	public static CabinetSummaryInfo GetCabinetSummaryInfo(StdCabinet cabinet)
	{
		return new CabinetSummaryInfo
		{
			TotalPoints = CalculateTotalPoints(cabinet),
			BackupPoints = CalculateBackupPoints(cabinet),
			AlarmPoints = CalculateAlarmPoints(cabinet),
			NormalPoints = CalculateNormalPoints(cabinet),
			UnsetPoints = CalculateUnsetPoints(cabinet),
			UnsetBoards = CalculateUnsetBoards(cabinet),
			RedundancyRates = CalculateRedundancyRates(cabinet)
		};
	}
}
