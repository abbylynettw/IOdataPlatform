namespace IODataPlatform.Views.Pages;

public partial class PaperPage : INavigableView<PaperViewModel> {

    public PaperViewModel ViewModel { get; }

    public PaperPage(PaperViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}