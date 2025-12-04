using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.Configs;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Microsoft.Extensions.Options;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 首页视图模型类
/// 应用程序主页的ViewModel，负责显示应用版本信息、用户信息和快捷操作
/// 提供禅道链接访问、用户手册下载等核心功能的入口
/// 实现INavigationAware接口以支持页面导航生命周期管理
/// </summary>
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class HomeViewModel : ObservableObject, INavigationAware
{
	/// <summary>禅道项目管理系统的访问URL地址</summary>
	[ObservableProperty]
	public string zentaoUrl;

	/// <summary>当前应用程序的版本号信息</summary>
	[ObservableProperty]
	public string appVersion;

	/// <summary>页面初始化状态标记，防止重复初始化</summary>
	private bool _isInitialized;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.HomeViewModel.CopyToClipboardCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<string>? copyToClipboardCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.HomeViewModel.GoWorkPageCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? goWorkPageCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.HomeViewModel.DownLoadHelpDocCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? downLoadHelpDocCommand;

	/// <summary>获取当前登录用户信息，用于页面显示和权限控制</summary>
	public UserInfo User { get; }

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.HomeViewModel.zentaoUrl" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string ZentaoUrl
	{
		get
		{
			return zentaoUrl;
		}
		[MemberNotNull("zentaoUrl")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(zentaoUrl, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ZentaoUrl);
				zentaoUrl = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ZentaoUrl);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.HomeViewModel.appVersion" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string AppVersion
	{
		get
		{
			return appVersion;
		}
		[MemberNotNull("appVersion")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(appVersion, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.AppVersion);
				appVersion = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.AppVersion);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.HomeViewModel.CopyToClipboard(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<string> CopyToClipboardCommand => copyToClipboardCommand ?? (copyToClipboardCommand = new AsyncRelayCommand<string>(CopyToClipboard));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.HomeViewModel.GoWorkPage" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand GoWorkPageCommand => goWorkPageCommand ?? (goWorkPageCommand = new AsyncRelayCommand(GoWorkPage));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.HomeViewModel.DownLoadHelpDoc" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand DownLoadHelpDocCommand => downLoadHelpDocCommand ?? (downLoadHelpDocCommand = new AsyncRelayCommand(DownLoadHelpDoc));

	/// <summary>
	/// 首页视图模型类
	/// 应用程序主页的ViewModel，负责显示应用版本信息、用户信息和快捷操作
	/// 提供禅道链接访问、用户手册下载等核心功能的入口
	/// 实现INavigationAware接口以支持页面导航生命周期管理
	/// </summary>
	public HomeViewModel(SqlSugarContext context, INavigationService navigation, GlobalModel model, IPickerService picker, ExcelService excel, StorageService storage, IMessageService message, IOptions<OtherPlatFormConfig> config)
	{
		_003Cmodel_003EP = model;
		_003Cstorage_003EP = storage;
		_003Cmessage_003EP = message;
		_003Cconfig_003EP = config;
		User = _003Cmodel_003EP.User;
		base._002Ector();
	}

	/// <summary>
	/// 页面导航到此页面时触发
	/// 首次访问时执行初始化操作，获取应用版本和外部系统配置信息
	/// </summary>
	public void OnNavigatedTo()
	{
		if (!_isInitialized)
		{
			InitializeViewModel();
		}
	}

	/// <summary>
	/// 页面导航离开时触发
	/// 当前实现为空，预留用于后续功能扩展
	/// </summary>
	public void OnNavigatedFrom()
	{
	}

	/// <summary>
	/// 初始化视图模型数据
	/// 获取应用程序版本信息和禅道系统URL配置
	/// 设置初始化完成标记以避免重复初始化
	/// </summary>
	private void InitializeViewModel()
	{
		AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
		ZentaoUrl = _003Cconfig_003EP.Value.ZentaoUrl;
		_isInitialized = true;
	}

	/// <summary>
	/// 复制文本到剪贴板命令
	/// 将指定的文本内容复制到系统剪贴板，并显示成功提示
	/// </summary>
	/// <param name="param">要复制到剪贴板的文本内容</param>
	/// <returns>异步任务，表示复制操作的完成</returns>
	[RelayCommand]
	private async Task CopyToClipboard(string param)
	{
		Clipboard.SetText(param);
		await _003Cmessage_003EP.SuccessAsync("已复制到剪贴板：" + param);
	}

	/// <summary>
	/// 跳转到工作页面命令
	/// 预留的命令方法，用于导航到主要工作界面
	/// 当前实现为空，等待后续功能开发
	/// </summary>
	/// <returns>异步任务，表示导航操作的完成</returns>
	[RelayCommand]
	public async Task GoWorkPage()
	{
	}

	/// <summary>
	/// 下载用户手册命令
	/// 从服务器下载IO管理软件用户手册PDF文件到桌面
	/// 包含完整的下载进度提示和错误处理机制
	/// </summary>
	/// <returns>异步任务，表示下载操作的完成</returns>
	[RelayCommand]
	public async Task DownLoadHelpDoc()
	{
		try
		{
			_003Cmodel_003EP.Status.Busy("正在下载IO管理软件用户手册……");
			string sourceFileName = await _003Cstorage_003EP.DownloadtemplatesDepFileAsync("IO管理软件用户手册.pdf");
			string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "IO管理软件用户手册.pdf");
			File.Copy(sourceFileName, text);
			_003Cmodel_003EP.Status.Success("已成功下载到" + text);
		}
		catch (Exception ex)
		{
			_003Cmodel_003EP.Status.Error("下载失败：" + ex.Message);
		}
	}
}
