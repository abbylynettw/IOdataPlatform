using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Aspose.Pdf;
using IODataPlatform.Models;
using IODataPlatform.Models.Configs;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.AQJ;
using IODataPlatform.Views.SubPages.Common;
using IODataPlatform.Views.SubPages.Paper;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace IODataPlatform.Views.Pages;

public partial class ConfigSignalGroupViewModel(SqlSugarContext context, GlobalModel model,
    StorageService storage, IMessageService message, INavigationService navigation, ConfigTableViewModel configvm,IPickerService picker,ExcelService excel) : ObservableObject, INavigationAware
{

    [ObservableProperty]
    private ObservableCollection<config_aqj_signalGroup>? signalGroups;

    [ObservableProperty]
    private config_aqj_signalGroup? signalGroup;

    public void OnNavigatedFrom() { }

    public async void OnNavigatedTo()
    {
        model.Status.Busy("正在刷新……");
        await RefreshAsync();
        model.Status.Reset();
    }

    [RelayCommand]
    private async Task AddSignalGroup()
    {
        var signalGroup = new config_aqj_signalGroup();
        if (!Edit(signalGroup, "添加信号组")) { return; }
        await context.Db.Insertable(signalGroup).ExecuteCommandIdentityIntoEntityAsync();
        var signalGroupDetails = new List<config_aqj_signalGroupDetail>() {
            new() { signalGroupId = signalGroup.Id, 序号 = string.Empty, 信号名称 = string.Empty },
        };
        await context.Db.Insertable(signalGroupDetails).ExecuteCommandAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task EditSignalGroup(config_aqj_signalGroup signalGroup)
    {
        var signalGroupToEdit = new config_aqj_signalGroup().CopyPropertiesFrom(signalGroup);
        if (!Edit(signalGroupToEdit, "编辑信号组")) { return; }
        await context.Db.Updateable(signalGroupToEdit).ExecuteCommandAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    private void ConfigureSystem(config_aqj_signalGroup signalGroup)
    {
        SignalGroup = signalGroup;
        navigation.NavigateWithHierarchy(typeof(ConfigSignalGroupDetailPage));
    }

    [RelayCommand]
    private async Task DeleteSignalGroup(config_aqj_signalGroup signalGroup)
    {
        if (!await message.ConfirmAsync($"确认删除\"{signalGroup.signalGroupName}\"\r\n此操作会同时删除信号组相关的所有数据，包括信号组详情")) { return; }
        model.Status.Busy("正在获取信号组详情数据……");
        var signalGroupDetails = await context.Db.Queryable<config_aqj_signalGroupDetail>().Where(x => x.signalGroupId == signalGroup.Id).ToListAsync();
        var signalGroupDetailIds = signalGroupDetails.Select(x => x.Id).ToArray();

        model.Status.Busy("正在删除信号组详情数据……");
        foreach (var signalGroupDetailId in signalGroupDetailIds)
        {
            await storage.DeleteSubprojectFolderAsync(signalGroupDetailId); // 假设这个方法可以删除相关文件夹
        }

        await context.Db.Deleteable(signalGroupDetails).ExecuteCommandAsync();
        await context.Db.Deleteable(signalGroup).ExecuteCommandAsync();
        await RefreshAsync();
        model.Status.Reset();
    }

    private static bool Edit(config_aqj_signalGroup obj, string title)
    {
        var builder = obj.CreateEditorBuilder();

        builder.WithTitle(title).WithEditorHeight(300);

        builder.AddProperty<string>(nameof(config_aqj_signalGroup.signalGroupName)).WithHeader("信号组名称"); 
        builder.AddProperty<string>(nameof(config_project.Note)).WithHeader("备注").EditAsText().WithMultiLine();

        return builder.Build().EditWithWpfUI();
    }

    private async Task RefreshAsync()
    {
        SignalGroups = null;
        SignalGroups = [.. await context.Db.Queryable<config_aqj_signalGroup>().ToListAsync()];
    }
    [RelayCommand]
    private async Task ImportFiles()
    {
        //选择文件夹下的所有文件，返回Excel 
        if (picker.PickFolderAndGetFiles("Excel 文件(*.xls; *.xlsx)|*.xls; *.xlsx") is not string[] files) { return; }
        model.Status.Busy("开始导入");
        foreach (var filePath in files)
        {
            try
            {
                var groupName = picker.GetFileNameWithoutExtension(filePath);

                // 查找或插入组名，并返回其对象
                var signalGroup = await context.Db.Queryable<config_aqj_signalGroup>()
                                                  .FirstAsync(g => g.signalGroupName == groupName)
                                  ?? new config_aqj_signalGroup { signalGroupName = groupName };

                if (signalGroup.Id == 0) // 如果新创建的组名对象还没有 Id，则插入到数据库
                {
                    await context.Db.Insertable(signalGroup).ExecuteCommandIdentityIntoEntityAsync();
                }
                //插入该组详情信息
                using var dataTable = await excel.GetDataTableAsStringAsync(filePath, true);
                var data = await Task.Run(() => dataTable.StringTableToIEnumerableByDiplay<config_aqj_signalGroupDetail>());
                var list = data.Select(item => {
                    item.signalGroupId = signalGroup!.Id;
                    return item;
                }).ToList();

                // 先删除，再新增
                await context.Db.Deleteable<config_aqj_signalGroupDetail>().Where(d => d.signalGroupId == signalGroup!.Id).ExecuteCommandAsync();
                await context.Db.Insertable(list).ExecuteCommandAsync();
                await RefreshAsync();
            }
            catch (Exception)
            {
                continue;
            }
           
        }
        model.Status.Success($"导入成功，共导入{files.Count()}种信号组");
    }
}
