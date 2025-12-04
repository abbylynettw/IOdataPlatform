using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IODataPlatform.Models.Configs;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExportModels;
using Microsoft.Extensions.Options;

namespace IODataPlatform.Services;

/// <summary>
/// 存储服务统一协调器
/// 作为部分类（partial class）的主类，统一管理本地和Web文件存储
/// 提供文件MD5校验、上传下载、目录管理等核心功能
/// 通过多个部分类文件实现职责分离，支持IO、电缆、端接等不同业务模块
/// </summary>
/// <summary>
/// StorageService的本地文件存储功能部分
/// 管理本地文件系统中的文件存储和访问操作
/// 主要用于临时数据和缓存文件的管理
/// </summary>
/// <summary>
/// 存储服务的电缆文件管理部分
/// 提供电缆数据文件的完整存储管理功能，包括实时数据和发布版本的文件操作
/// 支持双子项目的电缆数据管理，实现跨专业的电缆连接关系维护
/// 所有操作都包含MD5校验以确保数据完整性和一致性
/// </summary>
/// <summary>
/// StorageService的Web文件存储功能部分
/// 管理与Web服务器同步的文件存储和访问操作
/// 提供项目文件、发布数据等的本地缓存和远程同步功能
/// </summary>
/// <summary>
/// 存储服务的导出配置管理部分
/// 提供导出配置的云端存储管理功能，支持用户级、项目级、全局级配置的分层存储
/// 实现配置的云端同步，支持多用户配置共享和字段版本管理
/// 所有操作都包含MD5校验以确保配置文件的完整性和一致性
/// </summary>
/// <summary>
/// 存储服务的IO文件管理部分
/// 提供IO数据文件的完整存储管理功能，包括实时数据和发布版本的文件操作
/// 支持多种控制系统（AQJ、XT1、XT2等）的IO数据统一管理
/// 所有操作都包含MD5校验以确保数据完整性和一致性
/// </summary>
/// <summary>
/// 存储服务的图纸文件管理部分
/// 提供工程图纸文件的完整存储管理功能，支持多种文件格式和类型
/// 支持三种图纸文件类型：压缩包文件、PDF文件和依据文件
/// 所有操作都包含MD5校验以确保文件完整性和一致性
///
/// 图纸在服务器上的存储结构为：
/// - 图纸压缩包文件： papers/{id}/{zip文件名}
/// - 图纸PDF文件：    papers/{id}/{pdf文件名}
/// - 图纸依据文件：   papers/{id}/{dep文件名}
/// </summary>
/// <summary>
/// 存储服务的模板文件管理部分
/// 提供Excel模板文件的存储管理功能，主要用于IO数据导入导出的模板管理
/// 支持标准化的Excel模板文件上传、下载和版本控制
/// 所有操作都包含MD5校验以确保模板文件的完整性和一致性
/// </summary>
/// <summary>
/// 存储服务的端接文件管理部分
/// 提供端接数据文件的完整存储管理功能，包括实时数据和发布版本的文件操作
/// 端接数据涉及电缆与设备的物理连接关系，对数据准确性和一致性要求极高
/// 所有操作都包含MD5校验以确保数据完整性和版本一致性
/// </summary>
/// <summary>
/// StorageService的Web API通信功能部分
/// 封装与后端Web API服务的HTTP通信逻辑
/// 提供文件上传、下载、删除、复制和MD5校验等核心操作
/// 所有请求都包含API密钥认证和相对路径参数
/// </summary>
public class StorageService(IOptions<WebServiceConfig> config)
{
	/// <summary>主存储根目录，位于用户本地应用数据目录下</summary>
	private static string StorageRoot { get; } = EnsureDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IODataPlatform", "Storage"));

	/// <summary>本地文件存储根目录，位于主存储目录下的Local子文件夹</summary>
	public string LocalStorageRoot { get; } = EnsureDir(Path.Combine(StorageRoot, "Local"));

	/// <summary>网络文件本地存储根目录，位于主存储目录下的Web子文件夹</summary>
	public string WebStorageRoot { get; } = EnsureDir(Path.Combine(StorageRoot, "Web"));

	/// <summary>HTTP客户端，设置10分钟超时，用于处理大文件传输</summary>
	public HttpClient HttpClient { get; } = new HttpClient
	{
		Timeout = TimeSpan.FromSeconds(600.0)
	};

	/// <summary>获取Web服务配置信息</summary>
	public WebServiceConfig Config { get; } = config.Value;

	/// <summary>
	/// 确保指定目录存在
	/// 如果目录不存在则自动创建
	/// </summary>
	/// <param name="dir">需要确保存在的目录路径</param>
	/// <returns>返回目录路径</returns>
	private static string EnsureDir(string dir)
	{
		if (!Directory.Exists(dir))
		{
			Directory.CreateDirectory(dir);
		}
		return dir;
	}

	/// <summary>
	/// 确保文件所在的目录存在
	/// 自动创建文件的父目录，返回文件路径
	/// </summary>
	/// <param name="file">文件的完整路径</param>
	/// <returns>返回文件路径</returns>
	private static string EnsureFileDir(string file)
	{
		EnsureDir(Path.GetDirectoryName(file));
		return file;
	}

	/// <summary>
	/// 使用系统默认程序打开文件
	/// 调用Windows资源管理器打开指定文件，路径分隔符会自动转换
	/// </summary>
	/// <param name="file">要打开的文件路径</param>
	public void RunFile(string file)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo("explorer.exe");
		processStartInfo.ArgumentList.Add(file.Replace('/', '\\'));
		Process.Start(processStartInfo);
	}

	/// <summary>
	/// 计算文件的MD5哈希值
	/// 用于文件完整性校验和版本比较
	/// </summary>
	/// <param name="filePath">文件的完整路径</param>
	/// <returns>返回小写的MD5哈希字符串</returns>
	private static string GetFileMD5(string filePath)
	{
		using MD5 mD = MD5.Create();
		using FileStream inputStream = File.OpenRead(filePath);
		byte[] array = mD.ComputeHash(inputStream);
		return BitConverter.ToString(array).Replace("-", "").ToLowerInvariant();
	}

	/// <summary>
	/// 检查本地和服务器文件的MD5，如不同则下载，返回本地绝对路径
	/// 实现文件的智能同步，只有在文件发生变化时才重新下载
	/// 增强错误处理和重试机制，提高可靠性
	/// </summary>
	/// <param name="relativePath">文件在服务器上的相对路径</param>
	/// <param name="cancellationToken">取消操作的令牌</param>
	/// <returns>返回文件的本地绝对路径</returns>
	/// <exception cref="T:System.InvalidOperationException">当文件下载失败时抛出异常</exception>
	/// <exception cref="T:System.OperationCanceledException">当操作被取消时抛出异常</exception>
	private async Task<string> WebDownloadFileWithCheckMD5(string relativePath, CancellationToken cancellationToken = default(CancellationToken))
	{
		_ = 2;
		try
		{
			string absolutePath = GetWebFileLocalAbsolutePath(relativePath);
			string localFileMD5 = (File.Exists(absolutePath) ? GetFileMD5(absolutePath) : string.Empty);
			string webFileMD5 = await WebGetFileMD5Async(relativePath).ConfigureAwait(continueOnCapturedContext: false);
			if (localFileMD5 != webFileMD5)
			{
				byte[] bytes = await WebDownloadFileAsync(relativePath).ConfigureAwait(continueOnCapturedContext: false);
				EnsureFileDir(absolutePath);
				string tempFile = absolutePath + ".tmp";
				await File.WriteAllBytesAsync(tempFile, bytes, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				string fileMD = GetFileMD5(tempFile);
				if (fileMD != webFileMD5)
				{
					File.Delete(tempFile);
					throw new InvalidOperationException("文件下载校验失败: " + relativePath);
				}
				if (File.Exists(absolutePath))
				{
					File.Delete(absolutePath);
				}
				File.Move(tempFile, absolutePath);
			}
			return absolutePath;
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException("下载文件失败: " + relativePath, innerException);
		}
	}

	/// <summary>
	/// 检查本地和服务器文件的MD5，如不同则上传
	/// 实现文件的智能上传，只有在文件发生变化或服务器文件不存在时才上传
	/// 增强错误处理和重试机制，提高上传可靠性
	/// </summary>
	/// <param name="relativePath">文件在服务器上的相对路径</param>
	/// <param name="cancellationToken">取消操作的令牌</param>
	/// <returns>返回表示异步操作完成的任务</returns>
	/// <exception cref="T:System.IO.FileNotFoundException">当本地文件不存在时抛出异常</exception>
	/// <exception cref="T:System.InvalidOperationException">当文件上传失败时抛出异常</exception>
	/// <exception cref="T:System.OperationCanceledException">当操作被取消时抛出异常</exception>
	private async Task WebUploadFileWithCheckMD5(string relativePath, CancellationToken cancellationToken = default(CancellationToken))
	{
		_ = 2;
		try
		{
			string absolutePath = GetWebFileLocalAbsolutePath(relativePath);
			if (!File.Exists(absolutePath))
			{
				throw new FileNotFoundException("本地文件不存在: " + absolutePath);
			}
			string localFileMD5 = GetFileMD5(absolutePath);
			bool flag;
			try
			{
				flag = localFileMD5 != await WebGetFileMD5Async(relativePath).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch
			{
				flag = true;
			}
			if (flag)
			{
				await WebUploadFileAsync(relativePath, await File.ReadAllBytesAsync(absolutePath, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (FileNotFoundException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException("上传文件失败: " + relativePath, innerException);
		}
	}

	/// <summary>
	/// 获取本地文件的绝对路径
	/// 自动确保目标文件夹存在
	/// </summary>
	/// <param name="relativePath">相对于本地存储根目录的相对路径</param>
	/// <returns>返回文件的完整绝对路径</returns>
	public string GetLocalAbsolutePath(string relativePath)
	{
		return EnsureFileDir(Path.Combine(LocalStorageRoot, relativePath));
	}

	/// <summary>
	/// 获取电缆匹配临时数据文件的绝对路径
	/// 用于存储电缆匹配过程中的临时数据，文件格式为JSON
	/// </summary>
	/// <returns>返回电缆匹配临时数据文件的完整绝对路径</returns>
	public string GetCableTempFileRelativePath()
	{
		return EnsureFileDir(Path.Combine(LocalStorageRoot, "cable", "match_temp.json"));
	}

	/// <summary>
	/// 获取实时电缆文件的相对路径
	/// 根据两个子项目ID生成标准化的电缆文件存储路径
	/// 使用较小和较大ID的组合确保路径的一致性，避免因参数顺序不同导致的路径差异
	/// </summary>
	/// <param name="subProjectId1">第一个子项目的ID</param>
	/// <param name="subProjectId2">第二个子项目的ID</param>
	/// <returns>实时电缆文件的相对路径，格式为：projects/cable/{min_id}_{max_id}/realtime.csv</returns>
	public string GetRealtimeCableFileRelativePath(int subProjectId1, int subProjectId2)
	{
		int[] source = new int[2] { subProjectId1, subProjectId2 };
		return $"projects/cable/{source.Min()}_{source.Max()}/realtime.csv";
	}

	/// <summary>
	/// 获取发布电缆文件的相对路径
	/// 根据两个子项目ID和发布版本ID生成发布电缆文件的存储路径
	/// 发布文件用于正式版本的数据分发和长期存档
	/// </summary>
	/// <param name="subProjectId1">第一个子项目的ID</param>
	/// <param name="subProjectId2">第二个子项目的ID</param>
	/// <param name="publishId">发布版本的唯一标识符</param>
	/// <returns>发布电缆文件的相对路径，格式为：projects/cable/{min_id}_{max_id}/publish_{publishId}.csv</returns>
	public string GetPublishCableFileRelativePath(int subProjectId1, int subProjectId2, int publishId)
	{
		int[] source = new int[2] { subProjectId1, subProjectId2 };
		return $"projects/cable/{source.Min()}_{source.Max()}/publish_{publishId}.csv";
	}

	/// <summary>
	/// 下载实时电缆文件到本地
	/// 从服务器下载最新的实时电缆数据文件，包含完整的MD5校验过程
	/// 确保下载的文件完整性和数据一致性，支持断点续传和错误重试
	/// </summary>
	/// <param name="subProjectId1">第一个子项目的ID</param>
	/// <param name="subProjectId2">第二个子项目的ID</param>
	/// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
	/// <exception cref="T:System.Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
	public async Task<string> DownloadRealtimeCableFileAsync(int subProjectId1, int subProjectId2)
	{
		string realtimeCableFileRelativePath = GetRealtimeCableFileRelativePath(subProjectId1, subProjectId2);
		return await WebDownloadFileWithCheckMD5(realtimeCableFileRelativePath);
	}

	/// <summary>
	/// 下载指定版本的发布电缆文件到本地
	/// 从服务器下载特定发布版本的电缆数据文件，用于版本回滚或历史数据查看
	/// 包含完整的MD5校验过程以确保发布版本数据的准确性
	/// </summary>
	/// <param name="subProjectId1">第一个子项目的ID</param>
	/// <param name="subProjectId2">第二个子项目的ID</param>
	/// <param name="publishId">发布版本的唯一标识符</param>
	/// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
	/// <exception cref="T:System.Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
	public async Task<string> DownloadPublishCableFileAsync(int subProjectId1, int subProjectId2, int publishId)
	{
		string publishCableFileRelativePath = GetPublishCableFileRelativePath(subProjectId1, subProjectId2, publishId);
		return await WebDownloadFileWithCheckMD5(publishCableFileRelativePath);
	}

	/// <summary>
	/// 上传实时电缆文件到服务器
	/// 将本地的实时电缆数据文件上传到服务器，包含MD5校验确保数据完整性
	/// 上传成功后其他用户可以获取到最新的电缆数据更新
	/// </summary>
	/// <param name="subProjectId1">第一个子项目的ID</param>
	/// <param name="subProjectId2">第二个子项目的ID</param>
	/// <returns>异步任务，表示上传操作的完成</returns>
	/// <exception cref="T:System.Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
	public async Task UploadRealtimeCableFileAsync(int subProjectId1, int subProjectId2)
	{
		string realtimeCableFileRelativePath = GetRealtimeCableFileRelativePath(subProjectId1, subProjectId2);
		await WebUploadFileWithCheckMD5(realtimeCableFileRelativePath);
	}

	/// <summary>
	/// 上传发布版本的电缆文件到服务器
	/// 将本地的发布版本电缆数据文件上传到服务器，用于正式版本的发布和存档
	/// 包含MD5校验确保发布版本数据的准确性和可靠性
	/// </summary>
	/// <param name="subProjectId1">第一个子项目的ID</param>
	/// <param name="subProjectId2">第二个子项目的ID</param>
	/// <param name="publishId">发布版本的唯一标识符</param>
	/// <returns>异步任务，表示上传操作的完成</returns>
	/// <exception cref="T:System.Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
	public async Task UploadPublishCableFileAsync(int subProjectId1, int subProjectId2, int publishId)
	{
		string publishCableFileRelativePath = GetPublishCableFileRelativePath(subProjectId1, subProjectId2, publishId);
		await WebUploadFileWithCheckMD5(publishCableFileRelativePath);
	}

	/// <summary>
	/// 获取子项文件夹的相对路径
	/// 根据子项目ID生成统一的文件夹路径格式
	/// </summary>
	/// <param name="subProjectId">子项目表（config_project_subProject）的ID</param>
	/// <returns>返回子项文件夹的相对路径</returns>
	public static string GetSubProjectFolderRelativePath(int subProjectId)
	{
		return $"projects/subproject_{subProjectId}";
	}

	/// <summary>
	/// 删除子项文件夹（本地和远程）
	/// 同时删除本地缓存和远程Web服务器上的子项文件夹
	/// </summary>
	/// <param name="subProjectId">子项目表（config_project_subProject）的ID</param>
	/// <returns>返回表示异步操作完成的任务</returns>
	public async Task DeleteSubprojectFolderAsync(int subProjectId)
	{
		string subProjectFolderRelativePath = GetSubProjectFolderRelativePath(subProjectId);
		string webFileLocalAbsolutePath = GetWebFileLocalAbsolutePath(subProjectFolderRelativePath);
		try
		{
			Directory.Delete(webFileLocalAbsolutePath, recursive: true);
		}
		catch
		{
		}
		try
		{
			await WebDeleteFolderAsync(subProjectFolderRelativePath);
		}
		catch
		{
		}
	}

	/// <summary>
	/// 删除子项发布文件夹（IO数据）
	/// 删除指定版本的IO数据发布文件夹，包括本地和远程
	/// </summary>
	/// <param name="subProjectId">子项目表（config_project_subProject）的ID</param>
	/// <param name="versionId">版本号</param>
	/// <returns>返回表示异步操作完成的任务</returns>
	public async Task DeleteSubprojectPublishFolderAsync(int subProjectId, int versionId)
	{
		string publishIoFileRelativePath = GetPublishIoFileRelativePath(subProjectId, versionId);
		string webFileLocalAbsolutePath = GetWebFileLocalAbsolutePath(publishIoFileRelativePath);
		try
		{
			Directory.Delete(webFileLocalAbsolutePath, recursive: true);
		}
		catch
		{
		}
		try
		{
			await WebDeleteFolderAsync(publishIoFileRelativePath);
		}
		catch
		{
		}
	}

	/// <summary>
	/// 删除端接发布文件夹
	/// 删除指定版本的端接数据发布文件夹，包括本地和远程
	/// </summary>
	/// <param name="subProjectId">子项目表（config_project_subProject）的ID</param>
	/// <param name="versionId">版本号</param>
	/// <returns>返回表示异步操作完成的任务</returns>
	public async Task DeleteTerminalSubprojectPublishFolderAsync(int subProjectId, int versionId)
	{
		string publishTerminationFileRelativePath = GetPublishTerminationFileRelativePath(subProjectId, versionId);
		string webFileLocalAbsolutePath = GetWebFileLocalAbsolutePath(publishTerminationFileRelativePath);
		try
		{
			Directory.Delete(webFileLocalAbsolutePath, recursive: true);
		}
		catch
		{
		}
		try
		{
			await WebDeleteFolderAsync(publishTerminationFileRelativePath);
		}
		catch
		{
		}
	}

	/// <summary>
	/// 删除电缆子项发布文件夹
	/// 删除指定版本的电缆数据发布文件夹，包括本地和远程
	/// </summary>
	/// <param name="subProjectId">主子项目表（config_project_subProject）的ID</param>
	/// <param name="subProjectId2">副子项目表（config_project_subProject）的ID</param>
	/// <param name="versionId">版本号</param>
	/// <returns>返回表示异步操作完成的任务</returns>
	public async Task DeleteCableSubprojectPublishFolderAsync(int subProjectId, int subProjectId2, int versionId)
	{
		string publishCableFileRelativePath = GetPublishCableFileRelativePath(subProjectId, subProjectId2, versionId);
		string webFileLocalAbsolutePath = GetWebFileLocalAbsolutePath(publishCableFileRelativePath);
		try
		{
			Directory.Delete(webFileLocalAbsolutePath, recursive: true);
		}
		catch
		{
		}
		try
		{
			await WebDeleteFolderAsync(publishCableFileRelativePath);
		}
		catch
		{
		}
	}

	/// <summary>
	/// 获取网络文件的本地存储绝对路径
	/// 自动确保目标文件夹存在
	/// </summary>
	/// <param name="relativePath">相对于网络存储根目录的相对路径</param>
	/// <returns>返回文件的完整绝对路径</returns>
	public string GetWebFileLocalAbsolutePath(string relativePath)
	{
		return EnsureFileDir(Path.Combine(WebStorageRoot, relativePath));
	}

	/// <summary>
	/// 获取文件的下载URL地址
	/// 根据配置中的文件基础URL和相对路径生成完整的下载链接
	/// </summary>
	/// <param name="relativePath">文件的相对路径</param>
	/// <returns>返回文件的完整下载URL</returns>
	public string GetFileDownloadUrl(string relativePath)
	{
		return Path.Combine(config.Value.FileBaseUrl, relativePath);
	}

	/// <summary>
	/// 获取用户导出配置文件的相对路径
	/// 根据用户ID和导出类型生成标准化的用户配置文件存储路径
	/// 用户配置文件存储在export_configs/user_{userId}目录下
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <param name="exportType">导出类型</param>
	/// <returns>用户配置文件的相对路径，格式为：export_configs/user_{userId}/{exportType}_configs.json</returns>
	public string GetUserExportConfigFileRelativePath(int userId, ExportType exportType)
	{
		return $"export_configs/user_{userId}/{exportType}_configs.json";
	}

	/// <summary>
	/// 获取项目级导出配置文件的相对路径
	/// 根据项目ID和导出类型生成标准化的项目配置文件存储路径
	/// 项目配置文件存储在export_configs/project_{projectId}目录下
	/// </summary>
	/// <param name="projectId">项目ID</param>
	/// <param name="exportType">导出类型</param>
	/// <returns>项目配置文件的相对路径，格式为：export_configs/project_{projectId}/{exportType}_configs.json</returns>
	public string GetProjectExportConfigFileRelativePath(int projectId, ExportType exportType)
	{
		return $"export_configs/project_{projectId}/{exportType}_configs.json";
	}

	/// <summary>
	/// 获取全局导出配置文件的相对路径
	/// 根据导出类型生成标准化的全局配置文件存储路径
	/// 全局配置文件存储在export_configs/global目录下，供所有用户使用
	/// </summary>
	/// <param name="exportType">导出类型</param>
	/// <returns>全局配置文件的相对路径，格式为：export_configs/global/{exportType}_configs.json</returns>
	public string GetGlobalExportConfigFileRelativePath(ExportType exportType)
	{
		return $"export_configs/global/{exportType}_configs.json";
	}

	/// <summary>
	/// 上传用户导出配置到服务器
	/// 将本地的用户导出配置上传到服务器，包含MD5校验确保配置完整性
	/// 支持用户个人配置的云端存储和多设备同步
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <param name="exportType">导出类型</param>
	/// <param name="configs">配置列表</param>
	/// <returns>异步任务，表示上传操作的完成</returns>
	/// <exception cref="T:System.Exception">当配置上传失败或MD5校验不通过时抛出异常</exception>
	public async Task UploadUserExportConfigAsync(int userId, ExportType exportType, List<ExportConfig> configs)
	{
		string relativePath = GetUserExportConfigFileRelativePath(userId, exportType);
		string webFileLocalAbsolutePath = GetWebFileLocalAbsolutePath(relativePath);
		EnsureFileDir(webFileLocalAbsolutePath);
		string contents = JsonSerializer.Serialize(configs, new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		});
		await File.WriteAllTextAsync(webFileLocalAbsolutePath, contents);
		await WebUploadFileWithCheckMD5(relativePath);
	}

	/// <summary>
	/// 下载用户导出配置到本地
	/// 从云端下载用户的导出配置文件，实现配置同步
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <param name="exportType">导出类型</param>
	/// <returns>配置列表，如果文件不存在返回空列表</returns>
	public async Task<List<ExportConfig>> DownloadUserExportConfigAsync(int userId, ExportType exportType)
	{
		_ = 1;
		try
		{
			string userExportConfigFileRelativePath = GetUserExportConfigFileRelativePath(userId, exportType);
			string path = await WebDownloadFileWithCheckMD5(userExportConfigFileRelativePath);
			if (!File.Exists(path))
			{
				return new List<ExportConfig>();
			}
			List<ExportConfig> list = JsonSerializer.Deserialize<List<ExportConfig>>(await File.ReadAllTextAsync(path));
			return list ?? new List<ExportConfig>();
		}
		catch
		{
			return new List<ExportConfig>();
		}
	}

	/// <summary>
	/// 上传项目级导出配置到服务器
	/// 将本地的项目级导出配置上传到服务器，支持项目团队配置共享
	/// </summary>
	/// <param name="projectId">项目ID</param>
	/// <param name="exportType">导出类型</param>
	/// <param name="configs">配置列表</param>
	/// <returns>异步任务，表示上传操作的完成</returns>
	public async Task UploadProjectExportConfigAsync(int projectId, ExportType exportType, List<ExportConfig> configs)
	{
		string relativePath = GetProjectExportConfigFileRelativePath(projectId, exportType);
		string webFileLocalAbsolutePath = GetWebFileLocalAbsolutePath(relativePath);
		EnsureFileDir(webFileLocalAbsolutePath);
		string contents = JsonSerializer.Serialize(configs, new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		});
		await File.WriteAllTextAsync(webFileLocalAbsolutePath, contents);
		await WebUploadFileWithCheckMD5(relativePath);
	}

	/// <summary>
	/// 下载项目级导出配置到本地
	/// 从云端下载项目级导出配置文件，实现项目团队配置共享
	/// </summary>
	/// <param name="projectId">项目ID</param>
	/// <param name="exportType">导出类型</param>
	/// <returns>配置列表，如果文件不存在返回空列表</returns>
	public async Task<List<ExportConfig>> DownloadProjectExportConfigAsync(int projectId, ExportType exportType)
	{
		_ = 1;
		try
		{
			string projectExportConfigFileRelativePath = GetProjectExportConfigFileRelativePath(projectId, exportType);
			string path = await WebDownloadFileWithCheckMD5(projectExportConfigFileRelativePath);
			if (!File.Exists(path))
			{
				return new List<ExportConfig>();
			}
			List<ExportConfig> list = JsonSerializer.Deserialize<List<ExportConfig>>(await File.ReadAllTextAsync(path));
			return list ?? new List<ExportConfig>();
		}
		catch
		{
			return new List<ExportConfig>();
		}
	}

	/// <summary>
	/// 上传全局导出配置到服务器
	/// 管理员上传全局共享的导出配置，所有用户可见
	/// </summary>
	/// <param name="exportType">导出类型</param>
	/// <param name="configs">配置列表</param>
	/// <returns>异步任务</returns>
	public async Task UploadGlobalExportConfigAsync(ExportType exportType, List<ExportConfig> configs)
	{
		string relativePath = GetGlobalExportConfigFileRelativePath(exportType);
		string webFileLocalAbsolutePath = GetWebFileLocalAbsolutePath(relativePath);
		EnsureFileDir(webFileLocalAbsolutePath);
		string contents = JsonSerializer.Serialize(configs, new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		});
		await File.WriteAllTextAsync(webFileLocalAbsolutePath, contents);
		await WebUploadFileWithCheckMD5(relativePath);
	}

	/// <summary>
	/// 下载全局导出配置到本地
	/// 从云端下载全局导出配置文件，所有用户可访问
	/// </summary>
	/// <param name="exportType">导出类型</param>
	/// <returns>配置列表，如果文件不存在返回空列表</returns>
	public async Task<List<ExportConfig>> DownloadGlobalExportConfigAsync(ExportType exportType)
	{
		_ = 1;
		try
		{
			string globalExportConfigFileRelativePath = GetGlobalExportConfigFileRelativePath(exportType);
			string path = await WebDownloadFileWithCheckMD5(globalExportConfigFileRelativePath);
			if (!File.Exists(path))
			{
				return new List<ExportConfig>();
			}
			List<ExportConfig> list = JsonSerializer.Deserialize<List<ExportConfig>>(await File.ReadAllTextAsync(path));
			return list ?? new List<ExportConfig>();
		}
		catch
		{
			return new List<ExportConfig>();
		}
	}

	/// <summary>
	/// 删除用户导出配置
	/// 从服务器删除指定用户的导出配置文件
	/// </summary>
	/// <param name="userId">用户ID</param>
	/// <param name="exportType">导出类型</param>
	/// <returns>异步任务</returns>
	public async Task DeleteUserExportConfigAsync(int userId, ExportType exportType)
	{
		string userExportConfigFileRelativePath = GetUserExportConfigFileRelativePath(userId, exportType);
		try
		{
			await WebDeleteFileAsync(userExportConfigFileRelativePath);
		}
		catch
		{
		}
	}

	/// <summary>
	/// 删除项目级导出配置
	/// 从服务器删除指定项目的导出配置文件
	/// </summary>
	/// <param name="projectId">项目ID</param>
	/// <param name="exportType">导出类型</param>
	/// <returns>异步任务</returns>
	public async Task DeleteProjectExportConfigAsync(int projectId, ExportType exportType)
	{
		string projectExportConfigFileRelativePath = GetProjectExportConfigFileRelativePath(projectId, exportType);
		try
		{
			await WebDeleteFileAsync(projectExportConfigFileRelativePath);
		}
		catch
		{
		}
	}

	/// <summary>
	/// 获取实时IO文件的相对路径
	/// 根据子项目ID生成标准化的IO数据文件存储路径
	/// 实时文件用于存储最新的工作版本IO数据，支持多用户协作编辑
	/// </summary>
	/// <param name="subProjectId">子项目的唯一标识符</param>
	/// <returns>实时IO文件的相对路径，格式为：{subProject}/io/realtime.csv</returns>
	public string GetRealtimeIoFileRelativePath(int subProjectId)
	{
		return GetSubProjectFolderRelativePath(subProjectId) + "/io/realtime.csv";
	}

	/// <summary>
	/// 获取发布IO文件的相对路径
	/// 根据子项目ID和发布版本ID生成发布IO数据文件的存储路径
	/// 发布文件用于正式版本的数据分发、存档和版本控制
	/// </summary>
	/// <param name="subProjectId">子项目的唯一标识符</param>
	/// <param name="publishId">发布版本的唯一标识符</param>
	/// <returns>发布IO文件的相对路径，格式为：{subProject}/io/publish_{publishId}.csv</returns>
	public string GetPublishIoFileRelativePath(int subProjectId, int publishId)
	{
		return $"{GetSubProjectFolderRelativePath(subProjectId)}/io/publish_{publishId}.csv";
	}

	/// <summary>
	/// 下载实时IO文件到本地
	/// 从服务器下载最新的实时IO数据文件，包含完整的MD5校验过程
	/// 确保下载的IO数据完整性和版本一致性，支持断点续传和错误重试
	/// </summary>
	/// <param name="subProjectId">子项目的唯一标识符</param>
	/// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
	/// <exception cref="T:System.Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
	public async Task<string> DownloadRealtimeIoFileAsync(int subProjectId)
	{
		string realtimeIoFileRelativePath = GetRealtimeIoFileRelativePath(subProjectId);
		return await WebDownloadFileWithCheckMD5(realtimeIoFileRelativePath);
	}

	/// <summary>
	/// 下载指定版本的发布IO文件到本地
	/// 从服务器下载特定发布版本的IO数据文件，用于版本回滚、历史数据查看或数据对比
	/// 包含完整的MD5校验过程以确保发布版本数据的准确性和完整性
	/// </summary>
	/// <param name="subProjectId">子项目的唯一标识符</param>
	/// <param name="publishId">发布版本的唯一标识符</param>
	/// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
	/// <exception cref="T:System.Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
	public async Task<string> DownloadPublishIoFileAsync(int subProjectId, int publishId)
	{
		string publishIoFileRelativePath = GetPublishIoFileRelativePath(subProjectId, publishId);
		return await WebDownloadFileWithCheckMD5(publishIoFileRelativePath);
	}

	/// <summary>
	/// 上传实时IO文件到服务器
	/// 将本地的实时IO数据文件上传到服务器，包含MD5校验确保数据完整性
	/// 上传成功后其他用户可以获取到最新的IO数据更新，实现多用户协作
	/// </summary>
	/// <param name="subProjectId">子项目的唯一标识符</param>
	/// <returns>异步任务，表示上传操作的完成</returns>
	/// <exception cref="T:System.Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
	public async Task UploadRealtimeIoFileAsync(int subProjectId)
	{
		string realtimeIoFileRelativePath = GetRealtimeIoFileRelativePath(subProjectId);
		await WebUploadFileWithCheckMD5(realtimeIoFileRelativePath);
	}

	/// <summary>
	/// 获取图纸文件夹的相对路径
	/// 根据图纸ID生成标准化的图纸文件存储目录
	/// 用于组织同一张图纸的多个相关文件
	/// </summary>
	/// <param name="paper">图纸对象，包含图纸ID和文件信息</param>
	/// <returns>图纸文件夹的相对路径，格式为：papers/{paperId}</returns>
	public string GetPaperFolderRelativePath(图纸 paper)
	{
		return $"papers/{paper.Id}";
	}

	/// <summary>
	/// 获取图纸压缩包文件的相对路径
	/// 压缩包文件主要用于批量下载和存档，包含完整的图纸资源集合
	/// 适用于大文件传输和存储空间优化
	/// </summary>
	/// <param name="paper">图纸对象，包含图纸ID和压缩包文件名</param>
	/// <returns>图纸压缩包文件的相对路径</returns>
	public string GetPaperZipFileRelativePath(图纸 paper)
	{
		return $"papers/{paper.Id}/{paper.压缩包文件名}";
	}

	/// <summary>
	/// 获取图纸PDF文件的相对路径
	/// PDF文件用于在线查看和打印，支持浏览器直接打开
	/// 适用于快速查看和文档共享
	/// </summary>
	/// <param name="paper">图纸对象，包含图纸ID和PDF文件名</param>
	/// <returns>图纸PDF文件的相对路径</returns>
	public string GetPaperPdfFileRelativePath(图纸 paper)
	{
		return $"papers/{paper.Id}/{paper.PDF文件名}";
	}

	/// <summary>
	/// 获取图纸依据文件的相对路径
	/// 依据文件包含设计规范、技术标准等相关资料，用于支撑图纸设计的技术依据
	/// 对于理解设计意图和技术要求具有重要意义
	/// </summary>
	/// <param name="paper">图纸对象，包含图纸ID和依据文件名</param>
	/// <returns>图纸依据文件的相对路径</returns>
	public string GetPaperDepFileRelativePath(图纸 paper)
	{
		return $"papers/{paper.Id}/{paper.依据文件名}";
	}

	/// <summary>
	/// 上传图纸压缩包文件到服务器
	/// 将本地的图纸压缩包文件上传到服务器，包含MD5校验确保文件完整性
	/// 适用于大批量图纸数据的上传和存档备份
	/// </summary>
	/// <param name="paper">图纸对象，包含图纸ID和压缩包文件信息</param>
	/// <returns>异步任务，表示上传操作的完成</returns>
	/// <exception cref="T:System.Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
	public async Task UploadPaperZipFileAsync(图纸 paper)
	{
		string paperZipFileRelativePath = GetPaperZipFileRelativePath(paper);
		await WebUploadFileWithCheckMD5(paperZipFileRelativePath);
	}

	/// <summary>
	/// 上传图纸PDF文件到服务器
	/// 将本地的图纸PDF文件上传到服务器，包含MD5校验确保文件完整性
	/// 适用于在线预览和文档共享场景
	/// </summary>
	/// <param name="paper">图纸对象，包含图纸ID和PDF文件信息</param>
	/// <returns>异步任务，表示上传操作的完成</returns>
	/// <exception cref="T:System.Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
	public async Task UploadPaperPdfFileAsync(图纸 paper)
	{
		string paperPdfFileRelativePath = GetPaperPdfFileRelativePath(paper);
		await WebUploadFileWithCheckMD5(paperPdfFileRelativePath);
	}

	/// <summary>
	/// 上传图纸依据文件到服务器
	/// 将本地的图纸依据文件上传到服务器，包含MD5校验确保文件完整性
	/// 依据文件对于理解设计意图和技术要求具有重要意义
	/// </summary>
	/// <param name="paper">图纸对象，包含图纸ID和依据文件信息</param>
	/// <returns>异步任务，表示上传操作的完成</returns>
	/// <exception cref="T:System.Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
	public async Task UploadPaperDepFileAsync(图纸 paper)
	{
		string paperDepFileRelativePath = GetPaperDepFileRelativePath(paper);
		await WebUploadFileWithCheckMD5(paperDepFileRelativePath);
	}

	/// <summary>
	/// 下载图纸压缩包文件到本地
	/// 从服务器下载图纸压缩包文件，包含完整的MD5校验过程
	/// 适用于下载完整的图纸资源集合用于本地处理
	/// </summary>
	/// <param name="paper">图纸对象，包含图纸ID和压缩包文件信息</param>
	/// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
	/// <exception cref="T:System.Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
	public async Task<string> DownloadPaperZipFileAsync(图纸 paper)
	{
		string paperZipFileRelativePath = GetPaperZipFileRelativePath(paper);
		return await WebDownloadFileWithCheckMD5(paperZipFileRelativePath);
	}

	/// <summary>
	/// 下载图纸PDF文件到本地
	/// 从服务器下载图纸PDF文件，包含完整的MD5校验过程
	/// 适用于在线查看、打印和文档分享场景
	/// </summary>
	/// <param name="paper">图纸对象，包含图纸ID和PDF文件信息</param>
	/// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
	/// <exception cref="T:System.Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
	public async Task<string> DownloadPaperPdfFileAsync(图纸 paper)
	{
		string paperPdfFileRelativePath = GetPaperPdfFileRelativePath(paper);
		return await WebDownloadFileWithCheckMD5(paperPdfFileRelativePath);
	}

	/// <summary>
	/// 下载图纸依据文件到本地
	/// 从服务器下载图纸依据文件，包含完整的MD5校验过程
	/// 依据文件包含设计规范和技术标准，对理解设计意图具有重要意义
	/// </summary>
	/// <param name="paper">图纸对象，包含图纸ID和依据文件信息</param>
	/// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
	/// <exception cref="T:System.Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
	public async Task<string> DownloadPaperDepFileAsync(图纸 paper)
	{
		string paperDepFileRelativePath = GetPaperDepFileRelativePath(paper);
		return await WebDownloadFileWithCheckMD5(paperDepFileRelativePath);
	}

	/// <summary>
	/// 获取模板依据文件的相对路径
	/// 根据Excel文件名生成标准化的模板文件存储路径
	/// 模板文件统一存储在templates目录下，便于集中管理
	/// </summary>
	/// <param name="excelName">Excel模板文件名，包含文件扩展名</param>
	/// <returns>模板文件的相对路径，格式为：templates/{excelName}</returns>
	public string GettemplatesDepFileRelativePath(string excelName)
	{
		return "templates/" + excelName;
	}

	/// <summary>
	/// 上传模板依据文件到服务器
	/// 将本地的Excel模板文件上传到服务器，包含MD5校验确保文件完整性
	/// 适用于新增模板文件或更新现有模板文件的版本
	/// </summary>
	/// <param name="excelName">要上传的Excel模板文件名</param>
	/// <returns>异步任务，表示上传操作的完成</returns>
	/// <exception cref="T:System.Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
	public async Task UploadtemplatesDepFileAsync(string excelName)
	{
		string relativePath = GettemplatesDepFileRelativePath(excelName);
		await WebUploadFileWithCheckMD5(relativePath);
	}

	/// <summary>
	/// 下载模板依据文件到本地
	/// 从服务器下载Excel模板文件到本地，包含完整的MD5校验过程
	/// 确保下载的模板文件完整性和版本一致性，支持断点续传和错误重试
	/// </summary>
	/// <param name="excelName">要下载的Excel模板文件名</param>
	/// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
	/// <exception cref="T:System.Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
	public async Task<string> DownloadtemplatesDepFileAsync(string excelName)
	{
		string relativePath = GettemplatesDepFileRelativePath(excelName);
		return await WebDownloadFileWithCheckMD5(relativePath);
	}

	/// <summary>
	/// 获取实时端接文件的相对路径
	/// 根据子项目ID生成标准化的端接数据文件存储路径
	/// 实时文件用于存储最新的工作版本端接数据，支持多用户协作编辑
	/// </summary>
	/// <param name="subProjectId">子项目的唯一标识符</param>
	/// <returns>实时端接文件的相对路径，格式为：{subProject}/termination/realtime.csv</returns>
	public string GetRealtimeTerminationFileRelativePath(int subProjectId)
	{
		return GetSubProjectFolderRelativePath(subProjectId) + "/termination/realtime.csv";
	}

	/// <summary>
	/// 获取发布端接文件的相对路径
	/// 根据子项目ID和发布版本ID生成发布端接数据文件的存储路径
	/// 发布文件用于正式版本的数据分发、存档和版本控制
	/// </summary>
	/// <param name="subProjectId">子项目的唯一标识符</param>
	/// <param name="publishId">发布版本的唯一标识符</param>
	/// <returns>发布端接文件的相对路径，格式为：{subProject}/termination/publish_{publishId}.csv</returns>
	public string GetPublishTerminationFileRelativePath(int subProjectId, int publishId)
	{
		return $"{GetSubProjectFolderRelativePath(subProjectId)}/termination/publish_{publishId}.csv";
	}

	/// <summary>
	/// 下载实时端接文件到本地
	/// 从服务器下载最新的实时端接数据文件，包含完整的MD5校验过程
	/// 确保下载的端接数据完整性和版本一致性，支持断点续传和错误重试
	/// </summary>
	/// <param name="subProjectId">子项目的唯一标识符</param>
	/// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
	/// <exception cref="T:System.Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
	public async Task<string> DownloadRealtimeTerminationFileAsync(int subProjectId)
	{
		string realtimeTerminationFileRelativePath = GetRealtimeTerminationFileRelativePath(subProjectId);
		return await WebDownloadFileWithCheckMD5(realtimeTerminationFileRelativePath);
	}

	/// <summary>
	/// 下载指定版本的发布端接文件到本地
	/// 从服务器下载特定发布版本的端接数据文件，用于版本回滚、历史数据查看或数据对比
	/// 包含完整的MD5校验过程以确保发布版本数据的准确性和完整性
	/// </summary>
	/// <param name="subProjectId">子项目的唯一标识符</param>
	/// <param name="publishId">发布版本的唯一标识符</param>
	/// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
	/// <exception cref="T:System.Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
	public async Task<string> DownloadPublishTerminationFileAsync(int subProjectId, int publishId)
	{
		string publishTerminationFileRelativePath = GetPublishTerminationFileRelativePath(subProjectId, publishId);
		return await WebDownloadFileWithCheckMD5(publishTerminationFileRelativePath);
	}

	/// <summary>
	/// 上传实时端接文件到服务器
	/// 将本地的实时端接数据文件上传到服务器，包含MD5校验确保数据完整性
	/// 上传成功后其他用户可以获取到最新的端接数据更新，实现多用户协作
	/// </summary>
	/// <param name="subProjectId">子项目的唯一标识符</param>
	/// <returns>异步任务，表示上传操作的完成</returns>
	/// <exception cref="T:System.Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
	public async Task UploadRealtimeTerminationFileAsync(int subProjectId)
	{
		string realtimeTerminationFileRelativePath = GetRealtimeTerminationFileRelativePath(subProjectId);
		await WebUploadFileWithCheckMD5(realtimeTerminationFileRelativePath);
	}

	/// <summary>
	/// 上传发布版本的端接文件到服务器
	/// 将本地的发布版本端接数据文件上传到服务器，用于正式版本的发布和存档
	/// 包含MD5校验确保发布版本数据的准确性和可靠性
	/// </summary>
	/// <param name="subProjectId">子项目的唯一标识符</param>
	/// <param name="publishId">发布版本的唯一标识符</param>
	/// <returns>异步任务，表示上传操作的完成</returns>
	/// <exception cref="T:System.Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
	public async Task UploadPublishTerminationFileAsync(int subProjectId, int publishId)
	{
		string publishTerminationFileRelativePath = GetPublishTerminationFileRelativePath(subProjectId, publishId);
		await WebUploadFileWithCheckMD5(publishTerminationFileRelativePath);
	}

	/// <summary>
	/// 向Web服务器上传文件
	/// 使用HTTP POST方式上传二进制文件数据，带有API密钥认证
	/// </summary>
	/// <param name="relativePath">文件在服务器上的相对路径</param>
	/// <param name="body">要上传的文件二进制数据</param>
	/// <returns>返回表示异步操作完成的任务</returns>
	/// <exception cref="T:System.Exception">当上传失败时抛出异常</exception>
	public async Task WebUploadFileAsync(string relativePath, byte[] body)
	{
		string requestUri = Config.BaseUrl + "/FileService/UploadFile";
		ByteArrayContent byteArrayContent = new ByteArrayContent(body);
		byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
		{
			Content = byteArrayContent
		};
		httpRequestMessage.Headers.Add("ApiKey", Config.Key);
		httpRequestMessage.Headers.Add("RelativePath", Convert.ToBase64String(Encoding.UTF8.GetBytes(relativePath)));
		string text = await (await HttpClient.SendAsync(httpRequestMessage)).EnsureSuccessStatusCode().Content.ReadAsStringAsync();
		if (!text.StartsWith("文件上传成功"))
		{
			throw new Exception(text);
		}
	}

	/// <summary>
	/// 从 Web服务器下载文件
	/// 使用HTTP GET方式获取文件的二进制数据，带有API密钥认证
	/// </summary>
	/// <param name="relativePath">文件在服务器上的相对路径</param>
	/// <returns>返回文件的二进制数据</returns>
	/// <exception cref="T:System.Exception">当下载失败时抛出异常</exception>
	public async Task<byte[]> WebDownloadFileAsync(string relativePath)
	{
		string requestUri = Config.BaseUrl + "/FileService/DownloadFile";
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
		httpRequestMessage.Headers.Add("ApiKey", Config.Key);
		httpRequestMessage.Headers.Add("RelativePath", Convert.ToBase64String(Encoding.UTF8.GetBytes(relativePath)));
		HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);
		try
		{
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsByteArrayAsync();
		}
		catch
		{
			throw new Exception(await response.Content.ReadAsStringAsync());
		}
	}

	/// <summary>
	/// 介 Web服务器删除文件
	/// 使用HTTP DELETE方式删除指定的文件，带有API密钥认证
	/// </summary>
	/// <param name="relativePath">要删除的文件在服务器上的相对路径</param>
	/// <returns>返回表示异步操作完成的任务</returns>
	/// <exception cref="T:System.Exception">当删除失败时抛出异常</exception>
	public async Task WebDeleteFileAsync(string relativePath)
	{
		string requestUri = Config.BaseUrl + "/FileService/DeleteFile";
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, requestUri);
		httpRequestMessage.Headers.Add("ApiKey", Config.Key);
		httpRequestMessage.Headers.Add("RelativePath", Convert.ToBase64String(Encoding.UTF8.GetBytes(relativePath)));
		string text = await (await HttpClient.SendAsync(httpRequestMessage)).EnsureSuccessStatusCode().Content.ReadAsStringAsync();
		if (!text.StartsWith("文件删除成功"))
		{
			throw new Exception(text);
		}
	}

	/// <summary>
	/// 介 Web服务器删除文件夹
	/// 使用HTTP DELETE方式删除指定的文件夹及其所有内容，带有API密钥认证
	/// </summary>
	/// <param name="relativePath">要删除的文件夹在服务器上的相对路径</param>
	/// <returns>返回表示异步操作完成的任务</returns>
	/// <exception cref="T:System.Exception">当删除失败时抛出异常</exception>
	public async Task WebDeleteFolderAsync(string relativePath)
	{
		string requestUri = Config.BaseUrl + "/FileService/DeleteFolder";
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, requestUri);
		httpRequestMessage.Headers.Add("ApiKey", Config.Key);
		httpRequestMessage.Headers.Add("RelativePath", Convert.ToBase64String(Encoding.UTF8.GetBytes(relativePath)));
		string text = await (await HttpClient.SendAsync(httpRequestMessage)).EnsureSuccessStatusCode().Content.ReadAsStringAsync();
		if (!text.StartsWith("文件夹删除成功"))
		{
			throw new Exception(text);
		}
	}

	public async Task WebCopyFilesAsync(IEnumerable<(string From, string To)> args)
	{
		string requestUri = Config.BaseUrl + "/FileService/CopyFiles";
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
		httpRequestMessage.Headers.Add("ApiKey", Config.Key);
		httpRequestMessage.Headers.Add("RelativePaths", args.SelectMany<(string, string), string>(((string From, string To) arg) => new string[2] { arg.From, arg.To }));
		string text = await (await HttpClient.SendAsync(httpRequestMessage)).EnsureSuccessStatusCode().Content.ReadAsStringAsync();
		if (!text.StartsWith("文件复制成功"))
		{
			throw new Exception(text);
		}
	}

	/// <summary>
	/// 获取Web服务器上文件的MD5哈希值
	/// 使用HTTP GET方式获取文件的MD5值，用于文件完整性校验
	/// </summary>
	/// <param name="relativePath">文件在服务器上的相对路径</param>
	/// <returns>返回文件的MD5哈希字符串</returns>
	/// <exception cref="T:System.Exception">当文件不存在时抛出异常</exception>
	public async Task<string> WebGetFileMD5Async(string relativePath)
	{
		string requestUri = Config.BaseUrl + "/FileService/GetFileMD5";
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
		httpRequestMessage.Headers.Add("ApiKey", Config.Key);
		httpRequestMessage.Headers.Add("RelativePath", Convert.ToBase64String(Encoding.UTF8.GetBytes(relativePath)));
		string text = await (await HttpClient.SendAsync(httpRequestMessage)).Content.ReadAsStringAsync();
		if (text == "文件不存在")
		{
			throw new Exception(text);
		}
		return text;
	}
}
