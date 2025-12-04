using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// 阀门数据模型
/// </summary>
public class ValveData : INotifyPropertyChanged
{
	private string _valveNumber = string.Empty;

	private string _airPortOrder = string.Empty;

	public string ValveNumber
	{
		get
		{
			return _valveNumber;
		}
		set
		{
			_valveNumber = value;
			OnPropertyChanged("ValveNumber");
		}
	}

	public string AirPortOrder
	{
		get
		{
			return _airPortOrder;
		}
		set
		{
			_airPortOrder = value;
			OnPropertyChanged("AirPortOrder");
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
