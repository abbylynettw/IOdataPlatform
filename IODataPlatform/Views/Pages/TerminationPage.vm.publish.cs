using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Views.SubPages.Common;
using Microsoft.Extensions.DependencyInjection;

using Wpf.Ui.Extensions;

namespace IODataPlatform.Views.Pages;

// 发布部分

partial class TerminationViewModel {


    [RelayCommand]
    private async Task Publish()
    {
        var projectId = Project?.Id ?? throw new("开发人员注意");
        var subProjectId = SubProject?.Id ?? throw new("开发人员注意");
        var allData = AllData ?? throw new("开发人员注意");

        publishvm.Title = "端接发布";
        publishvm.SubProjectId = subProjectId;
        publishvm.saveAction = SaveAndUploadRealtimeFileAsync;
        publishvm.downloadAndCoverAction = DownloadAndCover;
        navigation.NavigateWithHierarchy(typeof(CommonDuanjiePublishPage));
    }

    private async Task DownloadAndCover(int subProjectId, int versionId)
    {
        var relativePath = storage.GetRealtimeTerminationFileRelativePath(subProjectId);
        await storage.WebCopyFilesAsync([(storage.GetPublishTerminationFileRelativePath(subProjectId, versionId), relativePath)]);
    }
}