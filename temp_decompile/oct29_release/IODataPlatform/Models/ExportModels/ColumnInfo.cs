using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IODataPlatform.Models.ExportModels;

/// <summary>
/// 列信息配置
/// </summary>
public class ColumnInfo : INotifyPropertyChanged
{
	private string _fieldName = string.Empty;

	private string _displayName = string.Empty;

	private int _order;

	private bool _isVisible = true;

	private bool _isRequired;

	private ColumnType _type;

	/// <summary>
	/// 字段名称
	/// </summary>
	public string FieldName
	{
		get
		{
			return _fieldName;
		}
		set
		{
			_fieldName = value;
			OnPropertyChanged("FieldName");
		}
	}

	/// <summary>
	/// 显示名称
	/// </summary>
	public string DisplayName
	{
		get
		{
			return _displayName;
		}
		set
		{
			_displayName = value;
			OnPropertyChanged("DisplayName");
		}
	}

	/// <summary>
	/// 显示顺序
	/// </summary>
	public int Order
	{
		get
		{
			return _order;
		}
		set
		{
			_order = value;
			OnPropertyChanged("Order");
		}
	}

	/// <summary>
	/// 是否可见
	/// </summary>
	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			_isVisible = value;
			OnPropertyChanged("IsVisible");
		}
	}

	/// <summary>
	/// 是否必需
	/// </summary>
	public bool IsRequired
	{
		get
		{
			return _isRequired;
		}
		set
		{
			_isRequired = value;
			OnPropertyChanged("IsRequired");
		}
	}

	/// <summary>
	/// 列类型
	/// </summary>
	public ColumnType Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
			OnPropertyChanged("Type");
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
