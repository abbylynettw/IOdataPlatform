using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using IODataPlatform.Views.Pages;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.AQJ;

/// <summary>
/// ConfigSignalGroupViewModel.xaml 的交互逻辑
/// </summary>
/// <summary>
/// ConfigSignalGroupPage
/// </summary>
public class ConfigSignalGroupPage : Page, INavigableView<ConfigSignalGroupViewModel>, IComponentConnector
{
	private bool _contentLoaded;

	public ConfigSignalGroupViewModel ViewModel { get; }

	public ConfigSignalGroupPage(ConfigSignalGroupViewModel viewModel)
	{
		ViewModel = viewModel;
		base.DataContext = this;
		InitializeComponent();
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/subpages/aqj/configsignalgrouppage.xaml", UriKind.Relative);
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
}
