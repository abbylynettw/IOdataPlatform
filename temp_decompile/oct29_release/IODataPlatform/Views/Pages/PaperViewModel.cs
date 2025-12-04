using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using IODataPlatform.Views.SubPages.Paper;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 图纸管理页面视图模型类
/// 负责工程图纸的综合管理，包括图纸浏览、下载、上传和依据文件管理
/// 支持PDF和ZIP格式图纸文件的在线预览和下载功能
/// 提供图纸上传、设备管理和搜索筛选等高级功能
/// </summary>
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class PaperViewModel(SqlSugarContext context, INavigationService navigation, GlobalModel model, IPickerService picker, StorageService storage) : ObservableObject(), INavigationAware
{
	/// <summary>页面初始化状态标记，防止重复初始化操作</summary>
	private bool isInit;

	/// <summary>所有图纸数据的完整集合，用于数据筛选和搜索</summary>
	[ObservableProperty]
	private ObservableCollection<PaperDisplay>? _allData;

	/// <summary>当前界面显示的图纸数据集合，经过搜索和筛选后的结果</summary>
	[ObservableProperty]
	private ObservableCollection<PaperDisplay>? _displayData;

	[ObservableProperty]
	private ObservableCollection<config_project>? _projects;

	[ObservableProperty]
	private config_project? _project;

	[ObservableProperty]
	private DateTime dateFrom = DateTime.Today - TimeSpan.FromDays(30.0);

	[ObservableProperty]
	private DateTime dateTo = DateTime.Today;

	[ObservableProperty]
	private ObservableCollection<盘箱柜类别> deviceTypes = new ObservableCollection<盘箱柜类别>
	{
		盘箱柜类别.机柜,
		盘箱柜类别.盘台,
		盘箱柜类别.阀箱
	};

	[ObservableProperty]
	private 盘箱柜类别 deviceType = 盘箱柜类别.机柜;

	[ObservableProperty]
	private string lot = string.Empty;

	[ObservableProperty]
	private string batch = string.Empty;

	[ObservableProperty]
	private string room = string.Empty;

	[ObservableProperty]
	private string version = string.Empty;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.PaperViewModel.BrowserPdfCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<PaperDisplay>? browserPdfCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.PaperViewModel.BrowserZipCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<PaperDisplay>? browserZipCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.PaperViewModel.UploadDepCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<PaperDisplay>? uploadDepCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.PaperViewModel.ViewDepCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<PaperDisplay>? viewDepCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.PaperViewModel.EditDeviceTableCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? editDeviceTableCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.PaperViewModel.UploadPaperCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? uploadPaperCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.PaperViewModel.RefreshAllCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshAllCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.PaperViewModel.SearchCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? searchCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.PaperViewModel._allData" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<PaperDisplay>? AllData
	{
		get
		{
			return _allData;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<PaperDisplay>>.Default.Equals(_allData, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.AllData);
				_allData = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.AllData);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.PaperViewModel._displayData" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<PaperDisplay>? DisplayData
	{
		get
		{
			return _displayData;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<PaperDisplay>>.Default.Equals(_displayData, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DisplayData);
				_displayData = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DisplayData);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.PaperViewModel._projects" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<config_project>? Projects
	{
		get
		{
			return _projects;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<config_project>>.Default.Equals(_projects, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Projects);
				_projects = value;
				OnProjectsChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Projects);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.PaperViewModel._project" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public config_project? Project
	{
		get
		{
			return _project;
		}
		set
		{
			if (!EqualityComparer<config_project>.Default.Equals(_project, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Project);
				_project = value;
				OnProjectChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Project);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.PaperViewModel.dateFrom" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public DateTime DateFrom
	{
		get
		{
			return dateFrom;
		}
		set
		{
			if (!EqualityComparer<DateTime>.Default.Equals(dateFrom, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DateFrom);
				dateFrom = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DateFrom);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.PaperViewModel.dateTo" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public DateTime DateTo
	{
		get
		{
			return dateTo;
		}
		set
		{
			if (!EqualityComparer<DateTime>.Default.Equals(dateTo, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DateTo);
				dateTo = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DateTo);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.PaperViewModel.deviceTypes" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<盘箱柜类别> DeviceTypes
	{
		get
		{
			return deviceTypes;
		}
		[MemberNotNull("deviceTypes")]
		set
		{
			if (!EqualityComparer<ObservableCollection<盘箱柜类别>>.Default.Equals(deviceTypes, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DeviceTypes);
				deviceTypes = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DeviceTypes);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.PaperViewModel.deviceType" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public 盘箱柜类别 DeviceType
	{
		get
		{
			return deviceType;
		}
		set
		{
			if (!EqualityComparer<盘箱柜类别>.Default.Equals(deviceType, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DeviceType);
				deviceType = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DeviceType);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.PaperViewModel.lot" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Lot
	{
		get
		{
			return lot;
		}
		[MemberNotNull("lot")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(lot, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Lot);
				lot = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Lot);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.PaperViewModel.batch" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Batch
	{
		get
		{
			return batch;
		}
		[MemberNotNull("batch")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(batch, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Batch);
				batch = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Batch);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.PaperViewModel.room" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Room
	{
		get
		{
			return room;
		}
		[MemberNotNull("room")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(room, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Room);
				room = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Room);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.PaperViewModel.version" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Version
	{
		get
		{
			return version;
		}
		[MemberNotNull("version")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(version, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Version);
				version = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Version);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.PaperViewModel.BrowserPdf(IODataPlatform.Views.Pages.PaperDisplay)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<PaperDisplay> BrowserPdfCommand => browserPdfCommand ?? (browserPdfCommand = new AsyncRelayCommand<PaperDisplay>(BrowserPdf));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.PaperViewModel.BrowserZip(IODataPlatform.Views.Pages.PaperDisplay)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<PaperDisplay> BrowserZipCommand => browserZipCommand ?? (browserZipCommand = new RelayCommand<PaperDisplay>(BrowserZip));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.PaperViewModel.UploadDep(IODataPlatform.Views.Pages.PaperDisplay)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<PaperDisplay> UploadDepCommand => uploadDepCommand ?? (uploadDepCommand = new AsyncRelayCommand<PaperDisplay>(UploadDep));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.PaperViewModel.ViewDep(IODataPlatform.Views.Pages.PaperDisplay)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<PaperDisplay> ViewDepCommand => viewDepCommand ?? (viewDepCommand = new AsyncRelayCommand<PaperDisplay>(ViewDep));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.PaperViewModel.EditDeviceTable" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand EditDeviceTableCommand => editDeviceTableCommand ?? (editDeviceTableCommand = new RelayCommand(EditDeviceTable));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.PaperViewModel.UploadPaper" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand UploadPaperCommand => uploadPaperCommand ?? (uploadPaperCommand = new RelayCommand(UploadPaper));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.PaperViewModel.RefreshAll" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshAllCommand => refreshAllCommand ?? (refreshAllCommand = new AsyncRelayCommand(RefreshAll));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.PaperViewModel.Search" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand SearchCommand => searchCommand ?? (searchCommand = new RelayCommand(Search));

	/// <summary>
	/// 页面导航离开时触发
	/// 当前实现为空，预留用于后续清理操作
	/// </summary>
	public void OnNavigatedFrom()
	{
	}

	/// <summary>
	/// 页面导航到此页面时触发
	/// 首次访问时执行全量刷新和初始化操作
	/// 非首次访问时触发项目变更事件和搜索操作
	/// </summary>
	public async void OnNavigatedTo()
	{
		if (!isInit)
		{
			await RefreshAll();
			isInit = true;
		}
		else
		{
			OnProjectChanged(null);
			Search();
		}
	}

	/// <summary>
	/// 浏览PDF图纸命令
	/// 从服务器下载PDF图纸文件并使用默认应用程序打开
	/// 显示下载进度状态，完成后自动重置状态
	/// </summary>
	/// <param name="paper">要浏览的图纸对象</param>
	/// <returns>异步任务，表示下载和打开操作的完成</returns>
	[RelayCommand]
	private async Task BrowserPdf(PaperDisplay paper)
	{
		model.Status.Busy("正在下载……");
		string file = await storage.DownloadPaperPdfFileAsync(paper.图纸);
		model.Status.Reset();
		storage.RunFile(file);
	}

	/// <summary>
	/// 浏览ZIP图纸命令
	/// 获取ZIP图纸文件的下载链接并使用系统浏览器直接打开
	/// 适用于大文件或批量下载图纸的情况
	/// </summary>
	/// <param name="paper">要浏览的图纸对象</param>
	[RelayCommand]
	private void BrowserZip(PaperDisplay paper)
	{
		string fileDownloadUrl = storage.GetFileDownloadUrl(storage.GetPaperZipFileRelativePath(paper.图纸));
		Process.Start("explorer.exe", fileDownloadUrl);
	}

	/// <summary>
	/// 上传依据文件命令
	/// 允许用户为指定图纸上传相关的依据文件（如设计规范、技术标准等）
	/// 如果已存在依据文件则先删除旧文件，然后上传新文件
	/// 自动更新数据库中的文件名记录
	/// </summary>
	/// <param name="paper">要上传依据文件的图纸对象</param>
	/// <returns>异步任务，表示上传操作的完成</returns>
	[RelayCommand]
	private async Task UploadDep(PaperDisplay paper)
	{
		string file = picker.OpenFile();
		if (file != null)
		{
			model.Status.Busy("正在上传……");
			if (!string.IsNullOrEmpty(paper.图纸.依据文件名))
			{
				await storage.WebDeleteFileAsync(storage.GetPaperDepFileRelativePath(paper.图纸));
			}
			paper.图纸.依据文件名 = Path.GetFileName(file);
			await context.Db.Updateable(paper.图纸).ExecuteCommandAsync();
			string paperDepFileRelativePath = storage.GetPaperDepFileRelativePath(paper.图纸);
			File.Copy(file, storage.GetWebFileLocalAbsolutePath(paperDepFileRelativePath));
			await storage.UploadPaperDepFileAsync(paper.图纸);
			model.Status.Reset();
		}
	}

	/// <summary>
	/// 查看依据文件命令
	/// 从服务器下载指定图纸的依据文件并使用默认应用程序打开
	/// 需要先检查是否已上传依据文件，未上传时抛出异常
	/// </summary>
	/// <param name="paper">要查看依据文件的图纸对象</param>
	/// <returns>异步任务，表示下载和打开操作的完成</returns>
	/// <exception cref="T:System.Exception">当尚未上传依据文件时抛出异常</exception>
	[RelayCommand]
	private async Task ViewDep(PaperDisplay paper)
	{
		if (string.IsNullOrEmpty(paper.图纸.依据文件名))
		{
			throw new Exception("还未上传依据文件");
		}
		model.Status.Busy("正在下载……");
		string file = await storage.DownloadPaperDepFileAsync(paper.图纸);
		model.Status.Reset();
		storage.RunFile(file);
	}

	/// <summary>
	/// 编辑设备表命令
	/// 导航到设备编辑页面，用于管理图纸中涉及的设备信息
	/// </summary>
	[RelayCommand]
	private void EditDeviceTable()
	{
		navigation.NavigateWithHierarchy(typeof(EditDevicePage));
	}

	/// <summary>
	/// 上传图纸命令
	/// 导航到图纸上传页面，用于批量上传新的图纸文件
	/// </summary>
	[RelayCommand]
	private void UploadPaper()
	{
		navigation.NavigateWithHierarchy(typeof(UploadPaperPage));
	}

	[RelayCommand]
	private async Task RefreshAll()
	{
		model.Status.Busy("正在刷新……");
		Projects = null;
		ObservableCollection<config_project> observableCollection = new ObservableCollection<config_project>();
		foreach (config_project item in await context.Db.Queryable<config_project>().ToListAsync())
		{
			observableCollection.Add(item);
		}
		Projects = observableCollection;
		model.Status.Reset();
	}

	[RelayCommand]
	private void Search()
	{
		DisplayData = null;
		if (AllData == null)
		{
			return;
		}
		ObservableCollection<PaperDisplay> observableCollection = new ObservableCollection<PaperDisplay>();
		foreach (PaperDisplay item in from x in (from x in AllData
				where x.图纸.发布日期.Date >= DateFrom.Date
				where x.图纸.发布日期.Date <= DateTo.Date
				select x).WhereIf((PaperDisplay x) => x.盘箱柜.类别 == DeviceType, DeviceType != (盘箱柜类别)0)
			where x.盘箱柜.LOT.Contains(Lot)
			where x.盘箱柜.Batch.Contains(Batch)
			where x.盘箱柜.房间号.Contains(Room)
			where x.图纸.版本.Contains(Version)
			select x)
		{
			observableCollection.Add(item);
		}
		DisplayData = observableCollection;
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.PaperViewModel.Projects" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.PaperViewModel.Projects" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnProjectsChanged(ObservableCollection<config_project>? value)
	{
		Project = null;
		_ = Projects;
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.PaperViewModel.Project" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.PaperViewModel.Project" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnProjectChanged(config_project? value)
	{
		AllData = null;
		DisplayData = null;
		if (Project == null)
		{
			return;
		}
		List<盘箱柜> 盘箱柜列表 = await (from x in context.Db.Queryable<盘箱柜>()
			where x.项目Id == Project.Id
			select x).ToListAsync();
		List<图纸> source = await (from x in context.Db.Queryable<图纸>()
			where 盘箱柜列表.Select((盘箱柜 盘箱柜) => 盘箱柜.Id).Contains(x.盘箱柜表Id)
			select x).ToListAsync();
		ObservableCollection<PaperDisplay> observableCollection = new ObservableCollection<PaperDisplay>();
		foreach (PaperDisplay item in source.Select((图纸 x) => new PaperDisplay
		{
			图纸 = x,
			盘箱柜 = 盘箱柜列表.Single((盘箱柜 y) => y.Id == x.盘箱柜表Id)
		}))
		{
			observableCollection.Add(item);
		}
		AllData = observableCollection;
	}
}
