namespace IODataPlatform.Views.SubPages.XT2;

public partial class UploadExcelDataPage {

    public UploadExcelDataViewModel ViewModel { get; }

    public UploadExcelDataPage(UploadExcelDataViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}