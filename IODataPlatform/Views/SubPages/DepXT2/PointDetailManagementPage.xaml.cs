using System.Windows.Controls;

namespace IODataPlatform.Views.SubPages.DepXT2;

public partial class PointDetailManagementPage : Page
{
    public PointDetailManagementPage(PointDetailManagementViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    public PointDetailManagementViewModel ViewModel { get; }
}
