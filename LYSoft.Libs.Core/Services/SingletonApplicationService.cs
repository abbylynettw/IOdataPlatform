using System.IO.Pipes;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

namespace LYSoft.Libs;

/// <summary>单例应用程序服务</summary>
public class SingletonApplicationService {

    private readonly string key;

    internal SingletonApplicationService(string key) {
        this.key = key;
    }

    /// <summary>作为单例应用程序启动，并接受后续实例的命令行参数</summary>
    /// <param name="actionIfFirstInstance">如果是第一个实例，将执行的行为，一般是正常启动应用程序</param>
    /// <param name="actionReceiveOtherInstanceData">第一个实例接收到其他实例传来的参数之后的行为</param>
    /// <param name="actionIfOtherInstance">如果是不第一个实例，将执行的行为，此行为将在向第一实例传递完命令行参数后触发，一般是退出应用程序</param>
    public void StartAsSingletonApplication(Action? actionIfFirstInstance, Action<string>? actionReceiveOtherInstanceData, Action? actionIfOtherInstance) {
        actionIfOtherInstance ??= () => Environment.Exit(0);

        try {
            var pipeClient = new NamedPipeClientStream(".", key, PipeDirection.Out);
            pipeClient.Connect(10); // 尝试连接，如10ms内无法连接，说明此管道上没有监听者，也就是说，此实例是第一个APP实例，也可用TCP/UDP实现，原理相似
            WriteString(pipeClient, Environment.CommandLine);
            pipeClient.Close();
            actionIfOtherInstance();
        } catch (TimeoutException) {

            actionIfFirstInstance?.Invoke();
            Task.Run(() => {
                using var pipeServer = new NamedPipeServerStream(key, PipeDirection.In, 1); // 启动此管道上的监听程序
                while (true) {
                    try {
                        pipeServer.WaitForConnection();
                        actionReceiveOtherInstanceData?.Invoke(ReadString(pipeServer));
                    } catch {
                    } finally {
                        pipeServer.Disconnect();
                    }
                }
            });
        } catch {
            Environment.Exit(0);
        }
    }

    private UTF8Encoding encoding = new();

    private string ReadString(NamedPipeServerStream stream) {
        var len = stream.ReadByte() * 256;
        len += stream.ReadByte();
        var buffer = new byte[len];
        stream.Read(buffer, 0, len);

        return encoding.GetString(buffer);
    }

    private int WriteString(NamedPipeClientStream stream, string message) {
        var buffer = encoding.GetBytes(message);
        var len = buffer.Length;
        if (len > ushort.MaxValue) {
            len = ushort.MaxValue;
        }
        stream.WriteByte((byte)(len / 256));
        stream.WriteByte((byte)(len & 255));
        stream.Write(buffer, 0, len);
        stream.Flush();

        return buffer.Length + 2;
    }
}

/// <summary>扩展方法</summary>
public static partial class Extensions {

    /// <summary>注册单实例APP服务</summary>
    /// <param name="services"></param>
    /// <param name="key">参数应为字面量或常量，且尽量确保此参数是唯一值</param>
    public static IServiceCollection AddSingletonApplicationService(this IServiceCollection services, string key) {
        return services.AddSingleton(new SingletonApplicationService(key));
    }

}