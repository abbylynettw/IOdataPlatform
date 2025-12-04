using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Aspose.Cells;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.SubPages.AQJ;
using IODataPlatform.Views.SubPages.Common;
using LYSoft.Libs;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// AQJ控制系统专业页面视图模型类
/// 负责AQJ控制系统IO数据的全生命周期管理，包括数据导入、智能分配、机柜计算等核心功能
/// 支持复杂的IO机柜自动分配算法，包括余量计算、卡件类型匹配和信号分组等高级功能
/// 提供完整的配置表管理和数据发布功能，支持实时数据同步和版本控制
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
/// <inheritdoc />
/// <inheritdoc />
public class DepAQJViewModel : ObservableObject, INavigationAware
{
	/// <summary>页面初始化状态标记，防止重复初始化操作</summary>
	private bool isInit;

	/// <summary>IO卡件类型配置集合，用于IO分配算法的类型匹配和针脚数量计算</summary>
	private List<config_card_type_judge> config_Card_Types;

	/// <summary>冗余率百分比，用于机柜容量计算时的安全余量设置，默认为20%</summary>
	[ObservableProperty]
	private int redundancyRate;

	/// <summary>当前页面的所有IO数据集合，包含完整的IO信号信息和机柜分配结果</summary>
	[ObservableProperty]
	private ObservableCollection<IoFullData>? allData;

	[ObservableProperty]
	private ObservableCollection<IoFullData>? displayPoints;

	[ObservableProperty]
	private int displayPointCount;

	[ObservableProperty]
	private int allDataCount;

	[ObservableProperty]
	private bool isAscending;

	private bool isRefreshingOptions;

	[ObservableProperty]
	private bool canEdit;

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

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.EditConfigurationTableCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<string>? editConfigurationTableCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.GonfigSignalGroupCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? gonfigSignalGroupCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.AllocateIOCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? allocateIOCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.AddTagCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addTagCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.DeleteTagCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? deleteTagCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.RecalcCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? recalcCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.PreviewAllocateIOCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? previewAllocateIOCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.AddCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.ClearCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? clearCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.EditCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<IoFullData>? editCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.DeleteCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<IoFullData>? deleteCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.ImportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? importCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.RefreshCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.ExportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<string>? exportCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.ClearAllFilterOptionsCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? clearAllFilterOptionsCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.FilterAndSortCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? filterAndSortCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.GenerateIOCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? generateIOCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.RefreshAllCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshAllCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.PublishCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? publishCommand;

	public ImmutableList<CommonFilter> Filters { get; }

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepAQJViewModel.redundancyRate" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepAQJViewModel.allData" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepAQJViewModel.displayPoints" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepAQJViewModel.displayPointCount" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepAQJViewModel.allDataCount" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepAQJViewModel.isAscending" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepAQJViewModel.canEdit" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool CanEdit
	{
		get
		{
			return canEdit;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(canEdit, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.CanEdit);
				canEdit = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.CanEdit);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepAQJViewModel.projects" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepAQJViewModel.majors" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepAQJViewModel.subProjects" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepAQJViewModel.project" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepAQJViewModel.major" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepAQJViewModel.subProject" />
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

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.EditConfigurationTable(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<string> EditConfigurationTableCommand => editConfigurationTableCommand ?? (editConfigurationTableCommand = new RelayCommand<string>(EditConfigurationTable));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.GonfigSignalGroup" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand GonfigSignalGroupCommand => gonfigSignalGroupCommand ?? (gonfigSignalGroupCommand = new RelayCommand(GonfigSignalGroup));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.AllocateIO" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AllocateIOCommand => allocateIOCommand ?? (allocateIOCommand = new AsyncRelayCommand(AllocateIO));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.AddTag" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddTagCommand => addTagCommand ?? (addTagCommand = new AsyncRelayCommand(AddTag));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.DeleteTag" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand DeleteTagCommand => deleteTagCommand ?? (deleteTagCommand = new AsyncRelayCommand(DeleteTag));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.Recalc" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RecalcCommand => recalcCommand ?? (recalcCommand = new AsyncRelayCommand(Recalc));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.PreviewAllocateIO" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand PreviewAllocateIOCommand => previewAllocateIOCommand ?? (previewAllocateIOCommand = new RelayCommand(PreviewAllocateIO));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.Add" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddCommand => addCommand ?? (addCommand = new AsyncRelayCommand(Add));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.Clear" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ClearCommand => clearCommand ?? (clearCommand = new AsyncRelayCommand(Clear));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.Edit(IODataPlatform.Models.ExcelModels.IoFullData)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<IoFullData> EditCommand => editCommand ?? (editCommand = new AsyncRelayCommand<IoFullData>(Edit));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.Delete(IODataPlatform.Models.ExcelModels.IoFullData)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<IoFullData> DeleteCommand => deleteCommand ?? (deleteCommand = new AsyncRelayCommand<IoFullData>(Delete));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.Import" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ImportCommand => importCommand ?? (importCommand = new AsyncRelayCommand(Import));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.Refresh" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new AsyncRelayCommand(Refresh));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.Export(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<string> ExportCommand => exportCommand ?? (exportCommand = new AsyncRelayCommand<string>(Export));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.ClearAllFilterOptions" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ClearAllFilterOptionsCommand => clearAllFilterOptionsCommand ?? (clearAllFilterOptionsCommand = new AsyncRelayCommand(ClearAllFilterOptions));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.FilterAndSort" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand FilterAndSortCommand => filterAndSortCommand ?? (filterAndSortCommand = new RelayCommand(FilterAndSort));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.GenerateIO" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand GenerateIOCommand => generateIOCommand ?? (generateIOCommand = new RelayCommand(GenerateIO));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.RefreshAll" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshAllCommand => refreshAllCommand ?? (refreshAllCommand = new AsyncRelayCommand(RefreshAll));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepAQJViewModel.Publish" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand PublishCommand => publishCommand ?? (publishCommand = new AsyncRelayCommand(Publish));

	/// <summary>
	/// AQJ控制系统专业页面视图模型类
	/// 负责AQJ控制系统IO数据的全生命周期管理，包括数据导入、智能分配、机柜计算等核心功能
	/// 支持复杂的IO机柜自动分配算法，包括余量计算、卡件类型匹配和信号分组等高级功能
	/// 提供完整的配置表管理和数据发布功能，支持实时数据同步和版本控制
	/// </summary>
	public DepAQJViewModel(SqlSugarContext context, GlobalModel model, ConfigTableViewModel configvm, IMessageService message, IContentDialogService dialog, INavigationService navigation, StorageService storage, ExcelService excel, IPickerService picker, PublishViewModel publishvm, DatabaseService database, NavigationParameterService parameterService)
	{
		_003Ccontext_003EP = context;
		_003Cmodel_003EP = model;
		_003Cconfigvm_003EP = configvm;
		_003Cmessage_003EP = message;
		_003Cnavigation_003EP = navigation;
		_003Cstorage_003EP = storage;
		_003Cexcel_003EP = excel;
		_003Cpicker_003EP = picker;
		_003Cpublishvm_003EP = publishvm;
		_003Cdatabase_003EP = database;
		_003CparameterService_003EP = parameterService;
		redundancyRate = 20;
		isAscending = true;
		_003C_003Ey__InlineArray6<CommonFilter> buffer = default(_003C_003Ey__InlineArray6<CommonFilter>);
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray6<CommonFilter>, CommonFilter>(ref buffer, 0) = new CommonFilter("机柜号");
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray6<CommonFilter>, CommonFilter>(ref buffer, 1) = new CommonFilter("机箱号");
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray6<CommonFilter>, CommonFilter>(ref buffer, 2) = new CommonFilter("槽位号");
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray6<CommonFilter>, CommonFilter>(ref buffer, 3) = new CommonFilter("通道号");
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray6<CommonFilter>, CommonFilter>(ref buffer, 4) = new CommonFilter("板卡类型");
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray6<CommonFilter>, CommonFilter>(ref buffer, 5) = new CommonFilter("IO类型");
		Filters = ImmutableList.Create(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<_003C_003Ey__InlineArray6<CommonFilter>, CommonFilter>(in buffer, 6));
		base._002Ector();
	}

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
		string relativePath = _003Cstorage_003EP.GetRealtimeIoFileRelativePath(subProjectId);
		string absolutePath = _003Cstorage_003EP.GetWebFileLocalAbsolutePath(relativePath);
		using DataTable dataTable = await data.ToTableByDisplayAttributeAsync();
		await _003Cexcel_003EP.FastExportAsync(dataTable, absolutePath);
		await _003Cstorage_003EP.UploadRealtimeIoFileAsync(subProjectId);
		if (versionId.HasValue)
		{
			await _003Cstorage_003EP.WebCopyFilesAsync(new _003C_003Ez__ReadOnlySingleElementList<(string, string)>((relativePath, _003Cstorage_003EP.GetPublishIoFileRelativePath(subProjectId, versionId.Value))));
		}
	}

	/// <summary>
	/// 从服务器下载并加载实时IO数据
	/// 清除当前数据，从服务器下载最新的实时数据文件并解析为IO对象集合
	/// 同时加载必要的IO卡件类型配置数据，用于后续的IO分配操作
	/// 包含完整的错误处理，当文件不存在或解析失败时返回空集合
	/// </summary>
	/// <returns>异步任务，表示数据加载操作的完成</returns>
	/// <exception cref="T:System.Exception">当必要的项目参数缺失时抛出异常</exception>
	public async Task ReloadAllData()
	{
		AllData = null;
		_ = (Project ?? throw new Exception("开发人员注意")).Id;
		_ = (Major ?? throw new Exception("开发人员注意")).Id;
		int id = (SubProject ?? throw new Exception("开发人员注意")).Id;
		try
		{
			string fileName = await _003Cstorage_003EP.DownloadRealtimeIoFileAsync(id);
			IEnumerable<IoFullData> enumerable = (await _003Cexcel_003EP.GetDataTableAsStringAsync(fileName, hasHeader: true)).StringTableToIEnumerableByDiplay<IoFullData>();
			ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
			foreach (IoFullData item in enumerable)
			{
				observableCollection.Add(item);
			}
			AllData = observableCollection;
		}
		catch
		{
			AllData = new ObservableCollection<IoFullData>();
		}
		config_Card_Types = await _003Ccontext_003EP.Db.Queryable<config_card_type_judge>().ToListAsync();
	}

	/// <summary>
	/// 编辑配置表命令
	/// 根据参数设置配置表的标题和数据类型，并导航到配置表编辑页面
	/// 支持多种业务配置：模拟量、数字量配置、控制站编号替换、变量扩展码替换等
	/// 这些配置直接影响AQJ系统的IO分配算法和数据处理逻辑
	/// </summary>
	/// <param name="param">配置表的类型名称参数</param>
	/// <exception cref="T:System.NotImplementedException">当遇到未实现的配置类型时抛出异常</exception>
	[RelayCommand]
	private void EditConfigurationTable(string param)
	{
		_003Cconfigvm_003EP.Title = param;
		ConfigTableViewModel configTableViewModel = _003Cconfigvm_003EP;
		configTableViewModel.DataType = param switch
		{
			"模拟量类型配置表" => typeof(config_aqj_analog), 
			"数字量类型配置表" => typeof(config_aqj_control), 
			"控制站编号替换配置表" => typeof(config_aqj_stationReplace), 
			"变量扩展码替换配置表" => typeof(config_aqj_tagReplace), 
			"控制站机柜对照配置表" => typeof(config_aqj_stationCabinet), 
			"IO卡型号配置表" => typeof(config_card_type_judge), 
			_ => throw new NotImplementedException(), 
		};
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(ConfigTablePage));
	}

	/// <summary>
	/// 配置信号分组命令
	/// 导航到信号分组配置页面，用于管理AQJ系统中的信号分组规则
	/// 信号分组影响IO分配算法中的信号类型识别和机柜分配逻辑
	/// </summary>
	[RelayCommand]
	private void GonfigSignalGroup()
	{
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(ConfigSignalGroupPage));
	}

	/// <summary>
	/// 智能IO分配命令
	/// 执行复杂的自动IO分配算法，根据信号类型、卡件规格和冗余率计算最优机柜分配方案
	/// 包括三个关键阶段：信号分类、卡件匹配、机柜容量计算和余量分配
	/// 算法考虑到电气安全、信号类型兼容性和系统扩展性等多个因素
	/// </summary>
	/// <returns>异步任务，表示分配操作的完成</returns>
	/// <exception cref="T:System.Exception">当必要的数据缺失或配置不当时抛出异常</exception>
	[RelayCommand]
	private async Task AllocateIO()
	{
		config_Card_Types = await _003Ccontext_003EP.Db.Queryable<config_card_type_judge>().ToListAsync();
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		_003Cmodel_003EP.Status.Busy("正在分配……");
		FormularHelper formularHelper = new FormularHelper();
		List<StdCabinet> cabinets = formularHelper.AutoAllocateLongHeIO(AllData.ToList(), config_Card_Types, (double)RedundancyRate / 100.0);
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData item in CabinetCalc.CabinetStructureToPoint(cabinets).ToObservable())
		{
			observableCollection.Add(item);
		}
		AllData = observableCollection;
		await SaveAndUploadFileAsync();
		_003Cmodel_003EP.Status.Success("分配完毕！");
	}

	/// <summary>
	/// 添加备用点位命令
	/// 为每个已分配的IO卡件自动添加剩余的备用通道
	/// 按照机柜、笼子、槽位和卡件类型分组，为每组填充剩余的针脚位置
	/// 备用点位用于系统扩展和维护，确保设备的可扩展性和维护便利性
	/// </summary>
	/// <returns>异步任务，表示添加操作的完成</returns>
	/// <exception cref="T:System.Exception">当找不到对应的卡件类型或数据异常时抛出异常</exception>
	[RelayCommand]
	private async Task AddTag()
	{
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		_003Cmodel_003EP.Status.Busy("正在添加备用点……");
		List<IoFullData> items = (from d in AllData
			group d by new { d.CabinetNumber, d.Cage, d.Slot, d.CardType }).SelectMany(group =>
		{
			config_card_type_judge config_card_type_judge = config_Card_Types.FirstOrDefault((config_card_type_judge c) => c.IoCardType == group.Key.CardType);
			if (config_card_type_judge == null)
			{
				throw new Exception("找不到" + group.Key.CardType + "板卡类型");
			}
			HashSet<int> second = group.Select((IoFullData g) => g.Channel).ToHashSet();
			IEnumerable<int> source = Enumerable.Range(1, config_card_type_judge.PinsCount).Except(second);
			IoFullData lastTag = group.LastOrDefault()?.JsonClone();
			if (lastTag == null)
			{
				throw new Exception($"{group.Key.CabinetNumber} {group.Key.Cage}{group.Key.Slot}上没有一个点");
			}
			return source.Select((int i) => new IoFullData
			{
				StationName = lastTag.StationName,
				CabinetNumber = lastTag.CabinetNumber,
				Cage = lastTag.Cage,
				Slot = lastTag.Slot,
				IoType = lastTag.IoType,
				CardType = lastTag.CardType,
				BackCardType = lastTag.BackCardType,
				TerminalBoardModel = lastTag.TerminalBoardModel,
				TerminalBoardNumber = lastTag.TerminalBoardNumber,
				Channel = i,
				TagName = "备用通道",
				PointType = TagType.BackUp
			});
		}).ToList();
		AllData.AddRange(items);
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData item in from c in AllData
			orderby c.CabinetNumber, c.Cage, c.Slot, c.Channel
			select c)
		{
			observableCollection.Add(item);
		}
		AllData = observableCollection;
		await SaveAndUploadFileAsync();
		_003Cmodel_003EP.Status.Reset();
	}

	[RelayCommand]
	private async Task DeleteTag()
	{
		if (AllData == null)
		{
			throw new Exception("未找到Io数据");
		}
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData item in AllData.RemoveWhere((IoFullData a) => a.PointType == TagType.BackUp))
		{
			observableCollection.Add(item);
		}
		AllData = observableCollection;
		await SaveAndUploadFileAsync();
	}

	[RelayCommand]
	private async Task Recalc()
	{
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData item in CabinetCalc.Recalc(AllData).ToObservable())
		{
			observableCollection.Add(item);
		}
		AllData = observableCollection;
		await SaveAndUploadFileAsync();
	}

	[RelayCommand]
	private void PreviewAllocateIO()
	{
		if (SubProject == null)
		{
			throw new Exception("子项目为空，找不到控制系统");
		}
		ControlSystem controlSystem = (from it in _003Ccontext_003EP.Db.Queryable<config_project_major>()
			where it.Id == SubProject.MajorId
			select it).First().ControlSystem;
		_003CparameterService_003EP.SetParameter("controlSystem", controlSystem);
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(CabinetAllocatedPage));
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
		if (await _003Cmessage_003EP.ConfirmAsync("确认清空"))
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
		if (await _003Cmessage_003EP.ConfirmAsync("是否删除"))
		{
			AllData.Remove(data);
			await SaveAndUploadFileAsync();
		}
	}

	[RelayCommand]
	private async Task Import()
	{
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(ImportPage));
	}

	[RelayCommand]
	private async Task Refresh()
	{
		_003Cmodel_003EP.Status.Busy("正在刷新……");
		await ReloadAllData();
		_003Cmodel_003EP.Status.Reset();
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

	public List<string> GetDefaultField()
	{
		if (SubProject == null)
		{
			throw new Exception("子项目为空，找不到控制系统");
		}
		ControlSystem controlSystem = (from it in _003Ccontext_003EP.Db.Queryable<config_project_major>()
			where it.Id == SubProject.MajorId
			select it).First().ControlSystem;
		return controlSystem switch
		{
			ControlSystem.龙核 => _003Cdatabase_003EP.GetLongHeFields(_003Ccontext_003EP), 
			ControlSystem.安全级模拟系统 => _003Cdatabase_003EP.GetAQJMNFields(_003Ccontext_003EP), 
			_ => throw new Exception($"安全级室不存在{controlSystem},控制系统错误"), 
		};
	}

	[RelayCommand]
	private async Task Export(string param)
	{
		if (AllData == null)
		{
			throw new Exception("没有可导出的数据");
		}
		string file = _003Cpicker_003EP.SaveFile("Excel 文件(*.xlsx)|*.xlsx", param);
		if (file == null)
		{
			return;
		}
		if (File.Exists(file))
		{
			File.Delete(file);
		}
		_003Cmodel_003EP.Status.Busy("正在" + param + "数据……");
		if (param == "导出IO清册（原格式-过渡使用）")
		{
			if (SubProject == null)
			{
				throw new Exception("子项目为空，找不到控制系统");
			}
			List<IoFullData> data = AllData.Reset(AllData).ToList();
			ControlSystem controlSystem = (from it in _003Ccontext_003EP.Db.Queryable<config_project_major>()
				where it.Id == SubProject.MajorId
				select it).First().ControlSystem;
			await ExportIOOriginal(data, file, controlSystem);
		}
		else if (param == "导出IO清册（标准格式）")
		{
			List<IoFullData> data2 = AllData.Reset(AllData).ToList();
			await ExportIOStandord(data2, file);
		}
		_003Cmodel_003EP.Status.Success("已成功" + param + "数据：" + file);
	}

	private async Task ExportIOStandord(List<IoFullData> data, string filePath)
	{
		using DataTable dataTable = await data.ToTableByDisplayAttributeAsync();
		await _003Cexcel_003EP.FastExportSheetAsync(dataTable, filePath, "标准IO清单");
	}

	private async Task ExportIOOriginal(List<IoFullData> data, string filePath, ControlSystem controlSystem)
	{
		using DataTable dataTable = data.ToCustomDataTable(_003Ccontext_003EP.Db, controlSystem);
		await _003Cexcel_003EP.FastExportSheetAsync(dataTable, filePath, "安全级IO清册");
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
			dictionary["机箱号"].SetOptions(AllData.Select((IoFullData x) => x.Cage.ToString()));
			dictionary["槽位号"].SetOptions(AllData.Select((IoFullData x) => x.Slot.ToString()));
			dictionary["通道号"].SetOptions(AllData.Select((IoFullData x) => x.Channel.ToString()));
			dictionary["板卡类型"].SetOptions(AllData.Select((IoFullData x) => x.CardType));
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
		IEnumerable<IoFullData> enumerable = AllData.WhereIf((IoFullData x) => x.CabinetNumber == filterDic["机柜号"].Option, filterDic["机柜号"].Option != "全部").WhereIf((IoFullData x) => x.IoType == filterDic["机箱号"].Option, filterDic["机箱号"].Option != "全部").WhereIf((IoFullData x) => x.IoType == filterDic["槽位号"].Option, filterDic["槽位号"].Option != "全部")
			.WhereIf((IoFullData x) => x.IoType == filterDic["通道号"].Option, filterDic["通道号"].Option != "全部")
			.WhereIf((IoFullData x) => x.IoType == filterDic["板卡类型"].Option, filterDic["板卡类型"].Option != "全部")
			.WhereIf((IoFullData x) => x.IoType == filterDic["IO类型"].Option, filterDic["IO类型"].Option != "全部");
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData item in enumerable)
		{
			observableCollection.Add(item);
		}
		DisplayPoints = observableCollection;
	}

	[RelayCommand]
	private async void GenerateIO()
	{
		if (Project?.Name == null)
		{
			throw new Exception("未选择子项！");
		}
		string log = "";
		string selectedFilePathA = _003Cpicker_003EP.OpenFile("请选择仪表信号基础表(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx");
		if (selectedFilePathA == null || string.IsNullOrEmpty(selectedFilePathA))
		{
			return;
		}
		string selectedFilePathD = _003Cpicker_003EP.OpenFile("请选择控制信号基础表(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx");
		if (selectedFilePathD == null || string.IsNullOrEmpty(selectedFilePathD))
		{
			return;
		}
		_003Cmodel_003EP.Status.Busy("正在读取模拟量配置表……");
		List<config_aqj_analog> configs = await _003Ccontext_003EP.Db.Queryable<config_aqj_analog>().ToListAsync();
		_003Cmodel_003EP.Status.Busy("正在读取仪表信号基础表并更新所属类型……");
		List<aqjs_InstrumentSignl_input> deviceInfosA = await GetInstrumentSignalListAsync(selectedFilePathA, configs);
		_003Cmodel_003EP.Status.Success("已成功写入所属类型到" + selectedFilePathA + "。");
		_003Cmodel_003EP.Status.Busy("正在读取数字量配置表……");
		List<config_aqj_control> configs2 = await _003Ccontext_003EP.Db.Queryable<config_aqj_control>().ToListAsync();
		_003Cmodel_003EP.Status.Busy("正在读取控制信号基础表并更新所属类型……");
		List<aqjs_controlSignal_input> deviceInfosD = await GetControlSignalListAsync(selectedFilePathD, configs2);
		_003Cmodel_003EP.Status.Success("已成功写入所属类型到" + selectedFilePathD + "。");
		List<config_aqj_signalGroup> configSignalGroups = await _003Ccontext_003EP.Db.Queryable<config_aqj_signalGroup>().ToListAsync();
		List<config_aqj_signalGroupDetail> configSignals = await _003Ccontext_003EP.Db.Queryable<config_aqj_signalGroupDetail>().ToListAsync();
		List<config_aqj_stationReplace> configReplaceStations = await _003Ccontext_003EP.Db.Queryable<config_aqj_stationReplace>().ToListAsync();
		List<config_aqj_tagReplace> configReplaceTagNames = await _003Ccontext_003EP.Db.Queryable<config_aqj_tagReplace>().ToListAsync();
		List<config_aqj_stationCabinet> source = await _003Ccontext_003EP.Db.Queryable<config_aqj_stationCabinet>().ToListAsync();
		List<config_aqj_signalGroupDetail> list = new List<config_aqj_signalGroupDetail>();
		foreach (IGrouping<string, aqjs_InstrumentSignl_input> item in from l in deviceInfosA
			group l by l.设备编号)
		{
			aqjs_InstrumentSignl_input device = item.FirstOrDefault();
			if (device == null)
			{
				continue;
			}
			config_aqj_signalGroup signalGroup = configSignalGroups.FirstOrDefault((config_aqj_signalGroup s) => s.signalGroupName == device.所属类型);
			if (signalGroup == null)
			{
				log = log + "未从配置表中找到" + device.所属类型 + "\n";
				continue;
			}
			List<config_aqj_signalGroupDetail> list2 = configSignals.Where((config_aqj_signalGroupDetail c) => c.signalGroupId == signalGroup.Id).ToList();
			if (list2.Count == 0)
			{
				log = log + "未从配置表中找到" + device.所属类型 + "的信号列表\n";
				continue;
			}
			foreach (config_aqj_signalGroupDetail item2 in list2)
			{
				config_aqj_signalGroupDetail model = item2.AsTrue<config_aqj_signalGroupDetail>().JsonClone();
				IEnumerable<config_aqj_stationReplace> enumerable = configReplaceStations.Where((config_aqj_stationReplace c) => c.ControlStation == device.站号);
				config_aqj_tagReplace config_aqj_tagReplace = configReplaceTagNames.FirstOrDefault((config_aqj_tagReplace c) => c.ControlStation == device.站号 && model.信号名称.Contains(c.IoPointNameBefore));
				model.信号名称 = model.信号名称.Replace("TagName", device.设备编号.Replace("-", ""));
				model.信号说明 = model.信号说明.Replace("TagName", device.设备编号.Replace("-", ""));
				model.原变量名 = model.原变量名.Replace("TagName", device.设备编号.Replace("-", ""));
				model.信号说明 = model.信号说明.Replace("FunctionDesc", device.功能描述);
				model.原变量名 = model.原变量名.Replace("设备编号", device.设备编号);
				model.安全分级 = ComputeSafetyClass(item2.源头目的, device.安全等级);
				if (enumerable != null && enumerable.Count() > 0)
				{
					foreach (config_aqj_stationReplace item3 in enumerable)
					{
						model.分配信息 = model.分配信息.Replace(item3.ControlStationBefore, item3.ControlStationAfter);
						model.控制站 = model.控制站.Replace(item3.ControlStationBefore, item3.ControlStationAfter);
					}
				}
				config_aqj_stationCabinet config_aqj_stationCabinet = source.FirstOrDefault((config_aqj_stationCabinet c) => c.ControlStationNumber == model.控制站);
				model.信号名称 = ((config_aqj_tagReplace != null) ? model.信号名称.Replace(config_aqj_tagReplace.IoPointNameBefore, config_aqj_tagReplace.IoPointNameAfter) : model.信号名称);
				model.机柜号 = ((config_aqj_stationCabinet != null) ? config_aqj_stationCabinet.CabinetNumber : model.机柜号);
				if (model.IO类型 == "AI")
				{
					model.量程上限 = device.仪表量程_最大.ToString();
					model.量程下限 = device.仪表量程_最小.ToString();
					model.单位 = device.仪表量程_单位;
				}
				else if (model.IO类型 == "AO")
				{
					model.量程上限 = device.二次表量程_最大.ToString();
					model.量程下限 = device.二次表量程_最小.ToString();
					model.单位 = device.二次表量程_单位;
				}
				list.Add(model);
			}
		}
		foreach (IGrouping<string, aqjs_controlSignal_input> item4 in from l in deviceInfosD
			group l by l.设备编号)
		{
			aqjs_controlSignal_input device2 = item4.FirstOrDefault();
			if (device2 == null)
			{
				continue;
			}
			config_aqj_signalGroup signalGroup2 = configSignalGroups.FirstOrDefault((config_aqj_signalGroup s) => s.signalGroupName == device2.所属类型);
			if (signalGroup2 == null)
			{
				log = log + "未从配置表中找到" + device2.所属类型 + "\n";
				continue;
			}
			List<config_aqj_signalGroupDetail> list3 = configSignals.Where((config_aqj_signalGroupDetail c) => c.signalGroupId == signalGroup2.Id).ToList();
			if (list3.Count == 0)
			{
				log = log + "未从配置表中找到" + device2.所属类型 + "的信号列表\n";
				continue;
			}
			foreach (config_aqj_signalGroupDetail item5 in list3)
			{
				config_aqj_signalGroupDetail model2 = item5.AsTrue<config_aqj_signalGroupDetail>().JsonClone();
				IEnumerable<config_aqj_stationReplace> enumerable2 = configReplaceStations.Where((config_aqj_stationReplace c) => c.ControlStation == device2.站号);
				config_aqj_tagReplace config_aqj_tagReplace2 = configReplaceTagNames.FirstOrDefault((config_aqj_tagReplace c) => c.ControlStation == device2.站号 && model2.信号名称.Contains(c.IoPointNameBefore));
				model2.信号名称 = model2.信号名称.Replace("TagName", device2.设备编号.Replace("-", ""));
				model2.信号说明 = model2.信号说明.Replace("TagName", device2.设备编号.Replace("-", ""));
				model2.原变量名 = model2.原变量名.Replace("TagName", device2.设备编号.Replace("-", ""));
				model2.信号说明 = model2.信号说明.Replace("FunctionDesc", device2.功能描述);
				model2.原变量名 = model2.原变量名.Replace("设备编号", device2.设备编号);
				model2.安全分级 = ComputeSafetyClass(item5.源头目的, device2.安全等级);
				if (enumerable2 != null && enumerable2.Count() > 0)
				{
					foreach (config_aqj_stationReplace item6 in enumerable2)
					{
						model2.分配信息 = model2.分配信息.Replace(item6.ControlStationBefore, item6.ControlStationAfter);
						model2.控制站 = model2.控制站.Replace(item6.ControlStationBefore, item6.ControlStationAfter);
					}
				}
				config_aqj_stationCabinet config_aqj_stationCabinet2 = source.FirstOrDefault((config_aqj_stationCabinet c) => c.ControlStationNumber == model2.控制站);
				model2.信号名称 = ((config_aqj_tagReplace2 != null) ? model2.信号名称.Replace(config_aqj_tagReplace2.IoPointNameBefore, config_aqj_tagReplace2.IoPointNameAfter) : model2.信号名称);
				model2.机柜号 = ((config_aqj_stationCabinet2 != null) ? config_aqj_stationCabinet2.CabinetNumber : model2.机柜号);
				model2.信号名称 = model2.信号名称.Replace("CabinetNumber", model2.机柜号);
				model2.信号说明 = model2.信号说明.Replace("CabinetNumber", model2.机柜号);
				list.Add(model2);
			}
		}
		IEnumerable<IGrouping<string, config_aqj_signalGroupDetail>> enumerable3 = from r in list
			group r by r.机柜号;
		foreach (IGrouping<string, config_aqj_signalGroupDetail> item7 in enumerable3)
		{
			config_aqj_signalGroup alarm = configSignalGroups.FirstOrDefault((config_aqj_signalGroup c) => c.signalGroupName.Contains("ALARM"));
			if (alarm == null)
			{
				continue;
			}
			IEnumerable<config_aqj_signalGroupDetail> enumerable4 = configSignals.Where((config_aqj_signalGroupDetail c) => c.signalGroupId == alarm.Id);
			if (enumerable4 == null || enumerable4.Count() == 0)
			{
				continue;
			}
			foreach (config_aqj_signalGroupDetail item8 in enumerable4)
			{
				config_aqj_signalGroupDetail config_aqj_signalGroupDetail = item8.AsTrue<config_aqj_signalGroupDetail>().JsonClone();
				config_aqj_signalGroupDetail.信号名称 = item8.信号名称.Replace("CabinetNumber", item7.Key);
				config_aqj_signalGroupDetail.信号说明 = item8.信号说明.Replace("CabinetNumber", item7.Key);
				config_aqj_signalGroupDetail.机柜号 = item7.Key;
				config_aqj_signalGroupDetail.控制站 = item7.ToList().FirstOrDefault().控制站;
				config_aqj_signalGroupDetail.TagType = TagType.Alarm;
				list.Add(config_aqj_signalGroupDetail);
			}
		}
		for (int num = 0; num < list.Count; num++)
		{
			list[num].序号 = (num + 1).ToString();
		}
		_003Cmodel_003EP.Status.Busy("开始生成安全级室IO清册...");
		DataTable data = await list.ToTableByDisplayAttributeAsync();
		string filePath = Path.Combine(new FileInfo(selectedFilePathA).DirectoryName, SubProject.Name + "生成的安全级室IO清册.xlsx");
		await _003Cexcel_003EP.FastExportToDesktopAsync(data, filePath);
		_003Cmodel_003EP.Status.Success("已生成IO清册到" + filePath);
	}

	private string ComputeSafetyClass(string destination, string inputSafetyClass)
	{
		if (string.IsNullOrEmpty(destination))
		{
			return "RS";
		}
		if (destination.ToUpper() == "LOCAL")
		{
			return inputSafetyClass;
		}
		if (destination.ToUpper().Contains("NR-DCS"))
		{
			return "NR";
		}
		return "RS";
	}

	public async Task<List<aqjs_InstrumentSignl_input>> GetInstrumentSignalListAsync(string fileName, List<config_aqj_analog> configs)
	{
		List<aqjs_InstrumentSignl_input> result = new List<aqjs_InstrumentSignl_input>();
		Workbook wb = new Workbook(fileName);
		try
		{
			Cells cells = wb.Worksheets[0].Cells;
			_003Cmodel_003EP.Status.Busy("正在检验信号表表头是否符合规范……");
			await VerifyTableAHeadersAsync(cells);
			Style style = wb.CreateStyle();
			style.ForegroundColor = Color.Yellow;
			style.Pattern = BackgroundType.Solid;
			style.HorizontalAlignment = TextAlignmentType.Center;
			StyleFlag flag = new StyleFlag();
			flag.All = true;
			return await Task.Run(delegate
			{
				aqjs_InstrumentSignl_input aqjs_InstrumentSignl_input = null;
				for (int i = 2; i <= cells.MaxDataRow; i++)
				{
					int result2;
					double result3;
					double result4;
					double result5;
					double result6;
					double result7;
					aqjs_InstrumentSignl_input aqjs_InstrumentSignl_input2 = new aqjs_InstrumentSignl_input
					{
						行号 = i,
						序号 = (int.TryParse(cells[i, 0].StringValue, out result2) ? result2 : 0),
						设备编号 = cells[i, 1].StringValue,
						功能1 = cells[i, 2].StringValue,
						安全等级 = cells[i, 3].StringValue,
						序列 = cells[i, 4].StringValue,
						安全级机柜 = cells[i, 5].StringValue,
						功能描述 = cells[i, 6].StringValue,
						IO类型_AI = cells[i, 7].StringValue,
						IO类型_AO = cells[i, 8].StringValue,
						IO类型_DI = cells[i, 9].StringValue,
						IO类型_DO = cells[i, 10].StringValue,
						信号类型 = cells[i, 11].StringValue,
						显示控制_DCS = cells[i, 12].StringValue,
						显示控制_中央控制室 = cells[i, 13].StringValue,
						显示控制_应急监控室 = cells[i, 14].StringValue,
						仪表量程_最小 = (double.TryParse(cells[i, 15].StringValue, out result3) ? result3 : 0.0),
						仪表量程_最大 = (double.TryParse(cells[i, 16].StringValue, out result4) ? result4 : 0.0),
						仪表量程_单位 = cells[i, 17].StringValue,
						运算 = cells[i, 18].StringValue,
						二次表量程_最小 = (double.TryParse(cells[i, 19].StringValue, out result5) ? result5 : 0.0),
						二次表量程_最大 = (double.TryParse(cells[i, 20].StringValue, out result6) ? result6 : 0.0),
						二次表量程_单位 = cells[i, 21].StringValue,
						功能2 = cells[i, 22].StringValue,
						阈值 = (double.TryParse(cells[i, 23].StringValue, out result7) ? result7 : 0.0),
						阈值_单位 = cells[i, 24].StringValue,
						转入DCS信号_AI = cells[i, 25].StringValue,
						转入DCS信号_AO = cells[i, 26].StringValue,
						转入DCS信号_DI = cells[i, 27].StringValue,
						转入DCS信号_DO = cells[i, 28].StringValue,
						DCS机柜 = cells[i, 29].StringValue,
						附注 = cells[i, 30].StringValue,
						原理图号 = cells[i, 31].StringValue,
						所属类型 = cells[i, 32].StringValue,
						站号 = cells[i, 33].StringValue
					};
					if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.设备编号) && aqjs_InstrumentSignl_input != null)
					{
						aqjs_InstrumentSignl_input2.行号 = i;
						aqjs_InstrumentSignl_input2.序号 = aqjs_InstrumentSignl_input.序号;
						cells[i, 0].Value = aqjs_InstrumentSignl_input2.序号;
						cells[i, 0].SetStyle(style, flag);
						aqjs_InstrumentSignl_input2.设备编号 = aqjs_InstrumentSignl_input.设备编号;
						cells[i, 1].Value = aqjs_InstrumentSignl_input2.设备编号;
						cells[i, 1].SetStyle(style, flag);
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.功能1))
						{
							aqjs_InstrumentSignl_input2.功能1 = aqjs_InstrumentSignl_input.功能1;
							cells[i, 2].Value = aqjs_InstrumentSignl_input2.功能1;
							cells[i, 2].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.安全等级))
						{
							aqjs_InstrumentSignl_input2.安全等级 = aqjs_InstrumentSignl_input.安全等级;
							cells[i, 3].Value = aqjs_InstrumentSignl_input2.安全等级;
							cells[i, 3].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.序列))
						{
							aqjs_InstrumentSignl_input2.序列 = aqjs_InstrumentSignl_input.序列;
							cells[i, 4].Value = aqjs_InstrumentSignl_input2.序列;
							cells[i, 4].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.安全级机柜))
						{
							aqjs_InstrumentSignl_input2.安全级机柜 = aqjs_InstrumentSignl_input.安全级机柜;
							cells[i, 5].Value = aqjs_InstrumentSignl_input2.安全级机柜;
							cells[i, 5].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.功能描述))
						{
							aqjs_InstrumentSignl_input2.功能描述 = aqjs_InstrumentSignl_input.功能描述;
							cells[i, 6].Value = aqjs_InstrumentSignl_input2.功能描述;
							cells[i, 6].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.信号类型))
						{
							aqjs_InstrumentSignl_input2.信号类型 = aqjs_InstrumentSignl_input.信号类型;
							cells[i, 11].Value = aqjs_InstrumentSignl_input2.信号类型;
							cells[i, 11].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.显示控制_DCS))
						{
							aqjs_InstrumentSignl_input2.显示控制_DCS = aqjs_InstrumentSignl_input.显示控制_DCS;
							cells[i, 12].Value = aqjs_InstrumentSignl_input2.显示控制_DCS;
							cells[i, 12].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.显示控制_中央控制室))
						{
							aqjs_InstrumentSignl_input2.显示控制_中央控制室 = aqjs_InstrumentSignl_input.显示控制_中央控制室;
							cells[i, 13].Value = aqjs_InstrumentSignl_input2.显示控制_中央控制室;
							cells[i, 13].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.显示控制_应急监控室))
						{
							aqjs_InstrumentSignl_input2.显示控制_应急监控室 = aqjs_InstrumentSignl_input.显示控制_应急监控室;
							cells[i, 14].Value = aqjs_InstrumentSignl_input2.显示控制_应急监控室;
							cells[i, 14].SetStyle(style, flag);
						}
						if (aqjs_InstrumentSignl_input2.仪表量程_最小 == 0.0)
						{
							aqjs_InstrumentSignl_input2.仪表量程_最小 = aqjs_InstrumentSignl_input.仪表量程_最小;
							cells[i, 15].Value = aqjs_InstrumentSignl_input2.仪表量程_最小;
							cells[i, 15].SetStyle(style, flag);
						}
						if (aqjs_InstrumentSignl_input2.仪表量程_最大 == 0.0)
						{
							aqjs_InstrumentSignl_input2.仪表量程_最大 = aqjs_InstrumentSignl_input.仪表量程_最大;
							cells[i, 16].Value = aqjs_InstrumentSignl_input2.仪表量程_最大;
							cells[i, 16].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.仪表量程_单位))
						{
							aqjs_InstrumentSignl_input2.仪表量程_单位 = aqjs_InstrumentSignl_input.仪表量程_单位;
							cells[i, 17].Value = aqjs_InstrumentSignl_input2.仪表量程_单位;
							cells[i, 17].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.运算))
						{
							aqjs_InstrumentSignl_input2.运算 = aqjs_InstrumentSignl_input.运算;
							cells[i, 18].Value = aqjs_InstrumentSignl_input2.运算;
							cells[i, 18].SetStyle(style, flag);
						}
						if (aqjs_InstrumentSignl_input2.二次表量程_最小 == 0.0)
						{
							aqjs_InstrumentSignl_input2.二次表量程_最小 = aqjs_InstrumentSignl_input.二次表量程_最小;
							cells[i, 19].Value = aqjs_InstrumentSignl_input2.二次表量程_最小;
							cells[i, 19].SetStyle(style, flag);
						}
						if (aqjs_InstrumentSignl_input2.二次表量程_最大 == 0.0)
						{
							aqjs_InstrumentSignl_input2.二次表量程_最大 = aqjs_InstrumentSignl_input.二次表量程_最大;
							cells[i, 20].Value = aqjs_InstrumentSignl_input2.二次表量程_最大;
							cells[i, 20].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.二次表量程_单位))
						{
							aqjs_InstrumentSignl_input2.二次表量程_单位 = aqjs_InstrumentSignl_input.二次表量程_单位;
							cells[i, 21].Value = aqjs_InstrumentSignl_input2.二次表量程_单位;
							cells[i, 21].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.DCS机柜))
						{
							aqjs_InstrumentSignl_input2.DCS机柜 = aqjs_InstrumentSignl_input.DCS机柜;
							cells[i, 29].Value = aqjs_InstrumentSignl_input2.DCS机柜;
							cells[i, 29].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.附注))
						{
							aqjs_InstrumentSignl_input2.附注 = aqjs_InstrumentSignl_input.附注;
							cells[i, 30].Value = aqjs_InstrumentSignl_input2.附注;
							cells[i, 30].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.原理图号))
						{
							aqjs_InstrumentSignl_input2.原理图号 = aqjs_InstrumentSignl_input.原理图号;
							cells[i, 31].Value = aqjs_InstrumentSignl_input2.原理图号;
							cells[i, 31].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.所属类型))
						{
							aqjs_InstrumentSignl_input2.所属类型 = aqjs_InstrumentSignl_input.所属类型;
							cells[i, 32].Value = aqjs_InstrumentSignl_input2.所属类型;
							cells[i, 32].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_InstrumentSignl_input2.站号))
						{
							aqjs_InstrumentSignl_input2.站号 = aqjs_InstrumentSignl_input.站号;
							cells[i, 33].Value = aqjs_InstrumentSignl_input2.站号;
							cells[i, 33].SetStyle(style, flag);
						}
					}
					else
					{
						aqjs_InstrumentSignl_input = aqjs_InstrumentSignl_input2;
					}
					result.Add(aqjs_InstrumentSignl_input2);
				}
				IEnumerable<IGrouping<string, aqjs_InstrumentSignl_input>> enumerable = from r in result
					group r by r.设备编号;
				foreach (IGrouping<string, aqjs_InstrumentSignl_input> device in enumerable)
				{
					aqjs_InstrumentSignl_input first = device.FirstOrDefault();
					config_aqj_analog config_aqj_analog = configs.FirstOrDefault((config_aqj_analog c) => c.SchematicType == first.原理图号.Trim() && c.IsContainsR == first.功能1.Contains("R") && IsValidFunction2(device.Select((aqjs_InstrumentSignl_input aqjs_InstrumentSignl_input3) => aqjs_InstrumentSignl_input3.功能2.Trim()), c));
					foreach (aqjs_InstrumentSignl_input item in device)
					{
						cells[item.行号, 32].Value = ((config_aqj_analog == null) ? "未找到" : config_aqj_analog.DeviceType);
						cells[item.行号, 32].SetStyle(style, flag);
					}
				}
				wb.Save(fileName);
				return result;
			});
		}
		finally
		{
			if (wb != null)
			{
				((IDisposable)wb).Dispose();
			}
		}
	}

	public async Task<List<aqjs_controlSignal_input>> GetControlSignalListAsync(string fileName, List<config_aqj_control> configs)
	{
		List<aqjs_controlSignal_input> result = new List<aqjs_controlSignal_input>();
		Workbook wb = new Workbook(fileName);
		try
		{
			Cells cells = wb.Worksheets[0].Cells;
			Style style = wb.CreateStyle();
			style.ForegroundColor = Color.Yellow;
			style.Pattern = BackgroundType.Solid;
			StyleFlag flag = new StyleFlag();
			flag.All = true;
			_003Cmodel_003EP.Status.Busy("正在检验信号表表头是否符合规范……");
			await VerifyTableDHeadersAsync(cells);
			return await Task.Run(delegate
			{
				aqjs_controlSignal_input aqjs_controlSignal_input = null;
				for (int i = 2; i <= cells.MaxDataRow; i++)
				{
					int result2;
					int result3;
					int result4;
					int result5;
					int result6;
					aqjs_controlSignal_input aqjs_controlSignal_input2 = new aqjs_controlSignal_input
					{
						行号 = i,
						序号 = (int.TryParse(cells[i, 0].StringValue, out result2) ? result2 : 0),
						设备编号 = cells[i, 1].StringValue,
						功能 = cells[i, 2].StringValue,
						安全等级 = cells[i, 3].StringValue,
						序列 = cells[i, 4].StringValue,
						安全级机柜 = cells[i, 5].StringValue,
						功能描述 = cells[i, 6].StringValue,
						IO类型_AI = (int.TryParse(cells[i, 7].StringValue, out result3) ? result3 : 0),
						IO类型_AO = (int.TryParse(cells[i, 8].StringValue, out result4) ? result4 : 0),
						IO类型_DI = (int.TryParse(cells[i, 9].StringValue, out result5) ? result5 : 0),
						IO类型_DO = (int.TryParse(cells[i, 10].StringValue, out result6) ? result6 : 0),
						信号类型 = cells[i, 11].StringValue,
						供电电压 = cells[i, 12].StringValue,
						显示控制_DCS = cells[i, 13].StringValue,
						显示控制_中央控制室 = cells[i, 14].StringValue,
						显示控制_应急监控室 = cells[i, 15].StringValue,
						转入DCS信号_AI = cells[i, 16].StringValue,
						转入DCS信号_AO = cells[i, 17].StringValue,
						转入DCS信号_DI = cells[i, 18].StringValue,
						转入DCS信号_DO = cells[i, 19].StringValue,
						DCS机柜 = cells[i, 20].StringValue,
						附注 = cells[i, 21].StringValue,
						原理图号 = cells[i, 22].StringValue,
						所属类型 = cells[i, 23].StringValue,
						站号 = cells[i, 24].StringValue
					};
					if (string.IsNullOrEmpty(aqjs_controlSignal_input2.设备编号) && aqjs_controlSignal_input != null)
					{
						aqjs_controlSignal_input2.行号 = i;
						aqjs_controlSignal_input2.序号 = aqjs_controlSignal_input.序号;
						cells[i, 0].Value = aqjs_controlSignal_input2.序号;
						cells[i, 0].SetStyle(style, flag);
						aqjs_controlSignal_input2.设备编号 = aqjs_controlSignal_input.设备编号;
						cells[i, 1].Value = aqjs_controlSignal_input2.设备编号;
						cells[i, 1].SetStyle(style, flag);
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.功能))
						{
							aqjs_controlSignal_input2.功能 = aqjs_controlSignal_input.功能;
							cells[i, 2].Value = aqjs_controlSignal_input2.功能;
							cells[i, 2].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.安全等级))
						{
							aqjs_controlSignal_input2.安全等级 = aqjs_controlSignal_input.安全等级;
							cells[i, 3].Value = aqjs_controlSignal_input2.安全等级;
							cells[i, 3].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.序列))
						{
							aqjs_controlSignal_input2.序列 = aqjs_controlSignal_input.序列;
							cells[i, 4].Value = aqjs_controlSignal_input2.序列;
							cells[i, 4].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.安全级机柜))
						{
							aqjs_controlSignal_input2.安全级机柜 = aqjs_controlSignal_input.安全级机柜;
							cells[i, 5].Value = aqjs_controlSignal_input2.安全级机柜;
							cells[i, 5].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.功能描述))
						{
							aqjs_controlSignal_input2.功能描述 = aqjs_controlSignal_input.功能描述;
							cells[i, 6].Value = aqjs_controlSignal_input2.功能描述;
							cells[i, 6].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.信号类型))
						{
							aqjs_controlSignal_input2.信号类型 = aqjs_controlSignal_input.信号类型;
							cells[i, 11].Value = aqjs_controlSignal_input2.信号类型;
							cells[i, 11].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.供电电压))
						{
							aqjs_controlSignal_input2.供电电压 = aqjs_controlSignal_input.供电电压;
							cells[i, 12].Value = aqjs_controlSignal_input2.供电电压;
							cells[i, 12].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.显示控制_DCS))
						{
							aqjs_controlSignal_input2.显示控制_DCS = aqjs_controlSignal_input.显示控制_DCS;
							cells[i, 13].Value = aqjs_controlSignal_input2.显示控制_DCS;
							cells[i, 13].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.显示控制_中央控制室))
						{
							aqjs_controlSignal_input2.显示控制_中央控制室 = aqjs_controlSignal_input.显示控制_中央控制室;
							cells[i, 14].Value = aqjs_controlSignal_input2.显示控制_中央控制室;
							cells[i, 14].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.显示控制_应急监控室))
						{
							aqjs_controlSignal_input2.显示控制_应急监控室 = aqjs_controlSignal_input.显示控制_应急监控室;
							cells[i, 15].Value = aqjs_controlSignal_input2.显示控制_应急监控室;
							cells[i, 15].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.DCS机柜))
						{
							aqjs_controlSignal_input2.DCS机柜 = aqjs_controlSignal_input.DCS机柜;
							cells[i, 20].Value = aqjs_controlSignal_input2.DCS机柜;
							cells[i, 20].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.附注))
						{
							aqjs_controlSignal_input2.附注 = aqjs_controlSignal_input.附注;
							cells[i, 21].Value = aqjs_controlSignal_input2.附注;
							cells[i, 21].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.原理图号))
						{
							aqjs_controlSignal_input2.原理图号 = aqjs_controlSignal_input.原理图号;
							cells[i, 22].Value = aqjs_controlSignal_input2.原理图号;
							cells[i, 22].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.所属类型))
						{
							aqjs_controlSignal_input2.所属类型 = aqjs_controlSignal_input.所属类型;
							cells[i, 23].Value = aqjs_controlSignal_input2.所属类型;
							cells[i, 23].SetStyle(style, flag);
						}
						if (string.IsNullOrEmpty(aqjs_controlSignal_input2.站号))
						{
							aqjs_controlSignal_input2.站号 = aqjs_controlSignal_input.站号;
							cells[i, 24].Value = aqjs_controlSignal_input2.站号;
							cells[i, 24].SetStyle(style, flag);
						}
					}
					else
					{
						aqjs_controlSignal_input = aqjs_controlSignal_input2;
					}
					result.Add(aqjs_controlSignal_input2);
				}
				IEnumerable<IGrouping<string, aqjs_controlSignal_input>> enumerable = from r in result
					group r by r.设备编号;
				foreach (IGrouping<string, aqjs_controlSignal_input> item in enumerable)
				{
					aqjs_controlSignal_input first = item.FirstOrDefault();
					int totalDOCount = item.Sum((aqjs_controlSignal_input d) => d.IO类型_DO);
					int totalDICount = item.Sum((aqjs_controlSignal_input d) => d.IO类型_DI);
					config_aqj_control config_aqj_control = configs.FirstOrDefault((config_aqj_control c) => c.SchematicType == first.原理图号 && c.IOTypeDONumber == totalDOCount && c.IOTypeDINumber == totalDICount && (string.IsNullOrEmpty(c.FunctionDescription) || c.FunctionDescription.Split(';').Any((string f) => first.功能描述.Contains(f))));
					foreach (aqjs_controlSignal_input item2 in item)
					{
						cells[item2.行号, 23].Value = ((config_aqj_control == null) ? "未找到" : config_aqj_control.DeviceType);
						cells[item2.行号, 23].SetStyle(style, flag);
					}
				}
				wb.Save(fileName);
				return result;
			});
		}
		finally
		{
			if (wb != null)
			{
				((IDisposable)wb).Dispose();
			}
		}
	}

	/// <summary> 验证模拟量输入信息表头  /// </summary> 
	public async Task<bool> VerifyTableAHeadersAsync(Cells cells)
	{
		return await Task.Run(delegate
		{
			List<string> list = new List<string>
			{
				"序号", "设备编号", "功能1", "安全等级", "序列", "安全级机柜", "功能描述", "IO类型", "", "",
				"", "信号类型", "显示/控制", "", "", "仪表量程", "", "", "运算", "二次表量程",
				"", "", "阈值", "", "", "转入DCS信号", "", "", "", "DCS机柜",
				"附注", "原理图号", "所属类型", "站号"
			};
			for (int i = 0; i < list.Count; i++)
			{
				Cell cell = cells[0, i];
				Cell cell2 = cells[1, i];
				if (cells[0, i].StringValue != list[i])
				{
					throw new Exception("表头不匹配，期望值: " + list[i] + ", 但找到: " + cells[0, i].StringValue);
				}
			}
			return true;
		});
	}

	/// <summary> 验证数字量输入信息表头  /// </summary> 
	public async Task<bool> VerifyTableDHeadersAsync(Cells cells)
	{
		return await Task.Run(delegate
		{
			List<string> list = new List<string>
			{
				"序号", "设备编号", "功能", "安全等级", "序列", "安全级机柜", "功能描述", "IO类型", "", "",
				"", "信号类型", "供电电压", "显示/控制", "", "", "转入DCS信号", "", "", "",
				"DCS机柜", "附注", "原理图号", "所属类型", "站号"
			};
			for (int i = 0; i < list.Count; i++)
			{
				Cell cell = cells[0, i];
				Cell cell2 = cells[1, i];
				if (cells[0, i].StringValue != list[i])
				{
					throw new Exception("表头不匹配，期望值: " + list[i] + ", 但找到: " + cells[0, i].StringValue);
				}
			}
			return true;
		});
	}

	public bool IsValidFunction2(IEnumerable<string> function2, config_aqj_analog config)
	{
		if (function2 == null || function2.Count() == 0)
		{
			return false;
		}
		List<string> source = new List<string> { "SH", "AH", "AHH", "AL", "ALL", "SAH", "SAL" };
		return source.All(delegate(string propertyName)
		{
			PropertyInfo property = typeof(config_aqj_analog).GetProperty(propertyName);
			if (property == null)
			{
				return false;
			}
			bool flag = (bool)property.GetValue(config);
			return (!function2.Contains(propertyName)) ? (!flag) : flag;
		});
	}

	private void SetPermissionBySubProject()
	{
		if (SubProject != null)
		{
			CanEdit = SubProject.CreatorUserId == _003Cmodel_003EP.User.Id;
		}
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
	private async Task Publish()
	{
		_ = (Project ?? throw new Exception("开发人员注意")).Id;
		int id = (SubProject ?? throw new Exception("开发人员注意")).Id;
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		_003Cpublishvm_003EP.Title = "安全级室IO发布";
		_003Cpublishvm_003EP.SubProjectId = id;
		_003Cpublishvm_003EP.saveAction = SaveAndUploadFileAsync;
		_003Cpublishvm_003EP.downloadAndCoverAction = DownloadAndCover;
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(CommonPublishPage));
	}

	private async Task DownloadAndCover(int subProjectId, int versionId)
	{
		string realtimeIoFileRelativePath = _003Cstorage_003EP.GetRealtimeIoFileRelativePath(subProjectId);
		await _003Cstorage_003EP.WebCopyFilesAsync(new _003C_003Ez__ReadOnlySingleElementList<(string, string)>((_003Cstorage_003EP.GetPublishIoFileRelativePath(subProjectId, versionId), realtimeIoFileRelativePath)));
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.AllData" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.AllData" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnAllDataChanged(ObservableCollection<IoFullData>? value)
	{
		AllDataCount = value?.Count ?? 0;
		DisplayPoints = null;
		RefreshFilterOptions();
		FilterAndSort();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.DisplayPoints" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.DisplayPoints" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnDisplayPointsChanged(ObservableCollection<IoFullData>? value)
	{
		DisplayPointCount = value?.Count ?? 0;
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.IsAscending" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.IsAscending" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnIsAscendingChanged(bool value)
	{
		FilterAndSort();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.Project" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.Project" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnProjectChanged(config_project? value)
	{
		Major = null;
		if (value != null)
		{
			Majors = new ObservableCollection<config_project_major>(await (from x in _003Ccontext_003EP.Db.Queryable<config_project_major>()
				where (int)x.Department == 8
				where x.ProjectId == value.Id
				select x).ToListAsync());
			if (Majors.Count == 1)
			{
				Major = Majors[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.Major" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.Major" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnMajorChanged(config_project_major? value)
	{
		SubProjects = null;
		if (value != null)
		{
			SubProjects = new ObservableCollection<config_project_subProject>(await (from x in _003Ccontext_003EP.Db.Queryable<config_project_subProject>()
				where x.MajorId == value.Id
				select x).ToListAsync());
			if (SubProjects.Count == 1)
			{
				SubProject = SubProjects[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.SubProject" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepAQJViewModel.SubProject" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnSubProjectChanged(config_project_subProject? value)
	{
		AllData = null;
		if (value != null)
		{
			SetPermissionBySubProject();
			_003Cmodel_003EP.Status.Busy("正在获取数据……");
			await ReloadAllData();
			_003Cmodel_003EP.Status.Reset();
		}
	}
}
