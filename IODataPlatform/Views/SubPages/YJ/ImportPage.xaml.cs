namespace IODataPlatform.Views.SubPages.YJ;

public partial class ImportPage : INavigableView<ImportViewModel> {

    public ImportViewModel ViewModel { get; }

    public ImportPage(ImportViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}