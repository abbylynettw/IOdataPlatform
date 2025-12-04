using System;
using IODataPlatform.Models.Configs;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.Services;
using LYSoft.Libs.Wpf.WpfUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IODataPlatform.Extensions;

/// <summary>
/// 依赖注入容器配置扩展类
/// 提供统一的服务注册和配置管理，实现松耦合的架构设计
/// 支持不同环境的配置切换和服务生命周期管理
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// 配置应用程序的所有服务
	/// 按功能模块组织服务注册，确保依赖关系的正确配置
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <param name="configuration">配置对象</param>
	/// <returns>配置完成的服务容器</returns>
	public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.ConfigureAppConfiguration(configuration);
		services.ConfigureCoreServices(configuration);
		services.ConfigureDataServices(configuration);
		services.ConfigureBusinessServices();
		services.ConfigureUIServices();
		services.ConfigureUtilityServices();
		return services;
	}

	/// <summary>
	/// 配置应用程序配置服务
	/// 注册强类型配置和配置管理服务
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <param name="configuration">配置对象</param>
	/// <returns>服务容器</returns>
	private static IServiceCollection ConfigureAppConfiguration(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<ApplicationConfig>(configuration);
		services.Configure<DatabaseConfig>(configuration.GetSection("Database"));
		services.Configure<WebServiceConfig>(configuration.GetSection("WebService"));
		services.Configure<CacheConfig>(configuration.GetSection("Cache"));
		services.Configure<LoggingConfig>(configuration.GetSection("Logging"));
		services.Configure<UIConfig>(configuration.GetSection("UI"));
		services.AddSingleton<IConfigurationService, ConfigurationService>();
		return services;
	}

	/// <summary>
	/// 配置核心基础服务
	/// 包括日志、缓存等基础设施服务
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <param name="configuration">配置对象</param>
	/// <returns>服务容器</returns>
	private static IServiceCollection ConfigureCoreServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddLogging(delegate(ILoggingBuilder builder)
		{
			builder.AddConsole();
			builder.AddDebug();
			builder.SetMinimumLevel(LogLevel.Information);
		});
		services.AddSingleton<ICacheService, MemoryCacheService>();
		return services;
	}

	/// <summary>
	/// 配置数据访问服务
	/// 包括数据库上下文、数据访问服务等
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <param name="configuration">配置对象</param>
	/// <returns>服务容器</returns>
	private static IServiceCollection ConfigureDataServices(this IServiceCollection services, IConfiguration configuration)
	{
		DatabaseConfig databaseConfig = configuration.GetSection("Database").Get<DatabaseConfig>() ?? new DatabaseConfig();
		services.AddSqlSugar(databaseConfig.ConnectionString, databaseConfig.ConnectionString2);
		services.AddSingleton<DatabaseService>();
		return services;
	}

	/// <summary>
	/// 配置业务服务
	/// 包括存储服务、导航服务等核心业务逻辑
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <returns>服务容器</returns>
	private static IServiceCollection ConfigureBusinessServices(this IServiceCollection services)
	{
		services.AddSingleton<StorageService>();
		services.AddSingleton<NavigationParameterService>();
		services.AddScoped<CloudExportConfigService>();
		services.AddAllViews();
		services.AddHostedService<ApplicationHostService>();
		return services;
	}

	/// <summary>
	/// 配置用户界面服务
	/// 包括消息服务、选择器服务等UI相关服务
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <returns>服务容器</returns>
	private static IServiceCollection ConfigureUIServices(this IServiceCollection services)
	{
		services.AddSingleton<IMessageService, WpfUIMessageService>();
		services.AddSingleton<IPickerService, PickerService>();
		return services;
	}

	/// <summary>
	/// 配置工具服务
	/// 包括Excel、PDF、Word处理等工具类服务
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <returns>服务容器</returns>
	private static IServiceCollection ConfigureUtilityServices(this IServiceCollection services)
	{
		services.AddSingleton<ExcelService>();
		services.AddSingleton<PdfService>();
		services.AddSingleton<WordService>();
		return services;
	}

	/// <summary>
	/// 配置视图模型服务
	/// 使用自动扫描和注册机制，无需手动配置每个视图模型
	/// 所有视图和视图模型已通过 AddAllViews() 扩展方法自动注册
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <returns>服务容器</returns>
	/// <remarks>
	/// 该方法已被废弃，所有视图和视图模型现在通过 AddAllViews() 自动注册
	/// </remarks>
	[Obsolete("该方法已被废弃，请使用 AddAllViews() 扩展方法")]
	public static IServiceCollection ConfigureViewModels(this IServiceCollection services)
	{
		return services;
	}

	/// <summary>
	/// 配置开发环境特定服务
	/// 在开发模式下注册额外的调试和开发工具
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <returns>服务容器</returns>
	public static IServiceCollection ConfigureDevelopmentServices(this IServiceCollection services)
	{
		services.AddLogging(delegate(ILoggingBuilder builder)
		{
			builder.SetMinimumLevel(LogLevel.Debug);
			builder.AddFilter("Microsoft", LogLevel.Warning);
			builder.AddFilter("System", LogLevel.Warning);
		});
		return services;
	}

	/// <summary>
	/// 配置生产环境特定服务
	/// 在生产模式下优化性能和安全性配置
	/// </summary>
	/// <param name="services">服务容器</param>
	/// <returns>服务容器</returns>
	public static IServiceCollection ConfigureProductionServices(this IServiceCollection services)
	{
		services.AddLogging(delegate(ILoggingBuilder builder)
		{
			builder.SetMinimumLevel(LogLevel.Information);
			builder.AddFilter("Microsoft", LogLevel.Error);
			builder.AddFilter("System", LogLevel.Error);
		});
		return services;
	}

	/// <summary>
	/// 验证服务注册配置
	/// 确保所有必需的服务都已正确注册，防止运行时缺少关键依赖
	/// 检查核心服务的可用性，包括日志、配置、缓存和数据库服务
	/// </summary>
	/// <param name="serviceProvider">服务提供者，用于获取已注册的服务实例</param>
	/// <returns>验证是否通过，true表示所有必需服务都可用，false表示缺少关键服务</returns>
	/// <exception cref="T:System.Exception">当服务获取过程中发生异常时抛出</exception>
	public static bool ValidateServiceRegistration(this IServiceProvider serviceProvider)
	{
		ILogger logger = null;
		try
		{
			logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("服务验证");
			IConfigurationService service = serviceProvider.GetService<IConfigurationService>();
			ICacheService service2 = serviceProvider.GetService<ICacheService>();
			SqlSugarContext service3 = serviceProvider.GetService<SqlSugarContext>();
			if (service == null || service2 == null || service3 == null)
			{
				string text = $"关键服务注册不完整: ConfigService={service != null}, CacheService={service2 != null}, DbContext={service3 != null}";
				if (logger != null)
				{
					logger.LogError(text);
				}
				else
				{
					Console.WriteLine(text);
				}
				return false;
			}
			string text2 = "服务注册验证通过，所有必需服务都已正确注册";
			if (logger != null)
			{
				logger.LogInformation(text2);
			}
			else
			{
				Console.WriteLine(text2);
			}
			return true;
		}
		catch (Exception ex)
		{
			string value = "服务注册验证过程中发生异常: " + ex.Message;
			if (logger != null)
			{
				logger.LogError(ex, "服务注册验证过程中发生异常");
			}
			else
			{
				Console.WriteLine(value);
			}
			return false;
		}
	}
}
