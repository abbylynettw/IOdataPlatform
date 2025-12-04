using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Aspose.Cells;
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
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace IODataPlatform.Views.Pages;

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
public class DepYJViewModel(SqlSugarContext context, GlobalModel model, ConfigTableViewModel configvm, IMessageService message, IContentDialogService dialog, INavigationService navigation, StorageService storage, ExcelService excel, IPickerService picker) : ObservableObject(), INavigationAware
{
	private bool isInit;

	[ObservableProperty]
	private ObservableCollection<IoData>? allData;

	[ObservableProperty]
	private int allDataCount;

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

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.EditConfigurationTableCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<string>? editConfigurationTableCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.AddCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.ClearCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? clearCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.EditCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<IoData>? editCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.DeleteCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<IoData>? deleteCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.ImportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? importCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.RefreshCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.GenerateDuanjieCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? generateDuanjieCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.AddSubProjectCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addSubProjectCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.DeleteSubProjectCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? deleteSubProjectCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.RefreshAllCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshAllCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.PublishCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? publishCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepYJViewModel.allData" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<IoData>? AllData
	{
		get
		{
			return allData;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<IoData>>.Default.Equals(allData, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.AllData);
				allData = value;
				OnAllDataChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.AllData);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepYJViewModel.allDataCount" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepYJViewModel.projects" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepYJViewModel.majors" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepYJViewModel.subProjects" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepYJViewModel.project" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepYJViewModel.major" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepYJViewModel.subProject" />
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

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepYJViewModel.EditConfigurationTable(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<string> EditConfigurationTableCommand => editConfigurationTableCommand ?? (editConfigurationTableCommand = new RelayCommand<string>(EditConfigurationTable));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepYJViewModel.Add" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddCommand => addCommand ?? (addCommand = new AsyncRelayCommand(Add));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepYJViewModel.Clear" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ClearCommand => clearCommand ?? (clearCommand = new AsyncRelayCommand(Clear));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepYJViewModel.Edit(IODataPlatform.Models.ExcelModels.IoData)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<IoData> EditCommand => editCommand ?? (editCommand = new AsyncRelayCommand<IoData>(Edit));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepYJViewModel.Delete(IODataPlatform.Models.ExcelModels.IoData)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<IoData> DeleteCommand => deleteCommand ?? (deleteCommand = new AsyncRelayCommand<IoData>(Delete));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepYJViewModel.Import" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ImportCommand => importCommand ?? (importCommand = new AsyncRelayCommand(Import));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepYJViewModel.Refresh" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new AsyncRelayCommand(Refresh));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepYJViewModel.GenerateDuanjie" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand GenerateDuanjieCommand => generateDuanjieCommand ?? (generateDuanjieCommand = new RelayCommand(GenerateDuanjie));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepYJViewModel.AddSubProject" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddSubProjectCommand => addSubProjectCommand ?? (addSubProjectCommand = new AsyncRelayCommand(AddSubProject));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepYJViewModel.DeleteSubProject" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand DeleteSubProjectCommand => deleteSubProjectCommand ?? (deleteSubProjectCommand = new AsyncRelayCommand(DeleteSubProject));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepYJViewModel.RefreshAll" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshAllCommand => refreshAllCommand ?? (refreshAllCommand = new AsyncRelayCommand(RefreshAll));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepYJViewModel.Publish" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand PublishCommand => publishCommand ?? (publishCommand = new AsyncRelayCommand(Publish));

	public void OnNavigatedFrom()
	{
	}

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

	/// <summary>保存当前AllData并上传实时文件，如需要发布，同时传入versionId参数</summary>
	/// <param name="versionId">发布版本ID</param>
	public async Task SaveAndUploadFileAsync(int? versionId = null)
	{
		_ = (Project ?? throw new Exception("开发人员注意")).Id;
		int subProjectId = (SubProject ?? throw new Exception("开发人员注意")).Id;
		ObservableCollection<IoData> data = AllData ?? throw new Exception("开发人员注意");
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

	/// <summary>从服务器下载实时数据文件并加载</summary>
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
				IEnumerable<IoData> enumerable = await Task.Run(() => dataTable.StringTableToIEnumerableByDiplay<IoData>());
				ObservableCollection<IoData> observableCollection = new ObservableCollection<IoData>();
				foreach (IoData item in enumerable)
				{
					observableCollection.Add(item);
				}
				AllData = observableCollection;
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
			AllData = new ObservableCollection<IoData>();
		}
	}

	[RelayCommand]
	private void EditConfigurationTable(string param)
	{
		configvm.Title = param;
		ConfigTableViewModel configTableViewModel = configvm;
		if (param == "典回配置表")
		{
			Type typeFromHandle = typeof(config_termination_yjs);
			configTableViewModel.DataType = typeFromHandle;
			navigation.NavigateWithHierarchy(typeof(ConfigTablePage));
			return;
		}
		throw new NotImplementedException();
	}

	[RelayCommand]
	private async Task Add()
	{
		if (SubProject != null && AllData != null)
		{
			IoData ioData = new IoData();
			if (EditData(ioData, "添加"))
			{
				AllData.Add(ioData);
				await SaveAndUploadFileAsync();
			}
		}
	}

	[RelayCommand]
	private async Task Clear()
	{
		if (await message.ConfirmAsync("确认清空"))
		{
			AllData = new ObservableCollection<IoData>();
			await SaveAndUploadFileAsync();
		}
	}

	[RelayCommand]
	private async Task Edit(IoData data)
	{
		IoData ioData = new IoData().CopyPropertiesFrom(data);
		if (EditData(ioData, "编辑"))
		{
			data.CopyPropertiesFrom(ioData);
			await SaveAndUploadFileAsync();
			await ReloadAllData();
		}
	}

	[RelayCommand]
	private async Task Delete(IoData data)
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
		string text = picker.OpenFile("Excel 文件(*.xls;*.xlsx)|*.xls;*.xlsx");
		if (text == null)
		{
			return;
		}
		model.Status.Busy("正在导入IO数据……");
		AllData = null;
		DataTable data = await excel.GetDataTableAsStringAsync(text, hasHeader: true);
		try
		{
			IEnumerable<IoData> enumerable = await Task.Run(() => data.StringTableToIEnumerableByDiplay<IoData>());
			ObservableCollection<IoData> observableCollection = new ObservableCollection<IoData>();
			foreach (IoData item in enumerable)
			{
				observableCollection.Add(item);
			}
			AllData = observableCollection;
			model.Status.Reset();
			await SaveAndUploadFileAsync();
		}
		finally
		{
			if (data != null)
			{
				((IDisposable)data).Dispose();
			}
		}
	}

	[RelayCommand]
	private async Task Refresh()
	{
		model.Status.Busy("正在刷新……");
		await ReloadAllData();
		model.Status.Reset();
	}

	private bool EditData(IoData data, string title)
	{
		EditorOptionBuilder<IoData> editorOptionBuilder = data.CreateEditorBuilder();
		editorOptionBuilder.WithTitle(title);
		PropertyInfo[] properties = typeof(IoData).GetProperties();
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
	public async void GenerateDuanjie()
	{
		string log = "";
		try
		{
			string text = picker.OpenFile("请选择要提取的内部接线清单(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx");
			if (text == null || string.IsNullOrEmpty(text))
			{
				return;
			}
			List<内部接线清单> qingdanData = GetQingdanData(text);
			List<典回> loopData = GetLoopData();
			List<config_termination_yjs> list = new List<config_termination_yjs>();
			IEnumerable<IGrouping<string, 内部接线清单>> enumerable = from data in qingdanData
				group data by data.典回类型;
			foreach (IGrouping<string, 内部接线清单> item2 in enumerable)
			{
				string loopType = item2.Key;
				典回 典回 = loopData.FirstOrDefault((典回 template) => template.典回类型 == loopType);
				if (典回 != null)
				{
					log = log + "已找到【" + loopType + "】典回";
					IEnumerable<IGrouping<string, 内部接线清单>> enumerable2 = from data in item2.ToList()
						group data by data.信号名称;
					foreach (IGrouping<string, 内部接线清单> item3 in enumerable2)
					{
						List<内部接线清单> source = item3.ToList();
						int num = source.Min((内部接线清单 data) => ExtractNumber(data.端子号)) - 1;
						内部接线清单 内部接线清单 = source.FirstOrDefault();
						foreach (config_termination_yjs loopRow in 典回.loopRows)
						{
							config_termination_yjs item = new config_termination_yjs
							{
								SignalName = item3.Key,
								SafetyLevel = 内部接线清单.安全等级,
								SafetyColumn = 内部接线清单.安全列,
								TerminalGroupName = 内部接线清单.端子组名,
								XCoordinate = ((内部接线清单.位置.Length == 4) ? 内部接线清单.位置.Substring(0, 2) : 内部接线清单.位置),
								YCoordinate = ((内部接线清单.位置.Length == 4) ? 内部接线清单.位置.Substring(2, 2) : 内部接线清单.位置),
								TerminalNumberP = ((!string.IsNullOrEmpty(loopRow.TerminalNumberP)) ? loopRow.TerminalNumberP.Replace(ExtractNumber(loopRow.TerminalNumberP).ToString(), (ExtractNumber(loopRow.TerminalNumberP) + num).ToString()) : loopRow.TerminalNumberP),
								TerminalNumberN = ((!string.IsNullOrEmpty(loopRow.TerminalNumberN)) ? loopRow.TerminalNumberN.Replace(ExtractNumber(loopRow.TerminalNumberN).ToString(), (ExtractNumber(loopRow.TerminalNumberN) + num).ToString()) : loopRow.TerminalNumberN)
							};
							list.Add(item);
						}
					}
				}
				else
				{
					log = log + "【错误】未在典回模板中找到【" + loopType + "】典回类型";
				}
			}
			ObservableCollection<IoData> observableCollection = new ObservableCollection<IoData>();
			foreach (IoData item4 in list.Select((config_termination_yjs config_termination_yjs) => new IoData
			{
				TagName = config_termination_yjs.SignalName + config_termination_yjs.ExtensionCode,
				OldVarName = config_termination_yjs.SignalName,
				OldExtCode = config_termination_yjs.ExtensionCode,
				Unit = config_termination_yjs.UnitNumber,
				Cabinet = config_termination_yjs.PanelNumber,
				IoType = config_termination_yjs.IOType,
				Destination = config_termination_yjs.Terminal,
				PowerType = config_termination_yjs.PowerSupply,
				LoopVoltage = config_termination_yjs.LoopVoltage,
				Ees = config_termination_yjs.EmergencyPowerSupply,
				TypicalLoopDrawing = config_termination_yjs.LoopType,
				SafetyClassDivision = config_termination_yjs.SafetyLevel,
				System = config_termination_yjs.SystemNumber,
				TerminalBlock = config_termination_yjs.TerminalGroupName,
				Connection1 = config_termination_yjs.TerminalNumberP,
				Connection2 = config_termination_yjs.TerminalNumberN
			}).ToList())
			{
				observableCollection.Add(item4);
			}
			AllData = observableCollection;
			model.Status.Busy("已完成");
			model.Status.Reset();
			await SaveAndUploadFileAsync();
		}
		catch (Exception ex)
		{
			_ = log + "Error: " + ex.Message;
		}
	}

	/// <summary>
	/// 提取字符串中的数字部分
	/// </summary>
	/// <param name="terminalNumber">包含数字的字符串</param>
	/// <returns>提取出的数字</returns>
	private int ExtractNumber(string terminalNumber)
	{
		return int.Parse(new string(terminalNumber.Where(char.IsDigit).ToArray()));
	}

	public List<内部接线清单> GetQingdanData(string filePath)
	{
		List<内部接线清单> list = new List<内部接线清单>();
		Workbook workbook = excel.GetWorkbook(filePath);
		foreach (Worksheet worksheet in workbook.Worksheets)
		{
			if (!(worksheet.Name != "目录") || !(worksheet.Name != "说明") || !(worksheet.Name != "对照表"))
			{
				continue;
			}
			List<内部接线清单> list2 = ReadDataFromWorksheet(worksheet).ToList();
			if (list2 != null && list2.Count > 0)
			{
				list2.ForEach(delegate(内部接线清单 item)
				{
					item.SheetName = worksheet.Name;
				});
				List<内部接线清单> list3 = list2.Where((内部接线清单 item) => !string.IsNullOrEmpty(item.典回类型) && item.典回类型 != "-").ToList();
				if (list3.Count > 0)
				{
					list.AddRange(list3);
				}
			}
		}
		return list;
	}

	private IEnumerable<内部接线清单> ReadDataFromWorksheet(Worksheet worksheet)
	{
		for (int rowIndex = 2; rowIndex <= worksheet.Cells.MaxDataRow; rowIndex++)
		{
			yield return new 内部接线清单
			{
				位置 = worksheet.Cells[rowIndex, 1].StringValue,
				典回类型 = worksheet.Cells[rowIndex, 7].StringValue,
				信号名称 = worksheet.Cells[rowIndex, 3].StringValue,
				安全等级 = worksheet.Cells[rowIndex, 5].StringValue,
				安全列 = worksheet.Cells[rowIndex, 6].StringValue,
				端子组名 = worksheet.Cells[rowIndex, 8].StringValue,
				端子号 = worksheet.Cells[rowIndex, 9].StringValue
			};
		}
	}

	/// <summary>
	/// 获取数据库中的典回数据
	/// </summary>
	/// <returns></returns>
	public List<典回> GetLoopData()
	{
		List<典回> list = new List<典回>();
		List<config_termination_yjs> source = context.Db.Queryable<config_termination_yjs>().ToList();
		IEnumerable<IGrouping<string, config_termination_yjs>> enumerable = from g in source
			group g by g.LoopType;
		foreach (IGrouping<string, config_termination_yjs> item2 in enumerable)
		{
			典回 item = new 典回
			{
				典回类型 = item2.Key,
				loopRows = item2.ToList()
			};
			list.Add(item);
		}
		return list;
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
		_ = (SubProject ?? throw new Exception("开发人员注意")).Id;
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		TextBox textbox = new TextBox
		{
			VerticalAlignment = VerticalAlignment.Center,
			PlaceholderText = "请输入发布版本号",
			Width = 180.0
		};
		if (await dialog.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions
		{
			Title = "发布",
			Content = textbox,
			PrimaryButtonText = "发布",
			CloseButtonText = "取消"
		}) == ContentDialogResult.Primary)
		{
			if (string.IsNullOrWhiteSpace(textbox.Text))
			{
				throw new Exception("请输入发布版本号");
			}
			string version = textbox.Text;
			string.IsNullOrWhiteSpace(version);
			model.Status.Busy("正在发布版本……");
			if ((await (from x in context.Db.Queryable<publish_io>()
				where x.SubProjectId == SubProject.Id
				select x.PublishedVersion).ToListAsync()).Contains(version))
			{
				throw new Exception("版本已存在：" + version);
			}
			publish_io publish = new publish_io
			{
				SubProjectId = SubProject.Id,
				PublishedVersion = version
			};
			await context.Db.Insertable(publish).ExecuteCommandIdentityIntoEntityAsync();
			await SaveAndUploadFileAsync(publish.Id);
			model.Status.Success("发布成功");
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.AllData" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.AllData" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnAllDataChanged(ObservableCollection<IoData>? value)
	{
		AllDataCount = AllData?.Count ?? 0;
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.Project" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.Project" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnProjectChanged(config_project? value)
	{
		Major = null;
		if (value != null)
		{
			Majors = new ObservableCollection<config_project_major>(await (from x in context.Db.Queryable<config_project_major>()
				where (int)x.Department == 4
				where x.ProjectId == value.Id
				select x).ToListAsync());
			if (Majors.Count == 1)
			{
				Major = Majors[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.Major" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.Major" /> is changed.</remarks>
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

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.SubProject" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepYJViewModel.SubProject" /> is changed.</remarks>
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
