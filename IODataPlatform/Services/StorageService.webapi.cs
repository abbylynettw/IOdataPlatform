﻿using System.Net.Http;
using System.Net.Http.Headers;

using IODataPlatform.Models.Configs;

namespace IODataPlatform.Services;

/// <summary>
/// StorageService的Web API通信功能部分
/// 封装与后端Web API服务的HTTP通信逻辑
/// 提供文件上传、下载、删除、复制和MD5校验等核心操作
/// 所有请求都包含API密钥认证和相对路径参数
/// </summary>
partial class StorageService {

    /// <summary>HTTP客户端，设置10分钟超时，用于处理大文件传输</summary>
    public HttpClient HttpClient { get; } = new() { Timeout = TimeSpan.FromSeconds(600) };
    
    /// <summary>获取Web服务配置信息</summary>
    public WebServiceConfig Config { get; } = config.Value;

    /// <summary>
    /// 向Web服务器上传文件
    /// 使用HTTP POST方式上传二进制文件数据，带有API密钥认证
    /// </summary>
    /// <param name="relativePath">文件在服务器上的相对路径</param>
    /// <param name="body">要上传的文件二进制数据</param>
    /// <returns>返回表示异步操作完成的任务</returns>
    /// <exception cref="Exception">当上传失败时抛出异常</exception>
    public async Task WebUploadFileAsync(string relativePath, byte[] body) {
        var url = $"{Config.BaseUrl}/FileService/UploadFile";
        var content = new ByteArrayContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        
        request.Headers.Add("ApiKey", Config.Key);
        request.Headers.Add("RelativePath", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(relativePath)));

        var response = await HttpClient.SendAsync(request);
        var responseString = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
        if (!responseString.StartsWith("文件上传成功")) { throw new(responseString); }
    }

    /// <summary>
    /// 从 Web服务器下载文件
    /// 使用HTTP GET方式获取文件的二进制数据，带有API密钥认证
    /// </summary>
    /// <param name="relativePath">文件在服务器上的相对路径</param>
    /// <returns>返回文件的二进制数据</returns>
    /// <exception cref="Exception">当下载失败时抛出异常</exception>
    public async Task<byte[]> WebDownloadFileAsync(string relativePath) {
        var url = $"{Config.BaseUrl}/FileService/DownloadFile";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("ApiKey", Config.Key);
        request.Headers.Add("RelativePath", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(relativePath)));

        var response = await HttpClient.SendAsync(request);
        try {
            response.EnsureSuccessStatusCode();
            var responseBuffer = await response.Content.ReadAsByteArrayAsync();
            return responseBuffer;
        } catch {
            var responseString = await response.Content.ReadAsStringAsync();
            throw new(responseString);
        }
    }

    /// <summary>
    /// 介 Web服务器删除文件
    /// 使用HTTP DELETE方式删除指定的文件，带有API密钥认证
    /// </summary>
    /// <param name="relativePath">要删除的文件在服务器上的相对路径</param>
    /// <returns>返回表示异步操作完成的任务</returns>
    /// <exception cref="Exception">当删除失败时抛出异常</exception>
    public async Task WebDeleteFileAsync(string relativePath) {
        var url = $"{Config.BaseUrl}/FileService/DeleteFile";
        var request = new HttpRequestMessage(HttpMethod.Delete, url);

        request.Headers.Add("ApiKey", Config.Key);
        request.Headers.Add("RelativePath", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(relativePath)));

        var response = await HttpClient.SendAsync(request);
        var responseString = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
        if (!responseString.StartsWith("文件删除成功")) { throw new(responseString); }
    }

    /// <summary>
    /// 介 Web服务器删除文件夹
    /// 使用HTTP DELETE方式删除指定的文件夹及其所有内容，带有API密钥认证
    /// </summary>
    /// <param name="relativePath">要删除的文件夹在服务器上的相对路径</param>
    /// <returns>返回表示异步操作完成的任务</returns>
    /// <exception cref="Exception">当删除失败时抛出异常</exception>
    public async Task WebDeleteFolderAsync(string relativePath) {
        var url = $"{Config.BaseUrl}/FileService/DeleteFolder";
        var request = new HttpRequestMessage(HttpMethod.Delete, url);

        request.Headers.Add("ApiKey", Config.Key);
        request.Headers.Add("RelativePath", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(relativePath)));

        var response = await HttpClient.SendAsync(request);
        var responseString = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
        if (!responseString.StartsWith("文件夹删除成功")) { throw new(responseString); }
    }

    /// <summary>
    /// 在Web服务器上批量复制文件
    /// 使用HTTP POST方式批量复制文件，支持多个文件的同时复制操作
    /// </summary>
    /// <param name="args">文件复制映射列表，每个元素包含源路径(From)和目标路径(To)</param>
    /// <returns>返回表示异步操作完成的任务</returns>
    /// <exception cref="Exception">当复制失败时抛出异常</exception>
    public async Task WebCopyFilesAsync(IEnumerable<(string From, string To)> args) {
        var url = $"{Config.BaseUrl}/FileService/CopyFiles";
        var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Add("ApiKey", Config.Key);
        request.Headers.Add("RelativePaths", args.SelectMany(arg => new[] { arg.From, arg.To }));

        var response = await HttpClient.SendAsync(request);
        var responseString = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
        if (!responseString.StartsWith("文件复制成功")) { throw new(responseString); }
    }

    /// <summary>
    /// 获取Web服务器上文件的MD5哈希值
    /// 使用HTTP GET方式获取文件的MD5值，用于文件完整性校验
    /// </summary>
    /// <param name="relativePath">文件在服务器上的相对路径</param>
    /// <returns>返回文件的MD5哈希字符串</returns>
    /// <exception cref="Exception">当文件不存在时抛出异常</exception>
    public async Task<string> WebGetFileMD5Async(string relativePath) {
        var url = $"{Config.BaseUrl}/FileService/GetFileMD5";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("ApiKey", Config.Key);
        request.Headers.Add("RelativePath", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(relativePath)));

        var response = await HttpClient.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        if (responseString == "文件不存在") { throw new(responseString); }
        return responseString;
    }

}