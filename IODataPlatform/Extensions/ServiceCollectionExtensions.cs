using IODataPlatform.Models.Configs;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf;
using LYSoft.Libs.Wpf.WpfUI;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LYSoft.Libs.Wpf.Services;

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
        // 注册配置服务
        services.ConfigureAppConfiguration(configuration);
        
        // 注册核心基础服务
        services.ConfigureCoreServices(configuration);
        
        // 注册数据访问服务
        services.ConfigureDataServices(configuration);
        
        // 注册业务服务
        services.ConfigureBusinessServices();
        
        // 注册UI服务
        services.ConfigureUIServices();
        
        // 注册工具服务
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
        // 注册强类型配置
        services.Configure<ApplicationConfig>(configuration);
        services.Configure<DatabaseConfig>(configuration.GetSection("Database"));
        services.Configure<WebServiceConfig>(configuration.GetSection("WebService"));
        services.Configure<CacheConfig>(configuration.GetSection("Cache"));
        services.Configure<LoggingConfig>(configuration.GetSection("Logging"));
        services.Configure<UIConfig>(configuration.GetSection("UI"));

        // 注册配置管理服务
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
        // 配置日志服务
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // 注册缓存服务
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
        // 获取数据库连接字符串
        var databaseConfig = configuration.GetSection("Database").Get<DatabaseConfig>() ?? new DatabaseConfig();
        
        // 注册数据库上下文
        services.AddSqlSugar(databaseConfig.ConnectionString, databaseConfig.ConnectionString2);

        // 注册数据库服务
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
        // 注册存储服务
        services.AddSingleton<StorageService>();

        // 注册导航服务
        services.AddSingleton<NavigationParameterService>();
        
        // 注册云端导出配置服务
        services.AddScoped<CloudExportConfigService>();
        
        // 注册所有视图和视图模型（使用扩展方法）
        services.AddAllViews();

        // 注册应用程序主机服务
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
        // 注册消息服务
        services.AddSingleton<IMessageService, WpfUIMessageService>();

        // 注册选择器服务
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
        // 注册Excel服务
        services.AddSingleton<ExcelService>();

        // 注册PDF服务
        services.AddSingleton<PdfService>();

        // 注册Word服务
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
        // 所有视图和视图模型已通过 AddAllViews() 方法自动注册
        // 这里不需要再手动注册
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
        // 开发环境下的特殊配置
        services.AddLogging(builder =>
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
        // 生产环境下的优化配置
        services.AddLogging(builder =>
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
    /// <exception cref="Exception">当服务获取过程中发生异常时抛出</exception>
    public static bool ValidateServiceRegistration(this IServiceProvider serviceProvider)
    {
        ILogger? logger = null;
        try
        {
            // 使用日志工厂创建日志器，避免依赖特定类型
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            logger = loggerFactory?.CreateLogger("服务验证");
            
            // 验证核心服务的可用性
            var configService = serviceProvider.GetService<IConfigurationService>();
            var cacheService = serviceProvider.GetService<ICacheService>();
            var dbContext = serviceProvider.GetService<SqlSugarContext>();

            // 检查关键服务是否都已正确注册
            if (configService == null || cacheService == null || dbContext == null)
            {
                var errorMessage = $"关键服务注册不完整: " +
                    $"ConfigService={configService != null}, " +
                    $"CacheService={cacheService != null}, " +
                    $"DbContext={dbContext != null}";
                
                if (logger != null)
                {
                    logger.LogError(errorMessage);
                }
                else
                {
                    Console.WriteLine(errorMessage);
                }
                return false;
            }

            var successMessage = "服务注册验证通过，所有必需服务都已正确注册";
            if (logger != null)
            {
                logger.LogInformation(successMessage);
            }
            else
            {
                Console.WriteLine(successMessage);
            }
            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = $"服务注册验证过程中发生异常: {ex.Message}";
            
            // 如果日志服务不可用，则输出到控制台
            if (logger != null)
            {
                logger.LogError(ex, "服务注册验证过程中发生异常");
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
            return false;
        }
    }
}