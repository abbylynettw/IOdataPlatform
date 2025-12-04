namespace IODataPlatform.Views.SubPages.XT2;

public partial class ImportPage : INavigableView<ImportViewModel> {

    public ImportViewModel ViewModel { get; }

    public ImportPage(ImportViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}