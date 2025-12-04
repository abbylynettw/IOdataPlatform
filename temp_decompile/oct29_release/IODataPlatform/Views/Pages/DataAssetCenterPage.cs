using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using LYSoft.Libs.Wpf.WpfUI.Custom;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// DataAssetCenterPage
/// </summary>
public class DataAssetCenterPage : Page, INavigableView<DataAssetCenterViewModel>, IComponentConnector
{
	internal CustomDataGrid grid;

	internal CustomDataGrid Data;

	private bool _contentLoaded;

	public DataAssetCenterViewModel ViewModel { get; }

	public DataAssetCenterPage(DataAssetCenterViewModel viewModel)
	{
		ViewModel = viewModel;
		base.DataContext = this;
		InitializeComponent();
	}

	private void Hyperlink_Click(object sender, RoutedEventArgs e)
	{
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/pages/dataassetcenterpage.xaml", UriKind.Relative);
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
			grid = (CustomDataGrid)target;
			break;
		case 2:
			Data = (CustomDataGrid)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
