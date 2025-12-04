using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using IODataPlatform.Models;
using IODataPlatform.Views.Pages;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Windows;

/// <inheritdoc />
public class MainWindowViewModel : ObservableObject
{
	[ObservableProperty]
	private string _applicationTitle = "IO数据管理平台";

	[ObservableProperty]
	private ObservableCollection<NavigationViewItem> menuItems = new ObservableCollection<NavigationViewItem>
	{
		NavItem<HomePage>("首页", SymbolRegular.Home24),
		NavItem<ProjectPage>("项目管理", SymbolRegular.TextNumberListLtr24),
		NavItem<DepXT1Page>("系统一室生成IO", SymbolRegular.DocumentPrint32),
		NavItem<DepXT2Page>("系统二室生成IO", SymbolRegular.DocumentPrint48),
		NavItem<DepYJPage>("硬件室生成IO", SymbolRegular.Info32),
		NavItem<DepAQJPage>("安全级室生成IO", SymbolRegular.FlagClock32),
		NavItem<TerminationPage>("生成与发布端接", SymbolRegular.PlugDisconnected24),
		NavItem<CablePage>("生成与发布电缆", SymbolRegular.Connected20),
		NavItem<DataAssetCenterPage>("数据资产中心", SymbolRegular.Home24),
		NavItem<FormulaEditorPage>("公式编辑器", SymbolRegular.MathFormula24),
		NavItem<DocumentManagementPage>("文档管理", SymbolRegular.DocumentText24),
		NavItem<PaperPage>("图纸管理", SymbolRegular.Album24),
		NavItem<OtherFunctionPage>("其他功能", SymbolRegular.ShiftsTeam24)
	};

	public Status Status { get; } = model.Status;

	public UserInfo User { get; } = model.User;

	public ObservableCollection<NavigationViewItem> FooterMenuItems { get; } = new ObservableCollection<NavigationViewItem> { NavItem<SettingsPage>("系统", SymbolRegular.Settings24, Visibility.Visible) };

	/// <inheritdoc cref="F:IODataPlatform.Views.Windows.MainWindowViewModel._applicationTitle" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string ApplicationTitle
	{
		get
		{
			return _applicationTitle;
		}
		[MemberNotNull("_applicationTitle")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(_applicationTitle, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ApplicationTitle);
				_applicationTitle = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ApplicationTitle);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Windows.MainWindowViewModel.menuItems" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<NavigationViewItem> MenuItems
	{
		get
		{
			return menuItems;
		}
		[MemberNotNull("menuItems")]
		set
		{
			if (!EqualityComparer<ObservableCollection<NavigationViewItem>>.Default.Equals(menuItems, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.MenuItems);
				menuItems = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.MenuItems);
			}
		}
	}

	public MainWindowViewModel(GlobalModel model)
	{
	}

	private static NavigationViewItem NavItem<T>(string content, SymbolRegular icon, Visibility visibility = Visibility.Collapsed)
	{
		NavigationViewItem navigationViewItem = new NavigationViewItem();
		navigationViewItem.Content = content;
		navigationViewItem.Icon = new SymbolIcon(icon);
		navigationViewItem.TargetPageType = typeof(T);
		navigationViewItem.ToolTip = content;
		navigationViewItem.Visibility = visibility;
		return navigationViewItem;
	}
}
