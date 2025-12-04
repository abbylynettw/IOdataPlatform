namespace IODataPlatform.Views.SubPages.Cable;

public partial class MatchPage : INavigableView<MatchViewModel> {

    public MatchViewModel ViewModel { get; }

    public MatchPage(MatchViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}