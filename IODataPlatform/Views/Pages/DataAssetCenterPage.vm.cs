﻿﻿using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.Common;
using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 数据资产中心页面视图模型类
/// 负责已发布数据的集中展示和管理，支持IO、端接、电缆三种数据类型
/// 提供已发布版本的数据浏览、下载和配置管理功能
/// 实现不同数据类型的统一显示接口，支持动态类型转换
/// </summary>
public partial class DataAssetCenterViewModel(SqlSugarContext context,
    GlobalModel model, IMessageService message, StorageService storage,
    ExcelService excel, IPickerService picker, ConfigTableViewModel configvm, INavigationService navigation) : ObservableObject, INavigationAware {

    /// <summary>页面初始化状态标记，防止重复初始化操作</summary>
    private bool isInit = false;

    /// <summary>
    /// 页面导航离开时触发
    /// 当前实现为空，预留用于后续数据清理操作
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

    /// <summary>当前页面显示的所有数据集合，支持多种数据类型的统一存储</summary>
    [ObservableProperty]
    private ObservableCollection<object>? allData;
    
    /// <summary>当前数据的下载链接URL，用于用户直接下载原始文件</summary>
    [ObservableProperty]
    private string? downloadUrl;

    /// <summary>
    /// 从服务器下载并加载已发布的数据文件
    /// 根据选中的数据类型（IO、端接、电缆）动态获取对应的发布记录
    /// 支持不同数据类型的自动转换和统一显示，同时生成下载链接
    /// </summary>
    /// <returns>异步任务，表示数据加载操作的完成</returns>
    /// <exception cref="Exception">当必要的项目参数缺失时抛出异常</exception>
    /// <exception cref="NotSupportedException">当遇到不支持的数据类型时抛出异常</exception>
    private async Task ReloadAllData() {
        AllData = null;
        var subProjectId = SubProject1?.Id ?? throw new("开发人员注意");
        var publishId = PublishVersion?? throw new("开发人员注意");
        string file = "";
        
        // 根据数据类型获取对应的发布记录和文件
        switch (Category)
        {
            case "IO":
                var publishIo = context.Db.Queryable<publish_io>().Where(x => x.SubProjectId == SubProject1.Id && x.PublishedVersion == PublishVersion).ToList().FirstOrDefault();
                if (publishIo == null) return;
                file = await storage.DownloadPublishIoFileAsync(subProjectId, publishIo.Id); 
                DownloadUrl = storage.GetFileDownloadUrl(storage.GetPublishIoFileRelativePath(subProjectId, publishIo.Id)); break;
            case "端接":
                var publishTermination = context.Db.Queryable<publish_termination>().Where(x => x.SubProjectId == SubProject1.Id && x.PublishedVersion == PublishVersion).ToList().FirstOrDefault();
                if (publishTermination == null) return;
                file = await storage.DownloadPublishTerminationFileAsync(subProjectId, publishTermination.Id); 
                DownloadUrl = storage.GetFileDownloadUrl(storage.GetPublishTerminationFileRelativePath(subProjectId, publishTermination.Id)); break;
            case "电缆":
                if (SubProject2 == null) return;
                var publishCable = context.Db.Queryable<publish_cable>().Where(x => x.SubProjectId1 == SubProject1.Id && x.SubProjectId2 == SubProject2.Id && x.PublishedVersion == PublishVersion).ToList().FirstOrDefault();
                if (publishCable == null) return;
                file = await storage.DownloadPublishCableFileAsync(subProjectId, SubProject2?.Id ?? throw new("电缆需要选择两个子项"), publishCable.Id);
                DownloadUrl = storage.GetFileDownloadUrl(storage.GetPublishCableFileRelativePath(subProjectId, SubProject2?.Id ?? throw new("电缆需要选择两个子项"), publishCable.Id)); break;
            default:
                break;
        }
        
        // 解析Excel文件并转换为对应数据类型
        using var dataTable = await excel.GetDataTableAsStringAsync(file, true);

        var objList = Category switch {
            "IO" => dataTable.StringTableToIEnumerableByDiplay<IoData>().Cast<object>(),
            "端接" => dataTable.StringTableToIEnumerableByDiplay<TerminationData>().Cast<object>(),
            "电缆" => dataTable.StringTableToIEnumerableByDiplay<CableData>().Cast<object>(),
            _ => throw new NotSupportedException(),
        };

        AllData = [.. objList];

    }

    /// <summary>
    /// 编辑配置表命令
    /// 根据参数设置配置表的标题和数据类型，并导航到配置表编辑页面
    /// 用于管理系统的各种配置数据，如控制系统IO数据映射配置等
    /// </summary>
    /// <param name="param">配置表的类型名称参数</param>
    /// <exception cref="NotImplementedException">当遇到未实现的配置类型时抛出异常</exception>
    [RelayCommand]
    private void EditConfigurationTable(string param)
    {
        configvm.Title = param;
        configvm.DataType = param switch
        {
            "控制系统IO数据配置" => typeof(config_controlSystem_mapping),            
            _ => throw new NotImplementedException(),
        };
        navigation.NavigateWithHierarchy(typeof(ConfigTablePage));
    }
    /// <summary>
    /// 刷新数据命令
    /// 显示加载状态并重新加载当前选中版本的数据
    /// 完成后重置状态为默认状态
    /// </summary>
    /// <returns>异步任务，表示刷新操作的完成</returns>
    [RelayCommand]
    private async Task Refresh() {
        model.Status.Busy("正在刷新……");
        await ReloadAllData();
        model.Status.Reset();
    }

}