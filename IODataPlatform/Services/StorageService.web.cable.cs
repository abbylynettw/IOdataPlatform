﻿namespace IODataPlatform.Services;

/// <summary>
/// 存储服务的电缆文件管理部分
/// 提供电缆数据文件的完整存储管理功能，包括实时数据和发布版本的文件操作
/// 支持双子项目的电缆数据管理，实现跨专业的电缆连接关系维护
/// 所有操作都包含MD5校验以确保数据完整性和一致性
/// </summary>
partial class StorageService {

    /// <summary>
    /// 获取实时电缆文件的相对路径
    /// 根据两个子项目ID生成标准化的电缆文件存储路径
    /// 使用较小和较大ID的组合确保路径的一致性，避免因参数顺序不同导致的路径差异
    /// </summary>
    /// <param name="subProjectId1">第一个子项目的ID</param>
    /// <param name="subProjectId2">第二个子项目的ID</param>
    /// <returns>实时电缆文件的相对路径，格式为：projects/cable/{min_id}_{max_id}/realtime.csv</returns>
    public string GetRealtimeCableFileRelativePath(int subProjectId1, int subProjectId2) {
        var ids = new[] { subProjectId1, subProjectId2 };
        return $"projects/cable/{ids.Min()}_{ids.Max()}/realtime.csv";
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
    public string GetPublishCableFileRelativePath(int subProjectId1, int subProjectId2, int publishId) {
        var ids = new[] { subProjectId1, subProjectId2 };
        return $"projects/cable/{ids.Min()}_{ids.Max()}/publish_{publishId}.csv";
    }

    /// <summary>
    /// 下载实时电缆文件到本地
    /// 从服务器下载最新的实时电缆数据文件，包含完整的MD5校验过程
    /// 确保下载的文件完整性和数据一致性，支持断点续传和错误重试
    /// </summary>
    /// <param name="subProjectId1">第一个子项目的ID</param>
    /// <param name="subProjectId2">第二个子项目的ID</param>
    /// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
    /// <exception cref="Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
    public async Task<string> DownloadRealtimeCableFileAsync(int subProjectId1, int subProjectId2) {
        var relativePath = GetRealtimeCableFileRelativePath(subProjectId1, subProjectId2);
        return await WebDownloadFileWithCheckMD5(relativePath);
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
    /// <exception cref="Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
    public async Task<string> DownloadPublishCableFileAsync(int subProjectId1, int subProjectId2, int publishId) {
        var relativePath = GetPublishCableFileRelativePath(subProjectId1, subProjectId2, publishId);
        return await WebDownloadFileWithCheckMD5(relativePath);
    }

    /// <summary>
    /// 上传实时电缆文件到服务器
    /// 将本地的实时电缆数据文件上传到服务器，包含MD5校验确保数据完整性
    /// 上传成功后其他用户可以获取到最新的电缆数据更新
    /// </summary>
    /// <param name="subProjectId1">第一个子项目的ID</param>
    /// <param name="subProjectId2">第二个子项目的ID</param>
    /// <returns>异步任务，表示上传操作的完成</returns>
    /// <exception cref="Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
    public async Task UploadRealtimeCableFileAsync(int subProjectId1, int subProjectId2) {
        var relativePath = GetRealtimeCableFileRelativePath(subProjectId1, subProjectId2);
        await WebUploadFileWithCheckMD5(relativePath);
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
    /// <exception cref="Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
    public async Task UploadPublishCableFileAsync(int subProjectId1, int subProjectId2, int publishId) {
        var relativePath = GetPublishCableFileRelativePath(subProjectId1, subProjectId2, publishId);
        await WebUploadFileWithCheckMD5(relativePath);
    }

}