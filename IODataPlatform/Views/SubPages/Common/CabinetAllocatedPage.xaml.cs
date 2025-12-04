using System.Globalization;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using IODataPlatform.Models;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Views.Windows;

using LYSoft.Libs.Wpf.Extensions;

namespace IODataPlatform.Views.SubPages.Common;

public partial class CabinetAllocatedPage : INavigableView<CabinetAllocatedViewModel>
{
    private readonly GlobalModel model;
    private readonly Dictionary<string, DataGridColumn> columnMapping = new Dictionary<string, DataGridColumn>();
    private readonly Dictionary<string, bool> userColumnVisibility = new Dictionary<string, bool>();

    public CabinetAllocatedViewModel ViewModel { get; }

    public CabinetAllocatedPage(CabinetAllocatedViewModel viewModel, GlobalModel model)
    {
        ViewModel = viewModel;
        this.model = model;
        DataContext = this;

        Loaded += CabinetAllocatedPage_Loaded;
        InitializeComponent();
    }

    private void CabinetAllocatedPage_Loaded(object sender, RoutedEventArgs e) 
    {
        // 不需要在这里注册事件，改用DataGrid的Loaded/Unloaded事件
    }

    /// <summary>
    /// 刷新DataGrid以重新生成列
    /// </summary>
    private void RefreshDataGrid()
    {        
        if (Data1 != null)
        {
            var itemsSource = Data1.ItemsSource;
            
            // 清空所有动态生成的列（保留前两列：复选框和移动列）
            var fixedColumns = Data1.Columns.Take(2).ToList();
            Data1.Columns.Clear();
            
            // 重新添加固定列
            foreach (var col in fixedColumns)
            {
                Data1.Columns.Add(col);
            }
            
            // 重新设置ItemsSource以触发列重新生成
            Data1.ItemsSource = null;
            Data1.ItemsSource = itemsSource;
        }
        
        if (Data2 != null)
        {
            var itemsSource = Data2.ItemsSource;
            
            // 清空所有动态生成的列（保留前两列：复选框和移动列）
            var fixedColumns = Data2.Columns.Take(2).ToList();
            Data2.Columns.Clear();
            
            // 重新添加固定列
            foreach (var col in fixedColumns)
            {
                Data2.Columns.Add(col);
            }
            
            // 重新设置ItemsSource以触发列重新生成
            Data2.ItemsSource = null;
            Data2.ItemsSource = itemsSource;
        }
    }

    /// <summary>
    /// 显示列选择对话框
    /// </summary>
    private void ShowColumnsDialog_Click(object sender, RoutedEventArgs e)
    {
        var defaultFields = ViewModel.GetDefaultField();
        
        // 构建当前列的可见性状态
        var currentVisibility = new Dictionary<string, bool>();
        foreach (var kvp in columnMapping)
        {
            currentVisibility[kvp.Key] = kvp.Value.Visibility == Visibility.Visible;
        }
        
        var dialog = new ColumnSelectionWindow(defaultFields, currentVisibility)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            // 保存用户的选择
            foreach (var kvp in dialog.ColumnVisibility)
            {
                userColumnVisibility[kvp.Key] = kvp.Value; // 保存用户选择
            }
            
            // 直接更新现有列的可见性，而不是清空columnMapping
            foreach (var kvp in dialog.ColumnVisibility)
            {
                if (columnMapping.TryGetValue(kvp.Key, out var column))
                {
                    column.Visibility = kvp.Value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
    }

    /// <summary>
    /// DataGrid自动生成列事件
    /// </summary>
    private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        var header = e.Column.Header?.ToString();
        if (string.IsNullOrEmpty(header)) return;
        
        var defaultFields = ViewModel.GetDefaultField();

        // 默认不显示报警相关字段
        bool isAlarmRelated = header.Contains("报警") ||
                             header.Contains("限") ||
                             header.Contains("告警") ||
                             header.Contains("Alarm") ||
                             header.Contains("Limit");

        // 检查是否在“电磁阀箱类型”之后，如果是则默认不显示
        bool isAfterEletroValueBox = false;
        var properties = typeof(IoFullData).GetProperties().ToList();
        
        bool foundEletroValueBox = false;
        foreach (var prop in properties)
        {
            var displayName = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>()?.Name;
            
            // 找到“电磁阀箱类型”字段（属性名是eletroValueBox）
            if (prop.Name == "eletroValueBox")
            {
                foundEletroValueBox = true;
            }
            else if (foundEletroValueBox && displayName == header)
            {
                isAfterEletroValueBox = true;
                break;
            }
        }

        // 如果用户已经设置过，使用用户的选择；否则使用默认值
        bool isVisible;
        if (userColumnVisibility.ContainsKey(header))
        {
            isVisible = userColumnVisibility[header];
        }
        else
        {
            isVisible = defaultFields.Contains(header) && !isAlarmRelated && !isAfterEletroValueBox;
        }
        
        e.Column.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

        // 将生成的列添加到映射字典中
        columnMapping[header] = e.Column;
    }

    /// <summary>
    /// 更新列的可见性
    /// </summary>
    private void UpdateColumnVisibility(string columnHeader, bool isVisible)
    {
        if (columnMapping.TryGetValue(columnHeader, out var column))
        {
            column.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
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

    private void BoardDragStart(object sender, MouseEventArgs e) {
        e.Handled = true;
    
        if (e.LeftButton != MouseButtonState.Pressed) { return; }
        var board = sender.AsTrue<Border>().GetTag<Board>()!;
        var data = new DataObject();
        data.SetData(board);
        model.Status.Message($"正在拖拽端子板：[{board.Type}]");
        DragDrop.DoDragDrop(this, data, DragDropEffects.All);
        model.Status.Reset();
    }

    private void PointDragStart(object sender, MouseEventArgs e) {
        e.Handled = true;
   
        if (e.LeftButton != MouseButtonState.Pressed) { return; }
        var point = sender.AsTrue<Border>().GetTag<IoFullData>()!;
        var data = new DataObject();
        data.SetData(point);
        model.Status.Message($"正在拖拽点：{point.TagName}");
        DragDrop.DoDragDrop(this, data, DragDropEffects.All);
        model.Status.Reset();
    }

    private void ChannelDragOver(object sender, DragEventArgs e) {
        e.Handled = true;

        if (e.Data.GetDataPresent(typeof(IoFullData))) {
            e.Effects = DragDropEffects.Move;
        } else {
            e.Effects = DragDropEffects.None;
        }
    }

    private void ChannelDrop(object sender, DragEventArgs e) {
        e.Handled = true;

        if (e.Data.GetDataPresent(typeof(IoFullData))) {
            var border = sender.AsTrue<Border>();
            var tag = border.Tag;
            
            // 支持三种通道类型
            if (tag is Xt2Channel xt2Channel)
            {
                // 普通板卡通道
                ViewModel.Move(GetSelectedPoints(), xt2Channel);
            }
            else if (tag is FFBusChannel ffBusChannel)
            {
                // FF总线箱通道（需要在ViewModel中添加支持）
                ViewModel.Move(GetSelectedPoints(), ffBusChannel);
            }
            else if (tag is FFSlaveChannel ffSlaveChannel)
            {
                // FF从站通道（需要在ViewModel中添加支持）
                ViewModel.Move(GetSelectedPoints(), ffSlaveChannel);
            }
        }
    }
    
    private void UnsetBoardsDragOver(object sender, DragEventArgs e) {
        e.Handled = true;

        if (e.Data.GetDataPresent(typeof(Board))) {
            e.Effects = DragDropEffects.Move;
        } else {
            e.Effects = DragDropEffects.None;
        }
    }

    private void UnsetBoardsDrop(object sender, DragEventArgs e) {
        e.Handled = true;

        if (e.Data.GetDataPresent(typeof(Board))) {
            var board = e.Data.GetData(typeof(Board)).AsTrue<Board>();
            ViewModel.Unset(board);
        }
    }
       
    private void UnsetPointsDragOver(object sender, DragEventArgs e) {
        e.Handled = true;

        if (e.Data.GetDataPresent(typeof(IoFullData))) {
            e.Effects = DragDropEffects.Move;
        } else {
            e.Effects = DragDropEffects.None;
        }
    }

    private void UnsetPointsDrop(object sender, DragEventArgs e) {
        e.Handled = true;

        if (e.Data.GetDataPresent(typeof(IoFullData))) {
            //var point = e.Data.GetData(typeof(IoFullData)).AsTrue<IoFullData>();
            ViewModel.Unset(GetSelectedPoints());
        }
    }
          
    private void DeleteDragOver(object sender, DragEventArgs e) {
        e.Handled = true;

        if (!e.Data.GetDataPresent(typeof(Board))) { e.Effects = DragDropEffects.None; return; }
        if (e.Data.GetData(typeof(Board)).AsTrue<Board>().Channels.Any(x => x.Point != null)) { e.Effects = DragDropEffects.None; return; }
        e.Effects = DragDropEffects.Move;
    }

    private void DeleteDrop(object sender, DragEventArgs e) {
        e.Handled = true;

        if (e.Data.GetDataPresent(typeof(Board))) {
            var board = e.Data.GetData(typeof(Board)).AsTrue<Board>();
            ViewModel.Delete(board);
        }
    }
             
    private void ViewDragOver(object sender, DragEventArgs e) {
        e.Handled = true;

        if (e.Data.GetDataPresent(typeof(Board))) {
            e.Effects = DragDropEffects.Move;
        } else {
            e.Effects = DragDropEffects.None;
        }
    }

    private void ViewDrop(object sender, DragEventArgs e) {
        e.Handled = true;

        if (e.Data.GetDataPresent(typeof(Board))) {
            var board = e.Data.GetData(typeof(Board)).AsTrue<Board>();
            ViewModel.View(board);
        }
    }

    private void SlotDragOver(object sender, DragEventArgs e) {
        e.Handled = true;

        if (e.Data.GetDataPresent(typeof(Board))) {
            e.Effects = DragDropEffects.Move;
        } else {
            e.Effects = DragDropEffects.None;
        }
    }

    private void SlotDrop(object sender, DragEventArgs e) {
        e.Handled = true;

        if (e.Data.GetDataPresent(typeof(Board))) {
            var slot = sender.AsTrue<Border>().GetTag<SlotInfo>()!;
            var board = e.Data.GetData(typeof(Board)).AsTrue<Board>();
            ViewModel.Move(board, slot);
        }
    }

    private List<CheckBox> GetAllCheckBox(Wpf.Ui.Controls.DataGrid ctrl) {
        var allCheckBoxes = new List<CheckBox>();

        foreach (var item in ctrl.Items) {
            var dataGridRow = ctrl.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
            if (dataGridRow != null) {
                var checkBoxes = FindVisualChildren<CheckBox>(dataGridRow);
                foreach (var checkBox in checkBoxes) {
                    allCheckBoxes.Add(checkBox);
                }
            }
        }

        return allCheckBoxes;
    }

    private List<IoFullData> GetSelectedPoints() {
        return GetAllCheckBox(Data1).Concat(GetAllCheckBox(Data2)).Where(x => x.IsChecked == true).Select(x => x.GetTag<IoFullData>()!).ToList();
    }

    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject {
        if (parent != null) {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T t) {
                    yield return t;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child!)) {
                    yield return childOfChild;
                }
            }
        }
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e) {
        GetAllCheckBox(sender.GetTag<Wpf.Ui.Controls.DataGrid>()!).AllDo(x => x.IsChecked = true);
    }

    private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
        GetAllCheckBox(sender.GetTag<Wpf.Ui.Controls.DataGrid>()!).AllDo(x => x.IsChecked = false);
    }

    /// <summary>
    /// 板卡眼睛按钮点击事件：选中板卡并查看整个板卡的信号
    /// </summary>
    private void ViewBoardClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        e.Handled = true;
        
        if (sender is Border border && border.Tag is Board board)
        {
            // 设置选中的板卡
            ViewModel.SelectedBoard = board;
            ViewModel.SelectedNetwork = null; // 清除网段选中
            ViewModel.SelectedModule = null; // 清除模块选中
            ViewModel.ViewBoard = board;
            
            // 收集该板卡的所有信号
            var boardSignals = StdCabinet.GetAllSignals(board);
            
            // 设置DisplayBoardPoints
            ViewModel.DisplayBoardPoints = new System.Collections.ObjectModel.ObservableCollection<IoFullData>(boardSignals);
            
            System.Diagnostics.Debug.WriteLine($"选中板卡 {board.Type}：共{boardSignals.Count()}个信号");
        }
    }

    /// <summary>
    /// 网段框点击事件：选中网段并显示该网段的所有信号
    /// </summary>
    private void NetworkBorder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        e.Handled = true;
        
        if (sender is Border border && border.Tag is FFNetwork network)
        {
            // 设置选中的网段
            ViewModel.SelectedNetwork = network;
            ViewModel.SelectedModule = null; // 清除模块选中
            ViewModel.SelectedBoard = null; // 清除板卡选中，板卡不变红
            
            // 获取网段所属的板卡（向上查找）
            var parentBoard = FindParentBoard(border);
            
            if (parentBoard != null && ViewModel != null)
            {
                // 收集该网段的所有信号
                var networkSignals = new List<IoFullData>();
                
                // 根据板卡类型收集信号
                if (parentBoard.FFBoardType == BoardType.FFBus)
                {
                    // FF总线箱：直接从 FFBusChannels 收集
                    foreach (var channel in network.FFBusChannels)
                    {
                        if (channel.Point != null)
                        {
                            networkSignals.Add(channel.Point);
                        }
                    }
                }
                else if (parentBoard.FFBoardType == BoardType.FFSlave)
                {
                    // FF从站箱：从 Modules → FFSlaveChannels 收集，加上 UnallocatedSignals
                    // 1. 收集已分配到模块的信号
                    foreach (var module in network.Modules)
                    {
                        foreach (var channel in module.FFSlaveChannels)
                        {
                            if (channel.Point != null)
                            {
                                networkSignals.Add(channel.Point);
                            }
                        }
                    }
                    
                    // 2. 收集未分配的信号（IO分配后、FF分配前）
                    foreach (var signal in network.UnallocatedSignals)
                    {
                        networkSignals.Add(signal);
                    }
                }
                
                // 直接设置DisplayBoardPoints，覆盖Filter的结果
                ViewModel.DisplayBoardPoints = new System.Collections.ObjectModel.ObservableCollection<IoFullData>(networkSignals);
                
                // 输出调试信息（可选）
                var networkName = network.NetworkType.ToString();
                var signalCount = networkSignals.Count;
                System.Diagnostics.Debug.WriteLine($"选中{networkName}：共{signalCount}个信号");
            }
        }
    }
    
    /// <summary>
    /// 模块点击事件：选中模块并显示该模块的所有信号
    /// </summary>
    private void ModuleBorder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        e.Handled = true;
        
        if (sender is FrameworkElement element && element.Tag is FFSlaveModule module)
        {
            // 设置选中的模块
            ViewModel.SelectedModule = module;
            ViewModel.SelectedNetwork = null; // 清除网段选中
            ViewModel.SelectedBoard = null; // 清除板卡选中，板卡不变红
            
            // 获取模块所属的板卡
            var parentBoard = FindParentBoard(element);
            
            if (parentBoard != null && ViewModel != null)
            {
                // 收集该模块的所有信号
                var moduleSignals = new List<IoFullData>();
                foreach (var channel in module.FFSlaveChannels)
                {
                    if (channel.Point != null)
                    {
                        moduleSignals.Add(channel.Point);
                    }
                }
                
                // 设置DisplayBoardPoints
                ViewModel.DisplayBoardPoints = new System.Collections.ObjectModel.ObservableCollection<IoFullData>(moduleSignals);
                
                System.Diagnostics.Debug.WriteLine($"选中模块 站{module.StationNumber:D2} {module.ModuleType}：共{moduleSignals.Count}个信号");
            }
        }
    }
    
    /// <summary>
    /// 查承FF网段所属的板卡
    /// </summary>
    private Board? FindParentBoard(DependencyObject element)
    {
        var current = element;
        while (current != null)
        {
            current = VisualTreeHelper.GetParent(current);
            
            // 查找Border，Tag为SlotInfo的元素
            if (current is Border border && border.Tag is SlotInfo slot)
            {
                return slot.Board;
            }
        }
        return null;
    }
}

/// <summary>FF总线板卡可见性转换器</summary>
public class FFBusVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BoardType boardType)
        {
            return boardType == BoardType.FFBus ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>FF从站板卡可见性转换器</summary>
public class FFSlaveVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BoardType boardType)
        {
            return boardType == BoardType.FFSlave ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>普通板卡可见性转换器</summary>
public class NormalBoardVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BoardType boardType)
        {
            return boardType == BoardType.Normal ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>网段边框颜色转换器（选中变红）</summary>
public class NetworkBorderBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is FFNetwork currentNetwork && values[1] is FFNetwork selectedNetwork)
        {
            if (currentNetwork == selectedNetwork)
            {
                return new SolidColorBrush(Colors.Red); // 选中变红
            }
        }
        // 默认普通边框颜色
        return System.Windows.Application.Current.TryFindResource("TextFillColorPrimaryBrush") as SolidColorBrush 
               ?? new SolidColorBrush(Colors.Gray);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>网段背景颜色转换器（选中变红）</summary>
public class NetworkBackgroundBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is FFNetwork currentNetwork && values[1] is FFNetwork selectedNetwork)
        {
            if (currentNetwork == selectedNetwork)
            {
                return new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)); // 选中变红色半透明背景
            }
        }
        // 默认背景色
        return System.Windows.Application.Current.TryFindResource("CardBackgroundFillColorDefaultBrush") as SolidColorBrush 
               ?? new SolidColorBrush(Colors.Transparent);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>模块边框颜色转换器（选中变红）</summary>
public class ModuleBorderBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is FFSlaveModule currentModule && values[1] is FFSlaveModule selectedModule)
        {
            if (currentModule == selectedModule)
            {
                return new SolidColorBrush(Colors.Red); // 选中变红
            }
        }
        // 默认透明
        return new SolidColorBrush(Colors.Transparent);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>模块背景颜色转换器（选中变红）</summary>
public class ModuleBackgroundBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is FFSlaveModule currentModule && values[1] is FFSlaveModule selectedModule)
        {
            if (currentModule == selectedModule)
            {
                return new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)); // 选中变红色半透明背景
            }
        }
        // 默认透明
        return new SolidColorBrush(Colors.Transparent);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>板卡边框颜色转换器（选中变红）</summary>
public class BoardBorderBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is Board currentBoard && values[1] is Board selectedBoard)
        {
            if (currentBoard == selectedBoard)
            {
                return new SolidColorBrush(Colors.Red); // 选中变红
            }
        }
        // 默认普通边框颜色
        return System.Windows.Application.Current.TryFindResource("ControlStrokeColorDefaultBrush") as SolidColorBrush 
               ?? new SolidColorBrush(Colors.Gray);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>板卡背景颜色转换器（选中变红）</summary>
public class BoardBackgroundBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is Board currentBoard && values[1] is Board selectedBoard)
        {
            if (currentBoard == selectedBoard)
            {
                return new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)); // 选中变红色半透明背景
            }
        }
        // 默认背景色
        return System.Windows.Application.Current.TryFindResource("CardBackgroundFillColorDefaultBrush") as SolidColorBrush 
               ?? new SolidColorBrush(Colors.Transparent);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
