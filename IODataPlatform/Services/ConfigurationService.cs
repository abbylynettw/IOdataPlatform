using IODataPlatform.Models.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json;

namespace IODataPlatform.Services;

/// <summary>
/// 配置管理服务接口
/// 定义配置读取、验证、更新和监控的核心功能
/// 支持强类型配置访问和实时配置变更通知
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// 获取应用程序配置
    /// </summary>
    ApplicationConfig GetConfiguration();

    /// <summary>
    /// 获取特定类型的配置节点
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="sectionName">配置节点名称</param>
    /// <returns>配置对象</returns>
    T GetSection<T>(string sectionName) where T : class, new();

    /// <summary>
    /// 更新配置并保存到文件
    /// </summary>
    /// <param name="config">新的配置对象</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UpdateConfigurationAsync(ApplicationConfig config);

    /// <summary>
    /// 验证配置的有效性
    /// </summary>
    /// <param name="config">要验证的配置对象</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateConfiguration(ApplicationConfig config);

    /// <summary>
    /// 配置变更事件
    /// </summary>
    event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
}

/// <summary>
/// 配置管理服务实现类
/// 提供完整的配置管理功能，包括读取、验证、保存和监控
/// 实现配置热更新和自动备份机制
/// </summary>
public class ConfigurationService : IConfigurationService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly string _configFilePath;
    private readonly FileSystemWatcher _fileWatcher;
    private ApplicationConfig? _cachedConfig;
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// 配置变更事件
    /// 当配置文件发生变化时触发，允许其他服务响应配置变更
    /// </summary>
    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    /// <summary>
    /// 构造函数
    /// 初始化配置服务，设置文件监控和缓存机制
    /// </summary>
    /// <param name="configuration">配置根对象</param>
    /// <param name="logger">日志记录器</param>
    public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Settings", "appsettings.json");

        // 初始化文件监控
        _fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(_configFilePath)!, Path.GetFileName(_configFilePath));
        _fileWatcher.Changed += OnConfigFileChanged;
        _fileWatcher.EnableRaisingEvents = true;

        // 预加载配置
        _cachedConfig = LoadConfiguration();
    }

    /// <summary>
    /// 获取完整的应用程序配置
    /// 实现配置缓存机制，提高访问性能
    /// </summary>
    /// <returns>应用程序配置对象</returns>
    public ApplicationConfig GetConfiguration()
    {
        lock (_lock)
        {
            return _cachedConfig ??= LoadConfiguration();
        }
    }

    /// <summary>
    /// 获取特定配置节点
    /// 支持强类型配置访问，提供编译时类型检查
    /// </summary>
    /// <typeparam name="T">配置节点类型</typeparam>
    /// <param name="sectionName">配置节点名称</param>
    /// <returns>配置节点对象</returns>
    public T GetSection<T>(string sectionName) where T : class, new()
    {
        try
        {
            var section = _configuration.GetSection(sectionName);
            var result = section.Get<T>() ?? new T();
            
            _logger.LogDebug("成功获取配置节点: {SectionName}", sectionName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取配置节点失败: {SectionName}", sectionName);
            return new T();
        }
    }

    /// <summary>
    /// 异步更新配置并保存到文件
    /// 实现原子性操作，确保配置更新的一致性
    /// 包含自动备份机制，防止配置丢失
    /// </summary>
    /// <param name="config">新的配置对象</param>
    /// <returns>操作是否成功</returns>
    public async Task<bool> UpdateConfigurationAsync(ApplicationConfig config)
    {
        try
        {
            // 验证配置
            var validationResult = ValidateConfiguration(config);
            if (!validationResult.IsValid)
            {
                _logger.LogError("配置验证失败: {Errors}", string.Join(", ", validationResult.Errors));
                return false;
            }

            // 创建备份
            await CreateBackupAsync();

            // 序列化配置
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var jsonContent = JsonSerializer.Serialize(config, options);

            // 原子性写入
            var tempFile = _configFilePath + ".tmp";
            await File.WriteAllTextAsync(tempFile, jsonContent);
            
            // 验证写入的文件
            if (File.Exists(tempFile))
            {
                File.Move(tempFile, _configFilePath, true);
                
                // 更新缓存
                lock (_lock)
                {
                    _cachedConfig = config;
                }

                _logger.LogInformation("配置更新成功");
                
                // 触发配置变更事件
                ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(config));
                
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新配置失败");
            return false;
        }
    }

    /// <summary>
    /// 验证配置对象的有效性
    /// 使用数据注解属性进行全面的配置验证，支持嵌套对象的递归验证
    /// 确保所有配置项都符合定义的验证规则，避免运行时配置错误
    /// </summary>
    /// <param name="config">要验证的配置对象</param>
    /// <returns>验证结果，包含验证状态和错误信息列表</returns>
    /// <exception cref="ArgumentNullException">当配置对象为null时抛出</exception>
    public ValidationResult ValidateConfiguration(ApplicationConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "配置对象不能为空");
        }

        var context = new ValidationContext(config);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        
        // 使用标准验证器进行基本验证
        var isValid = Validator.TryValidateObject(config, context, results, true);
        
        // 递归验证嵌套属性
        isValid &= ValidateNestedProperties(config, results);

        return new ValidationResult
        {
            IsValid = isValid,
            Errors = results.Select(r => r.ErrorMessage ?? "未知验证错误").ToList()
        };
    }

    /// <summary>
    /// 递归验证嵌套属性
    /// 对配置对象的所有复杂类型属性进行深度验证
    /// </summary>
    /// <param name="obj">要验证的对象</param>
    /// <param name="results">验证结果列表</param>
    /// <returns>验证是否通过</returns>
    private bool ValidateNestedProperties(object obj, ICollection<System.ComponentModel.DataAnnotations.ValidationResult> results)
    {
        var isValid = true;
        var properties = obj.GetType().GetProperties();
        
        foreach (var property in properties)
        {
            // 只验证复杂类型属性（排除字符串）
            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var value = property.GetValue(obj);
                if (value != null)
                {
                    var nestedContext = new ValidationContext(value);
                    var nestedResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                    
                    // 验证嵌套对象
                    var nestedValid = Validator.TryValidateObject(value, nestedContext, nestedResults, true);
                    
                    if (!nestedValid)
                    {
                        isValid = false;
                        // 添加带有属性路径前缀的错误信息
                        foreach (var result in nestedResults)
                        {
                            results.Add(new System.ComponentModel.DataAnnotations.ValidationResult(
                                $"{property.Name}.{result.ErrorMessage}",
                                result.MemberNames.Select(x => $"{property.Name}.{x}")));
                        }
                    }
                    
                    // 继续递归验证更深层的嵌套属性
                    isValid &= ValidateNestedProperties(value, results);
                }
            }
        }
        
        return isValid;
    }

    /// <summary>
    /// 从配置文件加载配置对象
    /// </summary>
    /// <returns>配置对象</returns>
    private ApplicationConfig LoadConfiguration()
    {
        try
        {
            var config = new ApplicationConfig();
            _configuration.Bind(config);
            
            _logger.LogDebug("配置加载成功");
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "配置加载失败，使用默认配置");
            return new ApplicationConfig();
        }
    }

    /// <summary>
    /// 创建配置文件备份
    /// </summary>
    private async Task CreateBackupAsync()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var backupPath = $"{_configFilePath}.bak.{DateTime.Now:yyyyMMddHHmmss}";
                await File.WriteAllTextAsync(backupPath, await File.ReadAllTextAsync(_configFilePath));
                
                // 清理旧备份（保留最近5个）
                var backupDir = Path.GetDirectoryName(_configFilePath)!;
                var backupFiles = Directory.GetFiles(backupDir, "appsettings.json.bak.*")
                    .OrderByDescending(f => f)
                    .Skip(5);
                    
                foreach (var oldBackup in backupFiles)
                {
                    File.Delete(oldBackup);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "创建配置备份失败");
        }
    }

    /// <summary>
    /// 配置文件变更事件处理
    /// </summary>
    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            // 延迟处理，避免文件写入未完成
            Task.Delay(500).ContinueWith(_ =>
            {
                lock (_lock)
                {
                    var oldConfig = _cachedConfig;
                    _cachedConfig = LoadConfiguration();
                    
                    if (oldConfig != null)
                    {
                        ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(_cachedConfig));
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理配置文件变更失败");
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _fileWatcher?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// 配置变更事件参数类
/// 包含变更后的配置信息
/// </summary>
public class ConfigurationChangedEventArgs : EventArgs
{
    /// <summary>
    /// 新的配置对象
    /// </summary>
    public ApplicationConfig NewConfiguration { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="newConfiguration">新的配置对象</param>
    public ConfigurationChangedEventArgs(ApplicationConfig newConfiguration)
    {
        NewConfiguration = newConfiguration;
    }
}

/// <summary>
/// 配置验证结果类
/// 包含验证状态和错误信息
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// 验证是否通过
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// 验证错误信息列表
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

