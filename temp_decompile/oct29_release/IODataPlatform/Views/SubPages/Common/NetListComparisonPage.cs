using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.Common;

/// <summary>
/// NetListComparisonPage
/// </summary>
public class NetListComparisonPage : Page, INavigableView<NetListComparisonViewModel>, IComponentConnector, IStyleConnector
{
	private bool _contentLoaded;

	public NetListComparisonViewModel ViewModel { get; }

	public NetListComparisonPage(NetListComparisonViewModel viewModel)
	{
		ViewModel = viewModel;
		base.DataContext = this;
		InitializeComponent();
	}

	/// <summary>
	/// 显示旧数据按钮点击事件
	/// </summary>
	private void ShowOldDataButton_Click(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement { DataContext: CableComparisonRow dataContext })
		{
			dataContext.ShowOldData = !dataContext.ShowOldData;
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/subpages/common/netlistcomparisonpage.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		_contentLoaded = true;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IStyleConnector.Connect(int connectionId, object target)
	{
		if (connectionId == 1)
		{
			((System.Windows.Controls.Button)target).Click += ShowOldDataButton_Click;
		}
	}
}
