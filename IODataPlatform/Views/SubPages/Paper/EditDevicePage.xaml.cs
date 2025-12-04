namespace IODataPlatform.Views.SubPages.Paper;

public partial class EditDevicePage : INavigableView<EditDeviceViewModel> {

    public EditDeviceViewModel ViewModel { get; }

    public EditDevicePage(EditDeviceViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}