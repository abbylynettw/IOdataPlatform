using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// 列验证结果项
/// </summary>
public class ColumnValidationResult
{
    public string SheetName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<string> ExpectedColumns { get; set; } = new();
    public List<string> ActualColumns { get; set; } = new();
    public List<string> MissingColumnsList { get; set; } = new();
    public List<string> ExtraColumnsList { get; set; } = new();

    public SymbolRegular StatusIcon => IsValid ? SymbolRegular.CheckmarkCircle24 : SymbolRegular.ErrorCircle24;
    public string StatusColor => IsValid ? "#4CAF50" : "#D32F2F";
    public string StatusText => IsValid ? "✓ 验证通过" : "✗ 验证失败";
    
    public string MissingColumns => string.Join("、", MissingColumnsList);
    public string ExtraColumns => string.Join("、", ExtraColumnsList);
    
    public Visibility HasMissingColumns => MissingColumnsList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility HasExtraColumns => ExtraColumnsList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    
    public string ColumnCountInfo => $"期望：{ExpectedColumns.Count}列，实际：{ActualColumns.Count}列";
}

/// <summary>
/// 列结构验证结果窗口
/// </summary>
public partial class ColumnValidationResultWindow : Window
{
    private readonly ObservableCollection<ColumnValidationResult> _results;

    public ColumnValidationResultWindow(List<ColumnValidationResult> results)
    {
        InitializeComponent();

        _results = new ObservableCollection<ColumnValidationResult>(results);
        ValidationResultsItemsControl.ItemsSource = _results;

        UpdateSummary();
    }

    private void UpdateSummary()
    {
        int totalCount = _results.Count;
        int validCount = _results.Count(r => r.IsValid);
        int invalidCount = totalCount - validCount;

        if (invalidCount == 0)
        {
            SummaryText.Text = $"✓ 所有{totalCount}个工作表的列结构验证通过";
            SummaryText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
        }
        else
        {
            SummaryText.Text = $"✗ {totalCount}个工作表中，{invalidCount}个验证失败，{validCount}个验证通过";
            SummaryText.Foreground = new SolidColorBrush(Color.FromRgb(211, 47, 47)); // Red
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// 是否所有验证都通过
    /// </summary>
    public bool IsAllValid => _results.All(r => r.IsValid);
}
