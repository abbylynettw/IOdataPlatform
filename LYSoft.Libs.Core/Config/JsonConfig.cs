﻿﻿﻿#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

using System.ComponentModel;
using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LYSoft.Libs.Config;

/// <summary>
/// JSON格式配置文件的核心管理类
/// 提供内部共享的配置根目录管理，用于统一所有JSON配置文件的存储位置
/// 通过静态属性集中管理配置文件的根路径，确保配置文件存储的一致性
/// </summary>
internal class JsonConfig {

    /// <summary>
    /// 配置文件的根目录路径
    /// 所有JSON配置文件都将存储在此目录及其子目录中
    /// 通过SetConfigBasePath扩展方法进行设置
    /// </summary>
    internal static string ConfigRootDir { get; set; } = string.Empty;

}

/// <summary>
/// 泛型JSON配置文件管理类
/// 实现了INotifyPropertyChanged接口，支持配置数据的双向绑定和变更通知
/// 提供完整的JSON配置文件的加载、保存和实时监控功能
/// 支持自动创建不存在的配置文件，并在JSON解析失败时自动初始化为默认值
/// </summary>
/// <typeparam name="T">配置对象的类型，必须是引用类型</typeparam>
public class JsonConfig<T> : INotifyPropertyChanged where T : class {
    private T config;

    internal JsonConfig(string filename) {
        if (string.IsNullOrEmpty(filename)) { throw new ArgumentNullException(nameof(filename)); }
        FileName = filename;
        var absolutePath = Path.Combine(JsonConfig.ConfigRootDir, FileName);
        if (!File.Exists(absolutePath)) { File.Create(absolutePath).Dispose(); }

        try {
            var result = JsonSerializer.Deserialize<T>(File.ReadAllText(Path.Combine(JsonConfig.ConfigRootDir, FileName)));
            if (result != null) { Config = result; }
        } catch (JsonException) {
            Config = Activator.CreateInstance<T>();
        }

    }

    /// <summary>
    /// JSON配置文件的相对路径名称
    /// 相对于配置根目录的文件路径，支持子目录结构
    /// </summary>
    internal string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 配置对象实例
    /// 实际存储和管理的配置数据，支持属性变更通知
    /// </summary>
    public T Config {
        get => config;
        set {
            if (config == value) { return; }
            config = value;
            PropertyChanged?.Invoke(this, new(nameof(Config)));
        }
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 异步加载配置文件内容
    /// 从文件系统中重新读取JSON配置文件，并更新Config属性
    /// 包含重试机制，在文件被占用时会等待并重试
    /// 如果JSON解析失败，会初始化为类型的默认实例
    /// </summary>
    /// <returns>返回表示异步操作的Task</returns>
    public async Task LoadAsync() {
        var loaded = false;
        while (!loaded) {
            try {
                try {
                    var result = JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(Path.Combine(JsonConfig.ConfigRootDir, FileName)));
                    if (result != null) { Config = result; }
                } catch (JsonException) {
                    Config = Activator.CreateInstance<T>();
                }
                loaded = true;
            } catch {
                await Task.Delay(10);
            }
        }
    }

    /// <summary>
    /// 异步保存配置内容到文件
    /// 将当前Config属性的内容序列化为JSON格式并写入文件
    /// 包含重试机制，在文件被占用时会等待并重试
    /// </summary>
    /// <returns>返回表示异步操作的Task</returns>
    public async Task SaveAsync() {
        var saved = false;
        while (!saved) {
            try {
                await File.WriteAllTextAsync(Path.Combine(JsonConfig.ConfigRootDir, FileName), JsonSerializer.Serialize(Config));
                saved = true;
            } catch {
                await Task.Delay(10);
            }
        }
    }

}

/// <summary>
/// JSON配置系统的扩展方法类
/// 为.NET Core的配置系统提供JSON配置文件的根目录设置功能
/// </summary>
public static class JsonConfigExtensions {

    /// <summary>
    /// 设置JSON配置文件的根目录路径
    /// 所有JSON配置文件都将相对于此目录进行存储和读取
    /// 如果指定的目录不存在，会自动创建
    /// </summary>
    /// <param name="builder">配置构建器实例</param>
    /// <param name="basePath">配置文件的根目录绝对路径</param>
    /// <returns>返回配置构建器，支持链式调用</returns>
    public static IConfigurationBuilder SetConfigBasePath(this IConfigurationBuilder builder, string basePath) {
        JsonConfig.ConfigRootDir = basePath;
        if (!Directory.Exists(basePath)) { Directory.CreateDirectory(basePath); }
        return builder;
    }
   
}