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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.Common;
using IODataPlatform.Views.SubPages.Termination;
using LYSoft.Libs;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 端接管理页面视图模型类
/// 负责端接数据的全生命周期管理，包括数据导入、编辑、发布和文件同步功能
/// 支持实时数据处理和版本化发布机制，确保数据的准确性和可追溯性
/// 实现INavigationAware接口以支持页面导航和数据刷新机制
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
public class TerminationViewModel(SqlSugarContext context, INavigationService navigation, GlobalModel model, IMessageService message, IContentDialogService dialog, StorageService storage, ExcelService excel, IPickerService picker, PublishTerminationViewModel publishvm) : ObservableObject, INavigationAware
{
	/// <summary>页面初始化状态标记，防止重复初始化操作</summary>
	private bool isInit;

	/// <summary>当前页面的所有端接数据集合，用于界面显示和数据操作</summary>
	[ObservableProperty]
	private ObservableCollection<TerminationData>? allData;

	[ObservableProperty]
	private ObservableCollection<TerminationData>? displayData;

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
	private ObservableCollection<config_project_subProject>? subProjects;

	[ObservableProperty]
	private config_project? project;

	[ObservableProperty]
	private config_project_major? major;

	[ObservableProperty]
	private config_project_subProject? subProject;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.AddCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.ClearCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? clearCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.EditCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<TerminationData>? editCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.DeleteCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<TerminationData>? deleteCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.RefreshCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.ClearAllFilterOptionsCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? clearAllFilterOptionsCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.FilterCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? filterCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.ImportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<string>? importCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.ExportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<string>? exportCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.RefreshAllCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshAllCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.PublishCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? publishCommand;

	public ImmutableList<CommonFilter> Filters { get; }

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.TerminationViewModel.allData" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<TerminationData>? AllData
	{
		get
		{
			return allData;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<TerminationData>>.Default.Equals(allData, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.AllData);
				allData = value;
				OnAllDataChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.AllData);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.TerminationViewModel.displayData" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<TerminationData>? DisplayData
	{
		get
		{
			return displayData;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<TerminationData>>.Default.Equals(displayData, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DisplayData);
				displayData = value;
				OnDisplayDataChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DisplayData);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.TerminationViewModel.allDataCount" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.TerminationViewModel.displayDataCount" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.TerminationViewModel.projects" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.TerminationViewModel.majors" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.TerminationViewModel.subProjects" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.TerminationViewModel.project" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.TerminationViewModel.major" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.TerminationViewModel.subProject" />
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

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.TerminationViewModel.Add" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddCommand => addCommand ?? (addCommand = new AsyncRelayCommand(Add));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.TerminationViewModel.Clear" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ClearCommand => clearCommand ?? (clearCommand = new AsyncRelayCommand(Clear));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.TerminationViewModel.Edit(IODataPlatform.Models.ExcelModels.TerminationData)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<TerminationData> EditCommand => editCommand ?? (editCommand = new AsyncRelayCommand<TerminationData>(Edit));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.TerminationViewModel.Delete(IODataPlatform.Models.ExcelModels.TerminationData)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<TerminationData> DeleteCommand => deleteCommand ?? (deleteCommand = new AsyncRelayCommand<TerminationData>(Delete));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.TerminationViewModel.Refresh" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new AsyncRelayCommand(Refresh));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.TerminationViewModel.ClearAllFilterOptions" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ClearAllFilterOptionsCommand => clearAllFilterOptionsCommand ?? (clearAllFilterOptionsCommand = new AsyncRelayCommand(ClearAllFilterOptions));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.TerminationViewModel.Filter" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand FilterCommand => filterCommand ?? (filterCommand = new RelayCommand(Filter));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.TerminationViewModel.Import(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<string> ImportCommand => importCommand ?? (importCommand = new AsyncRelayCommand<string>(Import));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.TerminationViewModel.Export(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<string> ExportCommand => exportCommand ?? (exportCommand = new AsyncRelayCommand<string>(Export));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.TerminationViewModel.RefreshAll" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshAllCommand => refreshAllCommand ?? (refreshAllCommand = new AsyncRelayCommand(RefreshAll));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.TerminationViewModel.Publish" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand PublishCommand => publishCommand ?? (publishCommand = new AsyncRelayCommand(Publish));

	/// <summary>
	/// 页面导航离开时触发
	/// 当前实现为空，预留用于后续数据保存或清理操作
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
	/// 保存并上传实时端接文件，支持可选的版本发布
	/// 将当前AllData导出为Excel文件并上传到服务器
	/// 如果提供versionId参数，同时创建发布版本的副本用于正式发布
	/// </summary>
	/// <param name="versionId">可选的发布版本标识符，如果提供则同时创建发布版本</param>
	/// <returns>异步任务，表示保存和上传操作的完成</returns>
	/// <exception cref="T:System.Exception">当必要的项目或子项目信息缺失时抛出异常</exception>
	public async Task SaveAndUploadRealtimeFileAsync(int? versionId = null)
	{
		_ = (Project ?? throw new Exception("开发人员注意")).Id;
		int subProjectId = (SubProject ?? throw new Exception("开发人员注意")).Id;
		ObservableCollection<TerminationData> data = AllData ?? throw new Exception("开发人员注意");
		string realtimeTerminationFileRelativePath = storage.GetRealtimeTerminationFileRelativePath(subProjectId);
		string absolutePath = storage.GetWebFileLocalAbsolutePath(realtimeTerminationFileRelativePath);
		using DataTable dataTable = await data.ToTableByDisplayAttributeAsync();
		await excel.FastExportAsync(dataTable, absolutePath);
		await storage.UploadRealtimeTerminationFileAsync(subProjectId);
		if (versionId.HasValue)
		{
			string publishTerminationFileRelativePath = storage.GetPublishTerminationFileRelativePath(subProjectId, versionId.Value);
			string webFileLocalAbsolutePath = storage.GetWebFileLocalAbsolutePath(publishTerminationFileRelativePath);
			File.Copy(absolutePath, webFileLocalAbsolutePath, overwrite: true);
			await storage.UploadPublishTerminationFileAsync(subProjectId, versionId.Value);
		}
	}

	/// <summary>
	/// 从服务器下载并加载实时端接数据
	/// 清除当前数据，从服务器下载最新的实时数据文件并解析为对象集合
	/// 包含完整的错误处理，当文件不存在或解析失败时返回空集合
	/// </summary>
	/// <returns>异步任务，表示数据加载操作的完成</returns>
	/// <exception cref="T:System.Exception">当必要的项目信息缺失时抛出异常</exception>
	private async Task ReloadAllData()
	{
		AllData = null;
		_ = (Project ?? throw new Exception("开发人员注意")).Id;
		int id = (SubProject ?? throw new Exception("开发人员注意")).Id;
		try
		{
			string fileName = await storage.DownloadRealtimeTerminationFileAsync(id);
			DataTable dataTable = await excel.GetDataTableAsStringAsync(fileName, hasHeader: true);
			try
			{
				AllData = await Task.Run(() => new ObservableCollection<TerminationData>(dataTable.StringTableToIEnumerableByDiplay<TerminationData>()));
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
			AllData = new ObservableCollection<TerminationData>();
		}
	}

	[RelayCommand]
	private async Task Add()
	{
		if (SubProject != null && AllData != null)
		{
			TerminationData terminationData = new TerminationData();
			if (EditData(terminationData, "添加"))
			{
				AllData.Add(terminationData);
				await SaveAndUploadRealtimeFileAsync();
				RefreshFilterOptions();
				Filter();
			}
		}
	}

	[RelayCommand]
	private async Task Clear()
	{
		if (await message.ConfirmAsync("确认清空"))
		{
			AllData = new ObservableCollection<TerminationData>();
			await SaveAndUploadRealtimeFileAsync();
		}
	}

	[RelayCommand]
	private async Task Edit(TerminationData data)
	{
		TerminationData terminationData = new TerminationData().CopyPropertiesFrom(data);
		if (!EditData(terminationData, "编辑"))
		{
			return;
		}
		data.CopyPropertiesFrom(terminationData);
		ObservableCollection<TerminationData> observableCollection = new ObservableCollection<TerminationData>();
		foreach (TerminationData displayDatum in DisplayData)
		{
			observableCollection.Add(displayDatum);
		}
		DisplayData = observableCollection;
		await SaveAndUploadRealtimeFileAsync();
	}

	[RelayCommand]
	private async Task Delete(TerminationData data)
	{
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		if (await message.ConfirmAsync("是否删除"))
		{
			AllData.Remove(data);
			await SaveAndUploadRealtimeFileAsync();
		}
	}

	[RelayCommand]
	private async Task Refresh()
	{
		model.Status.Busy("正在刷新……");
		await ReloadAllData();
		model.Status.Reset();
	}

	private bool EditData(TerminationData data, string title)
	{
		EditorOptionBuilder<TerminationData> editorOptionBuilder = data.CreateEditorBuilder();
		editorOptionBuilder.WithTitle(title);
		PropertyInfo[] properties = typeof(TerminationData).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			DisplayAttribute customAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();
			if (!(customAttribute.GetAutoGenerateField() ?? true))
			{
				continue;
			}
			string propDisplayName = customAttribute.Name;
			CommonFilter commonFilter = Filters.SingleOrDefault((CommonFilter x) => x.Title == propDisplayName);
			if (commonFilter != null && !(commonFilter.Option == "全部"))
			{
				propertyInfo.SetValue(data, commonFilter.Option);
			}
			if (propertyInfo.PropertyType == typeof(string))
			{
				editorOptionBuilder.AddProperty<string>(propertyInfo.Name).WithHeader(propDisplayName).WithPlaceHoplder("请输入" + propDisplayName);
			}
			else if (propertyInfo.PropertyType == typeof(float))
			{
				editorOptionBuilder.AddProperty<float>(propertyInfo.Name).WithHeader(propDisplayName).EditAsDouble()
					.ConvertFromProperty((float x) => x)
					.ConvertToProperty((double x) => (float)x);
			}
			else if (propertyInfo.PropertyType == typeof(int))
			{
				editorOptionBuilder.AddProperty<int>(propertyInfo.Name).WithHeader(propDisplayName).EditAsInt();
			}
			else if (propertyInfo.PropertyType == typeof(int?))
			{
				editorOptionBuilder.AddProperty<int?>(propertyInfo.Name).WithHeader(propDisplayName).EditAsInt()
					.ConvertFromProperty((int? x) => x.GetValueOrDefault());
			}
			else if (propertyInfo.PropertyType == typeof(DateTime?))
			{
				editorOptionBuilder.AddProperty<DateTime?>(propertyInfo.Name).WithHeader(propDisplayName).EditAsDateTime()
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
			dictionary["机柜号"].SetOptions(AllData.Select((TerminationData x) => x.CabinetNumber));
			dictionary["点名"].SetOptions(AllData.Select((TerminationData x) => x.IOPointName));
		}
		isRefreshingOptions = false;
	}

	[RelayCommand]
	private void Filter()
	{
		if (!isRefreshingOptions && AllData != null)
		{
			Dictionary<string, CommonFilter> filterDic = Filters.ToDictionary((CommonFilter x) => x.Title);
			IEnumerable<TerminationData> collection = AllData.WhereIf((TerminationData x) => x.CabinetNumber == filterDic["机柜号"].Option, filterDic["机柜号"].Option != "全部").WhereIf((TerminationData x) => x.IOPointName == filterDic["点名"].Option, filterDic["点名"].Option != "全部");
			DisplayData = new ObservableCollection<TerminationData>(collection);
		}
	}

	[RelayCommand]
	private async Task Import(string param)
	{
		if (param == "导入发布IO数据")
		{
			await ImportIoData();
			return;
		}
		if (param == "导入数据")
		{
			ImportData();
			return;
		}
		throw new Exception("开发人员注意");
	}

	private async Task ImportIoData()
	{
		_ = (Project ?? throw new Exception("开发人员注意")).Id;
		if (Major == null)
		{
			throw new Exception();
		}
		int subProjectId = (SubProject ?? throw new Exception("开发人员注意")).Id;
		List<string> list = await (from x in context.Db.Queryable<publish_io>()
			where x.SubProjectId == subProjectId
			select x.PublishedVersion).ToListAsync();
		Wpf.Ui.Controls.TextBlock element = new Wpf.Ui.Controls.TextBlock
		{
			Text = "发布版本",
			HorizontalAlignment = HorizontalAlignment.Left
		};
		ComboBox combobox = new ComboBox
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			ItemsSource = list,
			Margin = new Thickness(0.0, 5.0, 0.0, 5.0)
		};
		Wpf.Ui.Controls.TextBlock element2 = new Wpf.Ui.Controls.TextBlock
		{
			Text = "注意：此操作会覆盖当前实时（未发布）数据",
			Foreground = new SolidColorBrush(Colors.Red),
			HorizontalAlignment = HorizontalAlignment.Left
		};
		StackPanel stackPanel = new StackPanel
		{
			VerticalAlignment = VerticalAlignment.Center,
			Width = 300.0
		};
		if (list.Count == 1)
		{
			combobox.Text = list[0];
		}
		stackPanel.Children.Add(element);
		stackPanel.Children.Add(combobox);
		stackPanel.Children.Add(element2);
		if (await dialog.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions
		{
			Title = "导入发布IO数据",
			Content = stackPanel,
			PrimaryButtonText = "导入",
			CloseButtonText = "取消"
		}) != ContentDialogResult.Primary)
		{
			return;
		}
		if (string.IsNullOrWhiteSpace(combobox.Text))
		{
			throw new Exception("请选择发布版本");
		}
		string versionText = combobox.Text;
		string.IsNullOrWhiteSpace(versionText);
		model.Status.Busy("正在导入IO数据……");
		publish_io publish_io = await Task.Run(() => (from x in context.Db.Queryable<publish_io>()
			where x.SubProjectId == subProjectId
			where x.PublishedVersion == versionText
			select x).Single());
		string text = await storage.DownloadPublishIoFileAsync(subProjectId, publish_io.Id);
		if (Major.Department == Department.系统二室)
		{
			List<StdCabinet> list2 = JsonSerializer.Deserialize<List<StdCabinet>>(await File.ReadAllTextAsync(text));
			if (list2 == null)
			{
				throw new Exception("开发人员注意");
			}
			IEnumerable<TerminationData> enumerable = from x in CabinetCalc.CabinetStructureToPoint(list2)
				select x.ToIoData().ToTerminationData();
			ObservableCollection<TerminationData> observableCollection = new ObservableCollection<TerminationData>();
			foreach (TerminationData item in enumerable)
			{
				observableCollection.Add(item);
			}
			AllData = observableCollection;
		}
		else if (Major.Department == Department.安全级室)
		{
			IEnumerable<TerminationData> enumerable2 = from x in (await excel.GetDataTableAsStringAsync(text, hasHeader: true)).StringTableToIEnumerableByDiplay<AQJIoData>()
				select x.ToIoData().ToTerminationData();
			ObservableCollection<TerminationData> observableCollection2 = new ObservableCollection<TerminationData>();
			foreach (TerminationData item2 in enumerable2)
			{
				observableCollection2.Add(item2);
			}
			AllData = observableCollection2;
		}
		else
		{
			if (Major.Department != Department.系统一室)
			{
				throw new NotImplementedException();
			}
			IEnumerable<TerminationData> enumerable3 = from x in (await excel.GetDataTableAsStringAsync(text, hasHeader: true)).StringTableToIEnumerableByDiplay<IoData>()
				select x.ToTerminationData();
			ObservableCollection<TerminationData> observableCollection3 = new ObservableCollection<TerminationData>();
			foreach (TerminationData item3 in enumerable3)
			{
				observableCollection3.Add(item3);
			}
			AllData = observableCollection3;
		}
		model.Status.Success("导入成功");
		await SaveAndUploadRealtimeFileAsync();
	}

	private void ImportData()
	{
		navigation.NavigateWithHierarchy(typeof(ImportPage));
	}

	[RelayCommand]
	private async Task Export(string param)
	{
		if (AllData == null)
		{
			throw new Exception("没有可导出的数据");
		}
		string file = picker.SaveFile("Excel 文件(*.xlsx)|*.xlsx", param);
		if (file == null)
		{
			return;
		}
		if (File.Exists(file))
		{
			File.Delete(file);
		}
		model.Status.Busy("正在" + param + "……");
		if (param == "导出全部端接数据")
		{
			await ExportAllData(file);
		}
		else
		{
			if (!(param == "导出筛选端接数据"))
			{
				throw new Exception("开发人员注意");
			}
			await ExportFilterData(file);
		}
		model.Status.Success("已成功" + param + "：" + file);
	}

	private async Task ExportAllData(string file)
	{
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		ExcelService excelService = excel;
		await excelService.FastExportAsync(await AllData.ToTableByDisplayAttributeAsync(), file);
	}

	private async Task ExportFilterData(string file)
	{
		if (DisplayData == null)
		{
			throw new Exception("开发人员注意");
		}
		ExcelService excelService = excel;
		await excelService.FastExportAsync(await DisplayData.ToTableByDisplayAttributeAsync(), file);
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
		publishvm.Title = "端接发布";
		publishvm.SubProjectId = id;
		publishvm.saveAction = SaveAndUploadRealtimeFileAsync;
		publishvm.downloadAndCoverAction = DownloadAndCover;
		navigation.NavigateWithHierarchy(typeof(CommonDuanjiePublishPage));
	}

	private async Task DownloadAndCover(int subProjectId, int versionId)
	{
		string realtimeTerminationFileRelativePath = storage.GetRealtimeTerminationFileRelativePath(subProjectId);
		await storage.WebCopyFilesAsync(new _003C_003Ez__ReadOnlySingleElementList<(string, string)>((storage.GetPublishTerminationFileRelativePath(subProjectId, versionId), realtimeTerminationFileRelativePath)));
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.AllData" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.AllData" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnAllDataChanged(ObservableCollection<TerminationData>? value)
	{
		DisplayData = null;
		AllDataCount = AllData?.Count ?? 0;
		if (AllData != null)
		{
			RefreshFilterOptions();
			Filter();
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.DisplayData" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.DisplayData" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnDisplayDataChanged(ObservableCollection<TerminationData>? value)
	{
		DisplayDataCount = DisplayData?.Count ?? 0;
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.Project" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.Project" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnProjectChanged(config_project? value)
	{
		Majors = null;
		if (Project != null)
		{
			Majors = new ObservableCollection<config_project_major>(await (from x in context.Db.Queryable<config_project_major>()
				where x.ProjectId == Project.Id
				select x).ToListAsync());
			if (Majors.Count == 1)
			{
				Major = Majors[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.Major" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.Major" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnMajorChanged(config_project_major? value)
	{
		SubProjects = null;
		if (Major != null)
		{
			SubProjects = new ObservableCollection<config_project_subProject>(await (from x in context.Db.Queryable<config_project_subProject>()
				where x.MajorId == Major.Id
				select x).ToListAsync());
			if (SubProjects.Count == 1)
			{
				SubProject = SubProjects[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.SubProject" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.TerminationViewModel.SubProject" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnSubProjectChanged(config_project_subProject? value)
	{
		AllData = null;
		if (SubProject != null)
		{
			model.Status.Busy("正在获取数据……");
			await ReloadAllData();
			model.Status.Reset();
		}
	}
}
