namespace IODataPlatform.Models.Configs;

/// <summary>
/// Web服务配置类
/// 包含与后端Web API通信所需的基本配置信息
/// 用于StorageService中的Web文件上传下载功能
/// </summary>
public class WebServiceConfig
{
	/// <summary>Web API的基础URL地址</summary>
	public string BaseUrl { get; set; } = string.Empty;

	/// <summary>文件服务的基础URL地址</summary>
	public string FileBaseUrl { get; set; } = string.Empty;

	/// <summary>API访问的安全密钥</summary>
	public string Key { get; set; } = string.Empty;
}
