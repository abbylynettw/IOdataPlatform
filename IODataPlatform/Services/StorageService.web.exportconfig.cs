using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExportModels;
using System.IO;
using System.Text.Json;

namespace IODataPlatform.Services
{
    /// <summary>
    /// 存储服务的导出配置管理部分
    /// 提供导出配置的云端存储管理功能，支持用户级、项目级、全局级配置的分层存储
    /// 实现配置的云端同步，支持多用户配置共享和字段版本管理
    /// 所有操作都包含MD5校验以确保配置文件的完整性和一致性
    /// </summary>
    public partial class StorageService
    {
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
        /// <exception cref="Exception">当配置上传失败或MD5校验不通过时抛出异常</exception>
        public async Task UploadUserExportConfigAsync(int userId, ExportType exportType, List<ExportConfig> configs)
        {
            var relativePath = GetUserExportConfigFileRelativePath(userId, exportType);
            var absolutePath = GetWebFileLocalAbsolutePath(relativePath);
            
            // 确保目录存在
            EnsureFileDir(absolutePath);
            
            // 序列化配置到JSON文件
            var json = JsonSerializer.Serialize(configs, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            
            await File.WriteAllTextAsync(absolutePath, json);
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
            try
            {
                var relativePath = GetUserExportConfigFileRelativePath(userId, exportType);
                var absolutePath = await WebDownloadFileWithCheckMD5(relativePath);
                
                if (!File.Exists(absolutePath))
                    return new List<ExportConfig>();
                    
                var json = await File.ReadAllTextAsync(absolutePath);
                var configs = JsonSerializer.Deserialize<List<ExportConfig>>(json);
                
                return configs ?? new List<ExportConfig>();
            }
            catch
            {
                // 如果文件不存在或下载失败，返回空列表
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
            var relativePath = GetProjectExportConfigFileRelativePath(projectId, exportType);
            var absolutePath = GetWebFileLocalAbsolutePath(relativePath);
            
            // 确保目录存在
            EnsureFileDir(absolutePath);
            
            // 序列化配置到JSON文件
            var json = JsonSerializer.Serialize(configs, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            
            await File.WriteAllTextAsync(absolutePath, json);
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
            try
            {
                var relativePath = GetProjectExportConfigFileRelativePath(projectId, exportType);
                var absolutePath = await WebDownloadFileWithCheckMD5(relativePath);
                
                if (!File.Exists(absolutePath))
                    return new List<ExportConfig>();
                    
                var json = await File.ReadAllTextAsync(absolutePath);
                var configs = JsonSerializer.Deserialize<List<ExportConfig>>(json);
                
                return configs ?? new List<ExportConfig>();
            }
            catch
            {
                // 如果文件不存在或下载失败，返回空列表
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
            var relativePath = GetGlobalExportConfigFileRelativePath(exportType);
            var absolutePath = GetWebFileLocalAbsolutePath(relativePath);
            
            // 确保目录存在
            EnsureFileDir(absolutePath);
            
            // 序列化配置到JSON文件
            var json = JsonSerializer.Serialize(configs, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            
            await File.WriteAllTextAsync(absolutePath, json);
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
            try
            {
                var relativePath = GetGlobalExportConfigFileRelativePath(exportType);
                var absolutePath = await WebDownloadFileWithCheckMD5(relativePath);
                
                if (!File.Exists(absolutePath))
                    return new List<ExportConfig>();
                    
                var json = await File.ReadAllTextAsync(absolutePath);
                var configs = JsonSerializer.Deserialize<List<ExportConfig>>(json);
                
                return configs ?? new List<ExportConfig>();
            }
            catch
            {
                // 如果文件不存在或下载失败，返回空列表
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
            var relativePath = GetUserExportConfigFileRelativePath(userId, exportType);
            try
            {
                await WebDeleteFileAsync(relativePath);
            }
            catch
            {
                // 忽略删除不存在文件的错误
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
            var relativePath = GetProjectExportConfigFileRelativePath(projectId, exportType);
            try
            {
                await WebDeleteFileAsync(relativePath);
            }
            catch
            {
                // 忽略删除不存在文件的错误
            }
        }
    }
}
