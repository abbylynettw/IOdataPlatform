using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IODataPlatform.Models.ExportModels;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.Export;

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
		base.Margin = new Thickness(1.0);
		base.Padding = new Thickness(4.0);
		base.BorderBrush = (Brush)Application.Current.Resources["ControlElevationBorderBrush"];
		base.BorderThickness = new Thickness(1.0);
		base.CornerRadius = new CornerRadius(3.0);
		base.Background = (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"];
		Grid grid = new Grid
		{
			ColumnDefinitions = 
			{
				new ColumnDefinition
				{
					Width = new GridLength(1.0, GridUnitType.Star)
				},
				new ColumnDefinition
				{
					Width = GridLength.Auto
				}
			}
		};
		Wpf.Ui.Controls.TextBlock element = new Wpf.Ui.Controls.TextBlock
		{
			Text = columnInfo.DisplayName,
			FontWeight = FontWeights.Medium,
			FontSize = 12.0,
			TextWrapping = TextWrapping.Wrap,
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Left
		};
		Grid.SetColumn(element, 0);
		grid.Children.Add(element);
		StackPanel stackPanel = new StackPanel
		{
			Orientation = Orientation.Horizontal,
			VerticalAlignment = VerticalAlignment.Center,
			Margin = new Thickness(2.0, 0.0, 0.0, 0.0)
		};
		Grid.SetColumn(stackPanel, 1);
		Wpf.Ui.Controls.Button button = new Wpf.Ui.Controls.Button
		{
			Content = new SymbolIcon
			{
				Symbol = SymbolRegular.ArrowUp20,
				FontSize = 12.0
			},
			Width = 18.0,
			Height = 18.0,
			Padding = new Thickness(0.0),
			Margin = new Thickness(0.0),
			ToolTip = "向上",
			IsEnabled = (currentIndex > 0)
		};
		button.Click += delegate
		{
			this.MoveUpRequested?.Invoke(this, EventArgs.Empty);
		};
		stackPanel.Children.Add(button);
		Wpf.Ui.Controls.Button button2 = new Wpf.Ui.Controls.Button
		{
			Content = new SymbolIcon
			{
				Symbol = SymbolRegular.ArrowDown20,
				FontSize = 12.0
			},
			Width = 18.0,
			Height = 18.0,
			Padding = new Thickness(0.0),
			Margin = new Thickness(0.0),
			ToolTip = "向下",
			IsEnabled = (currentIndex < totalCount - 1)
		};
		button2.Click += delegate
		{
			this.MoveDownRequested?.Invoke(this, EventArgs.Empty);
		};
		stackPanel.Children.Add(button2);
		Wpf.Ui.Controls.Button button3 = new Wpf.Ui.Controls.Button
		{
			Content = new SymbolIcon
			{
				Symbol = SymbolRegular.ArrowStepOut20,
				FontSize = 12.0
			},
			Width = 18.0,
			Height = 18.0,
			Padding = new Thickness(0.0),
			Margin = new Thickness(0.0),
			ToolTip = "置顶",
			IsEnabled = (currentIndex > 0)
		};
		button3.Click += delegate
		{
			this.MoveToTopRequested?.Invoke(this, EventArgs.Empty);
		};
		stackPanel.Children.Add(button3);
		Wpf.Ui.Controls.Button button4 = new Wpf.Ui.Controls.Button
		{
			Content = new SymbolIcon
			{
				Symbol = SymbolRegular.ArrowStepIn20,
				FontSize = 12.0
			},
			Width = 18.0,
			Height = 18.0,
			Padding = new Thickness(0.0),
			Margin = new Thickness(0.0),
			ToolTip = "置底",
			IsEnabled = (currentIndex < totalCount - 1)
		};
		button4.Click += delegate
		{
			this.MoveToBottomRequested?.Invoke(this, EventArgs.Empty);
		};
		stackPanel.Children.Add(button4);
		grid.Children.Add(stackPanel);
		Child = grid;
		base.MouseEnter += delegate
		{
			base.Background = (Brush)Application.Current.Resources["SubtleFillColorSecondaryBrush"];
			base.BorderBrush = (Brush)Application.Current.Resources["AccentTextFillColorPrimaryBrush"];
		};
		base.MouseLeave += delegate
		{
			base.Background = (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"];
			base.BorderBrush = (Brush)Application.Current.Resources["ControlElevationBorderBrush"];
		};
	}
}
