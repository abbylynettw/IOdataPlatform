namespace IODataPlatform.Views.SubPages.Paper;

public partial class UploadPaperPage : INavigableView<UploadPaperViewModel> {

    public UploadPaperViewModel ViewModel { get; }

    public UploadPaperPage(UploadPaperViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}