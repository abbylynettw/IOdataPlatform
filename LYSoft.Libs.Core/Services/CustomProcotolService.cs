using System.Diagnostics;
using System.Runtime.Versioning;

using Microsoft.Extensions.DependencyInjection;

namespace LYSoft.Libs;

/// <summary>自定义协议服务</summary>
[SupportedOSPlatform("windows")]
public class CustomProcotolService {
    private readonly string procotol;

    internal CustomProcotolService(string procotol) {
        this.procotol = procotol;
    }

    /// <summary>注册启动项到注册表</summary>
    public void Regist() {
        var surekamKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(procotol);
        var commandKey = surekamKey.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command");
        surekamKey.SetValue("URL Protocol", "");
        commandKey.SetValue("", $@"""{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Process.GetCurrentProcess().ProcessName}.exe")}"" ""%1""");
    }

    /// <summary>取消注册</summary>
    public void UnRegist() {
        Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(procotol);
    }

}

/// <summary>扩展方法</summary>
public static partial class Extensions {

    /// <summary>注册自定义协议服务</summary>
    /// <param name="services"></param>
    /// <param name="procotol">协议名</param>
    [SupportedOSPlatform("windows")]
    public static IServiceCollection AddCustomProcotolService(this IServiceCollection services, string procotol) {
        try {
            var service = new CustomProcotolService(procotol);
            service.Regist();
            return services.AddKeyedSingleton("LYSoft.Libs.CustomProcotolService", service);
        } catch {
            return services;
        }
    }

}