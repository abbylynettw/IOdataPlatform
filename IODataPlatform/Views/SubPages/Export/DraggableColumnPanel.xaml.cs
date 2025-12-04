using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using IODataPlatform.Models.ExportModels;

namespace IODataPlatform.Views.SubPages.Export;

/// <summary>
/// 可拖拽的列排序面板控件
/// 支持通过拖拽调整列的显示顺序
/// </summary>
public partial class DraggableColumnPanel : UserControl
{
    #region 依赖属性

    /// <summary>
    /// 列信息集合
    /// </summary>
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(ObservableCollection<ColumnInfo>), 
            typeof(DraggableColumnPanel), new PropertyMetadata(null, OnColumnsChanged));

    /// <summary>
    /// 列信息集合
    /// </summary>
    public ObservableCollection<ColumnInfo> Columns
    {
        get => (ObservableCollection<ColumnInfo>)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    #endregion

    #region 私有字段

    private readonly List<DraggableColumnItem> _columnItems = new();
    private DraggableColumnItem? _draggingItem;
    private Point _dragStartPoint;
    private bool _isDragging;
    private int _insertIndex = -1;
    
    // 布局常量
    private const double ItemWidth = 170;
    private const double ItemHeight = 45;
    private const double ItemSpacingX = 8;
    private const double ItemSpacingY = 8;
    private const int ColumnsPerRow = 5;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化DraggableColumnPanel
    /// </summary>
    public DraggableColumnPanel()
    {
        InitializeComponent();
    }

    #endregion

    #region 事件处理

    /// <summary>
    /// 列集合变化时的处理
    /// </summary>
    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DraggableColumnPanel panel)
        {
            panel.UpdateColumnsDisplay();
        }
    }

    /// <summary>
    /// 更新列显示
    /// </summary>
    private void UpdateColumnsDisplay()
    {
        // 直接调用刷新方法，因为我们使用Canvas而不是ItemsControl
        RefreshColumnItems();
    }

    #endregion


    #region 私有方法

    /// <summary>
    /// 刷新列项目显示
    /// </summary>
    private void RefreshColumnItems()
    {
        ColumnCanvas.Children.Clear();
        _columnItems.Clear();
        if (Columns == null) return;

        var visibleColumns = Columns.Where(c => c.IsVisible).OrderBy(c => c.Order).ToList();
        
        for (int i = 0; i < visibleColumns.Count; i++)
        {
            var column = visibleColumns[i];
            var item = new DraggableColumnItem(column, i, visibleColumns.Count);
            item.MoveUpRequested += OnMoveUpRequested;
            item.MoveDownRequested += OnMoveDownRequested;
            item.MoveToTopRequested += OnMoveToTopRequested;
            item.MoveToBottomRequested += OnMoveToBottomRequested;

            // 计算网格位置
            int row = i / ColumnsPerRow;
            int col = i % ColumnsPerRow;
            
            double x = col * (ItemWidth + ItemSpacingX) + 10;
            double y = row * (ItemHeight + ItemSpacingY) + 10;

            Canvas.SetLeft(item, x);
            Canvas.SetTop(item, y);
            
            // 设置固定大小
            item.Width = ItemWidth;
            item.Height = ItemHeight;
            
            ColumnCanvas.Children.Add(item);
            _columnItems.Add(item);
        }

        // 更新Canvas尺寸
        int totalRows = (visibleColumns.Count + ColumnsPerRow - 1) / ColumnsPerRow;
        ColumnCanvas.Width = ColumnsPerRow * (ItemWidth + ItemSpacingX) + 20;
        ColumnCanvas.Height = Math.Max(totalRows * (ItemHeight + ItemSpacingY) + 20, 100);
    }

    #region 箭头按钮事件处理

    /// <summary>
    /// 向上移动请求
    /// </summary>
    private void OnMoveUpRequested(object sender, EventArgs e)
    {
        if (sender is DraggableColumnItem item)
        {
            var column = item.ColumnInfo;
            var currentIndex = Columns.IndexOf(column);
            
            if (currentIndex > 0)
            {
                Columns.Move(currentIndex, currentIndex - 1);
                UpdateOrderProperties();
                RefreshColumnItems();
            }
        }
    }

    /// <summary>
    /// 向下移动请求
    /// </summary>
    private void OnMoveDownRequested(object sender, EventArgs e)
    {
        if (sender is DraggableColumnItem item)
        {
            var column = item.ColumnInfo;
            var currentIndex = Columns.IndexOf(column);
            
            if (currentIndex < Columns.Count - 1)
            {
                Columns.Move(currentIndex, currentIndex + 1);
                UpdateOrderProperties();
                RefreshColumnItems();
            }
        }
    }

    /// <summary>
    /// 置顶请求
    /// </summary>
    private void OnMoveToTopRequested(object sender, EventArgs e)
    {
        if (sender is DraggableColumnItem item)
        {
            var column = item.ColumnInfo;
            var currentIndex = Columns.IndexOf(column);
            
            if (currentIndex > 0)
            {
                Columns.Move(currentIndex, 0);
                UpdateOrderProperties();
                RefreshColumnItems();
            }
        }
    }

    /// <summary>
    /// 置底请求
    /// </summary>
    private void OnMoveToBottomRequested(object sender, EventArgs e)
    {
        if (sender is DraggableColumnItem item)
        {
            var column = item.ColumnInfo;
            var currentIndex = Columns.IndexOf(column);
            
            if (currentIndex < Columns.Count - 1)
            {
                Columns.Move(currentIndex, Columns.Count - 1);
                UpdateOrderProperties();
                RefreshColumnItems();
            }
        }
    }

    /// <summary>
    /// 更新Order属性
    /// </summary>
    private void UpdateOrderProperties()
    {
        for (int i = 0; i < Columns.Count; i++)
        {
            Columns[i].Order = i;
        }
    }

    #endregion

   

    #endregion
}

/// <summary>
/// 可排序的列项目控件
/// </summary>
internal class DraggableColumnItem : Border
{
    /// <summary>
    /// 关联的列信息
    /// </summary>
    public ColumnInfo ColumnInfo { get; }

    /// <summary>
    /// 向上移动事件
    /// </summary>
    public event EventHandler MoveUpRequested;

    /// <summary>
    /// 向下移动事件
    /// </summary>
    public event EventHandler MoveDownRequested;

    /// <summary>
    /// 置顶事件
    /// </summary>
    public event EventHandler MoveToTopRequested;

    /// <summary>
    /// 置底事件
    /// </summary>
    public event EventHandler MoveToBottomRequested;

    /// <summary>
    /// 初始化DraggableColumnItem
    /// </summary>
    /// <param name="columnInfo">列信息</param>
    /// <param name="currentIndex">当前索引</param>
    /// <param name="totalCount">总数量</param>
    public DraggableColumnItem(ColumnInfo columnInfo, int currentIndex, int totalCount)
    {
        ColumnInfo = columnInfo;
        
        // 卡片样式
        Margin = new Thickness(1);
        Padding = new Thickness(4);
        BorderBrush = (Brush)Application.Current.Resources["ControlElevationBorderBrush"];
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(3);
        Background = (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"];

        // 创建水平布局
        var mainGrid = new Grid();
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // 列名显示
        var nameText = new Wpf.Ui.Controls.TextBlock
        {
            Text = columnInfo.DisplayName,
            FontWeight = FontWeights.Medium,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetColumn(nameText, 0);
        mainGrid.Children.Add(nameText);

        // 按钮容器
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(2, 0, 0, 0)
        };
        Grid.SetColumn(buttonPanel, 1);

        // 向上按钮
        var upButton = new Wpf.Ui.Controls.Button
        {
            Content = new Wpf.Ui.Controls.SymbolIcon 
            { 
                Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowUp20,
                FontSize = 12
            },
            Width = 18,
            Height = 18,
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            ToolTip = "向上",
            IsEnabled = currentIndex > 0
        };
        upButton.Click += (s, e) => MoveUpRequested?.Invoke(this, EventArgs.Empty);
        buttonPanel.Children.Add(upButton);

        // 向下按钮
        var downButton = new Wpf.Ui.Controls.Button
        {
            Content = new Wpf.Ui.Controls.SymbolIcon 
            { 
                Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowDown20,
                FontSize = 12
            },
            Width = 18,
            Height = 18,
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            ToolTip = "向下",
            IsEnabled = currentIndex < totalCount - 1
        };
        downButton.Click += (s, e) => MoveDownRequested?.Invoke(this, EventArgs.Empty);
        buttonPanel.Children.Add(downButton);

        // 置顶按钮
        var topButton = new Wpf.Ui.Controls.Button
        {
            Content = new Wpf.Ui.Controls.SymbolIcon 
            { 
                Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowStepOut20,
                FontSize = 12
            },
            Width = 18,
            Height = 18,
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            ToolTip = "置顶",
            IsEnabled = currentIndex > 0
        };
        topButton.Click += (s, e) => MoveToTopRequested?.Invoke(this, EventArgs.Empty);
        buttonPanel.Children.Add(topButton);

        // 置底按钮
        var bottomButton = new Wpf.Ui.Controls.Button
        {
            Content = new Wpf.Ui.Controls.SymbolIcon 
            { 
                Symbol = Wpf.Ui.Controls.SymbolRegular.ArrowStepIn20,
                FontSize = 12
            },
            Width = 18,
            Height = 18,
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            ToolTip = "置底",
            IsEnabled = currentIndex < totalCount - 1
        };
        bottomButton.Click += (s, e) => MoveToBottomRequested?.Invoke(this, EventArgs.Empty);
        buttonPanel.Children.Add(bottomButton);

        mainGrid.Children.Add(buttonPanel);
        Child = mainGrid;

        // 添加鼠标悬停效果
        MouseEnter += (s, e) =>
        {
            Background = (Brush)Application.Current.Resources["SubtleFillColorSecondaryBrush"];
            BorderBrush = (Brush)Application.Current.Resources["AccentTextFillColorPrimaryBrush"];
        };

        MouseLeave += (s, e) =>
        {
            Background = (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"];
            BorderBrush = (Brush)Application.Current.Resources["ControlElevationBorderBrush"];
        };
    }
}
