namespace IODataPlatform.Views.SubPages.Common;

public partial class ExtractPdfPage {

    public ExtractPdfViewModel ViewModel { get; }

    public ExtractPdfPage(ExtractPdfViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}