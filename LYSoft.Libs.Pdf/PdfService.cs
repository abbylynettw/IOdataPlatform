﻿#pragma warning disable CA1822 // 将成员标记为 static
using System.Text;

using Aspose.Pdf;
using Aspose.Pdf.Text;

using Microsoft.Extensions.DependencyInjection;

namespace LYSoft.Libs;

/// <summary>
/// PDF文件处理服务类
/// 基于Aspose.Pdf组件提供完整的PDF文件操作功能，包括文档创建、读取、表格解析等操作
/// 支持PDF文档的创建、加载和内容提取，适用于IO数据管理平台的PDF文件处理需求
/// 自动处理Aspose许可证初始化，确保组件正常运行
/// </summary>
public class PdfService {

    /// <summary>
    /// Aspose.Pdf组件的许可证XML内容
    /// 包含完整的企业级许可证信息，支持最多10个开发者和无限部署位置
    /// 与Excel服务使用相同的Aspose.Total许可证
    /// </summary>
    private const string ASPOSE_LICENSE_TEXT =
"""
<?xml version="1.0"?>
<License>
<Data>
<LicensedTo>KMD A/S</LicensedTo>
<EmailTo>iit-software@kmd.dk</EmailTo>
<LicenseType>Site OEM</LicenseType>
<LicenseNote>Up To 10 Developers And Unlimited Deployment Locations</LicenseNote>
<OrderID>220815085749</OrderID>
<UserID>324045</UserID>
<OEM>This is a redistributable license</OEM>
<Products>
<Product>Aspose.Total for .NET</Product>
</Products>
<EditionType>Enterprise</EditionType>
<SerialNumber>acea5afd-3c7d-4052-8991-f1e8522f63b4</SerialNumber>
<SubscriptionExpiry>20230818</SubscriptionExpiry>
<LicenseVersion>3.0</LicenseVersion>
<LicenseInstructions>https://purchase.aspose.com/policies/use-license</LicenseInstructions>
</Data>
<Signature>d6CNxPzdmeo0I8EJmarUMRizSisbxluOwz5BdYKprWEyJbqjvs93//lCgP0tNzxIzvniD9T7PefYeEtlkQoVKV9fo3pdjfh2QrWFxJZuRby9yzfTqK7Ahghj81URDTpneve+RAL3Z63bwkCNH0anWR0Z1I6Bdug5L8QZpduoS5k=</Signature>
</License>
""";

    /// <summary>
    /// 创建PdfService的新实例
    /// 自动初始化Aspose.Pdf许可证，并预加载PDF文档以提高后续操作性能
    /// 在后台线程中预创建Document实例，避免首次使用时的延迟
    /// </summary>
    public PdfService() {
        new License().SetLicense(new MemoryStream(Encoding.UTF8.GetBytes(ASPOSE_LICENSE_TEXT)));
        Task.Run(() => new Document().Dispose());
    }

    /// <summary>
    /// 创建新的空白PDF文档实例
    /// 返回一个新的Document对象，可用于创建新的PDF文件
    /// </summary>
    /// <returns>返回新创建的PDF文档对象</returns>
    public Document GetDocument() => new();

    /// <summary>
    /// 从指定文件路径打开PDF文档
    /// 支持各种标准PDF格式文件的加载和解析
    /// </summary>
    /// <param name="fileName">PDF文件的完整路径</param>
    /// <returns>返回加载的PDF文档对象</returns>
    /// <exception cref="FileNotFoundException">当指定的PDF文件不存在时抛出</exception>
    /// <exception cref="InvalidDataException">当文件格式不受支持或文件损坏时抛出</exception>
    public Document GetDocument(string fileName) => new(fileName);

    /// <summary>
    /// 从字节数组加载PDF文档
    /// 通常用于加载嵌入资源、网络下载或内存中的PDF文件数据
    /// 适用于处理动态生成或从其他来源获取的PDF内容
    /// </summary>
    /// <param name="body">PDF文件的字节数组数据</param>
    /// <returns>返回加载的PDF文档对象</returns>
    /// <exception cref="ArgumentNullException">当字节数组为null时抛出</exception>
    /// <exception cref="InvalidDataException">当数据格式不受支持时抛出</exception>
    public Document GetDocument(byte[] body) => new(new MemoryStream(body));

    /// <summary>
    /// 获取PDF表格单元格中的文本内容
    /// 从AbsorbedCell对象中提取所有文本片段和段落，合并为完整的文本字符串
    /// 处理复杂的单元格内容，包括多个文本片段和格式化文本
    /// </summary>
    /// <param name="cell">要提取文本的PDF表格单元格</param>
    /// <returns>返回单元格中的完整文本内容</returns>
    /// <exception cref="ArgumentNullException">当cell参数为null时抛出</exception>
    public string GetCellText(AbsorbedCell cell) {
        return string.Join("", cell.TextFragments.SelectMany(x => x.Segments).Select(x => x.Text));
    }

    /// <summary>
    /// 获取PDF文档中的所有表格
    /// 遍历PDF文档的所有页面，使用TableAbsorber提取表格结构和数据
    /// 返回文档中找到的所有表格对象，适用于表格数据的批量提取和分析
    /// </summary>
    /// <param name="doc">要解析的PDF文档</param>
    /// <returns>返回包含所有表格的可枚举集合</returns>
    /// <exception cref="ArgumentNullException">当doc参数为null时抛出</exception>
    public IEnumerable<AbsorbedTable> GetTables(Document doc) {
        var absorber = new TableAbsorber();
        return doc.Pages.SelectMany(x => {
            absorber.Visit(x);
            return absorber.TableList;
        });
    }
}

/// <summary>
/// PDF服务相关的扩展方法类
/// 提供依赖注入容器的扩展方法，用于注册和配置PdfService服务
/// 支持.NET Core依赖注入框架，简化服务注册过程
/// </summary>
public static partial class Extension {

    /// <summary>
    /// 在依赖注入容器中注册PdfService服务
    /// 将PdfService注册为单例服务，确保在整个应用程序生命周期中只有一个实例
    /// 单例模式有助于优化Aspose.Pdf许可证的初始化性能
    /// </summary>
    /// <param name="services">服务集合容器</param>
    /// <returns>返回服务集合，支持链式调用</returns>
    /// <example>
    /// 使用示例：
    /// <code>
    /// services.AddPdfService();
    /// </code>
    /// </example>
    public static IServiceCollection AddPdfService(this IServiceCollection services) => services.AddSingleton(new PdfService());

}