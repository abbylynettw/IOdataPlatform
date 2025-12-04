using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using IODataPlatform.Models;
using IODataPlatform.Models.ExcelModels;
using LYSoft.Libs;
using LYSoft.Libs.Wpf.Extensions;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.Common;

/// <summary>
/// CabinetAllocatedPage
/// </summary>
public class CabinetAllocatedPage : Page, INavigableView<CabinetAllocatedViewModel>, IComponentConnector, IStyleConnector
{
	private readonly GlobalModel model;

	internal Slider SliderRedundancy;

	internal StackPanel ColumnsVisibilityPanel;

	internal Wpf.Ui.Controls.DataGrid Data1;

	internal Wpf.Ui.Controls.DataGrid Data2;

	private bool _contentLoaded;

	public CabinetAllocatedViewModel ViewModel { get; }

	public CabinetAllocatedPage(CabinetAllocatedViewModel viewModel, GlobalModel model)
	{
		ViewModel = viewModel;
		this.model = model;
		base.DataContext = this;
		base.Loaded += CabinetAllocatedPage_Loaded;
		InitializeComponent();
	}

	private void CabinetAllocatedPage_Loaded(object sender, RoutedEventArgs e)
	{
		ColumnsVisibilityPanel.Children.Clear();
		CheckBox checkBox = new CheckBox
		{
			Content = "全选",
			IsChecked = false
		};
		checkBox.Checked += delegate
		{
			ColumnsVisibilityPanel.Children.Cast<CheckBox>().AllDo(delegate(CheckBox x)
			{
				x.IsChecked = true;
			});
		};
		checkBox.Unchecked += delegate
		{
			ColumnsVisibilityPanel.Children.Cast<CheckBox>().AllDo(delegate(CheckBox x)
			{
				x.IsChecked = false;
			});
		};
		ColumnsVisibilityPanel.Children.Add(checkBox);
		List<string> columnsInit = new List<string>
		{
			"序号", "就地箱号", "机笼", "插槽", "通道", "板卡类型", "板卡柜内编号", "板卡地址", "供电类型", "标签名称",
			"描述", "备注"
		};
		IEnumerable<DataGridColumn> first = Data1.Columns.Skip(2);
		IEnumerable<DataGridColumn> second = Data2.Columns.Skip(2);
		first.Zip(second).ToObservable().Subscribe(delegate((DataGridColumn First, DataGridColumn Second) x)
		{
			CheckBox checkBox2 = new CheckBox
			{
				Content = x.First.Header,
				IsChecked = columnsInit.Contains($"{x.First.Header}")
			};
			x.First.Visibility = ((!(checkBox2.IsChecked ?? false)) ? Visibility.Collapsed : Visibility.Visible);
			x.Second.Visibility = ((!(checkBox2.IsChecked ?? false)) ? Visibility.Collapsed : Visibility.Visible);
			checkBox2.Checked += delegate
			{
				x.First.Visibility = Visibility.Visible;
				x.Second.Visibility = Visibility.Visible;
			};
			checkBox2.Unchecked += delegate
			{
				x.First.Visibility = Visibility.Collapsed;
				x.Second.Visibility = Visibility.Collapsed;
			};
			ColumnsVisibilityPanel.Children.Add(checkBox2);
		});
	}

	private void BoardDragStart(object sender, MouseEventArgs e)
	{
		e.Handled = true;
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			Board tag = sender.AsTrue<Border>().GetTag<Board>();
			DataObject dataObject = new DataObject();
			dataObject.SetData(tag);
			model.Status.Message("正在拖拽端子板：[" + tag.Type + "]");
			DragDrop.DoDragDrop((DependencyObject)(object)this, dataObject, DragDropEffects.All);
			model.Status.Reset();
		}
	}

	private void PointDragStart(object sender, MouseEventArgs e)
	{
		e.Handled = true;
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			IoFullData tag = sender.AsTrue<Border>().GetTag<IoFullData>();
			DataObject dataObject = new DataObject();
			dataObject.SetData(tag);
			model.Status.Message("正在拖拽点：" + tag.TagName);
			DragDrop.DoDragDrop((DependencyObject)(object)this, dataObject, DragDropEffects.All);
			model.Status.Reset();
		}
	}

	private void ChannelDragOver(object sender, DragEventArgs e)
	{
		e.Handled = true;
		if (e.Data.GetDataPresent(typeof(IoFullData)))
		{
			e.Effects = DragDropEffects.Move;
		}
		else
		{
			e.Effects = DragDropEffects.None;
		}
	}

	private void ChannelDrop(object sender, DragEventArgs e)
	{
		e.Handled = true;
		if (e.Data.GetDataPresent(typeof(IoFullData)))
		{
			Xt2Channel tag = sender.AsTrue<Border>().GetTag<Xt2Channel>();
			ViewModel.Move(GetSelectedPoints(), tag);
		}
	}

	private void UnsetBoardsDragOver(object sender, DragEventArgs e)
	{
		e.Handled = true;
		if (e.Data.GetDataPresent(typeof(Board)))
		{
			e.Effects = DragDropEffects.Move;
		}
		else
		{
			e.Effects = DragDropEffects.None;
		}
	}

	private void UnsetBoardsDrop(object sender, DragEventArgs e)
	{
		e.Handled = true;
		if (e.Data.GetDataPresent(typeof(Board)))
		{
			Board board = e.Data.GetData(typeof(Board)).AsTrue<Board>();
			ViewModel.Unset(board);
		}
	}

	private void UnsetPointsDragOver(object sender, DragEventArgs e)
	{
		e.Handled = true;
		if (e.Data.GetDataPresent(typeof(IoFullData)))
		{
			e.Effects = DragDropEffects.Move;
		}
		else
		{
			e.Effects = DragDropEffects.None;
		}
	}

	private void UnsetPointsDrop(object sender, DragEventArgs e)
	{
		e.Handled = true;
		if (e.Data.GetDataPresent(typeof(IoFullData)))
		{
			ViewModel.Unset(GetSelectedPoints());
		}
	}

	private void DeleteDragOver(object sender, DragEventArgs e)
	{
		e.Handled = true;
		if (!e.Data.GetDataPresent(typeof(Board)))
		{
			e.Effects = DragDropEffects.None;
		}
		else if (e.Data.GetData(typeof(Board)).AsTrue<Board>().Channels.Any((Xt2Channel x) => x.Point != null))
		{
			e.Effects = DragDropEffects.None;
		}
		else
		{
			e.Effects = DragDropEffects.Move;
		}
	}

	private void DeleteDrop(object sender, DragEventArgs e)
	{
		e.Handled = true;
		if (e.Data.GetDataPresent(typeof(Board)))
		{
			Board board = e.Data.GetData(typeof(Board)).AsTrue<Board>();
			ViewModel.Delete(board);
		}
	}

	private void ViewDragOver(object sender, DragEventArgs e)
	{
		e.Handled = true;
		if (e.Data.GetDataPresent(typeof(Board)))
		{
			e.Effects = DragDropEffects.Move;
		}
		else
		{
			e.Effects = DragDropEffects.None;
		}
	}

	private void ViewDrop(object sender, DragEventArgs e)
	{
		e.Handled = true;
		if (e.Data.GetDataPresent(typeof(Board)))
		{
			Board board = e.Data.GetData(typeof(Board)).AsTrue<Board>();
			ViewModel.View(board);
		}
	}

	private void SlotDragOver(object sender, DragEventArgs e)
	{
		e.Handled = true;
		if (e.Data.GetDataPresent(typeof(Board)))
		{
			e.Effects = DragDropEffects.Move;
		}
		else
		{
			e.Effects = DragDropEffects.None;
		}
	}

	private void SlotDrop(object sender, DragEventArgs e)
	{
		e.Handled = true;
		if (e.Data.GetDataPresent(typeof(Board)))
		{
			SlotInfo tag = sender.AsTrue<Border>().GetTag<SlotInfo>();
			Board board = e.Data.GetData(typeof(Board)).AsTrue<Board>();
			ViewModel.Move(board, tag);
		}
	}

	private List<CheckBox> GetAllCheckBox(Wpf.Ui.Controls.DataGrid ctrl)
	{
		List<CheckBox> list = new List<CheckBox>();
		foreach (object item in (IEnumerable)ctrl.Items)
		{
			if (!(ctrl.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow parent))
			{
				continue;
			}
			IEnumerable<CheckBox> enumerable = FindVisualChildren<CheckBox>((DependencyObject)(object)parent);
			foreach (CheckBox item2 in enumerable)
			{
				list.Add(item2);
			}
		}
		return list;
	}

	private List<IoFullData> GetSelectedPoints()
	{
		return (from x in GetAllCheckBox(Data1).Concat(GetAllCheckBox(Data2))
			where x.IsChecked == true
			select x.GetTag<IoFullData>()).ToList();
	}

	public static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
	{
		if (parent == null)
		{
			yield break;
		}
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(parent, i);
			if (child != null)
			{
				T val = (T)(object)((child is T) ? child : null);
				if (val != null)
				{
					yield return val;
				}
			}
			foreach (T item in FindVisualChildren<T>(child))
			{
				yield return item;
			}
		}
	}

	private void CheckBox_Checked(object sender, RoutedEventArgs e)
	{
		GetAllCheckBox(sender.GetTag<Wpf.Ui.Controls.DataGrid>()).AllDo(delegate(CheckBox x)
		{
			x.IsChecked = true;
		});
	}

	private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
	{
		GetAllCheckBox(sender.GetTag<Wpf.Ui.Controls.DataGrid>()).AllDo(delegate(CheckBox x)
		{
			x.IsChecked = false;
		});
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/subpages/common/cabinetallocatedpage.xaml", UriKind.Relative);
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
			SliderRedundancy = (Slider)target;
			break;
		case 2:
			ColumnsVisibilityPanel = (StackPanel)target;
			break;
		case 3:
			((Grid)target).DragOver += DeleteDragOver;
			((Grid)target).Drop += DeleteDrop;
			break;
		case 7:
			((Border)target).DragOver += UnsetBoardsDragOver;
			((Border)target).Drop += UnsetBoardsDrop;
			break;
		case 10:
			((Grid)target).DragOver += ViewDragOver;
			((Grid)target).Drop += ViewDrop;
			break;
		case 11:
			Data1 = (Wpf.Ui.Controls.DataGrid)target;
			break;
		case 12:
			((CheckBox)target).Checked += CheckBox_Checked;
			((CheckBox)target).Unchecked += CheckBox_Unchecked;
			break;
		case 14:
			((Grid)target).DragOver += UnsetPointsDragOver;
			((Grid)target).Drop += UnsetPointsDrop;
			break;
		case 15:
			Data2 = (Wpf.Ui.Controls.DataGrid)target;
			break;
		case 16:
			((CheckBox)target).Checked += CheckBox_Checked;
			((CheckBox)target).Unchecked += CheckBox_Unchecked;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IStyleConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 4:
			((Border)target).DragOver += SlotDragOver;
			((Border)target).Drop += SlotDrop;
			break;
		case 5:
			((Border)target).MouseDown += BoardDragStart;
			break;
		case 6:
			((Border)target).DragOver += ChannelDragOver;
			((Border)target).Drop += ChannelDrop;
			break;
		case 8:
			((Border)target).MouseDown += BoardDragStart;
			break;
		case 9:
			((Border)target).DragOver += ChannelDragOver;
			((Border)target).Drop += ChannelDrop;
			break;
		case 13:
			((Border)target).MouseDown += PointDragStart;
			break;
		case 17:
			((Border)target).MouseDown += PointDragStart;
			break;
		}
	}
}
