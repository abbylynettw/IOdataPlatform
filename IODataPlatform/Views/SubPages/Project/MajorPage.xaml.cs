namespace IODataPlatform.Views.SubPages.Project;

public partial class MajorPage : INavigableView<MajorViewModel>
{

    public MajorViewModel ViewModel { get; }

    public MajorPage(MajorViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

}