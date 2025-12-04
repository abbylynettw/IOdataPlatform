using System.IO;

using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.Common;
using IODataPlatform.Views.SubPages.YJ;

using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.Pages;

public partial class DepYJViewModel(SqlSugarContext context, GlobalModel model, ConfigTableViewModel configvm, IMessageService message, IContentDialogService dialog, 
    INavigationService navigation, StorageService storage, ExcelService excel, IPickerService picker) : ObservableObject, INavigationAware {

    private bool isInit = false;

    public void OnNavigatedFrom() {

    }

    public async void OnNavigatedTo() {

        if (!isInit)
        {
            await RefreshProjects();
            isInit = true;
        }
        else
        {
            try { await ReloadAllData(); } catch { }
        }
    }

    [ObservableProperty]
    private ObservableCollection<IoData>? allData;       // 全部的数据

    [ObservableProperty]
    private int allDataCount;

    partial void OnAllDataChanged(ObservableCollection<IoData>? value) {
        AllDataCount = AllData?.Count ?? 0;
    }

    /// <summary>保存当前AllData并上传实时文件，如需要发布，同时传入versionId参数</summary>
    /// <param name="versionId">发布版本ID</param>
    public async Task SaveAndUploadFileAsync(int? versionId = null) {
        _ = Project?.Id ?? throw new("开发人员注意");
        var subProjectId = SubProject?.Id ?? throw new("开发人员注意");
        var allData = AllData ?? throw new("开发人员注意");

        var relativePath = storage.GetRealtimeIoFileRelativePath(subProjectId);
        var absolutePath = storage.GetWebFileLocalAbsolutePath(relativePath);

        using var dataTable = await allData.ToTableByDisplayAttributeAsync();
        await excel.FastExportAsync(dataTable, absolutePath);
        await storage.UploadRealtimeIoFileAsync(subProjectId);

        if (versionId == null) { return; }
        await storage.WebCopyFilesAsync([(relativePath, storage.GetPublishIoFileRelativePath(subProjectId, versionId.Value))]);
    }

    /// <summary>从服务器下载实时数据文件并加载</summary>
    private async Task ReloadAllData() {
        AllData = null;
        _ = Project?.Id ?? throw new("开发人员注意");
        var subProjectId = SubProject?.Id ?? throw new("开发人员注意");
        try {
            var file = await storage.DownloadRealtimeIoFileAsync(subProjectId);
            using var dataTable = await excel.GetDataTableAsStringAsync(file, true);
            var list = await Task.Run(() => dataTable.StringTableToIEnumerableByDiplay<IoData>());
            AllData = [.. list];
        } catch {
            AllData = [];
        }
    }

    [RelayCommand]
    private void EditConfigurationTable(string param)
    {
        configvm.Title = param;
        configvm.DataType = param switch
        {
            "典回配置表" => typeof(config_termination_yjs),            
            _ => throw new NotImplementedException(),
        };
        navigation.NavigateWithHierarchy(typeof(ConfigTablePage));
    }

}