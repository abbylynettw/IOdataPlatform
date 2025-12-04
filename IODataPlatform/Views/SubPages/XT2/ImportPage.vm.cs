using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;
using IODataPlatform.Views.SubPages.Common;
using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.SubPages.XT2;

public partial class ImportViewModel(SqlSugarContext context, IMessageService message, GlobalModel model, DepXT2ViewModel xt2, ExcelService excel, IPickerService picker, INavigationService navigation,IContentDialogService dialogService, SelectExcelSheetDialogViewModel vm) : ObservableObject, INavigationAware {

    public void OnNavigatedFrom() { }

    public void OnNavigatedTo() {
        _ = xt2.Project ?? throw new("开发人员注意");
        _ = xt2.SubProject ?? throw new("开发人员注意");
        _ = xt2.AllData ?? throw new("开发人员注意");
        oldData = [.. xt2.AllData];
    }

    public ObservableCollection<DifferentObject<string>> DiffObjects { get; } = [];
    public ObservableCollection<DifferentProperty> DiffProps { get; } = [];

    private List<IoFullData>? oldData;
    private readonly List<IoFullData> newData = [];

    [RelayCommand]
    private async Task ImportFile()
    {
        _ = oldData ?? throw new();
        _ = xt2.Major ?? throw new();
        _ = xt2.Project ?? throw new();

        var viewModel = App.GetService<SelectExcelSheetDialogViewModel>();
        
        // 设置当前系统信息
        viewModel.SetCurrentSystemInfo(xt2.Major.ControlSystem);
        
        var termsOfUseContentDialog = new SelectExcelSheetDialog(viewModel, dialogService.GetContentPresenter());

        ContentDialogResult result = await termsOfUseContentDialog.ShowAsync();
        if (result!= ContentDialogResult.Primary) return;

        model.Status.Busy("正在获取数据……");
        DiffObjects.Clear();
        DiffProps?.Clear();


        using var dataTable = await excel.GetDataTableAsStringAsync(vm.SelectFilePath, vm.SelectedSheetName, true);
      
        newData.Reset(dataTable.ConvertOldDataTableToIoFullData(message,context.Db, xt2.Major.ControlSystem));

        //model.Status.Busy("正在比对数据……");
        //DiffObjects.Reset(await DataComparator.ComparerAsync(newData, oldData, x => $"{x.SignalPositionNumber}_{x.ExtensionCode}"));
        model.Status.Reset();
        
        // 提示用户取消对比，直接提交
        await message.AlertAsync ("文件加载完成！\n\n当前已取消对比功能，请直接点击提交导入。");
    }

    [RelayCommand]
    private void ViewData(DifferentObject<string> obj) {
        DiffProps.Reset(obj.DiffProps);
    }

    [RelayCommand]
    private async Task Confirm() {
        if (!await message.ConfirmAsync("确认操作\r\n将会覆盖之前子项的全部内容")) { return; }
        model.Status.Success("正在提交数据……");
        var configs = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
        xt2.AllData = [.. newData.OrderBy(n => n.CabinetNumber)];
        await xt2.SaveAndUploadFileAsync();
        //await xt2.Recalc();
        model.Status.Success("导入成功");
        navigation.GoBack();
    }

    [RelayCommand]
    private void ConvertBack() {
        navigation.GoBack();
    }

}
