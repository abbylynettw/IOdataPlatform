using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.Common;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 数据资产中心页面视图模型类
/// 负责已发布数据的集中展示和管理，支持IO、端接、电缆三种数据类型
/// 提供已发布版本的数据浏览、下载和配置管理功能
/// 实现不同数据类型的统一显示接口，支持动态类型转换
/// </summary>
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class DataAssetCenterViewModel(SqlSugarContext context, GlobalModel model, IMessageService message, StorageService storage, ExcelService excel, IPickerService picker, ConfigTableViewModel configvm, INavigationService navigation) : ObservableObject(), INavigationAware
{
	/// <summary>页面初始化状态标记，防止重复初始化操作</summary>
	private bool isInit;

	/// <summary>当前页面显示的所有数据集合，支持多种数据类型的统一存储</summary>
	[ObservableProperty]
	private ObservableCollection<object>? allData;

	/// <summary>当前数据的下载链接URL，用于用户直接下载原始文件</summary>
	[ObservableProperty]
	private string? downloadUrl;

	[ObservableProperty]
	private ObservableCollection<config_project>? projects;

	[ObservableProperty]
	private ObservableCollection<config_project_major>? majors;

	[ObservableProperty]
	private ObservableCollection<config_project_subProject>? subProjects1;

	[ObservableProperty]
	private ObservableCollection<config_project_subProject>? subProjects2;

	[ObservableProperty]
	private ObservableCollection<string>? publishVersions;

	[ObservableProperty]
	private config_project? project;

	[ObservableProperty]
	private config_project_major? major1;

	[ObservableProperty]
	private config_project_major? major2;

	[ObservableProperty]
	private config_project_subProject? subProject1;

	[ObservableProperty]
	private config_project_subProject? subProject2;

	[ObservableProperty]
	private string? publishVersion;

	[ObservableProperty]
	private string category = "IO";

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.EditConfigurationTableCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<string>? editConfigurationTableCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.RefreshCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.ExportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? exportCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.CopyToClipboardCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<string>? copyToClipboardCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.RefreshAllCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshAllCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.allData" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<object>? AllData
	{
		get
		{
			return allData;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<object>>.Default.Equals(allData, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.AllData);
				allData = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.AllData);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.downloadUrl" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string? DownloadUrl
	{
		get
		{
			return downloadUrl;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(downloadUrl, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DownloadUrl);
				downloadUrl = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DownloadUrl);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.projects" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<config_project>? Projects
	{
		get
		{
			return projects;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<config_project>>.Default.Equals(projects, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Projects);
				projects = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Projects);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.majors" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<config_project_major>? Majors
	{
		get
		{
			return majors;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<config_project_major>>.Default.Equals(majors, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Majors);
				majors = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Majors);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.subProjects1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<config_project_subProject>? SubProjects1
	{
		get
		{
			return subProjects1;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<config_project_subProject>>.Default.Equals(subProjects1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SubProjects1);
				subProjects1 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SubProjects1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.subProjects2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<config_project_subProject>? SubProjects2
	{
		get
		{
			return subProjects2;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<config_project_subProject>>.Default.Equals(subProjects2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SubProjects2);
				subProjects2 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SubProjects2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.publishVersions" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<string>? PublishVersions
	{
		get
		{
			return publishVersions;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<string>>.Default.Equals(publishVersions, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PublishVersions);
				publishVersions = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PublishVersions);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.project" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public config_project? Project
	{
		get
		{
			return project;
		}
		set
		{
			if (!EqualityComparer<config_project>.Default.Equals(project, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Project);
				project = value;
				OnProjectChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Project);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.major1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public config_project_major? Major1
	{
		get
		{
			return major1;
		}
		set
		{
			if (!EqualityComparer<config_project_major>.Default.Equals(major1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Major1);
				major1 = value;
				OnMajor1Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Major1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.major2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public config_project_major? Major2
	{
		get
		{
			return major2;
		}
		set
		{
			if (!EqualityComparer<config_project_major>.Default.Equals(major2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Major2);
				major2 = value;
				OnMajor2Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Major2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.subProject1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public config_project_subProject? SubProject1
	{
		get
		{
			return subProject1;
		}
		set
		{
			if (!EqualityComparer<config_project_subProject>.Default.Equals(subProject1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SubProject1);
				subProject1 = value;
				OnSubProject1Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SubProject1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.subProject2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public config_project_subProject? SubProject2
	{
		get
		{
			return subProject2;
		}
		set
		{
			if (!EqualityComparer<config_project_subProject>.Default.Equals(subProject2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SubProject2);
				subProject2 = value;
				OnSubProject2Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SubProject2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.publishVersion" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string? PublishVersion
	{
		get
		{
			return publishVersion;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(publishVersion, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PublishVersion);
				publishVersion = value;
				OnPublishVersionChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PublishVersion);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DataAssetCenterViewModel.category" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Category
	{
		get
		{
			return category;
		}
		[MemberNotNull("category")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(category, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Category);
				category = value;
				OnCategoryChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Category);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DataAssetCenterViewModel.EditConfigurationTable(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<string> EditConfigurationTableCommand => editConfigurationTableCommand ?? (editConfigurationTableCommand = new RelayCommand<string>(EditConfigurationTable));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DataAssetCenterViewModel.Refresh" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new AsyncRelayCommand(Refresh));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DataAssetCenterViewModel.Export" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ExportCommand => exportCommand ?? (exportCommand = new AsyncRelayCommand(Export));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DataAssetCenterViewModel.CopyToClipboard(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<string> CopyToClipboardCommand => copyToClipboardCommand ?? (copyToClipboardCommand = new AsyncRelayCommand<string>(CopyToClipboard));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DataAssetCenterViewModel.RefreshAll" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshAllCommand => refreshAllCommand ?? (refreshAllCommand = new AsyncRelayCommand(RefreshAll));

	/// <summary>
	/// 页面导航离开时触发
	/// 当前实现为空，预留用于后续数据清理操作
	/// </summary>
	public void OnNavigatedFrom()
	{
	}

	/// <summary>
	/// 页面导航到此页面时触发
	/// 首次访问时执行项目刷新和初始化操作
	/// 非首次访问时尝试重新加载数据，确保数据的时效性
	/// </summary>
	public async void OnNavigatedTo()
	{
		if (!isInit)
		{
			await RefreshProjects();
			isInit = true;
			return;
		}
		try
		{
			await ReloadAllData();
		}
		catch
		{
		}
	}

	/// <summary>
	/// 从服务器下载并加载已发布的数据文件
	/// 根据选中的数据类型（IO、端接、电缆）动态获取对应的发布记录
	/// 支持不同数据类型的自动转换和统一显示，同时生成下载链接
	/// </summary>
	/// <returns>异步任务，表示数据加载操作的完成</returns>
	/// <exception cref="T:System.Exception">当必要的项目参数缺失时抛出异常</exception>
	/// <exception cref="T:System.NotSupportedException">当遇到不支持的数据类型时抛出异常</exception>
	private async Task ReloadAllData()
	{
		AllData = null;
		int subProjectId = (SubProject1 ?? throw new Exception("开发人员注意")).Id;
		if (PublishVersion == null)
		{
			throw new Exception("开发人员注意");
		}
		string fileName = "";
		switch (Category)
		{
		case "IO":
		{
			publish_io publishIo = (from x in context.Db.Queryable<publish_io>()
				where x.SubProjectId == SubProject1.Id && x.PublishedVersion == PublishVersion
				select x).ToList().FirstOrDefault();
			if (publishIo == null)
			{
				return;
			}
			fileName = await storage.DownloadPublishIoFileAsync(subProjectId, publishIo.Id);
			DownloadUrl = storage.GetFileDownloadUrl(storage.GetPublishIoFileRelativePath(subProjectId, publishIo.Id));
			break;
		}
		case "端接":
		{
			publish_termination publishTermination = (from x in context.Db.Queryable<publish_termination>()
				where x.SubProjectId == SubProject1.Id && x.PublishedVersion == PublishVersion
				select x).ToList().FirstOrDefault();
			if (publishTermination == null)
			{
				return;
			}
			fileName = await storage.DownloadPublishTerminationFileAsync(subProjectId, publishTermination.Id);
			DownloadUrl = storage.GetFileDownloadUrl(storage.GetPublishTerminationFileRelativePath(subProjectId, publishTermination.Id));
			break;
		}
		case "电缆":
		{
			if (SubProject2 == null)
			{
				return;
			}
			publish_cable publishCable = (from x in context.Db.Queryable<publish_cable>()
				where x.SubProjectId1 == SubProject1.Id && x.SubProjectId2 == SubProject2.Id && x.PublishedVersion == PublishVersion
				select x).ToList().FirstOrDefault();
			if (publishCable == null)
			{
				return;
			}
			fileName = await storage.DownloadPublishCableFileAsync(subProjectId, (SubProject2 ?? throw new Exception("电缆需要选择两个子项")).Id, publishCable.Id);
			DownloadUrl = storage.GetFileDownloadUrl(storage.GetPublishCableFileRelativePath(subProjectId, (SubProject2 ?? throw new Exception("电缆需要选择两个子项")).Id, publishCable.Id));
			break;
		}
		}
		using DataTable table = await excel.GetDataTableAsStringAsync(fileName, hasHeader: true);
		IEnumerable<object> enumerable = Category switch
		{
			"IO" => table.StringTableToIEnumerableByDiplay<IoData>().Cast<object>(), 
			"端接" => table.StringTableToIEnumerableByDiplay<TerminationData>().Cast<object>(), 
			"电缆" => table.StringTableToIEnumerableByDiplay<CableData>().Cast<object>(), 
			_ => throw new NotSupportedException(), 
		};
		ObservableCollection<object> observableCollection = new ObservableCollection<object>();
		foreach (object item in enumerable)
		{
			observableCollection.Add(item);
		}
		AllData = observableCollection;
	}

	/// <summary>
	/// 编辑配置表命令
	/// 根据参数设置配置表的标题和数据类型，并导航到配置表编辑页面
	/// 用于管理系统的各种配置数据，如控制系统IO数据映射配置等
	/// </summary>
	/// <param name="param">配置表的类型名称参数</param>
	/// <exception cref="T:System.NotImplementedException">当遇到未实现的配置类型时抛出异常</exception>
	[RelayCommand]
	private void EditConfigurationTable(string param)
	{
		configvm.Title = param;
		ConfigTableViewModel configTableViewModel = configvm;
		if (param == "控制系统IO数据配置")
		{
			Type typeFromHandle = typeof(config_controlSystem_mapping);
			configTableViewModel.DataType = typeFromHandle;
			navigation.NavigateWithHierarchy(typeof(ConfigTablePage));
			return;
		}
		throw new NotImplementedException();
	}

	/// <summary>
	/// 刷新数据命令
	/// 显示加载状态并重新加载当前选中版本的数据
	/// 完成后重置状态为默认状态
	/// </summary>
	/// <returns>异步任务，表示刷新操作的完成</returns>
	[RelayCommand]
	private async Task Refresh()
	{
		model.Status.Busy("正在刷新……");
		await ReloadAllData();
		model.Status.Reset();
	}

	[RelayCommand]
	private async Task Export()
	{
		if (AllData == null)
		{
			throw new Exception("没有可导出的数据");
		}
		string file = picker.SaveFile("Excel 文件(*.xlsx)|*.xlsx");
		if (file != null)
		{
			if (File.Exists(file))
			{
				File.Delete(file);
			}
			model.Status.Busy("正在导出数据……");
			ExcelService excelService = excel;
			await excelService.FastExportAsync(await AllData.ToTableByDisplayAttributeAsync(), file);
			model.Status.Success("已成功导出数据：" + file);
		}
	}

	[RelayCommand]
	private async Task CopyToClipboard(string param)
	{
		Clipboard.SetText(param);
		await message.SuccessAsync("已复制到剪贴板：" + param);
	}

	private async Task RefreshProjects()
	{
		Projects = null;
		Projects = new ObservableCollection<config_project>(await context.Db.Queryable<config_project>().ToListAsync());
	}

	[RelayCommand]
	private async Task RefreshAll()
	{
		if (await message.ConfirmAsync("是否全部刷新"))
		{
			Category = "IO";
			await RefreshProjects();
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.Project" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.Project" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnProjectChanged(config_project? value)
	{
		Major1 = null;
		Major2 = null;
		if (value != null)
		{
			Majors = new ObservableCollection<config_project_major>(await (from x in context.Db.Queryable<config_project_major>()
				where x.ProjectId == value.Id
				select x).ToListAsync());
			if (Majors.Count == 1)
			{
				Major1 = Majors[0];
				Major2 = Majors[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.Major1" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.Major1" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnMajor1Changed(config_project_major? value)
	{
		SubProjects1 = null;
		if (value != null)
		{
			SubProjects1 = new ObservableCollection<config_project_subProject>(await (from x in context.Db.Queryable<config_project_subProject>()
				where x.MajorId == value.Id
				select x).ToListAsync());
			if (SubProjects1.Count == 1)
			{
				SubProject1 = SubProjects1[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.Major2" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.Major2" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnMajor2Changed(config_project_major? value)
	{
		SubProjects2 = null;
		if (value != null)
		{
			SubProjects2 = new ObservableCollection<config_project_subProject>(await (from x in context.Db.Queryable<config_project_subProject>()
				where x.MajorId == value.Id
				select x).ToListAsync());
			if (SubProjects2.Count == 1)
			{
				SubProject2 = SubProjects2[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.SubProject1" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.SubProject1" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnSubProject1Changed(config_project_subProject? value)
	{
		if (Project == null || Major1 == null || value == null)
		{
			return;
		}
		switch (Category)
		{
		case "IO":
			PublishVersions = new ObservableCollection<string>(await (from s in context.Db.Queryable<publish_io>()
				where s.SubProjectId == value.Id
				select s.PublishedVersion).ToListAsync());
			break;
		case "端接":
			PublishVersions = new ObservableCollection<string>(await (from s in context.Db.Queryable<publish_termination>()
				where s.SubProjectId == value.Id
				select s.PublishedVersion).ToListAsync());
			break;
		case "电缆":
			if (SubProject2 != null && Major2 != null)
			{
				PublishVersions = new ObservableCollection<string>(await (from c in context.Db.Queryable<publish_cable>()
					where c.SubProjectId1 == value.Id && c.SubProjectId2 == SubProject2.Id
					select c.PublishedVersion).ToListAsync());
				if (PublishVersions.Count == 1)
				{
					PublishVersion = PublishVersions[0];
				}
			}
			break;
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.SubProject2" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.SubProject2" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnSubProject2Changed(config_project_subProject? value)
	{
		if (Project == null || Major2 == null || value == null)
		{
			return;
		}
		switch (Category)
		{
		case "IO":
			PublishVersions = new ObservableCollection<string>(await (from s in context.Db.Queryable<publish_io>()
				where s.SubProjectId == value.Id
				select s.PublishedVersion).ToListAsync());
			break;
		case "端接":
			PublishVersions = new ObservableCollection<string>(await (from s in context.Db.Queryable<publish_termination>()
				where s.SubProjectId == value.Id
				select s.PublishedVersion).ToListAsync());
			break;
		case "电缆":
			if (SubProject1 != null && Major1 != null)
			{
				PublishVersions = new ObservableCollection<string>(await (from c in context.Db.Queryable<publish_cable>()
					where c.SubProjectId1 == SubProject1.Id && c.SubProjectId2 == value.Id
					select c.PublishedVersion).ToListAsync());
				if (PublishVersions.Count == 1)
				{
					PublishVersion = PublishVersions[0];
				}
			}
			break;
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.PublishVersion" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.PublishVersion" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnPublishVersionChanged(string? value)
	{
		AllData = null;
		if (PublishVersion != null)
		{
			model.Status.Busy("正在获取数据……");
			await ReloadAllData();
			model.Status.Reset();
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.Category" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DataAssetCenterViewModel.Category" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnCategoryChanged(string value)
	{
		if (Project == null || Major1 == null || SubProject1 == null)
		{
			return;
		}
		switch (Category)
		{
		case "IO":
			PublishVersions = new ObservableCollection<string>(await (from s in context.Db.Queryable<publish_io>()
				where s.SubProjectId == SubProject1.Id
				select s.PublishedVersion).ToListAsync());
			break;
		case "端接":
			PublishVersions = new ObservableCollection<string>(await (from s in context.Db.Queryable<publish_termination>()
				where s.SubProjectId == SubProject1.Id
				select s.PublishedVersion).ToListAsync());
			break;
		case "电缆":
			if (SubProject2 == null || Major2 == null)
			{
				return;
			}
			PublishVersions = new ObservableCollection<string>(await (from c in context.Db.Queryable<publish_cable>()
				where c.SubProjectId1 == SubProject1.Id && c.SubProjectId2 == SubProject2.Id
				select c.PublishedVersion).ToListAsync());
			if (PublishVersions.Count == 1)
			{
				PublishVersion = PublishVersions[0];
			}
			break;
		}
		model.Status.Busy("正在获取数据……");
		await ReloadAllData();
		model.Status.Reset();
	}
}
