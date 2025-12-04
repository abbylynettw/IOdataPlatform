using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;
using IODataPlatform.Views.SubPages.XT2;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using IODataPlatform.Views.SubPages.Common;

namespace IODataPlatform.Views.SubPages.DepXT2;

/// <summary>
/// 机柜详细统计信息
/// </summary>
public partial class CabinetDetailInfo : ObservableObject
{
    [ObservableProperty]
    private string cabinetName = string.Empty;

    /// <summary>通讯报警点数量</summary>
    [ObservableProperty]
    private int communicationAlarmCount;

    /// <summary>硬点报警点数量</summary>
    [ObservableProperty]
    private int hardwareAlarmCount;

    /// <summary>备份点数量</summary>
    [ObservableProperty]
    private int backupPointsCount;

    /// <summary>一般点数量</summary>
    [ObservableProperty]
    private int normalPointsCount;

    /// <summary>未分配点数量</summary>
    [ObservableProperty]
    private int unsetPointsCount;

    /// <summary>未分配端子板数量</summary>
    [ObservableProperty]
    private int unsetBoardsCount;

    /// <summary>总点数</summary>
    [ObservableProperty]
    private int totalPointsCount;

    /// <summary>冗余率信息</summary>
    [ObservableProperty]
    private List<CardSpareRate> redundancyRates = new();
}

public partial class PointDetailManagementViewModel(
    GlobalModel model,
    ExcelService excel,
    NavigationParameterService navigation,
    INavigationService navigationService,
    IContentDialogService dialogService,
    IMessageService message,
    SelectExcelSheetDialogViewModel sheetDialog,
    SqlSugarContext context,
    DepXT2ViewModel parentVm) : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private ObservableCollection<CabinetDetailInfo> cabinetAlarmInfos = new();

    [ObservableProperty]
    private string? currentCabinetName;

    [ObservableProperty]
    private string logText = "📋 等待操作...\n";

    public async void OnNavigatedTo()
    {
        CurrentCabinetName = navigation.GetParameter<string?>("CabinetName");
        await Task.Run(() => RefreshAlarmStatistics());
    }

    public void OnNavigatedFrom()
    {
        // 清理工作（如需要）
    }

    [RelayCommand]
    private void RefreshAlarmStatistics()
    {
        CabinetAlarmInfos.Clear();
        LogText = "📋 等待操作...\n";
        
        if (parentVm.AllData == null || parentVm.AllData.Count == 0)
        {
            AppendLog("⚠️ 无法获取IO数据，请确保已打开项目并加载数据");
            AppendLog($"🔍 调试信息：AllData = {(parentVm.AllData == null ? "null" : $"Count={parentVm.AllData.Count}")}");
            return;
        }

        // 获取配置数据 - 使用异步查询避免DataReader错误
        var configs = context.Db.Queryable<config_card_type_judge>().ToListAsync().GetAwaiter().GetResult();

        // 构建机柜结构
        var cabinets = CabinetCalc.BuildCabinetStructureOther(parentVm.AllData.ToList(), configs);

        foreach (var cabinet in cabinets.OrderBy(c => c.Name))
        {
            // 如果是单机柜模式，只显示指定机柜
            if (CurrentCabinetName != null && cabinet.Name != CurrentCabinetName)
                continue;

            // 获取机柜统计信息
            var summaryInfo = CabinetCalc.GetCabinetSummaryInfo(cabinet);
            
            // 统计报警点（通讯和硬点）
            var commCount = parentVm.AllData.Count(p => 
                p.CabinetNumber == cabinet.Name && 
                p.PointType == TagType.CommunicationAlarm);
            
            var hwCount = parentVm.AllData.Count(p => 
                p.CabinetNumber == cabinet.Name && 
                p.PointType == TagType.Alarm);

            CabinetAlarmInfos.Add(new CabinetDetailInfo
            {
                CabinetName = cabinet.Name,
                CommunicationAlarmCount = commCount,
                HardwareAlarmCount = hwCount,
                BackupPointsCount = (int)summaryInfo.BackupPoints.Number,
                NormalPointsCount = (int)summaryInfo.NormalPoints.Number,
                UnsetPointsCount = (int)summaryInfo.UnsetPoints.Number,
                UnsetBoardsCount = (int)summaryInfo.UnsetBoards.Number,
                TotalPointsCount = (int)summaryInfo.TotalPoints.Number,
                RedundancyRates = summaryInfo.RedundancyRates
            });
        }

        AppendLog($"📊 已加载 {CabinetAlarmInfos.Count} 个机柜的详细统计信息");
    }

    [RelayCommand]
    private void RefreshStatistics()
    {
        RefreshAlarmStatistics();
    }

    [RelayCommand]
    private async Task AddAllPoints(string pointType)
    {
        if (pointType == "Alarm")
        {
            // 打开报警点添加窗口
            var window = new Views.Windows.PointAddWindow(excel, context);
            await window.InitializeDataAsync(parentVm, null);
            window.Owner = Application.Current.MainWindow;
            
            if (window.ShowDialog() == true)
            {
                // 窗口关闭后刷新统计
                RefreshAlarmStatistics();
                await message.SuccessAsync("报警点添加成功！");
            }
        }
        else if (pointType == "BackUp")
        {
            // 添加备用点
            await AddBackupPointsForAllCabinets();
        }
    }

    [RelayCommand]
    private async Task DeleteAllPoints(string pointType)
    {
        if (pointType == "Alarm")
        {
            await DeleteAlarmPointsInternal(null);
        }
        else if (pointType == "BackUp")
        {
            await DeleteBackupPointsForAllCabinets();
        }
    }

    [RelayCommand]
    private async Task AddCabinetPoints(string cabinetName)
    {
        // 打开报警点添加窗口，传入指定机柜
        var window = new Views.Windows.PointAddWindow(excel, context);
        await window.InitializeDataAsync(parentVm, cabinetName);
        window.Owner = Application.Current.MainWindow;
        
        if (window.ShowDialog() == true)
        {
            // 窗口关闭后刷新统计
            RefreshAlarmStatistics();
            Task.Run(async () => await message.SuccessAsync("报警点添加成功！"));
        }
    }

    [RelayCommand]
    private async Task DeleteCabinetPoints(string cabinetName)
    {
        await DeleteAlarmPointsInternal(cabinetName);
    }

    // 旧的AddAlarmPointsInternal方法已废弃，现在统一使用PointAddWindow + FormularHelper.AllocateCommunicationAlarmPoints

    private async Task DeleteAlarmPointsInternal(string? targetCabinet)
    {
        try
        {
            if (parentVm?.AllData == null)
            {
                await message.AlertAsync("无法获取IO数据");
                return;
            }

            string scope = targetCabinet == null ? "所有机柜的" : $"机柜 {targetCabinet} 的";
            if (!await message.ConfirmAsync($"确定要删除{scope}全部报警点吗？此操作不可撤销！"))
                return;

            AppendLog($"\n🗑️ 开始删除{scope}报警点...");

            int beforeCount = parentVm.AllData.Count;
            
            if (targetCabinet == null)
            {
                parentVm.AllData.RemoveWhere(p => 
                    p.PointType == TagType.Alarm || 
                    p.PointType == TagType.CommunicationAlarm);
            }
            else
            {
                parentVm.AllData.RemoveWhere(p => 
                    (p.PointType == TagType.Alarm || p.PointType == TagType.CommunicationAlarm) &&
                    p.CabinetNumber == targetCabinet);
            }

            int deletedCount = beforeCount - parentVm.AllData.Count;

            AppendLog($"✅ 删除完成，共删除 {deletedCount} 个报警点");

            await parentVm.SaveAndUploadFileAsync();
            AppendLog("💾 数据已保存");

            RefreshAlarmStatistics();
        }
        catch (Exception ex)
        {
            AppendLog($"❌ 删除失败: {ex.Message}");
            await message.AlertAsync($"删除失败：{ex.Message}");
        }
    }

    // 旧的GenerateAlarmPointsForCabinet和GetCardTypeConfig方法已废弃，现在统一使用FormularHelper.AllocateCommunicationAlarmPoints

    private void AppendLog(string message)
    {
        LogText += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
    }

    // ======== 备用点管理功能 ========

    private async Task AddBackupPointsForAllCabinets()
    {
        try
        {
            if (parentVm?.AllData == null)
            {
                await message.ErrorAsync("无法获取IO数据，请确保已打开项目");
                return;
            }

            model.Status.Busy("正在添加备用点...");
            AppendLog("\n🚀 开始添加全部机柜的备用点...");

            int totalAdded = 0;

            foreach (var cabinet in parentVm.AllData.GroupBy(d => d.CabinetNumber))
            {
                if (string.IsNullOrEmpty(cabinet.Key))
                    continue;

                AppendLog($"\n🔧 处理机柜: {cabinet.Key}");
                var cages = cabinet.ToList().GroupBy(c => c.Cage);
                
                foreach (var cage in cages)
                {
                    var slots = cage.ToList().GroupBy(c => c.Slot);
                    
                    foreach (var slot in slots)
                    {
                        var list = slot.ToList();
                        var cardType = slot.FirstOrDefault()?.CardType;
                        
                        if (cardType == null || cardType.Contains("DP") || cardType.Contains("FF"))
                            continue;

                        var configCardType = context.Db.Queryable<config_card_type_judge>()
                            .First(c => c.IoCardType == cardType);
                        
                        if (configCardType == null)
                            continue;

                        for (int i = 1; i <= configCardType.PinsCount; i++)
                        {
                            if (list.All(l => l.Channel != i))
                            {
                                var lastTag = list.FirstOrDefault();
                                if (lastTag == null)
                                    continue;

                                var point = new IoFullData()
                                {
                                    CabinetNumber = cabinet.Key,
                                    SignalPositionNumber = $"{cabinet.Key}{cage.Key}{slot.Key:00}{cardType.Substring(0, 2)}CH{i:00}",
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
                                parentVm.AllData.Add(point);
                                totalAdded++;
                            }
                        }
                    }
                }
            }

            model.Status.Success("备用点添加完毕");
            AppendLog($"\n✅ 备用点添加完毕！");
            AppendLog($"📊 总计添加: {totalAdded} 个备用点");

            await parentVm.Recalc();
            await parentVm.SaveAndUploadFileAsync();
            AppendLog("💾 数据已保存");

            RefreshAlarmStatistics();
        }
        catch (Exception ex)
        {
            model.Status.Reset();
            AppendLog($"❌ 操作失败: {ex.Message}");
            await message.ErrorAsync($"添加备用点失败：{ex.Message}");
        }
    }

    private async Task DeleteBackupPointsForAllCabinets()
    {
        try
        {
            if (parentVm?.AllData == null)
            {
                await message.ErrorAsync("无法获取IO数据，请确保已打开项目");
                return;
            }

            var backupCount = parentVm.AllData.Count(x => x.PointType == TagType.BackUp);
            if (backupCount == 0)
            {
                await message.MessageAsync("没有备用点可删除");
                return;
            }

            if (!await message.ConfirmAsync($"确认删除全部 {backupCount} 个备用点吗？"))
                return;

            model.Status.Busy("正在删除备用点...");
            AppendLog("\n🗑️ 开始删除全部备用点...");

            parentVm.AllData.RemoveWhere(x => x.PointType == TagType.BackUp);

            model.Status.Success("备用点删除完毕");
            AppendLog($"✅ 已删除 {backupCount} 个备用点");

            await parentVm.SaveAndUploadFileAsync();
            await parentVm.Refresh();
            AppendLog("💾 数据已保存");

            RefreshAlarmStatistics();
        }
        catch (Exception ex)
        {
            model.Status.Reset();
            AppendLog($"❌ 操作失败: {ex.Message}");
            await message.ErrorAsync($"删除备用点失败：{ex.Message}");
        }
    }
}
