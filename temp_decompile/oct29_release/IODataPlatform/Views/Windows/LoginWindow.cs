using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using WpfAnimatedGif;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// LoginWindow
/// </summary>
public class LoginWindow : FluentWindow, INavigableView<LoginWindowViewModel>, IComponentConnector
{
	internal System.Windows.Controls.Image gifImage;

	internal SnackbarPresenter SnackbarPresenter;

	private bool _contentLoaded;

	public LoginWindowViewModel ViewModel { get; }

	public LoginWindow(LoginWindowViewModel viewModel, ISnackbarService snackbarService)
	{
		ViewModel = viewModel;
		base.DataContext = this;
		SystemThemeWatcher.Watch(this);
		InitializeComponent();
		snackbarService.SetSnackbarPresenter(SnackbarPresenter);
		base.Closed += delegate
		{
			Application.Current.Shutdown();
		};
		BitmapImage bitmapImage = new BitmapImage();
		bitmapImage.BeginInit();
		bitmapImage.UriSource = new Uri("../../Assets/loginbg3.gif", UriKind.Relative);
		bitmapImage.EndInit();
		ImageBehavior.SetAnimatedSource(gifImage, bitmapImage);
		ImageBehavior.SetAnimationSpeedRatio((DependencyObject)(object)gifImage, 0.5);
		ImageBehavior.SetRepeatBehavior(gifImage, RepeatBehavior.Forever);
	}

	private void Grid_KeyUp(object sender, KeyEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)e.Key == 6)
		{
			ViewModel.LoginCommand.Execute(null);
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
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/views/windows/loginwindow.xaml", UriKind.Relative);
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
			((Grid)target).KeyUp += Grid_KeyUp;
			break;
		case 2:
			gifImage = (System.Windows.Controls.Image)target;
			break;
		case 3:
			SnackbarPresenter = (SnackbarPresenter)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
