﻿﻿﻿﻿﻿﻿﻿﻿using Aspose.Cells;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.AQJ;
using IODataPlatform.Views.SubPages.Common;
using IODataPlatform.Views.Windows;
using LYSoft.Libs.ServiceInterfaces;
using SqlSugar;
using System.Reactive.Linq;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// AQJ控制系统专业页面视图模型类
/// 负责AQJ控制系统IO数据的全生命周期管理，包括数据导入、智能分配、机柜计算等核心功能
/// 支持复杂的IO机柜自动分配算法，包括余量计算、卡件类型匹配和信号分组等高级功能
/// 提供完整的配置表管理和数据发布功能，支持实时数据同步和版本控制
/// </summary>
public partial class DepAQJViewModel(SqlSugarContext context, GlobalModel model, ConfigTableViewModel configvm, IMessageService message, IContentDialogService dialog, INavigationService navigation,
    StorageService storage, ExcelService excel, IPickerService picker,PublishViewModel publishvm,DatabaseService database, NavigationParameterService parameterService) : ObservableObject, INavigationAware {

    /// <summary>页面初始化状态标记，防止重复初始化操作</summary>
    private bool isInit = false;

    /// <summary>IO卡件类型配置集合，用于IO分配算法的类型匹配和针脚数量计算</summary>
    private List<config_card_type_judge> config_Card_Types;

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

    /// <summary>当前页面的所有IO数据集合，包含完整的IO信号信息和机柜分配结果</summary>
    [ObservableProperty]
    private ObservableCollection<IoFullData>? allData;

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
    /// 从服务器下载并加载实时IO数据
    /// 清除当前数据，从服务器下载最新的实时数据文件并解析为IO对象集合
    /// 同时加载必要的IO卡件类型配置数据，用于后续的IO分配操作
    /// 包含完整的错误处理，当文件不存在或解析失败时返回空集合
    /// </summary>
    /// <returns>异步任务，表示数据加载操作的完成</returns>
    /// <exception cref="Exception">当必要的项目参数缺失时抛出异常</exception>
    public async Task ReloadAllData() {
        AllData = null;
        _ = Project?.Id ?? throw new("开发人员注意");
        _ = Major?.Id ?? throw new("开发人员注意");
        var subProjectId = SubProject?.Id ?? throw new("开发人员注意");
        try {
            // 下载并解析Excel文件
            var file = await storage.DownloadRealtimeIoFileAsync(subProjectId);
            var data = await excel.GetDataTableAsStringAsync(file, true);
            var list = data.StringTableToIEnumerableByDiplay<IoFullData>();
            AllData = [.. list];
        } catch {
            // 异常时返回空集合，保证界面正常显示
            AllData = [];
        }
        
        // 加载必要的卡件类型配置数据
        this.config_Card_Types = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
    }

    /// <summary>
    /// 编辑配置表命令
    /// 根据参数设置配置表的标题和数据类型，并导航到配置表编辑页面
    /// 支持多种业务配置：模拟量、数字量配置、控制站编号替换、变量扩展码替换等
    /// 这些配置直接影响AQJ系统的IO分配算法和数据处理逻辑
    /// </summary>
    /// <param name="param">配置表的类型名称参数</param>
    /// <exception cref="NotImplementedException">当遇到未实现的配置类型时抛出异常</exception>
    [RelayCommand]
    private void EditConfigurationTable(string param)
    {
        configvm.Title = param;
        configvm.DataType = param switch
        {
            "模拟量类型配置表" => typeof(config_aqj_analog),
            "数字量类型配置表" => typeof(config_aqj_control),
            "控制站编号替换配置表" => typeof(config_aqj_stationReplace),
            "变量扩展码替换配置表" => typeof(config_aqj_tagReplace),
            "控制站机柜对照配置表" => typeof(config_aqj_stationCabinet),
            "IO卡型号配置表" => typeof(config_card_type_judge),
            _ => throw new NotImplementedException(),
        };
        navigation.NavigateWithHierarchy(typeof(ConfigTablePage));
    }

    /// <summary>
    /// 配置信号分组命令
    /// 导航到信号分组配置页面，用于管理AQJ系统中的信号分组规则
    /// 信号分组影响IO分配算法中的信号类型识别和机柜分配逻辑
    /// </summary>
    [RelayCommand]
    private void GonfigSignalGroup()
    {        
        navigation.NavigateWithHierarchy(typeof(ConfigSignalGroupPage));
    }

    /// <summary>
    /// 智能IO分配命令
    /// 执行复杂的自动IO分配算法，根据信号类型、卡件规格和冗余率计算最优机柜分配方案
    /// 包括三个关键阶段：信号分类、卡件匹配、机柜容量计算和余量分配
    /// 算法考虑到电气安全、信号类型兼容性和系统扩展性等多个因素
    /// </summary>
    /// <returns>异步任务，表示分配操作的完成</returns>
    /// <exception cref="Exception">当必要的数据缺失或配置不当时抛出异常</exception>
    [RelayCommand]
    private async Task AllocateIO()
    {
        // 更新卡件类型配置，确保使用最新的配置数据
        this.config_Card_Types = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
        _ = AllData ?? throw new("开发人员注意");
        
        model.Status.Busy($"正在分配……");
        
        // 执行核心分配算法
        var formularHelper = new FormularHelper();
        var cabinets = formularHelper.AutoAllocateLongHeIO([.. AllData], this.config_Card_Types, RedundancyRate / 100.0);
        
        // 将机柜结构转换回点表格式
        AllData = [.. CabinetCalc.CabinetStructureToPoint(cabinets).ToObservable()];
        
        await SaveAndUploadFileAsync();
        model.Status.Success($"分配完毕！");
    }

    /// <summary>
    /// 添加备用点位命令
    /// 为每个已分配的IO卡件自动添加剩余的备用通道
    /// 按照机柜、笼子、槽位和卡件类型分组，为每组填充剩余的针脚位置
    /// 备用点位用于系统扩展和维护，确保设备的可扩展性和维护便利性
    /// </summary>
    /// <returns>异步任务，表示添加操作的完成</returns>
    /// <exception cref="Exception">当找不到对应的卡件类型或数据异常时抛出异常</exception>
    [RelayCommand]
    private async Task AddTag()
    {
        _ = AllData ?? throw new("开发人员注意");

        model.Status.Busy("正在添加备用点……");

        // 按照机柜结构分组并生成备用点位
        var backupPoints = AllData
            .GroupBy(d => new { d.CabinetNumber, d.Cage, d.Slot, d.CardType })
            .SelectMany(group =>
            {
                // 获取对应的卡件类型配置
                var configCardType = config_Card_Types.FirstOrDefault(c => c.IoCardType == group.Key.CardType);
                if (configCardType == null) throw new Exception($"找不到{group.Key.CardType}板卡类型");

                // 计算可用的备用通道
                var existingChannels = group.Select(g => g.Channel).ToHashSet();
                var availableChannels = Enumerable.Range(1, configCardType.PinsCount).Except(existingChannels);

                // 创建备用点位模板
                var lastTag = group.LastOrDefault()?.JsonClone();
                if (lastTag == null) throw new Exception($"{group.Key.CabinetNumber} {group.Key.Cage}{group.Key.Slot}上没有一个点");
                
                return availableChannels.Select(i =>
                {
                    var newTag = new IoFullData(); // 创建新的备用点位
                    newTag.StationName = lastTag.StationName;
                    newTag.CabinetNumber = lastTag.CabinetNumber;
                    newTag.Cage = lastTag.Cage;
                    newTag.Slot = lastTag.Slot;
                    newTag.IoType = lastTag.IoType;
                    newTag.CardType = lastTag.CardType;
                    newTag.BackCardType = lastTag.BackCardType;
                    newTag.TerminalBoardModel = lastTag.TerminalBoardModel;
                    newTag.TerminalBoardNumber = lastTag.TerminalBoardNumber;
                    newTag.Channel = i;
                    newTag.TagName = "备用通道";
                    newTag.PointType = TagType.BackUp;
                    return newTag;
                });
            }).ToList();

        // 添加备用点并重新排序
        AllData.AddRange(backupPoints);
        AllData = [.. AllData.OrderBy(c => c.CabinetNumber).ThenBy(c => c.Cage).ThenBy(c => c.Slot).ThenBy(c => c.Channel)];
        await SaveAndUploadFileAsync();
        model.Status.Reset();
    }
    [RelayCommand]
    private async Task DeleteTag()
    {
        _ = AllData ?? throw new("未找到Io数据");
        AllData = [.. AllData.RemoveWhere(a => a.PointType == TagType.BackUp)];
        await SaveAndUploadFileAsync();
    }

    [RelayCommand]
    private async Task Recalc()
    {
        _ = AllData ?? throw new("开发人员注意");
        AllData = [.. CabinetCalc.Recalc(AllData).ToObservable()];
        await SaveAndUploadFileAsync();
    }

    [RelayCommand]
    private void PreviewAllocateIO()
    {
        if (SubProject is null) { throw new Exception("子项目为空，找不到控制系统"); }
        var controsystem = context.Db.Queryable<config_project_major>()
                                  .Where(it => it.Id == SubProject.MajorId).First().ControlSystem;
        parameterService.SetParameter("controlSystem", controsystem);
        navigation.NavigateWithHierarchy(typeof(CabinetAllocatedPage));   
    }

   
}