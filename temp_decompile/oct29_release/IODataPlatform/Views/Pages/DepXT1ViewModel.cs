using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
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
using IODataPlatform.Views.SubPages.XT1;
using IODataPlatform.Views.SubPages.XT2;
using LYSoft.Libs;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// XT1控制系统专业页面视图模型类
/// 负责XT1(龙鳞)控制系统IO数据的全生命周期管理，包括Excel数据导入、PDF提取、智能分配等
/// 支持复杂的IO机柜自动分配算法，针对XT1系统的特殊规格和要求进行优化
/// 提供数据导入、PDF文档解析和机柜分配预览等专业功能
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
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class DepXT1ViewModel(SqlSugarContext context, GlobalModel model, IMessageService message, IContentDialogService dialog, INavigationService navigation, StorageService storage, ExcelService excel, IPickerService picker, PublishViewModel publishvm, ExtractPdfViewModel epvm, NavigationParameterService parameterService) : ObservableObject, INavigationAware
{
	/// <summary>页面初始化状态标记，防止重复初始化操作</summary>
	private bool isInit;

	/// <summary>当前页面的所有IO数据集合，包含完整的IO信号信息和机柜分配结果</summary>
	[ObservableProperty]
	private ObservableCollection<IoFullData>? allData;

	/// <summary>冗余率百分比，用于机柜容量计算时的安全余量设置，默认为20%</summary>
	[ObservableProperty]
	private int redundancyRate = 20;

	[ObservableProperty]
	private ObservableCollection<IoFullData>? displayPoints;

	[ObservableProperty]
	private int displayPointCount;

	[ObservableProperty]
	private int allDataCount;

	[ObservableProperty]
	private bool isAscending = true;

	private bool isRefreshingOptions;

	[ObservableProperty]
	private ObservableCollection<config_project>? projects;

	[ObservableProperty]
	private ObservableCollection<config_project_major>? majors;

	[ObservableProperty]
	private ObservableCollection<config_project_subProject>? subProjects;

	[ObservableProperty]
	private config_project? project;

	[ObservableProperty]
	private config_project_major? major;

	[ObservableProperty]
	private config_project_subProject? subProject;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.ImportExcelDataCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? importExcelDataCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.ExtractPdfDataCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? extractPdfDataCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.AllocateIOCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? allocateIOCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.PreviewAllocateIOCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? previewAllocateIOCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.AddCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.ClearCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? clearCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.EditCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<IoFullData>? editCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.DeleteCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<IoFullData>? deleteCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.ImportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? importCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.RefreshCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.ExportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<string>? exportCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.ClearAllFilterOptionsCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? clearAllFilterOptionsCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.FilterAndSortCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? filterAndSortCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.AddSubProjectCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addSubProjectCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.DeleteSubProjectCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? deleteSubProjectCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.RefreshAllCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshAllCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.PublishCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? publishCommand;

	public ImmutableList<CommonFilter> Filters { get; }

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT1ViewModel.allData" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<IoFullData>? AllData
	{
		get
		{
			return allData;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<IoFullData>>.Default.Equals(allData, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.AllData);
				allData = value;
				OnAllDataChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.AllData);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT1ViewModel.redundancyRate" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int RedundancyRate
	{
		get
		{
			return redundancyRate;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(redundancyRate, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.RedundancyRate);
				redundancyRate = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.RedundancyRate);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT1ViewModel.displayPoints" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<IoFullData>? DisplayPoints
	{
		get
		{
			return displayPoints;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<IoFullData>>.Default.Equals(displayPoints, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DisplayPoints);
				displayPoints = value;
				OnDisplayPointsChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DisplayPoints);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT1ViewModel.displayPointCount" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int DisplayPointCount
	{
		get
		{
			return displayPointCount;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(displayPointCount, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DisplayPointCount);
				displayPointCount = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DisplayPointCount);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT1ViewModel.allDataCount" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT1ViewModel.isAscending" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsAscending
	{
		get
		{
			return isAscending;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(isAscending, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsAscending);
				isAscending = value;
				OnIsAscendingChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsAscending);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT1ViewModel.projects" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT1ViewModel.majors" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT1ViewModel.subProjects" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<config_project_subProject>? SubProjects
	{
		get
		{
			return subProjects;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<config_project_subProject>>.Default.Equals(subProjects, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SubProjects);
				subProjects = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SubProjects);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT1ViewModel.project" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT1ViewModel.major" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public config_project_major? Major
	{
		get
		{
			return major;
		}
		set
		{
			if (!EqualityComparer<config_project_major>.Default.Equals(major, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Major);
				major = value;
				OnMajorChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Major);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT1ViewModel.subProject" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public config_project_subProject? SubProject
	{
		get
		{
			return subProject;
		}
		set
		{
			if (!EqualityComparer<config_project_subProject>.Default.Equals(subProject, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SubProject);
				subProject = value;
				OnSubProjectChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SubProject);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.ImportExcelData" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ImportExcelDataCommand => importExcelDataCommand ?? (importExcelDataCommand = new RelayCommand(ImportExcelData));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.ExtractPdfData" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ExtractPdfDataCommand => extractPdfDataCommand ?? (extractPdfDataCommand = new RelayCommand(ExtractPdfData));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.AllocateIO" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AllocateIOCommand => allocateIOCommand ?? (allocateIOCommand = new AsyncRelayCommand(AllocateIO));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.PreviewAllocateIO" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand PreviewAllocateIOCommand => previewAllocateIOCommand ?? (previewAllocateIOCommand = new RelayCommand(PreviewAllocateIO));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.Add" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddCommand => addCommand ?? (addCommand = new AsyncRelayCommand(Add));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.Clear" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ClearCommand => clearCommand ?? (clearCommand = new AsyncRelayCommand(Clear));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.Edit(IODataPlatform.Models.ExcelModels.IoFullData)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<IoFullData> EditCommand => editCommand ?? (editCommand = new AsyncRelayCommand<IoFullData>(Edit));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.Delete(IODataPlatform.Models.ExcelModels.IoFullData)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<IoFullData> DeleteCommand => deleteCommand ?? (deleteCommand = new AsyncRelayCommand<IoFullData>(Delete));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.Import" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ImportCommand => importCommand ?? (importCommand = new AsyncRelayCommand(Import));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.Refresh" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new AsyncRelayCommand(Refresh));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.Export(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<string> ExportCommand => exportCommand ?? (exportCommand = new AsyncRelayCommand<string>(Export));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.ClearAllFilterOptions" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ClearAllFilterOptionsCommand => clearAllFilterOptionsCommand ?? (clearAllFilterOptionsCommand = new AsyncRelayCommand(ClearAllFilterOptions));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.FilterAndSort" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand FilterAndSortCommand => filterAndSortCommand ?? (filterAndSortCommand = new RelayCommand(FilterAndSort));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.AddSubProject" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddSubProjectCommand => addSubProjectCommand ?? (addSubProjectCommand = new AsyncRelayCommand(AddSubProject));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.DeleteSubProject" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand DeleteSubProjectCommand => deleteSubProjectCommand ?? (deleteSubProjectCommand = new AsyncRelayCommand(DeleteSubProject));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.RefreshAll" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshAllCommand => refreshAllCommand ?? (refreshAllCommand = new AsyncRelayCommand(RefreshAll));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT1ViewModel.Publish" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand PublishCommand => publishCommand ?? (publishCommand = new AsyncRelayCommand(Publish));

	/// <summary>
	/// 页面导航离开时触发
	/// 当前实现为空，预留用于后续数据保存或状态清理操作
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
	/// 从服务器下载并加载实时IO数据
	/// 清除当前数据，从服务器下载最新的实时数据文件并解析为IO对象集合
	/// 包含完整的错误处理，当文件不存在或解析失败时返回空集合
	/// </summary>
	/// <returns>异步任务，表示数据加载操作的完成</returns>
	/// <exception cref="T:System.Exception">当必要的项目参数缺失时抛出异常</exception>
	private async Task ReloadAllData()
	{
		AllData = null;
		_ = (Project ?? throw new Exception("开发人员注意")).Id;
		int id = (SubProject ?? throw new Exception("开发人员注意")).Id;
		try
		{
			string fileName = await storage.DownloadRealtimeIoFileAsync(id);
			DataTable dataTable = await excel.GetDataTableAsStringAsync(fileName, hasHeader: true);
			try
			{
				AllData = await Task.Run(() => new ObservableCollection<IoFullData>(dataTable.StringTableToIEnumerableByDiplay<IoFullData>()));
			}
			finally
			{
				if (dataTable != null)
				{
					((IDisposable)dataTable).Dispose();
				}
			}
		}
		catch
		{
			AllData = new ObservableCollection<IoFullData>();
		}
	}

	/// <summary>
	/// 导入Excel数据命令
	/// 导航到Excel数据上传页面，用于批量导入IO数据
	/// 支持标准的Excel模板格式，自动识别和解析列映射关系
	/// </summary>
	[RelayCommand]
	private void ImportExcelData()
	{
		navigation.NavigateWithHierarchy(typeof(UploadExcelDataPage));
	}

	/// <summary>
	/// 保存并上传实时IO文件，支持可选的版本发布
	/// 将当前AllData导出为Excel文件并上传到服务器
	/// 如果提供versionId参数，同时创建发布版本的副本用于正式发布
	/// </summary>
	/// <param name="versionId">可选的发布版本标识符，如果提供则同时创建发布版本</param>
	/// <returns>异步任务，表示保存和上传操作的完成</returns>
	/// <exception cref="T:System.Exception">当必要的项目参数缺失时抛出异常</exception>
	public async Task SaveAndUploadFileAsync(int? versionId = null)
	{
		_ = (Project ?? throw new Exception("开发人员注意")).Id;
		int subProjectId = (SubProject ?? throw new Exception("开发人员注意")).Id;
		ObservableCollection<IoFullData> data = AllData ?? throw new Exception("开发人员注意");
		string relativePath = storage.GetRealtimeIoFileRelativePath(subProjectId);
		string absolutePath = storage.GetWebFileLocalAbsolutePath(relativePath);
		using DataTable dataTable = await data.ToTableByDisplayAttributeAsync();
		await excel.FastExportAsync(dataTable, absolutePath);
		await storage.UploadRealtimeIoFileAsync(subProjectId);
		if (versionId.HasValue)
		{
			await storage.WebCopyFilesAsync(new _003C_003Ez__ReadOnlySingleElementList<(string, string)>((relativePath, storage.GetPublishIoFileRelativePath(subProjectId, versionId.Value))));
		}
	}

	/// <summary>
	/// 提取PDF数据命令
	/// 配置PDF数据提取器的字段映射关系，并导航到PDF提取页面
	/// 支持两种数据类型：IO字段和设置参数字段的自动识别和提取
	/// 用于从技术文档和设计图纸中自动化提取IO数据
	/// </summary>
	[RelayCommand]
	private void ExtractPdfData()
	{
		epvm.IoFields = new ObservableCollection<string>
		{
			"序号", "信号位号", "扩展码", "信号说明", "安全分级分组", "功能分级", "抗震类别", "IO类型", "信号特性", "供电方",
			"测量单位", "量程下限", "量程上限", "缺省值", "SOETRA", "负载信息", "图号", "备注", "版本"
		};
		navigation.NavigateWithHierarchy(typeof(ExtractPdfPage));
	}

	/// <summary>
	/// 智能IO分配命令
	/// 执行针对XT1系统优化的自动IO分配算法，根据信号类型、卡件规格和冗余率计算最优机柜分配方案
	/// XT1系统的分配算法考虑了龙鳞系统的特殊架构和性能要求
	/// 包括信号分类、卡件匹配、机柜容量计算和余量分配等关键步骤
	/// </summary>
	/// <returns>异步任务，表示分配操作的完成</returns>
	/// <exception cref="T:System.Exception">当必要的数据缺失或配置不当时抛出异常</exception>
	[RelayCommand]
	private async Task AllocateIO()
	{
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		model.Status.Busy("正在分配……");
		FormularHelper formularHelper = new FormularHelper();
		List<config_card_type_judge> configs = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData item in formularHelper.AutoAllocateXT1IO(AllData.ToList(), configs, (double)RedundancyRate / 100.0))
		{
			observableCollection.Add(item);
		}
		AllData = observableCollection;
		await SaveAndUploadFileAsync();
		model.Status.Success("分配完毕！");
	}

	/// <summary>
	/// 预览IO分配结果命令
	/// 设置当前控制系统为龙鳞，并导航到机柜分配结果预览页面
	/// 用于在正式分配之前预览和验证分配算法的结果
	/// 提供可视化的机柜布局和资源利用率分析
	/// </summary>
	[RelayCommand]
	private void PreviewAllocateIO()
	{
		parameterService.SetParameter("controlSystem", ControlSystem.龙鳍);
		navigation.NavigateWithHierarchy(typeof(CabinetAllocatedPage));
	}

	[RelayCommand]
	private async Task Add()
	{
		if (SubProject != null && AllData != null)
		{
			IoFullData ioFullData = new IoFullData();
			if (EditData(ioFullData, "添加"))
			{
				AllData.Add(ioFullData);
				await SaveAndUploadFileAsync();
			}
		}
	}

	[RelayCommand]
	private async Task Clear()
	{
		if (await message.ConfirmAsync("确认清空"))
		{
			AllData = new ObservableCollection<IoFullData>();
			await SaveAndUploadFileAsync();
		}
	}

	[RelayCommand]
	private async Task Edit(IoFullData data)
	{
		IoFullData ioFullData = new IoFullData().CopyPropertiesFrom(data);
		if (EditData(ioFullData, "编辑"))
		{
			data.CopyPropertiesFrom(ioFullData);
			await SaveAndUploadFileAsync();
			await ReloadAllData();
		}
	}

	[RelayCommand]
	private async Task Delete(IoFullData data)
	{
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		if (await message.ConfirmAsync("是否删除"))
		{
			AllData.Remove(data);
			await SaveAndUploadFileAsync();
		}
	}

	[RelayCommand]
	private async Task Import()
	{
		navigation.NavigateWithHierarchy(typeof(IODataPlatform.Views.SubPages.XT1.ImportPage));
	}

	[RelayCommand]
	private async Task Refresh()
	{
		model.Status.Busy("正在刷新……");
		await ReloadAllData();
		model.Status.Reset();
	}

	private bool EditData(IoFullData data, string title)
	{
		EditorOptionBuilder<IoFullData> editorOptionBuilder = data.CreateEditorBuilder();
		editorOptionBuilder.WithTitle(title);
		PropertyInfo[] properties = typeof(IoFullData).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			string header = propertyInfo.GetCustomAttribute<DisplayAttribute>()?.Name ?? propertyInfo.Name;
			if (propertyInfo.PropertyType == typeof(string))
			{
				editorOptionBuilder.AddProperty<string>(propertyInfo.Name).WithHeader(header).EditAsText();
			}
			else if (propertyInfo.PropertyType == typeof(float))
			{
				editorOptionBuilder.AddProperty<float>(propertyInfo.Name).WithHeader(header).EditAsDouble()
					.ConvertFromProperty((float x) => x)
					.ConvertToProperty((double x) => (float)x);
			}
			else if (propertyInfo.PropertyType == typeof(int))
			{
				editorOptionBuilder.AddProperty<int>(propertyInfo.Name).WithHeader(header).EditAsInt();
			}
			else if (propertyInfo.PropertyType == typeof(int?))
			{
				editorOptionBuilder.AddProperty<int?>(propertyInfo.Name).WithHeader(header).EditAsInt()
					.ConvertFromProperty((int? x) => x.GetValueOrDefault());
			}
			else if (propertyInfo.PropertyType == typeof(DateTime?))
			{
				editorOptionBuilder.AddProperty<DateTime?>(propertyInfo.Name).WithHeader(header).EditAsDateTime()
					.ConvertFromProperty((DateTime? x) => x.GetValueOrDefault())
					.ConvertToProperty((DateTime x) => (!(x == default(DateTime))) ? new DateTime?(x) : ((DateTime?)null));
			}
			else
			{
				Debugger.Break();
			}
		}
		return editorOptionBuilder.Build().EditWithWpfUI();
	}

	[RelayCommand]
	private async Task Export(string param)
	{
		if (AllData == null)
		{
			throw new Exception("没有可导出的数据");
		}
		string file = picker.SaveFile("Excel 文件(*.xlsx)|*.xlsx", param);
		if (file != null)
		{
			if (File.Exists(file))
			{
				File.Delete(file);
			}
			model.Status.Busy("正在" + param + "数据……");
			if (param == "导出IO点表")
			{
				List<IoFullData> data = AllData.Reset(AllData).ToList();
				await ExportIOSubStation(data, file);
			}
			model.Status.Success("已成功" + param + "数据：" + file);
		}
	}

	private async Task ExportIOSubStation(List<IoFullData> data, string filePath)
	{
		using DataTable dataTable = await data.ToTableByDisplayAttributeAsync();
		await excel.FastExportSheetAsync(dataTable, filePath, "系统一室IO表");
	}

	[RelayCommand]
	private async Task ClearAllFilterOptions()
	{
		if (await message.ConfirmAsync("确认重置全部筛选条件"))
		{
			isRefreshingOptions = true;
			Filters.AllDo(delegate(CommonFilter x)
			{
				x.Option = "全部";
			});
			isRefreshingOptions = false;
			FilterAndSort();
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
			dictionary["机柜号"].SetOptions(AllData.Select((IoFullData x) => x.CabinetNumber));
			dictionary["IO类型"].SetOptions(AllData.Select((IoFullData x) => x.IoType));
		}
		isRefreshingOptions = false;
	}

	[RelayCommand]
	private void FilterAndSort()
	{
		if (isRefreshingOptions || AllData == null)
		{
			return;
		}
		Dictionary<string, CommonFilter> filterDic = Filters.ToDictionary((CommonFilter x) => x.Title);
		IEnumerable<IoFullData> enumerable = AllData.WhereIf((IoFullData x) => x.CabinetNumber == filterDic["机柜号"].Option, filterDic["机柜号"].Option != "全部").WhereIf((IoFullData x) => x.IoType == filterDic["IO类型"].Option, filterDic["IO类型"].Option != "全部");
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData item in enumerable)
		{
			observableCollection.Add(item);
		}
		DisplayPoints = observableCollection;
	}

	[RelayCommand]
	private async Task AddSubProject()
	{
		_ = (Project ?? throw new Exception("请先选择项目")).Id;
		_ = (Major ?? throw new Exception("请选择专业")).Id;
		TextBox textbox = new TextBox
		{
			VerticalAlignment = VerticalAlignment.Center,
			PlaceholderText = "请输入子项名",
			Width = 180.0
		};
		if (await dialog.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions
		{
			Title = "添加子项",
			Content = textbox,
			PrimaryButtonText = "确定",
			CloseButtonText = "取消"
		}) == ContentDialogResult.Primary)
		{
			if (string.IsNullOrWhiteSpace(textbox.Text))
			{
				throw new Exception("请输入子项名");
			}
			string subProjectName = textbox.Text;
			SubProject = SubProjects.Where((config_project_subProject x) => x.Name == subProjectName).SingleOrDefault();
		}
	}

	[RelayCommand]
	private async Task DeleteSubProject()
	{
		_ = (Project ?? throw new Exception("请先选择要删除的子项")).Id;
		int subProjectId = (SubProject ?? throw new Exception("请先选择要删除的子项")).Id;
		if (await message.ConfirmAsync("是否删除子项"))
		{
			await (from x in context.Db.Deleteable<publish_io>()
				where x.SubProjectId == subProjectId
				select x).ExecuteCommandAsync();
			await (from x in context.Db.Deleteable<config_project_subProject>()
				where x.Id == subProjectId
				select x).ExecuteCommandAsync();
			await storage.DeleteSubprojectFolderAsync(subProjectId);
			await RefreshProjects();
		}
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
			await RefreshProjects();
		}
	}

	[RelayCommand]
	private async Task Publish()
	{
		_ = (Project ?? throw new Exception("开发人员注意")).Id;
		int id = (SubProject ?? throw new Exception("开发人员注意")).Id;
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		publishvm.Title = "系统一室IO发布";
		publishvm.SubProjectId = id;
		publishvm.saveAction = SaveAndUploadFileAsync;
		publishvm.downloadAndCoverAction = DownloadAndCover;
		navigation.NavigateWithHierarchy(typeof(CommonPublishPage));
	}

	private async Task DownloadAndCover(int subProjectId, int versionId)
	{
		string realtimeIoFileRelativePath = storage.GetRealtimeIoFileRelativePath(subProjectId);
		await storage.WebCopyFilesAsync(new _003C_003Ez__ReadOnlySingleElementList<(string, string)>((storage.GetPublishIoFileRelativePath(subProjectId, versionId), realtimeIoFileRelativePath)));
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.AllData" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.AllData" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnAllDataChanged(ObservableCollection<IoFullData>? value)
	{
		AllDataCount = value?.Count ?? 0;
		DisplayPoints = null;
		RefreshFilterOptions();
		FilterAndSort();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.DisplayPoints" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.DisplayPoints" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnDisplayPointsChanged(ObservableCollection<IoFullData>? value)
	{
		DisplayPointCount = value?.Count ?? 0;
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.IsAscending" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.IsAscending" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnIsAscendingChanged(bool value)
	{
		FilterAndSort();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.Project" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.Project" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnProjectChanged(config_project? value)
	{
		Major = null;
		if (value != null)
		{
			Majors = new ObservableCollection<config_project_major>(await (from x in context.Db.Queryable<config_project_major>()
				where (int)x.Department == 1
				where x.ProjectId == value.Id
				select x).ToListAsync());
			if (Majors.Count == 1)
			{
				Major = Majors[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.Major" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.Major" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnMajorChanged(config_project_major? value)
	{
		SubProjects = null;
		if (value != null)
		{
			SubProjects = new ObservableCollection<config_project_subProject>(await (from x in context.Db.Queryable<config_project_subProject>()
				where x.MajorId == value.Id
				select x).ToListAsync());
			if (SubProjects.Count == 1)
			{
				SubProject = SubProjects[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.SubProject" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT1ViewModel.SubProject" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnSubProjectChanged(config_project_subProject? value)
	{
		AllData = null;
		if (value != null)
		{
			model.Status.Busy("正在获取数据……");
			await ReloadAllData();
			model.Status.Reset();
		}
	}
}
