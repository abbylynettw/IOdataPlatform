namespace IODataPlatform.Views.Pages;

public partial class FormulaEditorPage : INavigableView<FormulaEditorViewModel> {

    public FormulaEditorViewModel ViewModel { get; }

    public FormulaEditorPage(FormulaEditorViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}