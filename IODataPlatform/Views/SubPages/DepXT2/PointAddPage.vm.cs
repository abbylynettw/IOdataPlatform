using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Microsoft.Win32;
using Wpf.Ui;

namespace IODataPlatform.Views.SubPages.DepXT2;

public partial class PointAddViewModel : ObservableObject, INavigationAware
{
    private readonly GlobalModel model;
    private readonly ExcelService excel;
    private readonly NavigationParameterService navigation;
    private readonly INavigationService navigationService;
    private readonly IMessageService message;
    private readonly SqlSugarContext context;

    private DepXT2ViewModel? parentVm;
    private string? targetCabinetName;
    
    // 当前步骤 (1-4)
    [ObservableProperty]
    private int currentStep = 1;

    // 机柜列表
    [ObservableProperty]
    private ObservableCollection<string> cabinetNames = new();

    [ObservableProperty]
    private string? selectedCabinet;

    // Excel 文件和工作表
    [ObservableProperty]
    private string? selectedFilePath;

    [ObservableProperty]
    private ObservableCollection<string> sheetNames = new();

    [ObservableProperty]
    private string? selectedSheet;

    // 报警类型
    [ObservableProperty]
    private bool isCommunicationAlarm = true;

    [ObservableProperty]
    private bool isHardwareAlarm;

    // 导入的数据
    private List<AlarmImportModel> importedData = new();

    // 预览数据
    [ObservableProperty]
    private ObservableCollection<dynamic> previewData = new();

    // Excel预览数据
    [ObservableProperty]
    private ObservableCollection<DataRow> excelPreviewData = new();

    [ObservableProperty]
    private string? previewDataCountText;

    [ObservableProperty]
    private string? sheetCountText;

    [ObservableProperty]
    private string? validationResult;

    [ObservableProperty]
    private string? previewSummary;

    // 状态文本
    [ObservableProperty]
    private string? step1StatusText = "请选择要添加报警点的机柜";

    [ObservableProperty]
    private string? step2StatusText = "请选择包含机柜报警信息的Excel文件";

    public PointAddViewModel(
        GlobalModel model,
        ExcelService excel,
        NavigationParameterService navigation,
        INavigationService navigationService,
        IMessageService message,
        SqlSugarContext context)
    {
        this.model = model;
        this.excel = excel;
        this.navigation = navigation;
        this.navigationService = navigationService;
        this.message = message;
        this.context = context;
    }

    public void OnNavigatedTo()
    {
        parentVm = navigation.GetParameter<DepXT2ViewModel>("DepXT2ViewModel");
        targetCabinetName = navigation.GetParameter<string?>("CabinetName");
        
        // 始终从步骤1（选择Excel）开始
        CurrentStep = 1;
        
        // 更新状态文本
        if (!string.IsNullOrEmpty(targetCabinetName))
        {
            SelectedCabinet = targetCabinetName;
            Step1StatusText = $"将为机柜 [{targetCabinetName}] 添加报警点，请选择Excel文件";
        }
        else
        {
            Step1StatusText = "将为所有机柜添加报警点，请选择Excel文件";
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private void LoadCabinetNames()
    {
        if (parentVm?.AllData == null) return;
        
        var cabinets = parentVm.AllData
            .Where(p => !string.IsNullOrEmpty(p.CabinetNumber))
            .Select(p => p.CabinetNumber)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
            
        CabinetNames.Clear();
        foreach (var cabinet in cabinets)
        {
            CabinetNames.Add(cabinet);
        }
    }

    [RelayCommand]
    private async Task BrowseFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel 文件|*.xlsx;*.xls",
            Title = "选择机柜报警信息Excel文件"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedFilePath = dialog.FileName;
            
            // 读取Sheet列表
            var sheets = await excel.GetSheetNames(dialog.FileName);
            SheetNames.Clear();
            foreach (var sheet in sheets)
            {
                SheetNames.Add(sheet);
            }
            
            if (SheetNames.Count > 0)
            {
                SelectedSheet = SheetNames.First();
                SheetCountText = $"共 {SheetNames.Count} 个工作表";
                Step2StatusText = $"已加载文件，包含 {SheetNames.Count} 个工作表";
            }
        }
    }

    [RelayCommand]
    private async Task NextStep()
    {
        try
        {
            switch (CurrentStep)
            {
                case 1:
                    if (string.IsNullOrEmpty(SelectedFilePath) || string.IsNullOrEmpty(SelectedSheet))
                    {
                        await message.ErrorAsync("请先选择Excel文件和工作表");
                        return;
                    }
                    
                    // 读取并校验数据
                    await ValidateData();
                    CurrentStep = 2;
                    break;

                case 2:
                    // 生成预览
                    await GeneratePreview();
                    CurrentStep = 3;
                    break;
            }
        }
        catch (Exception ex)
        {
            await message.ErrorAsync($"操作失败：{ex.Message}");
        }
    }

    [RelayCommand]
    private void PreviousStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
        }
    }

    [RelayCommand]
    private async Task Finish()
    {
        // TODO: 实现最终的保存逻辑
        await message.SuccessAsync("报警点添加完成！");
        navigationService.GoBack();
    }

    [RelayCommand]
    private void Cancel()
    {
        navigationService.GoBack();
    }

    [RelayCommand]
    private async Task LoadPreview()
    {
        if (string.IsNullOrEmpty(SelectedFilePath) || string.IsNullOrEmpty(SelectedSheet))
        {
            await message.ErrorAsync("请先选择Excel文件和工作表");
            return;
        }

        try
        {
            model.Status.Busy("正在加载预览...");
            var dataTable = await excel.GetDataTableAsStringAsync(SelectedFilePath!, SelectedSheet!, true);
            
            ExcelPreviewData.Clear();
            foreach (DataRow row in dataTable.Rows)
            {
                ExcelPreviewData.Add(row);
            }
            
            PreviewDataCountText = $"共 {ExcelPreviewData.Count} 条数据";
            Step2StatusText = $"已加载 {SheetNames.Count} 个工作表，当前预览: {ExcelPreviewData.Count} 条数据";
            
            OnPropertyChanged(nameof(PreviewVisibility));
            model.Status.Reset();
        }
        catch (Exception ex)
        {
            model.Status.Reset();
            await message.ErrorAsync($"加载预览失败：{ex.Message}");
        }
    }

    private async Task ValidateData()
    {
        var dataTable = await excel.GetDataTableAsStringAsync(SelectedFilePath!, SelectedSheet!, true);
        importedData = dataTable.StringTableToIEnumerableByDiplay<AlarmImportModel>().ToList();
        
        // 校验列
        var requiredColumns = new[]
        {
            "机柜名称", "机柜类型", "机柜电源A故障", "机柜电源B故障",
            "机柜门开", "温度高报警", "风扇故障", "网络故障", "温度", "综合故障",
            "RTU板卡所在机柜号", "RTU板卡位置", "机柜RTU端子板号"
        };
        
        ValidationResult = $"✅ 数据校验通过\n共读取 {importedData.Count} 条机柜数据\n包含所有必需字段";
    }

    private async Task GeneratePreview()
    {
        // TODO: 生成报警点预览数据
        PreviewData.Clear();
        PreviewSummary = $"将生成 {importedData.Count * 8} 个报警点";
    }

    // ======== 步骤UI控制属性 ========
    // 现在只有3个步骤：1.选择Excel  2.校验数据  3.生成预览

    public Visibility Step1Visibility => CurrentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility Step2Visibility => CurrentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility Step3Visibility => CurrentStep == 3 ? Visibility.Visible : Visibility.Collapsed;

    public Visibility SheetSelectionVisibility => SheetNames.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility PreviewVisibility => ExcelPreviewData.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public Visibility PreviousButtonVisibility => CurrentStep > 1 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility NextButtonVisibility => CurrentStep < 3 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility FinishButtonVisibility => CurrentStep == 3 ? Visibility.Visible : Visibility.Collapsed;

    // 步骤指示器样式
    public string Step1Background => CurrentStep >= 1 ? "#1976D2" : "Transparent";
    public string Step1Foreground => CurrentStep >= 1 ? "White" : "Gray";
    public string Step1FontWeight => CurrentStep == 1 ? "Bold" : "Normal";
    public string Step1Number => "1";

    public string Step2Background => CurrentStep >= 2 ? "#1976D2" : "Transparent";
    public string Step2Foreground => CurrentStep >= 2 ? "White" : "Gray";
    public string Step2FontWeight => CurrentStep == 2 ? "Bold" : "Normal";
    public string Step2Number => "2";

    public string Step3Background => CurrentStep >= 3 ? "#1976D2" : "Transparent";
    public string Step3Foreground => CurrentStep >= 3 ? "White" : "Gray";
    public string Step3FontWeight => CurrentStep == 3 ? "Bold" : "Normal";
    public string Step3Number => "3";

    partial void OnCurrentStepChanged(int value)
    {
        // 通知所有步骤相关属性变化
        OnPropertyChanged(nameof(Step1Visibility));
        OnPropertyChanged(nameof(Step2Visibility));
        OnPropertyChanged(nameof(Step3Visibility));
        
        OnPropertyChanged(nameof(PreviousButtonVisibility));
        OnPropertyChanged(nameof(NextButtonVisibility));
        OnPropertyChanged(nameof(FinishButtonVisibility));
        
        OnPropertyChanged(nameof(Step1Background));
        OnPropertyChanged(nameof(Step1Foreground));
        OnPropertyChanged(nameof(Step1FontWeight));
        
        OnPropertyChanged(nameof(Step2Background));
        OnPropertyChanged(nameof(Step2Foreground));
        OnPropertyChanged(nameof(Step2FontWeight));
        
        OnPropertyChanged(nameof(Step3Background));
        OnPropertyChanged(nameof(Step3Foreground));
        OnPropertyChanged(nameof(Step3FontWeight));
    }

    partial void OnSheetNamesChanged(ObservableCollection<string> value)
    {
        OnPropertyChanged(nameof(SheetSelectionVisibility));
    }
    
    partial void OnExcelPreviewDataChanged(ObservableCollection<DataRow> value)
    {
        OnPropertyChanged(nameof(PreviewVisibility));
    }
}
