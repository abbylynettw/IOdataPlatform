namespace IODataPlatform.Views.Pages;

public partial class ProjectPage : INavigableView<ProjectViewModel> {
    public ProjectViewModel ViewModel { get; }

    public ProjectPage(ProjectViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}