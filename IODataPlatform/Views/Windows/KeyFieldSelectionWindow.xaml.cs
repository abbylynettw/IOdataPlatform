using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Windows;

public partial class KeyFieldSelectionWindow : FluentWindow
{
    private readonly List<CheckBox> allFieldCheckBoxes = new();
    private readonly List<string> selectedFields = new();
    private bool isOrderMode = false;

    public List<string> SelectedFields => selectedFields;
    public bool IsOrderMode => isOrderMode;

    public KeyFieldSelectionWindow(List<string> availableFields, List<string> currentSelectedFields)
    {
        InitializeComponent();
        InitializeFields(availableFields, currentSelectedFields);
    }

    private void InitializeFields(List<string> availableFields, List<string> currentSelectedFields)
    {
        allFieldCheckBoxes.Clear();
        FieldsPanel.Children.Clear();

        foreach (var field in availableFields)
        {
            var checkBox = new CheckBox
            {
                Content = field,
                IsChecked = currentSelectedFields.Contains(field),
                Margin = new Thickness(5),
                Width = 200
            };

            checkBox.Checked += FieldCheckBox_Changed;
            checkBox.Unchecked += FieldCheckBox_Changed;

            allFieldCheckBoxes.Add(checkBox);
            FieldsPanel.Children.Add(checkBox);
        }

        UpdateSelectedCount();
    }

    /// <summary>
    /// 搜索框文本变化，过滤字段
    /// </summary>
    private void FieldSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = FieldSearchBox.Text?.ToLower() ?? string.Empty;
        FieldsPanel.Children.Clear();

        foreach (var checkBox in allFieldCheckBoxes)
        {
            string fieldName = checkBox.Content.ToString()?.ToLower() ?? string.Empty;
            if (string.IsNullOrEmpty(searchText) || fieldName.Contains(searchText))
            {
                FieldsPanel.Children.Add(checkBox);
            }
        }
    }

    /// <summary>
    /// 字段复选框状态变化
    /// </summary>
    private void FieldCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        UpdateSelectedCount();
    }

    /// <summary>
    /// 更新已选择字段数量
    /// </summary>
    private void UpdateSelectedCount()
    {
        int count = allFieldCheckBoxes.Count(cb => cb.IsChecked == true);
        SelectedCountText.Text = count.ToString();
    }

    /// <summary>
    /// 按顺序对比按钮
    /// </summary>
    private void OrderMode_Click(object sender, RoutedEventArgs e)
    {
        // 取消所有选择
        foreach (var checkBox in allFieldCheckBoxes)
        {
            checkBox.IsChecked = false;
        }

        isOrderMode = true;
        DialogResult = true;
        Close();
    }

    /// <summary>
    /// 取消全选按钮
    /// </summary>
    private void UnselectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var checkBox in allFieldCheckBoxes)
        {
            if (FieldsPanel.Children.Contains(checkBox))
            {
                checkBox.IsChecked = false;
            }
        }
    }

    /// <summary>
    /// 确定按钮
    /// </summary>
    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        selectedFields.Clear();

        foreach (var checkBox in allFieldCheckBoxes)
        {
            if (checkBox.IsChecked == true)
            {
                selectedFields.Add(checkBox.Content.ToString());
            }
        }

        isOrderMode = false;
        DialogResult = true;
        Close();
    }

    /// <summary>
    /// 取消按钮
    /// </summary>
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
