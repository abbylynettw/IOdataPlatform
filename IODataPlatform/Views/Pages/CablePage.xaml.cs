using IODataPlatform.Models.ExcelModels;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace IODataPlatform.Views.Pages;

public partial class CablePage : INavigableView<CableViewModel> {

    public CableViewModel ViewModel { get; }

    public CablePage(CableViewModel viewModel) {
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

        List<string> columnsInit = new List<string> {
            nameof(CableData.序号),
            nameof(CableData.线缆编号),
            nameof(CableData.线缆列别),
            nameof(CableData.色标),
            nameof(CableData.特性代码),
            nameof(CableData.芯线对数号),
            nameof(CableData.芯线号),
            nameof(CableData.起点房间号),
            nameof(CableData.起点盘柜名称),
            nameof(CableData.起点安全分级分组),
            nameof(CableData.起点系统号),
            nameof(CableData.起点IO类型),
            nameof(CableData.起点设备名称),
            nameof(CableData.起点接线点1),
            nameof(CableData.起点接线点2),
            nameof(CableData.起点信号位号),
            nameof(CableData.终点房间号),
            nameof(CableData.终点盘柜名称),
            nameof(CableData.终点设备名称),
            nameof(CableData.终点安全分级分组),
            nameof(CableData.终点系统号),
            nameof(CableData.终点IO类型),
            nameof(CableData.终点接线点1),
            nameof(CableData.终点接线点2),
            nameof(CableData.终点信号位号),
            nameof(CableData.IO类型),
            nameof(CableData.信号说明),
            nameof(CableData.供货方),
            nameof(CableData.版本),
            nameof(CableData.备注),
            nameof(CableData.匹配情况),
            nameof(CableData.StartNumber),
            nameof(CableData.SystemNo),
            nameof(CableData.起点专业),
            nameof(CableData.终点专业)
        };


        Data.Columns.ToObservable().Subscribe(x =>{
            var check = new CheckBox() { Content = x.Header, IsChecked = columnsInit.Contains($"{x.Header}") };
            x.Visibility = check.IsChecked.GetValueOrDefault(false) ? Visibility.Visible : Visibility.Collapsed;
            check.Checked += (_, _) => x.Visibility = Visibility.Visible;
            check.Unchecked += (_, _) => x.Visibility = Visibility.Collapsed;
            ColumnsVisibilityPanel.Children.Add(check);
        });
    }
}