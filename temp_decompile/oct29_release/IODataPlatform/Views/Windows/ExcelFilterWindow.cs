using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using IODataPlatform.Utilities;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// ExcelFilterWindow
/// </summary>
public class ExcelFilterWindow : FluentWindow, IComponentConnector
{
	private bool _contentLoaded;

	public ExcelFilter Filter { get; }

	public ExcelFilterWindow(ExcelFilter filter)
	{
		Filter = filter;
		base.DataContext = filter;
		InitializeComponent();
	}

	private void SelectAll_Click(object sender, RoutedEventArgs e)
	{
		Filter.SelectAll();
	}

	private void UnselectAll_Click(object sender, RoutedEventArgs e)
	{
		Filter.UnselectAll();
	}

	private void InverseSelect_Click(object sender, RoutedEventArgs e)
	{
		Filter.InverseSelect();
	}

	private void ConfirmButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = true;
		Close();
	}

	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
		Close();
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/windows/excelfilterwindow.xaml", UriKind.Relative);
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
			((Button)target).Click += SelectAll_Click;
			break;
		case 2:
			((Button)target).Click += UnselectAll_Click;
			break;
		case 3:
			((Button)target).Click += InverseSelect_Click;
			break;
		case 4:
			((Button)target).Click += ConfirmButton_Click;
			break;
		case 5:
			((Button)target).Click += CancelButton_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
