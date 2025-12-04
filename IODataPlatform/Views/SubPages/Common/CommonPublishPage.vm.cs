using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.SubPages.Common;

public partial class PublishViewModel(SqlSugarContext context, IMessageService message, INavigationService navigation, GlobalModel model, StorageService storage) : ObservableObject, INavigationAware {

    [ObservableProperty]
    private string title = string.Empty;

    public Func<int?, Task> saveAction { get; set; }
    public Func<int, int, Task> downloadAndCoverAction { get; set; }
    public int? SubProjectId { get; set; } //子项Id

    [ObservableProperty]
    private string? publishedVersion; // 发布版本

    [ObservableProperty]
    private string? publishedReason;  // 发布原因

    [ObservableProperty]
    private string? publisher;        // 发布人      

    [ObservableProperty]
    private ObservableCollection<publish_io>? publishTableData;       // 全部的数据

    public void OnNavigatedFrom() {

    }

    public void OnNavigatedTo() {
        ReloadData();
    }

    private async void ReloadData() {
        var data = await context.Db.Queryable<publish_io>().Where(x => x.SubProjectId == SubProjectId).ToListAsync();
        PublishTableData = [.. data];
    }

    [RelayCommand]
    private async Task Publish() {
        var subProjectId = SubProjectId ?? throw new("开发人员注意");
        if (string.IsNullOrWhiteSpace(PublishedVersion)) { throw new("请输入发布版本号"); }
        if (string.IsNullOrWhiteSpace(PublishedReason)) { throw new("请输入发布原因"); }
        if (string.IsNullOrWhiteSpace(Publisher)) { throw new("请输入发布人"); }

        model.Status.Busy($"正在发布版本……");
        var currentVersions = await context.Db.Queryable<publish_io>().Where(x => x.SubProjectId == SubProjectId).Select(x => x.PublishedVersion).ToListAsync();
        if (currentVersions.Contains(PublishedVersion)) { throw new($"版本已存在：{PublishedVersion}"); }
        var publish = new publish_io() {
            SubProjectId = SubProjectId.Value,
            PublishedVersion = PublishedVersion,
            PublishedTime = DateTime.Now,
            PublishedReason = PublishedReason,
            Publisher = Publisher,
        };
        await context.Db.Insertable(publish).ExecuteCommandIdentityIntoEntityAsync();
        await saveAction(publish.Id);
        model.Status.Success("发布成功");
        model.Status.Reset();
        ReloadData();
    }

    [RelayCommand]
    private async Task ImportPublish(publish_io data) {
        if (!await message.ConfirmAsync("确认操作\r\n此操作使用已发布版本覆盖当前实时数据")) { return; }

        var versions = await context.Db.Queryable<publish_io>().Where(x => x.SubProjectId == SubProjectId).ToListAsync();
        model.Status.Busy($"正在提取发布版本……");
        var versionId = versions.Single(x => x.PublishedVersion == data.PublishedVersion).Id;

        await downloadAndCoverAction(SubProjectId.Value, versionId);

        model.Status.Success("覆盖成功");
    }

    [RelayCommand]
    private async Task DeletePublish(publish_io data)
    {
        if (!await message.ConfirmAsync("是否删除该版本")) { return; }
        await context.Db.Deleteable<publish_io>().Where(x => x.Id == data.Id).ExecuteCommandAsync();
        await storage.DeleteSubprojectPublishFolderAsync(data.SubProjectId, data.Id);
        ReloadData();
        model.Status.Success("删除成功");
        model.Status.Reset();
    }
}