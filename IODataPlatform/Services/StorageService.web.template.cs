﻿using IODataPlatform.Models.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Services
{
    /// <summary>
    /// 存储服务的模板文件管理部分
    /// 提供Excel模板文件的存储管理功能，主要用于IO数据导入导出的模板管理
    /// 支持标准化的Excel模板文件上传、下载和版本控制
    /// 所有操作都包含MD5校验以确保模板文件的完整性和一致性
    /// </summary>
    public partial class StorageService
    {

        /// <summary>
        /// 获取模板依据文件的相对路径
        /// 根据Excel文件名生成标准化的模板文件存储路径
        /// 模板文件统一存储在templates目录下，便于集中管理
        /// </summary>
        /// <param name="excelName">Excel模板文件名，包含文件扩展名</param>
        /// <returns>模板文件的相对路径，格式为：templates/{excelName}</returns>
        public string GettemplatesDepFileRelativePath(string excelName)
        {
            return $"templates/{excelName}";
        }

        /// <summary>
        /// 上传模板依据文件到服务器
        /// 将本地的Excel模板文件上传到服务器，包含MD5校验确保文件完整性
        /// 适用于新增模板文件或更新现有模板文件的版本
        /// </summary>
        /// <param name="excelName">要上传的Excel模板文件名</param>
        /// <returns>异步任务，表示上传操作的完成</returns>
        /// <exception cref="Exception">当文件上传失败或MD5校验不通过时抛出异常</exception>
        public async Task UploadtemplatesDepFileAsync(string excelName)
        {
            var relativePath = GettemplatesDepFileRelativePath(excelName);
            await WebUploadFileWithCheckMD5(relativePath);
        }

        /// <summary>
        /// 下载模板依据文件到本地
        /// 从服务器下载Excel模板文件到本地，包含完整的MD5校验过程
        /// 确保下载的模板文件完整性和版本一致性，支持断点续传和错误重试
        /// </summary>
        /// <param name="excelName">要下载的Excel模板文件名</param>
        /// <returns>异步任务，返回下载后的本地文件绝对路径</returns>
        /// <exception cref="Exception">当文件下载失败或MD5校验不通过时抛出异常</exception>
        public async Task<string> DownloadtemplatesDepFileAsync(string excelName)
        {
            var relativePath = GettemplatesDepFileRelativePath(excelName);
            return await WebDownloadFileWithCheckMD5(relativePath);
        }

    }
}
