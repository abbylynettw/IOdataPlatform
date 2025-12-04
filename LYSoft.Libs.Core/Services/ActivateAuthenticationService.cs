﻿﻿﻿﻿using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace LYSoft.Libs.Services;

/// <summary>
/// 应用程序激活认证服务类
/// 提供应用程序的软件激活和授权管理功能，支持基于机器码的软件授权控制
/// 通过生成唯一机器码、验证激活码来控制软件的使用权限
/// 使用MD5双重加密算法确保激活码的安全性，支持基于时间戳的动态机器码生成
/// 适用场景：商业软件的许可证管理、试用版本的激活控制、企业内部软件的授权管理
/// </summary>
/// <param name="appName">应用程序名称，用于区分不同应用的激活数据，建议使用应用程序的唯一标识符</param>
/// <exception cref="DirectoryServiceException">当无法创建认证数据存储目录时抛出</exception>
/// <exception cref="UnauthorizedAccessException">当没有权限访问应用数据目录时抛出</exception>
public class ActivateAuthenticationService(string appName)
{
    /// <summary>
    /// 应用程序认证数据的存储目录路径
    /// 位于用户应用数据目录下的LYSoft\ActivateAuthentication\{appName}文件夹
    /// 用于存储机器码和激活码等认证相关文件
    /// </summary>
    private string AppAuthDir { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LYSoft", "ActivateAuthentication", appName); }
    
    /// <summary>
    /// 原始机器码文件的完整路径
    /// 存储用户机器的唯一标识码，该文件名为"Raw"
    /// 如果目录不存在会自动创建，用于生成用户提供给管理员的机器码
    /// </summary>
    /// <exception cref="DirectoryServiceException">当无法创建目录时抛出</exception>
    private string AppRawCodeFile 
    { 
        get 
        { 
            if (!Directory.Exists(AppAuthDir)) 
            { 
                Directory.CreateDirectory(AppAuthDir); 
            } 
            return Path.Combine(AppAuthDir, "Raw"); 
        } 
    }
    
    /// <summary>
    /// 激活码文件的完整路径
    /// 存储从管理员获取的激活码，该文件名为"Activate"
    /// 如果目录不存在会自动创建，用于验证软件的激活状态
    /// </summary>
    /// <exception cref="DirectoryServiceException">当无法创建目录时抛出</exception>
    private string AppActivateCodeFile 
    { 
        get 
        { 
            if (!Directory.Exists(AppAuthDir)) 
            { 
                Directory.CreateDirectory(AppAuthDir); 
            } 
            return Path.Combine(AppAuthDir, "Activate"); 
        } 
    }

    /// <summary>
    /// 获取原始机器码，用户提供此机器码给管理员换取激活码
    /// 机器码由GUID和文件创建时间构成，确保唯一性和时间绑定
    /// 首次调用时会生成并保存机器码，后续调用直接读取已保存的值
    /// </summary>
    /// <returns>返回格式化后的机器码字符串</returns>
    public async Task<string> GetRawCodeAsync()
    {
        string? code;
        if (!File.Exists(AppRawCodeFile))
        {
            code = $"{Guid.NewGuid():D}{Guid.NewGuid():D}{Guid.NewGuid():D}".Replace("-", "");
            await File.WriteAllTextAsync(AppRawCodeFile, code);
        }
        else
        {
            code = await File.ReadAllTextAsync(AppRawCodeFile);
        }
        var time = new FileInfo(AppRawCodeFile).CreationTime;
        code = $"{code[..32]}{time:MMmmddyyyy}{code[32..64]}{time:HHfffffffss}{code[64..]}";
        return code;
    }

    /// <summary>
    /// 获取用于校验的处理后机器码
    /// 在原始机器码的基础上附加应用程序名称的Base64编码，增强安全性
    /// 此方法的返回值用于内部激活码验证计算
    /// </summary>
    /// <returns>返回处理后的机器码字符串</returns>
    public async Task<string> GetRawCodeForCheckAsync()
    {
        var rawCode = await GetRawCodeAsync();
        return $"{rawCode}{Convert.ToBase64String(Encoding.UTF8.GetBytes(appName))}";
    }

    /// <summary>
    /// 获取当前保存的激活码
    /// 从本地文件中读取之前保存的激活码，如果文件不存在则返回空字符串
    /// </summary>
    /// <returns>返回激活码字符串，如果未激活则返回空字符串</returns>
    public async Task<string> GetActivateTextAsync()
    {
        if (!File.Exists(AppActivateCodeFile)) { return string.Empty; }
        return await File.ReadAllTextAsync(AppActivateCodeFile);
    }

    /// <summary>
    /// 设置并保存激活码
    /// 将管理员提供的激活码保存到本地文件，用于后续的激活验证
    /// </summary>
    /// <param name="code">管理员提供的激活码</param>
    /// <returns>返回表示异步操作的Task</returns>
    public async Task SetActivateCodeAsync(string code)
    {
        await File.WriteAllTextAsync(AppActivateCodeFile, code);
    }

    /// <summary>
    /// 验证应用程序的激活状态
    /// 通过对比处理后的机器码的MD5双重哈希值与保存的激活码来验证激活状态
    /// 使用MD5双重加密增强安全性，防止简单的逆向破解
    /// </summary>
    /// <returns>如果激活成功返回true，否则返回false</returns>
    public async Task<bool> CheckAsync()
    {
        var raw = await GetRawCodeForCheckAsync();
        // Debug.Print(Convert.ToBase64String(MD5.HashData(MD5.HashData(Encoding.UTF8.GetBytes(raw)))).Trim('='));
        return Convert.ToBase64String(MD5.HashData(MD5.HashData(Encoding.UTF8.GetBytes(raw)))).Trim('=') == await GetActivateTextAsync();
    }

}