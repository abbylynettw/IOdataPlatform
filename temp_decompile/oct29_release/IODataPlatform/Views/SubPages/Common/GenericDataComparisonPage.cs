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
/// GenericDataComparisonPage
/// </summary>
public class GenericDataComparisonPage : Page, INavigableView<GenericDataComparisonViewModel>, IComponentConnector
{
	internal Wpf.Ui.Controls.DataGrid ComparisonDataGrid;

	private bool _contentLoaded;

	public GenericDataComparisonViewModel ViewModel { get; }

	public GenericDataComparisonPage(GenericDataComparisonViewModel viewModel)
	{
		ViewModel = viewModel;
		base.DataContext = this;
		InitializeComponent();
	}

	private void ComparisonDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/subpages/common/genericdatacomparisonpage.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		if (connectionId == 1)
		{
			ComparisonDataGrid = (Wpf.Ui.Controls.DataGrid)target;
		}
		else
		{
			_contentLoaded = true;
		}
	}
}
