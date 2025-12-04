﻿using System.IO;

namespace IODataPlatform.Services;

/// <summary>
/// 存储服务的端接文件管理部分
/// 提供端接数据文件的完整存储管理功能，包括实时数据和发布版本的文件操作
/// 端接数据涉及电缆与设备的物理连接关系，对数据准确性和一致性要求极高
/// 所有操作都包含MD5校验以确保数据完整性和版本一致性
/// </summary>
partial class StorageService {

    /// <summary>
    /// 获取实时端接文件的相对路径
    /// 根据子项目ID生成标准化的端接数据文件存储路径
    /// 实时文件用于存储最新的工作版本端接数据，支持多用户协作编辑
    /// </summary>
    /// <param name="subProjectId">子项目的唯一标识符</param>
    /// <returns>实时端接文件的相对路径，格式为：{subProject}/termination/realtime.csv</returns>
    public string GetRealtimeTerminationFileRelativePath(int subProjectId) {
        return $"{GetSubProjectFolderRelativePath(subProjectId)}/termination/realtime.csv";
    }

    /// <summary>
    /// 获取发布端接文件的相对路径
    /// 根据子项目ID和发布版本ID生成发布端接数据文件的存储路径
    /// 发布文件用于正式版本的数据分发、存档和版本控制
    /// </summary>
    /// <param name="subProjectId">子项目的唯一标识符</param>
    /// <param name="publishId">发布版本的唯一标识符</param>
    /// <returns>发布端接文件的相对路径，格式为：{subProject}/termination/publish_{publishId}.csv</returns>
    public string GetPublishTerminationFileRelativePath(int subProjectId, int publishId) {
        return $"{GetSubProjectFolderRelativePath(subProjectId)}/termination/publish_{publishId}.csv";
    }

    /// <summary>
    /// 下载实时端接文件到本地
    /// 从服务器下载最新的实时端接数据文件，包含完整的MD5校验过程
    /// 确保下载的端接数据完整性和版本一致性，支持断点续传和错误重试
    /// </summary>
    /// <param name="subProjectId">子项目的唯一标识符</param>
    /// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
    /// <exception cref="Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
    public async Task<string> DownloadRealtimeTerminationFileAsync(int subProjectId) {
        var relativePath = GetRealtimeTerminationFileRelativePath(subProjectId);
        return await WebDownloadFileWithCheckMD5(relativePath);
    }

    /// <summary>
    /// 下载指定版本的发布端接文件到本地
    /// 从服务器下载特定发布版本的端接数据文件，用于版本回滚、历史数据查看或数据对比
    /// 包含完整的MD5校验过程以确保发布版本数据的准确性和完整性
    /// </summary>
    /// <param name="subProjectId">子项目的唯一标识符</param>
    /// <param name="publishId">发布版本的唯一标识符</param>
    /// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
    /// <exception cref="Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
    public async Task<string> DownloadPublishTerminationFileAsync(int subProjectId, int publishId) {
        var relativePath = GetPublishTerminationFileRelativePath(subProjectId, publishId);
        return await WebDownloadFileWithCheckMD5(relativePath);
    }

    /// <summary>
    /// 上传实时端接文件到服务器
    /// 将本地的实时端接数据文件上传到服务器，包含MD5校验确保数据完整性
    /// 上传成功后其他用户可以获取到最新的端接数据更新，实现多用户协作
    /// </summary>
    /// <param name="subProjectId">子项目的唯一标识符</param>
    /// <returns>异步任务，表示上传操作的完成</returns>
    /// <exception cref="Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
    public async Task UploadRealtimeTerminationFileAsync(int subProjectId) {
        var relativePath = GetRealtimeTerminationFileRelativePath(subProjectId);
        await WebUploadFileWithCheckMD5(relativePath);
    }

    /// <summary>
    /// 上传发布版本的端接文件到服务器
    /// 将本地的发布版本端接数据文件上传到服务器，用于正式版本的发布和存档
    /// 包含MD5校验确保发布版本数据的准确性和可靠性
    /// </summary>
    /// <param name="subProjectId">子项目的唯一标识符</param>
    /// <param name="publishId">发布版本的唯一标识符</param>
    /// <returns>异步任务，表示上传操作的完成</returns>
    /// <exception cref="Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
    public async Task UploadPublishTerminationFileAsync(int subProjectId, int publishId) {
        var relativePath = GetPublishTerminationFileRelativePath(subProjectId, publishId);
        await WebUploadFileWithCheckMD5(relativePath);
    }

}