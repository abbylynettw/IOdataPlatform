using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using IODataPlatform.Models;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// OtherFunctionPage.xaml 的交互逻辑
/// </summary>
/// <summary>
/// OtherFunctionPage
/// </summary>
public class OtherFunctionPage : Page, INavigableView<OtherFunctionViewModel>, IComponentConnector
{
	private bool _contentLoaded;

	public OtherFunctionViewModel ViewModel { get; }

	public OtherFunctionPage(OtherFunctionViewModel viewModel, GlobalModel model)
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/pages/otherfunctionpage.xaml", UriKind.Relative);
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
