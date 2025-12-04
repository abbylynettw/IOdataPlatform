using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IODataPlatform.Models.ExportModels;

/// <summary>
/// 导出配置数据模型
/// </summary>
public class ExportConfig
{
    /// <summary>
    /// 配置名称
    /// </summary>
    public string ConfigName { get; set; } = string.Empty;

    /// <summary>
    /// 导出类型
    /// </summary>
    public ExportType Type { get; set; } = ExportType.CompleteList;

    /// <summary>
    /// 列顺序配置
    /// </summary>
    public List<ColumnInfo> ColumnOrder { get; set; } = new();

    /// <summary>
    /// 选中的字段（仅发布清单时有效）
    /// </summary>
    public List<string> SelectedFields { get; set; } = new();

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.Now;

    /// <summary>
    /// 是否为系统预设配置
    /// </summary>
    public bool IsSystemDefault { get; set; } = false;
}

/// <summary>
/// 导出类型枚举
/// </summary>
public enum ExportType
{
    /// <summary>
    /// 完整IO清单
    /// </summary>
    [Description("完整IO清单")]
    CompleteList,

    /// <summary>
    /// 当前控制系统IO清单
    /// </summary>
    [Description("当前控制系统IO清单")]
    CurrentSystemList,

    /// <summary>
    /// 发布的IO清单
    /// </summary>
    [Description("发布的IO清单")]
    PublishedList
}

/// <summary>
/// 列信息配置
/// </summary>
public class ColumnInfo : INotifyPropertyChanged
{
    private string _fieldName = string.Empty;
    private string _displayName = string.Empty;
    private int _order = 0;
    private bool _isVisible = true;
    private bool _isRequired = false;
    private ColumnType _type = ColumnType.Text;

    /// <summary>
    /// 字段名称
    /// </summary>
    public string FieldName
    {
        get => _fieldName;
        set
        {
            _fieldName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName
    {
        get => _displayName;
        set
        {
            _displayName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 显示顺序
    /// </summary>
    public int Order
    {
        get => _order;
        set
        {
            _order = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 是否可见
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 是否必需
    /// </summary>
    public bool IsRequired
    {
        get => _isRequired;
        set
        {
            _isRequired = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 列类型
    /// </summary>
    public ColumnType Type
    {
        get => _type;
        set
        {
            _type = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// 列类型枚举
/// </summary>
public enum ColumnType
{
    /// <summary>
    /// 文本
    /// </summary>
    Text,

    /// <summary>
    /// 数字
    /// </summary>
    Number,

    /// <summary>
    /// 日期
    /// </summary>
    Date,

    /// <summary>
    /// 布尔值
    /// </summary>
    Boolean
}