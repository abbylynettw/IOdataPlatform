using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

public partial class WordFormatBrushPage : INavigableView<WordFormatBrushViewModel>
{
    public WordFormatBrushViewModel ViewModel { get; }

    public WordFormatBrushPage(WordFormatBrushViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
