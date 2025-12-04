using IODataPlatform.Models.ExcelModels;
using System.Reflection;
using System.Windows.Controls;
using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Views.Pages;

public partial class DepAQJPage : INavigableView<DepAQJViewModel>
{
    public DepAQJViewModel ViewModel { get; }
    private readonly Dictionary<string, DataGridColumn> columnMapping = new();

    public DepAQJPage(DepAQJViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
       
    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        InitializeColumnsVisibilityPanel();    
    }
    private void InitializeColumnsVisibilityPanel()
    {
        var columnsInit = ViewModel.GetDefaultField();
        ColumnsVisibilityPanel.Children.Clear();
        // 全选复选框
        var checkAll = new CheckBox { Content = "全选", IsChecked = false };
        checkAll.Checked += (_, _) =>
        {
            foreach (var cb in ColumnsVisibilityPanel.Children.OfType<CheckBox>())
            {
                cb.IsChecked = true;
                UpdateColumnVisibility(cb.Content.ToString(), true);
            }
        };
        checkAll.Unchecked += (_, _) =>
        {
            foreach (var cb in ColumnsVisibilityPanel.Children.OfType<CheckBox>())
            {
                cb.IsChecked = false;
                UpdateColumnVisibility(cb.Content.ToString(), false);
            }
        };
        ColumnsVisibilityPanel.Children.Add(checkAll);

        // 生成对应每个字段的复选框
        foreach (var prop in typeof(IoFullData).GetProperties())
        {
            var displayName = prop.GetCustomAttribute<DisplayAttribute>()?.Name ?? prop.Name;
            var checkbox = new CheckBox { Content = displayName, IsChecked = columnsInit.Contains(displayName) };

            checkbox.Checked += (_, _) => UpdateColumnVisibility(displayName, true);
            checkbox.Unchecked += (_, _) => UpdateColumnVisibility(displayName, false);

            ColumnsVisibilityPanel.Children.Add(checkbox);
        }
    }

    private void UpdateColumnVisibility(string columnName, bool isVisible)
    {
        if (columnMapping.TryGetValue(columnName, out var column))
        {
            column.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        var header = e.Column.Header.ToString();
        // 根据复选框状态设置列的可见性
        var isVisible = ColumnsVisibilityPanel.Children.OfType<CheckBox>()
            .FirstOrDefault(cb => cb.Content.ToString() == header)?.IsChecked ?? true;
        e.Column.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        // 将生成的列添加到映射字典中
        columnMapping[header] = e.Column;
    }

    private void DataGrid_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.DataGrid dataGrid)
        {
            dataGrid.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
        }
    }

    private void DataGrid_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.DataGrid dataGrid)
        {
            dataGrid.AutoGeneratingColumn -= DataGrid_AutoGeneratingColumn;
        }
    }

   
}
