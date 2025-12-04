﻿﻿﻿﻿﻿﻿using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text;
using Aspose.Cells;

using Microsoft.Extensions.DependencyInjection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LYSoft.Libs;

/// <summary>
/// Excel文件处理服务类
/// 基于Aspose.Cells组件提供完整的Excel文件操作功能，包括读取、写入、导出等操作
/// 支持多种数据格式的导入导出，适用于IO数据管理平台的Excel文件处理需求
/// 自动处理Aspose许可证初始化，确保组件正常运行
/// </summary>
public class ExcelService {

    /// <summary>
    /// Aspose.Cells组件的许可证XML内容
    /// 包含完整的企业级许可证信息，支持最多10个开发者和无限部署位置
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
    /// 创建ExcelService的新实例
    /// 自动初始化Aspose.Cells许可证，并预加载工作簿以提高后续操作性能
    /// 在后台线程中预创建工作簿实例，避免首次使用时的延迟
    /// </summary>
    public ExcelService() {
        new License().SetLicense(new MemoryStream(Encoding.UTF8.GetBytes(ASPOSE_LICENSE_TEXT)));
        Task.Run(() => new Workbook().Dispose());
    }

    /// <summary>
    /// 创建新的空白工作簿实例
    /// 返回一个新的Workbook对象，可用于创建新的Excel文件
    /// </summary>
    /// <returns>返回新创建的工作簿对象</returns>
    public Workbook GetWorkbook() => new();

    /// <summary>
    /// 从指定文件路径打开Excel工作簿
    /// 支持.xls、.xlsx、.xlsm等多种Excel格式文件
    /// </summary>
    /// <param name="fileName">Excel文件的完整路径</param>
    /// <returns>返回加载的工作簿对象</returns>
    /// <exception cref="FileNotFoundException">当指定的文件不存在时抛出</exception>
    /// <exception cref="InvalidDataException">当文件格式不受支持时抛出</exception>
    public Workbook GetWorkbook(string fileName) => new(fileName);

    /// <summary>
    /// 从字节数组加载Excel工作簿
    /// 通常用于加载嵌入资源或网络下载的Excel文件数据
    /// </summary>
    /// <param name="body">Excel文件的字节数组数据</param>
    /// <returns>返回加载的工作簿对象</returns>
    /// <exception cref="ArgumentNullException">当字节数组为null时抛出</exception>
    /// <exception cref="InvalidDataException">当数据格式不受支持时抛出</exception>
    public Workbook GetWorkbook(byte[] body) => new(new MemoryStream(body));

    /// <summary>
    /// 从指定路径的Excel文件获取第一张工作表的数据（字符串格式）
    /// 所有数据都会转换为字符串类型，适用于无法确保数据类型的场景
    /// 读取完成后会自动释放工作簿资源，避免内存泄漏
    /// </summary>
    /// <param name="fileName">Excel文件的完整路径</param>
    /// <param name="hasHeader">是否包含标题行，如果为true则第一行作为列名</param>
    /// <returns>返回包含工作表数据的DataTable，所有数据都为字符串类型</returns>
    /// <exception cref="FileNotFoundException">当指定的Excel文件不存在时抛凥</exception>
    /// <exception cref="InvalidOperationException">当工作表为空或无法读取时抛出</exception>
    public async Task<DataTable> GetDataTableAsStringAsync(string fileName, bool hasHeader) {
        return await Task.Run(() => {
            using var wb = GetWorkbook(fileName);
            var cells = wb.Worksheets[0].Cells;
            return cells.ExportDataTableAsString(0, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, hasHeader);
        });
    }
    /// <summary>
    /// 从指定路径的Excel文件获取指定工作表的数据（字符串格式）
    /// 所有数据都会转换为字符串类型，适用于需要读取特定工作表的场景
    /// </summary>
    /// <param name="fileName">Excel文件的完整路径</param>
    /// <param name="sheetName">要读取的工作表名称</param>
    /// <param name="hasHeader">是否包含标题行</param>
    /// <returns>返回包含工作表数据的DataTable</returns>
    /// <exception cref="ArgumentException">当指定的工作表名不存在时抛出</exception>
    public async Task<DataTable> GetDataTableAsStringAsync(string fileName, string sheetName,bool hasHeader)
    {
        return await Task.Run(() => {
            using var wb = GetWorkbook(fileName);            
            var cells = wb.Worksheets[sheetName].Cells;
            return cells.ExportDataTableAsString(0, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, hasHeader);
        });
    }

    /// <summary>
    /// 从指定路径的Excel文件获取第一张工作表的数据（字符串格式，从指定行开始）
    /// 支持跳过前面的多个标题行，适用于复杂的Excel模板结构
    /// </summary>
    /// <param name="fileName">Excel文件的完整路径</param>
    /// <param name="hasHeader">是否包含标题行</param>
    /// <param name="headerCount">标题行数，从第 headerCount 行开始读取数据</param>
    /// <returns>返回包含工作表数据的DataTable</returns>
    public async Task<DataTable> GetDataTableAsStringAsync(string fileName, bool hasHeader, int headerCount)
    {
        return await Task.Run(() =>
        {
            using var wb = GetWorkbook(fileName);
            var cells = wb.Worksheets[0].Cells;
            return cells.ExportDataTableAsString(headerCount-1, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, hasHeader);
        });
    }   
    /// <summary>
    /// 从指定路径的Excel文件获取第一张工作表的原始数据（保持原始数据类型）
    /// 与GetDataTableAsStringAsync不同，此方法保持数据的原始类型（数字、日期等）
    /// 适用于需要精确数据类型的计算和分析场景
    /// </summary>
    /// <param name="fileName">Excel文件的完整路径</param>
    /// <param name="hasHeader">是否包含标题行</param>
    /// <returns>返回包含工作表数据的DataTable，保持原始数据类型</returns>
    /// <exception cref="FileNotFoundException">当指定的Excel文件不存在时抛出</exception>
    public async Task<DataTable> GetDataTableAsync(string fileName, bool hasHeader) {
        return await Task.Run(() => {
            using var wb = GetWorkbook(fileName);
            var cells = wb.Worksheets[0].Cells;
            return cells.ExportDataTable(0, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, hasHeader);
        });
    }

    /// <summary>
    /// 快速导出DataTable数据到Excel文件（支持多工作表）
    /// 如果目标文件已存在，会在现有文件中添加新的工作表
    /// 如果文件不存在，会创建新的Excel文件
    /// 注意：请确保工作表名称不重复，否则可能会导致错误
    /// </summary>
    /// <param name="data">要导出的DataTable数据，包含表结构和数据</param>
    /// <param name="path">Excel文件的保存路径</param>
    /// <param name="sheetName">新工作表的名称</param>
    /// <returns>返回表示异步操作的Task</returns>
    /// <exception cref="ArgumentNullException">当data或path为null时抛出</exception>
    /// <exception cref="UnauthorizedAccessException">当没有文件写入权限时抛出</exception>
    public async Task FastExportSheetAsync(DataTable data, string path, string sheetName) {
        await Task.Run(() => {
            var isNew = !File.Exists(path);
            using var wb = isNew ? GetWorkbook() : GetWorkbook(path);
            using var ws = isNew ? wb.Worksheets[0] : wb.Worksheets[wb.Worksheets.Add()];
            ws.Name = sheetName;
            var cells = ws.Cells;
            cells.ImportData(data, 0, 0, new() { IsFieldNameShown = true, ConvertNumericData=true });
            wb.Save(path);
        });
    }
    
    /// <summary>
    /// 获取Excel文件中所有工作表的名称
    /// </summary>
    /// <param name="fileName">Excel文件的完整路径</param>
    /// <returns>返回包含所有工作表名称的列表</returns>
    /// <exception cref="FileNotFoundException">当指定的Excel文件不存在时抛出</exception>
    public async Task<List<string>> GetSheetNamesAsync(string fileName) {
        return await Task.Run(() => {
            using var wb = GetWorkbook(fileName);
            var sheetNames = new List<string>();
            for (int i = 0; i < wb.Worksheets.Count; i++) {
                sheetNames.Add(wb.Worksheets[i].Name);
            }
            return sheetNames;
        });
    }
    
    /// <summary>
    /// 使用模板文件快速导出DataTable数据到Excel文件
    /// 基于现有的Excel模板文件进行数据填充，保持模板的格式和样式
    /// 如果目标文件已存在会被删除后重新创建
    /// </summary>
    /// <param name="data">要导出的DataTable数据</param>
    /// <param name="templatePath">Excel模板文件的路径</param>
    /// <param name="path">输出Excel文件的保存路径</param>
    /// <param name="sheetName">工作表名称</param>
    /// <returns>返回表示异步操作的Task</returns>
    /// <exception cref="FileNotFoundException">当模板文件不存在时抛出</exception>
    public async Task FastExportSheetAsync(DataTable data,string templatePath, string path, string sheetName)
    {
        await Task.Run(() => {
            if (File.Exists(path)) File.Delete(path);
            using var wb =  GetWorkbook(templatePath);
            using var ws = wb.Worksheets[0];
            ws.Name = sheetName;
            var cells = ws.Cells;
            cells.ImportData(data, 0, 0, new() { IsFieldNameShown = true, ConvertNumericData = true });
            wb.Save(path);
        });
    }
    /// <summary>
    /// 快速导出DataTable数据到Excel文件（支持数字格式设置）
    /// 在导出过程中会根据数据类型自动设置数字格式，支持自定义小数位数
    /// 如果同名工作表已存在会被删除后重新创建
    /// </summary>
    /// <param name="data">要导出的DataTable数据</param>
    /// <param name="path">Excel文件的保存路径</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="digits">数字类型列的小数位数，用于控制数字显示精度</param>
    /// <returns>返回表示异步操作的Task</returns>
    public async Task FastExportSheetAsync(DataTable data, string path, string sheetName, int digits)
    {
        await Task.Run(() => {
            var isNew = !File.Exists(path);
            using var wb = isNew ? GetWorkbook() : GetWorkbook(path);

            // 如果是新建文件，删除默认的 Sheet1
            if (isNew) wb.Worksheets.RemoveAt(0);           
            else
            {
                // 删除同名的工作表
                for (int i = 0; i < wb.Worksheets.Count; i++)
                {
                    if (wb.Worksheets[i].Name == sheetName)
                    {
                        wb.Worksheets.RemoveAt(i);
                        break;
                    }
                }
            }

            // 添加新的工作表
            var ws = wb.Worksheets[wb.Worksheets.Add()];
            ws.Name = sheetName;
            var cells = ws.Cells;

            // 创建NumberFormats数组
            string[] numberFormats = new string[data.Columns.Count];
            for (int col = 0; col < data.Columns.Count; col++)
            {
                if (data.Columns[col].DataType == typeof(double))
                {
                    numberFormats[col] = "0." + new string('0', digits); // 设置保留小数位数
                }
                else if (data.Columns[col].DataType == typeof(int))
                {
                    numberFormats[col] = "0"; // 设置整数格式
                }
            }

            // 设置导出选项，包括NumberFormats
            var options = new ImportTableOptions
            {
                IsFieldNameShown = true,
                ConvertNumericData = true,
                NumberFormats = numberFormats
            };

            cells.ImportData(data, 0, 0, options);

            wb.Save(path);
        });
    }

   

    /// <summary>
    /// 快速导出DataTable数据到Excel文件（使用默认工作表名）
    /// 使用“导出数据”作为默认工作表名称，适用于快速导出单个数据表的场景
    /// </summary>
    /// <param name="data">要导出的DataTable数据</param>
    /// <param name="path">Excel文件的保存路径</param>
    /// <returns>返回表示异步操作的Task</returns>
    public async Task FastExportSheetAsync(DataTable data, string path) {
        await FastExportSheetAsync(data, path, "导出数据");
    }

    /// <summary>
    /// 快速创建新的Excel工作簿并导出DataTable数据
    /// 总是创建新的Excel文件，不会在现有文件中添加工作表
    /// 适用于需要创建全新Excel文件的场景
    /// </summary>
    /// <param name="data">要导出的DataTable数据</param>
    /// <param name="path">Excel文件的保存路径</param>
    /// <returns>返回表示异步操作的Task</returns>
    public async Task FastExportAsync(DataTable data, string path) {
        await Task.Run(() => {
            using var wb = GetWorkbook();
            using var ws = wb.Worksheets[0];
            var cells = ws.Cells;
            cells.ImportData(data, 0, 0, new() { IsFieldNameShown = true });
            wb.Save(path);
        });
    }
    /// <summary>
    /// 在现有Excel文件的指定位置插入DataTable数据
    /// 不会显示列标题，直接在指定行号开始插入数据
    /// 适用于在模板文件中的特定位置插入数据
    /// </summary>
    /// <param name="data">要插入的DataTable数据</param>
    /// <param name="path">现有Excel文件的路径</param>
    /// <param name="firstRow">开始插入数据的行号（从0开始计数）</param>
    /// <returns>返回表示异步操作的Task</returns>
    /// <exception cref="FileNotFoundException">当指定的Excel文件不存在时抛出</exception>
    public async Task FastExportSheetAsync(DataTable data, string path,int firstRow)
    {
        await Task.Run(() => {
            using var wb = GetWorkbook(path);
            using var ws = wb.Worksheets[0];
            var cells = ws.Cells;
            cells.ImportData(data, firstRow, 0, new() { IsFieldNameShown = false });
            wb.Save(path);
        });
    }    
    /// <summary>
    /// 快速导出DataTable数据到桌面的Excel文件
    /// 初始化新的工作簿并将数据导出到指定的文件路径
    /// 适用于快速创建用户可访问的报表文件
    /// </summary>
    /// <param name="data">要导出的DataTable数据</param>
    /// <param name="filePath">文件的完整保存路径（包含文件名和扩展名）</param>
    /// <returns>返回表示异步操作的Task</returns>
    public async Task FastExportToDesktopAsync(DataTable data, string filePath)
    {       
        await Task.Run(() => {
            using var wb = GetWorkbook();
            using var ws = wb.Worksheets[0];
            var cells = ws.Cells;           
            cells.ImportData(data, 0, 0, new() { IsFieldNameShown = true });
            wb.Save(filePath);
        });
    }

    /// <summary>
    /// 获取Excel文件中所有工作表的名称列表
    /// 使用内存优化模式加载，仅获取工作表名称信息而不加载完整数据
    /// 适用于需要预览或选择工作表的场景
    /// </summary>
    /// <param name="fileName">Excel文件的完整路径</param>
    /// <returns>返回包含所有工作表名称的列表</returns>
    /// <exception cref="FileNotFoundException">当指定的Excel文件不存在时抛出</exception>
    /// <exception cref="InvalidDataException">当文件格式不受支持时抛出</exception>
    public async Task<List<string>> GetSheetNames(string fileName)
    {
        return await Task.Run(() =>
        {
            var loadOptions = new LoadOptions
            {
                // 优化内存使用
                MemorySetting = MemorySetting.MemoryPreference,
                // 不需要公式解析
                ParsingFormulaOnOpen = false,
                // 设置警告回调为空
                WarningCallback = null,
                // 可以通过设置 LoadFilter 仅加载必要的内容
                LoadFilter = new LoadFilter(LoadDataFilterOptions.None)
            };

            using var wb = new Workbook(fileName, loadOptions);
            var sheetNames = new List<string>();

            foreach (Worksheet sheet in wb.Worksheets)
            {
                sheetNames.Add(sheet.Name);
            }
            return sheetNames;
        });
    }




    /// <summary>
    /// 根据指定的列名数组创建空的DataTable
    /// 创建一个新的DataTable并添加指定的列，所有列的数据类型默认为string
    /// 适用于需要动态构建表结构的场景，为后续数据填充做准备
    /// </summary>
    /// <param name="columns">列名数组，用于定义DataTable的列结构</param>
    /// <returns>返回包含指定列的空DataTable</returns>
    /// <exception cref="ArgumentNullException">当columns为null时抛出</exception>
    /// <exception cref="DuplicateNameException">当存在重复列名时抛出</exception>
    public DataTable CreateDataTable(string[] columns)
    {
         var dt = new DataTable();       
        // 添加列
        foreach (var column in columns) { dt.Columns.Add(column); }
        return dt;
    }
}

/// <summary>
/// Excel服务相关的扩展方法类
/// 提供依赖注入容器的扩展方法，用于注册和配置ExcelService服务
/// 支持.NET Core依赖注入框架，简化服务注册过程
/// </summary>
public static partial class Extension {

    /// <summary>
    /// 在依赖注入容器中注册ExcelService服务
    /// 将ExcelService注册为单例服务，确保在整个应用程序生命周期中只有一个实例
    /// 单例模式有助于优化Aspose.Cells许可证的初始化性能
    /// </summary>
    /// <param name="services">服务集合容器</param>
    /// <returns>返回服务集合，支持链式调用</returns>
    /// <example>
    /// 使用示例：
    /// <code>
    /// services.AddExcelService();
    /// </code>
    /// </example>
    public static IServiceCollection AddExcelService(this IServiceCollection services) => services.AddSingleton(new ExcelService());

}
