﻿﻿﻿﻿﻿﻿﻿﻿﻿using IODataPlatform.Models;
using IODataPlatform.Views.Pages;

namespace IODataPlatform.Views.Windows;

public partial class MainWindowViewModel(GlobalModel model) : ObservableObject {

    public Status Status { get; } = model.Status;

    public UserInfo User { get; } = model.User;  
   

    [ObservableProperty]
    private string _applicationTitle = "IO数据管理平台";

    private static NavigationViewItem NavItem<T>(string content, SymbolRegular icon, Visibility visibility = Visibility.Collapsed) => 
        new() { Content = content, Icon = new SymbolIcon(icon), TargetPageType = typeof(T), ToolTip = content, Visibility = visibility };

    [ObservableProperty]
    private ObservableCollection<NavigationViewItem> menuItems = [
        NavItem<HomePage>("首页", SymbolRegular.Home24),
        NavItem<ProjectPage>("项目管理", SymbolRegular.TextNumberListLtr24),
        NavItem<DepXT1Page>("系统一室生成IO", SymbolRegular.DocumentPrint32),
        NavItem<DepXT2Page>("系统二室生成IO", SymbolRegular.DocumentPrint48),
        NavItem<DepYJPage>("硬件室生成IO", SymbolRegular.Info32),
        NavItem<DepAQJPage>("安全级室生成IO", SymbolRegular.FlagClock32),
        NavItem<TerminationPage>("生成与发布端接", SymbolRegular.PlugDisconnected24),
        NavItem<CablePage>("生成与发布电缆", SymbolRegular.Connected20),
        NavItem<DataAssetCenterPage>("数据资产中心", SymbolRegular.Home24),
        NavItem<DataComparePage>("数据对比", SymbolRegular.ColumnDoubleCompare20),
        NavItem<WordFormatBrushPage>("Word格式刷", SymbolRegular.PaintBrush24),
        NavItem<FormulaEditorPage>("公式编辑器", SymbolRegular.MathFormula24),
        NavItem<DocumentManagementPage>("文档管理", SymbolRegular.DocumentText24),
        NavItem<PaperPage>("图纸管理", SymbolRegular.Album24),
        NavItem<OtherFunctionPage>("其他功能", SymbolRegular.ShiftsTeam24),               
    ];

    public ObservableCollection<NavigationViewItem> FooterMenuItems { get; } = [
        NavItem<SettingsPage>("系统", SymbolRegular.Settings24, Visibility.Visible),
    ];

}