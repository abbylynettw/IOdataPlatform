﻿﻿using IODataPlatform.Models.DBModels;

namespace IODataPlatform.Services;

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
partial class StorageService {

    /// <summary>
    /// 获取图纸文件夹的相对路径
    /// 根据图纸ID生成标准化的图纸文件存储目录
    /// 用于组织同一张图纸的多个相关文件
    /// </summary>
    /// <param name="paper">图纸对象，包含图纸ID和文件信息</param>
    /// <returns>图纸文件夹的相对路径，格式为：papers/{paperId}</returns>
    public string GetPaperFolderRelativePath(图纸 paper) {
        return $"papers/{paper.Id}";
    }

    /// <summary>
    /// 获取图纸压缩包文件的相对路径
    /// 压缩包文件主要用于批量下载和存档，包含完整的图纸资源集合
    /// 适用于大文件传输和存储空间优化
    /// </summary>
    /// <param name="paper">图纸对象，包含图纸ID和压缩包文件名</param>
    /// <returns>图纸压缩包文件的相对路径</returns>
    public string GetPaperZipFileRelativePath(图纸 paper) {
        return $"papers/{paper.Id}/{paper.压缩包文件名}";
    }

    /// <summary>
    /// 获取图纸PDF文件的相对路径
    /// PDF文件用于在线查看和打印，支持浏览器直接打开
    /// 适用于快速查看和文档共享
    /// </summary>
    /// <param name="paper">图纸对象，包含图纸ID和PDF文件名</param>
    /// <returns>图纸PDF文件的相对路径</returns>
    public string GetPaperPdfFileRelativePath(图纸 paper) {
        return $"papers/{paper.Id}/{paper.PDF文件名}";
    }

    /// <summary>
    /// 获取图纸依据文件的相对路径
    /// 依据文件包含设计规范、技术标准等相关资料，用于支撑图纸设计的技术依据
    /// 对于理解设计意图和技术要求具有重要意义
    /// </summary>
    /// <param name="paper">图纸对象，包含图纸ID和依据文件名</param>
    /// <returns>图纸依据文件的相对路径</returns>
    public string GetPaperDepFileRelativePath(图纸 paper) {
        return $"papers/{paper.Id}/{paper.依据文件名}";
    }

    /// <summary>
    /// 上传图纸压缩包文件到服务器
    /// 将本地的图纸压缩包文件上传到服务器，包含MD5校验确保文件完整性
    /// 适用于大批量图纸数据的上传和存档备份
    /// </summary>
    /// <param name="paper">图纸对象，包含图纸ID和压缩包文件信息</param>
    /// <returns>异步任务，表示上传操作的完成</returns>
    /// <exception cref="Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
    public async Task UploadPaperZipFileAsync(图纸 paper) {
        var relativePath = GetPaperZipFileRelativePath(paper);
        await WebUploadFileWithCheckMD5(relativePath);
    }

    /// <summary>
    /// 上传图纸PDF文件到服务器
    /// 将本地的图纸PDF文件上传到服务器，包含MD5校验确保文件完整性
    /// 适用于在线预览和文档共享场景
    /// </summary>
    /// <param name="paper">图纸对象，包含图纸ID和PDF文件信息</param>
    /// <returns>异步任务，表示上传操作的完成</returns>
    /// <exception cref="Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
    public async Task UploadPaperPdfFileAsync(图纸 paper) {
        var relativePath = GetPaperPdfFileRelativePath(paper);
        await WebUploadFileWithCheckMD5(relativePath);
    }

    /// <summary>
    /// 上传图纸依据文件到服务器
    /// 将本地的图纸依据文件上传到服务器，包含MD5校验确保文件完整性
    /// 依据文件对于理解设计意图和技术要求具有重要意义
    /// </summary>
    /// <param name="paper">图纸对象，包含图纸ID和依据文件信息</param>
    /// <returns>异步任务，表示上传操作的完成</returns>
    /// <exception cref="Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
    public async Task UploadPaperDepFileAsync(图纸 paper) {
        var relativePath = GetPaperDepFileRelativePath(paper);
        await WebUploadFileWithCheckMD5(relativePath);
    }

    /// <summary>
    /// 下载图纸压缩包文件到本地
    /// 从服务器下载图纸压缩包文件，包含完整的MD5校验过程
    /// 适用于下载完整的图纸资源集合用于本地处理
    /// </summary>
    /// <param name="paper">图纸对象，包含图纸ID和压缩包文件信息</param>
    /// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
    /// <exception cref="Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
    public async Task<string> DownloadPaperZipFileAsync(图纸 paper) {
        var relativePath = GetPaperZipFileRelativePath(paper);
        return await WebDownloadFileWithCheckMD5(relativePath);
    }

    /// <summary>
    /// 下载图纸PDF文件到本地
    /// 从服务器下载图纸PDF文件，包含完整的MD5校验过程
    /// 适用于在线查看、打印和文档分享场景
    /// </summary>
    /// <param name="paper">图纸对象，包含图纸ID和PDF文件信息</param>
    /// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
    /// <exception cref="Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
    public async Task<string> DownloadPaperPdfFileAsync(图纸 paper) {
        var relativePath = GetPaperPdfFileRelativePath(paper);
        return await WebDownloadFileWithCheckMD5(relativePath);
    }

    /// <summary>
    /// 下载图纸依据文件到本地
    /// 从服务器下载图纸依据文件，包含完整的MD5校验过程
    /// 依据文件包含设计规范和技术标准，对理解设计意图具有重要意义
    /// </summary>
    /// <param name="paper">图纸对象，包含图纸ID和依据文件信息</param>
    /// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
    /// <exception cref="Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
    public async Task<string> DownloadPaperDepFileAsync(图纸 paper) {
        var relativePath = GetPaperDepFileRelativePath(paper);
        return await WebDownloadFileWithCheckMD5(relativePath);
    }

}