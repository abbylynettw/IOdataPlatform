using System.Reactive.Linq;
using System.Windows.Controls;

namespace IODataPlatform.Views.Pages;

public partial class DataAssetCenterPage : INavigableView<DataAssetCenterViewModel> {

    public DataAssetCenterViewModel ViewModel { get; }

    public DataAssetCenterPage(DataAssetCenterViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {

    }
}