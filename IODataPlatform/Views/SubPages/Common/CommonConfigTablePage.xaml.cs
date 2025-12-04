using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace IODataPlatform.Views.SubPages.Common;

public partial class ConfigTablePage : INavigableView<ConfigTableViewModel> {

    public ConfigTableViewModel ViewModel { get; }

    public ConfigTablePage(ConfigTableViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }    
}