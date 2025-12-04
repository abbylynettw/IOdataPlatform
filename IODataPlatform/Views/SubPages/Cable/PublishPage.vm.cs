using System.IO;

using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using IODataPlatform.Views.Pages;

using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.SubPages.Cable;

public partial class PublishViewModel(CableViewModel cable, SqlSugarContext context, IMessageService message, StorageService storage, INavigationService navigation, GlobalModel model) : ObservableObject, INavigationAware {

    [ObservableProperty]
    private string title = "电缆发布";

    [ObservableProperty]
    private string? publishedVersion; // 发布版本

    [ObservableProperty]
    private string? publishedReason;  // 发布原因

    [ObservableProperty]
    private string? publisher;        // 发布人      

    [ObservableProperty]
    private ObservableCollection<publish_cable>? publishTableData;       // 全部的数据

    public void OnNavigatedFrom() {

    }

    public void OnNavigatedTo() {
        ReloadData();
    }

    private async void ReloadData() {
        var ids = new[] { cable.SubProject1?.Id ?? throw new(), cable.SubProject2?.Id ?? throw new() };

        var data = await context.Db.Queryable<publish_cable>()
            .Where(x => x.SubProjectId1 == ids.Min())
            .Where(x => x.SubProjectId2 == ids.Max())
            .ToListAsync();
        PublishTableData = [.. data];
    }

    [RelayCommand]
    private async Task Publish() {
        var ids = new[] { cable.SubProject1?.Id ?? throw new(), cable.SubProject2?.Id ?? throw new() };
        if (string.IsNullOrWhiteSpace(PublishedVersion)) { throw new("请输入发布版本号"); }
        if (string.IsNullOrWhiteSpace(PublishedReason)) { throw new("请输入发布原因"); }
        if (string.IsNullOrWhiteSpace(Publisher)) { throw new("请输入发布人"); }

        model.Status.Busy($"正在发布版本……");
        var currentVersions = await context.Db.Queryable<publish_cable>()
            .Where(x => x.SubProjectId1 == ids.Min())
            .Where(x => x.SubProjectId2 == ids.Max())
            .Select(x => x.PublishedVersion)
            .ToListAsync();

        if (currentVersions.Contains(PublishedVersion)) { throw new($"版本已存在：{PublishedVersion}"); }

        var publish = new publish_cable() {
            SubProjectId1 = ids.Min(),
            SubProjectId2 = ids.Max(),
            PublishedVersion = PublishedVersion,
            PublishedTime = DateTime.Now,
            PublishedReason = PublishedReason,
            Publisher = Publisher,
        };

        await context.Db.Insertable(publish).ExecuteCommandIdentityIntoEntityAsync();
        await cable.SaveAndUploadRealtimeFileAsync(publish.Id);
        model.Status.Success("发布成功");
        model.Status.Reset();
        ReloadData();
    }

    [RelayCommand]
    private async Task ImportPublish(publish_cable data) {
        if (!await message.ConfirmAsync("确认操作\r\n此操作使用已发布版本覆盖当前实时数据")) { return; }
        model.Status.Busy($"正在提取发布版本……");

        var id1 = data.SubProjectId1;
        var id2 = data.SubProjectId2;
        var versionId = data.Id;
        
        var file = await storage.DownloadPublishCableFileAsync(id1, id2, versionId);
        var relativeRealtimePath = storage.GetRealtimeCableFileRelativePath(id1, id2);
        var absoluteRealtimePath = storage.GetWebFileLocalAbsolutePath(relativeRealtimePath);
        File.Copy(file, absoluteRealtimePath);
        await storage.WebCopyFilesAsync([(storage.GetPublishCableFileRelativePath(id1, id2, versionId), relativeRealtimePath)]);
        await cable.ReloadAllData();

        model.Status.Success("覆盖成功");
        navigation.GoBack();
    }
    [RelayCommand]
    private async Task DeletePublish(publish_cable data)
    {
        if (!await message.ConfirmAsync("是否删除该版本")) { return; }
        await context.Db.Deleteable<publish_cable>().Where(x => x.Id == data.Id).ExecuteCommandAsync();
        await storage.DeleteCableSubprojectPublishFolderAsync(data.SubProjectId1, data.SubProjectId2, data.Id);
        ReloadData();
        model.Status.Success("删除成功");
        model.Status.Reset();
    }
}