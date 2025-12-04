using System.IO;
using System.Reactive.Linq;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Media;

using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.Common;
using IODataPlatform.Views.SubPages.XT2;
using IODataPlatform.Views.Windows;
using LYSoft.Libs.ServiceInterfaces;

using SqlSugar;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace IODataPlatform.Views.Pages;

public partial class DepXT2ViewModel(SqlSugarContext context, ConfigTableViewModel configvm, INavigationService navigation,
    GlobalModel model, IMessageService message, IContentDialogService dialog, StorageService storage,
    ExcelService excel, IPickerService picker, PublishViewModel publishvm, DatabaseService database, ExtractPdfViewModel epvm, NavigationParameterService parameterService, CloudExportConfigService cloudExportConfigService) : ObservableObject, INavigationAware
{

    private bool isInit = false;

    [ObservableProperty]
    private ObservableCollection<IoFullData>? allData;//所有点

    private List<config_card_type_judge> config_Card_Types;

    public void OnNavigatedFrom()
    {

    }

    public async void OnNavigatedTo()
    {
        if (!isInit)
        {
            // 页面首次加载时初始化筛选器
            InitializeFilters();
            
            await RefreshProjects();
            this.config_Card_Types = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
            isInit = true;
        }
        else
        {
            FilterAndSort();
        }
    }

    //[ObservableProperty]
    //private ObservableCollection<StdCabinet>? allData;       // 全部的数据

    /// <summary>保存当前AllData并上传实时文件，如需要发布，同时传入versionId参数</summary>
    /// <param name="versionId">发布版本ID</param>
    public async Task SaveAndUploadFileAsync(int? versionId = null)
    {
        _ = Project?.Id ?? throw new("开发人员注意");
        var subProjectId = SubProject?.Id ?? throw new("开发人员注意");
        var allData = AllData ?? throw new("开发人员注意");

        var relativePath = storage.GetRealtimeIoFileRelativePath(subProjectId);
        var absolutePath = storage.GetWebFileLocalAbsolutePath(relativePath);

        using var dataTable = await allData.ToTableByDisplayAttributeAsync();
        await excel.FastExportAsync(dataTable, absolutePath);
        await storage.UploadRealtimeIoFileAsync(subProjectId);

        if (versionId == null)
        { return; }
        await storage.WebCopyFilesAsync([(relativePath, storage.GetPublishIoFileRelativePath(subProjectId, versionId.Value))]);
    }

    /// <summary>从服务器下载实时数据文件并加载</summary>
    public async Task ReloadAllData()
    {
        AllData = null;
        _ = Project?.Id ?? throw new("开发人员注意");
        var subProjectId = SubProject?.Id ?? throw new("开发人员注意");
        try
        {
            var file = await storage.DownloadRealtimeIoFileAsync(subProjectId);
            var data = await excel.GetDataTableAsStringAsync(file, true);
            var list = data.StringTableToIEnumerableByDiplay<IoFullData>();
            AllData = [.. list];
        }
        catch (Exception ex)
        {
            AllData = [];
        }
    }

    [ObservableProperty]
    private bool isCabinetSummaryFlyoutOpen = false;

    [ObservableProperty]
    private bool isTotalSummaryFlyoutOpen = false;

    [ObservableProperty]
    private TotalSummaryInfo? totalSummaryInfo;

    [ObservableProperty]
    private int redundancyRate = 20;

    [RelayCommand]
    private async void GetTotalSummaryInfo()
    {
        _ = AllData ?? throw new();
        TotalSummaryInfo = CabinetCalc.GetTotalSummaryInfo([.. AllData], this.config_Card_Types);
        IsTotalSummaryFlyoutOpen = true;
    }

    [RelayCommand]
    private void ExtractPdfData()
    {
        epvm.IoFields = ["序号", "机柜号", "就地箱号", "信号位号", "扩展码", "信号功能", "安全分级", "抗震类别", "传感器类型",
        "IO类型", "信号特性", "供电类型", "最小测量范围", "最大测量范围", "单位", "电压等级", "仪表功能号", "版本", "备注"];
        navigation.NavigateWithHierarchy(typeof(ExtractPdfPage));
    }

    [RelayCommand]
    private void ImportExcelData()
    {
        parameterService.SetParameter("controlSystem", ControlSystem.龙鳍);
        navigation.NavigateWithHierarchy(typeof(UploadExcelDataPage));
    }

    [RelayCommand]
    private void OpenPointDetailManagement()
    {
        // 传递当前ViewModel，以便点详情管理页面可以访问AllData
        parameterService.SetParameter("DepXT2ViewModel", this);
        navigation.NavigateWithHierarchy(typeof(SubPages.DepXT2.PointDetailManagementPage));
    }


    [RelayCommand]
    private async Task AddTag(TagType type)
    {
        _ = AllData ?? throw new("开发人员注意");
        if (type == TagType.Alarm)
        {
            var configs = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
            var configDI211 = configs.FirstOrDefault(c => c.IoCardType == "DI211");
            var configDO211 = configs.FirstOrDefault(c => c.IoCardType == "DO211");

            if (configDI211 == null)
                throw new Exception("未找到卡件DI211的数量");
            if (configDO211 == null)
                throw new Exception("未找到卡件DO211的数量");

            model.Status.Busy($"正在添加报警点……");

            foreach (var cabinet in AllData.GroupBy(c => c.CabinetNumber))
            {
                // 检查机柜是否已经有报警点
                var hasAlarmPoint = AllData.Where(a => a.PointType == TagType.Alarm).Count() > 0;
                if (hasAlarmPoint)
                    continue;

                // 添加DI211报警点
                AddAlarmPoints(cabinet.Key, cabinet.ToList());

                // 添加DO211报警点
                AddControlAlarmPoint(cabinet.Key, cabinet.ToList());
            }

            model.Status.Busy($"添加报警点完毕……");
        }
        else if (type == TagType.BackUp)
        {
            //添加备用点
            model.Status.Busy($"正在添加备用点……");
            foreach (var cabinet in AllData.GroupBy(d => d.CabinetNumber))
            {
                var cages = cabinet.ToList().GroupBy(c => c.Cage);
                foreach (var cage in cages)
                {
                    var slots = cage.ToList().GroupBy(c => c.Slot);
                    foreach (var slot in slots)
                    {
                        var list = slot.ToList();
                        var cardType = slot.FirstOrDefault().CardType;
                        if (cardType == null || cardType.Contains("DP") || cardType.Contains("FF"))
                            continue;
                        var configCardType = this.config_Card_Types.FirstOrDefault(c => c.IoCardType == cardType);
                        if (configCardType == null)
                            throw new Exception($"找不到{configCardType}板卡类型");
                        for (int i = 1; i <= configCardType.PinsCount; i++)
                        {
                            if (list.Where(l => l.Channel == i).Count() == 0)
                            {
                                var lastTag = list.FirstOrDefault();
                                if (lastTag == null)
                                    continue;
                                //throw new Exception($"机柜{cabinet.Name}机笼{cage.Index}插槽{slot.Index.ToString("00")}卡件{cardType.Substring(0, 2)}没有点，无法添加备用点！");
                                var point = new IoFullData()
                                {
                                    CabinetNumber = cabinet.Key,
                                    SignalPositionNumber = $"{cabinet.Key}{cage.Key}{slot.Key.ToString("00")}{cardType.Substring(0, 2)}CH{i.ToString("00")}",
                                    SystemCode = "BEIYONG",
                                    Cage = cage.Key,
                                    Slot = slot.Key,
                                    CardType = cardType,
                                    Description = "备用",
                                    Channel = i,
                                    SubNet = lastTag.SubNet,
                                    StationNumber = lastTag.StationNumber,
                                    IoType = lastTag.IoType,
                                    PowerType = lastTag.PowerType,
                                    ElectricalCharacteristics = lastTag.ElectricalCharacteristics,
                                    SignalEffectiveMode = lastTag.SignalEffectiveMode,
                                    PointType = TagType.BackUp,
                                    Version = "A",
                                    ModificationDate = DateTime.Now
                                };
                                AllData.Add(point);
                            }
                        }
                    }
                }
            }
            model.Status.Busy($"添加备用点完毕……");
        }

        // 添加DI211报警点的方法
        void AddAlarmPoints(string cabinetName, List<IoFullData> data)
        {
            var alarmDescriptions = new[] { "电源A故障报警", "电源B故障报警", "机柜门开", "温度高报警", "风扇故障", "网络故障" };
            var extensionCodes = new[] { "PWFA", "PWFB", "DROP", "TEPH", "FAN", "SWF" };
            var samePoint = data.FirstOrDefault();
            for (int i = 0; i < 6; i++)
            {
                var point = new IoFullData
                {
                    CabinetNumber = cabinetName,
                    PointType = TagType.Alarm,
                    SignalPositionNumber = cabinetName,
                    Cage = 0,
                    Slot = 0,
                    Channel = 0,
                    IoType = "DI",
                    PowerType = "DI1",
                    SubNet = samePoint != null ? samePoint.SubNet : "未找到",
                    StationNumber = samePoint != null ? samePoint.StationNumber : "未找到",
                    ElectricalCharacteristics = "无源常开",
                    SignalEffectiveMode = "NO",
                    SystemCode = "JIGUIBAOJING",
                    ExtensionCode = extensionCodes[i],
                    Description = $"控制柜{cabinetName}机柜{alarmDescriptions[i]}"
                };
                AllData.Add(point);
            }
        }

        // 添加DO211报警点的方法
        void AddControlAlarmPoint(string cabinetName, List<IoFullData> data)
        {
            var samePoint = data.FirstOrDefault();
            var point = new IoFullData
            {
                CabinetNumber = cabinetName,
                PointType = TagType.Alarm,
                Cage = 0,
                Slot = 0,
                Channel = 0,
                SignalPositionNumber = cabinetName,
                SystemCode = "JIGUIBAOJING",
                ExtensionCode = "ALM",
                SubNet = samePoint != null ? samePoint.SubNet : "未找到",
                StationNumber = samePoint != null ? samePoint.StationNumber : "未找到",
                Description = $"控制柜{cabinetName}机柜报警灯",
                IoType = "DO",
                PowerType = "DO2",
                ElectricalCharacteristics = "有源常闭",
                SignalEffectiveMode = "NO"
            };
            AllData.Add(point);
        }
        AllData = [.. AllData];
        await Recalc();
        await SaveAndUploadFileAsync();
        model.Status.Reset();
    }

    [RelayCommand]
    private async Task DeleteTag(TagType type)
    {
        _ = AllData ?? throw new("开发人员注意");
        model.Status.Busy($"正在删除点……");
        AllData.RemoveWhere(x => x.PointType == type);
        await SaveAndUploadFileAsync();
        await Refresh();
        model.Status.Reset();
    }

    [RelayCommand]
    private void EditConfigurationTable(string param)
    {
        configvm.Title = param;
        configvm.DataType = param switch
        {
            "IO卡型号配置表" => typeof(config_card_type_judge),
            "TB型号配置表" => typeof(config_terminalboard_type_judge),
            "接线点配置表" => typeof(config_connection_points),
            "供电方式配置表" => typeof(config_power_supply_method),
            "OF显示格式值配置表" => typeof(config_output_format_values),
            "机柜报警清单配置表" => typeof(config_xt2_cabinet_alarm),
            _ => throw new NotImplementedException(),
        };
        navigation.NavigateWithHierarchy(typeof(ConfigTablePage));
    }

    /// <summary>
    /// IO自动分配命令（主界面入口 - 分配所有机柜）
    /// </summary>
    [RelayCommand]
    private async Task AllocateIO()
    {
        await PerformIOAllocationWithReservedConfirmation(null);
    }
    
    /// <summary>
    /// 执行IO分配流程（包含预留确认逻辑）
    /// 供主界面和预览界面共同调用
    /// </summary>
    /// <param name="cabinetName">机柜名称，如果为null则分配所有机柜</param>
    public async Task PerformIOAllocationWithReservedConfirmation(string? cabinetName = null)
    {
        _ = AllData ?? throw new("无IO数据可分配，请先导入数据");
        
        // 🔑 步骤1：直接显示预留配置窗口
        var reservedConfigsForAllocation = await ShowReservedSlotConfigDialog(cabinetName);
        
        // 如果用户取消了配置窗口，则退出
        if (reservedConfigsForAllocation == null)
        {
            return;
        }
        
        // 🔑 步骤2：删除旧的预留信号（通讯预留和报警预留）
        var existingReservedSignals = cabinetName == null 
            ? AllData.Where(d => d.PointType == TagType.CommunicationReserved || d.PointType == TagType.AlarmReserved).ToList()
            : AllData.Where(d => (d.PointType == TagType.CommunicationReserved || d.PointType == TagType.AlarmReserved) && d.CabinetNumber == cabinetName).ToList();
            
        foreach (var sig in existingReservedSignals)
        {
            AllData.Remove(sig);
        }
        
        // 🔑 步骤3：执行IO分配
        model.Status.Busy($"正在分配……");
        
        try
        {
            var formularHelper = new FormularHelper();
            List<config_card_type_judge> config_Card_Type_Judges = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
            
            // 执行 IO 自动分配（传入预留配置信息）
            AllData = [.. formularHelper.AutoAllocateIO([.. AllData], config_Card_Type_Judges, RedundancyRate / 100.0, reservedConfigsForAllocation)];
            
            // 重新计算
            if (SubProject is null)
            { throw new Exception("子项目为空，找不到控制系统"); }
            var controlSystem = context.Db.Queryable<config_project_major>()
                                      .Where(it => it.Id == SubProject.MajorId).First().ControlSystem;
            await RecalcMethodInternal(controlSystem, showStatus: false);
            
            // 保存
            await SaveAndUploadFileAsync();
            model.Status.Success($"分配完毕！");
            
            // 显示IO分配报告
            var allocationReport = formularHelper.GetAllocationReport();
            await ShowAllocationReport(allocationReport);
        }
        catch (Exception ex)
        {
            model.Status.Error($"分配失败：{ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 将现有预留信号转换回配置信息
    /// </summary>
    private List<CabinetReservedSlotConfig> ConvertSignalsToReservedConfigs(List<IoFullData> reservedSignals)
    {
        return reservedSignals
            .GroupBy(s => s.CabinetNumber)
            .Select(group => {
                var config = new CabinetReservedSlotConfig
                {
                    CabinetName = group.Key,
                    IsSelected = true
                };
                
                // 为每个预留信号创建插槽配置
                foreach (var signal in group)
                {
                    // 根据信号类型判断预留目的
                    var reservedPurpose = signal.PointType == TagType.AlarmReserved 
                        ? ReservedPurpose.报警预留 
                        : ReservedPurpose.通讯预留;
                    
                    config.SlotConfigs.Add(new SlotCardTypeConfig
                    {
                        SelectedCardType = signal.CardType,
                        ReservedPurpose = reservedPurpose,
                        AvailableCardTypes = config.AvailableCardTypes
                    });
                }
                
                return config;
            })
            .ToList();
    }
    
    /// <summary>
    /// 显示预留插槽配置对话框
    /// </summary>
    /// <param name="cabinetName">机柜名称，如果为null则显示所有机柜</param>
    /// <returns>预留配置列表，如果用户取消则返回null</returns>
    private async Task<List<CabinetReservedSlotConfig>?> ShowReservedSlotConfigDialog(string? cabinetName = null)
    {
        // 获取机柜名称列表（根据cabinetName参数过滤）
        var cabinetNames = cabinetName == null
            ? AllData.Select(d => d.CabinetNumber).Distinct().ToList()
            : new List<string> { cabinetName };
            
        var cabinetStructures = new List<StdCabinet>();
        foreach (var name in cabinetNames)
        {
            StdCabinet cabinet;
            if (name.Contains("EX2"))
                cabinet = StdCabinet.CreateExEx(name);
            else if (name.Contains("EX"))
                cabinet = StdCabinet.CreateEx(name);
            else if (name.Contains("LH"))
                cabinet = StdCabinet.CreateLH(name);
            else
                cabinet = StdCabinet.Create(name);
            cabinetStructures.Add(cabinet);
        }
        
        // 🔑 获取现有的预留信号并转换为配置信息（包括通讯预留和报警预留）
        var existingReservedSignals = cabinetName == null 
            ? AllData.Where(d => d.PointType == TagType.CommunicationReserved || d.PointType == TagType.AlarmReserved).ToList()
            : AllData.Where(d => (d.PointType == TagType.CommunicationReserved || d.PointType == TagType.AlarmReserved) && d.CabinetNumber == cabinetName).ToList();
            
        List<CabinetReservedSlotConfig>? existingConfigs = null;
        if (existingReservedSignals.Any())
        {
            existingConfigs = ConvertSignalsToReservedConfigs(existingReservedSignals);
        }
        
        // 显示预留插槽设置窗口（传入现有配置）
        var reservedSlotWindow = new ReservedSlotConfigWindow(cabinetStructures, existingConfigs);
        var windowResult = reservedSlotWindow.ShowDialog();
        
        if (windowResult != true)
        {
            return null;
        }
        
        // 🔑 直接返回预留配置信息，不生成预留信号
        // 预留信号的生成将在AutoAllocateIO方法内部完成
        return reservedSlotWindow.ReservedSlotConfigs.Where(c => c.IsSelected).ToList();
    }

    [RelayCommand]
    private async Task PreviewAllocateIO()
    {
        if (SubProject is null)
        { throw new Exception("子项目为空，找不到控制系统"); }
        
        model.Status.Busy("正在加载预览...");
        
        var controsystem = await context.Db.Queryable<config_project_major>()
                                  .Where(it => it.Id == SubProject.MajorId)
                                  .FirstAsync();
        parameterService.SetParameter("controlSystem", controsystem.ControlSystem);
        
        model.Status.Reset();
        navigation.NavigateWithHierarchy(typeof(CabinetAllocatedPage));
    }

    /// <summary>
    /// FF从站模块自动分配命令
    /// 根据新的输入格式进行FF从站模块的自动分配和通道计算
    /// </summary>
    [RelayCommand]
    private async Task AllocateFFSlaveModules()
    {
        _ = AllData ?? throw new("无IO数据可分配，请先导入数据");
        
        model.Status.Busy("正在进行FF从站模块分配...");
        
        // 调用FF从站分配逻辑（内部已捕获所有异常并记录到报告）
        var report = await PerformFFSlaveModuleAllocation();
        
        // 先判断分配是否成功（根据报告内容）
        bool isSuccess = report.Contains("【分配成功】");
        
        if (isSuccess)
        {
            // 成功：保存数据并设置成功状态
            AllData = [.. AllData]; // 刷新界面显示
            await SaveAndUploadFileAsync();
            model.Status.Success("分配完成！");
        }
        else
        {
            // 失败：不保存数据，只设置警告状态
            model.Status.Warn("分配未完成，请查看报告了解详情");
        }
        
        // 最后显示分配结果报告（无论成功还是失败）
        await ShowFFSlaveAllocationReport(report);
    }

    /// <summary>
    /// 显示IO分配报告对话框
    /// </summary>
    /// <param name="report">分配报告内容</param>
    private async Task ShowAllocationReport(string report)
    {
        var contentDialog = new ContentDialog
        {
            Title = "IO自动分配报告",
            Content = CreateReportContent(report),
            CloseButtonText = "关闭",
            PrimaryButtonText = "下载到桌面",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync(contentDialog, CancellationToken.None);
        
        // 如果用户点击了"下载到桌面"按钮
        if (result == ContentDialogResult.Primary)
        {
            await DownloadReportToDesktop(report, "IO自动分配报告");
        }
    }

    /// <summary>
    /// 显示FF从站分配报告对话框
    /// </summary>
    /// <param name="report">分配报告内容</param>
    private async Task ShowFFSlaveAllocationReport(string report)
    {
        var contentDialog = new ContentDialog
        {
            Title = "FF从站模块分配报告",
            Content = CreateReportContent(report),
            CloseButtonText = "关闭",
            PrimaryButtonText = "下载到桌面",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync(contentDialog, CancellationToken.None);
        
        // 如果用户点击了"下载到桌面"按钮
        if (result == ContentDialogResult.Primary)
        {
            await DownloadReportToDesktop(report, "FF从站模块分配报告");
        }
    }

    /// <summary>
    /// 创建报告内容控件
    /// </summary>
    /// <param name="report">报告文本</param>
    /// <returns>报告显示控件</returns>
    private FrameworkElement CreateReportContent(string report)
    {
        var scrollViewer = new ScrollViewer
        {
            Width = 800,
            Height = 500,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var textBox = new TextBox
        {
            Text = report,
            TextWrapping = TextWrapping.Wrap,
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12,
            Padding = new Thickness(15),
            Background = SystemColors.ControlBrush,
            Margin = new Thickness(5),
            IsReadOnly = true,  // 设置为只读
            BorderThickness = new Thickness(0),  // 移除边框
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,  // 禁用内部滚动条
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        scrollViewer.Content = textBox;
        return scrollViewer;
    }

    /// <summary>
    /// 下载报告到桌面并打开
    /// </summary>
    /// <param name="report">报告内容</param>
    /// <param name="reportTitle">报告标题</param>
    private async Task DownloadReportToDesktop(string report, string reportTitle)
    {
        try
        {
            // 获取桌面路径
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            
            // 构建文件名（不包含时间戳，允许覆盖）
            string fileName = $"{reportTitle}.txt";
            string filePath = Path.Combine(desktopPath, fileName);
            
            // 保存报告到文件
            await File.WriteAllTextAsync(filePath, report, System.Text.Encoding.UTF8);
            
            // 使用默认程序打开文件
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(processStartInfo);
            
            await message.SuccessAsync($"报告已保存到桌面并打开：\n{fileName}");
        }
        catch (Exception ex)
        {
            await message.ErrorAsync($"下载报告失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 生成预留通讯板卡信号列表
    /// </summary>
    /// <param name="reservedConfigs">预留配置列表</param>
    /// <returns>预留信号列表</returns>
    // 🗑️ GenerateReservedSignals 方法已不再需要，预留信号的生成已在 FormularHelper.AutoAllocateIOSingleCabinet 中完成
}