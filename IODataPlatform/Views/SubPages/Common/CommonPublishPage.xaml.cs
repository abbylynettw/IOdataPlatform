namespace IODataPlatform.Views.SubPages.Common;

public partial class CommonPublishPage : INavigableView<PublishViewModel> {

    public PublishViewModel ViewModel { get; }

    public CommonPublishPage(PublishViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}