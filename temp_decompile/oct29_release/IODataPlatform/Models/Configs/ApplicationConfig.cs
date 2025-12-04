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
	public DatabaseConfig Database { get; set; } = new DatabaseConfig();

	/// <summary>
	/// Web服务配置节点
	/// 包含API地址、超时设置、重试策略等
	/// </summary>
	public WebServiceConfig WebService { get; set; } = new WebServiceConfig();

	/// <summary>
	/// 缓存配置节点
	/// 定义各种缓存策略和存储配置
	/// </summary>
	public CacheConfig Cache { get; set; } = new CacheConfig();

	/// <summary>
	/// 日志配置节点
	/// 定义日志级别、输出路径等配置
	/// </summary>
	public LoggingConfig Logging { get; set; } = new LoggingConfig();

	/// <summary>
	/// 用户界面配置节点
	/// 包含主题、语言、窗口设置等用户偏好
	/// </summary>
	public UIConfig UI { get; set; } = new UIConfig();
}
