using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using IODataPlatform.Utilities;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// MainWindow
/// </summary>
public class MainWindow : FluentWindow, INavigationWindow, IComponentConnector
{
	internal NavigationView RootNavigation;

	internal BreadcrumbBar BreadcrumbBar;

	internal ContentPresenter MessageContentDialog;

	internal SnackbarPresenter SnackbarPresenter;

	internal TitleBar TitleBar;

	private bool _contentLoaded;

	public MainWindowViewModel ViewModel { get; }

	public MainWindow(MainWindowViewModel viewModel, INavigationService navigationService, IContentDialogService contentDialogService, ISnackbarService snackbarService)
	{
		MainWindow mainWindow = this;
		ViewModel = viewModel;
		base.DataContext = this;
		SystemThemeWatcher.Watch(this);
		InitializeComponent();
		contentDialogService.SetContentPresenter(MessageContentDialog);
		snackbarService.SetSnackbarPresenter(SnackbarPresenter);
		navigationService.SetNavigationControl(RootNavigation);
		base.Closing += OnClosing;
		WindowState state = base.WindowState;
		bool isopen = RootNavigation.IsPaneOpen;
		Messengers.FullScreen.Subscribe(delegate(bool full)
		{
			if (full)
			{
				state = mainWindow.WindowState;
				isopen = mainWindow.RootNavigation.IsPaneOpen;
				mainWindow.WindowState = WindowState.Maximized;
				mainWindow.RootNavigation.IsPaneOpen = false;
			}
			else
			{
				mainWindow.WindowState = state;
				mainWindow.RootNavigation.IsPaneOpen = isopen;
			}
		});
	}

	private void OnClosing(object sender, CancelEventArgs e)
	{
		System.Windows.MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("确认退出？", "确认", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
		if (messageBoxResult != System.Windows.MessageBoxResult.Yes)
		{
			e.Cancel = true;
		}
	}

	public INavigationView GetNavigation()
	{
		return RootNavigation;
	}

	public bool Navigate(Type pageType)
	{
		return RootNavigation.Navigate(pageType);
	}

	public void SetPageService(IPageService pageService)
	{
		RootNavigation.SetPageService(pageService);
	}

	public void ShowWindow()
	{
		Show();
	}

	public void CloseWindow()
	{
		Close();
	}

	protected override void OnClosed(EventArgs e)
	{
		base.OnClosed(e);
		Application.Current.Shutdown();
	}

	public void SetServiceProvider(IServiceProvider serviceProvider)
	{
		throw new NotImplementedException();
	}

	private void RootNavigation_SelectionChanged(NavigationView sender, RoutedEventArgs args)
	{
		if (sender.SelectedItem.Content.ToString() == "首页")
		{
			BreadcrumbBar.Visibility = Visibility.Collapsed;
		}
		else
		{
			BreadcrumbBar.Visibility = Visibility.Visible;
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/windows/mainwindow.xaml", UriKind.Relative);
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
			RootNavigation = (NavigationView)target;
			break;
		case 2:
			BreadcrumbBar = (BreadcrumbBar)target;
			break;
		case 3:
			MessageContentDialog = (ContentPresenter)target;
			break;
		case 4:
			SnackbarPresenter = (SnackbarPresenter)target;
			break;
		case 5:
			TitleBar = (TitleBar)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
