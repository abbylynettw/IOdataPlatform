using System.IO;
using System.Text;
using System.Windows;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui.Controls;
using Aspose.Words;
using Aspose.Words.Drawing;
using Aspose.Words.Tables;
using AsposeStyle = Aspose.Words.Style;
using WpfMessageBoxResult = System.Windows.MessageBoxResult;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// Word格式刷功能ViewModel
/// 提供从模板Word文档复制格式到目标Word文档的功能
/// </summary>
public partial class WordFormatBrushViewModel : ObservableObject, INavigationAware
{
    private readonly IPickerService picker;
    private readonly IMessageService message;
    private readonly WordService word;
    private readonly GlobalModel model;

    [ObservableProperty]
    private string templateFilePath = string.Empty;

    [ObservableProperty]
    private string templateFileName = string.Empty;

    [ObservableProperty]
    private string templateInfo = "尚未选择模板文件\n\n请选择一个Word文档作为格式源";

    [ObservableProperty]
    private string targetFilePath = string.Empty;

    [ObservableProperty]
    private string targetFileName = string.Empty;

    [ObservableProperty]
    private string targetInfo = "尚未选择目标文件\n\n请选择需要应用格式的Word文档";

    [ObservableProperty]
    private string logText = "欢迎使用Word格式刷功能\n\n操作步骤：\n1. 选择模板文件（格式源）\n2. 读取并查看模板格式\n3. 选择目标文件（需要套用格式的文档）\n4. 勾选需要应用的选项\n5. 点击套用模板按钮\n\n";

    // ===== 基础样式选项 =====
    [ObservableProperty]
    private bool applyBaseStyles = true; // 应用模板样式（标题1-5级、正文）
    
    // ===== 图片题注选项 =====
    [ObservableProperty]
    private bool applyImageCaption = true; // 图片题注格式
    
    [ObservableProperty]
    private bool imageCaptionsByChapter = true; // 图片题注按章排列
    
    [ObservableProperty]
    private bool imageCaptionSpacing = true; // 图片题注前后空行
    
    // ===== 表格题注选项 =====
    [ObservableProperty]
    private bool applyTableCaption = true; // 表格题注格式
    
    [ObservableProperty]
    private bool tableCaptionsByChapter = true; // 表格题注按章排列
    
    [ObservableProperty]
    private bool tableCaptionSpacing = true; // 表格题注前后空行
    
    // ===== 页面设置选项 =====
    [ObservableProperty]
    private bool applyPageSetup = true; // 应用页面设置

    public WordFormatBrushViewModel(
        IPickerService picker,
        IMessageService message,
        WordService word,
        GlobalModel model)
    {
        this.picker = picker;
        this.message = message;
        this.word = word;
        this.model = model;
    }

    /// <summary>
    /// 选择模板文件
    /// </summary>
    [RelayCommand]
    private async Task SelectTemplate()
    {
        var file = picker.OpenFile("Word 文档 (*.doc;*.docx)|*.doc;*.docx");
        if (file != null)
        {
            TemplateFilePath = file;
            TemplateFileName = Path.GetFileName(file);
            UpdateTemplateInfo();
            AddLog($"✓ 已选择模板文件: {TemplateFileName}");
            
            // 自动读取并预览模板格式
            await ReadTemplateFormats();
        }
    }
    
    /// <summary>
    /// 读取模板格式
    /// </summary>
    [RelayCommand]
    private async Task ReadTemplateFormats()
    {
        if (string.IsNullOrEmpty(TemplateFilePath) || !File.Exists(TemplateFilePath))
        {
            await message.WarnAsync("请先选择模板文件！");
            return;
        }
        
        try
        {
            model.Status.Busy("正在读取模板格式...");
            AddLog("\n========== 开始读取模板格式 ==========");
            
            await Task.Run(() =>
            {
                var doc = word.GetDocument(TemplateFilePath);
                var paragraphs = doc.GetChildNodes(NodeType.Paragraph, true).OfType<Paragraph>().ToList();
                var images = doc.GetChildNodes(NodeType.Shape, true).OfType<Shape>().Where(s => s.HasImage).ToList();
                var tables = doc.GetChildNodes(NodeType.Table, true).OfType<Table>().ToList();
                
                AddLog($"• 检测到 {paragraphs.Count} 个段落");
                AddLog($"• 检测到 {images.Count} 张图片");
                AddLog($"• 检测到 {tables.Count} 个表格");
                
                // 识别图片题注
                if (images.Any())
                {
                    AddLog($"\n【图片题注】");
                    foreach (var img in images)
                    {
                        var imgPara = img.ParentParagraph;
                        if (imgPara != null)
                        {
                            var nextNode = imgPara.NextSibling;
                            while (nextNode != null && !(nextNode is Paragraph)) nextNode = nextNode.NextSibling;
                            
                            if (nextNode is Paragraph captionPara)
                            {
                                var hasStyleRef = captionPara.Range.Fields.Any(f => f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);
                                var spaceBefore = captionPara.ParagraphFormat.SpaceBefore;
                                var spaceAfter = captionPara.ParagraphFormat.SpaceAfter;
                                bool hasSpacing = (spaceBefore >= 12 || spaceAfter >= 12);
                                
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    ImageCaptionsByChapter = hasStyleRef;
                                    ImageCaptionSpacing = hasSpacing;
                                });
                                
                                AddLog($"按章排列: {(hasStyleRef ? "是" : "否")}");
                                AddLog($"前后空行: {(hasSpacing ? "是" : "否")}");
                                break;
                            }
                        }
                    }
                }
                
                // 识别表格题注
                if (tables.Any())
                {
                    AddLog($"\n【表格题注】");
                    foreach (var tbl in tables)
                    {
                        var prevNode = tbl.PreviousSibling;
                        while (prevNode != null && !(prevNode is Paragraph)) prevNode = prevNode.PreviousSibling;
                        
                        Paragraph? captionPara = null;
                        if (prevNode is Paragraph prevPara)
                        {
                            var hasSeq = prevPara.Range.Fields.Any(f => 
                                f.Type == Aspose.Words.Fields.FieldType.FieldSequence || 
                                f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);
                            if (hasSeq || prevPara.GetText().Trim().StartsWith("表"))
                                captionPara = prevPara;
                        }
                        
                        if (captionPara != null)
                        {
                            var hasStyleRef = captionPara.Range.Fields.Any(f => f.Type == Aspose.Words.Fields.FieldType.FieldStyleRef);
                            var spaceBefore = captionPara.ParagraphFormat.SpaceBefore;
                            var spaceAfter = captionPara.ParagraphFormat.SpaceAfter;
                            bool hasSpacing = (spaceBefore >= 12 || spaceAfter >= 12);
                            
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                TableCaptionsByChapter = hasStyleRef;
                                TableCaptionSpacing = hasSpacing;
                            });
                            
                            AddLog($"按章排列: {(hasStyleRef ? "是" : "否")}");
                            AddLog($"前后空行: {(hasSpacing ? "是" : "否")}");
                            break;
                        }
                    }
                }
                
                AddLog("✓ 模板格式读取完成");
            });
            
            model.Status.Reset();
        }
        catch (Exception ex)
        {
            model.Status.Reset();
            AddLog($"✗ 错误: {ex.Message}");
            await message.ErrorAsync($"读取模板格式失败：\n{ex.Message}");
        }
    }
    
    /// <summary>
    /// 选择目标文件
    /// </summary>
    [RelayCommand]
    private void SelectTarget()
    {
        var file = picker.OpenFile("Word 文档 (*.doc;*.docx)|*.doc;*.docx");
        if (file != null)
        {
            TargetFilePath = file;
            TargetFileName = Path.GetFileName(file);
            UpdateTargetInfo();
            AddLog($"✓ 已选择目标文件: {TargetFileName}");
        }
    }

    /// <summary>
    /// 清空所有选择
    /// </summary>
    [RelayCommand]
    private void Clear()
    {
        TemplateFilePath = string.Empty;
        TemplateFileName = string.Empty;
        TemplateInfo = "尚未选择模板文件\n\n请选择一个Word文档作为格式源";
        
        TargetFilePath = string.Empty;
        TargetFileName = string.Empty;
        TargetInfo = "尚未选择目标文件\n\n请选择需要应用格式的Word文档";
        
        LogText = "已清空所有选择\n\n";
    }

    /// <summary>
    /// 套用模板（导入合并方式）
    /// </summary>
    [RelayCommand]
    private async Task ApplyTemplateMerge()
    {
        if (string.IsNullOrEmpty(TemplateFilePath) || string.IsNullOrEmpty(TargetFilePath))
        {
            await message.WarnAsync("请先选择模板文件和目标文件！");
            return;
        }

        if (!File.Exists(TemplateFilePath) || !File.Exists(TargetFilePath))
        {
            await message.ErrorAsync("文件不存在，请重新选择！");
            return;
        }

        try
        {
            model.Status.Busy("正在套用模板格式...");
            AddLog("\n========== 开始套用模板格式 ==========");

            // 在后台线程执行
            await Task.Run(() =>
            {
                AddLog("正在读取模板和目标文档...");
                
                // 保存文件
                AddLog("正在生成输出文件...");
                var outputPath = GetOutputPath(TargetFilePath);
                
                // 调用套模板方法，传递格式选项
                word.ApplyTemplateMergeFromFile(
                    TemplateFilePath, 
                    TargetFilePath, 
                    outputPath,
                    imageCaptionsByChapter: ImageCaptionsByChapter,
                    imageCaptionSpacing: ImageCaptionSpacing,
                    tableCaptionsByChapter: TableCaptionsByChapter,
                    tableCaptionSpacing: TableCaptionSpacing
                );
                
                AddLog($"✓ 文件已保存: {Path.GetFileName(outputPath)}");
                AddLog("========== 模板格式套用完成 ==========\n");

                // 在UI线程显示成功消息
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    model.Status.Reset();
                    
                    // 显示成功消息
                    await message.SuccessAsync(
                        $"模板格式套用成功！\n\n输出文件：{Path.GetFileName(outputPath)}",
                        "成功");
                    
                    // 询问是否打开文件
                    bool openFile = await message.ConfirmAsync(
                        "是否打开生成的文件？",
                        "打开文件");
                    
                    if (openFile)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = outputPath,
                            UseShellExecute = true
                        });
                    }
                });
            });
        }
        catch (Exception ex)
        {
            model.Status.Reset();
            AddLog($"✗ 错误: {ex.Message}");
            await message.ErrorAsync($"模板套用失败：\n{ex.Message}");
        }
    }

    /// <summary>
    /// 更新模板文件信息
    /// </summary>
    private void UpdateTemplateInfo()
    {
        try
        {
            var doc = word.GetDocument(TemplateFilePath);
            var info = new StringBuilder();
            info.AppendLine($"文件名: {Path.GetFileName(TemplateFilePath)}");
            info.AppendLine($"路径: {TemplateFilePath}");
            info.AppendLine($"大小: {new FileInfo(TemplateFilePath).Length / 1024.0:F2} KB");
            info.AppendLine($"\n页数: {doc.PageCount}");
            info.AppendLine($"段落数: {doc.GetChildNodes(NodeType.Paragraph, true).Count}");
            info.AppendLine($"表格数: {doc.GetChildNodes(NodeType.Table, true).Count}");
            info.AppendLine($"图片数: {doc.GetChildNodes(NodeType.Shape, true).OfType<Aspose.Words.Drawing.Shape>().Count(s => s.HasImage)}");
            info.AppendLine($"样式数: {doc.Styles.Count}");
            
            TemplateInfo = info.ToString();
        }
        catch (Exception ex)
        {
            TemplateInfo = $"无法读取文件信息:\n{ex.Message}";
        }
    }

    /// <summary>
    /// 更新目标文件信息
    /// </summary>
    private void UpdateTargetInfo()
    {
        try
        {
            var doc = word.GetDocument(TargetFilePath);
            var info = new StringBuilder();
            info.AppendLine($"文件名: {Path.GetFileName(TargetFilePath)}");
            info.AppendLine($"路径: {TargetFilePath}");
            info.AppendLine($"大小: {new FileInfo(TargetFilePath).Length / 1024.0:F2} KB");
            info.AppendLine($"\n页数: {doc.PageCount}");
            info.AppendLine($"段落数: {doc.GetChildNodes(NodeType.Paragraph, true).Count}");
            info.AppendLine($"表格数: {doc.GetChildNodes(NodeType.Table, true).Count}");
            info.AppendLine($"图片数: {doc.GetChildNodes(NodeType.Shape, true).OfType<Aspose.Words.Drawing.Shape>().Count(s => s.HasImage)}");
            info.AppendLine($"样式数: {doc.Styles.Count}");
            
            TargetInfo = info.ToString();
        }
        catch (Exception ex)
        {
            TargetInfo = $"无法读取文件信息:\n{ex.Message}";
        }
    }

    /// <summary>
    /// 添加日志
    /// </summary>
    private void AddLog(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            LogText += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
        });
    }

    /// <summary>
    /// 获取输出文件路径
    /// </summary>
    private string GetOutputPath(string originalPath)
    {
        var dir = Path.GetDirectoryName(originalPath);
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalPath);
        var ext = Path.GetExtension(originalPath);
        
        return Path.Combine(dir, $"{fileNameWithoutExt}_格式化{ext}");
    }

    // 以下方法调用WordService中的实现
    private void CopyStyles(Document templateDoc, Document targetDoc)
    {
        foreach (AsposeStyle style in templateDoc.Styles)
        {
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

    private void ApplyParagraphFormats(Document templateDoc, Document targetDoc)
    {
        var templateParagraphs = templateDoc.GetChildNodes(NodeType.Paragraph, true)
            .OfType<Paragraph>()
            .Where(p => !string.IsNullOrEmpty(p.GetText().Trim()))
            .ToList();

        if (!templateParagraphs.Any()) return;

        var templateByLevel = new Dictionary<OutlineLevel, Paragraph>();
        foreach (var para in templateParagraphs)
        {
            var level = para.ParagraphFormat.OutlineLevel;
            if (!templateByLevel.ContainsKey(level))
            {
                templateByLevel[level] = para;
            }
        }

        var targetParagraphs = targetDoc.GetChildNodes(NodeType.Paragraph, true)
            .OfType<Paragraph>()
            .ToList();

        foreach (var targetPara in targetParagraphs)
        {
            var level = targetPara.ParagraphFormat.OutlineLevel;
            
            if (templateByLevel.ContainsKey(level))
            {
                var templatePara = templateByLevel[level];
                ApplyParagraphFormat(templatePara, targetPara);
            }
            else if (templateByLevel.ContainsKey(OutlineLevel.BodyText))
            {
                var templatePara = templateByLevel[OutlineLevel.BodyText];
                ApplyParagraphFormat(templatePara, targetPara);
            }
        }
    }

    private void ApplyParagraphFormat(Paragraph templatePara, Paragraph targetPara)
    {
        CopyParagraphFormatProperties(templatePara.ParagraphFormat, targetPara.ParagraphFormat);

        if (targetPara.Runs.Count > 0 && templatePara.Runs.Count > 0)
        {
            var templateRun = templatePara.Runs[0];
            foreach (Run targetRun in targetPara.Runs)
            {
                CopyFontFormat(templateRun.Font, targetRun.Font);
            }
        }
    }

    private void ApplyTableFormats(Document templateDoc, Document targetDoc)
    {
        var templateTables = word.GetTables(templateDoc);
        var targetTables = word.GetTables(targetDoc);

        if (!templateTables.Any()) return;

        var templateTable = templateTables[0];

        foreach (var targetTable in targetTables)
        {
            ApplyTableFormat(templateTable, targetTable);
        }
    }

    private void ApplyTableFormat(Aspose.Words.Tables.Table templateTable, Aspose.Words.Tables.Table targetTable)
    {
        targetTable.Alignment = templateTable.Alignment;
        targetTable.PreferredWidth = templateTable.PreferredWidth;
        targetTable.LeftIndent = templateTable.LeftIndent;
        targetTable.AllowAutoFit = templateTable.AllowAutoFit;
        targetTable.Bidi = templateTable.Bidi;

        if (templateTable.Style != null)
        {
            targetTable.StyleIdentifier = templateTable.StyleIdentifier;
        }

        targetTable.SetBorders(
            templateTable.FirstRow?.RowFormat?.Borders?.LineStyle ?? LineStyle.Single,
            templateTable.FirstRow?.RowFormat?.Borders?.LineWidth ?? 0.5,
            templateTable.FirstRow?.RowFormat?.Borders?.Color ?? System.Drawing.Color.Black
        );

        for (int rowIndex = 0; rowIndex < targetTable.Rows.Count && rowIndex < templateTable.Rows.Count; rowIndex++)
        {
            var templateRow = templateTable.Rows[rowIndex];
            var targetRow = targetTable.Rows[rowIndex];

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

    private void ApplyCellFormat(Aspose.Words.Tables.Cell templateCell, Aspose.Words.Tables.Cell targetCell)
    {
        targetCell.CellFormat.VerticalAlignment = templateCell.CellFormat.VerticalAlignment;
        targetCell.CellFormat.Width = templateCell.CellFormat.Width;
        targetCell.CellFormat.PreferredWidth = templateCell.CellFormat.PreferredWidth;
        targetCell.CellFormat.Shading.BackgroundPatternColor = templateCell.CellFormat.Shading.BackgroundPatternColor;
        targetCell.CellFormat.Shading.ForegroundPatternColor = templateCell.CellFormat.Shading.ForegroundPatternColor;

        targetCell.CellFormat.Borders.LineStyle = templateCell.CellFormat.Borders.LineStyle;
        targetCell.CellFormat.Borders.LineWidth = templateCell.CellFormat.Borders.LineWidth;
        targetCell.CellFormat.Borders.Color = templateCell.CellFormat.Borders.Color;

        if (templateCell.Paragraphs.Count > 0 && targetCell.Paragraphs.Count > 0)
        {
            var templatePara = templateCell.Paragraphs[0];
            foreach (Paragraph targetPara in targetCell.Paragraphs)
            {
                ApplyParagraphFormat(templatePara, targetPara);
            }
        }
    }

    private void ApplyImageFormats(Document templateDoc, Document targetDoc)
    {
        var templateShapes = templateDoc.GetChildNodes(NodeType.Shape, true)
            .OfType<Aspose.Words.Drawing.Shape>()
            .Where(s => s.HasImage)
            .ToList();

        var targetShapes = targetDoc.GetChildNodes(NodeType.Shape, true)
            .OfType<Aspose.Words.Drawing.Shape>()
            .Where(s => s.HasImage)
            .ToList();

        if (!templateShapes.Any()) return;

        var templateShape = templateShapes[0];

        foreach (var targetShape in targetShapes)
        {
            ApplyShapeFormat(templateShape, targetShape);
        }
    }

    private void ApplyShapeFormat(Aspose.Words.Drawing.Shape templateShape, Aspose.Words.Drawing.Shape targetShape)
    {
        if (templateShape.Width > 0 && templateShape.Height > 0)
        {
            double aspectRatio = targetShape.Height / targetShape.Width;
            targetShape.Width = templateShape.Width;
            targetShape.Height = templateShape.Width * aspectRatio;
        }

        targetShape.WrapType = templateShape.WrapType;
        targetShape.WrapSide = templateShape.WrapSide;

        targetShape.HorizontalAlignment = templateShape.HorizontalAlignment;
        targetShape.VerticalAlignment = templateShape.VerticalAlignment;
        targetShape.RelativeHorizontalPosition = templateShape.RelativeHorizontalPosition;
        targetShape.RelativeVerticalPosition = templateShape.RelativeVerticalPosition;

        if (templateShape.Stroke != null)
        {
            targetShape.Stroke.Color = templateShape.Stroke.Color;
            targetShape.Stroke.Weight = templateShape.Stroke.Weight;
            targetShape.Stroke.LineStyle = templateShape.Stroke.LineStyle;
        }
    }

    private void ApplyPageSetupFormat(Document templateDoc, Document targetDoc)
    {
        foreach (Section targetSection in targetDoc.Sections)
        {
            if (templateDoc.Sections.Count > 0)
            {
                var templateSection = templateDoc.Sections[0];
                var templateSetup = templateSection.PageSetup;
                var targetSetup = targetSection.PageSetup;

                targetSetup.PaperSize = templateSetup.PaperSize;
                targetSetup.PageWidth = templateSetup.PageWidth;
                targetSetup.PageHeight = templateSetup.PageHeight;
                targetSetup.Orientation = templateSetup.Orientation;

                targetSetup.TopMargin = templateSetup.TopMargin;
                targetSetup.BottomMargin = templateSetup.BottomMargin;
                targetSetup.LeftMargin = templateSetup.LeftMargin;
                targetSetup.RightMargin = templateSetup.RightMargin;
                targetSetup.HeaderDistance = templateSetup.HeaderDistance;
                targetSetup.FooterDistance = templateSetup.FooterDistance;

                targetSetup.DifferentFirstPageHeaderFooter = templateSetup.DifferentFirstPageHeaderFooter;
                targetSetup.OddAndEvenPagesHeaderFooter = templateSetup.OddAndEvenPagesHeaderFooter;
            }
        }
    }

    /// <summary>
    /// 复制字体格式属性
    /// </summary>
    private void CopyFontFormat(Font source, Font target)
    {
        target.Name = source.Name;
        target.Size = source.Size;
        target.Bold = source.Bold;
        target.Italic = source.Italic;
        target.Underline = source.Underline;
        target.Color = source.Color;
        target.HighlightColor = source.HighlightColor;
        target.Subscript = source.Subscript;
        target.Superscript = source.Superscript;
        target.AllCaps = source.AllCaps;
        target.SmallCaps = source.SmallCaps;
        target.StrikeThrough = source.StrikeThrough;
        target.DoubleStrikeThrough = source.DoubleStrikeThrough;
        target.Hidden = source.Hidden;
        target.Emboss = source.Emboss;
        target.Engrave = source.Engrave;
        target.Outline = source.Outline;
        target.Shadow = source.Shadow;
        target.Spacing = source.Spacing;
        target.Scaling = source.Scaling;
        target.Position = source.Position;
        target.Kerning = source.Kerning;
    }

    /// <summary>
    /// 复制段落格式属性
    /// </summary>
    private void CopyParagraphFormatProperties(ParagraphFormat source, ParagraphFormat target)
    {
        target.Alignment = source.Alignment;
        target.LeftIndent = source.LeftIndent;
        target.RightIndent = source.RightIndent;
        target.FirstLineIndent = source.FirstLineIndent;
        target.LineSpacing = source.LineSpacing;
        target.LineSpacingRule = source.LineSpacingRule;
        target.SpaceBefore = source.SpaceBefore;
        target.SpaceAfter = source.SpaceAfter;
        target.KeepTogether = source.KeepTogether;
        target.KeepWithNext = source.KeepWithNext;
        target.PageBreakBefore = source.PageBreakBefore;
        target.WidowControl = source.WidowControl;
        target.OutlineLevel = source.OutlineLevel;
        target.Bidi = source.Bidi;
        target.SuppressLineNumbers = source.SuppressLineNumbers;
        target.SuppressAutoHyphens = source.SuppressAutoHyphens;
    }

    public void OnNavigatedTo() { }
    public void OnNavigatedFrom() { }
}
