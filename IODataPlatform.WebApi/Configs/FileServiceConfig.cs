﻿namespace IODataPlatform.WebApi.Configs;

/// <summary>
/// 文件服务配置类
/// 定义Web API文件服务的基本配置参数
/// 包含本地文件存储根目录和API访问安全密钥
/// </summary>
public class FileServiceConfig {
    /// <summary>
    /// 本地文件存储的根目录路径
    /// 所有上传的文件都将存储在此目录及其子目录中
    /// </summary>
    public string LocalRootDir { get; set; } = string.Empty;
    
    /// <summary>
    /// API访问安全密钥
    /// 用于验证客户端请求的合法性，保证文件操作的安全性
    /// </summary>
    public string Key { get; set; } = string.Empty;
}