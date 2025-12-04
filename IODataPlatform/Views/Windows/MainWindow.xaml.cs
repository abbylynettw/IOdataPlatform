using IODataPlatform.Utilities;

namespace IODataPlatform.Views.Windows;

public partial class MainWindow : INavigationWindow {

    public MainWindowViewModel ViewModel { get; }

    public MainWindow(
        MainWindowViewModel viewModel,
        INavigationService navigationService,
        IContentDialogService contentDialogService,
        ISnackbarService snackbarService
    ) {
        ViewModel = viewModel;
        DataContext = this;

        SystemThemeWatcher.Watch(this);

        InitializeComponent();

        contentDialogService.SetContentPresenter(MessageContentDialog);
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);
        navigationService.SetNavigationControl(RootNavigation);
        Closing += OnClosing;
        var state = WindowState;
        var isopen = RootNavigation.IsPaneOpen;

        Messengers.FullScreen.Subscribe(full => {

            if (full) {
                state = WindowState;
                isopen = RootNavigation.IsPaneOpen;

                WindowState = WindowState.Maximized;
                RootNavigation.IsPaneOpen = false;
            } else {
                WindowState = state;
                RootNavigation.IsPaneOpen = isopen;
            }

        });
    }
    private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var result = System.Windows.MessageBox.Show("确认退出？","确认", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != System.Windows.MessageBoxResult.Yes)
        {e.Cancel = true; }
    }
    #region INavigationWindow methods

    public INavigationView GetNavigation() => RootNavigation;

    public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

    public void SetPageService(IPageService pageService) => RootNavigation.SetPageService(pageService);

    public void ShowWindow() => Show();

    public void CloseWindow() => Close();

    #endregion INavigationWindow methods

    protected override void OnClosed(EventArgs e) {
        base.OnClosed(e);
        Application.Current.Shutdown();
    }

    public void SetServiceProvider(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    private void RootNavigation_SelectionChanged(NavigationView sender, RoutedEventArgs args)
    {        
        if (sender.SelectedItem.Content.ToString() == "首页")
        {
            BreadcrumbBar.Visibility = Visibility.Collapsed;
        }
        else BreadcrumbBar.Visibility = Visibility.Visible;
    }

}