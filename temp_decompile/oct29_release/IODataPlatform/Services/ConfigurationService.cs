using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using IODataPlatform.Models.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IODataPlatform.Services;

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

	private readonly object _lock = new object();

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
		_fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(_configFilePath), Path.GetFileName(_configFilePath));
		_fileWatcher.Changed += OnConfigFileChanged;
		_fileWatcher.EnableRaisingEvents = true;
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
			return _cachedConfig ?? (_cachedConfig = LoadConfiguration());
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
			IConfigurationSection section = _configuration.GetSection(sectionName);
			T result = section.Get<T>() ?? new T();
			_logger.LogDebug("成功获取配置节点: {SectionName}", sectionName);
			return result;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "获取配置节点失败: {SectionName}", sectionName);
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
		_ = 1;
		try
		{
			ValidationResult validationResult = ValidateConfiguration(config);
			if (!validationResult.IsValid)
			{
				_logger.LogError("配置验证失败: {Errors}", string.Join(", ", validationResult.Errors));
				return false;
			}
			await CreateBackupAsync();
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				WriteIndented = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			};
			string contents = JsonSerializer.Serialize(config, options);
			string tempFile = _configFilePath + ".tmp";
			await File.WriteAllTextAsync(tempFile, contents);
			if (File.Exists(tempFile))
			{
				File.Move(tempFile, _configFilePath, overwrite: true);
				lock (_lock)
				{
					_cachedConfig = config;
				}
				_logger.LogInformation("配置更新成功");
				this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(config));
				return true;
			}
			return false;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "更新配置失败");
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
	/// <exception cref="T:System.ArgumentNullException">当配置对象为null时抛出</exception>
	public ValidationResult ValidateConfiguration(ApplicationConfig config)
	{
		if (config == null)
		{
			throw new ArgumentNullException("config", "配置对象不能为空");
		}
		ValidationContext validationContext = new ValidationContext(config);
		List<System.ComponentModel.DataAnnotations.ValidationResult> list = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
		bool flag = Validator.TryValidateObject(config, validationContext, list, validateAllProperties: true);
		flag &= ValidateNestedProperties(config, list);
		return new ValidationResult
		{
			IsValid = flag,
			Errors = list.Select((System.ComponentModel.DataAnnotations.ValidationResult r) => r.ErrorMessage ?? "未知验证错误").ToList()
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
		bool flag = true;
		PropertyInfo[] properties = obj.GetType().GetProperties();
		PropertyInfo[] array = properties;
		foreach (PropertyInfo property in array)
		{
			if (!property.PropertyType.IsClass || !(property.PropertyType != typeof(string)))
			{
				continue;
			}
			object value = property.GetValue(obj);
			if (value == null)
			{
				continue;
			}
			ValidationContext validationContext = new ValidationContext(value);
			List<System.ComponentModel.DataAnnotations.ValidationResult> list = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
			if (!Validator.TryValidateObject(value, validationContext, list, validateAllProperties: true))
			{
				flag = false;
				foreach (System.ComponentModel.DataAnnotations.ValidationResult item in list)
				{
					results.Add(new System.ComponentModel.DataAnnotations.ValidationResult(property.Name + "." + item.ErrorMessage, item.MemberNames.Select((string x) => property.Name + "." + x)));
				}
			}
			flag &= ValidateNestedProperties(value, results);
		}
		return flag;
	}

	/// <summary>
	/// 从配置文件加载配置对象
	/// </summary>
	/// <returns>配置对象</returns>
	private ApplicationConfig LoadConfiguration()
	{
		try
		{
			ApplicationConfig applicationConfig = new ApplicationConfig();
			_configuration.Bind(applicationConfig);
			_logger.LogDebug("配置加载成功");
			return applicationConfig;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "配置加载失败，使用默认配置");
			return new ApplicationConfig();
		}
	}

	/// <summary>
	/// 创建配置文件备份
	/// </summary>
	private async Task CreateBackupAsync()
	{
		_ = 1;
		try
		{
			if (!File.Exists(_configFilePath))
			{
				return;
			}
			string text = $"{_configFilePath}.bak.{DateTime.Now:yyyyMMddHHmmss}";
			string path = text;
			await File.WriteAllTextAsync(path, await File.ReadAllTextAsync(_configFilePath));
			string directoryName = Path.GetDirectoryName(_configFilePath);
			IEnumerable<string> enumerable = (from f in Directory.GetFiles(directoryName, "appsettings.json.bak.*")
				orderby f descending
				select f).Skip(5);
			foreach (string item in enumerable)
			{
				File.Delete(item);
			}
		}
		catch (Exception exception)
		{
			_logger.LogWarning(exception, "创建配置备份失败");
		}
	}

	/// <summary>
	/// 配置文件变更事件处理
	/// </summary>
	private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
	{
		try
		{
			Task.Delay(500).ContinueWith(delegate
			{
				lock (_lock)
				{
					ApplicationConfig cachedConfig = _cachedConfig;
					_cachedConfig = LoadConfiguration();
					if (cachedConfig != null)
					{
						this.ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(_cachedConfig));
					}
				}
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "处理配置文件变更失败");
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
