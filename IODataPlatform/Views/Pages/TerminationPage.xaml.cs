using System.Reactive.Linq;
using System.Windows.Controls;

namespace IODataPlatform.Views.Pages;

public partial class TerminationPage : INavigableView<TerminationViewModel> {

    public TerminationViewModel ViewModel { get; }

    public TerminationPage(TerminationViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
        Loaded += DepXT2Page_Loaded;
    }

    private void DepXT2Page_Loaded(object sender, RoutedEventArgs e) {

        ColumnsVisibilityPanel.Children.Clear();

        var checkAll = new CheckBox() { Content = "全选", IsChecked = false };
        checkAll.Checked += (_, _) => ColumnsVisibilityPanel.Children.Cast<CheckBox>().AllDo(x => x.IsChecked = true);
        checkAll.Unchecked += (_, _) => ColumnsVisibilityPanel.Children.Cast<CheckBox>().AllDo(x => x.IsChecked = false);
        ColumnsVisibilityPanel.Children.Add(checkAll);

        var columnsInit = new List<string> { "I/O点名","原变量名", "信号说明", "盘箱柜号", "I/O类型", "接线板型号", "安全分级/分组", "I/O卡型号", "I/O卡编号", "接线点1","信号说明1", "接线点2","信号说明2", "去向专业" };

        Data.Columns.ToObservable().Subscribe(x =>{
            var check = new CheckBox() { Content = x.Header, IsChecked = columnsInit.Contains($"{x.Header}") };
            x.Visibility = check.IsChecked.GetValueOrDefault(false) ? Visibility.Visible : Visibility.Collapsed;
            check.Checked += (_, _) => x.Visibility = Visibility.Visible;
            check.Unchecked += (_, _) => x.Visibility = Visibility.Collapsed;
            ColumnsVisibilityPanel.Children.Add(check);
        });
    }

}