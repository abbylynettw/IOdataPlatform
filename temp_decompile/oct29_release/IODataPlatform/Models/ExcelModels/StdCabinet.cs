using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;

namespace IODataPlatform.Models.ExcelModels;

/// <summary>机柜</summary>
/// <inheritdoc />
public class StdCabinet : ObservableObject
{
	[ObservableProperty]
	private ObservableCollection<IoFullData> unsetPoints = new ObservableCollection<IoFullData>();

	/// <summary>机柜名</summary>
	public required string Name { get; set; }

	/// <summary>机笼列表</summary>
	public required List<ChassisInfo> Cages { get; set; }

	/// <summary>虚拟插槽列表，用于存放未分配的板卡</summary>
	public ObservableCollection<SlotInfo> VirtualSlots { get; set; } = new ObservableCollection<SlotInfo>();

	[JsonIgnore]
	public CabinetSummaryInfo SummaryInfo => CabinetCalc.GetCabinetSummaryInfo(this);

	/// <summary>获取所有插槽（包括虚拟插槽）</summary>
	[JsonIgnore]
	public IEnumerable<SlotInfo> AllSlots
	{
		get
		{
			foreach (ChassisInfo cage in Cages)
			{
				foreach (SlotInfo slot in cage.Slots)
				{
					yield return slot;
				}
			}
			foreach (SlotInfo virtualSlot in VirtualSlots)
			{
				yield return virtualSlot;
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Models.ExcelModels.StdCabinet.unsetPoints" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<IoFullData> UnsetPoints
	{
		get
		{
			return unsetPoints;
		}
		[MemberNotNull("unsetPoints")]
		set
		{
			if (!EqualityComparer<ObservableCollection<IoFullData>>.Default.Equals(unsetPoints, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.UnsetPoints);
				unsetPoints = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.UnsetPoints);
			}
		}
	}

	/// <summary>使用此方法创建Cabinet，而不是直接new，保留new方法给序列化器使用</summary>
	public static StdCabinet Create(string name)
	{
		StdCabinet obj = new StdCabinet
		{
			Name = name
		};
		int num = 3;
		List<ChassisInfo> list = new List<ChassisInfo>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<ChassisInfo> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = new ChassisInfo
		{
			Index = 1,
			Slots = CreateSlots(3, 8)
		};
		num2++;
		span[num2] = new ChassisInfo
		{
			Index = 2,
			Slots = CreateSlots(1, 10)
		};
		num2++;
		span[num2] = new ChassisInfo
		{
			Index = 3,
			Slots = CreateSlots(1, 10)
		};
		obj.Cages = list;
		return obj;
	}

	public static StdCabinet CreateEx(string name)
	{
		StdCabinet obj = new StdCabinet
		{
			Name = name
		};
		int num = 3;
		List<ChassisInfo> list = new List<ChassisInfo>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<ChassisInfo> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = new ChassisInfo
		{
			Index = 4,
			Slots = CreateSlots(1, 10)
		};
		num2++;
		span[num2] = new ChassisInfo
		{
			Index = 5,
			Slots = CreateSlots(1, 10)
		};
		num2++;
		span[num2] = new ChassisInfo
		{
			Index = 6,
			Slots = CreateSlots(1, 10)
		};
		obj.Cages = list;
		return obj;
	}

	public static StdCabinet CreateExEx(string name)
	{
		StdCabinet obj = new StdCabinet
		{
			Name = name
		};
		int num = 2;
		List<ChassisInfo> list = new List<ChassisInfo>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<ChassisInfo> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = new ChassisInfo
		{
			Index = 7,
			Slots = CreateSlots(1, 10)
		};
		num2++;
		span[num2] = new ChassisInfo
		{
			Index = 8,
			Slots = CreateSlots(1, 10)
		};
		obj.Cages = list;
		return obj;
	}

	public static StdCabinet CreateLH(string name)
	{
		StdCabinet obj = new StdCabinet
		{
			Name = name
		};
		int num = 3;
		List<ChassisInfo> list = new List<ChassisInfo>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<ChassisInfo> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = new ChassisInfo
		{
			Index = 1,
			Slots = CreateSlots(7, 7)
		};
		num2++;
		span[num2] = new ChassisInfo
		{
			Index = 2,
			Slots = CreateSlots(0, 14)
		};
		num2++;
		span[num2] = new ChassisInfo
		{
			Index = 3,
			Slots = CreateSlots(0, 14)
		};
		obj.Cages = list;
		return obj;
	}

	private static List<SlotInfo> CreateSlots(int startIndex, int count)
	{
		return (from x in Enumerable.Range(startIndex, count)
			select new SlotInfo
			{
				Index = x
			}).ToList();
	}

	/// <summary>添加板卡到虚拟插槽</summary>
	public void AddBoardToVirtualSlot(Board board)
	{
		SlotInfo item = new SlotInfo
		{
			Index = -(VirtualSlots.Count + 1),
			Board = board,
			IsVirtual = true
		};
		VirtualSlots.Add(item);
	}
}
