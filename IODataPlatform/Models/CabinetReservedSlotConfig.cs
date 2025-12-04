using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace IODataPlatform.Models;

/// <summary>
/// 机柜预留插槽配置
/// </summary>
public class CabinetReservedSlotConfig : INotifyPropertyChanged
{
	private string _cabinetName;
	private bool _isSelected;

	/// <summary>
	/// 机柜名称
	/// </summary>
	public string CabinetName
	{
		get => _cabinetName;
		set
		{
			_cabinetName = value;
			OnPropertyChanged(nameof(CabinetName));
		}
	}

	/// <summary>
	/// 是否选中（需要预留插槽）
	/// </summary>
	public bool IsSelected
	{
		get => _isSelected;
		set
		{
			_isSelected = value;
			OnPropertyChanged(nameof(IsSelected));
			if (!_isSelected)
			{
				SlotConfigs.Clear();
			}
		}
	}

	/// <summary>
	/// 预留插槽数量（只读，等于SlotConfigs.Count）
	/// </summary>
	public int ReservedCount => SlotConfigs.Count;

	/// <summary>
	/// 插槽配置列表
	/// </summary>
	public ObservableCollection<SlotCardTypeConfig> SlotConfigs { get; set; } = new();

	/// <summary>
	/// 可用的通讯板卡类型列表（MD211、MD216和DP211）
	/// </summary>
	public List<string> AvailableCardTypes { get; set; } = new List<string> { "MD211", "MD216", "DP211" };

	public CabinetReservedSlotConfig()
	{
		// 监听SlotConfigs变化，自动更新ReservedCount
		SlotConfigs.CollectionChanged += (s, e) => OnPropertyChanged(nameof(ReservedCount));
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
