using System.Collections.Concurrent;
using System.Security.Cryptography;

using IODataPlatform.WebApi.Configs;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using IoFile = System.IO.File;

namespace IODataPlatform.WebApi.Controllers {
    /// <summary>
    /// 文件服务控制器
    /// 提供完整的文件管理Web API服务，支持文件和文件夹的增删改查操作
    /// 包含文件上传、下载、删除、复制和MD5校验等核心功能
    /// 使用API密钥进行安全验证，支持大文件上传和并发访问控制
    /// 实现文件操作的读写锁机制，确保数据一致性和安全性
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class FileServiceController(IOptionsMonitor<FileServiceConfig> options) : ControllerBase {
        /// <summary>文件服务配置，包含本地根目录和API密钥</summary>
        private FileServiceConfig config = options.CurrentValue;

        /// <summary>正在写入操作的文件路径列表，用于并发控制</summary>
        private readonly List<string> WritingList = [];
        
        /// <summary>正在读取操作的文件路径列表，用于并发控制</summary>
        private readonly List<string> ReadingList = [];

        /// <summary>
        /// 等待文件可读取状态
        /// 在文件正在被写入时等待，直到写入操作完成后才允许读取
        /// 实现文件级别的读写互斥锁，确保数据一致性
        /// </summary>
        /// <param name="filePath">要读取的文件绝对路径</param>
        /// <returns>异步任务，表示等待操作的完成</returns>
        private async Task WaitForRead(string filePath) {
            while (WritingList.Contains(filePath.ToLower())) {
                await Task.Delay(10);
            }

            ReadingList.Add(filePath.ToLower());
        }

        /// <summary>
        /// 等待文件可写入状态
        /// 在文件正在被读取或写入时等待，直到所有操作完成后才允许写入
        /// 实现文件级别的写入互斥锁，确保数据完整性
        /// </summary>
        /// <param name="filePath">要写入的文件绝对路径</param>
        /// <returns>异步任务，表示等待操作的完成</returns>
        private async Task WaitForWrite(string filePath) {
            while (ReadingList.Contains(filePath.ToLower()) || WritingList.Contains(filePath.ToLower())) {
                await Task.Delay(10);
            }

            WritingList.Add(filePath.ToLower());
        }

        /// <summary>
        /// 上传文件接口
        /// 接收客户端上传的二进制文件数据并保存到本地文件系统
        /// 支持大文件上传，包含完整的安全验证和错误处理机制
        /// 使用读写锁确保文件操作的并发安全性
        /// </summary>
        /// <returns>返回上传结果的字符串消息</returns>
        /// <exception cref="Exception">当文件上传过程中出现IO错误时抛出异常</exception>
        [DisableRequestSizeLimit]
        [HttpPost(nameof(UploadFile), Name = nameof(UploadFile))]
        public async ValueTask<string> UploadFile() {
            if (!Request.Headers.TryGetValue("ApiKey", out Microsoft.Extensions.Primitives.StringValues keyHeader)) { return "需要提供ApiKey"; }
            if (!Request.Headers.TryGetValue("RelativePath", out Microsoft.Extensions.Primitives.StringValues relativePathHeader)) { return "需要提供RelativePath"; }
            var key = keyHeader.ToString();
            var relativePath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(relativePathHeader.ToString()));

            if (string.IsNullOrEmpty(config.Key)) { return "服务器未配置 Key"; }
            if (key != config.Key) { return "提供了不正确的key"; }
            if (string.IsNullOrEmpty(config.LocalRootDir)) { return "服务器未配置 LocalRootDir"; }
            if (!Directory.Exists(config.LocalRootDir)) { return "服务器文件夹指定的LocalRootDir不存在"; }
            if (string.IsNullOrEmpty(relativePath)) { return "未指定文件保存的相对路径"; }
            var filePath = Path.Combine(config.LocalRootDir, relativePath.Trim('/', '\\'));
            await WaitForWrite(filePath);
            var destDir = Path.GetDirectoryName(filePath)!;
            if (!Directory.Exists(destDir)) { Directory.CreateDirectory(destDir); }
            try {
                using (var stream = new FileStream(filePath, FileMode.Create)) {
                    await Request.Body.CopyToAsync(stream);
                }
                WritingList.Remove(filePath.ToLower());
                return $"文件上传成功，已保存到 {relativePath}";
            } catch (Exception ex) {
                WritingList.Remove(filePath.ToLower());
                return $"文件上传失败: {ex.Message + ex.StackTrace.ToString()}";
            }
        }

        /// <summary>
        /// 下载文件接口
        /// 根据相对路径从本地文件系统读取文件并返回给客户端
        /// 支持二进制文件流传输，包含完整的安全验证和错误处理
        /// 使用读写锁确保文件读取的并发安全性
        /// 支持从 HTTP Header 或 Query String 中获取参数，兼容浏览器图片加载
        /// </summary>
        /// <returns>返回文件流或错误响应</returns>
        /// <exception cref="Exception">当文件读取过程中出现IO错误时抛出异常</exception>
        [HttpGet(nameof(DownloadFile), Name = nameof(DownloadFile))]
        public async Task<IActionResult> DownloadFile() {
            // 优先从 Header 中读取，其次从 Query String 中读取
            string? key = null;
            string? relativePath = null;
            
            // 1. 尝试从 Header 中读取
            if (Request.Headers.TryGetValue("ApiKey", out Microsoft.Extensions.Primitives.StringValues keyHeader)) {
                key = keyHeader.ToString();
            }
            if (Request.Headers.TryGetValue("RelativePath", out Microsoft.Extensions.Primitives.StringValues relativePathHeader)) {
                relativePath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(relativePathHeader.ToString()));
            }
            
            // 2. 如果 Header 中没有，尝试从 Query String 中读取
            if (string.IsNullOrEmpty(key) && Request.Query.ContainsKey("ApiKey")) {
                key = Request.Query["ApiKey"].ToString();
            }
            if (string.IsNullOrEmpty(relativePath) && Request.Query.ContainsKey("RelativePath")) {
                var encodedPath = Request.Query["RelativePath"].ToString();
                try {
                    relativePath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedPath));
                } catch {
                    return BadRequest("无效的RelativePath编码");
                }
            }
            
            // 3. 参数验证
            if (string.IsNullOrEmpty(key)) { return BadRequest("需要提供ApiKey"); }
            if (string.IsNullOrEmpty(relativePath)) { return BadRequest("需要提供RelativePath"); }

            if (string.IsNullOrEmpty(config.Key)) { return BadRequest("服务器未配置 Key"); }
            if (key != config.Key) { return BadRequest("提供了不正确的key"); }
            if (string.IsNullOrEmpty(config.LocalRootDir)) { return BadRequest("服务器未配置 LocalRootDir"); }
            if (string.IsNullOrEmpty(relativePath)) { return BadRequest("未指定要下载的文件相对路径"); }
            var filePath = Path.Combine(config.LocalRootDir, relativePath.Trim('/', '\\'));
            await WaitForRead(filePath);
            if (!IoFile.Exists(filePath)) { return NotFound("文件不存在");  }
            try {
                var fs = new FileStream(filePath, FileMode.Open);
                var fileName = Path.GetFileName(filePath);
                var extension = Path.GetExtension(filePath).ToLower();
                
                ReadingList.Remove(filePath.ToLower());
                
                // 根据文件类型返回不同的 Content-Type
                var contentType = extension switch
                {
                    ".pdf" => "application/pdf",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".svg" => "image/svg+xml",
                    ".txt" => "text/plain",
                    ".html" or ".htm" => "text/html",
                    ".css" => "text/css",
                    ".js" => "application/javascript",
                    ".json" => "application/json",
                    _ => "application/octet-stream"
                };
                
                // 对于 PDF 文件，不设置文件名，让浏览器在线预览
                if (extension == ".pdf")
                {
                    return File(fs, contentType, enableRangeProcessing: true);
                }
                
                // 其他文件正常返回，带文件名
                return File(fs, contentType, fileName, true);
            } catch (Exception ex) {
                ReadingList.Remove(filePath.ToLower());
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 删除文件接口
        /// 根据相对路径从本地文件系统删除指定文件
        /// 包含完整的安全验证和错误处理机制
        /// 使用读写锁确保文件删除的并发安全性
        /// </summary>
        /// <returns>返回删除结果的字符串消息</returns>
        /// <exception cref="Exception">当文件删除过程中出现IO错误时抛出异常</exception>
        [HttpDelete(nameof(DeleteFile), Name = nameof(DeleteFile))]
        public async Task<string> DeleteFile() {
            if (!Request.Headers.TryGetValue("ApiKey", out Microsoft.Extensions.Primitives.StringValues keyHeader)) { return "需要提供ApiKey"; }
            if (!Request.Headers.TryGetValue("RelativePath", out Microsoft.Extensions.Primitives.StringValues relativePathHeader)) { return "需要提供RelativePath"; }
            var key = keyHeader.ToString();
            var relativePath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(relativePathHeader.ToString()));

            if (string.IsNullOrEmpty(config.Key)) { return "服务器未配置 Key"; }
            if (key != config.Key) { return "提供了不正确的key"; }
            if (string.IsNullOrEmpty(config.LocalRootDir)) { return "服务器未配置 LocalRootDir"; }
            if (string.IsNullOrEmpty(relativePath)) { return "未指定要删除的文件相对路径"; }
            var filePath = Path.Combine(config.LocalRootDir, relativePath.Trim('/', '\\'));
            await WaitForWrite(filePath);
            if (!IoFile.Exists(filePath)) { return "文件不存在"; }
            try {
                IoFile.Delete(filePath);
                WritingList.Remove(filePath.ToLower());
                return "文件删除成功";
            } catch (Exception ex) {
                WritingList.Remove(filePath.ToLower());
                return $"文件删除失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 删除文件夹接口
        /// 根据相对路径递归删除指定文件夹及其所有子文件和子文件夹
        /// 包含完整的安全验证和错误处理机制
        /// 使用读写锁确保文件夹删除的并发安全性，支持批量文件锁定
        /// </summary>
        /// <returns>返回删除结果的字符串消息</returns>
        /// <exception cref="Exception">当文件夹删除过程中出现IO错误时抛出异常</exception>
        [HttpDelete(nameof(DeleteFolder), Name = nameof(DeleteFolder))]
        public async Task<string> DeleteFolder() {
            if (!Request.Headers.TryGetValue("ApiKey", out Microsoft.Extensions.Primitives.StringValues keyHeader)) { return "需要提供ApiKey"; }
            if (!Request.Headers.TryGetValue("RelativePath", out Microsoft.Extensions.Primitives.StringValues relativePathHeader)) { return "需要提供RelativePath"; }
            var key = keyHeader.ToString();
            var relativePath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(relativePathHeader.ToString()));

            if (string.IsNullOrEmpty(config.Key)) { return "服务器未配置 Key"; }
            if (key != config.Key) { return "提供了不正确的key"; }
            if (string.IsNullOrEmpty(config.LocalRootDir)) { return "服务器未配置 LocalRootDir"; }
            if (string.IsNullOrEmpty(relativePath)) { return "未指定要删除的文件夹相对路径"; }
            var targetPath = Path.Combine(config.LocalRootDir, relativePath.Trim('/', '\\'));
            Console.WriteLine($"[DeleteFolder] 相对路径: {relativePath}");
            Console.WriteLine($"[DeleteFolder] 服务器实际路径: {targetPath}");

            // 检查路径是否存在（文件或文件夹）
            bool isDirectory = Directory.Exists(targetPath);
            bool isFile = System.IO.File.Exists(targetPath);
            
            Console.WriteLine($"[DeleteFolder] 是否为文件夹: {isDirectory}, 是否为文件: {isFile}");
            
            // 如果既不是文件也不是文件夹，说明已经不存在了
            if (!isDirectory && !isFile) {
                Console.WriteLine($"[DeleteFolder] 路径不存在，直接返回成功");
                return "文件夹删除成功（路径不存在）"; 
            }

            try {
                if (isDirectory) {
                    // 删除文件夹
                    Console.WriteLine($"[DeleteFolder] 开始删除文件夹...");
                    var files = Directory.GetFiles(targetPath, "*", new EnumerationOptions() { RecurseSubdirectories = true });
                    Console.WriteLine($"[DeleteFolder] 文件夹包含 {files.Length} 个文件");

                    foreach (var file in files) {
                        await WaitForWrite(file);
                        WritingList.Add(file.ToLower());
                    }

                    Directory.Delete(targetPath, true);
                    Console.WriteLine($"[DeleteFolder] 文件夹删除成功");
                    
                    foreach (var file in files) {
                        WritingList.Remove(file.ToLower());
                    }
                    
                    return $"文件夹删除成功（包含 {files.Length} 个文件）";
                } else {
                    // 删除单个文件
                    Console.WriteLine($"[DeleteFolder] 开始删除文件...");
                    await WaitForWrite(targetPath);
                    WritingList.Add(targetPath.ToLower());
                    
                    var fileInfo = new FileInfo(targetPath);
                    var fileName = fileInfo.Name;
                    var fileSize = fileInfo.Length;
                    
                    System.IO.File.Delete(targetPath);
                    WritingList.Remove(targetPath.ToLower());
                    
                    Console.WriteLine($"[DeleteFolder] 文件删除成功: {fileName} ({fileSize} 字节)");
                    return $"文件删除成功: {fileName}";
                }
            } catch (Exception ex) {
                Console.WriteLine($"[DeleteFolder] 删除失败: {ex.Message}");
                // 发生异常时，尝试清理锁
                try {
                    if (isDirectory && Directory.Exists(targetPath)) {
                        var files = Directory.GetFiles(targetPath, "*", new EnumerationOptions() { RecurseSubdirectories = true });
                        foreach (var file in files) {
                            WritingList.Remove(file.ToLower());
                        }
                    } else if (isFile) {
                        WritingList.Remove(targetPath.ToLower());
                    }
                } catch {
                    // 忽略清理错误
                }
                
                return $"删除失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 批量复制文件接口
        /// 根据源路径和目标路径的映射关系批量复制文件
        /// 支持同时复制多个文件，包含完整的安全验证和错误处理
        /// 使用读写锁确保文件复制的并发安全性
        /// </summary>
        /// <returns>返回复制结果的字符串消息</returns>
        /// <exception cref="Exception">当文件复制过程中出现IO错误时抛出异常</exception>
        [HttpPost(nameof(CopyFiles), Name = nameof(CopyFiles))]
        public async Task<string> CopyFiles() {
            if (!Request.Headers.TryGetValue("ApiKey", out Microsoft.Extensions.Primitives.StringValues keyHeader)) { return "需要提供ApiKey"; }
            if (!Request.Headers.TryGetValue("RelativePaths", out Microsoft.Extensions.Primitives.StringValues relativePathsHeader)) { return "需要提供RelativePaths"; }
            var key = keyHeader.ToString();
            var relativePaths = relativePathsHeader.ToString().Split(", ").Chunk(2).Select(x => new { From = x[0], To = x[1] }).ToList();

            if (string.IsNullOrEmpty(config.Key)) { return "服务器未配置 Key"; }
            if (key != config.Key) { return "提供了不正确的key"; }
            if (string.IsNullOrEmpty(config.LocalRootDir)) { return "服务器未配置 LocalRootDir"; }

            foreach (var relativePath in relativePaths) {
                var source = Path.Combine(config.LocalRootDir, relativePath.From);
                var dest = Path.Combine(config.LocalRootDir, relativePath.To);
                await WaitForWrite(dest);
                await WaitForRead(source);
                if (!IoFile.Exists(source)) { continue; }
                IoFile.Copy(source, dest, true);
                WritingList.Remove(dest.ToLower());
                ReadingList.Remove(source.ToLower());
            }

            return "文件复制成功";
        }

        /// <summary>
        /// 获取文件MD5哈希值接口
        /// 根据相对路径计算指定文件的MD5哈希值，用于文件完整性校验
        /// 包含完整的安全验证和错误处理机制
        /// 使用读写锁确保文件读取的并发安全性
        /// </summary>
        /// <returns>返回文件的MD5哈希值字符串，失败时返回错误消息</returns>
        /// <exception cref="Exception">当文件读取或哈希计算过程中出现错误时抛出异常</exception>
        [HttpGet(nameof(GetFileMD5), Name = nameof(GetFileMD5))]
        public async Task<string> GetFileMD5() {
            if (!Request.Headers.TryGetValue("ApiKey", out Microsoft.Extensions.Primitives.StringValues keyHeader)) { return "需要提供ApiKey"; }
            if (!Request.Headers.TryGetValue("RelativePath", out Microsoft.Extensions.Primitives.StringValues relativePathHeader)) { return "需要提供RelativePath"; }
            var key = keyHeader.ToString();
            var relativePath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(relativePathHeader.ToString()));

            if (string.IsNullOrEmpty(config.Key)) { return "服务器未配置 Key"; }
            if (key != config.Key) { return "提供了不正确的key"; }
            if (string.IsNullOrEmpty(config.LocalRootDir)) { return "服务器未配置 LocalRootDir"; }

            var filePath = Path.Combine(config.LocalRootDir, relativePath);
            await WaitForRead(filePath);
            if (!IoFile.Exists(filePath)) { return "文件不存在"; }
            using var md5 = MD5.Create();
            using var stream = IoFile.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            ReadingList.Remove(filePath.ToLower());
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

    }

}