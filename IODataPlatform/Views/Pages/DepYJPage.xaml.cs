using System.Reactive.Linq;
using System.Windows.Controls;
namespace IODataPlatform.Views.Pages;

public partial class DepYJPage : INavigableView<DepYJViewModel> {

    public DepYJViewModel ViewModel { get; }

    public DepYJPage(DepYJViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += DepYJPage_Loaded;
    }

    private void DepYJPage_Loaded(object sender, RoutedEventArgs e)
    {
        ColumnsVisibilityPanel.Children.Clear();

        var checkAll = new CheckBox() { Content = "全选", IsChecked = false };
        checkAll.Checked += (_, _) => ColumnsVisibilityPanel.Children.Cast<CheckBox>().AllDo(x => x.IsChecked = true);
        checkAll.Unchecked += (_, _) => ColumnsVisibilityPanel.Children.Cast<CheckBox>().AllDo(x => x.IsChecked = false);
        ColumnsVisibilityPanel.Children.Add(checkAll);

        var columnsInit = new List<string> { "房间号", "标签名", "原变量名", "原扩展码", "信号说明", "盘箱柜号", "供电类型", "IO类型", "IO卡型号", "IO卡编号", "去向专业" };

        grid1.Columns.ToObservable().Subscribe(x => {
            var check = new CheckBox() { Content = x.Header, IsChecked = columnsInit.Contains($"{x.Header}") };
            x.Visibility = check.IsChecked.GetValueOrDefault(false) ? Visibility.Visible : Visibility.Collapsed;
            check.Checked += (_, _) => x.Visibility = Visibility.Visible;
            check.Unchecked += (_, _) => x.Visibility = Visibility.Collapsed;
            ColumnsVisibilityPanel.Children.Add(check);
        });
    }
}