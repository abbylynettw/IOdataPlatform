/// <summary>
/// IODataPlatform.WebApi 服务端应用程序入口
/// 提供文件服务API，支持文件上传、下载、删除、复制和MD5校验等功能
/// 使用ASP.NET Core框架构建，集成Swagger文档和配置管理
/// </summary>
using IODataPlatform.WebApi.Configs;

// 创建应用程序构建器
var builder = WebApplication.CreateBuilder(args);

// 获取服务和配置对象
var svc = builder.Services;
var cfg = builder.Configuration;

// 配置服务注册
svc.Configure<FileServiceConfig>(cfg.GetSection(nameof(FileServiceConfig))); // 文件服务配置
svc.AddControllers(); // 添加控制器支持
svc.AddEndpointsApiExplorer(); // 添加API探索器
svc.AddSwaggerGen(); // 添加Swagger文档生成

// 构建应用程序
var app = builder.Build();

// 开发环境配置
if (app.Environment.IsDevelopment()) {
    app.UseSwagger(); // 启用Swagger文档
    app.UseSwaggerUI(); // 启用SwaggerUI界面
}

// 中间件管道配置
app.UseHttpsRedirection(); // HTTPS重定向
app.UseAuthorization(); // 授权验证
app.MapControllers(); // 映射控制器路由

// 启动应用程序
app.Run();

// ��������ȡ���ϴ���С����
// [DisableRequestSizeLimit]

/*
web.config �ļ��� system.webServer �ڵ��������·����������ϴ�ʵ����С������
ʵ�� 615MB ����

<security>
  <requestFiltering>
    <requestLimits maxAllowedContentLength="1073741824"/>
  </requestFiltering>
</security>
 */ 