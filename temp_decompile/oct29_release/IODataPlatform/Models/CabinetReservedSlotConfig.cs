using System.ComponentModel;

namespace IODataPlatform.Models;

/// <summary>
/// 机柜预留插槽数量配置
/// </summary>
public class CabinetReservedSlotConfig : INotifyPropertyChanged
{
	private string _cabinetName;

	private bool _isSelected;

	private int _reservedCount;

	/// <summary>
	/// 机柜名称
	/// </summary>
	public string CabinetName
	{
		get
		{
			return _cabinetName;
		}
		set
		{
			_cabinetName = value;
			OnPropertyChanged("CabinetName");
		}
	}

	/// <summary>
	/// 是否选中（需要预留插槽）
	/// </summary>
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
			OnPropertyChanged("IsSelected");
			if (!_isSelected)
			{
				ReservedCount = 0;
			}
		}
	}

	/// <summary>
	/// 预留插槽数量
	/// </summary>
	public int ReservedCount
	{
		get
		{
			return _reservedCount;
		}
		set
		{
			_reservedCount = value;
			OnPropertyChanged("ReservedCount");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
