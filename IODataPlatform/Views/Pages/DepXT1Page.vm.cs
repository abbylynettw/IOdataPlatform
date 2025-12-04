﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System.IO;

using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Models.ExtDBModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.Common;
using IODataPlatform.Views.SubPages.XT2;
using IODataPlatform.Views.Windows;
using LYSoft.Libs.ServiceInterfaces;
using SqlSugar;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// XT1控制系统专业页面视图模型类
/// 负责XT1(龙鳞)控制系统IO数据的全生命周期管理，包括Excel数据导入、PDF提取、智能分配等
/// 支持复杂的IO机柜自动分配算法，针对XT1系统的特殊规格和要求进行优化
/// 提供数据导入、PDF文档解析和机柜分配预览等专业功能
/// </summary>
public partial class DepXT1ViewModel(SqlSugarContext context, GlobalModel model, IMessageService message, IContentDialogService dialog, INavigationService navigation,
    StorageService storage, ExcelService excel, IPickerService picker, PublishViewModel publishvm,ExtractPdfViewModel epvm, NavigationParameterService parameterService) : ObservableObject, INavigationAware
{

    /// <summary>页面初始化状态标记，防止重复初始化操作</summary>
    private bool isInit = false;

    /// <summary>当前页面的所有IO数据集合，包含完整的IO信号信息和机柜分配结果</summary>
    [ObservableProperty]
    private ObservableCollection<IoFullData>? allData;

    /// <summary>冗余率百分比，用于机柜容量计算时的安全余量设置，默认为20%</summary>
    [ObservableProperty]
    private int redundancyRate = 20;

    /// <summary>
    /// 页面导航离开时触发
    /// 当前实现为空，预留用于后续数据保存或状态清理操作
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

    /// <summary>
    /// 从服务器下载并加载实时IO数据
    /// 清除当前数据，从服务器下载最新的实时数据文件并解析为IO对象集合
    /// 包含完整的错误处理，当文件不存在或解析失败时返回空集合
    /// </summary>
    /// <returns>异步任务，表示数据加载操作的完成</returns>
    /// <exception cref="Exception">当必要的项目参数缺失时抛出异常</exception>
    private async Task ReloadAllData() {
        AllData = null;
        _ = Project?.Id ?? throw new("开发人员注意");
        var subProjectId = SubProject?.Id ?? throw new("开发人员注意");
        try {
            // 下载并解析Excel文件
            var file = await storage.DownloadRealtimeIoFileAsync(subProjectId);
            using var dataTable = await excel.GetDataTableAsStringAsync(file, true);
            var list = await Task.Run(() => new ObservableCollection<IoFullData>(dataTable.StringTableToIEnumerableByDiplay<IoFullData>()));
            AllData = list;
        } catch {
            // 异常时返回空集合，保证界面正常显示
            AllData = [];
        }
    }    

    /// <summary>
    /// 导入Excel数据命令
    /// 导航到Excel数据上传页面，用于批量导入IO数据
    /// 支持标准的Excel模板格式，自动识别和解析列映射关系
    /// </summary>
    [RelayCommand]
    private void ImportExcelData()
    {
        navigation.NavigateWithHierarchy(typeof(UploadExcelDataPage));
    }



    /// <summary>
    /// 保存并上传实时IO文件，支持可选的版本发布
    /// 将当前AllData导出为Excel文件并上传到服务器
    /// 如果提供versionId参数，同时创建发布版本的副本用于正式发布
    /// </summary>
    /// <param name="versionId">可选的发布版本标识符，如果提供则同时创建发布版本</param>
    /// <returns>异步任务，表示保存和上传操作的完成</returns>
    /// <exception cref="Exception">当必要的项目参数缺失时抛出异常</exception>
    public async Task SaveAndUploadFileAsync(int? versionId = null) {
        _ = Project?.Id ?? throw new("开发人员注意");
        var subProjectId = SubProject?.Id ?? throw new("开发人员注意");
        var allData = AllData ?? throw new("开发人员注意");

        // 获取实时文件路径并导出Excel
        var relativePath = storage.GetRealtimeIoFileRelativePath(subProjectId);
        var absolutePath = storage.GetWebFileLocalAbsolutePath(relativePath);

        using var dataTable = await allData.ToTableByDisplayAttributeAsync();
        await excel.FastExportAsync(dataTable, absolutePath);
        await storage.UploadRealtimeIoFileAsync(subProjectId);

        // 如果指定了版本号，则创建发布版本
        if (versionId == null) { return; }
        await storage.WebCopyFilesAsync([(relativePath, storage.GetPublishIoFileRelativePath(subProjectId, versionId.Value))]);
    }

    /// <summary>
    /// 提取PDF数据命令
    /// 配置PDF数据提取器的字段映射关系，并导航到PDF提取页面
    /// 支持两种数据类型：IO字段和设置参数字段的自动识别和提取
    /// 用于从技术文档和设计图纸中自动化提取IO数据
    /// </summary>
    [RelayCommand]
    private void ExtractPdfData()
    {
        // 配置IO字段映射关系
        epvm.IoFields = ["序号", "信号位号", "扩展码", "信号说明", "安全分级分组", "功能分级", "抗震类别", "IO类型", "信号特性", "供电方", "测量单位",
            "量程下限", "量程上限", "缺省值", "SOETRA", "负载信息", "图号", "备注", "版本"];
        navigation.NavigateWithHierarchy(typeof(ExtractPdfPage));
    }

    /// <summary>
    /// 智能IO分配命令
    /// 执行针对XT1系统优化的自动IO分配算法，根据信号类型、卡件规格和冗余率计算最优机柜分配方案
    /// XT1系统的分配算法考虑了龙鳞系统的特殊架构和性能要求
    /// 包括信号分类、卡件匹配、机柜容量计算和余量分配等关键步骤
    /// </summary>
    /// <returns>异步任务，表示分配操作的完成</returns>
    /// <exception cref="Exception">当必要的数据缺失或配置不当时抛出异常</exception>
    [RelayCommand]
    private async Task AllocateIO()
    {
        _ = AllData ?? throw new("开发人员注意");
        model.Status.Busy($"正在分配……");
        
        var formularHelper = new FormularHelper();
        // 获取必要的卡件类型配置数据
        List<config_card_type_judge> config_Card_Type_Judges = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
        
        // 执行针对XT1系统优化的分配算法
        AllData = [..formularHelper.AutoAllocateXT1IO([.. AllData], config_Card_Type_Judges, RedundancyRate / 100.0)];
       
        await SaveAndUploadFileAsync();
        model.Status.Success($"分配完毕！");
    }

    /// <summary>
    /// 预览IO分配结果命令
    /// 设置当前控制系统为龙鳞，并导航到机柜分配结果预览页面
    /// 用于在正式分配之前预览和验证分配算法的结果
    /// 提供可视化的机柜布局和资源利用率分析
    /// </summary>
    [RelayCommand]
    private void PreviewAllocateIO()
    {
        parameterService.SetParameter("controlSystem", ControlSystem.龙鳍);
        navigation.NavigateWithHierarchy(typeof(CabinetAllocatedPage));
    }

}
