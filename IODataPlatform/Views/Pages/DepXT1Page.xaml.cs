using System.Reactive.Linq;
using System.Windows.Controls;

namespace IODataPlatform.Views.Pages;

public partial class DepXT1Page : INavigableView<DepXT1ViewModel> {

    public DepXT1ViewModel ViewModel { get; set; }

    public DepXT1Page(DepXT1ViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += DepXT1Page_Loaded;
    }

    private void DepXT1Page_Loaded(object sender, RoutedEventArgs e) {

        ColumnsVisibilityPanel.Children.Clear();

        var checkAll = new CheckBox() { Content = "全选", IsChecked = false };
        checkAll.Checked += (_, _) => ColumnsVisibilityPanel.Children.Cast<CheckBox>().AllDo(x => x.IsChecked = true);
        checkAll.Unchecked += (_, _) => ColumnsVisibilityPanel.Children.Cast<CheckBox>().AllDo(x => x.IsChecked = false);
        ColumnsVisibilityPanel.Children.Add(checkAll);

        var columnsInit = new List<string> { "房间号", "标签名","原变量名","原扩展码","信号说明","房间号","盘箱柜号","IO类型","IO卡型号","IO卡编号", "安全分级/分组", "端子排编号", "接线点1","接线点2" };

        grid.Columns.ToObservable().Subscribe(x => {
            var check = new CheckBox() { Content = x.Header, IsChecked = columnsInit.Contains($"{x.Header}") };
            x.Visibility = check.IsChecked.GetValueOrDefault(false) ? Visibility.Visible : Visibility.Collapsed;
            check.Checked += (_, _) => x.Visibility = Visibility.Visible;
            check.Unchecked += (_, _) => x.Visibility = Visibility.Collapsed;
            ColumnsVisibilityPanel.Children.Add(check);
        });
    }

}
