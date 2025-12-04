﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿#pragma warning disable CA1822 // 将成员标记为 static
using System.Text;

using Aspose.Words;
using Aspose.Words.Drawing;
using Aspose.Words.Tables;
using Microsoft.Extensions.DependencyInjection;

namespace LYSoft.Libs;

/// <summary>
/// Word文档处理服务类
/// 基于Aspose.Words组件提供完整的Word文档操作功能，包括文档创建、读取、表格提取等操作
/// 支持.doc、.docx、.rtf等多种Word文档格式，适用于IO数据管理平台的Word文档处理需求
/// 自动处理Aspose许可证初始化，确保组件正常运行
/// </summary>
public class WordService
{

    /// <summary>
    /// Aspose.Words组件的许可证XML内容
    /// 包含完整的企业级许可证信息，支持最多10个开发者和无限部署位置
    /// 与Excel、PDF服务使用相同的Aspose.Total许可证
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
    /// 创建WordService的新实例
    /// 自动初始化Aspose.Words许可证，并预加载Word文档以提高后续操作性能
    /// 在后台线程中预创建Document实例，避免首次使用时的延迟
    /// </summary>
    public WordService()
    {
        new License().SetLicense(new MemoryStream(Encoding.UTF8.GetBytes(ASPOSE_LICENSE_TEXT)));
        Task.Run(() => new Document());
    }

    /// <summary>
    /// 创建新的空白Word文档实例
    /// 返回一个新的Document对象，可用于创建新的Word文档
    /// 创建的文档包含默认的空白页面和基本格式设置
    /// </summary>
    /// <returns>返回新创建的Word文档对象</returns>
    public Document GetDocument() => new();

    /// <summary>
    /// 从指定文件路径打开Word文档
    /// 支持.doc、.docx、.rtf、.odt等多种Word文档格式的加载和解析
    /// 自动检测文档格式并使用相应的解析器处理文档内容
    /// </summary>
    /// <param name="fileName">Word文档的完整路径</param>
    /// <returns>返回加载的Word文档对象</returns>
    /// <exception cref="FileNotFoundException">当指定的Word文档不存在时抛出</exception>
    /// <exception cref="InvalidDataException">当文档格式不受支持或文档损坏时抛出</exception>
    /// <exception cref="UnauthorizedAccessException">当没有文件读取权限时抛出</exception>
    public Document GetDocument(string fileName) => new(fileName);

    /// <summary>
    /// 从字节数组加载Word文档
    /// 通常用于加载嵌入资源、网络下载或内存中的Word文档数据
    /// 适用于处理动态生成或从其他来源获取的Word文档内容
    /// </summary>
    /// <param name="body">Word文档的字节数组数据</param>
    /// <returns>返回加载的Word文档对象</returns>
    /// <exception cref="ArgumentNullException">当字节数组为null时抛出</exception>
    /// <exception cref="InvalidDataException">当数据格式不受支持时抛出</exception>
    public Document GetDocument(byte[] body) => new(new MemoryStream(body));

    /// <summary>
    /// 获取Word文档中所有顶层表格
    /// 遍历文档的所有节和正文部分，提取所有顶层表格对象（不包括嵌套表格）
    /// 返回的表格按在文档中出现的顺序排列，适用于表格数据的批量提取和分析
    /// </summary>
    /// <param name="doc">要提取表格的Word文档</param>
    /// <returns>返回包含所有顶层表格的列表</returns>
    /// <exception cref="ArgumentNullException">当doc参数为null时抛出</exception>
    public List<Table> GetTables(Document doc)
    {
        return doc.Sections.OfType<Section>().Select(x => x.Body).SelectMany(x => x.Tables).OfType<Table>().ToList();
    }

    /// <summary>
    /// 复制字体格式属性（完整复制所有属性，避免遗漏）
    /// </summary>
    private void CopyFontFormat(Font source, Font target)
    {
        // 使用反射复制所有可写属性，确保不遗漏任何格式
        var properties = typeof(Font).GetProperties()
            .Where(p => p.CanWrite && p.CanRead && p.GetIndexParameters().Length == 0);
        
        foreach (var prop in properties)
        {
            try
            {
                // 跳过只读或特殊属性
                if (prop.Name == "Style" || prop.Name == "Document" || prop.Name == "ParentNode")
                    continue;
                    
                var value = prop.GetValue(source);
                
                // 特殊处理复杂类型
                if (prop.Name == "Border")
                {
                    CopyFontBorder(source.Border, target.Border);
                }
                else if (prop.Name == "Shading")
                {
                    CopyShading(source.Shading, target.Shading);
                }
                else if (value != null && (prop.PropertyType.IsValueType || prop.PropertyType == typeof(string) || prop.PropertyType.IsEnum))
                {
                    prop.SetValue(target, value);
                }
            }
            catch
            {
                // 忽略无法复制的属性
            }
        }
    }
    
    /// <summary>
    /// 复制字体边框格式
    /// </summary>
    private void CopyFontBorder(Border source, Border target)
    {
        try
        {
            target.LineStyle = source.LineStyle;
            target.LineWidth = source.LineWidth;
            target.Color = source.Color;
            target.DistanceFromText = source.DistanceFromText;
            target.Shadow = source.Shadow;
        }
        catch { }
    }

    /// <summary>
    /// 复制段落格式属性（完整复制所有属性，避免遗漏）
    /// </summary>
    private void CopyParagraphFormatProperties(ParagraphFormat source, ParagraphFormat target)
    {
        // 使用反射复制所有可写属性，确保不遗漏任何格式
        var properties = typeof(ParagraphFormat).GetProperties()
            .Where(p => p.CanWrite && p.CanRead && p.GetIndexParameters().Length == 0);
        
        foreach (var prop in properties)
        {
            try
            {
                // 跳过只读或特殊属性
                if (prop.Name == "Style" || prop.Name == "Document" || prop.Name == "ParentNode")
                    continue;
                    
                var value = prop.GetValue(source);
                
                // 特殊处理集合类型
                if (prop.Name == "TabStops")
                {
                    target.TabStops.Clear();
                    // TabStopCollection 使用索引访问
                    for (int i = 0; i < source.TabStops.Count; i++)
                    {
                        var tab = source.TabStops[i];
                        target.TabStops.Add(tab.Position, tab.Alignment, tab.Leader);
                    }
                }
                else if (prop.Name == "Borders")
                {
                    CopyBorders(source.Borders, target.Borders);
                }
                else if (prop.Name == "Shading")
                {
                    CopyShading(source.Shading, target.Shading);
                }
                else if (value != null && prop.PropertyType.IsValueType || prop.PropertyType == typeof(string) || prop.PropertyType.IsEnum)
                {
                    prop.SetValue(target, value);
                }
            }
            catch
            {
                // 忽略无法复制的属性
            }
        }
    }
    
    /// <summary>
    /// 复制边框格式
    /// </summary>
    private void CopyBorders(BorderCollection source, BorderCollection target)
    {
        try
        {
            target.LineStyle = source.LineStyle;
            target.LineWidth = source.LineWidth;
            target.Color = source.Color;
            target.DistanceFromText = source.DistanceFromText;
            target.Shadow = source.Shadow;
        }
        catch { }
    }
    
    /// <summary>
    /// 复制底纹格式
    /// </summary>
    private void CopyShading(Shading source, Shading target)
    {
        try
        {
            target.Texture = source.Texture;
            target.BackgroundPatternColor = source.BackgroundPatternColor;
            target.ForegroundPatternColor = source.ForegroundPatternColor;
        }
        catch { }
    }

    /// <summary>
    /// 将模板文档的格式应用到目标文档
    /// 包括样式、段落格式、字体格式、表格格式、图片格式等
    /// </summary>
    /// <param name="templateDoc">作为格式源的模板文档</param>
    /// <param name="targetDoc">需要应用格式的目标文档</param>
    public void ApplyFormatsFromTemplate(Document templateDoc, Document targetDoc)
    {
        // 1. 复制所有样式定义
        CopyStyles(templateDoc, targetDoc);

        // 2. 应用段落格式（包括标题、正文等）
        ApplyParagraphFormats(templateDoc, targetDoc);

        // 3. 应用表格格式
        ApplyTableFormats(templateDoc, targetDoc);

        // 4. 应用图片/图形格式
        ApplyImageFormats(templateDoc, targetDoc);

        // 5. 应用页面设置（页边距、纸张大小等）
        ApplyPageSetup(templateDoc, targetDoc);
    }

    /// <summary>
    /// 复制模板文档的所有样式到目标文档
    /// 包括标题样式、正文样式、列表样式等
    /// </summary>
    private void CopyStyles(Document templateDoc, Document targetDoc)
    {
        foreach (Style style in templateDoc.Styles)
        {
            // 跳过内置样式，只处理自定义样式
            if (!style.BuiltIn)
            {
                if (!targetDoc.Styles.Any(s => s.Name == style.Name))
                {
                    var styleType = style.Type;
                    var newStyle = targetDoc.Styles.Add(styleType, style.Name);
                    CopyFontFormat(style.Font, newStyle.Font);
                    if (styleType == StyleType.Paragraph)
                    {
                        CopyParagraphFormatProperties(style.ParagraphFormat, newStyle.ParagraphFormat);
                    }
                }
            }
            else
            {
                // 对于内置样式，更新其格式
                if (targetDoc.Styles.Any(s => s.Name == style.Name))
                {
                    var targetStyle = targetDoc.Styles[style.Name];
                    CopyFontFormat(style.Font, targetStyle.Font);
                    if (style.Type == StyleType.Paragraph)
                    {
                        CopyParagraphFormatProperties(style.ParagraphFormat, targetStyle.ParagraphFormat);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 应用段落格式，包括标题和正文的格式
    /// 根据段落的大纲级别和样式匹配对应的模板格式
    /// </summary>
    private void ApplyParagraphFormats(Document templateDoc, Document targetDoc)
    {
        // 获取模板中的段落格式样本
        var templateParagraphs = templateDoc.GetChildNodes(NodeType.Paragraph, true)
            .OfType<Paragraph>()
            .Where(p => !string.IsNullOrEmpty(p.GetText().Trim()))
            .ToList();

        if (!templateParagraphs.Any()) return;

        // 按大纲级别分组模板段落
        var templateByLevel = new Dictionary<OutlineLevel, Paragraph>();
        foreach (var para in templateParagraphs)
        {
            var level = para.ParagraphFormat.OutlineLevel;
            if (!templateByLevel.ContainsKey(level))
            {
                templateByLevel[level] = para;
            }
        }

        // 为目标文档的段落应用格式
        var targetParagraphs = targetDoc.GetChildNodes(NodeType.Paragraph, true)
            .OfType<Paragraph>()
            .ToList();

        foreach (var targetPara in targetParagraphs)
        {
            var level = targetPara.ParagraphFormat.OutlineLevel;
            
            // 如果找到对应级别的模板格式，应用它
            if (templateByLevel.ContainsKey(level))
            {
                var templatePara = templateByLevel[level];
                ApplyParagraphFormat(templatePara, targetPara);
            }
            else if (templateByLevel.ContainsKey(OutlineLevel.BodyText))
            {
                // 默认使用正文格式
                var templatePara = templateByLevel[OutlineLevel.BodyText];
                ApplyParagraphFormat(templatePara, targetPara);
            }
        }
    }

    /// <summary>
    /// 将单个段落的格式从模板应用到目标段落
    /// </summary>
    private void ApplyParagraphFormat(Paragraph templatePara, Paragraph targetPara)
    {
        // 复制段落格式
        CopyParagraphFormatProperties(templatePara.ParagraphFormat, targetPara.ParagraphFormat);

        // 复制字符格式（如果目标段落有文本）
        if (targetPara.Runs.Count > 0 && templatePara.Runs.Count > 0)
        {
            var templateRun = templatePara.Runs[0];
            foreach (Run targetRun in targetPara.Runs)
            {
                CopyFontFormat(templateRun.Font, targetRun.Font);
            }
        }
    }

    /// <summary>
    /// 应用表格格式
    /// 将模板中的表格样式应用到目标文档的表格
    /// </summary>
    private void ApplyTableFormats(Document templateDoc, Document targetDoc)
    {
        var templateTables = GetTables(templateDoc);
        var targetTables = GetTables(targetDoc);

        if (!templateTables.Any()) return;

        // 选择最有代表性的表格作为格式源
        var templateTable = SelectRepresentativeTable(templateTables, templateDoc);

        foreach (var targetTable in targetTables)
        {
            ApplyTableFormat(templateTable, targetTable);
        }
    }
    
    /// <summary>
    /// 应用表格格式（包含题注选项）
    /// </summary>
    private void ApplyTableFormatsWithOptions(Document templateDoc, Document targetDoc, bool captionsByChapter, bool captionSpacing)
    {
        // 先应用基本表格格式
        ApplyTableFormats(templateDoc, targetDoc);
        
        // 如果需要应用题注格式
        if (captionsByChapter || captionSpacing)
        {
            ApplyTableCaptionFormats(templateDoc, targetDoc, captionsByChapter, captionSpacing);
        }
    }
    
    /// <summary>
    /// 应用表格题注格式
    /// </summary>
    private void ApplyTableCaptionFormats(Document templateDoc, Document targetDoc, bool addChapterNumber, bool addSpacing)
    {
        // 查找模板中的表格题注
        var templateTables = templateDoc.GetChildNodes(NodeType.Table, true).OfType<Table>().ToList();
        if (!templateTables.Any()) return;
        
        Paragraph? templateCaption = null;
        foreach (var tbl in templateTables)
        {
            // 查找前一个段落（题注可能在表上方）
            var prevNode = tbl.PreviousSibling;
            while (prevNode != null && !(prevNode is Paragraph))
            {
                prevNode = prevNode.PreviousSibling;
            }
            
            if (prevNode is Paragraph prevPara)
            {
                var prevText = prevPara.GetText().Trim();
                var hasSeqField = prevPara.Range.Fields.Any(f => 
                    f.Type == Aspose.Words.Fields.FieldType.FieldSequence || 
                    f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);
                
                if (hasSeqField || prevText.StartsWith("表"))
                {
                    templateCaption = prevPara;
                    break;
                }
            }
            
            // 如果上方没有，查找下一个段落
            if (templateCaption == null)
            {
                var nextNode = tbl.NextSibling;
                while (nextNode != null && !(nextNode is Paragraph))
                {
                    nextNode = nextNode.NextSibling;
                }
                
                if (nextNode is Paragraph nextPara)
                {
                    var nextText = nextPara.GetText().Trim();
                    var hasSeqField = nextPara.Range.Fields.Any(f => 
                        f.Type == Aspose.Words.Fields.FieldType.FieldSequence || 
                        f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);
                    
                    if (hasSeqField || nextText.StartsWith("表"))
                    {
                        templateCaption = nextPara;
                        break;
                    }
                }
            }
        }
        
        if (templateCaption == null) return;
        
        // 应用到目标文档的所有表格
        var targetTables = targetDoc.GetChildNodes(NodeType.Table, true).OfType<Table>().ToList();
        foreach (var tbl in targetTables)
        {
            Paragraph? captionPara = null;
            bool hasCaptionText = false;
            
            // 查找前一个段落
            var prevNode = tbl.PreviousSibling;
            while (prevNode != null && !(prevNode is Paragraph))
            {
                prevNode = prevNode.PreviousSibling;
            }
            
            if (prevNode is Paragraph prevPara)
            {
                var prevText = prevPara.GetText().Trim();
                var hasSeqField = prevPara.Range.Fields.Any(f => 
                    f.Type == Aspose.Words.Fields.FieldType.FieldSequence || 
                    f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);
                
                if (hasSeqField || prevText.StartsWith("表"))
                {
                    captionPara = prevPara;
                    hasCaptionText = true;
                }
            }
            
            // 如果上方没有，查找下一个段落
            if (captionPara == null)
            {
                var nextNode = tbl.NextSibling;
                while (nextNode != null && !(nextNode is Paragraph))
                {
                    nextNode = nextNode.NextSibling;
                }
                
                if (nextNode is Paragraph nextPara)
                {
                    var nextText = nextPara.GetText().Trim();
                    var hasSeqField = nextPara.Range.Fields.Any(f => 
                        f.Type == Aspose.Words.Fields.FieldType.FieldSequence || 
                        f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);
                    
                    if (hasSeqField || nextText.StartsWith("表"))
                    {
                        captionPara = nextPara;
                        hasCaptionText = true;
                    }
                }
            }
            
            // 如果没有题注，在表格前创建一个
            if (captionPara == null)
            {
                var doc = (Document)targetDoc;
                var builder = new DocumentBuilder(doc);
                
                // 在表格前插入新段落作为题注
                builder.MoveTo(tbl.FirstRow.FirstCell.FirstParagraph);
                captionPara = builder.InsertParagraph();
                
                // 将题注段落移到表格前面
                var parent = tbl.ParentNode;
                parent.InsertBefore(captionPara, tbl);
                
                // 设置为题注样式
                captionPara.ParagraphFormat.StyleIdentifier = StyleIdentifier.Caption;
            }
            
            // 应用格式
            CopyParagraphFormatProperties(templateCaption.ParagraphFormat, captionPara.ParagraphFormat);
            if (templateCaption.Runs.Count > 0 && captionPara.Runs.Count > 0)
            {
                CopyFontFormat(templateCaption.Runs[0].Font, captionPara.Runs[0].Font);
            }
            
            // 如果需要前后空一行，设置段前距和段后距
            if (addSpacing)
            {
                captionPara.ParagraphFormat.SpaceBefore = Math.Max(12, templateCaption.ParagraphFormat.SpaceBefore);
                captionPara.ParagraphFormat.SpaceAfter = Math.Max(12, templateCaption.ParagraphFormat.SpaceAfter);
            }
            
            // 如果需要按章排列，确保STYLEREF字段存在
            if (addChapterNumber)
            {
                EnsureCaptionHasChapterNumber(captionPara, templateCaption, "表");
            }
            else if (!hasCaptionText)
            {
                // 如果不需要按章排列，但是新建的题注，添加简单题注
                AddSimpleCaption(captionPara, "表");
            }
            
            // 更新字段
            UpdateCaptionFields(captionPara);
        }
    }

    /// <summary>
    /// 选择最有代表性的表格（优先选择有题注或有说明的表格）
    /// </summary>
    private Table SelectRepresentativeTable(List<Table> tables, Document doc)
    {
        // 1. 优先选择有题注或有上下方说明的表格
        foreach (var table in tables)
        {
            var captionInfo = AnalyzeTableCaption(table, doc);
            
            // 如果有上方题注或下方说明，就认为是有效表格
            if (!string.IsNullOrEmpty(captionInfo.AboveText) || 
                !string.IsNullOrEmpty(captionInfo.BelowText))
            {
                return table; // 返回第一个有题注或说明的表格
            }
        }

        // 2. 如果没有题注，选择有边框的表格（更可能是正式表格）
        var borderedTable = tables.FirstOrDefault(t => 
            t.FirstRow != null && 
            t.FirstRow.RowFormat.Borders.LineStyle != LineStyle.None);
        if (borderedTable != null)
        {
            return borderedTable;
        }

        // 3. 默认返回第一个表格
        return tables[0];
    }

    /// <summary>
    /// 将单个表格的格式从模板应用到目标表格
    /// </summary>
    private void ApplyTableFormat(Table templateTable, Table targetTable)
    {
        // 复制表格级别属性
        targetTable.Alignment = templateTable.Alignment;
        targetTable.PreferredWidth = templateTable.PreferredWidth;
        targetTable.LeftIndent = templateTable.LeftIndent;
        targetTable.AllowAutoFit = templateTable.AllowAutoFit;
        targetTable.Bidi = templateTable.Bidi;

        // 复制表格样式
        if (templateTable.Style != null)
        {
            targetTable.StyleIdentifier = templateTable.StyleIdentifier;
        }

        // 复制边框
        targetTable.SetBorders(
            templateTable.FirstRow?.RowFormat?.Borders?.LineStyle ?? LineStyle.Single,
            templateTable.FirstRow?.RowFormat?.Borders?.LineWidth ?? 0.5,
            templateTable.FirstRow?.RowFormat?.Borders?.Color ?? System.Drawing.Color.Black
        );

        // 为每个单元格应用格式
        for (int rowIndex = 0; rowIndex < targetTable.Rows.Count && rowIndex < templateTable.Rows.Count; rowIndex++)
        {
            var templateRow = templateTable.Rows[rowIndex];
            var targetRow = targetTable.Rows[rowIndex];

            // 复制行格式
            targetRow.RowFormat.Height = templateRow.RowFormat.Height;
            targetRow.RowFormat.HeightRule = templateRow.RowFormat.HeightRule;
            targetRow.RowFormat.AllowBreakAcrossPages = templateRow.RowFormat.AllowBreakAcrossPages;

            for (int cellIndex = 0; cellIndex < targetRow.Cells.Count && cellIndex < templateRow.Cells.Count; cellIndex++)
            {
                var templateCell = templateRow.Cells[cellIndex];
                var targetCell = targetRow.Cells[cellIndex];

                ApplyCellFormat(templateCell, targetCell);
            }
        }
    }

    /// <summary>
    /// 应用单元格格式
    /// </summary>
    private void ApplyCellFormat(Cell templateCell, Cell targetCell)
    {
        // 复制单元格格式
        targetCell.CellFormat.VerticalAlignment = templateCell.CellFormat.VerticalAlignment;
        targetCell.CellFormat.Width = templateCell.CellFormat.Width;
        targetCell.CellFormat.PreferredWidth = templateCell.CellFormat.PreferredWidth;
        targetCell.CellFormat.Shading.BackgroundPatternColor = templateCell.CellFormat.Shading.BackgroundPatternColor;
        targetCell.CellFormat.Shading.ForegroundPatternColor = templateCell.CellFormat.Shading.ForegroundPatternColor;

        // 复制边框
        targetCell.CellFormat.Borders.LineStyle = templateCell.CellFormat.Borders.LineStyle;
        targetCell.CellFormat.Borders.LineWidth = templateCell.CellFormat.Borders.LineWidth;
        targetCell.CellFormat.Borders.Color = templateCell.CellFormat.Borders.Color;

        // 复制段落格式
        if (templateCell.Paragraphs.Count > 0 && targetCell.Paragraphs.Count > 0)
        {
            var templatePara = templateCell.Paragraphs[0];
            foreach (Paragraph targetPara in targetCell.Paragraphs)
            {
                ApplyParagraphFormat(templatePara, targetPara);
            }
        }
    }

    /// <summary>
    /// 应用图片和图形格式
    /// 包括大小、位置、环绕方式等
    /// </summary>
    private void ApplyImageFormats(Document templateDoc, Document targetDoc)
    {
        var templateShapes = templateDoc.GetChildNodes(NodeType.Shape, true)
            .OfType<Shape>()
            .Where(s => s.HasImage)
            .ToList();

        var targetShapes = targetDoc.GetChildNodes(NodeType.Shape, true)
            .OfType<Shape>()
            .Where(s => s.HasImage)
            .ToList();

        if (!templateShapes.Any()) return;

        // 选择最有代表性的图片作为格式源
        var templateShape = SelectRepresentativeShape(templateShapes, templateDoc);

        foreach (var targetShape in targetShapes)
        {
            ApplyShapeFormat(templateShape, targetShape);
        }
    }
    
    /// <summary>
    /// 应用图片格式（包含题注选项）
    /// </summary>
    private void ApplyImageFormatsWithOptions(Document templateDoc, Document targetDoc, bool captionsByChapter, bool captionSpacing)
    {
        // 先应用基本图片格式
        ApplyImageFormats(templateDoc, targetDoc);
        
        // 如果需要应用题注格式
        if (captionsByChapter || captionSpacing)
        {
            ApplyImageCaptionFormats(templateDoc, targetDoc, captionsByChapter, captionSpacing);
        }
    }
    
    /// <summary>
    /// 应用图片题注格式
    /// </summary>
    private void ApplyImageCaptionFormats(Document templateDoc, Document targetDoc, bool addChapterNumber, bool addSpacing)
    {
        // 查找模板中的图片题注
        var templateImages = templateDoc.GetChildNodes(NodeType.Shape, true).OfType<Shape>().Where(s => s.HasImage).ToList();
        if (!templateImages.Any()) return;
        
        Paragraph? templateCaption = null;
        foreach (var img in templateImages)
        {
            var imgPara = img.ParentParagraph;
            if (imgPara != null)
            {
                var nextNode = imgPara.NextSibling;
                while (nextNode != null && !(nextNode is Paragraph))
                {
                    nextNode = nextNode.NextSibling;
                }
                
                if (nextNode is Paragraph captionPara)
                {
                    var captionText = captionPara.GetText().Trim();
                    var hasSeqField = captionPara.Range.Fields.Any(f => 
                        f.Type == Aspose.Words.Fields.FieldType.FieldSequence || 
                        f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);
                    
                    if (hasSeqField || captionText.StartsWith("图"))
                    {
                        templateCaption = captionPara;
                        break;
                    }
                }
            }
        }
        
        if (templateCaption == null) return;
        
        // 应用到目标文档的所有图片
        var targetImages = targetDoc.GetChildNodes(NodeType.Shape, true).OfType<Shape>().Where(s => s.HasImage).ToList();
        foreach (var img in targetImages)
        {
            var imgPara = img.ParentParagraph;
            if (imgPara != null)
            {
                var nextNode = imgPara.NextSibling;
                while (nextNode != null && !(nextNode is Paragraph))
                {
                    nextNode = nextNode.NextSibling;
                }
                
                Paragraph? captionPara = null;
                bool hasCaptionText = false;
                
                if (nextNode is Paragraph nextPara)
                {
                    var captionText = nextPara.GetText().Trim();
                    var hasSeqField = nextPara.Range.Fields.Any(f => 
                        f.Type == Aspose.Words.Fields.FieldType.FieldSequence || 
                        f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);
                    
                    if (hasSeqField || captionText.StartsWith("图"))
                    {
                        captionPara = nextPara;
                        hasCaptionText = true;
                    }
                }
                
                // 如果没有题注，创建一个
                if (captionPara == null)
                {
                    var doc = (Document)targetDoc;
                    var builder = new DocumentBuilder(doc);
                    
                    // 在图片后插入新段落作为题注
                    builder.MoveTo(imgPara);
                    builder.MoveToBookmark(""); // 移到段落结束
                    captionPara = builder.InsertParagraph();
                    
                    // 设置为题注样式
                    captionPara.ParagraphFormat.StyleIdentifier = StyleIdentifier.Caption;
                }
                
                // 应用格式
                CopyParagraphFormatProperties(templateCaption.ParagraphFormat, captionPara.ParagraphFormat);
                if (templateCaption.Runs.Count > 0 && captionPara.Runs.Count > 0)
                {
                    CopyFontFormat(templateCaption.Runs[0].Font, captionPara.Runs[0].Font);
                }
                
                // 如果需要前后空一行，设置段前距和段后距
                if (addSpacing)
                {
                    captionPara.ParagraphFormat.SpaceBefore = Math.Max(12, templateCaption.ParagraphFormat.SpaceBefore);
                    captionPara.ParagraphFormat.SpaceAfter = Math.Max(12, templateCaption.ParagraphFormat.SpaceAfter);
                }
                
                // 如果需要按章排列，确保STYLEREF字段存在
                if (addChapterNumber)
                {
                    EnsureCaptionHasChapterNumber(captionPara, templateCaption, "图");
                }
                else if (!hasCaptionText)
                {
                    // 如果不需要按章排列，但是新建的题注，添加简单题注
                    AddSimpleCaption(captionPara, "图");
                }
                
                // 更新字段
                UpdateCaptionFields(captionPara);
            }
        }
    }

    /// <summary>
    /// 选择最有代表性的图片（优先选择有题注的、正文中的图片）
    /// </summary>
    private Shape SelectRepresentativeShape(List<Shape> shapes, Document doc)
    {
        // 1. 优先选择有题注的图片
        foreach (var shape in shapes)
        {
            var captionPos = AnalyzeCaptionPosition(shape, doc);
            if (!captionPos.Contains("无图注") && !captionPos.Contains("无法分析"))
            {
                return shape; // 返回第一个有题注的图片
            }
        }

        // 2. 如果没有题注，选择非 Inline 的图片（更可能是正文图片）
        var nonInlineShape = shapes.FirstOrDefault(s => s.WrapType != Aspose.Words.Drawing.WrapType.Inline);
        if (nonInlineShape != null)
        {
            return nonInlineShape;
        }

        // 3. 默认返回第一个图片
        return shapes[0];
    }

    /// <summary>
    /// 应用图形格式
    /// 包括尺寸、对齐、环绕、边框等所有格式
    /// </summary>
    private void ApplyShapeFormat(Shape templateShape, Shape targetShape)
    {
        // 复制锁定纵横比设置
        targetShape.AspectRatioLocked = templateShape.AspectRatioLocked;

        // 复制文本环绕方式
        targetShape.WrapType = templateShape.WrapType;
        targetShape.WrapSide = templateShape.WrapSide;
        
        // 复制环绕距离
        targetShape.DistanceTop = templateShape.DistanceTop;
        targetShape.DistanceBottom = templateShape.DistanceBottom;
        targetShape.DistanceLeft = templateShape.DistanceLeft;
        targetShape.DistanceRight = templateShape.DistanceRight;

        // 复制对齐方式
        targetShape.HorizontalAlignment = templateShape.HorizontalAlignment;
        targetShape.VerticalAlignment = templateShape.VerticalAlignment;
        targetShape.RelativeHorizontalPosition = templateShape.RelativeHorizontalPosition;
        targetShape.RelativeVerticalPosition = templateShape.RelativeVerticalPosition;
        
        // 复制绝对位置（如果有）
        if (templateShape.HorizontalAlignment == Aspose.Words.Drawing.HorizontalAlignment.None)
        {
            targetShape.Left = templateShape.Left;
        }
        if (templateShape.VerticalAlignment == Aspose.Words.Drawing.VerticalAlignment.None)
        {
            targetShape.Top = templateShape.Top;
        }

        // 复制边框和填充
        if (templateShape.Stroke != null && targetShape.Stroke != null)
        {
            targetShape.Stroke.Visible = templateShape.Stroke.Visible;
            if (templateShape.Stroke.Visible)
            {
                targetShape.Stroke.Color = templateShape.Stroke.Color;
                targetShape.Stroke.Weight = templateShape.Stroke.Weight;
                targetShape.Stroke.LineStyle = templateShape.Stroke.LineStyle;
            }
        }
        
        // 复制其他属性
        targetShape.AllowOverlap = templateShape.AllowOverlap;
        targetShape.BehindText = templateShape.BehindText;
    }

    /// <summary>
    /// 应用页面设置
    /// 包括页边距、纸张大小、页眉页脚等
    /// </summary>
    private void ApplyPageSetup(Document templateDoc, Document targetDoc)
    {
        foreach (Section targetSection in targetDoc.Sections)
        {
            if (templateDoc.Sections.Count > 0)
            {
                var templateSection = templateDoc.Sections[0];
                var templateSetup = templateSection.PageSetup;
                var targetSetup = targetSection.PageSetup;

                // 复制页面尺寸
                targetSetup.PaperSize = templateSetup.PaperSize;
                targetSetup.PageWidth = templateSetup.PageWidth;
                targetSetup.PageHeight = templateSetup.PageHeight;
                targetSetup.Orientation = templateSetup.Orientation;

                // 复制页边距
                targetSetup.TopMargin = templateSetup.TopMargin;
                targetSetup.BottomMargin = templateSetup.BottomMargin;
                targetSetup.LeftMargin = templateSetup.LeftMargin;
                targetSetup.RightMargin = templateSetup.RightMargin;
                targetSetup.HeaderDistance = templateSetup.HeaderDistance;
                targetSetup.FooterDistance = templateSetup.FooterDistance;

                // 复制其他设置
                targetSetup.DifferentFirstPageHeaderFooter = templateSetup.DifferentFirstPageHeaderFooter;
                targetSetup.OddAndEvenPagesHeaderFooter = templateSetup.OddAndEvenPagesHeaderFooter;
            }
        }
    }

    /// <summary>
    /// 格式刷：从模板文件复制格式到目标文件
    /// </summary>
    /// <param name="templatePath">模板文件路径</param>
    /// <param name="targetPath">目标文件路径</param>
    /// <param name="outputPath">输出文件路径</param>
    public void ApplyFormatFromFile(string templatePath, string targetPath, string outputPath)
    {
        var templateDoc = GetDocument(templatePath);
        var targetDoc = GetDocument(targetPath);

        ApplyFormatsFromTemplate(templateDoc, targetDoc);

        targetDoc.Save(outputPath);
    }

    /// <summary>
    /// 方案B：导入合并方式应用模板格式（一键套模板）
    /// 以模板为基底，将目标内容导入并使用模板的样式定义
    /// </summary>
    /// <param name="templatePath">模板文件路径</param>
    /// <param name="targetPath">目标文件路径</param>
    /// <param name="outputPath">输出文件路径</param>
    public void ApplyTemplateMergeFromFile(
        string templatePath, 
        string targetPath, 
        string outputPath,
        bool imageCaptionsByChapter = false,
        bool imageCaptionSpacing = false,
        bool tableCaptionsByChapter = false,
        bool tableCaptionSpacing = false)
    {
        // 1) 读取文档
        var templateDoc = GetDocument(templatePath);
        var sourceDoc = GetDocument(targetPath);

        // 2) 复制模板为工作文档（避免直接改模板）
        Document destDoc;
        using (var ms = new MemoryStream())
        {
            templateDoc.Save(ms, SaveFormat.Docx);
            ms.Position = 0;
            destDoc = new Document(ms);
        }

        // 3) 使用 UseDestinationStyles 导入目标文档所有内容到工作文档
        var importOptions = new ImportFormatOptions
        {
            KeepSourceNumbering = false // 使用模板的编号样式和层级
        };
        var importer = new NodeImporter(sourceDoc, destDoc, ImportFormatMode.UseDestinationStyles, importOptions);
        foreach (Section srcSection in sourceDoc.Sections)
        {
            // 将整段 Section 导入并追加到模板副本中
            var importedSection = (Section)importer.ImportNode(srcSection, true);
            destDoc.AppendChild(importedSection);
        }

        // 4) 样式规范化（标题/正文/题注强制映射到模板的内置样式）
        NormalizeBuiltInStylesToTemplate(destDoc, templateDoc);

        // 5) 补刷格式（图片、表格、题注、页面设置）
        ApplyImageFormatsWithOptions(templateDoc, destDoc, imageCaptionsByChapter, imageCaptionSpacing);   // 图片和题注
        ApplyTableFormatsWithOptions(templateDoc, destDoc, tableCaptionsByChapter, tableCaptionSpacing);   // 表格和题注
        ApplyPageSetup(templateDoc, destDoc);      // 按模板 Section[0] 刷整个文档
        
        // 6) 全局更新所有字段（确保题注编号正确显示）
        destDoc.UpdateFields();
        
        // 7) 设置文档选项，确保显示字段结果而不是代码
        destDoc.FieldOptions.PreProcessCulture = new System.Globalization.CultureInfo("zh-CN");

        // 8) 保存输出
        destDoc.Save(outputPath);
    }
    
    
    /// <summary>
    /// 将导入后的段落按模板的内置样式规范化，并复制模板的段落/字体格式
    /// </summary>
    private void NormalizeBuiltInStylesToTemplate(Document destDoc, Document templateDoc)
    {
        // 先分析模板中正文的缩进层级结构
        var templateIndentLevels = AnalyzeTemplateIndentLevels(templateDoc);
        
        foreach (Paragraph p in destDoc.GetChildNodes(NodeType.Paragraph, true))
        {
            var id = p.ParagraphFormat.StyleIdentifier;

            // 仅处理常见内置样式：标题、正文、题注
            if (id == StyleIdentifier.Heading1 || id == StyleIdentifier.Heading2 ||
                id == StyleIdentifier.Heading3 || id == StyleIdentifier.Heading4 ||
                id == StyleIdentifier.Heading5 || id == StyleIdentifier.Heading6 ||
                id == StyleIdentifier.Heading7 || id == StyleIdentifier.Heading8 ||
                id == StyleIdentifier.Heading9 || id == StyleIdentifier.BodyText ||
                id == StyleIdentifier.Caption)
            {
                // 获取模板的同标识样式
                var tplStyle = templateDoc.Styles[id];
                // 获取目标文档的同标识样式
                var destStyle = destDoc.Styles[id];
                
                if (tplStyle != null && destStyle != null)
                {
                    // 将段落指向目标文档中的样式（而非模板的样式对象）
                    p.ParagraphFormat.StyleIdentifier = id;

                    // 复制模板段落格式到目标文档的样式
                    CopyParagraphFormatProperties(tplStyle.ParagraphFormat, destStyle.ParagraphFormat);
                    
                    // 复制模板字体格式到目标文档的样式
                    CopyFontFormat(tplStyle.Font, destStyle.Font);
                    
                    // 然后将这些格式应用到段落
                    CopyParagraphFormatProperties(tplStyle.ParagraphFormat, p.ParagraphFormat);

                    // 复制模板字体格式到每个 Run（保持内容不变）
                    foreach (Run r in p.Runs)
                    {
                        CopyFontFormat(tplStyle.Font, r.Font);
                    }
                    
                    // 如果是正文段落，根据缩进匹配模板的多级格式
                    if (id == StyleIdentifier.BodyText && templateIndentLevels.Count > 0)
                    {
                        ApplyIndentLevelFormat(p, templateIndentLevels);
                    }
                }
            }

            // 识别图/表题注：发现 SEQ 或 STYLEREF 字段的段落，强制套模板的 Caption 样式
            bool hasCaptionField = p.Range.Fields.Any(f =>
                f.Type == Aspose.Words.Fields.FieldType.FieldSequence ||      // SEQ 字段（图 1, 表 1）
                f.Type == Aspose.Words.Fields.FieldType.FieldRef ||           // REF 字段
                f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);       // STYLEREF 字段（引用章节编号）
            
            if (hasCaptionField)
            {
                var tplCaptionStyle = templateDoc.Styles[StyleIdentifier.Caption];
                var destCaptionStyle = destDoc.Styles[StyleIdentifier.Caption];
                
                if (tplCaptionStyle != null && destCaptionStyle != null)
                {
                    p.ParagraphFormat.StyleIdentifier = StyleIdentifier.Caption;
                    
                    // 复制模板Caption样式到目标文档的Caption样式
                    CopyParagraphFormatProperties(tplCaptionStyle.ParagraphFormat, destCaptionStyle.ParagraphFormat);
                    CopyFontFormat(tplCaptionStyle.Font, destCaptionStyle.Font);
                    
                    // 应用到段落
                    CopyParagraphFormatProperties(tplCaptionStyle.ParagraphFormat, p.ParagraphFormat);
                    foreach (Run r in p.Runs)
                        CopyFontFormat(tplCaptionStyle.Font, r.Font);
                    
                    // 确保题注字段正确更新（包括章节引用）
                    UpdateCaptionFields(p);
                }
            }
        }
        
        // 全局更新所有字段，确保章节编号正确显示
        destDoc.UpdateFields();
    }
    
    /// <summary>
    /// 分析模板文档中正文的缩进层级结构（用于识别 a/b/c 或 1/2/3 等分层格式）
    /// </summary>
    private Dictionary<double, Paragraph> AnalyzeTemplateIndentLevels(Document templateDoc)
    {
        var result = new Dictionary<double, Paragraph>();
        
        try
        {
            var allParagraphs = templateDoc.GetChildNodes(NodeType.Paragraph, true)
                .OfType<Paragraph>()
                .Where(p => p.ParagraphFormat.StyleIdentifier == StyleIdentifier.BodyText)
                .Where(p => !string.IsNullOrWhiteSpace(p.GetText()))
                .ToList();
            
            if (!allParagraphs.Any()) return result;
            
            // 收集所有不同的左缩进值
            var indentGroups = new Dictionary<double, List<Paragraph>>();
            
            foreach (var para in allParagraphs)
            {
                // 使用左缩进作为层级标识
                var leftIndent = Math.Round(para.ParagraphFormat.LeftIndent, 1);
                
                if (!indentGroups.ContainsKey(leftIndent))
                {
                    indentGroups[leftIndent] = new List<Paragraph>();
                }
                indentGroups[leftIndent].Add(para);
            }
            
            // 如果有多个缩进层级，说明存在分层结构
            if (indentGroups.Count > 1)
            {
                // 按缩进值排序，每个缩进层级取一个代表段落
                foreach (var group in indentGroups.OrderBy(g => g.Key))
                {
                    result[group.Key] = group.Value.First();
                }
            }
        }
        catch
        {
            // 忽略分析失败
        }
        
        return result;
    }
    
    /// <summary>
    /// 根据段落的缩进值，应用模板对应层级的格式
    /// </summary>
    private void ApplyIndentLevelFormat(Paragraph targetPara, Dictionary<double, Paragraph> templateIndentLevels)
    {
        try
        {
            var targetIndent = Math.Round(targetPara.ParagraphFormat.LeftIndent, 1);
            
            // 查找最接近的模板缩进层级
            var closestTemplate = templateIndentLevels
                .OrderBy(kvp => Math.Abs(kvp.Key - targetIndent))
                .FirstOrDefault();
            
            if (closestTemplate.Value != null)
            {
                var templatePara = closestTemplate.Value;
                
                // 复制该层级的完整格式（包括编号格式、列表样式等）
                CopyParagraphFormatProperties(templatePara.ParagraphFormat, targetPara.ParagraphFormat);
                
                // 复制字体格式
                if (templatePara.Runs.Count > 0 && targetPara.Runs.Count > 0)
                {
                    foreach (Run targetRun in targetPara.Runs)
                    {
                        CopyFontFormat(templatePara.Runs[0].Font, targetRun.Font);
                    }
                }
                
                // 关键：如果模板段落有列表格式，应用同样的列表
                if (templatePara.ListFormat.IsListItem)
                {
                    var templateList = templatePara.ListFormat.List;
                    var templateLevel = templatePara.ListFormat.ListLevelNumber;
                    
                    if (templateList != null)
                    {
                        // 在目标文档中查找或创建对应的列表
                        var destDoc = (Document)targetPara.Document;
                        Aspose.Words.Lists.List matchingList = null;
                        
                        // 尝试找到相同编号格式的列表
                        foreach (var list in destDoc.Lists)
                        {
                            if (list.ListLevels.Count > templateLevel)
                            {
                                var destLevel = list.ListLevels[templateLevel];
                                var srcLevel = templateList.ListLevels[templateLevel];
                                
                                // 比较编号格式是否匹配
                                if (destLevel.NumberFormat == srcLevel.NumberFormat && 
                                    destLevel.NumberStyle == srcLevel.NumberStyle)
                                {
                                    matchingList = list;
                                    break;
                                }
                            }
                        }
                        
                        // 如果没找到，创建新列表
                        if (matchingList == null)
                        {
                            matchingList = destDoc.Lists.Add(Aspose.Words.Lists.ListTemplate.NumberDefault);
                            
                            // 复制列表所有级别的格式
                            for (int i = 0; i < Math.Min(templateList.ListLevels.Count, 9); i++)
                            {
                                var srcLevel = templateList.ListLevels[i];
                                var destLevel = matchingList.ListLevels[i];
                                
                                destLevel.NumberFormat = srcLevel.NumberFormat;
                                destLevel.NumberStyle = srcLevel.NumberStyle;
                                destLevel.NumberPosition = srcLevel.NumberPosition;
                                destLevel.TextPosition = srcLevel.TextPosition;
                                destLevel.TabPosition = srcLevel.TabPosition;
                                destLevel.Alignment = srcLevel.Alignment;
                                destLevel.StartAt = srcLevel.StartAt;
                                destLevel.RestartAfterLevel = srcLevel.RestartAfterLevel;
                                
                                CopyFontFormat(srcLevel.Font, destLevel.Font);
                            }
                        }
                        
                        // 应用列表到目标段落
                        targetPara.ListFormat.List = matchingList;
                        targetPara.ListFormat.ListLevelNumber = templateLevel;
                    }
                }
                else
                {
                    // 如果模板没有列表格式，确保目标也没有
                    if (targetPara.ListFormat.IsListItem)
                    {
                        targetPara.ListFormat.RemoveNumbers();
                    }
                }
            }
        }
        catch
        {
            // 忽略应用失败
        }
    }
    
    /// <summary>
    /// 更新题注字段，确保 STYLEREF 等字段正确关联章节
    /// </summary>
    private void UpdateCaptionFields(Paragraph captionPara)
    {
        try
        {   
            bool hasStyleRef = false;
            
            // 更新段落内的所有字段
            foreach (var field in captionPara.Range.Fields)
            {
                if (field.Type == Aspose.Words.Fields.FieldType.FieldStyleRef)
                {
                    hasStyleRef = true;
                    // 直接更新 STYLEREF 字段，让它自动查找最近的对应标题
                    field.Update();
                }
                else if (field.Type == Aspose.Words.Fields.FieldType.FieldSequence)
                {
                    // SEQ 字段用于自动编号
                    field.Update();
                }
                else if (field.Type == Aspose.Words.Fields.FieldType.FieldRef)
                {
                    // REF 字段引用书签
                    field.Update();
                }
            }
            
            // 如果没有 STYLEREF 字段但应该有（模板有），则从模板学习并添加
            if (!hasStyleRef)
            {
                // 尝试从文档中其他题注学习 STYLEREF 格式
                // 这里简单处理：如果没有就不添加，让用户的题注保持原样
            }
        }
        catch
        {
            // 忽略更新失败
        }
    }
    
    /// <summary>
    /// 确保题注中包含章节编号（STYLEREF字段）
    /// 策略：删除原有的手动编号，使用字段重新构建标准题注
    /// </summary>
    /// <param name="captionPara">目标题注段落</param>
    /// <param name="templateCaption">模板题注段落</param>
    /// <param name="captionType">题注类型（图或表）</param>
    private void EnsureCaptionHasChapterNumber(Paragraph captionPara, Paragraph templateCaption, string captionType)
    {
        try
        {
            // 检查模板题注是否有STYLEREF字段
            var templateStyleRefField = templateCaption.Range.Fields.FirstOrDefault(f => f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);
            if (templateStyleRefField == null)
            {
                return; // 模板也没有STYLEREF，不需要添加
            }
            
            // 检查目标题注是否已有STYLEREF字段
            bool hasStyleRef = captionPara.Range.Fields.Any(f => f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);
            if (hasStyleRef)
            {
                return; // 已经有了，不需要重建
            }
            
            // 提取原有的描述文字（去掉"表12-1"或"图3-2"这种手动编号）
            var captionText = captionPara.GetText().Trim();
            
            // 正则匹配：图/表 + 可选的数字编号（如12-1、3-2等） + 描述文字
            // 匹配模式：表14-1 项目总经费概算 -> 提取"项目总经费概算"
            var descriptionMatch = System.Text.RegularExpressions.Regex.Match(captionText, 
                $@"{captionType}\s*\d+[-—]?\d*\s+(.*)");
            
            string description = "";
            if (descriptionMatch.Success && descriptionMatch.Groups.Count > 1)
            {
                description = descriptionMatch.Groups[1].Value.Trim();
            }
            else
            {
                // 如果没有匹配到，尝试简单模式：图/表 + 描述
                var simpleMatch = System.Text.RegularExpressions.Regex.Match(captionText, 
                    $@"{captionType}\s+(.*)");
                if (simpleMatch.Success && simpleMatch.Groups.Count > 1)
                {
                    var temp = simpleMatch.Groups[1].Value.Trim();
                    // 去掉开头的数字编号（如果有）
                    temp = System.Text.RegularExpressions.Regex.Replace(temp, @"^\d+[-—]?\d*\s*", "");
                    description = temp;
                }
            }
            
            var doc = (Document)captionPara.Document;
            var builder = new DocumentBuilder(doc);
            var styleRefCode = templateStyleRefField.GetFieldCode();
            
            // 移动到题注段落的开始
            builder.MoveTo(captionPara);
            
            // 清空段落内容，重新构建带有STYLEREF的题注
            captionPara.RemoveAllChildren();
            builder.MoveTo(captionPara);
            
            // 插入题注类型文字
            builder.Write(captionType);
            
            // 插入STYLEREF字段（引用章节编号）
            var fieldStart = builder.InsertField(styleRefCode);
            fieldStart.Update();
            
            // 插入连字符
            builder.Write("-");
            
            // 插入SEQ字段（自动编号）
            var seqField = builder.InsertField($"SEQ {captionType} \\* ARABIC \\s 1");
            seqField.Update();
            
            // 插入空格和描述
            if (!string.IsNullOrEmpty(description))
            {
                builder.Write(" " + description);
            }
        }
        catch (Exception ex)
        {
            // 如果添加失败，记录错误但保持原样
            System.Diagnostics.Debug.WriteLine($"添加章节编号失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 添加简单题注（不包含STYLEREF，只有SEQ字段）
    /// </summary>
    private void AddSimpleCaption(Paragraph captionPara, string captionType)
    {
        try
        {
            // 检查是否已有SEQ字段
            bool hasSeqField = captionPara.Range.Fields.Any(f => f.Type == Aspose.Words.Fields.FieldType.FieldSequence);
            if (hasSeqField)
            {
                return; // 已经有题注了
            }
            
            var doc = (Document)captionPara.Document;
            var builder = new DocumentBuilder(doc);
            
            // 移动到题注段落的开始
            builder.MoveTo(captionPara);
            
            // 清空段落内容
            captionPara.RemoveAllChildren();
            builder.MoveTo(captionPara);
            
            // 插入题注类型文字
            builder.Write(captionType);
            
            // 插入SEQ字段（自动编号）
            var seqField = builder.InsertField($"SEQ {captionType} \\* ARABIC");
            seqField.Update();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"添加简单题注失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 读取Word文档的所有格式信息（按层级结构）
    /// </summary>
    /// <param name="filePath">Word文档路径</param>
    /// <returns>格式信息的详细描述</returns>
    public string ReadAllFormats(string filePath)
    {
        var doc = GetDocument(filePath);
        var sb = new StringBuilder();

        sb.AppendLine("══════════════════════════════");
        sb.AppendLine($"Word文档格式分析报告: {Path.GetFileName(filePath)}");
        sb.AppendLine("══════════════════════════════");
        sb.AppendLine();

        // Ⅰ. 文档级别格式
        sb.AppendLine("Ⅰ. 【文档级别格式】");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        if (doc.Sections.Count > 0)
        {
            var section = doc.Sections[0];
            var setup = section.PageSetup;
            sb.AppendLine($"  ● 纸张大小: {setup.PaperSize}");
            sb.AppendLine($"  ● 页面尺寸: {setup.PageWidth:F2} x {setup.PageHeight:F2} 磅");
            sb.AppendLine($"  ● 方向: {setup.Orientation}");
            sb.AppendLine($"  ● 页边距: 上={setup.TopMargin:F2}, 下={setup.BottomMargin:F2}, 左={setup.LeftMargin:F2}, 右={setup.RightMargin:F2} 磅");
            sb.AppendLine($"  ● 页眉页脚距离: 页眉={setup.HeaderDistance:F2}, 页脚={setup.FooterDistance:F2} 磅");
        }
        sb.AppendLine();

        // Ⅱ. 标题级别格式（按大纲级别分类）
        sb.AppendLine("Ⅱ. 【标题级别格式】");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        var allParagraphs = doc.GetChildNodes(NodeType.Paragraph, true)
            .OfType<Paragraph>()
            .Where(p => !string.IsNullOrWhiteSpace(p.GetText()))
            .ToList();

        // 按大纲级别分组
        var headingGroups = allParagraphs
            .Where(p => p.ParagraphFormat.OutlineLevel >= OutlineLevel.Level1 && 
                       p.ParagraphFormat.OutlineLevel <= OutlineLevel.Level9)
            .GroupBy(p => p.ParagraphFormat.OutlineLevel)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var group in headingGroups)
        {
            var level = group.Key;
            var levelNum = (int)level + 1; // OutlineLevel.Level1 = 0
            var sample = group.First();
            var text = sample.GetText().Trim();
            if (text.Length > 30) text = text.Substring(0, 30) + "...";

            sb.AppendLine($"\n  {levelNum}级标题 (大纲级别: {level})");
            sb.AppendLine($"  ├─ 示例文字: \"{text}\"");
            sb.AppendLine($"  ├─ 段落格式:");
            sb.AppendLine($"  │   ├─ 对齐方式: {sample.ParagraphFormat.Alignment}");
            sb.AppendLine($"  │   ├─ 缩进: 左={sample.ParagraphFormat.LeftIndent:F2}pt, 首行={sample.ParagraphFormat.FirstLineIndent:F2}pt");
            sb.AppendLine($"  │   ├─ 行距: {sample.ParagraphFormat.LineSpacing:F2}pt ({sample.ParagraphFormat.LineSpacingRule})");
            sb.AppendLine($"  │   └─ 段间距: 段前={sample.ParagraphFormat.SpaceBefore:F2}pt, 段后={sample.ParagraphFormat.SpaceAfter:F2}pt");
            
            if (sample.Runs.Count > 0)
            {
                var run = sample.Runs[0];
                sb.AppendLine($"  └─ 字体格式:");
                sb.AppendLine($"      ├─ 字体: {run.Font.Name}, 大小: {run.Font.Size}pt");
                sb.AppendLine($"      ├─ 样式: 粗体={run.Font.Bold}, 斜体={run.Font.Italic}, 下划线={run.Font.Underline}");
                sb.AppendLine($"      └─ 颜色: {run.Font.Color}");
            }
        }
        sb.AppendLine();

        // Ⅲ. 正文段落格式
        sb.AppendLine("Ⅲ. 【正文段落格式】");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        // 找到标题后的第一个正文段落
        var bodyParagraphs = new List<Paragraph>();
        for (int i = 0; i < allParagraphs.Count - 1 && bodyParagraphs.Count < 3; i++)
        {
            var current = allParagraphs[i];
            var next = allParagraphs[i + 1];
            
            // 如果当前段是标题，且下一段是正文
            if (current.ParagraphFormat.OutlineLevel >= OutlineLevel.Level1 && 
                current.ParagraphFormat.OutlineLevel <= OutlineLevel.Level9 &&
                next.ParagraphFormat.OutlineLevel == OutlineLevel.BodyText)
            {
                // 确保下一段不是空段落
                var text = next.GetText().Trim();
                if (!string.IsNullOrWhiteSpace(text) && text.Length > 10)
                {
                    bodyParagraphs.Add(next);
                }
            }
        }

        // 如果没找到，就用普通的BodyText段落
        if (!bodyParagraphs.Any())
        {
            bodyParagraphs = allParagraphs
                .Where(p => p.ParagraphFormat.OutlineLevel == OutlineLevel.BodyText)
                .Where(p => !string.IsNullOrWhiteSpace(p.GetText()) && p.GetText().Trim().Length > 10)
                .Take(3)
                .ToList();
        }

        // 分析正文段落的缩进层级
        var bodyIndentGroups = AnalyzeBodyIndentLevels(allParagraphs);
        
        if (bodyIndentGroups.Any())
        {
            sb.AppendLine();
            sb.AppendLine("  检测到多级缩进结构：");
            sb.AppendLine();
            
            foreach (var group in bodyIndentGroups.OrderBy(g => g.Key))
            {
                var indentLevel = group.Key;
                var sample = group.Value;
                var text = sample.GetText().Trim();
                if (text.Length > 35) text = text.Substring(0, 35) + "...";
                
                // 根据缩进层级使用不同的标识符
                string levelLabel = GetIndentLevelLabel(indentLevel);
                string indentSymbol = GetIndentSymbol(indentLevel);
                
                sb.AppendLine($"  {indentSymbol} {levelLabel}级缩进 (左缩进={sample.ParagraphFormat.LeftIndent:F2}pt, 首行={sample.ParagraphFormat.FirstLineIndent:F2}pt)");
                sb.AppendLine($"  {GetIndentPrefix(indentLevel)}├─ 示例: \"{text}\"");
                sb.AppendLine($"  {GetIndentPrefix(indentLevel)}├─ 对齐: {sample.ParagraphFormat.Alignment}");
                sb.AppendLine($"  {GetIndentPrefix(indentLevel)}├─ 行距: {sample.ParagraphFormat.LineSpacing:F2}pt ({sample.ParagraphFormat.LineSpacingRule})");
                
                if (sample.Runs.Count > 0)
                {
                    var run = sample.Runs[0];
                    sb.AppendLine($"  {GetIndentPrefix(indentLevel)}└─ 字体: {run.Font.Name}, {run.Font.Size}pt");
                }
                sb.AppendLine();
            }
        }
        else
        {
            // 如果没有多级缩进，显示基本正文格式
            for (int i = 0; i < bodyParagraphs.Count; i++)
            {
                var para = bodyParagraphs[i];
                var text = para.GetText().Trim();
                if (text.Length > 40) text = text.Substring(0, 40) + "...";

                sb.AppendLine($"\n  正文示例 {i + 1}: \"{text}\"");
                sb.AppendLine($"  ├─ 段落格式:");
                sb.AppendLine($"  │   ├─ 对齐方式: {para.ParagraphFormat.Alignment}");
                sb.AppendLine($"  │   ├─ 缩进: 左={para.ParagraphFormat.LeftIndent:F2}pt, 首行={para.ParagraphFormat.FirstLineIndent:F2}pt");
                sb.AppendLine($"  │   ├─ 行距: {para.ParagraphFormat.LineSpacing:F2}pt ({para.ParagraphFormat.LineSpacingRule})");
                sb.AppendLine($"  │   └─ 段间距: 段前={para.ParagraphFormat.SpaceBefore:F2}pt, 段后={para.ParagraphFormat.SpaceAfter:F2}pt");
                
                if (para.Runs.Count > 0)
                {
                    var run = para.Runs[0];
                    sb.AppendLine($"  └─ 字体格式:");
                    sb.AppendLine($"      ├─ 字体: {run.Font.Name}, 大小: {run.Font.Size}pt");
                    sb.AppendLine($"      ├─ 样式: 粗体={run.Font.Bold}, 斜体={run.Font.Italic}, 下划线={run.Font.Underline}");
                    sb.AppendLine($"      └─ 颜色: {run.Font.Color}");
                }
            }
        }
        sb.AppendLine();

        // Ⅳ. 表格格式
        var tables = GetTables(doc);
        if (tables.Any())
        {
            sb.AppendLine("Ⅳ. 【表格格式】");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine($"  总计: {tables.Count} 个表格\n");
            
            for (int i = 0; i < Math.Min(tables.Count, 2); i++)
            {
                var table = tables[i];
                sb.AppendLine($"  表格 {i + 1}:");
                
                // 检查表格上方的标题
                var captionInfo = AnalyzeTableCaption(table, doc);
                if (!string.IsNullOrEmpty(captionInfo.AboveText))
                {
                    sb.AppendLine($"  ├─ 表格上方文字（表题）:");
                    sb.AppendLine($"  │   ├─ 内容: \"{captionInfo.AboveText}\"");
                    sb.AppendLine($"  │   ├─ 对齐: {captionInfo.AboveAlignment}");
                    sb.AppendLine($"  │   ├─ 字体: {captionInfo.AboveFont}");
                    sb.AppendLine($"  │   └─ 段间距: 段前={captionInfo.AboveSpaceBefore:F2}pt, 段后={captionInfo.AboveSpaceAfter:F2}pt");
                }
                
                sb.AppendLine($"  ├─ 表格属性:");
                sb.AppendLine($"  │   ├─ 对齐方式: {table.Alignment}");
                sb.AppendLine($"  │   ├─ 宽度设置: {table.PreferredWidth}");
                sb.AppendLine($"  │   ├─ 左缩进: {table.LeftIndent:F2}pt");
                sb.AppendLine($"  │   └─ 行数: {table.Rows.Count}");
                
                if (table.FirstRow != null)
                {
                    sb.AppendLine($"  ├─ 边框格式:");
                    sb.AppendLine($"  │   ├─ 线型: {table.FirstRow.RowFormat.Borders.LineStyle}");
                    sb.AppendLine($"  │   ├─ 粗细: {table.FirstRow.RowFormat.Borders.LineWidth:F2}pt");
                    sb.AppendLine($"  │   └─ 颜色: {table.FirstRow.RowFormat.Borders.Color}");
                    
                    if (table.FirstRow.Cells.Count > 0)
                    {
                        var cell = table.FirstRow.Cells[0];
                        sb.AppendLine($"  ├─ 单元格格式:");
                        sb.AppendLine($"  │   ├─ 垂直对齐: {cell.CellFormat.VerticalAlignment}");
                        sb.AppendLine($"  │   └─ 背景色: {cell.CellFormat.Shading.BackgroundPatternColor}");
                    }
                }
                
                // 检查表格下方的说明
                if (!string.IsNullOrEmpty(captionInfo.BelowText))
                {
                    sb.AppendLine($"  └─ 表格下方文字（表注）:");
                    sb.AppendLine($"      ├─ 内容: \"{captionInfo.BelowText}\"");
                    sb.AppendLine($"      ├─ 对齐: {captionInfo.BelowAlignment}");
                    sb.AppendLine($"      ├─ 字体: {captionInfo.BelowFont}");
                    sb.AppendLine($"      └─ 段间距: 段前={captionInfo.BelowSpaceBefore:F2}pt, 段后={captionInfo.BelowSpaceAfter:F2}pt");
                }
                else if (string.IsNullOrEmpty(captionInfo.AboveText))
                {
                    sb.AppendLine($"  └─ 表格上下均无标题或注释");
                }
                
                sb.AppendLine();
            }
        }

        // Ⅴ. 图片格式
        var shapes = doc.GetChildNodes(NodeType.Shape, true)
            .OfType<Shape>()
            .Where(s => s.HasImage)
            .ToList();
        
        if (shapes.Any())
        {
            sb.AppendLine("Ⅴ. 【图片格式】");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine($"  总计: {shapes.Count} 张图片\n");
            
            for (int i = 0; i < Math.Min(shapes.Count, 3); i++)
            {
                var shape = shapes[i];
                sb.AppendLine($"  图片 {i + 1}:");
                
                // 图片尺寸
                sb.AppendLine($"  ├─ 尺寸:");
                sb.AppendLine($"  │   ├─ 宽度: {shape.Width:F2}pt");
                sb.AppendLine($"  │   ├─ 高度: {shape.Height:F2}pt");
                sb.AppendLine($"  │   └─ 锁定纵横比: {shape.AspectRatioLocked}");
                
                // 图片位置和对齐
                sb.AppendLine($"  ├─ 位置和对齐:");
                sb.AppendLine($"  │   ├─ 水平对齐: {shape.HorizontalAlignment}");
                sb.AppendLine($"  │   ├─ 垂直对齐: {shape.VerticalAlignment}");
                sb.AppendLine($"  │   ├─ 水平基准: {shape.RelativeHorizontalPosition}");
                sb.AppendLine($"  │   └─ 垂直基准: {shape.RelativeVerticalPosition}");
                
                if (shape.HorizontalAlignment == Aspose.Words.Drawing.HorizontalAlignment.None)
                {
                    sb.AppendLine($"  │       └─ 水平绝对位置: {shape.Left:F2}pt");
                }
                if (shape.VerticalAlignment == Aspose.Words.Drawing.VerticalAlignment.None)
                {
                    sb.AppendLine($"  │       └─ 垂直绝对位置: {shape.Top:F2}pt");
                }
                
                // 文字环绕
                sb.AppendLine($"  ├─ 文字环绕:");
                sb.AppendLine($"  │   ├─ 环绕类型: {shape.WrapType}");
                sb.AppendLine($"  │   ├─ 环绕侧: {shape.WrapSide}");
                sb.AppendLine($"  │   └─ 环绕距离: 上={shape.DistanceTop:F2}, 下={shape.DistanceBottom:F2}, 左={shape.DistanceLeft:F2}, 右={shape.DistanceRight:F2} pt");
                
                // 边框
                if (shape.Stroke != null && shape.Stroke.Visible)
                {
                    sb.AppendLine($"  ├─ 边框:");
                    sb.AppendLine($"  │   ├─ 颜色: {shape.Stroke.Color}");
                    sb.AppendLine($"  │   ├─ 粗细: {shape.Stroke.Weight:F2}pt");
                    sb.AppendLine($"  │   └─ 样式: {shape.Stroke.LineStyle}");
                }
                else
                {
                    sb.AppendLine($"  ├─ 边框: 无");
                }
                
                // 其他属性
                sb.AppendLine($"  ├─ 其他属性:");
                sb.AppendLine($"  │   ├─ 允许重叠: {shape.AllowOverlap}");
                sb.AppendLine($"  │   └─ 置于文字下方: {shape.BehindText}");
                
                // 检查图注（Caption）位置
                sb.AppendLine($"  └─ 图注位置: {AnalyzeCaptionPosition(shape, doc)}");
                
                sb.AppendLine();
            }
        }

        sb.AppendLine("══════════════════════════════");
        sb.AppendLine("格式分析完成");
        sb.AppendLine("══════════════════════════════");
        
        return sb.ToString();
    }

    /// <summary>
    /// 分析图注的位置（上方或下方）
    /// </summary>
    private string AnalyzeCaptionPosition(Shape shape, Document doc)
    {
        try
        {
            var shapePara = shape.ParentNode as Paragraph;
            if (shapePara == null) return "无图注";

            var shapeIndex = shapePara.ParentNode.ChildNodes.IndexOf(shapePara);
            var parentNode = shapePara.ParentNode;

            // 检查上一个段落
            if (shapeIndex > 0)
            {
                var prevNode = parentNode.ChildNodes[shapeIndex - 1];
                if (prevNode is Paragraph prevPara)
                {
                    // 检查是否包含题注字段
                    var captionText = ExtractCaptionText(prevPara);
                    if (!string.IsNullOrEmpty(captionText))
                    {
                        return $"上方 - 标题: \"{captionText}\" (对齐: {prevPara.ParagraphFormat.Alignment}, 字体: {GetParagraphFont(prevPara)})";
                    }
                }
            }

            // 检查下一个段落
            if (shapeIndex < parentNode.ChildNodes.Count - 1)
            {
                var nextNode = parentNode.ChildNodes[shapeIndex + 1];
                if (nextNode is Paragraph nextPara)
                {
                    // 检查是否包含题注字段
                    var captionText = ExtractCaptionText(nextPara);
                    if (!string.IsNullOrEmpty(captionText))
                    {
                        return $"下方 - 标题: \"{captionText}\" (对齐: {nextPara.ParagraphFormat.Alignment}, 字体: {GetParagraphFont(nextPara)})";
                    }
                }
            }

            return "无图注";
        }
        catch
        {
            return "无法分析";
        }
    }

    /// <summary>
    /// 提取段落中的题注文字（包括 STYLEREF 字段用于章节编号）
    /// </summary>
    private string ExtractCaptionText(Paragraph para)
    {
        if (para == null) return string.Empty;
        
        var text = para.GetText().Trim();
        
        // 检查是否有题注相关字段（SEQ、STYLEREF、REF）
        var hasCaptionField = para.Range.Fields.Any(f => 
            f.Type == Aspose.Words.Fields.FieldType.FieldSequence ||      // SEQ 字段（图 1, 图 2）
            f.Type == Aspose.Words.Fields.FieldType.FieldRef ||           // REF 字段
            f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);       // STYLEREF 字段（引用章节编号）
        
        // 如果有题注字段，或者文字以常见图注开头，就认为是题注
        if (hasCaptionField || IsCaptionText(text))
        {
            return text.Length > 60 ? text.Substring(0, 60) + "..." : text;
        }
        
        return string.Empty;
    }

    /// <summary>
    /// 获取段落的字体信息
    /// </summary>
    private string GetParagraphFont(Paragraph para)
    {
        if (para.Runs.Count > 0)
        {
            var run = para.Runs[0];
            var fontInfo = $"{run.Font.Name}, {run.Font.Size}pt";
            if (run.Font.Bold) fontInfo += ", 粗体";
            if (run.Font.Italic) fontInfo += ", 斜体";
            return fontInfo;
        }
        return "未知";
    }

    /// <summary>
    /// 判断是否为图注文字
    /// </summary>
    private bool IsCaptionText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        
        // 常见的图注开头
        return text.StartsWith("图") || 
               text.StartsWith("Figure") || 
               text.StartsWith("Fig.") ||
               text.StartsWith("图表") ||
               text.Contains("图") && text.Length < 100; // 简短且包含"图"字
    }

    /// <summary>
    /// 分析正文段落的缩进层级
    /// </summary>
    private Dictionary<int, Paragraph> AnalyzeBodyIndentLevels(List<Paragraph> allParagraphs)
    {
        var indentGroups = new Dictionary<double, List<Paragraph>>();
        
        // 收集所有正文段落的缩进值
        var bodyParas = allParagraphs
            .Where(p => p.ParagraphFormat.OutlineLevel == OutlineLevel.BodyText)
            .Where(p => !string.IsNullOrWhiteSpace(p.GetText()) && p.GetText().Trim().Length > 10)
            .ToList();

        foreach (var para in bodyParas)
        {
            var leftIndent = Math.Round(para.ParagraphFormat.LeftIndent, 1);
            
            if (!indentGroups.ContainsKey(leftIndent))
            {
                indentGroups[leftIndent] = new List<Paragraph>();
            }
            indentGroups[leftIndent].Add(para);
        }

        // 如果只有一种缩进，说明没有多级结构
        if (indentGroups.Count <= 1)
        {
            return new Dictionary<int, Paragraph>();
        }

        // 按缩进值排序，分配层级编号
        var sortedIndents = indentGroups.OrderBy(g => g.Key).ToList();
        var result = new Dictionary<int, Paragraph>();
        
        for (int i = 0; i < Math.Min(sortedIndents.Count, 5); i++) // 最多5个层级
        {
            result[i] = sortedIndents[i].Value.First();
        }

        return result;
    }

    /// <summary>
    /// 获取缩进层级的标签
    /// </summary>
    private string GetIndentLevelLabel(int level)
    {
        return level switch
        {
            0 => "基础",
            1 => "一",
            2 => "二",
            3 => "三",
            4 => "四",
            _ => level.ToString()
        };
    }

    /// <summary>
    /// 获取缩进层级的符号
    /// </summary>
    private string GetIndentSymbol(int level)
    {
        return level switch
        {
            0 => "●",      // 圆点
            1 => "a.",     // 字母
            2 => "1.",     // 数字
            3 => "i.",     // 罗马数字
            4 => "-",      // 横线
            _ => "•"       // 默认项目符号
        };
    }

    /// <summary>
    /// 获取缩进层级的前缀空格
    /// </summary>
    private string GetIndentPrefix(int level)
    {
        return new string(' ', level * 2);
    }

    /// <summary>
    /// 表格标注信息
    /// </summary>
    private class TableCaptionInfo
    {
        public string AboveText { get; set; } = string.Empty;
        public string AboveAlignment { get; set; } = string.Empty;
        public string AboveFont { get; set; } = string.Empty;
        public double AboveSpaceBefore { get; set; }
        public double AboveSpaceAfter { get; set; }
        
        public string BelowText { get; set; } = string.Empty;
        public string BelowAlignment { get; set; } = string.Empty;
        public string BelowFont { get; set; } = string.Empty;
        public double BelowSpaceBefore { get; set; }
        public double BelowSpaceAfter { get; set; }
    }

    /// <summary>
    /// 分析表格的标题和注释（上下方文字）
    /// </summary>
    private TableCaptionInfo AnalyzeTableCaption(Table table, Document doc)
    {
        var info = new TableCaptionInfo();
        
        try
        {
            var tableNode = table as Node;
            var parentNode = tableNode.ParentNode;
            if (parentNode == null) return info;

            var tableIndex = parentNode.ChildNodes.IndexOf(tableNode);

            // 检查表格上方的段落
            if (tableIndex > 0)
            {
                var prevNode = parentNode.ChildNodes[tableIndex - 1];
                if (prevNode is Paragraph prevPara)
                {
                    var captionText = ExtractTableCaptionText(prevPara);
                    if (!string.IsNullOrEmpty(captionText))
                    {
                        info.AboveText = captionText;
                        info.AboveAlignment = prevPara.ParagraphFormat.Alignment.ToString();
                        info.AboveSpaceBefore = prevPara.ParagraphFormat.SpaceBefore;
                        info.AboveSpaceAfter = prevPara.ParagraphFormat.SpaceAfter;
                        info.AboveFont = GetParagraphFont(prevPara);
                    }
                }
            }

            // 检查表格下方的段落
            if (tableIndex < parentNode.ChildNodes.Count - 1)
            {
                var nextNode = parentNode.ChildNodes[tableIndex + 1];
                if (nextNode is Paragraph nextPara)
                {
                    var text = nextPara.GetText().Trim();
                    // 下方文字通常是说明性文字，不一定有SEQ字段
                    if (!string.IsNullOrWhiteSpace(text) && text.Length > 5 && text.Length < 200)
                    {
                        info.BelowText = text.Length > 50 ? text.Substring(0, 50) + "..." : text;
                        info.BelowAlignment = nextPara.ParagraphFormat.Alignment.ToString();
                        info.BelowSpaceBefore = nextPara.ParagraphFormat.SpaceBefore;
                        info.BelowSpaceAfter = nextPara.ParagraphFormat.SpaceAfter;
                        info.BelowFont = GetParagraphFont(nextPara);
                    }
                }
            }

            return info;
        }
        catch
        {
            return info;
        }
    }

    /// <summary>
    /// 提取表格题注文字（包括 STYLEREF 字段用于章节编号）
    /// </summary>
    private string ExtractTableCaptionText(Paragraph para)
    {
        if (para == null) return string.Empty;
        
        var text = para.GetText().Trim();
        
        // 检查是否有题注相关字段（SEQ、STYLEREF、REF）
        var hasCaptionField = para.Range.Fields.Any(f => 
            f.Type == Aspose.Words.Fields.FieldType.FieldSequence ||      // SEQ 字段（表 1, 表 2）
            f.Type == Aspose.Words.Fields.FieldType.FieldRef ||           // REF 字段
            f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);       // STYLEREF 字段（引用章节编号）
        
        // 如果有题注字段，或者文字以常见表注开头，就认为是题注
        if (hasCaptionField || IsTableCaptionText(text))
        {
            return text.Length > 50 ? text.Substring(0, 50) + "..." : text;
        }
        
        return string.Empty;
    }

    /// <summary>
    /// 判断是否为表格标题文字
    /// </summary>
    private bool IsTableCaptionText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        
        // 常见的表格标题开头
        return text.StartsWith("表") || 
               text.StartsWith("Table") || 
               text.StartsWith("附表") ||
               (text.Contains("表") && text.Length < 100); // 简短且包含"表"字
    }
}

/// <summary>
/// Word服务相关的扩展方法类
/// 提供依赖注入容器的扩展方法，用于注册和配置WordService服务
/// 支持.NET Core依赖注入框架，简化服务注册过程
/// </summary>
public static partial class Extension
{

    /// <summary>
    /// 在依赖注入容器中注册WordService服务
    /// 将WordService注册为单例服务，确保在整个应用程序生命周期中只有一个实例
    /// 单例模式有助于优化Aspose.Words许可证的初始化性能和内存使用
    /// </summary>
    /// <param name="services">服务集合容器</param>
    /// <returns>返回服务集合，支持链式调用</returns>
    /// <example>
    /// 使用示例：
    /// <code>
    /// services.AddWordService();
    /// </code>
    /// </example>
    public static IServiceCollection AddWordService(this IServiceCollection services) => services.AddSingleton(new WordService());

}
