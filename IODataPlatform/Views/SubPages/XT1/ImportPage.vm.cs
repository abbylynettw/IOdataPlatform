using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;
using IODataPlatform.Views.SubPages.Common;
using LYSoft.Libs.ServiceInterfaces;
using System;
using System.Reflection;

namespace IODataPlatform.Views.SubPages.XT1
{
    public partial class ImportViewModel(SqlSugarContext context, IMessageService message, GlobalModel model, DepXT1ViewModel xt1, ExcelService excel, IPickerService picker, INavigationService navigation, IContentDialogService dialogService, SelectExcelSheetDialogViewModel vm) : ObservableObject, INavigationAware
    {

        public void OnNavigatedFrom() { }

        public void OnNavigatedTo()
        {
            _ = xt1.Project ?? throw new("开发人员注意");
            _ = xt1.SubProject ?? throw new("开发人员注意");
            _ = xt1.AllData ?? throw new("开发人员注意");
            oldData = [.. xt1.AllData];
        }

        public ObservableCollection<DifferentObject<string>> DiffObjects { get; } = [];
        public ObservableCollection<DifferentProperty> DiffProps { get; } = [];

        private List<IoFullData>? oldData;
        private readonly List<IoFullData> newData = [];

        [RelayCommand]
        private async Task ImportFile()
        {

            var viewModel = App.GetService<SelectExcelSheetDialogViewModel>();
            var termsOfUseContentDialog = new SelectExcelSheetDialog(viewModel, dialogService.GetContentPresenter());

            ContentDialogResult result = await termsOfUseContentDialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            model.Status.Busy("正在获取数据……");
            DiffObjects.Clear();
            DiffProps?.Clear();


            using var dataTable = await excel.GetDataTableAsStringAsync(vm.SelectFilePath, vm.SelectedSheetName, true);
            var controsystem = context.Db.Queryable<config_project_major>()
                                 .Where(it => it.Id == xt1.SubProject.MajorId).First().ControlSystem;
            newData.Reset(dataTable.ConvertOldDataTableToIoFullData(message, context.Db, controsystem));


            model.Status.Busy("正在比对数据……");
            DiffObjects.Reset(await DataComparator.ComparerAsync(newData, oldData, x => $"{x.TagName}"));
            model.Status.Reset();
        }

        [RelayCommand]
        private void ViewData(DifferentObject<string> obj)
        {
            DiffProps.Reset(obj.DiffProps);
        }

        [RelayCommand]
        private async Task Confirm()
        {
            if (!await message.ConfirmAsync("确认操作\r\n将会覆盖之前子项的全部内容")) { return; }
            model.Status.Success("正在提交数据……");
            xt1.AllData = [.. newData];
            await xt1.SaveAndUploadFileAsync();

            model.Status.Success("导入成功");
            navigation.GoBack();
        }

        [RelayCommand]
        private void ConvertBack()
        {
            navigation.GoBack();
        }

    }
}
