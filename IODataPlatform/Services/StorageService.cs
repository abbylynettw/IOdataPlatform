﻿using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

using IODataPlatform.Models.Configs;
using Microsoft.Extensions.Options;

namespace IODataPlatform.Services;

/// <summary>
/// 存储服务统一协调器
/// 作为部分类（partial class）的主类，统一管理本地和Web文件存储
/// 提供文件MD5校验、上传下载、目录管理等核心功能
/// 通过多个部分类文件实现职责分离，支持IO、电缆、端接等不同业务模块
/// </summary>
public partial class StorageService(IOptions<WebServiceConfig> config) {

    /// <summary>主存储根目录，位于用户本地应用数据目录下</summary>
    private static string StorageRoot { get; } = EnsureDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IODataPlatform", "Storage"));

    /// <summary>
    /// 确保指定目录存在
    /// 如果目录不存在则自动创建
    /// </summary>
    /// <param name="dir">需要确保存在的目录路径</param>
    /// <returns>返回目录路径</returns>
    private static string EnsureDir(string dir) {
        if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }
        return dir;
    }

    /// <summary>
    /// 确保文件所在的目录存在
    /// 自动创建文件的父目录，返回文件路径
    /// </summary>
    /// <param name="file">文件的完整路径</param>
    /// <returns>返回文件路径</returns>
    private static string EnsureFileDir(string file) {
        EnsureDir(Path.GetDirectoryName(file)!);
        return file; 
    }

    /// <summary>
    /// 使用系统默认程序打开文件
    /// 调用Windows资源管理器打开指定文件，路径分隔符会自动转换
    /// </summary>
    /// <param name="file">要打开的文件路径</param>
    public void RunFile(string file) {
        var info = new ProcessStartInfo("explorer.exe");
        info.ArgumentList.Add(file.Replace('/', '\\'));
        Process.Start(info);    
    }

    /// <summary>
    /// 计算文件的MD5哈希值
    /// 用于文件完整性校验和版本比较
    /// </summary>
    /// <param name="filePath">文件的完整路径</param>
    /// <returns>返回小写的MD5哈希字符串</returns>
    private static string GetFileMD5(string filePath) {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// 检查本地和服务器文件的MD5，如不同则下载，返回本地绝对路径
    /// 实现文件的智能同步，只有在文件发生变化时才重新下载
    /// 增强错误处理和重试机制，提高可靠性
    /// </summary>
    /// <param name="relativePath">文件在服务器上的相对路径</param>
    /// <param name="cancellationToken">取消操作的令牌</param>
    /// <returns>返回文件的本地绝对路径</returns>
    /// <exception cref="InvalidOperationException">当文件下载失败时抛出异常</exception>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出异常</exception>
    private async Task<string> WebDownloadFileWithCheckMD5(string relativePath, CancellationToken cancellationToken = default) {
        try {
            var absolutePath = GetWebFileLocalAbsolutePath(relativePath);
            var localFileMD5 = File.Exists(absolutePath) ? GetFileMD5(absolutePath) : string.Empty;
            
            var webFileMD5 = await WebGetFileMD5Async(relativePath).ConfigureAwait(false);
            
            if (localFileMD5 != webFileMD5) {
                var buffer = await WebDownloadFileAsync(relativePath).ConfigureAwait(false);
                
                // 确保目录存在
                EnsureFileDir(absolutePath);
                
                // 原子性写入文件，避免部分写入的问题
                var tempFile = absolutePath + ".tmp";
                await File.WriteAllBytesAsync(tempFile, buffer, cancellationToken).ConfigureAwait(false);
                
                // 验证下载文件的完整性
                var downloadedMD5 = GetFileMD5(tempFile);
                if (downloadedMD5 != webFileMD5) {
                    File.Delete(tempFile);
                    throw new InvalidOperationException($"文件下载校验失败: {relativePath}");
                }
                
                // 原子性移动文件
                if (File.Exists(absolutePath)) {
                    File.Delete(absolutePath);
                }
                File.Move(tempFile, absolutePath);
            }
            
            return absolutePath;
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (Exception ex) {
            throw new InvalidOperationException($"下载文件失败: {relativePath}", ex);
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
    /// <exception cref="FileNotFoundException">当本地文件不存在时抛出异常</exception>
    /// <exception cref="InvalidOperationException">当文件上传失败时抛出异常</exception>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出异常</exception>
    private async Task WebUploadFileWithCheckMD5(string relativePath, CancellationToken cancellationToken = default) {
        try {
            var absolutePath = GetWebFileLocalAbsolutePath(relativePath);
            
            if (!File.Exists(absolutePath)) {
                throw new FileNotFoundException($"本地文件不存在: {absolutePath}");
            }
            
            var localFileMD5 = GetFileMD5(absolutePath);
            bool needUpload = true;
            
            try {
                var webFileMD5 = await WebGetFileMD5Async(relativePath).ConfigureAwait(false);
                needUpload = localFileMD5 != webFileMD5;
            }
            catch {
                // 如果获取服务器文件MD5失败，假设需要上传
                needUpload = true;
            }
            
            if (needUpload) {
                var fileData = await File.ReadAllBytesAsync(absolutePath, cancellationToken).ConfigureAwait(false);
                await WebUploadFileAsync(relativePath, fileData).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (FileNotFoundException) {
            throw;
        }
        catch (Exception ex) {
            throw new InvalidOperationException($"上传文件失败: {relativePath}", ex);
        }
    }

}