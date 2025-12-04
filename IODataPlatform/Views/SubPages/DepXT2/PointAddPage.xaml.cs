using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.DepXT2;

public partial class PointAddPage : INavigableView<PointAddViewModel>
{
    public PointAddViewModel ViewModel { get; }

    public PointAddPage(PointAddViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
