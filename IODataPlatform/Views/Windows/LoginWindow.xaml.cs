using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

using WpfAnimatedGif;

namespace IODataPlatform.Views.Windows;

public partial class LoginWindow : INavigableView<LoginWindowViewModel> {
    public LoginWindowViewModel ViewModel { get; }

    public LoginWindow(LoginWindowViewModel viewModel, ISnackbarService snackbarService) {
        ViewModel = viewModel;
        DataContext = this;

        SystemThemeWatcher.Watch(this);

        InitializeComponent();
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);

        Closed += (_, _) => Application.Current.Shutdown();
        #if DEBUG
        return;
        #endif
        //// 加载GIF文件并设置为循环播放
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri("../../Assets/loginbg3.gif", UriKind.Relative);
        bitmap.EndInit();
        ImageBehavior.SetAnimatedSource(gifImage, bitmap);
        ImageBehavior.SetAnimationSpeedRatio(gifImage, 0.5);
        ImageBehavior.SetRepeatBehavior(gifImage, RepeatBehavior.Forever);

    }

    private void Grid_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
        if (e.Key == System.Windows.Input.Key.Enter) { ViewModel.LoginCommand.Execute(null); }
    }
}
