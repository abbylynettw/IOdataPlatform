using System.IO;

using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.Cable;
using IODataPlatform.Views.SubPages.Common;
using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 电缆管理页面视图模型类
/// 负责电缆数据的全生命周期管理，包括数据导入、编辑、匹配、发布等核心功能
/// 支持多子项目的电缆数据对比和实时同步，提供丰富的配置管理功能
/// 实现INavigationAware接口以支持页面生命周期管理和数据刷新
/// </summary>
public partial class CableViewModel(SqlSugarContext context, INavigationService navigation, ConfigTableViewModel configvm,
    GlobalModel model, IMessageService message, IContentDialogService dialog, StorageService storage,
    ExcelService excel, IPickerService picker) : ObservableObject, INavigationAware {

    /// <summary>页面初始化状态标记，防止重复初始化</summary>
    private bool isInit = false;

    /// <summary>
    /// 页面导航离开时触发
    /// 当前实现为空，预留用于后续数据保存或清理操作
    /// </summary>
    public void OnNavigatedFrom() {

    }

    /// <summary>
    /// 页面导航到此页面时触发
    /// 首次访问时执行初始化操作，加载项目列表
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

    /// <summary>当前页面的所有电缆数据集合，用于界面显示和编辑</summary>
    [ObservableProperty]
    private ObservableCollection<CableData>? allData;       // 全部的数据

    /// <summary>
    /// 保存并上传实时文件，支持可选的版本发布
    /// 将当前AllData导出为Excel文件并上传到服务器
    /// 如果提供versionId参数，同时创建发布版本的副本
    /// </summary>
    /// <param name="versionId">可选的发布版本标识符，如果提供则同时创建发布版本</param>
    /// <returns>异步任务，表示保存和上传操作的完成</returns>
    /// <exception cref="Exception">当必要的项目或子项目信息缺失时抛出异常</exception>
    public async Task SaveAndUploadRealtimeFileAsync(int? versionId = null) {
        _ = Project?.Id ?? throw new("开发人员注意");
        var subProjectId1 = SubProject1?.Id ?? throw new("开发人员注意");
        var subProjectId2 = SubProject2?.Id ?? throw new("开发人员注意");
        var allData = AllData ?? throw new("开发人员注意");

        // 获取实时文件路径并导出Excel
        var relativePath = storage.GetRealtimeCableFileRelativePath(subProjectId1, subProjectId2);
        var absolutePath = storage.GetWebFileLocalAbsolutePath(relativePath);

        using var dataTable = await allData.ToTableByDisplayAttributeAsync();
        await excel.FastExportAsync(dataTable, absolutePath);
        await storage.UploadRealtimeCableFileAsync(subProjectId1, subProjectId2);

        // 如果指定了版本号，则创建发布版本
        if (versionId == null) { return; }
        var publishRelativePath = storage.GetPublishCableFileRelativePath(subProjectId1, subProjectId2, versionId.Value);
        var publishAbsolutePath = storage.GetWebFileLocalAbsolutePath(publishRelativePath);
        File.Copy(absolutePath, publishAbsolutePath, true);
        await storage.WebCopyFilesAsync([(relativePath, publishRelativePath)]);
    }

    /// <summary>
    /// 从服务器下载并加载实时电缆数据
    /// 清除当前数据，从服务器下载最新的实时数据文件并解析为对象集合
    /// 包含完整的错误处理，当文件不存在或解析失败时返回空集合
    /// </summary>
    /// <returns>异步任务，表示数据加载操作的完成</returns>
    /// <exception cref="Exception">当必要的项目信息缺失时抛出异常</exception>
    public async Task ReloadAllData() {
        AllData = null;
        _ = Project?.Id ?? throw new("开发人员注意");
        var subProjectId1 = SubProject1?.Id;
        var subProjectId2 = SubProject2?.Id;
        if (subProjectId1 == null || subProjectId2 == null) { return; }
        
        try {
            // 下载并解析Excel文件
            var file = await storage.DownloadRealtimeCableFileAsync(subProjectId1.Value, subProjectId2.Value);
            using var dataTable = await excel.GetDataTableAsStringAsync(file, true);
            var list = await Task.Run(dataTable.StringTableToIEnumerableByDiplay<CableData>);
            AllData = [.. list];
        } catch {
            // 异常时返回空集合，保证界面正常显示
            AllData = [];
        }
    }

    /// <summary>
    /// 刷新数据命令
    /// 手动触发数据重新加载，包含状态提示和错误处理
    /// 用户可通过此命令获取最新的服务器数据
    /// </summary>
    /// <returns>异步任务，表示刷新操作的完成</returns>
    [RelayCommand]
    private async Task Refresh() {
        model.Status.Busy("正在刷新……");
        await ReloadAllData();
        model.Status.Reset();
    }

    /// <summary>
    /// 跳转到电缆匹配页面命令
    /// 导航到专用的电缆匹配功能页面，提供高级的数据匹配和对比功能
    /// </summary>
    /// <returns>异步任务，表示导航操作的完成</returns>
    [RelayCommand]
    private async Task GoToMatchPage()
    {
        await Task.Delay(1);
        navigation.NavigateWithHierarchy(typeof(MatchPage));
    }

    /// <summary>
    /// 编辑配置表命令
    /// 根据传入参数决定要编辑的配置表类型，并导航到相应的配置编辑页面
    /// 支持电缆相关的多种配置表，包括类别、规格、编号等
    /// </summary>
    /// <param name="param">配置表类型名称，用于决定编辑哪个配置表</param>
    /// <exception cref="NotImplementedException">当参数不匹配任何已知配置表类型时抛出</exception>
    [RelayCommand]
    private void EditConfigurationTable(string param)
    {
        configvm.Title = param;
        configvm.DataType = param switch
        {
            "配置线缆列别及色标" => typeof(config_cable_categoryAndColor),
            "配置电缆特性代码" => typeof(config_cable_spec),
            "配置电缆的起始流水号" => typeof(config_cable_startNumber),
            "配置电缆的系统号" => typeof(config_cable_systemNumber),
            "配置电缆IO类型" => typeof(config_cable_function),
            _ => throw new NotImplementedException(),
        };
        navigation.NavigateWithHierarchy(typeof(ConfigTablePage));
    }

}