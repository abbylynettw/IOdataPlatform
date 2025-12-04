﻿using System.IO;

namespace IODataPlatform.Services;

/// <summary>
/// StorageService的Web文件存储功能部分
/// 管理与Web服务器同步的文件存储和访问操作
/// 提供项目文件、发布数据等的本地缓存和远程同步功能
/// </summary>
partial class StorageService {

    /// <summary>网络文件本地存储根目录，位于主存储目录下的Web子文件夹</summary>
    public string WebStorageRoot { get; } = EnsureDir(Path.Combine(StorageRoot, "Web"));

    /// <summary>
    /// 获取子项文件夹的相对路径
    /// 根据子项目ID生成统一的文件夹路径格式
    /// </summary>
    /// <param name="subProjectId">子项目表（config_project_subProject）的ID</param>
    /// <returns>返回子项文件夹的相对路径</returns>
    public static string GetSubProjectFolderRelativePath(int subProjectId) {
        return $"projects/subproject_{subProjectId}";
    }

    /// <summary>
    /// 删除子项文件夹（本地和远程）
    /// 同时删除本地缓存和远程Web服务器上的子项文件夹
    /// </summary>
    /// <param name="subProjectId">子项目表（config_project_subProject）的ID</param>
    /// <returns>返回表示异步操作完成的任务</returns>
    public async Task DeleteSubprojectFolderAsync(int subProjectId) {
        var relativePath = GetSubProjectFolderRelativePath(subProjectId);
        var absolutePath = GetWebFileLocalAbsolutePath(relativePath);
        try { Directory.Delete(absolutePath, true); } catch { }
        try { await WebDeleteFolderAsync(relativePath); } catch { }
    }

    /// <summary>
    /// 删除子项发布文件夹（IO数据）
    /// 删除指定版本的IO数据发布文件夹，包括本地和远程
    /// </summary>
    /// <param name="subProjectId">子项目表（config_project_subProject）的ID</param>
    /// <param name="versionId">版本号</param>
    /// <returns>返回表示异步操作完成的任务</returns>
    public async Task DeleteSubprojectPublishFolderAsync(int subProjectId,int versionId)
    {
        var relativePath = GetPublishIoFileRelativePath(subProjectId,versionId);
        var absolutePath = GetWebFileLocalAbsolutePath(relativePath);
        try { Directory.Delete(absolutePath, true); } catch { }
        try { await WebDeleteFolderAsync(relativePath); } catch { }
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
        var relativePath = GetPublishTerminationFileRelativePath(subProjectId, versionId);
        var absolutePath = GetWebFileLocalAbsolutePath(relativePath);
        try { Directory.Delete(absolutePath, true); } catch { }
        try { await WebDeleteFolderAsync(relativePath); } catch { }
    }
    /// <summary>
    /// 删除电缆子项发布文件夹
    /// 删除指定版本的电缆数据发布文件夹，包括本地和远程
    /// </summary>
    /// <param name="subProjectId">主子项目表（config_project_subProject）的ID</param>
    /// <param name="subProjectId2">副子项目表（config_project_subProject）的ID</param>
    /// <param name="versionId">版本号</param>
    /// <returns>返回表示异步操作完成的任务</returns>
    public async Task DeleteCableSubprojectPublishFolderAsync(int subProjectId,int subProjectId2, int versionId)
    {
        var relativePath = GetPublishCableFileRelativePath(subProjectId, subProjectId2, versionId);
        var absolutePath = GetWebFileLocalAbsolutePath(relativePath);
        try { Directory.Delete(absolutePath, true); } catch { }
        try { await WebDeleteFolderAsync(relativePath); } catch { }
    }
    /// <summary>
    /// 获取网络文件的本地存储绝对路径
    /// 自动确保目标文件夹存在
    /// </summary>
    /// <param name="relativePath">相对于网络存储根目录的相对路径</param>
    /// <returns>返回文件的完整绝对路径</returns>
    public string GetWebFileLocalAbsolutePath(string relativePath) {
        return EnsureFileDir(Path.Combine(WebStorageRoot, relativePath));
    }

    /// <summary>
    /// 获取文件的下载URL地址
    /// 根据配置中的文件基础URL和相对路径生成完整的下载链接
    /// </summary>
    /// <param name="relativePath">文件的相对路径</param>
    /// <returns>返回文件的完整下载URL</returns>
    public string GetFileDownloadUrl(string relativePath) {
        return Path.Combine(config.Value.FileBaseUrl, relativePath);
    }

}