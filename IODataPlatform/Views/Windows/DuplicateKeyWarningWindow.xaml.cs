using System.Collections.Generic;
using System.Windows;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Windows;

public partial class DuplicateKeyWarningWindow : FluentWindow
{
    public bool ContinueComparison { get; private set; } = false;

    public DuplicateKeyWarningWindow(List<DuplicateKeyInfo> duplicateKeys)
    {
        InitializeComponent();
        
        // 绑定数据
        DuplicateKeysGrid.ItemsSource = duplicateKeys;
        
        // 设置统计信息
        int totalDuplicates = 0;
        var fileSet = new HashSet<string>();
        
        foreach (var item in duplicateKeys)
        {
            totalDuplicates += item.Count;
            fileSet.Add(item.FileName);
        }
        
        SummaryText.Text = $"共发现 {duplicateKeys.Count} 个重复的主键值，涉及 {fileSet.Count} 个文件，总计 {totalDuplicates} 条重复记录。";
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        ContinueComparison = true;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        ContinueComparison = false;
        DialogResult = false;
        Close();
    }
}

/// <summary>
/// 重复主键信息
/// </summary>
public class DuplicateKeyInfo
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// 主键值
    /// </summary>
    public string KeyValue { get; set; } = string.Empty;
    
    /// <summary>
    /// 重复次数
    /// </summary>
    public int Count { get; set; }
}
