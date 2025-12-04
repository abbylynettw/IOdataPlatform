using IODataPlatform.Utilities;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// Excel筛选窗口
/// </summary>
public partial class ExcelFilterWindow : FluentWindow
{
    public ExcelFilter Filter { get; }

    public ExcelFilterWindow(ExcelFilter filter)
    {
        Filter = filter;
        DataContext = filter;
        InitializeComponent();
    }

    private void SelectAll_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Filter.SelectAll();
    }

    private void UnselectAll_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Filter.UnselectAll();
    }

    private void InverseSelect_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Filter.InverseSelect();
    }

    private void ConfirmButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
