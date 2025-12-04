using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.Configs;

/// <summary>
/// 应用程序核心配置类
/// 定义系统的主要配置参数，支持数据库连接、Web服务、缓存等关键设置
/// 实现强类型配置验证，确保系统配置的正确性和完整性
/// </summary>
public class ApplicationConfig
{
    /// <summary>
    /// 数据库配置节点
    /// 包含主数据库和辅助数据库的连接配置
    /// </summary>
    public DatabaseConfig Database { get; set; } = new();

    /// <summary>
    /// Web服务配置节点
    /// 包含API地址、超时设置、重试策略等
    /// </summary>
    public WebServiceConfig WebService { get; set; } = new();

    /// <summary>
    /// 缓存配置节点
    /// 定义各种缓存策略和存储配置
    /// </summary>
    public CacheConfig Cache { get; set; } = new();

    /// <summary>
    /// 日志配置节点
    /// 定义日志级别、输出路径等配置
    /// </summary>
    public LoggingConfig Logging { get; set; } = new();

    /// <summary>
    /// 用户界面配置节点
    /// 包含主题、语言、窗口设置等用户偏好
    /// </summary>
    public UIConfig UI { get; set; } = new();
}

/// <summary>
/// 数据库连接配置类
/// 管理多数据库环境的连接字符串和相关参数
/// </summary>
public class DatabaseConfig
{
    /// <summary>
    /// 主数据库连接字符串（SQL Server）
    /// 用于存储核心业务数据
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 辅助数据库连接字符串（Access）
    /// 用于兼容性支持和临时数据存储
    /// </summary>
    public string ConnectionString2 { get; set; } = string.Empty;

    /// <summary>
    /// 连接池最大连接数
    /// </summary>
    [Range(1, 1000)]
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// 命令执行超时时间（秒）
    /// </summary>
    [Range(1, 300)]
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// 是否启用自动迁移
    /// </summary>
    public bool EnableAutoMigration { get; set; } = false;
}

/// <summary>
/// 缓存系统配置类
/// 定义内存缓存、分布式缓存等缓存策略
/// </summary>
public class CacheConfig
{
    /// <summary>
    /// 是否启用缓存功能
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 默认缓存过期时间（分钟）
    /// </summary>
    [Range(1, 1440)]
    public int DefaultExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// 配置数据缓存过期时间（分钟）
    /// 由于配置数据变更频率较低，可以设置更长的缓存时间
    /// </summary>
    [Range(5, 720)]
    public int ConfigCacheMinutes { get; set; } = 120;

    /// <summary>
    /// 最大缓存项数量
    /// 防止内存过度使用
    /// </summary>
    [Range(100, 10000)]
    public int MaxCacheItems { get; set; } = 1000;
}

/// <summary>
/// 日志系统配置类
/// 定义日志记录的各项参数和策略
/// </summary>
public class LoggingConfig
{
    /// <summary>
    /// 日志级别（Debug, Information, Warning, Error, Critical）
    /// </summary>
    public string LogLevel { get; set; } = "Information";

    /// <summary>
    /// 日志文件存储路径
    /// </summary>
    public string LogPath { get; set; } = "Logs";

    /// <summary>
    /// 是否启用文件日志
    /// </summary>
    public bool EnableFileLogging { get; set; } = true;

    /// <summary>
    /// 是否启用控制台日志
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    /// 日志文件保留天数
    /// </summary>
    [Range(1, 365)]
    public int RetainDays { get; set; } = 30;

    /// <summary>
    /// 单个日志文件最大大小（MB）
    /// </summary>
    [Range(1, 100)]
    public int MaxFileSizeMB { get; set; } = 10;
}

/// <summary>
/// 用户界面配置类
/// 管理应用程序的外观和行为设置
/// </summary>
public class UIConfig
{
    /// <summary>
    /// 应用程序主题（Light, Dark, Auto）
    /// </summary>
    public string Theme { get; set; } = "Light";

    /// <summary>
    /// 应用程序语言（zh-CN, en-US）
    /// </summary>
    public string Language { get; set; } = "zh-CN";

    /// <summary>
    /// 是否记住窗口位置和大小
    /// </summary>
    public bool RememberWindowState { get; set; } = true;

    /// <summary>
    /// 默认窗口宽度
    /// </summary>
    [Range(800, 3840)]
    public int DefaultWindowWidth { get; set; } = 1200;

    /// <summary>
    /// 默认窗口高度
    /// </summary>
    [Range(600, 2160)]
    public int DefaultWindowHeight { get; set; } = 800;

    /// <summary>
    /// 是否启用动画效果
    /// </summary>
    public bool EnableAnimations { get; set; } = true;

    /// <summary>
    /// 数据网格每页显示行数
    /// </summary>
    [Range(10, 1000)]
    public int DataGridPageSize { get; set; } = 50;
}