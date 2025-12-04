using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Shapes;
using IODataPlatform.Models.ExportModels;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.Export;

/// <summary>
/// 可拖拽的列排序面板控件
/// 支持通过拖拽调整列的显示顺序
/// </summary>
/// <summary>
/// DraggableColumnPanel
/// </summary>
public class DraggableColumnPanel : UserControl, IComponentConnector
{
	/// <summary>
	/// 列信息集合
	/// </summary>
	public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(ObservableCollection<ColumnInfo>), typeof(DraggableColumnPanel), new PropertyMetadata((object)null, new PropertyChangedCallback(OnColumnsChanged)));

	private readonly List<DraggableColumnItem> _columnItems = new List<DraggableColumnItem>();

	private DraggableColumnItem? _draggingItem;

	private Point _dragStartPoint;

	private bool _isDragging;

	private int _insertIndex = -1;

	private const double ItemWidth = 170.0;

	private const double ItemHeight = 45.0;

	private const double ItemSpacingX = 8.0;

	private const double ItemSpacingY = 8.0;

	private const int ColumnsPerRow = 5;

	internal Canvas ColumnCanvas;

	internal Canvas DragPreviewCanvas;

	internal Border DragPreview;

	internal Wpf.Ui.Controls.TextBlock DragPreviewText;

	internal Canvas DropIndicatorCanvas;

	internal Rectangle DropIndicator;

	private bool _contentLoaded;

	/// <summary>
	/// 列信息集合
	/// </summary>
	public ObservableCollection<ColumnInfo> Columns
	{
		get
		{
			return (ObservableCollection<ColumnInfo>)((DependencyObject)this).GetValue(ColumnsProperty);
		}
		set
		{
			((DependencyObject)this).SetValue(ColumnsProperty, (object)value);
		}
	}

	/// <summary>
	/// 初始化DraggableColumnPanel
	/// </summary>
	public DraggableColumnPanel()
	{
		InitializeComponent();
	}

	/// <summary>
	/// 列集合变化时的处理
	/// </summary>
	private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is DraggableColumnPanel draggableColumnPanel)
		{
			draggableColumnPanel.UpdateColumnsDisplay();
		}
	}

	/// <summary>
	/// 更新列显示
	/// </summary>
	private void UpdateColumnsDisplay()
	{
		RefreshColumnItems();
	}

	/// <summary>
	/// 刷新列项目显示
	/// </summary>
	private void RefreshColumnItems()
	{
		ColumnCanvas.Children.Clear();
		_columnItems.Clear();
		if (Columns != null)
		{
			List<ColumnInfo> list = (from c in Columns
				where c.IsVisible
				orderby c.Order
				select c).ToList();
			for (int num = 0; num < list.Count; num++)
			{
				ColumnInfo columnInfo = list[num];
				DraggableColumnItem draggableColumnItem = new DraggableColumnItem(columnInfo, num, list.Count);
				draggableColumnItem.MoveUpRequested += OnMoveUpRequested;
				draggableColumnItem.MoveDownRequested += OnMoveDownRequested;
				draggableColumnItem.MoveToTopRequested += OnMoveToTopRequested;
				draggableColumnItem.MoveToBottomRequested += OnMoveToBottomRequested;
				int num2 = num / 5;
				int num3 = num % 5;
				double length = (double)num3 * 178.0 + 10.0;
				double length2 = (double)num2 * 53.0 + 10.0;
				Canvas.SetLeft(draggableColumnItem, length);
				Canvas.SetTop(draggableColumnItem, length2);
				draggableColumnItem.Width = 170.0;
				draggableColumnItem.Height = 45.0;
				ColumnCanvas.Children.Add(draggableColumnItem);
				_columnItems.Add(draggableColumnItem);
			}
			int num4 = (list.Count + 5 - 1) / 5;
			ColumnCanvas.Width = 910.0;
			ColumnCanvas.Height = Math.Max((double)num4 * 53.0 + 20.0, 100.0);
		}
	}

	/// <summary>
	/// 向上移动请求
	/// </summary>
	private void OnMoveUpRequested(object sender, EventArgs e)
	{
		if (sender is DraggableColumnItem draggableColumnItem)
		{
			ColumnInfo columnInfo = draggableColumnItem.ColumnInfo;
			int num = Columns.IndexOf(columnInfo);
			if (num > 0)
			{
				Columns.Move(num, num - 1);
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
		if (sender is DraggableColumnItem draggableColumnItem)
		{
			ColumnInfo columnInfo = draggableColumnItem.ColumnInfo;
			int num = Columns.IndexOf(columnInfo);
			if (num < Columns.Count - 1)
			{
				Columns.Move(num, num + 1);
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
		if (sender is DraggableColumnItem draggableColumnItem)
		{
			ColumnInfo columnInfo = draggableColumnItem.ColumnInfo;
			int num = Columns.IndexOf(columnInfo);
			if (num > 0)
			{
				Columns.Move(num, 0);
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
		if (sender is DraggableColumnItem draggableColumnItem)
		{
			ColumnInfo columnInfo = draggableColumnItem.ColumnInfo;
			int num = Columns.IndexOf(columnInfo);
			if (num < Columns.Count - 1)
			{
				Columns.Move(num, Columns.Count - 1);
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

	/// <summary>
	/// InitializeComponent
	/// </summary>
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/subpages/export/draggablecolumnpanel.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			ColumnCanvas = (Canvas)target;
			break;
		case 2:
			DragPreviewCanvas = (Canvas)target;
			break;
		case 3:
			DragPreview = (Border)target;
			break;
		case 4:
			DragPreviewText = (Wpf.Ui.Controls.TextBlock)target;
			break;
		case 5:
			DropIndicatorCanvas = (Canvas)target;
			break;
		case 6:
			DropIndicator = (Rectangle)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
