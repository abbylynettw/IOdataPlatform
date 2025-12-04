namespace IODataPlatform.Views.SubPages.Cable;

public partial class PublishPage : INavigableView<PublishViewModel> {

    public PublishViewModel ViewModel { get; }

    public PublishPage(PublishViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}