using IODataPlatform.Views.Pages;
using System.Windows.Controls;

namespace IODataPlatform.Views.SubPages.AQJ
{
    /// <summary>
    /// ConfigSignalGroupViewModel.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigSignalGroupPage : Page, INavigableView<ConfigSignalGroupViewModel>
    {
        public ConfigSignalGroupViewModel ViewModel { get; }

        public ConfigSignalGroupPage(ConfigSignalGroupViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
