using System.Collections.Generic;
using System.ComponentModel;

namespace IODataPlatform.Models;

/// <summary>
/// 单个预留插槽的板卡类型配置
/// </summary>
public class SlotCardTypeConfig : INotifyPropertyChanged
{
	private string _selectedCardType = "MD211";
	private ReservedPurpose _reservedPurpose = ReservedPurpose.通讯预留;

	/// <summary>
	/// 选中的板卡类型
	/// </summary>
	public string SelectedCardType
	{
		get => _selectedCardType;
		set
		{
			_selectedCardType = value;
			OnPropertyChanged(nameof(SelectedCardType));
		}
	}

	/// <summary>
	/// 预留目的（通讯预留/报警预留）
	/// </summary>
	public ReservedPurpose ReservedPurpose
	{
		get => _reservedPurpose;
		set
		{
			_reservedPurpose = value;
			OnPropertyChanged(nameof(ReservedPurpose));
		}
	}

	/// <summary>
	/// 可用的板卡类型列表
	/// </summary>
	public List<string> AvailableCardTypes { get; set; } = new List<string> { "MD211", "MD216", "DP211" };

	/// <summary>
	/// 可用的预留目的列表
	/// </summary>
	public List<ReservedPurpose> AvailablePurposes { get; set; } = new List<ReservedPurpose> 
	{ 
		ReservedPurpose.通讯预留, 
		ReservedPurpose.报警预留 
	};

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
