namespace IODataPlatform.Views.SubPages.Termination;

public partial class ImportPage : INavigableView<ImportViewModel> {

    public ImportViewModel ViewModel { get; }

    public ImportPage(ImportViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}