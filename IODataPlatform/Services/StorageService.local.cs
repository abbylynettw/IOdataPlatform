using System.IO;

namespace IODataPlatform.Services;

/// <summary>
/// StorageService的本地文件存储功能部分
/// 管理本地文件系统中的文件存储和访问操作
/// 主要用于临时数据和缓存文件的管理
/// </summary>
partial class StorageService {

    /// <summary>本地文件存储根目录，位于主存储目录下的Local子文件夹</summary>
    public string LocalStorageRoot { get; } = EnsureDir(Path.Combine(StorageRoot, "Local"));

    /// <summary>
    /// 获取本地文件的绝对路径
    /// 自动确保目标文件夹存在
    /// </summary>
    /// <param name="relativePath">相对于本地存储根目录的相对路径</param>
    /// <returns>返回文件的完整绝对路径</returns>
    public string GetLocalAbsolutePath(string relativePath) {
        return EnsureFileDir(Path.Combine(LocalStorageRoot, relativePath));
    }

    /// <summary>
    /// 获取电缆匹配临时数据文件的绝对路径
    /// 用于存储电缆匹配过程中的临时数据，文件格式为JSON
    /// </summary>
    /// <returns>返回电缆匹配临时数据文件的完整绝对路径</returns>
    public string GetCableTempFileRelativePath() {
        return EnsureFileDir(Path.Combine(LocalStorageRoot, "cable", "match_temp.json"));
    }

}