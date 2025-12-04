using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.Cable;
using IODataPlatform.Views.SubPages.Common;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 电缆管理页面视图模型类
/// 负责电缆数据的全生命周期管理，包括数据导入、编辑、匹配、发布等核心功能
/// 支持多子项目的电缆数据对比和实时同步，提供丰富的配置管理功能
/// 实现INavigationAware接口以支持页面生命周期管理和数据刷新
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
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class CableViewModel : ObservableObject, INavigationAware
{
	/// <summary>页面初始化状态标记，防止重复初始化</summary>
	private bool isInit;

	/// <summary>当前页面的所有电缆数据集合，用于界面显示和编辑</summary>
	[ObservableProperty]
	private ObservableCollection<CableData>? allData;

	[ObservableProperty]
	private ObservableCollection<CableData>? displayData;

	[ObservableProperty]
	private int allDataCount;

	[ObservableProperty]
	private int displayDataCount;

	private bool isRefreshingOptions;

	[ObservableProperty]
	private ObservableCollection<config_project>? projects;

	[ObservableProperty]
	private ObservableCollection<config_project_major>? majors;

	[ObservableProperty]
	private ObservableCollection<config_project_subProject>? subProjects1;

	[ObservableProperty]
	private ObservableCollection<config_project_subProject>? subProjects2;

	[ObservableProperty]
	private ObservableCollection<publish_termination>? terminations1;

	[ObservableProperty]
	private ObservableCollection<publish_termination>? terminations2;

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

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.CableViewModel.CalcCableCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? calcCableCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.CableViewModel.RefreshCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.CableViewModel.GoToMatchPageCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? goToMatchPageCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.CableViewModel.EditConfigurationTableCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<string>? editConfigurationTableCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.CableViewModel.ClearAllFilterOptionsCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? clearAllFilterOptionsCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.CableViewModel.FilterCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? filterCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.CableViewModel.DownLoadTemplateCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? downLoadTemplateCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.CableViewModel.ImportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<string>? importCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.CableViewModel.ExportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<string>? exportCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.CableViewModel.RefreshAllCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshAllCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.CableViewModel.PublishCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? publishCommand;

	public ImmutableList<CommonFilter> Filters { get; }

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.allData" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<CableData>? AllData
	{
		get
		{
			return allData;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<CableData>>.Default.Equals(allData, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.AllData);
				allData = value;
				OnAllDataChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.AllData);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.displayData" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<CableData>? DisplayData
	{
		get
		{
			return displayData;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<CableData>>.Default.Equals(displayData, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DisplayData);
				displayData = value;
				OnDisplayDataChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DisplayData);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.allDataCount" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int AllDataCount
	{
		get
		{
			return allDataCount;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(allDataCount, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.AllDataCount);
				allDataCount = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.AllDataCount);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.displayDataCount" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int DisplayDataCount
	{
		get
		{
			return displayDataCount;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(displayDataCount, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DisplayDataCount);
				displayDataCount = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DisplayDataCount);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.projects" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.majors" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.subProjects1" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.subProjects2" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.terminations1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<publish_termination>? Terminations1
	{
		get
		{
			return terminations1;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<publish_termination>>.Default.Equals(terminations1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Terminations1);
				terminations1 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Terminations1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.terminations2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<publish_termination>? Terminations2
	{
		get
		{
			return terminations2;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<publish_termination>>.Default.Equals(terminations2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Terminations2);
				terminations2 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Terminations2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.project" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.major1" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.major2" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.subProject1" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.CableViewModel.subProject2" />
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

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.CableViewModel.CalcCable" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand CalcCableCommand => calcCableCommand ?? (calcCableCommand = new RelayCommand(CalcCable));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.CableViewModel.Refresh" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new AsyncRelayCommand(Refresh));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.CableViewModel.GoToMatchPage" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand GoToMatchPageCommand => goToMatchPageCommand ?? (goToMatchPageCommand = new AsyncRelayCommand(GoToMatchPage));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.CableViewModel.EditConfigurationTable(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<string> EditConfigurationTableCommand => editConfigurationTableCommand ?? (editConfigurationTableCommand = new RelayCommand<string>(EditConfigurationTable));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.CableViewModel.ClearAllFilterOptions" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ClearAllFilterOptionsCommand => clearAllFilterOptionsCommand ?? (clearAllFilterOptionsCommand = new AsyncRelayCommand(ClearAllFilterOptions));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.CableViewModel.Filter" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand FilterCommand => filterCommand ?? (filterCommand = new RelayCommand(Filter));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.CableViewModel.DownLoadTemplate" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand DownLoadTemplateCommand => downLoadTemplateCommand ?? (downLoadTemplateCommand = new RelayCommand(DownLoadTemplate));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.CableViewModel.Import(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<string> ImportCommand => importCommand ?? (importCommand = new RelayCommand<string>(Import));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.CableViewModel.Export(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<string> ExportCommand => exportCommand ?? (exportCommand = new AsyncRelayCommand<string>(Export));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.CableViewModel.RefreshAll" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshAllCommand => refreshAllCommand ?? (refreshAllCommand = new AsyncRelayCommand(RefreshAll));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.CableViewModel.Publish" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand PublishCommand => publishCommand ?? (publishCommand = new RelayCommand(Publish));

	[RelayCommand]
	private async void CalcCable()
	{
		List<CableData> excelData = AllData.ToList();
		ObservableCollection<CableData> observableCollection = new ObservableCollection<CableData>();
		foreach (CableData item in await DataConverter.NumberMatchCable1(excelData, _003Ccontext_003EP))
		{
			observableCollection.Add(item);
		}
		AllData = observableCollection;
	}

	/// <summary>
	/// 电缆管理页面视图模型类
	/// 负责电缆数据的全生命周期管理，包括数据导入、编辑、匹配、发布等核心功能
	/// 支持多子项目的电缆数据对比和实时同步，提供丰富的配置管理功能
	/// 实现INavigationAware接口以支持页面生命周期管理和数据刷新
	/// </summary>
	public CableViewModel(SqlSugarContext context, INavigationService navigation, ConfigTableViewModel configvm, GlobalModel model, IMessageService message, IContentDialogService dialog, StorageService storage, ExcelService excel, IPickerService picker)
	{
		_003Ccontext_003EP = context;
		_003Cnavigation_003EP = navigation;
		_003Cconfigvm_003EP = configvm;
		_003Cmodel_003EP = model;
		_003Cmessage_003EP = message;
		_003Cstorage_003EP = storage;
		_003Cexcel_003EP = excel;
		_003Cpicker_003EP = picker;
		Filters = ImmutableList.Create(default(ReadOnlySpan<CommonFilter>));
		base._002Ector();
	}

	/// <summary>
	/// 页面导航离开时触发
	/// 当前实现为空，预留用于后续数据保存或清理操作
	/// </summary>
	public void OnNavigatedFrom()
	{
	}

	/// <summary>
	/// 页面导航到此页面时触发
	/// 首次访问时执行初始化操作，加载项目列表
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
	/// 保存并上传实时文件，支持可选的版本发布
	/// 将当前AllData导出为Excel文件并上传到服务器
	/// 如果提供versionId参数，同时创建发布版本的副本
	/// </summary>
	/// <param name="versionId">可选的发布版本标识符，如果提供则同时创建发布版本</param>
	/// <returns>异步任务，表示保存和上传操作的完成</returns>
	/// <exception cref="T:System.Exception">当必要的项目或子项目信息缺失时抛出异常</exception>
	public async Task SaveAndUploadRealtimeFileAsync(int? versionId = null)
	{
		_ = (Project ?? throw new Exception("开发人员注意")).Id;
		int subProjectId1 = (SubProject1 ?? throw new Exception("开发人员注意")).Id;
		int subProjectId2 = (SubProject2 ?? throw new Exception("开发人员注意")).Id;
		ObservableCollection<CableData> data = AllData ?? throw new Exception("开发人员注意");
		string relativePath = _003Cstorage_003EP.GetRealtimeCableFileRelativePath(subProjectId1, subProjectId2);
		string absolutePath = _003Cstorage_003EP.GetWebFileLocalAbsolutePath(relativePath);
		using DataTable dataTable = await data.ToTableByDisplayAttributeAsync();
		await _003Cexcel_003EP.FastExportAsync(dataTable, absolutePath);
		await _003Cstorage_003EP.UploadRealtimeCableFileAsync(subProjectId1, subProjectId2);
		if (versionId.HasValue)
		{
			string publishCableFileRelativePath = _003Cstorage_003EP.GetPublishCableFileRelativePath(subProjectId1, subProjectId2, versionId.Value);
			string webFileLocalAbsolutePath = _003Cstorage_003EP.GetWebFileLocalAbsolutePath(publishCableFileRelativePath);
			File.Copy(absolutePath, webFileLocalAbsolutePath, overwrite: true);
			await _003Cstorage_003EP.WebCopyFilesAsync(new _003C_003Ez__ReadOnlySingleElementList<(string, string)>((relativePath, publishCableFileRelativePath)));
		}
	}

	/// <summary>
	/// 从服务器下载并加载实时电缆数据
	/// 清除当前数据，从服务器下载最新的实时数据文件并解析为对象集合
	/// 包含完整的错误处理，当文件不存在或解析失败时返回空集合
	/// </summary>
	/// <returns>异步任务，表示数据加载操作的完成</returns>
	/// <exception cref="T:System.Exception">当必要的项目信息缺失时抛出异常</exception>
	public async Task ReloadAllData()
	{
		AllData = null;
		_ = (Project ?? throw new Exception("开发人员注意")).Id;
		int? num = SubProject1?.Id;
		int? num2 = SubProject2?.Id;
		if (!num.HasValue || !num2.HasValue)
		{
			return;
		}
		try
		{
			string fileName = await _003Cstorage_003EP.DownloadRealtimeCableFileAsync(num.Value, num2.Value);
			using DataTable dataTable = await _003Cexcel_003EP.GetDataTableAsStringAsync(fileName, hasHeader: true);
			IEnumerable<CableData> enumerable = await Task.Run((Func<IEnumerable<CableData>>)dataTable.StringTableToIEnumerableByDiplay<CableData>);
			ObservableCollection<CableData> observableCollection = new ObservableCollection<CableData>();
			foreach (CableData item in enumerable)
			{
				observableCollection.Add(item);
			}
			AllData = observableCollection;
		}
		catch
		{
			AllData = new ObservableCollection<CableData>();
		}
	}

	/// <summary>
	/// 刷新数据命令
	/// 手动触发数据重新加载，包含状态提示和错误处理
	/// 用户可通过此命令获取最新的服务器数据
	/// </summary>
	/// <returns>异步任务，表示刷新操作的完成</returns>
	[RelayCommand]
	private async Task Refresh()
	{
		_003Cmodel_003EP.Status.Busy("正在刷新……");
		await ReloadAllData();
		_003Cmodel_003EP.Status.Reset();
	}

	/// <summary>
	/// 跳转到电缆匹配页面命令
	/// 导航到专用的电缆匹配功能页面，提供高级的数据匹配和对比功能
	/// </summary>
	/// <returns>异步任务，表示导航操作的完成</returns>
	[RelayCommand]
	private async Task GoToMatchPage()
	{
		await Task.Delay(1);
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(MatchPage));
	}

	/// <summary>
	/// 编辑配置表命令
	/// 根据传入参数决定要编辑的配置表类型，并导航到相应的配置编辑页面
	/// 支持电缆相关的多种配置表，包括类别、规格、编号等
	/// </summary>
	/// <param name="param">配置表类型名称，用于决定编辑哪个配置表</param>
	/// <exception cref="T:System.NotImplementedException">当参数不匹配任何已知配置表类型时抛出</exception>
	[RelayCommand]
	private void EditConfigurationTable(string param)
	{
		_003Cconfigvm_003EP.Title = param;
		ConfigTableViewModel configTableViewModel = _003Cconfigvm_003EP;
		configTableViewModel.DataType = param switch
		{
			"配置线缆列别及色标" => typeof(config_cable_categoryAndColor), 
			"配置电缆特性代码" => typeof(config_cable_spec), 
			"配置电缆的起始流水号" => typeof(config_cable_startNumber), 
			"配置电缆的系统号" => typeof(config_cable_systemNumber), 
			"配置电缆IO类型" => typeof(config_cable_function), 
			_ => throw new NotImplementedException(), 
		};
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(ConfigTablePage));
	}

	[RelayCommand]
	private async Task ClearAllFilterOptions()
	{
		if (await _003Cmessage_003EP.ConfirmAsync("确认重置全部筛选条件"))
		{
			isRefreshingOptions = true;
			Filters.AllDo(delegate(CommonFilter x)
			{
				x.Option = "全部";
			});
			isRefreshingOptions = false;
			Filter();
		}
	}

	private void RefreshFilterOptions()
	{
		isRefreshingOptions = true;
		if (AllData == null)
		{
			Filters.ForEach(delegate(CommonFilter x)
			{
				x.ClearAll();
			});
		}
		else
		{
			Dictionary<string, CommonFilter> dictionary = Filters.ToDictionary((CommonFilter x) => x.Title);
		}
		isRefreshingOptions = false;
	}

	[RelayCommand]
	private void Filter()
	{
		if (!isRefreshingOptions && AllData != null)
		{
			Dictionary<string, CommonFilter> dictionary = Filters.ToDictionary((CommonFilter x) => x.Title);
			ObservableCollection<CableData> collection = AllData;
			DisplayData = new ObservableCollection<CableData>(collection);
		}
	}

	[RelayCommand]
	private async void DownLoadTemplate()
	{
		DataTable dataTable = new DataTable();
		string[] array = new string[26]
		{
			"起点房间号", "起点系统号", "起点盘柜名称", "起点设备名称", "起点接线点1", "起点接线点2", "起点接线点3", "起点接线点4", "起点屏蔽端", "起点IO类型",
			"起点安全分级分组", "起点信号位号", "起点专业", "终点房间号", "终点系统号", "终点盘柜名称", "终点设备名称", "终点接线点1", "终点接线点2", "终点接线点3",
			"终点接线点4", "终点屏蔽端", "终点IO类型", "终点安全分级分组", "终点信号位号", "终点专业"
		};
		string[] array2 = array;
		foreach (string columnName in array2)
		{
			dataTable.Columns.Add(columnName);
		}
		dataTable.Rows.Add("房间号", "示例：IPP、IPC", "示例：3IPP001AR", "示例：205BN", "示例：1", "示例：2", "示例：3", "示例：4", "", "示例：DI", "示例：NC", "点名带扩展码", "示例：NC到DAS 即NC");
		string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "待导入的电缆清单模板.xlsx");
		await _003Cexcel_003EP.FastExportToDesktopAsync(dataTable, filePath);
		_003Cmodel_003EP.Status.Success("成功下载到" + filePath);
	}

	[RelayCommand]
	private void Import(string param)
	{
		if (param == "导入发布端接数据")
		{
			ImportIoData();
			return;
		}
		if (param == "导入数据")
		{
			ImportData();
			return;
		}
		throw new Exception("开发人员注意");
	}

	private void ImportIoData()
	{
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(MatchPage));
	}

	private async void ImportData()
	{
		string text = _003Cpicker_003EP.OpenFile("Excel 文件(*.xls; *.xlsx)|*.xls; *.xlsx");
		if (text == null)
		{
			return;
		}
		_003Cmodel_003EP.Status.Busy("正在导入···");
		using DataTable dataTable = await _003Cexcel_003EP.GetDataTableAsStringAsync(text, hasHeader: true);
		IEnumerable<CableData> enumerable = await Task.Run((Func<IEnumerable<CableData>>)dataTable.StringTableToIEnumerableByDiplay<CableData>);
		ObservableCollection<CableData> observableCollection = new ObservableCollection<CableData>();
		foreach (CableData item in enumerable)
		{
			observableCollection.Add(item);
		}
		AllData = observableCollection;
		await SaveAndUploadRealtimeFileAsync();
		_003Cmodel_003EP.Status.Reset();
	}

	[RelayCommand]
	private async Task Export(string param)
	{
		if (AllData == null)
		{
			throw new Exception("没有可导出的数据");
		}
		string file = _003Cpicker_003EP.SaveFile("Excel 文件(*.xlsx)|*.xlsx", param);
		if (file != null)
		{
			if (File.Exists(file))
			{
				File.Delete(file);
			}
			_003Cmodel_003EP.Status.Busy("正在" + param + "……");
			switch (param)
			{
			case "导出全部数据":
				await ExportAllData(file);
				break;
			case "导出筛选数据":
				await ExportFilterData(file);
				break;
			case "导出可发布数据":
				await ExportPublishData(file);
				break;
			default:
				throw new Exception("开发人员注意");
			}
			_003Cmodel_003EP.Status.Success("已成功" + param + "：" + file);
		}
	}

	private async Task ExportAllData(string file)
	{
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		ExcelService excelService = _003Cexcel_003EP;
		await excelService.FastExportAsync(await AllData.ToTableByDisplayAttributeAsync(), file);
	}

	private async Task ExportFilterData(string file)
	{
		if (DisplayData == null)
		{
			throw new Exception("开发人员注意");
		}
		ExcelService excelService = _003Cexcel_003EP;
		await excelService.FastExportAsync(await DisplayData.ToTableByDisplayAttributeAsync(), file);
	}

	private async Task ExportPublishData(string file)
	{
		if (AllData == null)
		{
			throw new Exception("无数据可导出");
		}
		_003Cmodel_003EP.Status.Busy("正在下载电缆模板……");
		string cableLacalPath = await _003Cstorage_003EP.DownloadtemplatesDepFileAsync("电缆清单模板.xlsx");
		string[] columns = new string[29]
		{
			"序号", "线缆编号", "线缆列别", "色标", "特性代码", "对数", "芯数", "起点房间号", "起点盘柜名称", "起点设备名称",
			"起点接线点1", "起点接线点2", "起点接线点3", "起点接线点4", "起点屏蔽端", "起点信号位号", "电缆长度", "终点房间号", "终点盘柜名称", "终点设备名称",
			"终点接线点1", "终点接线点2", "终点接线点3", "终点接线点4", "终点屏蔽端", "终点信号位号", "供货方", "版本", "备注"
		};
		DataTable dataTable = _003Cexcel_003EP.CreateDataTable(columns);
		foreach (CableData allDatum in AllData)
		{
			dataTable.Rows.Add(allDatum.序号, allDatum.线缆编号, allDatum.线缆列别, allDatum.色标, allDatum.特性代码, allDatum.芯线对数号, allDatum.芯线号, allDatum.起点房间号, allDatum.起点盘柜名称, allDatum.起点设备名称, allDatum.起点接线点1, allDatum.起点接线点2, allDatum.起点接线点3, allDatum.起点接线点4, allDatum.起点屏蔽端, allDatum.起点信号位号, allDatum.电缆长度, allDatum.终点房间号, allDatum.终点盘柜名称, allDatum.终点设备名称, allDatum.终点接线点1, allDatum.终点接线点2, allDatum.终点接线点3, allDatum.终点接线点4, allDatum.终点屏蔽端, allDatum.终点信号位号, allDatum.供货方, allDatum.版本, allDatum.备注);
		}
		await _003Cexcel_003EP.FastExportSheetAsync(dataTable, cableLacalPath, 4);
		File.Move(cableLacalPath, file);
		_003Cmodel_003EP.Status.Success("已成功导出到" + file);
	}

	private async Task RefreshProjects()
	{
		Projects = null;
		Projects = new ObservableCollection<config_project>(await _003Ccontext_003EP.Db.Queryable<config_project>().ToListAsync());
	}

	[RelayCommand]
	private async Task RefreshAll()
	{
		if (await _003Cmessage_003EP.ConfirmAsync("是否全部刷新"))
		{
			await RefreshProjects();
		}
	}

	[RelayCommand]
	private void Publish()
	{
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(PublishPage));
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.CableViewModel.AllData" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.CableViewModel.AllData" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnAllDataChanged(ObservableCollection<CableData>? value)
	{
		DisplayData = null;
		AllDataCount = AllData?.Count ?? 0;
		if (AllData != null)
		{
			RefreshFilterOptions();
			Filter();
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.CableViewModel.DisplayData" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.CableViewModel.DisplayData" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnDisplayDataChanged(ObservableCollection<CableData>? value)
	{
		DisplayDataCount = DisplayData?.Count ?? 0;
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.CableViewModel.Project" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.CableViewModel.Project" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnProjectChanged(config_project? value)
	{
		Major1 = null;
		Major2 = null;
		if (value != null)
		{
			Majors = new ObservableCollection<config_project_major>(await (from x in _003Ccontext_003EP.Db.Queryable<config_project_major>()
				where x.ProjectId == value.Id
				select x).ToListAsync());
			if (Majors.Count == 1)
			{
				Major1 = Majors[0];
				Major2 = Majors[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.CableViewModel.Major1" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.CableViewModel.Major1" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnMajor1Changed(config_project_major? value)
	{
		SubProjects1 = null;
		if (value != null)
		{
			SubProjects1 = new ObservableCollection<config_project_subProject>(await (from x in _003Ccontext_003EP.Db.Queryable<config_project_subProject>()
				where x.MajorId == value.Id
				select x).ToListAsync());
			if (SubProjects1.Count == 1)
			{
				SubProject1 = SubProjects1[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.CableViewModel.Major2" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.CableViewModel.Major2" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnMajor2Changed(config_project_major? value)
	{
		SubProjects2 = null;
		if (value != null)
		{
			SubProjects2 = new ObservableCollection<config_project_subProject>(await (from x in _003Ccontext_003EP.Db.Queryable<config_project_subProject>()
				where x.MajorId == value.Id
				select x).ToListAsync());
			if (SubProjects2.Count == 1)
			{
				SubProject2 = SubProjects2[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.CableViewModel.SubProject1" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.CableViewModel.SubProject1" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnSubProject1Changed(config_project_subProject? value)
	{
		AllData = null;
		_003Cmodel_003EP.Status.Busy("正在获取数据……");
		await ReloadAllData();
		_003Cmodel_003EP.Status.Reset();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.CableViewModel.SubProject2" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.CableViewModel.SubProject2" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnSubProject2Changed(config_project_subProject? value)
	{
		AllData = null;
		if (value != null)
		{
			_003Cmodel_003EP.Status.Busy("正在获取数据……");
			await ReloadAllData();
			_003Cmodel_003EP.Status.Reset();
		}
	}
}
