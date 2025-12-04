using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using IODataPlatform.Models;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using IODataPlatform.Views.Pages;
using IODataPlatform.Views.SubPages.Common;
using IODataPlatform.Utilities;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace IODataPlatform.Views.Windows;

/// <summary>
/// 添加报警点窗口
/// 功能：从配置表读取机柜报警信息，自动生成报警点并分配
/// </summary>
public partial class PointAddWindow : Window
{
    private readonly ExcelService _excelService;
    private readonly INavigationService _navigation;
    private readonly SqlSugarContext _context;
    private DepXT2ViewModel? _parentVm;
    private string? _targetCabinetName;
    
    private string? _selectedFilePath;
    private string? _selectedSheet;
    private List<config_xt2_cabinet_alarm> _importedData = new();  // 从配置表读取
    private List<IoFullData> _previewData = new();  // 使用IoFullData作为预览数据
    private int _currentStep = 0; // 当前步骤：0=配置表数据预览, 1=生成预览
    private bool _isCommunicationAlarm = true; // 是否为通讯报警

    public PointAddWindow(ExcelService excelService, SqlSugarContext context)
    {
        InitializeComponent();
        _excelService = excelService;
        _context = context;
        _navigation = App.GetService<INavigationService>();
        
        ExcelDataGrid.ItemsSource = null;
        PreviewGrid.ItemsSource = _previewData;
        
        // 默认选择通讯报警
        CommunicationAlarmTypeRadio.IsChecked = true;
        
        UpdateButtonStates();
    }

    /// <summary>
    /// 初始化窗口数据
    /// </summary>
    public async Task InitializeDataAsync(DepXT2ViewModel parentVm, string? targetCabinetName = null)
    {
        _parentVm = parentVm;
        _targetCabinetName = targetCabinetName;

        // 更新标题和描述
        if (!string.IsNullOrEmpty(targetCabinetName))
        {
            TitleText.Text = $"为机柜 [{targetCabinetName}] 添加报警点";
            DescriptionText.Text = $"将为机柜 {targetCabinetName} 生成报警点信号，数据来源于机柜报警清单配置表";
        }
        else
        {
            TitleText.Text = "批量添加报警点";
            DescriptionText.Text = "将为所有机柜生成报警点信号，数据来源于机柜报警清单配置表";
        }
        
        // 加载配置表数据
        await LoadConfigTableData();
    }

    /// <summary>
    /// 下一步按钮点击事件
    /// </summary>
    private async void NextButton_Click(object sender, RoutedEventArgs e)
    {
        // 获取当前选择的报警类型
        _isCommunicationAlarm = CommunicationAlarmTypeRadio.IsChecked == true;
        
        if (_currentStep == 0)
        {
            // 配置表数据预览 -> 生成预览
            GeneratePreview();
        }
    }

    /// <summary>
    /// 上一步按钮点击事件
    /// </summary>
    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStep == 1)
        {
            // 生成预览 -> 配置表数据预览
            _currentStep = 0;
            DataTabControl.SelectedItem = ExcelDataTab;
            UpdateButtonStates();
        }
    }

    /// <summary>
    /// 从配置表加载数据
    /// </summary>
    private async Task LoadConfigTableData()
    {
        try
        {
            StatusText.Text = "正在从配置表读取数据...";
            
            // 从数据库读取配置表数据
            _importedData = await _context.Db.Queryable<config_xt2_cabinet_alarm>().ToListAsync();
            
            if (!_importedData.Any())
            {
                MessageBox.Show("配置表中没有数据，请先导入机柜报警配置表", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // 转换为DataTable显示
            var dataTable = new DataTable();
            var properties = typeof(config_xt2_cabinet_alarm).GetProperties()
                .Where(p =>
                {
                    var displayAttr = p.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                    return displayAttr == null || displayAttr.GetAutoGenerateField() != false;
                })
                .ToList();
            
            foreach (var prop in properties)
            {
                var displayAttr = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                var columnName = displayAttr?.Name ?? prop.Name;
                dataTable.Columns.Add(columnName);
            }
            
            foreach (var item in _importedData)
            {
                var row = dataTable.NewRow();
                for (int i = 0; i < properties.Count; i++)
                {
                    row[i] = properties[i].GetValue(item)?.ToString() ?? "";
                }
                dataTable.Rows.Add(row);
            }
            
            ExcelDataGrid.ItemsSource = dataTable.DefaultView;
            ExcelDataCountText.Text = $"机柜数据：{_importedData.Count}条";
            StatusText.Text = $"已加载 {_importedData.Count} 条机柜数据";
            
            _currentStep = 0;
            UpdateButtonStates();
            
            // 切换到Excel数据 Tab
            DataTabControl.SelectedItem = ExcelDataTab;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载配置表失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 生成预览
    /// </summary>
    private void GeneratePreview()
    {
        try
        {
            StatusText.Text = "正在生成报警点预览...";
            
            _previewData.Clear();
            
            int totalGenerated = 0;

            if (_isCommunicationAlarm)
            {
                // 通讯报警：从配置表数据解析并生成（根据数量动态生成）
                foreach (var cabinet in _importedData)
                {
                    // 如果指定了机柜，只处理该机柜
                    if (!string.IsNullOrEmpty(_targetCabinetName) && cabinet.CabinetName != _targetCabinetName)
                        continue;

                    // 解析报警配置：根据配置表中的数量动态生成
                    var alarmConfigs = new[]
                    {
                        ("机柜电源A故障", "bool", "PWAF", cabinet.PowerAFault),
                        ("机柜电源B故障", "bool", "PWBF", cabinet.PowerBAlarmFault),
                        ("机柜门开", "bool", "DROP", cabinet.DoorOpen),
                        ("机柜温度高报警", "bool", "TEPH", cabinet.HighTemperatureAlarm),
                        ("风扇故障", "bool", "FAN", cabinet.FanFault),
                        ("网络故障", "bool", "SWF", cabinet.NetworkFault),
                        ("温度", "real", "TEP", cabinet.Temperature),
                        ("综合故障", "bool", "ALM", cabinet.ComprehensiveAlarm)
                    };

                    foreach (var (alarmName, dataType, extCode, count) in alarmConfigs)
                    {
                        // 网络故障特殊处理：只有值为1时才生成
                        if (alarmName == "网络故障" && count != 1)
                            continue;

                        // 解析RTU板卡位置（格式如：1-0-04）
                        string portName = "";      // 端口名称（COM0/COM1）
                        string portNumber = "";    // 端口号（0/1）
                        int channelNumber = 0;     // 通道号
                        if (!string.IsNullOrEmpty(cabinet.RTUBoardPosition))
                        {
                            var parts = cabinet.RTUBoardPosition.Split('-');
                            if (parts.Length >= 3)
                            {
                                // 提取中间位：端口号（0或1）
                                if (int.TryParse(parts[1], out int portNum))
                                {
                                    portName = portNum == 0 ? "COM0" : "COM1";  // 端口名称（用于ChannelAddress）
                                    portNumber = portNum.ToString();  // 端口号：0或1（用于NetType）
                                }
                                // 提取最后一位：通道号
                                int.TryParse(parts[2], out channelNumber);
                            }
                        }

                        // 根据数量生成对应数量的信号（不添加序号）
                        for (int i = 0; i < count; i++)
                        {
                            var point = new IoFullData
                            {
                                IoType = dataType == "bool" ? "DI" : "AI",  // bool→DI, real→AI
                                ExtensionCode = extCode,
                                SignalPositionNumber = cabinet.CabinetName,  // 不添加序号
                                TagName = cabinet.CabinetName + extCode,  // 不添加序号
                                Description = cabinet.CabinetType + cabinet.CabinetName + alarmName,  // 不添加序号
                                SystemCode = "ALARM",
                                CabinetNumber = cabinet.RTUCabinetNumber,  // 直接从配置表读取
                                NetType = portNumber,  // 端口号：0或1（从RTU板卡位置解析）
                                PointType = TagType.CommunicationAlarm  // 通讯报警
                                // Channel 和 ChannelAddress 在分配时设置
                            };
                            
                            _previewData.Add(point);
                            totalGenerated++;
                        }
                    }
                }
            }
            else
            {
                // 硬点报警：直接从机柜列表生成（固定6个）
                if (_parentVm == null)
                {
                    MessageBox.Show("未初始化数据", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 从 AllData 中获取机柜列表
                var cabinets = _parentVm.AllData?.GroupBy(x => x.CabinetNumber).Select(g => g.Key).ToList();
                
                if (cabinets == null || !cabinets.Any())
                {
                    MessageBox.Show("没有可用的机柜数据", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // 固定6个硬点报警
                var hardwareAlarms = new[]
                {
                    ("机柜电源A故障", "bool", "PWAF"),
                    ("机柜电源B故障", "bool", "PWBF"),
                    ("机柜门开", "bool", "DROP"),
                    ("机柜温度高报警", "bool", "TEPH"),
                    ("风扇故障", "bool", "FAN"),
                    ("网络故障", "bool", "SWF")
                };
                
                foreach (var cabinetName in cabinets)
                {
                    // 如果指定了机柜，只处理该机柜
                    if (!string.IsNullOrEmpty(_targetCabinetName) && cabinetName != _targetCabinetName)
                        continue;

                    // 获取机柜类型（假设是"控制柜"）
                    string cabinetType = "控制柜";  // TODO: 从数据中获取实际机柜类型

                    foreach (var (alarmName, dataType, extCode) in hardwareAlarms)
                    {
                        var point = new IoFullData
                        {
                            IoType = "DI",  // 硬点报警全是DI
                            ExtensionCode = extCode,
                            SignalPositionNumber = cabinetName,
                            TagName = cabinetName + extCode,
                            Description = cabinetType + cabinetName + alarmName,
                            SystemCode = "JIGUIBAOJING",  // 硬点报警使用机柜报警系统代码
                            PointType = TagType.Alarm  // 硬点报警
                        };
                        
                        _previewData.Add(point);
                        totalGenerated++;
                    }
                }
            }

            PreviewGrid.Items.Refresh();
            
            GeneratedCountText.Text = totalGenerated.ToString();
            AllocatedCountText.Text = "0";  // 预览阶段不分配
            UnallocatedCountText.Text = totalGenerated.ToString();
            
            StatusText.Text = $"预览生成完成，共 {totalGenerated} 个报警点";
            
            _currentStep = 1;
            UpdateButtonStates();
            ConfirmButton.IsEnabled = totalGenerated > 0;
            
            // 切换到预览Tab
            DataTabControl.SelectedItem = PreviewTab;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"生成预览失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 确认添加按钮点击事件
    /// </summary>
    private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = MessageBox.Show(
                $"确认要添加这些报警点吗？\n\n将生成 {_previewData.Count} 个报警点\n\n此操作将修改IO数据！",
                "确认添加",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                StatusText.Text = "正在添加报警点...";
                
                if (_isCommunicationAlarm)
                {
                    // 通讯报警：添加到AllData后调用分配方法
                    await AddCommunicationAlarmPoints();
                }
                else
                {
                    // 硬点报警：直接添加到未分配列表
                    await AddHardwareAlarmPoints();
                }
                
                DialogResult = true;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"添加失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>
    /// 添加通讯报警点：先删除旧的，然后添加新的并调用分配方法
    /// </summary>
    private async Task AddCommunicationAlarmPoints()
    {
        if (_parentVm == null || _previewData == null || !_previewData.Any())
        {
            MessageBox.Show("预览数据为空，请先生成预览", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // 步骤1：删除旧的通讯报警点
        StatusText.Text = "正在删除旧的通讯报警点...";
        var oldAlarmPoints = _parentVm.AllData
            .Where(p => p.PointType == TagType.CommunicationAlarm)
            .ToList();
        
        int deletedCount = oldAlarmPoints.Count;
        foreach (var old in oldAlarmPoints)
        {
            _parentVm.AllData.Remove(old);
        }
        
        if (deletedCount > 0)
        {
            StatusText.Text = $"已删除 {deletedCount} 个旧的通讯报警点";
            await Task.Delay(300); // 略微延迟，让用户看到提示
        }

        // 步骤2：检查是否有报警预留板卡
        StatusText.Text = "正在检查预留板卡...";
        var reservedCards = _parentVm.AllData
            .Where(p => p.PointType == TagType.AlarmReserved)
            .ToList();
        
        if (!reservedCards.Any())
        {
            var result = MessageBox.Show(
                "警告：系统中没有找到报警预留板卡！\n\n" +
                "通讯报警点需要分配到预留板卡上，否则将无法分配。\n\n" +
                "是否仍要继续添加？（报警点将被添加到未分配列表）",
                "无预留板卡",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.No)
            {
                StatusText.Text = "已取消添加";
                return;
            }
        }
        else
        {
            // 统计预留板卡信息
            var cabinets = reservedCards.Select(p => p.CabinetNumber).Distinct().Count();
            var totalSlots = reservedCards.GroupBy(p => new { p.CabinetNumber, p.Cage, p.Slot }).Count();
            StatusText.Text = $"找到 {reservedCards.Count} 个预留位置（{cabinets} 个机柜，{totalSlots} 个插槽）";
            await Task.Delay(300);
        }

        // 步骤3：添加新的报警点到AllData
        StatusText.Text = "正在添加新的报警点...";
        foreach (var previewPoint in _previewData)
        {
            var point = new IoFullData
            {
                IoType = previewPoint.IoType,
                ExtensionCode = previewPoint.ExtensionCode,
                SignalPositionNumber = previewPoint.SignalPositionNumber,
                TagName = previewPoint.TagName,
                Description = previewPoint.Description,
                SystemCode = previewPoint.SystemCode,
                CabinetNumber = previewPoint.CabinetNumber,
                NetType = previewPoint.NetType,
                PointType = TagType.CommunicationAlarm,
                Version = "A",
                ModificationDate = DateTime.Now,
                Cage = 0,  // 未分配状态
                Slot = 0,
                Channel = 0
            };
            _parentVm.AllData.Add(point);
        }

        // 步骤4：调用通讯报警分配方法
        StatusText.Text = "正在分配到预留板卡...";
        var formularHelper = new FormularHelper();
        var configs = await _context.Db.Queryable<config_card_type_judge>().ToListAsync();
        
        var (total, allocated, unallocated) = formularHelper.AllocateCommunicationAlarmPoints(
            _parentVm.AllData.ToList(),
            configs,
            null  // 分配所有机柜
        );

        // 步骤5：重新计算和保存
        StatusText.Text = "正在重新计算...";
        await _parentVm.RecalcMethodInternal(ControlSystem.龙鳍, null, showStatus: false);
        
        StatusText.Text = "正在保存数据...";
        await _parentVm.SaveAndUploadFileAsync();
        
        StatusText.Text = $"完成！已分配 {allocated} 个，未分配 {unallocated} 个";
        
        // 步骤6：提示结果
        var message = $"报警点添加完成！\n\n";
        if (deletedCount > 0)
        {
            message += $"删除旧的：{deletedCount} 个\n";
        }
        message += $"总计：{total} 个\n" +
                   $"已分配：{allocated} 个\n" +
                   $"未分配：{unallocated} 个\n\n" +
                   $"是否立即打开预览界面查看？";
        
        var openPreview = MessageBox.Show(
            message,
            "成功",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);
        
        if (openPreview == MessageBoxResult.Yes)
        {
            // 设置控制系统参数并打开预览界面
            var parameterService = App.GetService<NavigationParameterService>();
            parameterService.SetParameter("controlSystem", ControlSystem.龙鳍);
            _navigation.NavigateWithHierarchy(typeof(CabinetAllocatedPage));
        }
    }

    /// <summary>
    /// 添加硬点报警：直接添加固定的6个报警点到未分配列表
    /// </summary>
    private async Task AddHardwareAlarmPoints()
    {
        if (_parentVm == null)
        {
            MessageBox.Show("数据未初始化", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        StatusText.Text = "正在添加硬点报警...";
        
        // 直接将预览数据添加到AllData
        foreach (var previewPoint in _previewData)
        {
            var point = new IoFullData
            {
                IoType = previewPoint.IoType,
                ExtensionCode = previewPoint.ExtensionCode,
                SignalPositionNumber = previewPoint.SignalPositionNumber,
                TagName = previewPoint.TagName,
                Description = previewPoint.Description,
                SystemCode = previewPoint.SystemCode,
                PointType = TagType.Alarm,
                UnsetReason = "用户选择不分配",
                Version = "A",
                ModificationDate = DateTime.Now,
                Cage = 0,
                Slot = 0,
                Channel = 0
            };
            _parentVm.AllData.Add(point);
        }
        
        // 重新计算和保存
        StatusText.Text = "正在重新计算...";
        await _parentVm.RecalcMethodInternal(ControlSystem.龙鳍, null, showStatus: false);
        
        StatusText.Text = "正在保存数据...";
        await _parentVm.SaveAndUploadFileAsync();
        
        MessageBox.Show(
            $"硬点报警添加完成！\n\n总计：{_previewData.Count} 个\n\n已添加到未分配列表",
            "成功",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /// <summary>
    /// 更新按钮状态
    /// </summary>
    private void UpdateButtonStates()
    {
        switch (_currentStep)
        {
            case 0: // 配置表数据预览
                PreviousButton.Visibility = Visibility.Collapsed;
                NextButton.Visibility = Visibility.Visible;
                NextButton.IsEnabled = _importedData.Any();
                ConfirmButton.Visibility = Visibility.Collapsed;
                NextButton.Content = "生成预览";
                break;
            case 1: // 生成预览
                PreviousButton.Visibility = Visibility.Visible;
                NextButton.Visibility = Visibility.Collapsed;
                ConfirmButton.Visibility = Visibility.Visible;
                ConfirmButton.IsEnabled = _previewData.Any();
                break;
        }
    }
}
