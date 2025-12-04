﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

using LYSoft.Libs;
using LYSoft.Libs.Editor;
using LYSoft.Libs.Wpf.Services;
using Wpf.Ui.Controls;

namespace LYSoft;

public partial class EditorDialog {
    private readonly EditorOptions options;
    private readonly PickerService picker = new();

    internal EditorDialog(EditorOptions options) {
        InitializeComponent();
        Application.Current.Resources.MergedDictionaries.Cast<ResourceDictionary>().AllDo(Resources.MergedDictionaries.Add);
        TitleBar.Title = options.Title;
        Title = options.Title;
        OkButton.Content = options.OkButtonText;
        CancelButton.Content = options.CancelButtonText;
        Width = options.EditorWidth;
        Height = options.EditorHeight;
        if (!string.IsNullOrEmpty(options.IconPath)) { } //TitleBar.Icon = new BitmapImage(new(Path.GetFullPath(options.IconPath)));
        this.options = options;
        Build();
    }

    private void Build() {
        var index = 0;
        foreach (var option in options.Options) {
            Body.Children.Add(option switch {
                TextEditorOption x => BuildText(x, index),
                SecretTextEditorOption x => BuildSecretText(x, index),
                DoubleEditorOption x => BuildDouble(x, index),
                IntEditorOption x => BuildInt(x, index),
                DateTimeEditorOption x => BuildDateTime(x, index),
                BooleanEditorOption x => BuildSwitch(x, index),
                SingleSelectEditorOption x => BuildSingleSelect(x, index),
                MultiSelectEditorOption x => BuildMultiSelect(x, index),
                ComboSelectEditorOption x => BuildCombo(x, index),
                PickFileEditorOption x => BuildFilePicker(x, index),
                PickFolderEditorOption x => BuildFolderPicker(x, index),
                CommandEditorOption x => BuildCommand(x, index),
                PickImageEditorOption x => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            });
            index++;
        }
    }

    private static StackPanel BuildCommand(CommandEditorOption option, int index) {
        var button = new Wpf.Ui.Controls.Button() { Content = option.Header, Margin = new(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, TabIndex = index };
        button.Click += (_, _) => option.Command();
        return CreateBodyGridWithChildern("", button);
    }

    private StackPanel BuildFolderPicker(PickFolderEditorOption option, int index) {
        var ctrl = new Wpf.Ui.Controls.TextBox() { Width = options.EditorWidth - 250, Margin = new(10, 0, 0, 0), Text = option.Getter(options.Object), IsReadOnly = true, TabIndex = index };
        var btn = new Wpf.Ui.Controls.Button() { Width = 50, Height = 35, Icon = new SymbolIcon(SymbolRegular.MoreHorizontal24) };
        ctrl.TextChanged += (_, _) => option.Setter(options.Object, ctrl.Text);
        btn.Click += (_, _) => { if (picker.PickFolder() is string path) { ctrl.Text = path; } };
        return CreateBodyGridWithChildern(option.Header, ctrl, btn);
    }

    private StackPanel BuildFilePicker(PickFileEditorOption option, int index) {
        var ctrl = new Wpf.Ui.Controls.TextBox() { Width = options.EditorWidth - 250, Margin = new(10, 0, 0, 0), Text = option.Getter(options.Object), IsReadOnly = true, TabIndex = index };
        var btn = new Wpf.Ui.Controls.Button() { Width = 50, Height = 35, Icon = new SymbolIcon(SymbolRegular.MoreHorizontal24) };
        ctrl.TextChanged += (_, _) => option.Setter(options.Object, ctrl.Text);
        Func<string, string> action = option.IsOpenFile ? picker.OpenFile! : picker.SaveFile!;
        btn.Click += (_, _) => { if (action(option.FileFilter) is string path) { ctrl.Text = path; } };
        return CreateBodyGridWithChildern(option.Header, ctrl, btn);
    }

    private StackPanel BuildCombo(ComboSelectEditorOption option, int index) {
        var property = options.Object.GetType().GetProperty(option.PropertyName)!;
        var ctrl = new ComboBox() { Width = options.EditorWidth - 200, VerticalAlignment = VerticalAlignment.Center, Margin = new(10, 0, 0, 0), TabIndex = index };
        option.Options.AllDo(x => ctrl.Items.Add(new ComboBoxItem() { Content = x.Text, Tag = x.Value }));
        ctrl.SelectedItem = ctrl.Items.Cast<ComboBoxItem>().Where(x => x.Tag.Equals(option.ConverterFromProperty(property.GetValue(options.Object)!))).FirstOrDefault();
        ctrl.SelectionChanged += (_, _) => property.SetValue(options.Object, option.ConverterToProperty(((ComboBoxItem)ctrl.SelectedItem!).Tag));
        return CreateBodyGridWithChildern(option.PropertyHeader ?? option.PropertyName, ctrl);
    }

    private StackPanel BuildMultiSelect(MultiSelectEditorOption option, int index) {
        var property = options.Object.GetType().GetProperty(option.PropertyName)!;
        var container = new ItemsControl { Margin = new(10, 0, 0, 0), Width = options.EditorWidth - 200, ItemsPanel = new(new(typeof(WrapPanel))) };
        var init = (object[])option.ConverterFromProperty(property.GetValue(options.Object)!);
        option.Options.AllDo(x => container.Items.Add(new CheckBox() { Tag = x.Value, Content = x.Text, IsChecked = init.Any(y => y.Equals(x.Value)), TabIndex = index }));
        container.Items.Cast<CheckBox>().AllDo(x => x.Click += (_, _) => property.SetValue(options.Object, option.ConverterToProperty(container.Items.Cast<CheckBox>().Where(x => x.IsChecked.GetValueOrDefault(false)).Select(x => x.Tag).ToArray())));
        return CreateBodyGridWithChildern(option.PropertyHeader ?? option.PropertyName, container);
    }

    private StackPanel BuildSingleSelect(SingleSelectEditorOption option, int index) {
        var property = options.Object.GetType().GetProperty(option.PropertyName)!;
        var container = new ItemsControl { Margin = new(10, 0, 0, 0), Width = options.EditorWidth - 200, ItemsPanel = new(new(typeof(WrapPanel))) };
        var init = option.ConverterFromProperty(property.GetValue(options.Object)!);
        option.Options.AllDo(x => container.Items.Add(new RadioButton() { Tag = x.Value, Content = x.Text, IsChecked = init.Equals(x.Value), TabIndex = index }));
        container.Items.Cast<RadioButton>().AllDo(x => x.Checked += (_, _) => property.SetValue(options.Object, option.ConverterToProperty(container.Items.Cast<RadioButton>().Single(x => x.IsChecked.GetValueOrDefault(false)).Tag)));
        return CreateBodyGridWithChildern(option.PropertyHeader ?? option.PropertyName, container);
    }

    private StackPanel BuildSwitch(BooleanEditorOption option, int index) {
        var property = options.Object.GetType().GetProperty(option.PropertyName)!;
        var ctrl = new ToggleSwitch() { VerticalAlignment = VerticalAlignment.Center, Margin = new(10, 0, 0, 0), IsChecked = option.ConverterFromProperty(property.GetValue(options.Object)!), TabIndex = index };
        ctrl.Checked += (_, _) => property.SetValue(options.Object, option.ConverterToProperty(ctrl.IsChecked.GetValueOrDefault(false)));
        ctrl.Unchecked += (_, _) => property.SetValue(options.Object, option.ConverterToProperty(ctrl.IsChecked.GetValueOrDefault(false)));
        return CreateBodyGridWithChildern(option.PropertyHeader ?? option.PropertyName, ctrl);
    }

    private StackPanel BuildDateTime(DateTimeEditorOption option, int index) {
        if (option.MinValue > option.MinValue) { throw new("最小值不能大于最大值"); }
        var property = options.Object.GetType().GetProperty(option.PropertyName)!;
        var init = option.ConverterFromProperty(property.GetValue(options.Object)!);
        var ctrl = new DatePicker() { Width = options.EditorWidth - 200, Margin = new(10, 0, 0, 0), SelectedDate = init, DisplayDateEnd = option.MaxValue, DisplayDateStart = option.MinValue, TabIndex = index };
        property.SetValue(options.Object, option.ConverterToProperty(init));
        ctrl.SelectedDateChanged += (_, _) => property.SetValue(options.Object, option.ConverterToProperty(ctrl.SelectedDate.Value));
        return CreateBodyGridWithChildern(option.PropertyHeader ?? option.PropertyName, ctrl);
    }

    private StackPanel BuildInt(IntEditorOption option, int index) {
        if (option.MinValue > option.MinValue) { throw new("最小值不能大于最大值"); }
        var property = options.Object.GetType().GetProperty(option.PropertyName)!;
        var init = option.ConverterFromProperty(property.GetValue(options.Object)!);
        if (init < option.MinValue) { init = option.MinValue; }
        if (init > option.MaxValue) { init = option.MaxValue; }
        var ctrl = new NumberBox() { Width = options.EditorWidth - 200, Margin = new(10, 0, 0, 0), Value = init, PlaceholderText = option.PlaceHolder, TabIndex = index };
        property.SetValue(options.Object, option.ConverterToProperty(init));
        ctrl.ValueChanged += (_, _) => {
            var oldvalue = option.ConverterFromProperty(property.GetValue(options.Object)!);
            var newValue = (int)(ctrl.Value ?? 0);
            if (newValue < option.MinValue) { newValue = option.MinValue; ctrl.Text = newValue.ToString(); return; }
            if (newValue > option.MaxValue) { newValue = option.MaxValue; ctrl.Text = newValue.ToString(); return; }
            property.SetValue(options.Object, option.ConverterToProperty(newValue));
        };
        return CreateBodyGridWithChildern(option.PropertyHeader ?? option.PropertyName, ctrl);
    }

    private StackPanel BuildDouble(DoubleEditorOption option, int index) {
        if (option.MinValue > option.MinValue) { throw new("最小值不能大于最大值"); }
        var property = options.Object.GetType().GetProperty(option.PropertyName)!;
        var init = option.ConverterFromProperty(property.GetValue(options.Object)!);
        if (init < option.MinValue) { init = option.MinValue; }
        if (init > option.MaxValue) { init = option.MaxValue; }
        var ctrl = new NumberBox() { Width = options.EditorWidth - 200, Margin = new(10, 0, 0, 0), Value = init, PlaceholderText = option.PlaceHolder, TabIndex = index };
        property.SetValue(options.Object, option.ConverterToProperty(init));
        ctrl.ValueChanged += (_, _) => {
            var oldvalue = option.ConverterFromProperty(property.GetValue(options.Object)!);
            if (!double.TryParse(ctrl.Text, out var newValue)) { ctrl.Text = oldvalue.ToString(); return; }
            if (newValue < option.MinValue) { newValue = option.MinValue; ctrl.Text = newValue.ToString(); return; }
            if (newValue > option.MaxValue) { newValue = option.MaxValue; ctrl.Text = newValue.ToString(); return; }
            property.SetValue(options.Object, option.ConverterToProperty(newValue));
        };
        return CreateBodyGridWithChildern(option.PropertyHeader ?? option.PropertyName, ctrl);
    }

    private StackPanel BuildSecretText(SecretTextEditorOption option, int index) {
        var property = options.Object.GetType().GetProperty(option.PropertyName)!;
        var ctrl = new Wpf.Ui.Controls.PasswordBox() { Width = options.EditorWidth - 200, Margin = new(10, 0, 0, 0), Password = option.ConverterFromProperty(property.GetValue(options.Object)!), MaxLength = option.MaxLength, TabIndex = index };
        ctrl.TextChanged += (_, _) => property.SetValue(options.Object, option.ConverterToProperty(ctrl.Password));
        return CreateBodyGridWithChildern(option.PropertyHeader ?? option.PropertyName, ctrl);
    }

    private StackPanel BuildText(TextEditorOption option, int index) {
        var property = options.Object.GetType().GetProperty(option.PropertyName)!;
        var ctrl = new Wpf.Ui.Controls.TextBox() { Width = options.EditorWidth - 200, Margin = new(10, 0, 0, 0), PlaceholderText = option.PlaceHolder, Text = option.ConverterFromProperty(property.GetValue(options.Object)!), MaxLength = option.MaxLength, TabIndex = index };
        if (option.HasMultiLine) {
            ctrl.TextWrapping = TextWrapping.Wrap;
            ctrl.MinLines = 1;
            ctrl.AcceptsReturn = true;
        }
        ctrl.TextChanged += (_, _) => property.SetValue(options.Object, option.ConverterToProperty(ctrl.Text));
        return CreateBodyGridWithChildern(option.PropertyHeader ?? option.PropertyName, ctrl);
    }

    /// <summary>
    /// 处理确认按钮的点击事件
    /// 执行数据验证，如果验证通过则关闭对话框并返回true，否则显示错误信息
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private async void OK_Click(object sender, RoutedEventArgs e) {
        var result = await options.ValidateAsync();
        if (!string.IsNullOrEmpty(result)) {
            System.Windows.MessageBox.Show(result, "系统提示");
            return;
        }
        DialogResult = true;
    }

    /// <summary>
    /// 处理取消按钮的点击事件
    /// 直接关闭对话框并返回false，不保存任何修改
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void Cancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    /// <summary>
    /// 创建包含标签和子元素的布局容器
    /// 为编辑器元素创建统一的水平布局，包含标签和实际的输入控件
    /// </summary>
    /// <param name="header">显示在左侧的标签文本</param>
    /// <param name="elements">要添加到布局中的UI元素数组</param>
    /// <returns>返回包含所有元素的StackPanel容器</returns>
    private static StackPanel CreateBodyGridWithChildern(string header, params UIElement[] elements) {
        var body = new StackPanel() { Margin = new(0, 10, 0, 10), Orientation = Orientation.Horizontal, MinHeight = 35 };
        body.Children.Add(new Wpf.Ui.Controls.TextBlock() { Text = header, Width = 100, Margin = new(0, 9, 0, 0) });
        elements.AllDo(e => body.Children.Add(e));
        return body;
    }
}