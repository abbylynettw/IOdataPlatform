using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;

using Microsoft.Extensions.DependencyInjection;

using Wpf.Ui.Extensions;

namespace IODataPlatform.Views.Pages;

// 发布部分

partial class DepYJViewModel {

    [RelayCommand]
    private async Task Publish() {
        var projectId = Project?.Id ?? throw new("开发人员注意");
        var subProjectId = SubProject?.Id ?? throw new("开发人员注意");
        var allData = AllData ?? throw new("开发人员注意");
        var textbox = new TextBox() { VerticalAlignment = VerticalAlignment.Center, PlaceholderText = "请输入发布版本号", Width = 180 };

        var result = await dialog.ShowSimpleDialogAsync(
            new SimpleContentDialogCreateOptions() {
                Title = "发布",
                Content = textbox,
                PrimaryButtonText = "发布",
                CloseButtonText = "取消",
            }
        );

        if (result != ContentDialogResult.Primary) { return; }
        if (string.IsNullOrWhiteSpace(textbox.Text)) { throw new("请输入发布版本号"); }
        var version = textbox.Text;

        if (string.IsNullOrWhiteSpace(version)) { }
        model.Status.Busy($"正在发布版本……");
        var currentVersions = await context.Db.Queryable<publish_io>().Where(x => x.SubProjectId == SubProject.Id).Select(x => x.PublishedVersion).ToListAsync();
        if (currentVersions.Contains(version)) { throw new($"版本已存在：{version}"); }
        var publish = new publish_io() {
            SubProjectId = SubProject.Id,
            PublishedVersion = version,
        };
        await context.Db.Insertable(publish).ExecuteCommandIdentityIntoEntityAsync();
        await SaveAndUploadFileAsync(publish.Id);

        model.Status.Success("发布成功");
    }

}