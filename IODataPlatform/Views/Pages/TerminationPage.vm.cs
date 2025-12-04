﻿using System.IO;

using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.Common;
using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 端接管理页面视图模型类
/// 负责端接数据的全生命周期管理，包括数据导入、编辑、发布和文件同步功能
/// 支持实时数据处理和版本化发布机制，确保数据的准确性和可追溯性
/// 实现INavigationAware接口以支持页面导航和数据刷新机制
/// </summary>
public partial class TerminationViewModel(SqlSugarContext context, INavigationService navigation,
    GlobalModel model, IMessageService message, IContentDialogService dialog, StorageService storage,
    ExcelService excel, IPickerService picker,PublishTerminationViewModel publishvm) : ObservableObject, INavigationAware {

    /// <summary>页面初始化状态标记，防止重复初始化操作</summary>
    private bool isInit = false;

    /// <summary>
    /// 页面导航离开时触发
    /// 当前实现为空，预留用于后续数据保存或清理操作
    /// </summary>
    public void OnNavigatedFrom() {

    }

    /// <summary>
    /// 页面导航到此页面时触发
    /// 首次访问时执行项目刷新和初始化操作
    /// 非首次访问时尝试重新加载数据，确保数据的时效性
    /// </summary>
    public async void OnNavigatedTo() {
        if (!isInit) {
            await RefreshProjects();
            isInit = true;    
        } else {
            try { await ReloadAllData(); } catch { }
        }
    }

    /// <summary>当前页面的所有端接数据集合，用于界面显示和数据操作</summary>
    [ObservableProperty]
    private ObservableCollection<TerminationData>? allData;

    /// <summary>
    /// 保存并上传实时端接文件，支持可选的版本发布
    /// 将当前AllData导出为Excel文件并上传到服务器
    /// 如果提供versionId参数，同时创建发布版本的副本用于正式发布
    /// </summary>
    /// <param name="versionId">可选的发布版本标识符，如果提供则同时创建发布版本</param>
    /// <returns>异步任务，表示保存和上传操作的完成</returns>
    /// <exception cref="Exception">当必要的项目或子项目信息缺失时抛出异常</exception>
    public async Task SaveAndUploadRealtimeFileAsync(int? versionId = null) {
        _ = Project?.Id ?? throw new("开发人员注意");
        var subProjectId = SubProject?.Id ?? throw new("开发人员注意");
        var allData = AllData ?? throw new("开发人员注意");

        // 获取实时文件路径并导出Excel
        var relativePath = storage.GetRealtimeTerminationFileRelativePath(subProjectId);
        var absolutePath = storage.GetWebFileLocalAbsolutePath(relativePath);

        using var dataTable = await allData.ToTableByDisplayAttributeAsync();
        await excel.FastExportAsync(dataTable, absolutePath);
        await storage.UploadRealtimeTerminationFileAsync(subProjectId);

        // 如果指定了版本号，则创建发布版本
        if (versionId == null) { return; }
        var publishRelativePath = storage.GetPublishTerminationFileRelativePath(subProjectId, versionId.Value);
        var publishAbsolutePath = storage.GetWebFileLocalAbsolutePath(publishRelativePath);
        File.Copy(absolutePath, publishAbsolutePath, true);
        await storage.UploadPublishTerminationFileAsync(subProjectId, versionId.Value);
    }

    /// <summary>
    /// 从服务器下载并加载实时端接数据
    /// 清除当前数据，从服务器下载最新的实时数据文件并解析为对象集合
    /// 包含完整的错误处理，当文件不存在或解析失败时返回空集合
    /// </summary>
    /// <returns>异步任务，表示数据加载操作的完成</returns>
    /// <exception cref="Exception">当必要的项目信息缺失时抛出异常</exception>
    private async Task ReloadAllData() {
        AllData = null;
        _ = Project?.Id ?? throw new("开发人员注意");
        var subProjectId = SubProject?.Id ?? throw new("开发人员注意");
        try {
            // 下载并解析Excel文件
            var file = await storage.DownloadRealtimeTerminationFileAsync(subProjectId);
            using var dataTable = await excel.GetDataTableAsStringAsync(file, true);
            var list = await Task.Run(() => new ObservableCollection<TerminationData>(dataTable.StringTableToIEnumerableByDiplay<TerminationData>()));
            AllData = list;
        } catch {
            // 异常时返回空集合，保证界面正常显示
            AllData = [];
        }
    }    

}