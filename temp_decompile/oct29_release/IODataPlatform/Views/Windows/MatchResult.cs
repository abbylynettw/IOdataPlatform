using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// 匹配结果模型
/// </summary>
public class MatchResult : INotifyPropertyChanged
{
	private string _signalPositionNumber = string.Empty;

	private string _valveNumber = string.Empty;

	private string _airPortOrder = string.Empty;

	private string _dpTerminalChannel = string.Empty;

	private string _matchStatus = string.Empty;

	private string _description = string.Empty;

	public string SignalPositionNumber
	{
		get
		{
			return _signalPositionNumber;
		}
		set
		{
			_signalPositionNumber = value;
			OnPropertyChanged("SignalPositionNumber");
		}
	}

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

	public string DPTerminalChannel
	{
		get
		{
			return _dpTerminalChannel;
		}
		set
		{
			_dpTerminalChannel = value;
			OnPropertyChanged("DPTerminalChannel");
		}
	}

	public string MatchStatus
	{
		get
		{
			return _matchStatus;
		}
		set
		{
			_matchStatus = value;
			OnPropertyChanged("MatchStatus");
		}
	}

	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			_description = value;
			OnPropertyChanged("Description");
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
