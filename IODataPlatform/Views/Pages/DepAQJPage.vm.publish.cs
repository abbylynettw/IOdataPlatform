using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Views.SubPages.Common;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Extensions;

namespace IODataPlatform.Views.Pages;

// 发布部分

partial class DepAQJViewModel {

    [RelayCommand]
    private async Task Publish() {
        var projectId = Project?.Id ?? throw new("开发人员注意");
        var subProjectId = SubProject?.Id ?? throw new("开发人员注意");
        var allData = AllData ?? throw new("开发人员注意");

        publishvm.Title = "安全级室IO发布";
        publishvm.SubProjectId = subProjectId;      
        publishvm.saveAction = SaveAndUploadFileAsync;
        publishvm.downloadAndCoverAction = DownloadAndCover;
        navigation.NavigateWithHierarchy(typeof(CommonPublishPage));              
    }

    private async Task DownloadAndCover(int subProjectId,int versionId)
    {
        var relativePath = storage.GetRealtimeIoFileRelativePath(subProjectId);
        await storage.WebCopyFilesAsync([(storage.GetPublishIoFileRelativePath(subProjectId, versionId), relativePath)]);
    }
}