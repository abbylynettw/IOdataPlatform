using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IODataPlatform.Models.DBModels;

namespace IODataPlatform.Models.ExcelModels;

/// <summary>端子板，UI中不提供端子板的编辑方法，因为通道数可能变化</summary>
public class Board
{
	/// <summary>类型</summary>
	public required string Type { get; set; }

	/// <summary>通道列表</summary>
	public ObservableCollection<Xt2Channel> Channels { get; set; } = new ObservableCollection<Xt2Channel>();

	/// <summary>使用此方法创建Board，而不是直接new，保留new方法给序列化器使用</summary>
	public static Board Create(config_card_type_judge type)
	{
		IEnumerable<Xt2Channel> enumerable = from x in Enumerable.Range(1, type.PinsCount)
			select new Xt2Channel
			{
				Index = x
			};
		Board board = new Board();
		board.Type = type.IoCardType;
		Board board2 = board;
		ObservableCollection<Xt2Channel> observableCollection = new ObservableCollection<Xt2Channel>();
		foreach (Xt2Channel item in enumerable)
		{
			observableCollection.Add(item);
		}
		board2.Channels = observableCollection;
		return board;
	}
}
