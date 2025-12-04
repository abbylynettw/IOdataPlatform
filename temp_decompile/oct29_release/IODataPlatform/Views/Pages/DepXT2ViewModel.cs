using System;
using System.CodeDom.Compiler;
using System.Collections;
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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
using IODataPlatform.Views.SubPages.XT2;
using IODataPlatform.Views.Windows;
using LYSoft.Libs;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// DepXT2视图模型的计算功能部分类
/// 负责龙鳍控制系统(XT2)的IO数据计算和处理，包括板卡类型识别、信号映射、端子板配置等核心业务逻辑
/// 支持基于公式引擎的动态计算和硬编码计算两种模式，适用于工业自动化系统的IO配置管理
/// 主要业务场景：工程设计阶段的IO点配置、现场调试阶段的信号核对、运维阶段的配置变更
/// </summary>
/// <summary>
/// DepXT2ViewModel的排序功能扩展
/// 包含两类排序功能：
/// 1. 通用排序：按机柜号、机笼号、插槽、通道号等标准字段排序（不含DP阀位顺序）
/// 2. 阀门排序：在箱内信号中，按DP阀位顺序进行排序
/// 3. 导入阀门顺序：从 Excel 导入阀门编号和气口顺序，填充到 DP阀位顺序字段
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
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class DepXT2ViewModel(SqlSugarContext context, ConfigTableViewModel configvm, INavigationService navigation, GlobalModel model, IMessageService message, IContentDialogService dialog, StorageService storage, ExcelService excel, IPickerService picker, PublishViewModel publishvm, DatabaseService database, ExtractPdfViewModel epvm, NavigationParameterService parameterService, CloudExportConfigService cloudExportConfigService) : ObservableObject, INavigationAware
{
	/// <summary>
	/// 设备组的实现类，用于适配 IGrouping 接口
	/// </summary>
	private class DeviceGroup : IGrouping<string, IoFullData>, IEnumerable<IoFullData>, IEnumerable
	{
		private readonly List<IoFullData> _signals;

		public string Key { get; }

		public DeviceGroup(string key, List<IoFullData> signals)
		{
			Key = key;
			_signals = signals ?? new List<IoFullData>();
		}

		public IEnumerator<IoFullData> GetEnumerator()
		{
			return _signals.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	/// <summary>
	/// 是否使用公式模式进行计算
	/// 当设置为true时，使用硬编码的计算方法；设置为false时，使用数据库中的公式配置
	/// 用于在调试阶段快速验证计算逻辑，或在公式配置不完整时提供备用计算方案
	/// </summary>
	[ObservableProperty]
	private bool useFormula = true;

	private bool isInit;

	[ObservableProperty]
	private ObservableCollection<IoFullData>? allData;

	private List<config_card_type_judge> config_Card_Types;

	[ObservableProperty]
	private bool isCabinetSummaryFlyoutOpen;

	[ObservableProperty]
	private bool isTotalSummaryFlyoutOpen;

	[ObservableProperty]
	private TotalSummaryInfo? totalSummaryInfo;

	[ObservableProperty]
	private int redundancyRate = 20;

	[ObservableProperty]
	private ObservableCollection<IoFullData>? displayPoints;

	[ObservableProperty]
	private int displayPointCount;

	[ObservableProperty]
	private int allPointCount;

	[ObservableProperty]
	private int allDataCount;

	[ObservableProperty]
	private bool isAscending = true;

	/// <summary>
	/// 筛选模式是否启用（显示列头筛选图标）
	/// </summary>
	[ObservableProperty]
	private bool isFilterModeEnabled;

	private bool isRefreshingOptions;

	[ObservableProperty]
	private string sortType1 = "升序";

	[ObservableProperty]
	private string sortType2 = "升序";

	[ObservableProperty]
	private string sortType3 = "升序";

	[ObservableProperty]
	private string sortType4 = "升序";

	[ObservableProperty]
	private string sortType5 = "升序";

	[ObservableProperty]
	private ObservableCollection<string> sortOptions1 = new ObservableCollection<string> { "全部", "机柜号", "机架", "插槽", "通道", "FF/DP端子通道" };

	[ObservableProperty]
	private string sortOption1 = "全部";

	[ObservableProperty]
	private ObservableCollection<string> sortOptions2 = new ObservableCollection<string> { "全部" };

	[ObservableProperty]
	private string sortOption2 = "全部";

	[ObservableProperty]
	private ObservableCollection<string> sortOptions3 = new ObservableCollection<string> { "全部" };

	[ObservableProperty]
	private string sortOption3 = "全部";

	[ObservableProperty]
	private ObservableCollection<string> sortOptions4 = new ObservableCollection<string> { "全部" };

	[ObservableProperty]
	private string sortOption4 = "全部";

	[ObservableProperty]
	private ObservableCollection<string> sortOptions5 = new ObservableCollection<string> { "全部" };

	[ObservableProperty]
	private string sortOption5 = "全部";

	private ImmutableList<string> allSortOptions;

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

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.RecalcCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<string>? recalcCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.GetTotalSummaryInfoCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? getTotalSummaryInfoCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.ExtractPdfDataCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? extractPdfDataCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.ImportExcelDataCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? importExcelDataCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.AddTagCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<TagType>? addTagCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.DeleteTagCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<TagType>? deleteTagCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.EditConfigurationTableCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<string>? editConfigurationTableCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.AllocateIOCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? allocateIOCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.PreviewAllocateIOCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? previewAllocateIOCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.AllocateFFSlaveModulesCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? allocateFFSlaveModulesCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.AddCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.ClearCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? clearCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.EditCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<IoFullData>? editCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.DeleteCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<IoFullData>? deleteCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.ImportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? importCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.RefreshCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.DynamicExportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? dynamicExportCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.ExportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<string>? exportCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.ClearAllFilterOptionsCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? clearAllFilterOptionsCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.FilterAndSortCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? filterAndSortCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.RefreshAllCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? refreshAllCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.PublishCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? publishCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.GeneralSortingCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? generalSortingCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.ValvePositionSortingCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? valvePositionSortingCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.ValveSortingCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? valveSortingCommand;

	public ImmutableList<ExcelFilter> Filters { get; private set; } = ImmutableList.Create(default(ReadOnlySpan<ExcelFilter>));

	public ObservableCollection<string> SortTypes { get; } = new ObservableCollection<string> { "升序", "降序" };

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.useFormula" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool UseFormula
	{
		get
		{
			return useFormula;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(useFormula, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.UseFormula);
				useFormula = value;
				OnUseFormulaChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.UseFormula);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.allData" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.isCabinetSummaryFlyoutOpen" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsCabinetSummaryFlyoutOpen
	{
		get
		{
			return isCabinetSummaryFlyoutOpen;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(isCabinetSummaryFlyoutOpen, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsCabinetSummaryFlyoutOpen);
				isCabinetSummaryFlyoutOpen = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsCabinetSummaryFlyoutOpen);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.isTotalSummaryFlyoutOpen" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsTotalSummaryFlyoutOpen
	{
		get
		{
			return isTotalSummaryFlyoutOpen;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(isTotalSummaryFlyoutOpen, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsTotalSummaryFlyoutOpen);
				isTotalSummaryFlyoutOpen = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsTotalSummaryFlyoutOpen);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.totalSummaryInfo" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public TotalSummaryInfo? TotalSummaryInfo
	{
		get
		{
			return totalSummaryInfo;
		}
		set
		{
			if (!EqualityComparer<IODataPlatform.Models.ExcelModels.TotalSummaryInfo>.Default.Equals(totalSummaryInfo, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.TotalSummaryInfo);
				totalSummaryInfo = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.TotalSummaryInfo);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.redundancyRate" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.displayPoints" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.displayPointCount" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.allPointCount" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int AllPointCount
	{
		get
		{
			return allPointCount;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(allPointCount, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.AllPointCount);
				allPointCount = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.AllPointCount);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.allDataCount" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.isAscending" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.isFilterModeEnabled" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsFilterModeEnabled
	{
		get
		{
			return isFilterModeEnabled;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(isFilterModeEnabled, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsFilterModeEnabled);
				isFilterModeEnabled = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsFilterModeEnabled);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortType1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SortType1
	{
		get
		{
			return sortType1;
		}
		[MemberNotNull("sortType1")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(sortType1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortType1);
				sortType1 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortType1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortType2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SortType2
	{
		get
		{
			return sortType2;
		}
		[MemberNotNull("sortType2")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(sortType2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortType2);
				sortType2 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortType2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortType3" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SortType3
	{
		get
		{
			return sortType3;
		}
		[MemberNotNull("sortType3")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(sortType3, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortType3);
				sortType3 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortType3);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortType4" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SortType4
	{
		get
		{
			return sortType4;
		}
		[MemberNotNull("sortType4")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(sortType4, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortType4);
				sortType4 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortType4);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortType5" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SortType5
	{
		get
		{
			return sortType5;
		}
		[MemberNotNull("sortType5")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(sortType5, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortType5);
				sortType5 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortType5);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortOptions1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<string> SortOptions1
	{
		get
		{
			return sortOptions1;
		}
		[MemberNotNull("sortOptions1")]
		set
		{
			if (!EqualityComparer<ObservableCollection<string>>.Default.Equals(sortOptions1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortOptions1);
				sortOptions1 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortOptions1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortOption1" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SortOption1
	{
		get
		{
			return sortOption1;
		}
		[MemberNotNull("sortOption1")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(sortOption1, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortOption1);
				sortOption1 = value;
				OnSortOption1Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortOption1);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortOptions2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<string> SortOptions2
	{
		get
		{
			return sortOptions2;
		}
		[MemberNotNull("sortOptions2")]
		set
		{
			if (!EqualityComparer<ObservableCollection<string>>.Default.Equals(sortOptions2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortOptions2);
				sortOptions2 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortOptions2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortOption2" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SortOption2
	{
		get
		{
			return sortOption2;
		}
		[MemberNotNull("sortOption2")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(sortOption2, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortOption2);
				sortOption2 = value;
				OnSortOption2Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortOption2);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortOptions3" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<string> SortOptions3
	{
		get
		{
			return sortOptions3;
		}
		[MemberNotNull("sortOptions3")]
		set
		{
			if (!EqualityComparer<ObservableCollection<string>>.Default.Equals(sortOptions3, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortOptions3);
				sortOptions3 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortOptions3);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortOption3" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SortOption3
	{
		get
		{
			return sortOption3;
		}
		[MemberNotNull("sortOption3")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(sortOption3, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortOption3);
				sortOption3 = value;
				OnSortOption3Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortOption3);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortOptions4" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<string> SortOptions4
	{
		get
		{
			return sortOptions4;
		}
		[MemberNotNull("sortOptions4")]
		set
		{
			if (!EqualityComparer<ObservableCollection<string>>.Default.Equals(sortOptions4, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortOptions4);
				sortOptions4 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortOptions4);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortOption4" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SortOption4
	{
		get
		{
			return sortOption4;
		}
		[MemberNotNull("sortOption4")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(sortOption4, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortOption4);
				sortOption4 = value;
				OnSortOption4Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortOption4);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortOptions5" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<string> SortOptions5
	{
		get
		{
			return sortOptions5;
		}
		[MemberNotNull("sortOptions5")]
		set
		{
			if (!EqualityComparer<ObservableCollection<string>>.Default.Equals(sortOptions5, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortOptions5);
				sortOptions5 = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortOptions5);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.sortOption5" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string SortOption5
	{
		get
		{
			return sortOption5;
		}
		[MemberNotNull("sortOption5")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(sortOption5, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortOption5);
				sortOption5 = value;
				OnSortOption5Changed(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortOption5);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.canEdit" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.projects" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.majors" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.subProjects" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.project" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.major" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.DepXT2ViewModel.subProject" />
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

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.Recalc(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<string> RecalcCommand => recalcCommand ?? (recalcCommand = new AsyncRelayCommand<string>(Recalc));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.GetTotalSummaryInfo" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand GetTotalSummaryInfoCommand => getTotalSummaryInfoCommand ?? (getTotalSummaryInfoCommand = new RelayCommand(GetTotalSummaryInfo));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.ExtractPdfData" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ExtractPdfDataCommand => extractPdfDataCommand ?? (extractPdfDataCommand = new RelayCommand(ExtractPdfData));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.ImportExcelData" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ImportExcelDataCommand => importExcelDataCommand ?? (importExcelDataCommand = new RelayCommand(ImportExcelData));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.AddTag(IODataPlatform.Models.ExcelModels.TagType)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<TagType> AddTagCommand => addTagCommand ?? (addTagCommand = new AsyncRelayCommand<TagType>(AddTag));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.DeleteTag(IODataPlatform.Models.ExcelModels.TagType)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<TagType> DeleteTagCommand => deleteTagCommand ?? (deleteTagCommand = new AsyncRelayCommand<TagType>(DeleteTag));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.EditConfigurationTable(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<string> EditConfigurationTableCommand => editConfigurationTableCommand ?? (editConfigurationTableCommand = new RelayCommand<string>(EditConfigurationTable));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.AllocateIO" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AllocateIOCommand => allocateIOCommand ?? (allocateIOCommand = new AsyncRelayCommand(AllocateIO));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.PreviewAllocateIO" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand PreviewAllocateIOCommand => previewAllocateIOCommand ?? (previewAllocateIOCommand = new RelayCommand(PreviewAllocateIO));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.AllocateFFSlaveModules" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AllocateFFSlaveModulesCommand => allocateFFSlaveModulesCommand ?? (allocateFFSlaveModulesCommand = new AsyncRelayCommand(AllocateFFSlaveModules));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.Add" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddCommand => addCommand ?? (addCommand = new AsyncRelayCommand(Add));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.Clear" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ClearCommand => clearCommand ?? (clearCommand = new AsyncRelayCommand(Clear));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.Edit(IODataPlatform.Models.ExcelModels.IoFullData)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<IoFullData> EditCommand => editCommand ?? (editCommand = new AsyncRelayCommand<IoFullData>(Edit));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.Delete(IODataPlatform.Models.ExcelModels.IoFullData)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<IoFullData> DeleteCommand => deleteCommand ?? (deleteCommand = new AsyncRelayCommand<IoFullData>(Delete));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.Import" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ImportCommand => importCommand ?? (importCommand = new RelayCommand(Import));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.Refresh" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new AsyncRelayCommand(Refresh));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.DynamicExport" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand DynamicExportCommand => dynamicExportCommand ?? (dynamicExportCommand = new AsyncRelayCommand(DynamicExport));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.Export(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<string> ExportCommand => exportCommand ?? (exportCommand = new AsyncRelayCommand<string>(Export));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.ClearAllFilterOptions" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ClearAllFilterOptionsCommand => clearAllFilterOptionsCommand ?? (clearAllFilterOptionsCommand = new AsyncRelayCommand(ClearAllFilterOptions));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.FilterAndSort" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand FilterAndSortCommand => filterAndSortCommand ?? (filterAndSortCommand = new RelayCommand(FilterAndSort));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.RefreshAll" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshAllCommand => refreshAllCommand ?? (refreshAllCommand = new AsyncRelayCommand(RefreshAll));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.Publish" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand PublishCommand => publishCommand ?? (publishCommand = new AsyncRelayCommand(Publish));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.GeneralSorting" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand GeneralSortingCommand => generalSortingCommand ?? (generalSortingCommand = new RelayCommand(GeneralSorting));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.ValvePositionSorting" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ValvePositionSortingCommand => valvePositionSortingCommand ?? (valvePositionSortingCommand = new RelayCommand(ValvePositionSorting));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.DepXT2ViewModel.ValveSorting" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ValveSortingCommand => valveSortingCommand ?? (valveSortingCommand = new RelayCommand(ValveSorting));

	/// <summary>
	/// 重新计算命令方法
	/// 触发龙鳍控制系统的IO数据重新计算，支持全量计算或指定机柜的部分计算
	/// 适用于数据导入后的批量处理、配置变更后的重新生成、问题排查时的单点验证
	/// </summary>
	/// <param name="cabinetName">指定要计算的机柜名称，为null时计算所有机柜的数据</param>
	/// <returns>异步任务，表示计算操作的完成</returns>
	/// <exception cref="T:System.Exception">当没有可计算的数据时抛出异常</exception>
	[RelayCommand]
	public async Task Recalc(string cabinetName = null)
	{
		if (SubProject == null)
		{
			throw new Exception("子项目为空，找不到控制系统");
		}
		string value = (UseFormula ? "固定算法" : "公式编辑器");
		string calculatedFieldsList = GetCalculatedFieldsList();
		string value2 = ((cabinetName == null) ? "所有数据" : ("机柜 [" + cabinetName + "]"));
		string message = $"当前使用{value}模式\n计算范围：{value2}\n\n将计算以下列：\n{calculatedFieldsList}\n\n是否继续？";
		if (await message.ConfirmAsync(message))
		{
			ControlSystem controlSystem = (from it in context.Db.Queryable<config_project_major>()
				where it.Id == SubProject.MajorId
				select it).First().ControlSystem;
			await RecalcMethod(controlSystem, cabinetName);
		}
	}

	/// <summary>
	/// 获取将要计算的字段列表
	/// </summary>
	/// <returns>返回格式化的字段列表字符串</returns>
	private string GetCalculatedFieldsList()
	{
		List<string> values = new List<string>
		{
			"• OF显示格式", "• 点名 (TagName)", "• IO卡型号", "• 供电方式", "• 端子板型号", "• 柜内板卡编号", "• 板卡编号", "• 端子板编号", "• 板卡地址", "• 信号正极",
			"• 信号负极", "• RTD补偿C端", "• RTD补偿E端", "• 站号"
		};
		return string.Join("\n", values);
	}

	/// <summary>
	/// 核心重新计算方法
	/// 执行指定控制系统的IO数据重新计算，包括板卡类型识别、端子板配置、信号映射、供电方式判断等完整的IO配置计算流程
	/// 支持基于公式引擎和硬编码两种计算模式，能够处理模拟量、数字量、现场总线等多种信号类型
	/// 计算完成后自动保存结果并上传至服务器，同时更新界面显示状态
	/// 适用场景：工程设计阶段的IO配置生成、设备变更后的重新计算、调试阶段的配置验证
	/// </summary>
	/// <param name="controlSystem">目标控制系统类型，决定使用的计算规则和配置参数</param>
	/// <param name="cabinetName">指定要计算的机柜名称，为null时计算所有机柜数据，用于批量处理或单机柜调试</param>
	/// <returns>异步任务，表示计算操作的完成状态</returns>
	/// <exception cref="T:System.Exception">当AllData为null（无数据可计算）时抛出异常</exception>
	/// <exception cref="T:System.InvalidOperationException">当公式计算过程中发生错误时抛出异常</exception>
	/// <exception cref="!:DatabaseException">当数据库查询失败或数据不一致时抛出异常</exception>
	public async Task RecalcMethod(ControlSystem controlSystem, string cabinetName = null)
	{
		if (AllData == null)
		{
			throw new Exception("无数据可计算");
		}
		model.Status.Busy("正在重新计算……");
		List<formular> formulars = (UseFormula ? new List<formular>() : context.Db.Queryable<formular>().ToList());
		List<formular_Index> formularIndexes = (UseFormula ? new List<formular_Index>() : context.Db.Queryable<formular_Index>().ToList());
		List<formular_index_condition> formulaIndexConditions = (UseFormula ? new List<formular_index_condition>() : context.Db.Queryable<formular_index_condition>().ToList());
		FormulaBuilder formulaBuilder = new FormulaBuilder();
		DisplayInfo displayInfo = DisplayAttributeHelper.GetDisplayInfo<IoFullData>();
		new IoFullData();
		string controlSystemField = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames["CardType"]);
		string controlSystemField2 = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames["PowerSupplyMethod"]);
		string controlSystemField3 = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames["TerminalBoardModel"]);
		string controlSystemField4 = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames["SignalPlus"]);
		string controlSystemField5 = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames["SignalMinus"]);
		string controlSystemField6 = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames["RTDCompensationC"]);
		string controlSystemField7 = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames["RTDCompensationE"]);
		string controlSystemField8 = DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames["SignalEffectiveMode"]);
		DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames["FFSlaveModuleID"]);
		DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames["FFSlaveModuleSignalPositive"]);
		DataConverter.GetControlSystemField(context.Db, controlSystem, displayInfo.FieldDisplayNames["FFSlaveModuleSignalNegative"]);
		IEnumerable<IoFullData> enumerable;
		if (cabinetName != null)
		{
			enumerable = AllData.Where((IoFullData point) => point.CabinetNumber == cabinetName);
		}
		else
		{
			IEnumerable<IoFullData> enumerable2 = AllData;
			enumerable = enumerable2;
		}
		IEnumerable<IoFullData> enumerable3 = enumerable;
		if (enumerable3 == null)
		{
			throw new Exception();
		}
		foreach (IoFullData item in enumerable3)
		{
			if (UseFormula)
			{
				item.SignalEffectiveMode = formulaBuilder.FindMatchingFormulaReturnValueAsyncByExpressionNew(item, controlSystem, controlSystemField8, formulars, formularIndexes, formulaIndexConditions);
			}
			item.OFDisplayFormat = GetOfDisplayFormat(item.RangeUpperLimit, item.RangeLowerLimit);
			item.TagName = GetTagName(item.SignalPositionNumber, item.ExtensionCode);
			item.CardType = ((!UseFormula) ? GetIoCardTypeFormula(item) : formulaBuilder.FindMatchingFormulaReturnValueAsyncByExpression(item, controlSystem, controlSystemField, formulars, formularIndexes, formulaIndexConditions));
			item.PowerSupplyMethod = ((!UseFormula) ? GetPowerSupplyMethodFormula(item) : formulaBuilder.FindMatchingFormulaReturnValueAsyncByExpression(item, controlSystem, controlSystemField2, formulars, formularIndexes, formulaIndexConditions));
			item.TerminalBoardModel = ((!UseFormula) ? GetTerminalBoxTypeFormula(item.CardType, item.PowerSupplyMethod, item.ElectricalCharacteristics) : formulaBuilder.FindMatchingFormulaReturnValueAsyncByExpression(item, ControlSystem.龙鳍, controlSystemField3, formulars, formularIndexes, formulaIndexConditions));
			item.CardNumberInCabinet = GetCardInCabinetNumber(item.Cage, item.Slot, item.CardType);
			item.CardNumber = GetCardNumber(item.CabinetNumber, item.CardNumberInCabinet, item.CardType);
			item.TerminalBoardNumber = GetTerminalBoardNumber(item.TerminalBoardModel, item.CardType, item.Cage, item.Slot, item.Channel);
			item.CardAddress = GetCardAddress(item.Cage, item.Slot, item.CardType);
			item.SignalPlus = ((!UseFormula) ? GetSingalPlusFormula(item) : formulaBuilder.FindMatchingFormulaReturnValueAsyncByExpressionNew(item, controlSystem, controlSystemField4, formulars, formularIndexes, formulaIndexConditions));
			item.SignalMinus = ((!UseFormula) ? GetSingalMinusFormula(item) : formulaBuilder.FindMatchingFormulaReturnValueAsyncByExpressionNew(item, controlSystem, controlSystemField5, formulars, formularIndexes, formulaIndexConditions));
			item.RTDCompensationC = ((!UseFormula) ? GetRTDCompensationEndC(item) : formulaBuilder.FindMatchingFormulaReturnValueAsyncByExpressionNew(item, controlSystem, controlSystemField6, formulars, formularIndexes, formulaIndexConditions));
			item.RTDCompensationE = ((!UseFormula) ? GetRTDCompensationEndD(item.PowerSupplyMethod, item.Channel) : formulaBuilder.FindMatchingFormulaReturnValueAsyncByExpressionNew(item, controlSystem, controlSystemField7, formulars, formularIndexes, formulaIndexConditions));
		}
		SetSTationNumber();
		await SaveAndUploadFileAsync();
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData allDatum in AllData)
		{
			observableCollection.Add(allDatum);
		}
		AllData = observableCollection;
		model.Status.Success("重新计算完毕");
	}

	/// <summary>
	/// 设置站号方法
	/// 为每个机柜分配唯一的站号，用于现场总线通信的设备地址识别
	/// 站号从2开始递增，每个机柜中的所有IO点使用相同的站号
	/// 适用于PROFIBUS-DP、FF等现场总线系统的设备地址配置
	/// 业务场景：系统集成阶段的站号规划、现场调试时的地址对照、运维阶段的设备管理
	/// </summary>
	/// <remarks>
	/// 站号分配规则：
	/// - 起始地址：2（保留地址0和1为系统使用）
	/// - 分配方式：按机柜编号顺序递增
	/// - 地址范围：2-125（PROFIBUS协议限制）
	/// </remarks>
	private void SetSTationNumber()
	{
		int num = 2;
		IEnumerable<IGrouping<string, IoFullData>> enumerable = from a in AllData
			group a by a.CabinetNumber;
		foreach (IGrouping<string, IoFullData> item in enumerable)
		{
			foreach (IoFullData item2 in item.ToList())
			{
				item2.StationNumber = num.ToString();
			}
			num++;
		}
	}

	/// <summary>
	/// 获取字段显示名称的反射工具方法
	/// 通过反射机制获取指定字段的DisplayAttribute显示名称，用于动态构建界面显示和数据映射
	/// 适用于多语言支持、动态表单生成、配置化界面等场景
	/// </summary>
	/// <typeparam name="T">目标类型，通常为数据模型类</typeparam>
	/// <param name="fieldName">字段名称，必须与类中的属性名一致</param>
	/// <returns>返回DisplayAttribute中定义的显示名称，如果没有定义则返回字段名本身</returns>
	/// <exception cref="T:System.ArgumentException">当字段名不存在时可能引发反射异常</exception>
	public static string GetFieldDisplayName<T>(string fieldName)
	{
		Type typeFromHandle = typeof(T);
		PropertyInfo property = typeFromHandle.GetProperty(fieldName);
		if (property != null)
		{
			DisplayAttribute customAttribute = property.GetCustomAttribute<DisplayAttribute>();
			if (customAttribute != null)
			{
				return customAttribute.Name;
			}
		}
		return fieldName;
	}

	/// <summary>
	/// 龙鳍系统供电方式计算方法
	/// 根据新的复杂公式计算供电方式，支持多种协议检测和类型判断
	/// 支持AI、AO、DI、DO、PI、FF、DP等多种信号类型的供电判断，精确区分DCS供电和现场供电
	/// 适用于工程设计阶段的供电方案选型、现场施工阶段的接线指导、调试阶段的问题排查
	/// </summary>
	/// <param name="x">包含供电相关信息的IO数据实体，包括描述、供电类型等关键字段</param>
	/// <returns>返回格式化的供电方式字符串，如"DCS"、"USER提供24VDC"等，"--"表示不需要供电，"Err"表示计算错误</returns>
	/// <exception cref="T:System.Exception">当供电类型参数不正确或无法识别时抛出异常</exception>
	/// <remarks>
	/// 新公式逻辑：
	/// =IF(OR(IFERROR(SEARCH("TCP",AG4),-1)&gt;0,IFERROR(SEARCH("RTU",AG4),-1)&gt;0),"--",
	///   IF(LEFT(AI4,2)="AI",CHOOSE(MID(AI4,3,IF(IFERROR(SEARCH("NH",AI4),-1)&gt;0,"1",LEN(AI4)-1)),"DCS","DCS(提供24VDC)","DCS(提供220VAC)","USER","USER","DCS(提供220VAC)","DCS","DCS","DCS","DCS"),
	///     IF(LEFT(AI4,2)="AO","DCS",
	///       IF(LEFT(AI4,1)="P",CHOOSE(MID(AI4,2,LEN(AI4)-1),"DCS(提供24VDC)","DCS(提供220VAC)","USER"),
	///         IF(LEFT(AI4,2)="DI",CHOOSE(MID(AI4,3,LEN(AI4)-1),"DCS","DCS(提供24VDC)","DCS(提供220VAC)","USER","USER","DCS(提供24VDC)"),
	///           IF(LEFT(AI4,2)="DO",CHOOSE(MID(AI4,3,LEN(AI4)-1),"DCS","DCS(提供24VDC)","DCS(提供220VAC)","USER","USER"),
	///             IF(LEFT(AI4,2)="FF",CHOOSE(MID(AI4,3,LEN(AI4)-1),"DCS","DCS(提供24VDC)","DCS(提供220VAC)","USER","USER","DCS","DCS","DCS"),
	///               IF(LEFT(AI4,2)="DP","DCS(提供220VAC)",
	///                 IF(IFERROR(SEARCH("DP",AG4),-1)&gt;0,"--",
	///                   IF(IFERROR(SEARCH("OPC",AG4),-1)&gt;0,"--","Err")))))))))
	/// 供电方式编码规则：
	/// - TCP/RTU协议：根据描述中的关键字返回"--"
	/// - AI类型：根据编号选择不同供电方式，特殊处理NH格式
	/// - AO类型：统一返回"DCS"
	/// - P类型：根据编号选择供电方式
	/// - DI/DO类型：根据编号区分供电方案
	/// - FF类型：现场总线供电处理
	/// - DP类型：统一使用220VAC供电
	/// - 其他协议：DP/OPC根据描述判断
	/// </remarks>
	public string GetPowerSupplyMethodFormula(IoFullData x)
	{
		try
		{
			string text = x.Description ?? "";
			string text2 = x.PowerType ?? "";
			if (text.Contains("TCP") || text.Contains("RTU"))
			{
				return "--";
			}
			if (text2.Length >= 2)
			{
				string text3 = text2.Substring(0, 2);
				string text4 = text2.Substring(0, 1);
				if (text3 == "AI")
				{
					int num = (text2.Contains("NH") ? 1 : (text2.Length - 2));
					if (num > 0 && text2.Length >= 3)
					{
						return text2.Substring(2, Math.Min(num, text2.Length - 2)) switch
						{
							"1" => "DCS", 
							"2" => "DCS(提供24VDC)", 
							"3" => "DCS(提供220VAC)", 
							"4" => "USER", 
							"5" => "USER", 
							"6" => "DCS(提供220VAC)", 
							"7" => "DCS", 
							"8" => "DCS", 
							"9" => "DCS", 
							_ => "DCS", 
						};
					}
					return "DCS";
				}
				if (text3 == "AO")
				{
					return "DCS";
				}
				if (text4 == "P" && text2.Length >= 2)
				{
					return text2.Substring(1) switch
					{
						"1" => "DCS(提供24VDC)", 
						"2" => "DCS(提供220VAC)", 
						"3" => "USER", 
						_ => "DCS(提供24VDC)", 
					};
				}
				if (text3 == "DI" && text2.Length >= 3)
				{
					return text2.Substring(2) switch
					{
						"1" => "DCS", 
						"2" => "DCS(提供24VDC)", 
						"3" => "DCS(提供220VAC)", 
						"4" => "USER", 
						"5" => "USER", 
						"6" => "DCS(提供24VDC)", 
						_ => "DCS", 
					};
				}
				if (text3 == "DO" && text2.Length >= 3)
				{
					return text2.Substring(2) switch
					{
						"1" => "DCS", 
						"2" => "DCS(提供24VDC)", 
						"3" => "DCS(提供220VAC)", 
						"4" => "USER", 
						"5" => "USER", 
						_ => "DCS", 
					};
				}
				if (text3 == "FF" && text2.Length >= 3)
				{
					return text2.Substring(2) switch
					{
						"1" => "DCS", 
						"2" => "DCS(提供24VDC)", 
						"3" => "DCS(提供220VAC)", 
						"4" => "USER", 
						"5" => "USER", 
						"6" => "DCS", 
						"7" => "DCS", 
						"8" => "DCS", 
						_ => "DCS", 
					};
				}
				if (text3 == "DP")
				{
					return "DCS(提供220VAC)";
				}
			}
			if (text.Contains("DP"))
			{
				return "--";
			}
			if (text.Contains("OPC"))
			{
				return "--";
			}
			return "Err";
		}
		catch (Exception ex)
		{
			throw new Exception("供电类型计算错误：" + ex.Message);
		}
	}

	/// <summary>
	/// 端子板类型计算方法（新版Excel公式实现）
	/// 根据板卡类型、供电方式和电气特性等参数，自动选择合适的端子板型号
	/// 支持龙鳍系统所有板卡类型的端子板选型，确保信号、供电和安全隔离的匹配性
	/// 适用于工程设计阶段的端子板选型、采购阶段的材料统计、现场安装阶段的型号核对
	/// </summary>
	/// <param name="boardType">板卡类型，如"AI216"、"DI211"、"FF211"等，决定基本的端子板选型方向</param>
	/// <param name="powerSupply">供电方式，如"DCS"、"USER"等，影响端子板的隔离设计</param>
	/// <param name="electricalCharacteristics">电气特性，如"4~20mA"、"PT100"等，决定信号调理方式</param>
	/// <returns>返回端子板型号，如"TB241"、"TB271"等，"--"表示不需要端子板，"ERR"表示选型失败</returns>
	/// <remarks>
	/// 新公式逻辑：
	/// =IF(I3="DI211",IF(LEFT(AI3,4)="USER","TB221","TB222"),
	///   IF(I3="DO211",IF(LEFT(AI3,4)="USER","TB231","TB233"),
	///     IF(OR(I3="AI216",I3="AI212"),"TB241",
	///       IF(I3="AI221","TB242",
	///         IF(I3="AI232","TB246",
	///           IF(I3="AO211","TB251",
	///             IF(OR(I3="PI211",I3="AO215",I3="MD211"),"TB244",
	///               IF(I3="MD216","--",
	///                 IF(I3="FF211","TB271",
	///                   IF(I3="DP211","TB272",
	///                     IF(I3="--","--","ERR"))))))))))
	///
	/// 端子板选型规则：
	/// - DI211：根据供电方式选择TB221(USER)或TB222(DCS)
	/// - DO211：根据供电方式选择TB231(USER)或TB233(DCS)
	/// - AI216/AI212：使用TB241（4-20mA信号调理）
	/// - AI221：使用TB242（热电偶信号调理）
	/// - AI232：使用TB246（热电阻信号调理）
	/// - AO211：使用TB251（模拟量输出）
	/// - PI211/AO215/MD211：使用TB244（多功能端子板）
	/// - MD216：不需要端子板，返回"--"
	/// - FF211：使用TB271（FF总线端子板）
	/// - DP211：使用TB272（DP总线端子板）
	/// - "--"：无板卡，返回"--"
	/// - 其他：返回"ERR"表示选型错误
	/// </remarks>
	public string GetTerminalBoxTypeFormula(string boardType, string powerSupply, string electricalCharacteristics)
	{
		switch (boardType)
		{
		case "DI211":
			if (powerSupply.Length >= 4 && powerSupply.Substring(0, 4) == "USER")
			{
				return "TB221";
			}
			return "TB222";
		case "DO211":
			if (powerSupply.Length >= 4 && powerSupply.Substring(0, 4) == "USER")
			{
				return "TB231";
			}
			return "TB233";
		case "AI216":
		case "AI212":
			return "TB241";
		case "AI221":
			return "TB242";
		case "AI232":
			return "TB246";
		case "AO211":
			return "TB251";
		case "PI211":
		case "AO215":
		case "MD211":
			return "TB244";
		case "MD216":
			return "--";
		case "FF211":
			return "TB271";
		case "DP211":
			return "TB272";
		case "--":
			return "--";
		default:
			return "ERR";
		}
	}

	/// <summary>
	/// IO板卡类型计算方法（新版Excel公式实现）
	/// 根据新的Excel公式逻辑进行板卡类型识别，优先检测描述字段中的协议关键字
	/// 支持龙鳍系统全系列板卡的自动识别，包括模拟量、数字量、现场总线、串口通信等类型
	/// 适用于工程设计阶段的板卡配置、采购阶段的材料统计、现场安装阶段的板卡核对
	/// </summary>
	/// <param name="data">包含板卡选型所需全部信息的IO数据实体，包括描述、IO类型、供电类型、抗震类别等</param>
	/// <returns>返回板卡型号，如"AI216"、"DI211"、"FF211"等，"--"表示OPC协议，"未知板卡"表示无法识别</returns>
	/// <remarks>
	/// 新公式逻辑（按优先级从高到低）：
	/// 1. TCP协议检测：描述中包含"TCP" → MD216
	/// 2. RTU协议检测：描述中包含"RTU" → MD211
	/// 3. DP/PROFIBUS检测：描述中包含"DP"或"PROFIBUS" → DP211
	/// 4. FF检测：描述中包含"FF" → FF211
	/// 5. AI类型细分：
	///    - AI+供电类型含"NH" → AI212（计量泵等）
	///    - AI+描述含"4*20mA" → AI216（标准4-20mA）
	///    - AI+描述含"TC" → AI221（热电偶）
	///    - AI+描述含"PT" → AI232（热电阻）
	/// 6. AO类型细分：
	///    - AO+供电类型为AO1或AO2 → AO211
	///    - AO+供电类型为AOH → AO215
	/// 7. PI类型：IO类型为PI或抗震类别为P → PI211
	/// 8. DI类型：IO类型为DI → DI211
	/// 9. DO类型：IO类型为DO → DO211
	/// 10. OPC协议：描述中包含"OPC" → "--"
	/// 11. 其他情况 → "未知板卡"
	/// </remarks>
	public string GetIoCardTypeFormula(IoFullData data)
	{
		string text = data.Description ?? "";
		string text2 = data.IoType?.Trim() ?? "";
		string text3 = data.PowerType?.Trim() ?? "";
		string text4 = data.SeismicCategory?.Trim() ?? "";
		if (text.IndexOf("TCP", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return "MD216";
		}
		if (text.IndexOf("RTU", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return "MD211";
		}
		if (text.IndexOf("DP", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("PROFIBUS", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return "DP211";
		}
		if (text.IndexOf("FF", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return "FF211";
		}
		switch (text2)
		{
		case "AI":
			if (text3.IndexOf("NH", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "AI212";
			}
			if (text.IndexOf("4*20mA", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "AI216";
			}
			if (text.IndexOf("TC", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "AI221";
			}
			if (text.IndexOf("PT", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return "AI232";
			}
			return "未知AI板卡";
		case "AO":
			switch (text3)
			{
			case "AO1":
			case "AO2":
				return "AO211";
			case "AOH":
				return "AO215";
			default:
				return "未知AO板卡";
			}
		default:
			if (!(text4 == "P"))
			{
				if (text2 == "DI")
				{
					return "DI211";
				}
				if (text2 == "DO")
				{
					return "DO211";
				}
				if (text.IndexOf("OPC", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return "--";
				}
				return "未知板卡";
			}
			goto case "PI";
		case "PI":
			return "PI211";
		}
	}

	public string GetOfDisplayFormat(string upperLimitStr, string lowerLimitStr, string engineeringUnit = "")
	{
		if (string.IsNullOrEmpty(upperLimitStr))
		{
			return "";
		}
		if (engineeringUnit == "r/min")
		{
			return "0";
		}
		if (double.TryParse(upperLimitStr, out var result) && double.TryParse(lowerLimitStr, out var result2))
		{
			double num = Math.Abs(result - result2);
			if (num <= 10.0)
			{
				return "3";
			}
			if (num > 10.0 && num <= 100.0)
			{
				return "2";
			}
			if (num > 100.0)
			{
				return "1";
			}
			return "";
		}
		return "";
	}

	public string GetOfDisplayFormat(List<config_output_format_values> query, IoFullData data)
	{
		int.TryParse(data.RangeLowerLimit, out var low);
		int.TryParse(data.RangeUpperLimit, out var high);
		query = ((!string.IsNullOrEmpty(data.EngineeringUnit)) ? query.Where((config_output_format_values r) => r.RangeUnit == data.EngineeringUnit).ToList() : query.Where((config_output_format_values r) => r.RangeUnit == "").ToList());
		if (query != null)
		{
			query = query.Where((config_output_format_values f) => f.RangeLow < (double)low).ToList();
		}
		if (query != null)
		{
			query = query.Where((config_output_format_values f) => f.RangeHigh >= (double)high).ToList();
		}
		if (query != null && query.Count == 1)
		{
			return query.ToList().FirstOrDefault()?.DecimalPlaces ?? "";
		}
		return "";
	}

	/// <summary>
	/// 基于配置表的IO板卡类型判断方法
	/// 通过数据库配置表进行IO板卡类型的智能匹配，支持多条件组合判断
	/// 提供比硬编码方法更灵活的配置化板卡选型，便于后期维护和扩展
	/// 适用于标准化的板卡选型规则管理、产品线扩展的兼容支持
	/// </summary>
	/// <param name="list">板卡类型判断配置列表，包含IO类型、信号规格、供电类型等判断条件</param>
	/// <param name="data">IO数据实体，包含板卡选型所需的所有判断参数</param>
	/// <returns>返回匹配的IO板卡类型，如无匹配返回"未知板卡"</returns>
	/// <remarks>
	/// 匹配条件（所有条件必须同时满足）：
	/// - IO类型匹配或配置为空（兼容所有类型）
	/// - 信号规格匹配或配置为空（支持分号分隔的多值匹配）
	/// - 供电类型精确匹配或配置为空
	/// </remarks>
	public string GetIoCardType(List<config_card_type_judge> list, IoFullData data)
	{
		return (from x in list
			where string.IsNullOrEmpty(x.IoType) || x.IoType == data.IoType
			where string.IsNullOrEmpty(x.SignalSpec) || ((ReadOnlySpan<string>)x.SignalSpec.Split(";")).Contains(data.ElectricalCharacteristics)
			where string.IsNullOrEmpty(x.PowerType) || x.PowerType == data.PowerType
			select x).FirstOrDefault()?.IoCardType ?? "未知板卡";
	}

	/// <summary>
	/// 基于配置表的端子板类型判断方法
	/// 通过数据库配置表进行端子板类型的智能匹配，支持板卡类型、信号类型、信号规格的组合判断
	/// 提供灵活的配置化端子板选型，确保与板卡和信号特性的完美匹配
	/// 适用于标准化端子板选型规则、产品系列化管理、现场安装指导
	/// </summary>
	/// <param name="list">端子板类型判断配置列表，包含板卡类型、信号类型、信号规格等匹配条件</param>
	/// <param name="data">IO数据实体，包含端子板选型所需的板卡类型、供电方式、电气特性等信息</param>
	/// <returns>返回匹配的端子板型号，如无匹配返回"ERR"</returns>
	/// <remarks>
	/// 匹配逻辑：
	/// - 板卡类型：精确匹配或配置为空
	/// - 信号类型：供电方式前缀匹配或配置为空  
	/// - 信号规格：电气特性精确匹配或配置为空
	/// 所有条件必须同时满足才能匹配成功
	/// </remarks>
	public string GetTerminalBoxType(List<config_terminalboard_type_judge> list, IoFullData data)
	{
		return (from x in list
			where string.IsNullOrEmpty(x.CardType) || x.CardType == data.CardType
			where string.IsNullOrEmpty(x.SignalType) || data.PowerSupplyMethod.StartsWith(x.SignalType)
			where string.IsNullOrEmpty(x.SignalSpec) || data.ElectricalCharacteristics == x.SignalSpec
			select x).FirstOrDefault()?.TerminalBlock ?? "ERR";
	}

	/// <summary>
	/// 基于配置表的供电方式判断方法
	/// 通过数据库配置表进行供电方式的智能匹配，支持供电类型、板卡类型、传感器类型的组合判断
	/// 提供标准化的供电方式选择逻辑，确保供电配置的准确性和一致性
	/// 适用于供电方案的标准化管理、安全供电设计、设备兼容性验证
	/// </summary>
	/// <param name="list">供电方式配置列表，包含供电类型、板卡类型、传感器类型等匹配条件</param>
	/// <param name="data">IO数据实体，包含供电方式判断所需的供电方式、板卡类型、传感器类型等信息</param>
	/// <returns>返回匹配的供电模式，如无匹配返回"ERR"</returns>
	/// <remarks>
	/// 匹配条件：
	/// - 供电类型：与数据供电方式精确匹配或配置为空
	/// - 板卡类型：与数据板卡类型精确匹配或配置为空
	/// - 传感器类型：与数据传感器类型精确匹配或配置为空
	/// 注意：代码中第二个板卡类型判断可能存在逻辑错误（data.CardType == data.CardType）
	/// </remarks>
	public string GetPowerSupplyMethod(List<config_power_supply_method> list, IoFullData data)
	{
		return (from x in list
			where string.IsNullOrEmpty(x.SupplyType) || x.SupplyType == data.PowerSupplyMethod
			where string.IsNullOrEmpty(x.CardType) || data.CardType == data.CardType
			where string.IsNullOrEmpty(x.SensorType) || data.SensorType == x.SensorType
			select x).FirstOrDefault()?.SupplyModel ?? "ERR";
	}

	public string GetSignalPositionNumber(string SignalPositionNumber)
	{
		if (string.IsNullOrEmpty(SignalPositionNumber))
		{
			return "";
		}
		return SignalPositionNumber.Replace("-", "");
	}

	/// <summary>
	/// 生成标准化IO点名（TagName）
	/// 根据原始变量名和扩展码生成符合龙鳍系统规范的IO点名格式
	/// 采用"R"+变量名+"_"+扩展码的命名规则，确保点名的唯一性和可识别性
	/// 适用于DCS组态、监控画面开发、数据库建表、报警配置等场景
	/// </summary>
	/// <param name="oldVarName">原始变量名，通常为信号位号或设备标识</param>
	/// <param name="oldExtCode">扩展码，用于区分同一设备的不同信号或参数</param>
	/// <returns>返回格式化的IO点名，格式为"R{变量名}[_{扩展码}]"</returns>
	/// <remarks>
	/// 命名规则：
	/// - 前缀：固定添加"R"标识符
	/// - 变量名处理：去除空格和连字符
	/// - 扩展码：非空时添加下划线分隔符
	/// - 示例：R010001_PV, R010002（无扩展码）
	/// </remarks>
	public string GetTagName(string oldVarName, string oldExtCode)
	{
		string text = "R" + oldVarName.Trim().Replace("-", "");
		if (!string.IsNullOrEmpty(oldExtCode))
		{
			text = text + "_" + oldExtCode;
		}
		return text;
	}

	public string GetCardInCabinetNumber(int cage, int slot, string IOCardType)
	{
		if (IOCardType == "--")
		{
			return "--";
		}
		if (string.IsNullOrEmpty(IOCardType))
		{
			return "";
		}
		string value = $"{slot:00}";
		string value2 = ((IOCardType == "AI232") ? "RTD" : ((!(IOCardType == "AI221")) ? ((IOCardType.Length >= 2) ? IOCardType.Substring(0, 2) : IOCardType) : "TC"));
		return $"{cage}{value}{value2}";
	}

	public string GetCardNumber(string cabinetNumber, string cardNumberInCabinet, string cardType)
	{
		if (cardType == "--")
		{
			return "--";
		}
		if (string.IsNullOrEmpty(cabinetNumber) || string.IsNullOrEmpty(cardNumberInCabinet))
		{
			return "";
		}
		return cabinetNumber + cardNumberInCabinet;
	}

	public string GetTerminalBoardNumber(string terminalBoardModel, string cardType, int cage, int slot, int channel)
	{
		switch (cardType)
		{
		case "FF211":
			return $"{cage}{slot:00}_{channel}FI";
		case "MD216":
		case "--":
			return "--";
		default:
			return $"{cage}{slot:00}BN";
		}
	}

	/// <summary>
	/// 计算板卡系统地址（新版Excel公式实现）
	/// 根据笼号和槽位号计算板卡在龙鳍系统中的唯一地址编号
	/// 采用标准地址映射算法确保地址空间的合理分配和系统扩展性
	/// 适用于系统组态、通信配置、故障诊断、地址规划
	/// </summary>
	/// <param name="cage">笼号，从1开始编号</param>
	/// <param name="slot">槽位号，从1开始编号</param>
	/// <param name="cardType">板卡类型，用于判断是否为特殊值"--"</param>
	/// <returns>返回计算得到的板卡系统地址，板卡类型为"--"时返回"--"字符串</returns>
	/// <remarks>
	/// 新公式逻辑：=IF(I3="--","--",(L3-1)*10+M3)
	/// 地址计算公式：(笼号-1) × 10 + 槽位号
	/// 地址分配示例：
	/// - 笼1槽1 → 地址1
	/// - 笼1槽10 → 地址10  
	/// - 笼2槽1 → 地址11
	/// - 笼3槽5 → 地址25
	/// - 板卡类型"--" → "--"
	/// 每个笼架支持最多10个槽位（1-10）
	/// </remarks>
	public string GetCardAddress(int cage, int slot, string cardType)
	{
		if (cardType == "--")
		{
			return "--";
		}
		return ((cage - 1) * 10 + slot).ToString();
	}

	public string GetSingalPlusFormula(IoFullData data)
	{
		string text = data.TerminalBoardModel ?? "";
		int channel = data.Channel;
		string text2 = data.CardType ?? "";
		string text3 = data.SignalEffectiveMode ?? "";
		string text4 = data.PowerSupplyMethod ?? "";
		switch (text)
		{
		case "TB221":
		case "TB222":
		case "TB242":
		case "TB243":
		case "TB246":
			return $"{channel}A";
		case "TB231":
		case "TB232":
		case "TB233":
			if (string.IsNullOrEmpty(text3) || text3 == "NO")
			{
				return $"{channel}B";
			}
			break;
		}
		switch (text)
		{
		case "TB231":
		case "TB232":
		case "TB233":
			if (text3 == "NC")
			{
				return $"{channel}A";
			}
			break;
		}
		if (text == "TB241" && (text2 == "AI212" || text2 == "AI216"))
		{
			if (text4.Length >= 3)
			{
				string value = text4[2] switch
				{
					'1' => "B", 
					'2' => "A", 
					'3' => "A", 
					'4' => "A", 
					'5' => "A", 
					'6' => "B", 
					_ => "A", 
				};
				return $"{channel}{value}";
			}
			return $"{channel}A";
		}
		if (text == "TB244" && text2 == "AO215")
		{
			return (channel * 2 + 22).ToString();
		}
		if (text == "TB244" && text2 == "PI211")
		{
			return ((channel - 1) * 4 + 2).ToString();
		}
		if (text == "TB244" && text2 == "MD211")
		{
			return channel switch
			{
				1 => "8", 
				2 => "28", 
				_ => "8", 
			};
		}
		if (text == "TB271")
		{
			return "a";
		}
		switch (text)
		{
		case "TB272":
		case "TB271":
			return "--";
		case "TB251":
			return $"{channel}B";
		case "--":
			return "--";
		default:
			return "Err";
		}
	}

	public string GetRTDCompensationEndC(IoFullData data)
	{
		string text = data.TerminalBoardModel ?? "";
		string text2 = data.PowerSupplyMethod ?? "";
		int channel = data.Channel;
		string text3 = data.CardType ?? "";
		if (text == "TB271")
		{
			return "IE-BUS";
		}
		switch (text2)
		{
		case "AI7":
		case "AI8":
		case "AI9":
			return $"{channel}C";
		default:
			if (text == "TB244" && text3 == "MD211")
			{
				return channel switch
				{
					1 => "GND:10", 
					2 => "GND:30", 
					_ => "GND:10", 
				};
			}
			return "--";
		}
	}

	public string GetSingalMinusFormula(IoFullData data)
	{
		string text = data.TerminalBoardModel ?? "";
		int channel = data.Channel;
		string text2 = data.CardType ?? "";
		string text3 = data.PowerSupplyMethod ?? "";
		switch (text)
		{
		case "TB221":
		case "TB222":
		case "TB242":
		case "TB246":
			return $"{channel}B";
		case "TB231":
		case "TB233":
		case "TB251":
			return $"{channel}C";
		case "TB241":
			if (text2 == "AI212" || text2 == "AI216")
			{
				if (text3.Length >= 3)
				{
					string value = text3[2] switch
					{
						'1' => "A", 
						'2' => "C", 
						'3' => "C", 
						'4' => "C", 
						'5' => "C", 
						'6' => "A", 
						_ => "C", 
					};
					return $"{channel}{value}";
				}
				return $"{channel}C";
			}
			break;
		}
		if (text == "TB244" && text2 == "AO215")
		{
			return (channel * 2 + 19).ToString();
		}
		if (text == "TB244" && text2 == "PI211")
		{
			return (channel * 4).ToString();
		}
		if (text == "TB244" && text2 == "MD211")
		{
			return channel switch
			{
				1 => "12", 
				2 => "32", 
				_ => "12", 
			};
		}
		if (text == "TB271")
		{
			return "b";
		}
		switch (text)
		{
		case "TB272":
		case "TB271":
			return "--";
		case "--":
			return "--";
		default:
			return "Err";
		}
	}

	/// <summary>
	/// 异步获取信号正极连接点编号（基于配置表）
	/// 通过数据库配置表和C#脚本动态计算信号正极连接点，支持复杂的计算逻辑
	/// 提供比硬编码方法更灵活的配置化连接点计算，便于规则变更和系统扩展
	/// 适用于标准化连接点规则管理、复杂计算逻辑实现、产品化配置
	/// </summary>
	/// <param name="configs">连接点配置列表，包含端子板型号、IO类型、有效模式等匹配条件和计算公式</param>
	/// <param name="data">IO数据实体，包含连接点计算所需的全部参数信息</param>
	/// <returns>返回计算后的正极连接点编号，计算失败返回"ERR"</returns>
	/// <exception cref="!:CompilationErrorException">当C#脚本编译失败时抛出异常</exception>
	/// <exception cref="!:RuntimeException">当脚本运行时发生错误时抛出异常</exception>
	/// <remarks>
	/// 匹配条件（所有非空条件必须同时满足）：
	/// - 端子板型号匹配
	/// - IO类型匹配
	/// - 端子板编号后缀匹配
	/// - 信号有效模式匹配
	/// - 供电方式前缀匹配
	/// 计算公式中"CH"将被替换为实际通道号
	/// </remarks>
	public async Task<string> GetSingalPlusAsync(List<config_connection_points> configs, IoFullData data)
	{
		string text = (from x in configs
			where string.IsNullOrEmpty(x.TerminalBoardModel) || x.TerminalBoardModel == data.TerminalBoardModel
			where string.IsNullOrEmpty(x.IoType) || x.IoType == data.IoType
			where string.IsNullOrEmpty(x.TerminalBoardNumber) || data.TerminalBoardNumber.EndsWith(x.TerminalBoardNumber)
			where (x.SignalEffectiveMode ?? "") == data.SignalEffectiveMode
			where string.IsNullOrEmpty(x.PowerSupply) || data.TerminalBoardNumber.StartsWith(x.PowerSupply)
			select x).SingleOrDefault()?.SignalPlus ?? "ERR";
		if (text == "ERR")
		{
			return "ERR";
		}
		object value = await CSharpScript.EvaluateAsync(text.Replace("CH", $"{data.Channel}"));
		return $"{value}";
	}

	/// <summary>
	/// 异步获取信号负极连接点编号（基于配置表）
	/// 通过数据库配置表和C#脚本动态计算信号负极连接点，支持复杂的计算逻辑
	/// 与正极连接点计算方法配对使用，确保信号回路连接的完整性和准确性
	/// 适用于标准化连接点规则管理、复杂计算逻辑实现、产品化配置
	/// </summary>
	/// <param name="configs">连接点配置列表，包含端子板型号、IO类型、有效模式等匹配条件和计算公式</param>
	/// <param name="data">IO数据实体，包含连接点计算所需的全部参数信息</param>
	/// <returns>返回计算后的负极连接点编号，计算失败返回"ERR"</returns>
	/// <exception cref="!:CompilationErrorException">当C#脚本编译失败时抛出异常</exception>
	/// <exception cref="!:RuntimeException">当脚本运行时发生错误时抛出异常</exception>
	/// <remarks>
	/// 匹配逻辑与正极连接点相同：
	/// - 端子板型号、IO类型、端子板编号、有效模式、供电方式的组合匹配
	/// - 使用SignalMinus字段的计算公式
	/// - 通道号替换："CH" → 实际通道号
	/// - 单一匹配原则：必须找到唯一匹配的配置记录
	/// </remarks>
	public async Task<string> GetSignalMinusAsync(List<config_connection_points> configs, IoFullData data)
	{
		string text = (from x in configs
			where string.IsNullOrEmpty(x.TerminalBoardModel) || x.TerminalBoardModel == data.TerminalBoardModel
			where string.IsNullOrEmpty(x.IoType) || x.IoType == data.IoType
			where string.IsNullOrEmpty(x.TerminalBoardNumber) || data.TerminalBoardNumber.EndsWith(x.TerminalBoardNumber)
			where (x.SignalEffectiveMode ?? "") == data.SignalEffectiveMode
			where string.IsNullOrEmpty(x.PowerSupply) || data.TerminalBoardNumber.StartsWith(x.PowerSupply)
			select x).SingleOrDefault()?.SignalMinus ?? "ERR";
		if (text == "ERR")
		{
			return "ERR";
		}
		object value = await CSharpScript.EvaluateAsync(text.Replace("CH", $"{data.Channel}"));
		return $"{value}";
	}

	/// <summary>
	/// 获取RTD温度传感器C端补偿连接点
	/// 判断传感器类型是否需要C端补偿，并生成相应的连接点编号
	/// 主要用于PT100热电阻的温度补偿连接，确保测量精度
	/// 适用于温度测量回路设计、仪表接线图生成、现场调试指导
	/// </summary>
	/// <param name="sensorType">传感器类型描述，用于判断是否为PT100类型</param>
	/// <param name="channel">通道号，用于生成具体的连接点编号</param>
	/// <returns>如果是PT100传感器返回"{通道号}C"，否则返回"--"表示不需要补偿</returns>
	/// <remarks>
	/// 补偿连接规则：
	/// - PT100传感器：需要C端补偿连接，返回通道+"C"
	/// - 其他传感器：不需要补偿，返回"--"
	/// - 空值安全：传感器类型为null时返回"--"
	/// - 用途：三线制PT100的温度补偿
	/// </remarks>
	public string GetRTDCompensationEndC(string sensorType, int channel)
	{
		if (sensorType == null || !sensorType.Contains("PT100"))
		{
			return "--";
		}
		return $"{channel}C";
	}

	public string GetRTDCompensationEndD(string powerSupplyMethod, int channel)
	{
		if (powerSupplyMethod == "AI9")
		{
			return $"{channel}D";
		}
		return "--";
	}

	/// <summary>
	/// 生成FF从站模块编号
	/// 根据就地箱号、FF/DP从站号和FF从站模块型号生成完整的FF从站模块编号
	/// 仅当FF从站模块型号包含"FS"时才生成编号，否则返回"--"
	/// 适用于FF现场总线从站设备的标识管理、设备配置文档生成、现场设备标签制作
	/// </summary>
	/// <param name="localBoxNumber">就地箱号，标识现场设备所在的就地控制箱编号</param>
	/// <param name="ffSlaveStationNumber">FF/DP从站号，用于现场总线通信的设备地址标识</param>
	/// <param name="ffSlaveModuleModel">FF从站模块型号，如果包含"FS"则表示是FF从站模块</param>
	/// <returns>如果模块型号包含"FS"则返回格式为"{就地箱号}_{从站号}_{模块型号}"的编号，否则返回"--"</returns>
	/// <exception cref="T:System.ArgumentNullException">当任何输入参数为null时可能引发异常</exception>
	/// <remarks>
	/// 生成规则：
	/// - 检查FF从站模块型号是否包含"FS"字符串
	/// - 包含"FS"：生成格式为"{就地箱号}_{从站号}_{模块型号}"
	/// - 不包含"FS"：返回"--"表示不是FF从站模块
	/// - 空值安全：输入参数为null或空时返回"--"
	/// 业务场景：FF现场总线系统的从站设备管理、设备配置表生成、现场设备维护
	/// 示例：就地箱"JD001"，从站号"01"，模块"FS100" → "JD001_01_FS100"
	/// </remarks>
	public string GetFFSlaveModuleNumber(string localBoxNumber, string ffSlaveStationNumber, string ffSlaveModuleModel)
	{
		if (string.IsNullOrEmpty(localBoxNumber) || string.IsNullOrEmpty(ffSlaveStationNumber) || string.IsNullOrEmpty(ffSlaveModuleModel))
		{
			return "--";
		}
		try
		{
			if (ffSlaveModuleModel.Contains("FS"))
			{
				return $"{localBoxNumber}_{ffSlaveStationNumber}_{ffSlaveModuleModel}";
			}
			return "--";
		}
		catch (Exception)
		{
			return "--";
		}
	}

	/// <summary>
	/// 生成FF从站模块信号正极连接点
	/// 根据FF从站模块型号、IO类型、传感器类型和信号有效模式等参数，计算FF从站模块的正极连接点
	/// 仅当FF从站模块型号包含"FS"时才进行计算，否则返回"--"
	/// 适用于FF现场总线从站设备的信号连接设计、接线图生成、现场调试指导
	/// </summary>
	/// <param name="data">包含FF从站模块连接点计算所需信息的IO数据实体</param>
	/// <returns>返回正极连接点编号，如"01A"、"02B"等，"--"表示不需要连接或不是FF从站模块</returns>
	/// <exception cref="T:System.ArgumentNullException">当输入参数为null时可能引发异常</exception>
	/// <remarks>
	/// 计算规则（按优先级排序）：
	/// 1. 检查FF从站模块型号是否包含"FS"，不包含则返回"--"
	/// 2. A类型+四线制：{FF从站通道}A
	/// 3. AI类型：{FF从站通道}B
	/// 4. AO类型：{FF从站通道}C
	/// 5. DI类型：{FF从站通道}A
	/// 6. DO类型+NC模式：{FF从站通道}A
	/// 7. DO类型+其他模式：{FF从站通道}B
	/// 8. 其他情况："--"
	/// 业务场景：FF现场总线系统中从站设备的信号连接设计和施工指导
	/// </remarks>
	public string GetFFSlaveModuleSignalPlus(IoFullData data)
	{
		if (data == null || string.IsNullOrEmpty(data.FFSlaveModuleModel) || string.IsNullOrEmpty(data.IoType) || !data.FFTerminalChannel.HasValue)
		{
			return "--";
		}
		try
		{
			if (!data.FFSlaveModuleModel.Contains("FS"))
			{
				return "--";
			}
			int value = data.FFTerminalChannel.Value;
			string text = data.IoType?.Trim() ?? "";
			string text2 = data.SensorType ?? "";
			string text3 = data.SignalEffectiveMode ?? "";
			if (text.Contains("A") && text2.Contains("四"))
			{
				return $"{value}A";
			}
			if (text.Contains("AI"))
			{
				return $"{value}B";
			}
			if (text.Contains("AO"))
			{
				return $"{value}C";
			}
			if (text.Contains("DI"))
			{
				return $"{value}A";
			}
			if (text.Contains("DO") && text3.Contains("NC"))
			{
				return $"{value}A";
			}
			if (text.Contains("DO"))
			{
				return $"{value}B";
			}
			return "--";
		}
		catch (Exception)
		{
			return "--";
		}
	}

	/// <summary>
	/// 生成FF从站模块信号负极连接点
	/// 根据FF从站模块型号、IO类型、传感器类型等参数，计算FF从站模块的负极连接点
	/// 与正极连接点配对使用，确保信号回路连接的完整性和准确性
	/// 适用于FF现场总线从站设备的信号连接设计、接线图生成、现场调试指导
	/// </summary>
	/// <param name="data">包含FF从站模块连接点计算所需信息的IO数据实体</param>
	/// <returns>返回负极连接点编号，如"01C"、"02A"等，"--"表示不需要连接或不是FF从站模块</returns>
	/// <exception cref="T:System.ArgumentNullException">当输入参数为null时可能引发异常</exception>
	/// <remarks>
	/// 计算规则（按优先级排序）：
	/// 1. 检查FF从站模块型号是否包含"FS"，不包含则返回"--"
	/// 2. A类型+四线制：{FF从站通道}C
	/// 3. AI类型：{FF从站通道}A
	/// 4. AO类型：{FF从站通道}B
	/// 5. DI类型：{FF从站通道}B
	/// 6. DO类型+无源模式：{FF从站通道}C
	/// 7. DO类型+其他模式：{FF从站通道}D
	/// 8. 其他情况："--"
	/// 业务场景：FF现场总线系统中从站设备的信号连接设计和施工指导
	/// </remarks>
	public string GetFFSlaveModuleSignalMinus(IoFullData data)
	{
		if (data == null || string.IsNullOrEmpty(data.FFSlaveModuleModel) || string.IsNullOrEmpty(data.IoType) || !data.FFTerminalChannel.HasValue)
		{
			return "--";
		}
		try
		{
			if (!data.FFSlaveModuleModel.Contains("FS"))
			{
				return "--";
			}
			int value = data.FFTerminalChannel.Value;
			string text = data.IoType?.Trim() ?? "";
			string text2 = data.SensorType ?? "";
			if (text.Contains("A") && text2.Contains("四"))
			{
				return $"{value}C";
			}
			if (text.Contains("AI"))
			{
				return $"{value}A";
			}
			if (text.Contains("AO"))
			{
				return $"{value}B";
			}
			if (text.Contains("DI"))
			{
				return $"{value}B";
			}
			if (text.Contains("DO") && text2.Contains("无源"))
			{
				return $"{value}C";
			}
			if (text.Contains("DO"))
			{
				return $"{value}D";
			}
			return "--";
		}
		catch (Exception)
		{
			return "--";
		}
	}

	/// <summary>
	/// 执行FF模块自动分配
	/// 【修改】根据新的输入格式进行所有FF模块分配和通道计算，不再区分FF7/FF8和其他FF类型
	/// </summary>
	/// <returns>分配结果报告</returns>
	/// <exception cref="T:System.ArgumentException">当数据不完整或格式错误时抛出异常</exception>
	/// <exception cref="T:System.InvalidOperationException">当分配过程失败时抛出异常</exception>
	public async Task<string> PerformFFSlaveModuleAllocation()
	{
		try
		{
			if (AllData == null || AllData.Count == 0)
			{
				throw new ArgumentException("无IO数据可分配，请先导入数据");
			}
			StringBuilder report = new StringBuilder();
			report.AppendLine("FF模块分配报告");
			StringBuilder stringBuilder = report;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(5, 1, stringBuilder);
			handler.AppendLiteral("分配时间：");
			handler.AppendFormatted(DateTime.Now, "yyyy-MM-dd HH:mm:ss");
			stringBuilder2.AppendLine(ref handler);
			report.AppendLine(new string('=', 50));
			ClearFFSlaveAllocationResults();
			report.AppendLine("已清空之前的FF模块分配结果");
			report.AppendLine();
			List<IoFullData> list = AllData.Where((IoFullData signal) => !string.IsNullOrEmpty(signal.PowerType) && !string.IsNullOrEmpty(signal.IoType) && signal.PowerType.Contains("FF") && (signal.IoType.Trim() == "FFAI" || signal.IoType.Trim() == "FFAO" || signal.IoType.Trim() == "FFDI" || signal.IoType.Trim() == "FFDO")).ToList();
			if (!list.Any())
			{
				report.AppendLine("未找到有效的FF模块信号");
				return report.ToString();
			}
			stringBuilder = report;
			StringBuilder stringBuilder3 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder);
			handler.AppendLiteral("找到");
			handler.AppendFormatted(list.Count);
			handler.AppendLiteral("个包含FF模块信息的信号");
			stringBuilder3.AppendLine(ref handler);
			IEnumerable<IGrouping<string, IoFullData>> enumerable = from s in list
				group s by s.CabinetNumber;
			int totalProcessedSignals = 0;
			int totalModulesAllocated = 0;
			foreach (IGrouping<string, IoFullData> item in enumerable)
			{
				(string, int, int) tuple = await ProcessCabinetFFSlaveModuleAllocation(item.Key, item.ToList());
				report.AppendLine(tuple.Item1);
				totalProcessedSignals += tuple.Item2;
				totalModulesAllocated += tuple.Item3;
			}
			report.AppendLine(new string('=', 50));
			report.AppendLine("分配结果统计：");
			stringBuilder = report;
			StringBuilder stringBuilder4 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder);
			handler.AppendLiteral("处理信号数量：");
			handler.AppendFormatted(totalProcessedSignals);
			stringBuilder4.AppendLine(ref handler);
			stringBuilder = report;
			StringBuilder stringBuilder5 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder);
			handler.AppendLiteral("分配模块数量：");
			handler.AppendFormatted(totalModulesAllocated);
			stringBuilder5.AppendLine(ref handler);
			report.AppendLine("分配完成！");
			return report.ToString();
		}
		catch (ArgumentException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			throw new InvalidOperationException("FF从站模块分配过程中发生错误: " + ex2.Message, ex2);
		}
	}

	/// <summary>
	/// 清空FF模块分配结果
	/// 【修改】在重新分配之前清空所有FF模块相关字段，不再仅限于FF7/FF8，确保数据一致性
	/// </summary>
	/// <remarks>
	/// 清空字段包括：
	/// - FFSlaveStationNumber: FF从站站号
	/// - FFSlaveChannel: FF从站通道
	/// - DPValveSequence: DP阀位顺序
	/// - FFSlaveModuleID: FF从站模块编号
	/// - FFSlaveModuleSignalPositive: FF从站模块信号正极
	/// - FFSlaveModuleSignalNegative: FF从站模块信号负极
	/// 注意：FFSlaveModuleModel不清空，因为它作为输入配置保持
	/// </remarks>
	private void ClearFFSlaveAllocationResults()
	{
		foreach (IoFullData allDatum in AllData)
		{
			if (!string.IsNullOrEmpty(allDatum.PowerType) && allDatum.PowerType.Contains("FF"))
			{
				allDatum.FFDPStaionNumber = null;
				allDatum.FFTerminalChannel = null;
				allDatum.FFSlaveModuleModel = null;
				allDatum.FFSlaveModuleID = null;
				allDatum.FFSlaveModuleSignalPositive = null;
				allDatum.FFSlaveModuleSignalNegative = null;
			}
		}
	}

	/// <summary>
	/// 按设备对信号进行分组
	/// 根据信号类型组合采用不同的分组规则
	/// </summary>
	/// <param name="signals">需要分组的信号列表</param>
	/// <returns>按设备分组的信号组</returns>
	/// <exception cref="T:System.ArgumentException">当信号数据不完整时抛出异常</exception>
	/// <remarks>
	/// 分组规则：
	/// - A信号 + A信号：短横线之后相同
	/// - D信号 + D信号：整个信号位号相同
	/// - A信号 + D信号：短横线之后相同
	/// </remarks>
	private IEnumerable<IGrouping<string, IoFullData>> GroupSignalsByDevice(List<IoFullData> signals)
	{
		try
		{
			List<IoFullData> list = signals.Where((IoFullData s) => !string.IsNullOrEmpty(s.IoType) && !string.IsNullOrEmpty(s.SignalPositionNumber)).ToList();
			if (list.Count != signals.Count)
			{
				throw new ArgumentException("存在无效的信号数据（IoType或SignalPositionNumber为空）");
			}
			Dictionary<string, List<IoFullData>> dictionary = new Dictionary<string, List<IoFullData>>();
			foreach (IoFullData signal in list)
			{
				string key = null;
				bool flag = false;
				foreach (KeyValuePair<string, List<IoFullData>> item in dictionary)
				{
					if (item.Value.Any((IoFullData existingSignal) => IsSameDeviceGroup(signal, existingSignal)))
					{
						key = item.Key;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					key = GetDeviceIdentifier(signal);
					dictionary[key] = new List<IoFullData>();
				}
				dictionary[key].Add(signal);
			}
			return dictionary.Select((KeyValuePair<string, List<IoFullData>> kvp) => new DeviceGroup(kvp.Key, kvp.Value));
		}
		catch (Exception ex)
		{
			throw new ArgumentException("设备分组失败: " + ex.Message, ex);
		}
	}

	/// <summary>
	/// 判断两个信号是否属于同一设备组
	/// </summary>
	private bool IsSameDeviceGroup(IoFullData signal1, IoFullData signal2)
	{
		if (signal1 == null || signal2 == null)
		{
			return false;
		}
		string ioType = signal1.IoType;
		string ioType2 = signal2.IoType;
		if (ioType.Contains("A") && ioType2.Contains("A"))
		{
			return GetKey(signal1) == GetKey(signal2);
		}
		if (ioType.Contains("D") && ioType2.Contains("D"))
		{
			return GetKey(signal1, useFullNumber: true) == GetKey(signal2, useFullNumber: true);
		}
		if ((ioType.Contains("A") && ioType2.Contains("D")) || (ioType.Contains("D") && ioType2.Contains("A")))
		{
			return GetKey(signal1) == GetKey(signal2);
		}
		return false;
		static string GetKey(IoFullData ioFullData, bool useFullNumber = false)
		{
			if (!useFullNumber)
			{
				string[] array = ioFullData.SignalPositionNumber?.Split('-');
				string text;
				if (array == null || array.Length <= 1)
				{
					text = ioFullData.SignalPositionNumber;
					if (text == null)
					{
						return "";
					}
				}
				else
				{
					text = array.Last();
				}
				return text;
			}
			return ioFullData.SignalPositionNumber ?? "";
		}
	}

	/// <summary>
	/// 获取设备标识符
	/// </summary>
	private string GetDeviceIdentifier(IoFullData signal)
	{
		if (signal != null && signal.IoType?.Contains("D") == true)
		{
			return signal.SignalPositionNumber ?? "";
		}
		string[] array = signal?.SignalPositionNumber?.Split('-');
		object obj;
		if (array == null || array.Length <= 1)
		{
			obj = signal?.SignalPositionNumber;
			if (obj == null)
			{
				return "";
			}
		}
		else
		{
			obj = array.Last();
		}
		return (string)obj;
	}

	/// <summary>
	/// 处理单个机柜的FF从站模块分配
	/// 按照物理结构：机柜 → 机笼 → 插槽 → 板卡 → 网段 进行分组处理
	/// 每个网段内的信号为一个处理单元
	/// </summary>
	/// <param name="cabinetName">机柜名称</param>
	/// <param name="cabinetSignals">机柜内的所有信号</param>
	/// <returns>分配结果</returns>
	private async Task<(string Report, int ProcessedSignals, int ModulesAllocated)> ProcessCabinetFFSlaveModuleAllocation(string cabinetName, List<IoFullData> cabinetSignals)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(5, 1, stringBuilder2);
		handler.AppendLiteral("\n机柜 ");
		handler.AppendFormatted(cabinetName);
		handler.AppendLiteral(":");
		stringBuilder3.AppendLine(ref handler);
		int num = 0;
		int num2 = 0;
		try
		{
			var list = (from s in cabinetSignals
				where s.Cage > 0 && s.Slot > 0 && !string.IsNullOrEmpty(s.NetType)
				group s by new { s.Cage, s.Slot, s.NetType } into g
				orderby g.Key.Cage, g.Key.Slot, g.Key.NetType
				select g).ToList();
			if (!list.Any())
			{
				stringBuilder.AppendLine("  未找到有效的网段单元（机笼-插槽-网段）");
				return (Report: stringBuilder.ToString(), ProcessedSignals: 0, ModulesAllocated: 0);
			}
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
			handler.AppendLiteral("  找到 ");
			handler.AppendFormatted(list.Count);
			handler.AppendLiteral(" 个网段处理单元");
			stringBuilder4.AppendLine(ref handler);
			foreach (var item in list)
			{
				(string, int, int) tuple = ProcessBoardUnitFFSlaveModules(cabinetName, item.Key.Cage, item.Key.Slot, item.Key.NetType, item.ToList());
				stringBuilder.AppendLine(tuple.Item1);
				num += tuple.Item2;
				num2 += tuple.Item3;
			}
		}
		catch (Exception ex)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(5, 1, stringBuilder2);
			handler.AppendLiteral("  错误：");
			handler.AppendFormatted(ex.Message);
			stringBuilder5.AppendLine(ref handler);
		}
		return (Report: stringBuilder.ToString(), ProcessedSignals: num, ModulesAllocated: num2);
	}

	/// <summary>
	/// 处理板卡单元的FF从站模块分配
	/// 板卡单元：机笼 + 插槽 + 网段，这是最小的处理单元
	/// 先按设备分组，再按模块类型分配
	/// </summary>
	/// <param name="cabinetName">机柜名称</param>
	/// <param name="cage">机笼号</param>
	/// <param name="slot">插槽号</param>
	/// <param name="networkType">网段类型</param>
	/// <param name="unitSignals">板卡单元内的信号</param>
	/// <returns>处理结果</returns>
	private (string Report, int ProcessedSignals, int ModulesAllocated) ProcessBoardUnitFFSlaveModules(string cabinetName, int cage, int slot, string networkType, List<IoFullData> unitSignals)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(10, 3, stringBuilder2);
		handler.AppendLiteral("  板卡单元 ");
		handler.AppendFormatted(cage);
		handler.AppendLiteral("-");
		handler.AppendFormatted(slot, "00");
		handler.AppendLiteral("-");
		handler.AppendFormatted(networkType);
		handler.AppendLiteral(":");
		stringBuilder3.AppendLine(ref handler);
		int item = 0;
		int item2 = 0;
		int baseStationNumber = 1;
		try
		{
			IEnumerable<IGrouping<string, IoFullData>> source = GroupSignalsByDevice(unitSignals);
			if (!source.Any())
			{
				stringBuilder.AppendLine("    未找到有效的设备分组");
				return (Report: stringBuilder.ToString(), ProcessedSignals: 0, ModulesAllocated: 0);
			}
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, stringBuilder2);
			handler.AppendLiteral("    找到 ");
			handler.AppendFormatted(source.Count());
			handler.AppendLiteral(" 个设备组");
			stringBuilder4.AppendLine(ref handler);
			(item, item2) = ProcessAllDeviceGroupsInNetwork(cabinetName, cage, slot, networkType, unitSignals, baseStationNumber, stringBuilder);
		}
		catch (Exception ex)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder2);
			handler.AppendLiteral("    错误：");
			handler.AppendFormatted(ex.Message);
			stringBuilder5.AppendLine(ref handler);
		}
		return (Report: stringBuilder.ToString(), ProcessedSignals: item, ModulesAllocated: item2);
	}

	/// <summary>
	/// 处理网段内所有设备组的FF从站模块分配（共享模块资源）
	/// </summary>
	private (int ProcessedSignals, int ModulesAllocated) ProcessAllDeviceGroupsInNetwork(string cabinetName, int cage, int slot, string networkType, List<IoFullData> allSignals, int baseStationNumber, StringBuilder report)
	{
		int num = 0;
		int num2 = 0;
		try
		{
			if (!allSignals.Any())
			{
				report.AppendLine("    网段信号为空");
				return (ProcessedSignals: 0, ModulesAllocated: 0);
			}
			List<string> list = (from s in allSignals
				select s.FFSlaveModuleModel into m
				where !string.IsNullOrEmpty(m)
				select m).Distinct().ToList();
			if (!list.Any())
			{
				report.AppendLine("    网段无有效模块配置");
				return (ProcessedSignals: 0, ModulesAllocated: 0);
			}
			List<(int, string)> list2 = ParseAndMergeNetworkModules(list);
			StringBuilder stringBuilder = report;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, stringBuilder);
			handler.AppendLiteral("    网段模块配置: ");
			handler.AppendFormatted(string.Join(", ", list2.Select<(int, string), string>(((int Count, string ModuleType) m) => $"{m.Count}个{m.ModuleType}")));
			stringBuilder2.AppendLine(ref handler);
			(int, int) tuple = AllocateDeviceGroupsToNetworkModules(allSignals, list2, baseStationNumber, report);
			num = tuple.Item1;
			num2 = tuple.Item2;
			stringBuilder = report;
			StringBuilder stringBuilder3 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(22, 2, stringBuilder);
			handler.AppendLiteral("    网段分配完成：使用");
			handler.AppendFormatted(num2);
			handler.AppendLiteral("个模块，处理");
			handler.AppendFormatted(num);
			handler.AppendLiteral("个信号");
			stringBuilder3.AppendLine(ref handler);
		}
		catch (Exception ex)
		{
			StringBuilder stringBuilder = report;
			StringBuilder stringBuilder4 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder);
			handler.AppendLiteral("    错误：");
			handler.AppendFormatted(ex.Message);
			stringBuilder4.AppendLine(ref handler);
		}
		return (ProcessedSignals: num, ModulesAllocated: num2);
	}

	/// <summary>
	/// 解析网段模块配置（如"1FS201 1FS202"表示1个FS201和1个FS202）
	/// </summary>
	private List<(int Count, string ModuleType)> ParseNetworkModules(string moduleInput)
	{
		List<(int, string)> list = new List<(int, string)>();
		if (string.IsNullOrEmpty(moduleInput))
		{
			return list;
		}
		string[] array = moduleInput.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = array;
		foreach (string text in array2)
		{
			(int, string) item = ParseFFSlaveModuleModel(text.Trim());
			if (!string.IsNullOrEmpty(item.Item2))
			{
				list.Add(item);
			}
		}
		return list;
	}

	/// <summary>
	/// 解析并合并多个模块配置字符串，去重并统计数量
	/// 例如：["1FS201 1FS202", "2FS201"] → [(3, "FS201"), (1, "FS202")]
	/// </summary>
	/// <param name="moduleConfigs">多个模块配置字符串</param>
	/// <returns>合并后的模块列表</returns>
	private List<(int Count, string ModuleType)> ParseAndMergeNetworkModules(List<string> moduleConfigs)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (string moduleConfig in moduleConfigs)
		{
			if (string.IsNullOrEmpty(moduleConfig))
			{
				continue;
			}
			List<(int, string)> list = ParseNetworkModules(moduleConfig);
			foreach (var item in list)
			{
				if (dictionary.ContainsKey(item.Item2))
				{
					dictionary[item.Item2] = Math.Max(dictionary[item.Item2], item.Item1);
				}
				else
				{
					dictionary[item.Item2] = item.Item1;
				}
			}
		}
		return dictionary.Select((KeyValuePair<string, int> kvp) => (Value: kvp.Value, Key: kvp.Key)).ToList();
	}

	private (int ProcessedSignals, int ModulesUsed) AllocateDeviceGroupsToNetworkModules(List<IoFullData> signals, List<(int Count, string ModuleType)> networkModules, int baseStationNumber, StringBuilder report)
	{
		int num = 0;
		List<(string, Dictionary<string, int>, List<IoFullData>)> list = new List<(string, Dictionary<string, int>, List<IoFullData>)>();
		foreach (var networkModule in networkModules)
		{
			for (int i = 0; i < networkModule.Count; i++)
			{
				Dictionary<string, int> item = new Dictionary<string, int>
				{
					["AI"] = 0,
					["AO"] = 0,
					["DI"] = 0,
					["DO"] = 0
				};
				List<IoFullData> item2 = new List<IoFullData>();
				list.Add((networkModule.ModuleType, item, item2));
			}
		}
		StringBuilder stringBuilder = report;
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder);
		handler.AppendLiteral("        网段共有");
		handler.AppendFormatted(list.Count);
		handler.AppendLiteral("个模块:");
		stringBuilder2.AppendLine(ref handler);
		for (int j = 0; j < list.Count; j++)
		{
			(string, Dictionary<string, int>, List<IoFullData>) tuple = list[j];
			string moduleCapacityInfo = GetModuleCapacityInfo(tuple.Item1);
			stringBuilder = report;
			StringBuilder stringBuilder3 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(17, 3, stringBuilder);
			handler.AppendLiteral("          模块");
			handler.AppendFormatted(j + 1);
			handler.AppendLiteral(": ");
			handler.AppendFormatted(tuple.Item1);
			handler.AppendLiteral(" - ");
			handler.AppendFormatted(moduleCapacityInfo);
			stringBuilder3.AppendLine(ref handler);
		}
		IEnumerable<IGrouping<string, IoFullData>> enumerable = GroupSignalsByDevice(signals);
		stringBuilder = report;
		StringBuilder stringBuilder4 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder);
		handler.AppendLiteral("        设备分组数量: ");
		handler.AppendFormatted(enumerable.Count());
		stringBuilder4.AppendLine(ref handler);
		int num2 = 0;
		foreach (IGrouping<string, IoFullData> item3 in enumerable)
		{
			List<IoFullData> list2 = item3.ToList();
			num2++;
			stringBuilder = report;
			StringBuilder stringBuilder5 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(21, 3, stringBuilder);
			handler.AppendLiteral("        第");
			handler.AppendFormatted(num2);
			handler.AppendLiteral("个设备组 ");
			handler.AppendFormatted(item3.Key);
			handler.AppendLiteral(" (");
			handler.AppendFormatted(list2.Count);
			handler.AppendLiteral("个信号):");
			stringBuilder5.AppendLine(ref handler);
			int num3 = SelectBestModuleForDeviceGroup(list2, list, report);
			if (num3 == -1)
			{
				stringBuilder = report;
				StringBuilder stringBuilder6 = stringBuilder;
				handler = new StringBuilder.AppendInterpolatedStringHandler(36, 1, stringBuilder);
				handler.AppendLiteral("          警告: 没有模块能支持设备组 ");
				handler.AppendFormatted(item3.Key);
				handler.AppendLiteral(" 的所有信号或容量不足");
				stringBuilder6.AppendLine(ref handler);
				continue;
			}
			stringBuilder = report;
			StringBuilder stringBuilder7 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(20, 2, stringBuilder);
			handler.AppendLiteral("          选择模块");
			handler.AppendFormatted(num3 + 1);
			handler.AppendLiteral("(");
			handler.AppendFormatted(list[num3].Item1);
			handler.AppendLiteral(")进行分配");
			stringBuilder7.AppendLine(ref handler);
			num += AssignDeviceGroupToSelectedModule(list2, num3, list, report).ProcessedSignals;
			stringBuilder = report;
			StringBuilder stringBuilder8 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(22, 1, stringBuilder);
			handler.AppendLiteral("        设备组");
			handler.AppendFormatted(num2);
			handler.AppendLiteral("分配后，模块剩余情况:");
			stringBuilder8.AppendLine(ref handler);
			for (int k = 0; k < list.Count; k++)
			{
				(string, Dictionary<string, int>, List<IoFullData>) tuple2 = list[k];
				string detailedRemainingCapacity = GetDetailedRemainingCapacity(tuple2.Item1, tuple2.Item2);
				string value = $"已分配{tuple2.Item3.Count}个信号";
				stringBuilder = report;
				StringBuilder stringBuilder9 = stringBuilder;
				handler = new StringBuilder.AppendInterpolatedStringHandler(19, 4, stringBuilder);
				handler.AppendLiteral("          模块");
				handler.AppendFormatted(k + 1);
				handler.AppendLiteral("(");
				handler.AppendFormatted(tuple2.Item1);
				handler.AppendLiteral("): ");
				handler.AppendFormatted(detailedRemainingCapacity);
				handler.AppendLiteral(" (");
				handler.AppendFormatted(value);
				handler.AppendLiteral(")");
				stringBuilder9.AppendLine(ref handler);
			}
			report.AppendLine("");
		}
		for (int l = 0; l < list.Count; l++)
		{
			(string, Dictionary<string, int>, List<IoFullData>) tuple3 = list[l];
			int value2 = baseStationNumber + l;
			foreach (IoFullData item4 in tuple3.Item3)
			{
				item4.FFDPStaionNumber = value2.ToString("D2");
				item4.FFSlaveModuleModel = UpdateModuleTypeInOriginalFormat(item4.FFSlaveModuleModel, tuple3.Item1);
				item4.FFSlaveModuleID = GetFFSlaveModuleNumber(item4.LocalBoxNumber, item4.FFDPStaionNumber, tuple3.Item1);
				item4.FFSlaveModuleSignalPositive = GetFFSlaveModuleSignalPlus(item4);
				item4.FFSlaveModuleSignalNegative = GetFFSlaveModuleSignalMinus(item4);
			}
			if (tuple3.Item3.Any())
			{
				string detailedRemainingCapacity2 = GetDetailedRemainingCapacity(tuple3.Item1, tuple3.Item2);
				stringBuilder = report;
				StringBuilder stringBuilder10 = stringBuilder;
				handler = new StringBuilder.AppendInterpolatedStringHandler(29, 5, stringBuilder);
				handler.AppendLiteral("        模块");
				handler.AppendFormatted(l + 1);
				handler.AppendLiteral("(");
				handler.AppendFormatted(tuple3.Item1);
				handler.AppendLiteral(")分配完成: 站号");
				handler.AppendFormatted(value2, "D2");
				handler.AppendLiteral(", 分配");
				handler.AppendFormatted(tuple3.Item3.Count);
				handler.AppendLiteral("个信号, ");
				handler.AppendFormatted(detailedRemainingCapacity2);
				stringBuilder10.AppendLine(ref handler);
				stringBuilder = report;
				StringBuilder stringBuilder11 = stringBuilder;
				handler = new StringBuilder.AppendInterpolatedStringHandler(39, 2, stringBuilder);
				handler.AppendLiteral("          已更新");
				handler.AppendFormatted(tuple3.Item3.Count);
				handler.AppendLiteral("个信号的FF模块型号：实际分配到");
				handler.AppendFormatted(tuple3.Item1);
				handler.AppendLiteral("(保持原始数量信息)");
				stringBuilder11.AppendLine(ref handler);
			}
		}
		return (ProcessedSignals: num, ModulesUsed: list.Count);
	}

	private int SelectBestModuleForDeviceGroup(List<IoFullData> deviceSignals, List<(string ModuleType, Dictionary<string, int> UsedChannels, List<IoFullData> AssignedSignals)> moduleDefinitions, StringBuilder report)
	{
		List<string> list = deviceSignals.Select((IoFullData s) => GetIOTypePrefix(s.IoType)).Distinct().ToList();
		StringBuilder stringBuilder = report;
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(18, 1, stringBuilder);
		handler.AppendLiteral("          设备IO类型: ");
		handler.AppendFormatted(string.Join(", ", list));
		stringBuilder2.AppendLine(ref handler);
		List<(int, int)> list2 = new List<(int, int)>();
		for (int num = 0; num < moduleDefinitions.Count; num++)
		{
			(string ModuleType, Dictionary<string, int> UsedChannels, List<IoFullData> AssignedSignals) module = moduleDefinitions[num];
			if (!list.All((string ioTypePrefix) => GetChannelRange(module.ModuleType, ioTypePrefix).HasValue))
			{
				stringBuilder = report;
				StringBuilder stringBuilder3 = stringBuilder;
				handler = new StringBuilder.AppendInterpolatedStringHandler(25, 2, stringBuilder);
				handler.AppendLiteral("          模块");
				handler.AppendFormatted(num + 1);
				handler.AppendLiteral("(");
				handler.AppendFormatted(module.ModuleType);
				handler.AppendLiteral("): 不支持所有IO类型");
				stringBuilder3.AppendLine(ref handler);
				continue;
			}
			bool flag = true;
			List<string> list3 = new List<string>();
			foreach (string ioType in list)
			{
				(int, int) value = GetChannelRange(module.ModuleType, ioType).Value;
				int num2 = deviceSignals.Count((IoFullData s) => GetIOTypePrefix(s.IoType) == ioType);
				int num3 = value.Item2 - module.UsedChannels[ioType];
				list3.Add($"{ioType}需要{num2}/剩余{num3}");
				if (num2 > num3)
				{
					flag = false;
				}
			}
			stringBuilder = report;
			StringBuilder stringBuilder4 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(19, 4, stringBuilder);
			handler.AppendLiteral("          模块");
			handler.AppendFormatted(num + 1);
			handler.AppendLiteral("(");
			handler.AppendFormatted(module.ModuleType);
			handler.AppendLiteral("): ");
			handler.AppendFormatted(string.Join(", ", list3));
			handler.AppendLiteral(" - ");
			handler.AppendFormatted(flag ? "容量充足" : "容量不足");
			stringBuilder4.AppendLine(ref handler);
			if (flag)
			{
				int count = module.AssignedSignals.Count;
				list2.Add((num, count));
			}
		}
		if (!list2.Any())
		{
			return -1;
		}
		(int, int) tuple = list2.OrderBy<(int, int), int>(((int Index, int LoadScore) c) => c.LoadScore).First();
		stringBuilder = report;
		StringBuilder stringBuilder5 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(25, 2, stringBuilder);
		handler.AppendLiteral("          选择结果: 模块");
		handler.AppendFormatted(tuple.Item1 + 1);
		handler.AppendLiteral("(负载");
		handler.AppendFormatted(tuple.Item2);
		handler.AppendLiteral("个信号)");
		stringBuilder5.AppendLine(ref handler);
		return tuple.Item1;
	}

	private (int ProcessedSignals, bool Success) AssignDeviceGroupToSelectedModule(List<IoFullData> deviceSignals, int moduleIndex, List<(string ModuleType, Dictionary<string, int> UsedChannels, List<IoFullData> AssignedSignals)> moduleDefinitions, StringBuilder report)
	{
		int num = 0;
		(string, Dictionary<string, int>, List<IoFullData>) tuple = moduleDefinitions[moduleIndex];
		List<IGrouping<string, IoFullData>> list = (from s in deviceSignals
			group s by GetIOTypePrefix(s.IoType)).ToList();
		foreach (IGrouping<string, IoFullData> item in list)
		{
			string key = item.Key;
			List<IoFullData> list2 = item.OrderBy((IoFullData s) => s.SignalPositionNumber).ToList();
			int num2 = GetChannelRange(tuple.Item1, key).Value.Start + tuple.Item2[key];
			StringBuilder stringBuilder = report;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(21, 3, stringBuilder);
			handler.AppendLiteral("          分配");
			handler.AppendFormatted(key);
			handler.AppendLiteral("信号(");
			handler.AppendFormatted(list2.Count);
			handler.AppendLiteral("个)到模块");
			handler.AppendFormatted(moduleIndex + 1);
			handler.AppendLiteral(":");
			stringBuilder2.AppendLine(ref handler);
			for (int num3 = 0; num3 < list2.Count; num3++)
			{
				IoFullData ioFullData = list2[num3];
				ioFullData.FFTerminalChannel = num2 + num3;
				tuple.Item3.Add(ioFullData);
				stringBuilder = report;
				StringBuilder stringBuilder3 = stringBuilder;
				handler = new StringBuilder.AppendInterpolatedStringHandler(18, 2, stringBuilder);
				handler.AppendLiteral("            ");
				handler.AppendFormatted(ioFullData.SignalPositionNumber);
				handler.AppendLiteral(" -> 通道");
				handler.AppendFormatted(ioFullData.FFTerminalChannel);
				stringBuilder3.AppendLine(ref handler);
				num++;
			}
			tuple.Item2[key] += list2.Count;
		}
		return (ProcessedSignals: num, Success: true);
	}

	/// <summary>
	/// 获取模块容量信息
	/// </summary>
	private string GetModuleCapacityInfo(string moduleType)
	{
		string text = moduleType?.Trim();
		if (!(text == "FS201"))
		{
			if (text == "FS202")
			{
				return "DI:10, DO:6";
			}
			return "未知模块类型";
		}
		return "AI:2, AO:2, DI:6, DO:4";
	}

	/// <summary>
	/// 从原始网段配置中找到匹配的模块配置，保持原始格式
	/// 例如：网段配置"1FS201 2FS202"，实际分配到FS202 → 返回"2FS202"
	/// </summary>
	/// <param name="originalFormat">原始网段配置，如"1FS201 2FS202"</param>
	/// <param name="actualModuleType">实际分配到的模块型号，如"FS202"</param>
	/// <returns>匹配的原始模块配置字符串</returns>
	private string UpdateModuleTypeInOriginalFormat(string originalFormat, string actualModuleType)
	{
		if (string.IsNullOrEmpty(originalFormat) || string.IsNullOrEmpty(actualModuleType))
		{
			return actualModuleType ?? "";
		}
		try
		{
			string[] array = originalFormat.Split(new char[4] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string text in array2)
			{
				string text2 = text.Trim();
				if (text2.IndexOf(actualModuleType, StringComparison.OrdinalIgnoreCase) >= 0 && Regex.IsMatch(text2, "^\\d*FS\\d+$", RegexOptions.IgnoreCase))
				{
					return text2;
				}
			}
			string[] array3 = array;
			foreach (string text3 in array3)
			{
				string text4 = text3.Trim();
				if (Regex.IsMatch(text4, "^\\d*FS\\d+$", RegexOptions.IgnoreCase))
				{
					(int, string) tuple = ParseFFSlaveModuleModel(text4);
					return (tuple.Item1 > 1) ? $"{tuple.Item1}{actualModuleType}" : actualModuleType;
				}
			}
			return actualModuleType;
		}
		catch (Exception)
		{
			return actualModuleType;
		}
	}

	private string GetDetailedRemainingCapacity(string moduleType, Dictionary<string, int> usedChannels)
	{
		List<string> list = new List<string>();
		if (moduleType == "FS201")
		{
			int value = 2 - usedChannels["AI"];
			int value2 = 2 - usedChannels["AO"];
			int value3 = 6 - usedChannels["DI"];
			int value4 = 4 - usedChannels["DO"];
			list.Add($"AI剩余{value}通道");
			list.Add($"AO剩余{value2}通道");
			list.Add($"DI剩余{value3}通道");
			list.Add($"DO剩余{value4}通道");
		}
		else if (moduleType == "FS202")
		{
			int value5 = 10 - usedChannels["DI"];
			int value6 = 2 - usedChannels["DO"];
			list.Add($"DI剩余{value5}通道");
			list.Add($"DO剩余{value6}通道");
		}
		return string.Join(", ", list);
	}

	/// <summary>
	/// 获取IO类型前缀
	/// </summary>
	/// <param name="ioType">IO类型</param>
	/// <returns>IO类型前缀</returns>
	private string GetIOTypePrefix(string ioType)
	{
		if (string.IsNullOrEmpty(ioType))
		{
			return "";
		}
		string text = ioType.Trim().ToUpper();
		if (text.Contains("AI"))
		{
			return "AI";
		}
		if (text.Contains("AO"))
		{
			return "AO";
		}
		if (text.Contains("DI"))
		{
			return "DI";
		}
		if (text.Contains("DO"))
		{
			return "DO";
		}
		return "";
	}

	/// <summary>
	/// 解析FF从站模块型号，提取数量和模块类型
	/// 根据项目规范，输入格式为'数量+型号'，如'2FS202'表示2个FS202模块
	/// </summary>
	/// <param name="moduleInput">模块输入字符串，如"2FS202"、"1FS201"、"FS201"等</param>
	/// <returns>返回元组(数量, 模块型号)，解析失败时返回(1, 原字符串)</returns>
	/// <remarks>
	/// 解析规则：
	/// - "2FS202" → (2, "FS202")
	/// - "1FS201" → (1, "FS201")
	/// - "FS201" → (1, "FS201") // 没有数量前缀时默认为1个
	/// </remarks>
	private (int Count, string ModuleType) ParseFFSlaveModuleModel(string moduleInput)
	{
		if (string.IsNullOrEmpty(moduleInput))
		{
			return (Count: 1, ModuleType: moduleInput ?? "");
		}
		string text = moduleInput.Trim();
		int num = text.IndexOf("FS", StringComparison.OrdinalIgnoreCase);
		switch (num)
		{
		case -1:
			return (Count: 1, ModuleType: text);
		case 0:
			return (Count: 1, ModuleType: text);
		default:
		{
			string s = text.Substring(0, num).Trim();
			string item = text.Substring(num).Trim();
			if (int.TryParse(s, out var result) && result > 0)
			{
				return (Count: result, ModuleType: item);
			}
			return (Count: 1, ModuleType: text);
		}
		}
	}

	private (int Start, int Count)? GetChannelRange(string moduleType, string ioTypePrefix)
	{
		string text = moduleType?.Trim();
		if (!(text == "FS201"))
		{
			if (text == "FS202")
			{
				return ioTypePrefix switch
				{
					"DI" => (1, 10), 
					"DO" => (11, 6), 
					"AI" => null, 
					"AO" => null, 
					_ => null, 
				};
			}
			return null;
		}
		return ioTypePrefix switch
		{
			"AI" => (1, 2), 
			"AO" => (3, 2), 
			"DI" => (5, 6), 
			"DO" => (11, 4), 
			_ => null, 
		};
	}

	public void OnNavigatedFrom()
	{
	}

	public async void OnNavigatedTo()
	{
		if (!isInit)
		{
			InitializeFilters();
			await RefreshProjects();
			config_Card_Types = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
			isInit = true;
		}
		else
		{
			FilterAndSort();
		}
	}

	/// <summary>保存当前AllData并上传实时文件，如需要发布，同时传入versionId参数</summary>
	/// <param name="versionId">发布版本ID</param>
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

	/// <summary>从服务器下载实时数据文件并加载</summary>
	public async Task ReloadAllData()
	{
		AllData = null;
		_ = (Project ?? throw new Exception("开发人员注意")).Id;
		int id = (SubProject ?? throw new Exception("开发人员注意")).Id;
		try
		{
			string fileName = await storage.DownloadRealtimeIoFileAsync(id);
			IEnumerable<IoFullData> enumerable = (await excel.GetDataTableAsStringAsync(fileName, hasHeader: true)).StringTableToIEnumerableByDiplay<IoFullData>();
			ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
			foreach (IoFullData item in enumerable)
			{
				observableCollection.Add(item);
			}
			AllData = observableCollection;
		}
		catch (Exception)
		{
			AllData = new ObservableCollection<IoFullData>();
		}
	}

	[RelayCommand]
	private async void GetTotalSummaryInfo()
	{
		if (AllData == null)
		{
			throw new Exception();
		}
		TotalSummaryInfo = CabinetCalc.GetTotalSummaryInfo(AllData.ToList(), config_Card_Types);
		IsTotalSummaryFlyoutOpen = true;
	}

	[RelayCommand]
	private void ExtractPdfData()
	{
		epvm.IoFields = new ObservableCollection<string>
		{
			"序号", "机柜号", "就地箱号", "信号位号", "扩展码", "信号功能", "安全分级", "抗震类别", "传感器类型", "IO类型",
			"信号特性", "供电类型", "最小测量范围", "最大测量范围", "单位", "电压等级", "仪表功能号", "版本", "备注"
		};
		navigation.NavigateWithHierarchy(typeof(ExtractPdfPage));
	}

	[RelayCommand]
	private void ImportExcelData()
	{
		parameterService.SetParameter("controlSystem", ControlSystem.龙鳍);
		navigation.NavigateWithHierarchy(typeof(UploadExcelDataPage));
	}

	[RelayCommand]
	private async Task AddTag(TagType type)
	{
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		switch (type)
		{
		case TagType.Alarm:
		{
			List<config_card_type_judge> source2 = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
			config_card_type_judge config_card_type_judge2 = source2.FirstOrDefault((config_card_type_judge c) => c.IoCardType == "DI211");
			config_card_type_judge config_card_type_judge3 = source2.FirstOrDefault((config_card_type_judge c) => c.IoCardType == "DO211");
			if (config_card_type_judge2 == null)
			{
				throw new Exception("未找到卡件DI211的数量");
			}
			if (config_card_type_judge3 == null)
			{
				throw new Exception("未找到卡件DO211的数量");
			}
			model.Status.Busy("正在添加报警点……");
			foreach (IGrouping<string, IoFullData> item3 in from c in AllData
				group c by c.CabinetNumber)
			{
				if (AllData.Where((IoFullData a) => a.PointType == TagType.Alarm).Count() <= 0)
				{
					AddAlarmPoints(item3.Key, item3.ToList());
					AddControlAlarmPoint(item3.Key, item3.ToList());
				}
			}
			model.Status.Busy("添加报警点完毕……");
			break;
		}
		case TagType.BackUp:
			model.Status.Busy("正在添加备用点……");
			foreach (IGrouping<string, IoFullData> item4 in from d in AllData
				group d by d.CabinetNumber)
			{
				IEnumerable<IGrouping<int, IoFullData>> enumerable = from c in item4.ToList()
					group c by c.Cage;
				foreach (IGrouping<int, IoFullData> item5 in enumerable)
				{
					IEnumerable<IGrouping<int, IoFullData>> enumerable2 = from c in item5.ToList()
						group c by c.Slot;
					foreach (IGrouping<int, IoFullData> item6 in enumerable2)
					{
						List<IoFullData> source = item6.ToList();
						string cardType = item6.FirstOrDefault().CardType;
						if (cardType == null || cardType.Contains("DP") || cardType.Contains("FF"))
						{
							continue;
						}
						config_card_type_judge config_card_type_judge = config_Card_Types.FirstOrDefault((config_card_type_judge c) => c.IoCardType == cardType);
						if (config_card_type_judge == null)
						{
							throw new Exception($"找不到{config_card_type_judge}板卡类型");
						}
						int i;
						for (i = 1; i <= config_card_type_judge.PinsCount; i++)
						{
							if (source.Where((IoFullData l) => l.Channel == i).Count() == 0)
							{
								IoFullData ioFullData = source.FirstOrDefault();
								if (ioFullData != null)
								{
									IoFullData item = new IoFullData
									{
										CabinetNumber = item4.Key,
										SignalPositionNumber = $"{item4.Key}{item5.Key}{item6.Key.ToString("00")}{cardType.Substring(0, 2)}CH{i.ToString("00")}",
										SystemCode = "BEIYONG",
										Cage = item5.Key,
										Slot = item6.Key,
										CardType = cardType,
										Description = "备用",
										Channel = i,
										SubNet = ioFullData.SubNet,
										StationNumber = ioFullData.StationNumber,
										IoType = ioFullData.IoType,
										PowerType = ioFullData.PowerType,
										ElectricalCharacteristics = ioFullData.ElectricalCharacteristics,
										SignalEffectiveMode = ioFullData.SignalEffectiveMode,
										PointType = TagType.BackUp,
										Version = "A",
										ModificationDate = DateTime.Now
									};
									AllData.Add(item);
								}
							}
						}
					}
				}
			}
			model.Status.Busy("添加备用点完毕……");
			break;
		}
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData allDatum in AllData)
		{
			observableCollection.Add(allDatum);
		}
		AllData = observableCollection;
		await Recalc();
		await SaveAndUploadFileAsync();
		model.Status.Reset();
		void AddAlarmPoints(string cabinetName, List<IoFullData> data)
		{
			string[] array = new string[6] { "电源A故障报警", "电源B故障报警", "机柜门开", "温度高报警", "风扇故障", "网络故障" };
			string[] array2 = new string[6] { "PWFA", "PWFB", "DROP", "TEPH", "FAN", "SWF" };
			IoFullData ioFullData2 = data.FirstOrDefault();
			for (int j = 0; j < 6; j++)
			{
				IoFullData item2 = new IoFullData
				{
					CabinetNumber = cabinetName,
					PointType = TagType.Alarm,
					SignalPositionNumber = cabinetName,
					Cage = 0,
					Slot = 0,
					Channel = 0,
					IoType = "DI",
					PowerType = "DI1",
					SubNet = ((ioFullData2 != null) ? ioFullData2.SubNet : "未找到"),
					StationNumber = ((ioFullData2 != null) ? ioFullData2.StationNumber : "未找到"),
					ElectricalCharacteristics = "无源常开",
					SignalEffectiveMode = "NO",
					SystemCode = "JIGUIBAOJING",
					ExtensionCode = array2[j],
					Description = "控制柜" + cabinetName + "机柜" + array[j]
				};
				AllData.Add(item2);
			}
		}
		void AddControlAlarmPoint(string cabinetName, List<IoFullData> data)
		{
			IoFullData ioFullData2 = data.FirstOrDefault();
			IoFullData item2 = new IoFullData
			{
				CabinetNumber = cabinetName,
				PointType = TagType.Alarm,
				Cage = 0,
				Slot = 0,
				Channel = 0,
				SignalPositionNumber = cabinetName,
				SystemCode = "JIGUIBAOJING",
				ExtensionCode = "ALM",
				SubNet = ((ioFullData2 != null) ? ioFullData2.SubNet : "未找到"),
				StationNumber = ((ioFullData2 != null) ? ioFullData2.StationNumber : "未找到"),
				Description = "控制柜" + cabinetName + "机柜报警灯",
				IoType = "DO",
				PowerType = "DO2",
				ElectricalCharacteristics = "有源常闭",
				SignalEffectiveMode = "NO"
			};
			AllData.Add(item2);
		}
	}

	[RelayCommand]
	private async Task DeleteTag(TagType type)
	{
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		model.Status.Busy("正在删除点……");
		AllData.RemoveWhere((IoFullData x) => x.PointType == type);
		await SaveAndUploadFileAsync();
		await Refresh();
		model.Status.Reset();
	}

	[RelayCommand]
	private void EditConfigurationTable(string param)
	{
		configvm.Title = param;
		ConfigTableViewModel configTableViewModel = configvm;
		configTableViewModel.DataType = param switch
		{
			"IO卡型号配置表" => typeof(config_card_type_judge), 
			"TB型号配置表" => typeof(config_terminalboard_type_judge), 
			"接线点配置表" => typeof(config_connection_points), 
			"供电方式配置表" => typeof(config_power_supply_method), 
			"OF显示格式值配置表" => typeof(config_output_format_values), 
			_ => throw new NotImplementedException(), 
		};
		navigation.NavigateWithHierarchy(typeof(ConfigTablePage));
	}

	[RelayCommand]
	private async Task AllocateIO()
	{
		if (AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		if (await message.ConfirmAsync("是否需要为通讯点预留插槽？", "预留插槽设置"))
		{
			List<config_card_type_judge> configs = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
			List<IoFullData> points = AllData.ToList();
			List<StdCabinet> cabinets = points.BuildCabinetStructureOther(configs);
			ReservedSlotConfigWindow reservedSlotConfigWindow = new ReservedSlotConfigWindow(cabinets);
			if (reservedSlotConfigWindow.ShowDialog() == true)
			{
				await message.SuccessAsync("预留插槽设置已保存！");
			}
		}
		model.Status.Busy("正在分配……");
		FormularHelper formularHelper = new FormularHelper();
		List<config_card_type_judge> configs2 = await context.Db.Queryable<config_card_type_judge>().ToListAsync();
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData item in formularHelper.AutoAllocateIO(AllData.ToList(), configs2, (double)RedundancyRate / 100.0))
		{
			observableCollection.Add(item);
		}
		AllData = observableCollection;
		await Recalc();
		await SaveAndUploadFileAsync();
		model.Status.Success("分配完毕！");
	}

	[RelayCommand]
	private void PreviewAllocateIO()
	{
		if (SubProject == null)
		{
			throw new Exception("子项目为空，找不到控制系统");
		}
		ControlSystem controlSystem = (from it in context.Db.Queryable<config_project_major>()
			where it.Id == SubProject.MajorId
			select it).First().ControlSystem;
		parameterService.SetParameter("controlSystem", controlSystem);
		navigation.NavigateWithHierarchy(typeof(CabinetAllocatedPage));
	}

	/// <summary>
	/// FF从站模块自动分配命令
	/// 根据新的输入格式进行FF从站模块的自动分配和通道计算
	/// </summary>
	[RelayCommand]
	private async Task AllocateFFSlaveModules()
	{
		if (AllData == null)
		{
			throw new Exception("无IO数据可分配，请先导入数据");
		}
		try
		{
			model.Status.Busy("正在进行FF从站模块分配...");
			await ShowFFSlaveAllocationReport(await PerformFFSlaveModuleAllocation());
			ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
			foreach (IoFullData allDatum in AllData)
			{
				observableCollection.Add(allDatum);
			}
			AllData = observableCollection;
			await SaveAndUploadFileAsync();
			model.Status.Success("分配完成！");
		}
		catch (Exception ex)
		{
			model.Status.Error("FF从站模块分配失败：" + ex.Message);
			throw;
		}
	}

	/// <summary>
	/// 显示FF从站分配报告对话框
	/// </summary>
	/// <param name="report">分配报告内容</param>
	private async Task ShowFFSlaveAllocationReport(string report)
	{
		ContentDialog dialog = new ContentDialog
		{
			Title = "FF从站模块分配报告",
			Content = CreateReportContent(report),
			CloseButtonText = "关闭",
			DefaultButton = ContentDialogButton.Close
		};
		await dialog.ShowAsync(dialog, CancellationToken.None);
	}

	/// <summary>
	/// 创建报告内容控件
	/// </summary>
	/// <param name="report">报告文本</param>
	/// <returns>报告显示控件</returns>
	private FrameworkElement CreateReportContent(string report)
	{
		ScrollViewer scrollViewer = new ScrollViewer
		{
			Width = 800.0,
			Height = 500.0,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
		};
		Wpf.Ui.Controls.TextBox content = new Wpf.Ui.Controls.TextBox
		{
			Text = report,
			TextWrapping = TextWrapping.Wrap,
			FontFamily = new System.Windows.Media.FontFamily("Consolas"),
			FontSize = 12.0,
			Padding = new Thickness(15.0),
			Background = System.Windows.SystemColors.ControlBrush,
			Margin = new Thickness(5.0),
			IsReadOnly = true,
			BorderThickness = new Thickness(0.0),
			VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
		};
		scrollViewer.Content = content;
		return scrollViewer;
	}

	[RelayCommand]
	private async Task Add()
	{
		if (SubProject == null || AllData == null)
		{
			return;
		}
		IoFullData ioFullData = new IoFullData();
		if (!EditData(ioFullData, "添加"))
		{
			return;
		}
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData item in AllData.Append(ioFullData))
		{
			observableCollection.Add(item);
		}
		AllData = observableCollection;
		await SaveAndUploadFileAsync();
		RefreshFilterOptions();
		FilterAndSort();
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
		if (!EditData(ioFullData, "编辑"))
		{
			return;
		}
		data.CopyPropertiesFrom(ioFullData);
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData displayPoint in DisplayPoints)
		{
			observableCollection.Add(displayPoint);
		}
		DisplayPoints = observableCollection;
		await SaveAndUploadFileAsync();
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
	private void Import()
	{
		navigation.NavigateWithHierarchy(typeof(ImportPage));
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
			DisplayAttribute customAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();
			if (!(customAttribute.GetAutoGenerateField() ?? true))
			{
				continue;
			}
			string name = customAttribute.Name;
			if (propertyInfo.PropertyType == typeof(string))
			{
				editorOptionBuilder.AddProperty<string>(propertyInfo.Name).WithHeader(name).WithPlaceHoplder("请输入" + name);
			}
			else if (propertyInfo.PropertyType == typeof(float))
			{
				editorOptionBuilder.AddProperty<float>(propertyInfo.Name).WithHeader(name).EditAsDouble()
					.ConvertFromProperty((float x) => x)
					.ConvertToProperty((double x) => (float)x);
			}
			else if (propertyInfo.PropertyType == typeof(int))
			{
				editorOptionBuilder.AddProperty<int>(propertyInfo.Name).WithHeader(name).EditAsInt();
			}
			else if (propertyInfo.PropertyType == typeof(int?))
			{
				editorOptionBuilder.AddProperty<int?>(propertyInfo.Name).WithHeader(name).EditAsInt()
					.ConvertFromProperty((int? x) => x.GetValueOrDefault());
			}
			else if (propertyInfo.PropertyType == typeof(DateTime?))
			{
				editorOptionBuilder.AddProperty<DateTime?>(propertyInfo.Name).WithHeader(name).EditAsDateTime()
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
		return database.GetLongLqFields(context);
	}

	/// <summary>
	/// 动态导出命令 - 打开导出配置窗口
	/// </summary>
	[RelayCommand]
	private async Task DynamicExport()
	{
		if (AllData == null)
		{
			model.Status.Warn("没有可导出的数据");
			return;
		}
		try
		{
			ExportConfigWindow exportConfigWindow = new ExportConfigWindow(AllData.ToList(), picker, excel, message, context, cloudExportConfigService, SubProject?.Id);
			exportConfigWindow.Owner = Application.Current.MainWindow;
			if (exportConfigWindow.ShowDialog() == true)
			{
				model.Status.Success("IO清单导出完成");
			}
		}
		catch (Exception ex)
		{
			model.Status.Error("打开导出配置窗口失败：" + ex.Message);
		}
	}

	[RelayCommand]
	private async Task Export(string param)
	{
		if (AllData == null)
		{
			model.Status.Warn("没有可导出的数据");
			return;
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
		model.Status.Busy("正在" + param + "数据……");
		try
		{
			switch (param)
			{
			case "导出主系统板卡清单":
				await ExportMainSystemBoardList(AllData.ToList(), file);
				break;
			case "导出板卡统计信息":
				await ExportBoardStatisticsInfo(AllData.ToList(), file);
				break;
			case "导出龙鳍数据库":
			{
				DepXT2ViewModel depXT2ViewModel3 = this;
				ObservableCollection<IoFullData> observableCollection = AllData;
				List<IoFullData> list3 = new List<IoFullData>(observableCollection.Count);
				list3.AddRange(observableCollection);
				await depXT2ViewModel3.ExportDatabase(list3, file);
				break;
			}
			case "导出端接清单":
				await ExportDuanjie(AllData.ToList(), file);
				break;
			case "导出FF从站箱端接清单":
			{
				DepXT2ViewModel depXT2ViewModel2 = this;
				ObservableCollection<IoFullData> observableCollection = AllData;
				List<IoFullData> list2 = new List<IoFullData>(observableCollection.Count);
				list2.AddRange(observableCollection);
				await depXT2ViewModel2.ExportFFDuanjie(list2, file);
				break;
			}
			case "导出FF总线箱端接清单":
			{
				DepXT2ViewModel depXT2ViewModel = this;
				ObservableCollection<IoFullData> observableCollection = AllData;
				List<IoFullData> list = new List<IoFullData>(observableCollection.Count);
				list.AddRange(observableCollection);
				await depXT2ViewModel.ExportFFZXDuanjie(list, file);
				break;
			}
			}
			model.Status.Success("已成功" + param + "数据：" + file);
		}
		catch (Exception ex)
		{
			model.Status.Error(param + "失败：" + ex.Message);
		}
	}

	private async Task ExportMainSystemBoardList(List<IoFullData> data, string filePath)
	{
		List<xtes_mainsystemboard_list> data2 = Getxtes_mainsystemboard_list(data);
		using DataTable dataTable = await data2.ToTableByPropertyNameAsync();
		await excel.FastExportSheetAsync(dataTable, filePath, "主系统板卡");
	}

	public List<xtes_mainsystemboard_list> Getxtes_mainsystemboard_list(List<IoFullData> data)
	{
		List<xtes_mainsystemboard_list> list = new List<xtes_mainsystemboard_list>();
		List<StdCabinet> list2 = data.BuildCabinetStructureOther(config_Card_Types);
		foreach (StdCabinet item in list2)
		{
			AddControllerAndPowerBoards(list, item);
			foreach (ChassisInfo cage in item.Cages)
			{
				foreach (SlotInfo slot in cage.Slots)
				{
					if (slot.Board != null)
					{
						IoFullData tag = slot.Board.Channels.Select((Xt2Channel c) => c.Point).FirstOrDefault((IoFullData c) => c != null);
						list.Add(CreateBoardListItem(item.Name, cage.Index, slot.Index, slot.Board, tag));
					}
				}
			}
			CompleteUnusedSlots(list, item);
		}
		return (from g in list
			orderby g.机柜编号, g.机架号, g.插槽
			select g).ToList();
	}

	private void AddControllerAndPowerBoards(List<xtes_mainsystemboard_list> list, StdCabinet cabinet)
	{
		int result;
		int num = (int.TryParse((from channel in (from slot in cabinet.Cages.SelectMany((ChassisInfo cage) => cage.Slots)
				where slot.Board != null
				select slot).SelectMany((SlotInfo slot) => slot.Board.Channels)
			select channel.Point).FirstOrDefault((IoFullData point) => point != null && !string.IsNullOrEmpty(point.StationNumber))?.StationNumber, out result) ? result : 0);
		list.AddRange(new xtes_mainsystemboard_list[2]
		{
			new xtes_mainsystemboard_list
			{
				机柜编号 = cabinet.Name,
				机架号 = 1,
				插槽 = 1,
				地址 = num.ToString(),
				板卡类型 = "NP204",
				板卡编号 = cabinet.Name + "101UC",
				描述 = "控制器"
			},
			new xtes_mainsystemboard_list
			{
				机柜编号 = cabinet.Name,
				机架号 = 1,
				插槽 = 2,
				地址 = (num + 128).ToString(),
				板卡类型 = "NP204",
				板卡编号 = cabinet.Name + "102UC",
				描述 = "控制器"
			}
		});
		for (int num2 = 0; num2 < 3; num2++)
		{
			list.Add(new xtes_mainsystemboard_list
			{
				机柜编号 = cabinet.Name,
				机架号 = num2 + 1,
				插槽 = 11,
				地址 = ((num2 == 0) ? "B" : (num2 + "B")),
				板卡类型 = "PM211",
				描述 = "DC24V电源板卡"
			});
			list.Add(new xtes_mainsystemboard_list
			{
				机柜编号 = cabinet.Name,
				机架号 = num2 + 1,
				插槽 = 12,
				地址 = ((num2 == 0) ? "C" : (num2 + "C")),
				板卡类型 = "PM211",
				描述 = "DC24V电源板卡"
			});
		}
	}

	private xtes_mainsystemboard_list CreateBoardListItem(string cabinetName, int cageIndex, int slotIndex, Board board, IoFullData? tag)
	{
		return new xtes_mainsystemboard_list
		{
			机柜编号 = cabinetName,
			机架号 = cageIndex,
			插槽 = slotIndex,
			地址 = GetAddress(cageIndex, slotIndex),
			板卡类型 = GetCardType(cageIndex, slotIndex, board.Type),
			板卡编号 = GetCardNumber(cabinetName, cageIndex, slotIndex, board.Type),
			端子板类型 = GetTerminalBoardType(board.Type, slotIndex, tag),
			端子板编号 = GetTerminalBoardNumber(cageIndex, slotIndex, tag),
			描述 = GetDescription(board.Type)
		};
	}

	private void CompleteUnusedSlots(List<xtes_mainsystemboard_list> list, StdCabinet cabinet)
	{
		List<(int, int)> list2 = (from g in list
			where g.机柜编号 == cabinet.Name
			select (机架号: g.机架号, 插槽: g.插槽)).ToList();
		for (int num = 1; num <= 3; num++)
		{
			for (int num2 = 1; num2 <= 12; num2++)
			{
				if (!list2.Contains((num, num2)))
				{
					list.Add(new xtes_mainsystemboard_list
					{
						机柜编号 = cabinet.Name,
						机架号 = num,
						插槽 = num2,
						地址 = GetAddress(num, num2),
						板卡类型 = "备用",
						描述 = "空插槽"
					});
				}
			}
		}
	}

	private async Task ExportBoardStatisticsInfo(List<IoFullData> data, string filePath)
	{
		List<xtes_mainsystemboard_list> source = Getxtes_mainsystemboard_list(data);
		List<xtes_board_statistics> data2 = (from board in source
			group board by new { board.板卡类型, board.端子板类型 }).Select((group, int index) => new xtes_board_statistics
		{
			序号 = index + 1,
			板卡 = group.Key.板卡类型,
			板卡数量 = group.Count(),
			端子板 = group.Key.端子板类型,
			端子板数量 = group.Count()
		}).ToList();
		using DataTable dataTable = await data2.ToTableByPropertyNameAsync();
		await excel.FastExportSheetAsync(dataTable, filePath, "板卡统计");
	}

	private string GetAddress(int cage, int slot)
	{
		if (cage != 1 || slot >= 3)
		{
			if (slot < 11)
			{
				return ((cage - 1) * 10 + slot).ToString();
			}
			return "--";
		}
		return "";
	}

	private string GetCardType(int cage, int slot, string cardType)
	{
		if (cage == 1)
		{
			if ((uint)(slot - 1) <= 1u)
			{
				return "NP204";
			}
			if ((uint)(slot - 11) <= 1u)
			{
				goto IL_0027;
			}
		}
		else if (slot == 11 || slot == 12)
		{
			goto IL_0027;
		}
		return cardType;
		IL_0027:
		return "PM211";
	}

	private string GetCardNumber(string cabinetNumber, int cage, int slot, string cardType)
	{
		string result = cabinetNumber + GetCardInCabinetNumber(cage, slot, cardType);
		if (cage == 1)
		{
			if ((uint)(slot - 1) <= 1u)
			{
				return $"{cabinetNumber}{cage}{slot:00}UC";
			}
			if ((uint)(slot - 11) <= 1u)
			{
				goto IL_0083;
			}
			if ((uint)(slot - 13) <= 1u)
			{
				goto IL_008b;
			}
		}
		else
		{
			if (slot == 11 || slot == 12)
			{
				goto IL_0083;
			}
			if (slot == 13 || slot == 14)
			{
				goto IL_008b;
			}
		}
		return result;
		IL_008b:
		return $"{cabinetNumber}{cage}{slot:00}PM";
		IL_0083:
		return "--";
	}

	private string GetTerminalBoardType(string cardType, int slot, IoFullData? tag)
	{
		switch (cardType)
		{
		case "MD211":
			return "TB244";
		case "NP204":
		case "MD212":
		case "MD216":
			return "--";
		default:
			if ((uint)(slot - 11) <= 3u)
			{
				return "--";
			}
			return tag?.TerminalBoardModel ?? "--";
		}
	}

	private string GetTerminalBoardNumber(int cage, int slot, IoFullData? tag)
	{
		if (cage == 1)
		{
			if ((uint)(slot - 1) <= 1u)
			{
				return "--";
			}
			if ((uint)(slot - 11) <= 3u)
			{
				goto IL_0031;
			}
		}
		else if (slot == 11 || slot == 12 || slot == 13 || slot == 14)
		{
			goto IL_0031;
		}
		return tag?.TerminalBoardNumber ?? "--";
		IL_0031:
		return "--";
	}

	private string GetDescription(string cardType)
	{
		return cardType switch
		{
			"AI212" => "8通道模拟量输入模块", 
			"FF211" => "FF H1网关通信模块", 
			"DP211" => "PROFIBUS-DP", 
			"AI216" => "8通道带HART功能模拟量输入模块", 
			"AI221" => "8通道热电偶模块", 
			"AI231" => "8通道热电阻模块", 
			"AO211" => "8通道模拟量输出模块", 
			"AO212" => "8通道模拟量输出模块", 
			"DI211" => "16通道开关量输入模块", 
			"DO211" => "16通道开关量输出模块", 
			"NP204" => "控制器", 
			"PM211" => "DC24V电源板卡", 
			"备用" => "", 
			"--" => "总线扩展连接模块", 
			"MD212" => "MODBUS TCP通讯板卡", 
			"MD211" => "Modbus RTU通讯板卡", 
			"PI211" => "脉冲量输入模块", 
			_ => "Unknown", 
		};
	}

	private async Task ExportDuanjie(List<IoFullData> data, string filePath)
	{
		List<xtes_duanjie> data2 = data.Select((IoFullData x) => x.toduanjieData()).ToList();
		using DataTable dataTable = await data2.ToTableByDisplayAttributeAsync();
		await excel.FastExportSheetAsync(dataTable, filePath, "系统二室端接清单");
	}

	private async Task ExportDatabase(IList<IoFullData> data, string filePath)
	{
		IEnumerable<IoFullData> substations = data.Where((IoFullData d) => d.IoType.Contains("AI"));
		IEnumerable<IoFullData> substations2 = data.Where((IoFullData d) => d.CardType.Contains("PI"));
		IEnumerable<IoFullData> substations3 = data.Where((IoFullData d) => d.CardType.Contains("AO"));
		IEnumerable<IoFullData> substations4 = data.Where((IoFullData d) => d.CardType.Contains("DI"));
		IEnumerable<IoFullData> substations5 = data.Where((IoFullData d) => d.CardType.Contains("DO"));
		IEnumerable<IoFullData> substations6 = data.Where((IoFullData d) => d.CardType.Contains("DO"));
		IEnumerable<IoFullData> substations7 = data.Where((IoFullData d) => d.CardType.Contains("AO"));
		new List<xtes_GST>();
		new List<xtes_GKC>();
		Task<DataTable> task = new FormularHelper().ConvertToAviList(substations).ToTableByDisplayAttributeAsync();
		Task<DataTable> task2 = new FormularHelper().ConvertToPviList(substations2).ToTableByDisplayAttributeAsync();
		Task<DataTable> task3 = new FormularHelper().ConvertToAvoList(substations3).ToTableByDisplayAttributeAsync();
		Task<DataTable> task4 = new FormularHelper().ConvertToDviList(substations4).ToTableByDisplayAttributeAsync();
		Task<DataTable> task5 = new FormularHelper().ConvertToDvoList(substations5).ToTableByDisplayAttributeAsync();
		Task<DataTable> task6 = new FormularHelper().ConvertToGBPList(substations6).ToTableByDisplayAttributeAsync();
		Task<DataTable> task7 = new FormularHelper().ConvertToGCPList(substations7).ToTableByDisplayAttributeAsync();
		Task<DataTable> task8 = new FormularHelper().ConvertToGSTList(data).ToTableByDisplayAttributeAsync();
		Task<DataTable> task9 = new FormularHelper().ConvertToGKCList(data).ToTableByDisplayAttributeAsync();
		Task<DataTable> gkcTask = task9;
		Task<DataTable> gstTask = task8;
		Task<DataTable> gcpTask = task7;
		Task<DataTable> gbpTask = task6;
		Task<DataTable> dvoTask = task5;
		Task<DataTable> dviTask = task4;
		Task<DataTable> avoTask = task3;
		Task<DataTable> pviTask = task2;
		Task<DataTable> task10 = task;
		DataTable dataTable = await task10;
		DataTable dataTable2 = await pviTask;
		DataTable dataTable3 = await avoTask;
		DataTable dataTable4 = await dviTask;
		DataTable dataTable5 = await dvoTask;
		DataTable dataTable6 = await gbpTask;
		DataTable dataTable7 = await gcpTask;
		DataTable dataTable8 = await gstTask;
		DataTable gkc = await gkcTask;
		DataTable gst = dataTable8;
		DataTable gcp = dataTable7;
		DataTable gbp = dataTable6;
		DataTable dvo = dataTable5;
		DataTable dvi = dataTable4;
		DataTable avo = dataTable3;
		DataTable pvi = dataTable2;
		DataTable avi = dataTable;
		await Task.Run(delegate
		{
			Workbook wb = excel.GetWorkbook();
			Worksheet worksheet = wb.Worksheets.FirstOrDefault((Worksheet ws) => ws.Name == "Sheet1");
			if (worksheet != null)
			{
				wb.Worksheets.RemoveAt("Sheet1");
			}
			writeData(avi, "avi".ToUpper());
			writeData(pvi, "pvi".ToUpper());
			writeData(avo, "avo".ToUpper());
			writeData(dvi, "dvi".ToUpper());
			writeData(dvo, "dvo".ToUpper());
			writeData(gbp, "gbp".ToUpper());
			writeData(gcp, "gcp".ToUpper());
			writeData(gst, "gst".ToUpper());
			writeData(gkc, "gkc".ToUpper());
			wb.Save(filePath);
			void writeData(DataTable dataTable9, string name)
			{
				Worksheet worksheet2 = wb.Worksheets.Add(name);
				Cells cells = worksheet2.Cells;
				cells.ImportData(dataTable9, 0, 0, new ImportTableOptions
				{
					IsFieldNameShown = true
				});
				dataTable9.Dispose();
			}
		});
	}

	/// <summary>
	/// 导出FF从站箱端接表
	/// </summary>
	private async Task ExportFFDuanjie(IList<IoFullData> data, string filePath)
	{
		List<IoFullData> ffSlaveSignals = data.Where((IoFullData d) => d.SensorType != null && !string.IsNullOrEmpty(d.LocalBoxNumber) && d.SensorType.Contains("从站")).ToList();
		IList<IGrouping<string, IoFullData>> list = SortByCascadeOrder(ffSlaveSignals);
		List<FF从战箱端接表> exportDataList = new List<FF从战箱端接表>();
		List<string> source = list.Select((IGrouping<string, IoFullData> g) => g.Key).ToList();
		foreach (IGrouping<string, IoFullData> boxGroup in list)
		{
			int num = 1;
			int num2 = 7;
			string localBoxNumber = boxGroup.Key;
			string leftBoxNumber = source.FirstOrDefault((string box) => box != localBoxNumber && ffSlaveSignals.Any((IoFullData signal) => signal.LocalBoxNumber == box && signal.Remarks.Contains("串接") && signal.Remarks.Contains(localBoxNumber)));
			string rightBoxNumber = source.FirstOrDefault((string box) => box != localBoxNumber && boxGroup.Any((IoFullData signal) => !string.IsNullOrEmpty(signal.Remarks) && signal.Remarks.Contains("串接") && signal.Remarks.Contains(box)));
			List<FF从战箱端接表> collection = AddPowerSupplySignals(localBoxNumber, boxGroup.FirstOrDefault().CabinetNumber);
			exportDataList.AddRange(collection);
			List<FF从战箱端接表> collection2 = AddFFConnectionSignals(boxGroup.FirstOrDefault().CabinetNumber, leftBoxNumber, rightBoxNumber, boxGroup);
			exportDataList.AddRange(collection2);
			foreach (IoFullData item7 in boxGroup.OrderBy((IoFullData b) => b.FFSlaveModuleID))
			{
				FF从战箱端接表 item = new FF从战箱端接表
				{
					LocalBoxNumber = localBoxNumber,
					TerminalBlockNumber = item7.FFSlaveModuleID,
					Terminal = item7.FFSlaveModuleSignalPositive,
					SignalFunction = item7.Description,
					SignalPositionNumber = item7.SignalPositionNumber,
					CableDestination = item7.TagName
				};
				FF从战箱端接表 item2 = new FF从战箱端接表
				{
					LocalBoxNumber = localBoxNumber,
					TerminalBlockNumber = item7.FFSlaveModuleID,
					Terminal = item7.FFSlaveModuleSignalNegative,
					SignalFunction = item7.Description,
					SignalPositionNumber = item7.SignalPositionNumber,
					CableDestination = item7.TagName
				};
				exportDataList.Add(item);
				exportDataList.Add(item2);
				if (item7.PowerSupplyMethod.Contains("DCS") && item7.PowerSupplyMethod.Contains("220V"))
				{
					FF从战箱端接表 item3 = new FF从战箱端接表
					{
						LocalBoxNumber = localBoxNumber,
						TerminalBlockNumber = "102BN",
						Terminal = num++.ToString(),
						SignalFunction = "220V供电L",
						SignalPositionNumber = item7.SignalPositionNumber,
						CableDestination = item7.TagName
					};
					FF从战箱端接表 item4 = new FF从战箱端接表
					{
						LocalBoxNumber = localBoxNumber,
						TerminalBlockNumber = "102BN",
						Terminal = num++.ToString(),
						SignalFunction = "220V供电N",
						SignalPositionNumber = item7.SignalPositionNumber,
						CableDestination = item7.TagName
					};
					exportDataList.Add(item3);
					exportDataList.Add(item4);
				}
				if (item7.PowerSupplyMethod.Contains("DCS") && item7.PowerSupplyMethod.Contains("24V"))
				{
					FF从战箱端接表 item5 = new FF从战箱端接表
					{
						LocalBoxNumber = localBoxNumber,
						TerminalBlockNumber = "103BN",
						Terminal = num2++.ToString(),
						SignalFunction = "24V供电L",
						SignalPositionNumber = item7.SignalPositionNumber,
						CableDestination = item7.TagName
					};
					FF从战箱端接表 item6 = new FF从战箱端接表
					{
						LocalBoxNumber = localBoxNumber,
						TerminalBlockNumber = "103BN",
						Terminal = num2++.ToString(),
						SignalFunction = "24V供电N",
						SignalPositionNumber = item7.SignalPositionNumber,
						CableDestination = item7.TagName
					};
					exportDataList.Add(item5);
					exportDataList.Add(item6);
				}
			}
		}
		exportDataList = exportDataList.Select(delegate(FF从战箱端接表 fF从战箱端接表, int index)
		{
			fF从战箱端接表.SerialNumber = index + 1;
			return fF从战箱端接表;
		}).ToList();
		model.Status.Busy("正在下载电缆模板……");
		string cableLacalPath = await storage.DownloadtemplatesDepFileAsync("FF端子接线箱端接表模板.xlsx");
		DataTable dataTable = await exportDataList.ToTableByDisplayAttributeAsync();
		Workbook workbook = new Workbook(cableLacalPath);
		Worksheet worksheet = workbook.Worksheets[0];
		Cells cells = worksheet.Cells;
		cells.ImportData(dataTable, 2, 0, new ImportTableOptions
		{
			IsFieldNameShown = false
		});
		Aspose.Cells.Style style = workbook.CreateStyle();
		style.Pattern = BackgroundType.Solid;
		style.ForegroundColor = System.Drawing.Color.Yellow;
		string value = string.Empty;
		for (int num3 = 2; num3 < cells.MaxDataRow + 1; num3++)
		{
			string stringValue = cells[num3, 1].StringValue;
			if (!stringValue.Equals(value))
			{
				for (int num4 = 0; num4 < dataTable.Columns.Count; num4++)
				{
					worksheet.Cells[num3, num4].SetStyle(style);
				}
			}
			value = stringValue;
		}
		workbook.Save(filePath);
		model.Status.Success("已成功导出到" + filePath);
	}

	/// <summary>
	/// 添加供电信号
	/// </summary>
	/// <param name="exportDataList">导出的数据列表</param>
	/// <param name="localBoxNumber">就地箱号</param>
	private List<FF从战箱端接表> AddPowerSupplySignals(string localBoxNumber, string cabinetName)
	{
		List<FF从战箱端接表> list = new List<FF从战箱端接表>();
		list.Add(new FF从战箱端接表
		{
			LocalBoxNumber = localBoxNumber,
			TerminalBlockNumber = "101BN",
			Terminal = "01",
			SignalFunction = "箱体供电L端",
			SignalPositionNumber = "L",
			CableDestination = cabinetName
		});
		list.Add(new FF从战箱端接表
		{
			LocalBoxNumber = localBoxNumber,
			TerminalBlockNumber = "101BN",
			Terminal = "02",
			SignalFunction = "箱体供电N端",
			SignalPositionNumber = "N",
			CableDestination = cabinetName
		});
		return list;
	}

	/// <summary>
	/// 添加FF接线信号
	/// </summary>
	/// <param name="exportDataList">导出的数据列表</param>
	/// <param name="boxGroup">当前就地箱分组</param>
	/// <param name="isCascaded">是否串接</param>
	/// <param name="cascadeTargetBoxNumber">串接目标箱号</param>
	private List<FF从战箱端接表> AddFFConnectionSignals(string cabinetName, string leftBoxNumber, string rightBoxNumber, IGrouping<string, IoFullData> boxGroup)
	{
		List<FF从战箱端接表> list = new List<FF从战箱端接表>();
		string key = boxGroup.Key;
		IOrderedEnumerable<IGrouping<string, IoFullData>> source = from b in boxGroup
			group b by b.FFSlaveModuleID into g
			orderby g.Key
			select g;
		if (source.Count() > 2)
		{
			throw new Exception($"{key}有{source.Count()}个模块编号，大于2个，错误");
		}
		IGrouping<string, IoFullData> grouping = source.First();
		IGrouping<string, IoFullData> grouping2 = source.Last();
		list.Add(new FF从战箱端接表
		{
			LocalBoxNumber = key,
			TerminalBlockNumber = grouping.Key,
			Terminal = "FF+",
			SignalFunction = "FF接口+",
			SignalPositionNumber = "FF+",
			CableDestination = (string.IsNullOrEmpty(leftBoxNumber) ? cabinetName : leftBoxNumber)
		});
		list.Add(new FF从战箱端接表
		{
			LocalBoxNumber = key,
			TerminalBlockNumber = grouping.Key,
			Terminal = "FF-",
			SignalFunction = "FF接口-",
			SignalPositionNumber = "FF-",
			CableDestination = (string.IsNullOrEmpty(leftBoxNumber) ? cabinetName : leftBoxNumber)
		});
		if (!string.IsNullOrEmpty(rightBoxNumber) && source.Count() == 2)
		{
			list.Add(new FF从战箱端接表
			{
				LocalBoxNumber = key,
				TerminalBlockNumber = grouping2.Key,
				Terminal = "FF+",
				SignalFunction = "FF接口+",
				SignalPositionNumber = "FF+",
				CableDestination = rightBoxNumber
			});
			list.Add(new FF从战箱端接表
			{
				LocalBoxNumber = key,
				TerminalBlockNumber = grouping2.Key,
				Terminal = "FF-",
				SignalFunction = "FF接口-",
				SignalPositionNumber = "FF-",
				CableDestination = rightBoxNumber
			});
		}
		return list;
	}

	/// <summary>
	/// 根据箱子的串接顺序排序
	/// </summary>
	private IList<IGrouping<string, IoFullData>> SortByCascadeOrder(IList<IoFullData> data)
	{
		Dictionary<string, IGrouping<string, IoFullData>> groupedByBox = (from f in data
			group f by f.LocalBoxNumber).ToDictionary((IGrouping<string, IoFullData> g) => g.Key);
		Dictionary<string, string> cascadeMap = groupedByBox.ToDictionary<KeyValuePair<string, IGrouping<string, IoFullData>>, string, string>((KeyValuePair<string, IGrouping<string, IoFullData>> g) => g.Key, (KeyValuePair<string, IGrouping<string, IoFullData>> g) => groupedByBox.Keys.FirstOrDefault((string key) => g.Value.Any((IoFullData x) => !string.IsNullOrEmpty(x.Remarks) && x.Remarks.Contains("串接") && x.Remarks.Contains(key))));
		HashSet<string> visited = new HashSet<string>();
		List<IGrouping<string, IoFullData>> sortedGroups = new List<IGrouping<string, IoFullData>>();
		foreach (string key in groupedByBox.Keys)
		{
			ProcessChain(key);
		}
		return sortedGroups;
		void ProcessChain(string box)
		{
			if (!visited.Contains(box))
			{
				List<string> list = (from kvp in cascadeMap
					where kvp.Value == box
					select kvp.Key).ToList();
				foreach (string item in list)
				{
					ProcessChain(item);
				}
				if (!visited.Contains(box))
				{
					sortedGroups.Add(groupedByBox[box]);
					visited.Add(box);
				}
				string text = cascadeMap[box];
				if (!string.IsNullOrEmpty(text))
				{
					ProcessChain(text);
				}
			}
		}
	}

	/// <summary>
	/// 导出FF总线箱端接表
	/// </summary>
	private async Task ExportFFZXDuanjie(IList<IoFullData> data, string filePath)
	{
		try
		{
			if (data == null || !data.Any())
			{
				throw new ArgumentException("输入数据为空，无法生成FF总线箱端接表");
			}
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("文件路径不能为空");
			}
			List<IoFullData> data2 = data.JsonClone().ToList();
			List<IoFullData> list = FilterFFSignals(data2);
			if (!list.Any())
			{
				throw new InvalidOperationException("未找到供电类型包含FF1~FF6的信号");
			}
			Dictionary<string, BoxCascadeInfo> cascadeRelations = AnalyzeCascadeRelations(list);
			List<IGrouping<string, IoFullData>> boxGroups = GroupAndSortBoxes(list);
			List<FF总线箱端接表> exportData = GenerateConnectionData(boxGroups, cascadeRelations);
			await ExportToExcel(exportData, filePath);
			model.Status.Success("已成功导出到" + filePath);
		}
		catch (Exception ex)
		{
			model.Status.Error("导出FF总线箱端接表失败：" + ex.Message);
			throw;
		}
	}

	/// <summary>
	/// 筛选FF总线箱信号
	/// </summary>
	private List<IoFullData> FilterFFSignals(List<IoFullData> data)
	{
		string[] ffTypes = new string[6] { "FF1", "FF2", "FF3", "FF4", "FF5", "FF6" };
		return data.Where((IoFullData d) => ffTypes.Any((string ff) => d.PowerType != null && !string.IsNullOrEmpty(d.LocalBoxNumber) && d.PowerType.Contains(ff))).ToList();
	}

	/// <summary>
	/// 分析串接关系
	/// </summary>
	private Dictionary<string, BoxCascadeInfo> AnalyzeCascadeRelations(List<IoFullData> ffSignals)
	{
		Dictionary<string, BoxCascadeInfo> dictionary = new Dictionary<string, BoxCascadeInfo>();
		List<string> list = (from s in ffSignals
			where !string.IsNullOrEmpty(s.LocalBoxNumber)
			select s.LocalBoxNumber).Distinct().ToList();
		foreach (string item in list)
		{
			dictionary[item] = new BoxCascadeInfo();
		}
		IEnumerable<IGrouping<string, IoFullData>> enumerable = from s in ffSignals
			group s by s.LocalBoxNumber;
		foreach (IGrouping<string, IoFullData> item2 in enumerable)
		{
			string key = item2.Key;
			List<IoFullData> source = item2.ToList();
			string cascadeRemark = (from s in source
				where !string.IsNullOrEmpty(s.Remarks) && s.Remarks.Contains("串")
				select s.Remarks).FirstOrDefault();
			if (!string.IsNullOrEmpty(cascadeRemark))
			{
				List<string> list2 = FindTargetBox(cascadeRemark, key, list);
				if (!list2.Contains(key))
				{
					list2.Insert(0, key);
				}
				list2 = list2.OrderBy((string t) => cascadeRemark.IndexOf(t)).ToList();
				for (int num = 0; num < list2.Count - 1; num++)
				{
					dictionary[list2[num]].RightBox = list2[num + 1];
					dictionary[list2[num]].HasRightCascade = true;
					BoxCascadeInfo boxCascadeInfo = dictionary[list2[num]];
				}
				for (int num2 = list2.Count - 1; num2 > 0; num2--)
				{
					dictionary[list2[num2]].LeftBox = list2[num2 - 1];
					dictionary[list2[num2]].HasLeftCascade = true;
					BoxCascadeInfo boxCascadeInfo2 = dictionary[list2[num2]];
				}
			}
		}
		return dictionary;
	}

	/// <summary>
	/// 从备注中找到目标箱子
	/// </summary>
	private List<string> FindTargetBox(string remark, string currentBox, List<string> allBoxes)
	{
		List<string> list = new List<string>();
		foreach (string allBox in allBoxes)
		{
			if (!string.IsNullOrEmpty(allBox) && remark.Contains(allBox))
			{
				list.Add(allBox);
			}
		}
		return list;
	}

	/// <summary>
	/// 按箱号分组并排序
	/// </summary>
	private List<IGrouping<string, IoFullData>> GroupAndSortBoxes(List<IoFullData> ffSignals)
	{
		return (from s in ffSignals
			group s by s.LocalBoxNumber into g
			orderby g.Key
			select g).ToList();
	}

	/// <summary>
	/// 生成端接表数据
	/// </summary>
	private List<FF总线箱端接表> GenerateConnectionData(List<IGrouping<string, IoFullData>> boxGroups, Dictionary<string, BoxCascadeInfo> cascadeRelations)
	{
		List<FF总线箱端接表> list = new List<FF总线箱端接表>();
		int num = 1;
		foreach (IGrouping<string, IoFullData> boxGroup in boxGroups)
		{
			string key = boxGroup.Key;
			List<IoFullData> list2 = boxGroup.ToList();
			string cabinetNumber = list2.FirstOrDefault()?.CabinetNumber ?? "";
			BoxCascadeInfo valueOrDefault = cascadeRelations.GetValueOrDefault(key, new BoxCascadeInfo());
			List<FF总线箱端接表> list3 = GenerateMainSignals(key, cabinetNumber, valueOrDefault);
			foreach (FF总线箱端接表 item in list3)
			{
				item.SerialNumber = num++;
				list.Add(item);
			}
			List<FF总线箱端接表> list4 = GenerateBranchSignals(key, list2);
			foreach (FF总线箱端接表 item2 in list4)
			{
				item2.SerialNumber = num++;
				list.Add(item2);
			}
		}
		return list;
	}

	/// <summary>
	/// 生成主干信号
	/// </summary>
	private List<FF总线箱端接表> GenerateMainSignals(string boxNumber, string cabinetNumber, BoxCascadeInfo cascadeInfo)
	{
		List<FF总线箱端接表> list = new List<FF总线箱端接表>();
		string cableDestination = (cascadeInfo.HasRightCascade ? cascadeInfo.RightBox : cabinetNumber);
		list.Add(new FF总线箱端接表
		{
			LocalBoxNumber = boxNumber,
			TerminalBlockNumber = "001FI",
			Terminal = "A",
			SignalFunction = "主干信号+",
			SignalPositionNumber = "R",
			CableDestination = cableDestination
		});
		list.Add(new FF总线箱端接表
		{
			LocalBoxNumber = boxNumber,
			TerminalBlockNumber = "001FI",
			Terminal = "B",
			SignalFunction = "主干信号-",
			SignalPositionNumber = "IV",
			CableDestination = cableDestination
		});
		list.Add(new FF总线箱端接表
		{
			LocalBoxNumber = boxNumber,
			TerminalBlockNumber = "IE-BUS",
			Terminal = "/",
			SignalFunction = "主干信号屏蔽",
			SignalPositionNumber = "",
			CableDestination = cableDestination
		});
		if (cascadeInfo.HasLeftCascade)
		{
			list.Add(new FF总线箱端接表
			{
				LocalBoxNumber = boxNumber,
				TerminalBlockNumber = "001BN",
				Terminal = "T1+",
				SignalFunction = "主干信号+",
				SignalPositionNumber = "R",
				CableDestination = cascadeInfo.LeftBox
			});
			list.Add(new FF总线箱端接表
			{
				LocalBoxNumber = boxNumber,
				TerminalBlockNumber = "001BN",
				Terminal = "T1-",
				SignalFunction = "主干信号-",
				SignalPositionNumber = "IV",
				CableDestination = cascadeInfo.LeftBox
			});
		}
		return list;
	}

	/// <summary>
	/// 生成分支信号
	/// </summary>
	private List<FF总线箱端接表> GenerateBranchSignals(string boxNumber, List<IoFullData> signals)
	{
		List<FF总线箱端接表> list = new List<FF总线箱端接表>();
		List<IGrouping<string, IoFullData>> list2 = (from s in signals
			where !string.IsNullOrWhiteSpace(s.TagName)
			group s by s.TagName into g
			orderby g.Key
			select g).ToList();
		int num = 1;
		foreach (IGrouping<string, IoFullData> item in list2)
		{
			string key = item.Key;
			list.Add(new FF总线箱端接表
			{
				LocalBoxNumber = boxNumber,
				TerminalBlockNumber = "001BN",
				Terminal = $"{num}+",
				SignalFunction = $"{num}分支信号+",
				SignalPositionNumber = "R",
				CableDestination = key
			});
			list.Add(new FF总线箱端接表
			{
				LocalBoxNumber = boxNumber,
				TerminalBlockNumber = "001BN",
				Terminal = $"{num}-",
				SignalFunction = $"{num}分支信号-",
				SignalPositionNumber = "IV",
				CableDestination = key
			});
			list.Add(new FF总线箱端接表
			{
				LocalBoxNumber = boxNumber,
				TerminalBlockNumber = "001BN",
				Terminal = $"{num}S",
				SignalFunction = $"{num}分支信号屏蔽",
				SignalPositionNumber = "",
				CableDestination = key
			});
			num++;
		}
		return list;
	}

	/// <summary>
	/// 导出到Excel
	/// </summary>
	private async Task ExportToExcel(List<FF总线箱端接表> exportData, string filePath)
	{
		_ = 1;
		try
		{
			model.Status.Busy("正在下载模板...");
			string templatePath = await storage.DownloadtemplatesDepFileAsync("FF端子接线箱端接表模板.xlsx");
			if (!File.Exists(templatePath))
			{
				throw new FileNotFoundException("模板文件未找到");
			}
			DataTable dataTable = await exportData.ToTableByDisplayAttributeAsync();
			Workbook workbook = new Workbook(templatePath);
			Worksheet worksheet = workbook.Worksheets[0];
			Cells cells = worksheet.Cells;
			cells.ImportData(dataTable, 2, 0, new ImportTableOptions
			{
				IsFieldNameShown = false
			});
			Aspose.Cells.Style style = workbook.CreateStyle();
			style.Pattern = BackgroundType.Solid;
			style.ForegroundColor = System.Drawing.Color.Yellow;
			string value = string.Empty;
			for (int i = 2; i < cells.MaxDataRow + 1; i++)
			{
				string stringValue = cells[i, 1].StringValue;
				if (!stringValue.Equals(value))
				{
					for (int j = 0; j < dataTable.Columns.Count; j++)
					{
						worksheet.Cells[i, j].SetStyle(style);
					}
				}
				value = stringValue;
			}
			workbook.Save(filePath);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException("导出Excel文件失败：" + ex.Message, ex);
		}
	}

	private void InitializeFilters()
	{
		List<ExcelFilter> list = new List<ExcelFilter>();
		List<PropertyInfo> list2 = (from p in typeof(IoFullData).GetProperties()
			where p.GetCustomAttribute<DisplayAttribute>() != null
			orderby p.GetCustomAttribute<DisplayAttribute>()?.GetOrder() ?? 999
			select p).ToList();
		foreach (PropertyInfo item in list2)
		{
			string title = item.GetCustomAttribute<DisplayAttribute>()?.Name ?? item.Name;
			list.Add(new ExcelFilter(title));
		}
		Filters = ImmutableList.Create(new ReadOnlySpan<ExcelFilter>(list.ToArray()));
	}

	[RelayCommand]
	private async Task ClearAllFilterOptions()
	{
		if (await message.ConfirmAsync("确认重置全部筛选条件"))
		{
			isRefreshingOptions = true;
			Filters.AllDo(delegate(ExcelFilter x)
			{
				x.ClearAll();
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
			Filters.ForEach(delegate(ExcelFilter x)
			{
				x.ClearAll();
			});
		}
		else
		{
			Dictionary<string, PropertyInfo> dictionary = (from p in typeof(IoFullData).GetProperties()
				where p.GetCustomAttribute<DisplayAttribute>() != null
				select p).ToDictionary((PropertyInfo p) => p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name, (PropertyInfo p) => p);
			foreach (ExcelFilter filter in Filters)
			{
				if (!dictionary.TryGetValue(filter.Title, out var prop))
				{
					continue;
				}
				IEnumerable<string> options = AllData.Select(delegate(IoFullData x)
				{
					object value = prop.GetValue(x);
					if (value == null)
					{
						return string.Empty;
					}
					return (value is DateTime dateTime) ? dateTime.ToString("d") : (value.ToString() ?? string.Empty);
				});
				filter.SetOptions(options);
			}
		}
		isRefreshingOptions = false;
	}

	[RelayCommand]
	public void FilterAndSort()
	{
		if (isRefreshingOptions || AllData == null)
		{
			return;
		}
		IEnumerable<IoFullData> enumerable = AllData.AsEnumerable();
		Dictionary<string, PropertyInfo> dictionary = (from p in typeof(IoFullData).GetProperties()
			where p.GetCustomAttribute<DisplayAttribute>() != null
			select p).ToDictionary((PropertyInfo p) => p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name, (PropertyInfo p) => p);
		foreach (ExcelFilter filter in Filters)
		{
			HashSet<string> selectedValues = filter.GetSelectedValues();
			if (selectedValues.Count > 0 && selectedValues.Count < filter.AllOptions.Count && dictionary.TryGetValue(filter.Title, out var prop))
			{
				enumerable = enumerable.Where(delegate(IoFullData x)
				{
					object value = prop.GetValue(x);
					string item = ((value == null) ? string.Empty : ((value is DateTime dateTime) ? dateTime.ToString("d") : (value.ToString() ?? string.Empty)));
					return selectedValues.Contains(item);
				});
			}
		}
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData item2 in Sort(enumerable))
		{
			observableCollection.Add(item2);
		}
		DisplayPoints = observableCollection;
	}

	private IEnumerable<IoFullData> Sort(IEnumerable<IoFullData> data)
	{
		if (SortOption1 == "全部")
		{
			return data;
		}
		IOrderedEnumerable<IoFullData> orderedEnumerable;
		if (SortOption1 == "机柜号" && SortType1 == "升序")
		{
			orderedEnumerable = data.OrderBy((IoFullData x) => x.CabinetNumber);
		}
		else if (SortOption1 == "机柜号" && SortType1 == "降序")
		{
			orderedEnumerable = data.OrderByDescending((IoFullData x) => x.CabinetNumber);
		}
		else if (SortOption1 == "机架" && SortType1 == "升序")
		{
			orderedEnumerable = data.OrderBy((IoFullData x) => x.Cage);
		}
		else if (SortOption1 == "机架" && SortType1 == "降序")
		{
			orderedEnumerable = data.OrderByDescending((IoFullData x) => x.Cage);
		}
		else if (SortOption1 == "插槽" && SortType1 == "升序")
		{
			orderedEnumerable = data.OrderBy((IoFullData x) => x.Slot);
		}
		else if (SortOption1 == "插槽" && SortType1 == "降序")
		{
			orderedEnumerable = data.OrderByDescending((IoFullData x) => x.Slot);
		}
		else if (SortOption1 == "通道" && SortType1 == "升序")
		{
			orderedEnumerable = data.OrderBy((IoFullData x) => x.Channel);
		}
		else if (SortOption1 == "通道" && SortType1 == "降序")
		{
			orderedEnumerable = data.OrderByDescending((IoFullData x) => x.Channel);
		}
		else if (SortOption1 == "FF/DP端子通道" && SortType1 == "升序")
		{
			orderedEnumerable = data.OrderBy((IoFullData x) => x.FFTerminalChannel);
		}
		else
		{
			if (!(SortOption1 == "FF/DP端子通道") || !(SortType1 == "降序"))
			{
				throw new Exception("开发人员注意");
			}
			orderedEnumerable = data.OrderByDescending((IoFullData x) => x.FFTerminalChannel);
		}
		if (SortOption2 == "全部")
		{
			return orderedEnumerable;
		}
		if (SortOption2 == "机柜号" && SortType2 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.CabinetNumber);
		}
		else if (SortOption2 == "机柜号" && SortType2 == "降序")
		{
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.CabinetNumber);
		}
		else if (SortOption2 == "机架" && SortType2 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.Cage);
		}
		else if (SortOption2 == "机架" && SortType2 == "降序")
		{
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.Cage);
		}
		else if (SortOption2 == "插槽" && SortType2 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.Slot);
		}
		else if (SortOption2 == "插槽" && SortType2 == "降序")
		{
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.Slot);
		}
		else if (SortOption2 == "通道" && SortType2 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.Channel);
		}
		else if (SortOption2 == "通道" && SortType2 == "降序")
		{
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.Channel);
		}
		else if (SortOption2 == "FF/DP端子通道" && SortType2 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.FFTerminalChannel);
		}
		else
		{
			if (!(SortOption2 == "FF/DP端子通道") || !(SortType2 == "降序"))
			{
				throw new Exception("开发人员注意");
			}
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.FFTerminalChannel);
		}
		if (SortOption3 == "全部")
		{
			return orderedEnumerable;
		}
		if (SortOption3 == "机柜号" && SortType3 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.CabinetNumber);
		}
		else if (SortOption3 == "机柜号" && SortType3 == "降序")
		{
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.CabinetNumber);
		}
		else if (SortOption3 == "机架" && SortType3 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.Cage);
		}
		else if (SortOption3 == "机架" && SortType3 == "降序")
		{
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.Cage);
		}
		else if (SortOption3 == "插槽" && SortType3 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.Slot);
		}
		else if (SortOption3 == "插槽" && SortType3 == "降序")
		{
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.Slot);
		}
		else if (SortOption3 == "通道" && SortType3 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.Channel);
		}
		else if (SortOption3 == "通道" && SortType3 == "降序")
		{
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.Channel);
		}
		else if (SortOption3 == "FF/DP端子通道" && SortType3 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.FFTerminalChannel);
		}
		else
		{
			if (!(SortOption3 == "FF/DP端子通道") || !(SortType3 == "降序"))
			{
				throw new Exception("开发人员注意");
			}
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.FFTerminalChannel);
		}
		if (SortOption4 == "全部")
		{
			return orderedEnumerable;
		}
		if (SortOption4 == "机柜号" && SortType4 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.CabinetNumber);
		}
		else if (SortOption4 == "机柜号" && SortType4 == "降序")
		{
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.CabinetNumber);
		}
		else if (SortOption4 == "机架" && SortType4 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.Cage);
		}
		else if (SortOption4 == "机架" && SortType4 == "降序")
		{
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.Cage);
		}
		else if (SortOption4 == "插槽" && SortType4 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.Slot);
		}
		else if (SortOption4 == "插槽" && SortType4 == "降序")
		{
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.Slot);
		}
		else if (SortOption4 == "通道" && SortType4 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.Channel);
		}
		else if (SortOption4 == "通道" && SortType4 == "降序")
		{
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.Channel);
		}
		else if (SortOption4 == "FF/DP端子通道" && SortType4 == "升序")
		{
			orderedEnumerable = orderedEnumerable.ThenBy((IoFullData x) => x.FFTerminalChannel);
		}
		else
		{
			if (!(SortOption4 == "FF/DP端子通道") || !(SortType4 == "降序"))
			{
				throw new Exception("开发人员注意");
			}
			orderedEnumerable = orderedEnumerable.ThenByDescending((IoFullData x) => x.FFTerminalChannel);
		}
		if (SortOption5 == "全部")
		{
			return orderedEnumerable;
		}
		if (SortOption5 == "机柜号" && SortType5 == "升序")
		{
			return orderedEnumerable.ThenBy((IoFullData x) => x.CabinetNumber);
		}
		if (SortOption5 == "机柜号" && SortType5 == "降序")
		{
			return orderedEnumerable.ThenByDescending((IoFullData x) => x.CabinetNumber);
		}
		if (SortOption5 == "机架" && SortType5 == "升序")
		{
			return orderedEnumerable.ThenBy((IoFullData x) => x.Cage);
		}
		if (SortOption5 == "机架" && SortType5 == "降序")
		{
			return orderedEnumerable.ThenByDescending((IoFullData x) => x.Cage);
		}
		if (SortOption5 == "插槽" && SortType5 == "升序")
		{
			return orderedEnumerable.ThenBy((IoFullData x) => x.Slot);
		}
		if (SortOption5 == "插槽" && SortType5 == "降序")
		{
			return orderedEnumerable.ThenByDescending((IoFullData x) => x.Slot);
		}
		if (SortOption5 == "通道" && SortType5 == "升序")
		{
			return orderedEnumerable.ThenBy((IoFullData x) => x.Channel);
		}
		if (SortOption5 == "通道" && SortType5 == "降序")
		{
			return orderedEnumerable.ThenByDescending((IoFullData x) => x.Channel);
		}
		if (SortOption5 == "FF/DP端子通道" && SortType5 == "升序")
		{
			return orderedEnumerable.ThenBy((IoFullData x) => x.FFTerminalChannel);
		}
		if (SortOption5 == "FF/DP端子通道" && SortType5 == "降序")
		{
			return orderedEnumerable.ThenByDescending((IoFullData x) => x.FFTerminalChannel);
		}
		throw new Exception("开发人员注意");
	}

	private void SetPermissionBySubProject()
	{
		if (SubProject != null)
		{
			CanEdit = SubProject.CreatorUserId == model.User.Id;
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
		publishvm.Title = "系统二室IO发布";
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

	/// <summary>
	/// 通用排序命令
	/// 对所有数据按标准字段进行排序（箱外信号）：
	/// - 非FF信号：按机柜号、机笼号、插槽、通道号排序
	/// - FF信号：按机柜号、机笼号、插槽、网段、FF从站号、就地箱号排序（不含DP阀位顺序）
	/// </summary>
	[RelayCommand]
	private async void GeneralSorting()
	{
		try
		{
			if (AllData == null || AllData.Count == 0)
			{
				await message.ErrorAsync("当前没有IO数据，请先导入数据！");
				return;
			}
			if (await message.ConfirmAsync("确认要进行通用排序吗？\n\n排序规则：\n- 非FF信号：按机柜号、机笼号、插槽、通道号排序\n- FF信号：按机柜号、机笼号、插槽、网段、FF从站号、就地箱号排序\n\n注意：不包含DP阀位顺序排序"))
			{
				List<IoFullData> list = ApplyGeneralSorting(AllData.ToList());
				AllData = new ObservableCollection<IoFullData>(list);
				await SaveAndUploadFileAsync();
				await message.SuccessAsync("通用排序完成！");
			}
		}
		catch (Exception ex)
		{
			await message.ErrorAsync("通用排序功能出现错误：" + ex.Message);
		}
	}

	/// <summary>
	/// 阀门排序命令（箱内信号排序）
	/// 仅对箱内FF信号按DP阀位顺序进行排序
	/// 其他信号保持原有顺序不变
	/// </summary>
	[RelayCommand]
	private async void ValvePositionSorting()
	{
		try
		{
			if (AllData == null || AllData.Count == 0)
			{
				await message.ErrorAsync("当前没有IO数据，请先导入数据！");
				return;
			}
			if (await message.ConfirmAsync("确认要进行阀门排序吗？\n\n排序规则：\n- 以就地箱号为单位分组\n- 箱内有DP阀位顺序的记录按阀位顺序排在前面\n- 箱内无DP阀位顺序的记录排在后面（保持原顺序）\n- 就地箱号为空的记录保持原顺序不变\n\n注意：请先使用导入阀门顺序功能填充DP阀位顺序字段"))
			{
				List<IoFullData> list = ApplyValveSorting(AllData.ToList());
				AllData = new ObservableCollection<IoFullData>(list);
				await SaveAndUploadFileAsync();
				await message.SuccessAsync("阀门排序完成！");
			}
		}
		catch (Exception ex)
		{
			await message.ErrorAsync("阀门排序功能出现错误：" + ex.Message);
		}
	}

	/// <summary>
	/// 导入阀门顺序命令
	/// 打开导入阀门顺序窗口，支持：
	/// 1. 下载 Excel 模板
	/// 2. 上传阀门数据文件
	/// 3. 匹配阀门编号到信号位号
	/// 4. 填充气口顺序到 DP阀位顺序字段
	/// 注意：此功能只导入数据，不进行排序
	/// </summary>
	[RelayCommand]
	private void ValveSorting()
	{
		try
		{
			if (AllData == null || AllData.Count == 0)
			{
				message.ErrorAsync("当前没有IO数据，请先导入数据！");
				return;
			}
			IServiceProvider service = App.GetService<IServiceProvider>();
			ValveSortingWindow requiredService = service.GetRequiredService<ValveSortingWindow>();
			requiredService.InitializeData(AllData.ToList());
			requiredService.Owner = Application.Current.MainWindow;
			if (requiredService.ShowDialog() == true)
			{
				List<IoFullData> sortedData = requiredService.GetSortedData();
				if (sortedData != null && sortedData.Any())
				{
					AllData = new ObservableCollection<IoFullData>(sortedData);
					SaveAndUploadFileAsync();
					message.SuccessAsync("导入阀门顺序完成！");
				}
			}
		}
		catch (Exception ex)
		{
			message.ErrorAsync("导入阀门顺序功能出现错误：" + ex.Message);
		}
	}

	/// <summary>
	/// 应用通用排序规则
	/// - 非FF信号：按机柜号、机笼号、插槽、通道号排序
	/// - FF信号：按机柜号、机笼号、插槽、网段、FF从站号、就地箱号排序（不含DP阀位顺序）
	/// </summary>
	private List<IoFullData> ApplyGeneralSorting(List<IoFullData> data)
	{
		List<IoFullData> source = data.Where(IsFFSignal).ToList();
		List<IoFullData> source2 = data.Where((IoFullData x) => !IsFFSignal(x)).ToList();
		List<IoFullData> collection = (from x in source2
			orderby x.CabinetNumber ?? string.Empty, x.Cage, x.Slot, x.Channel
			select x).ToList();
		List<IoFullData> collection2 = (from x in source
			orderby x.CabinetNumber ?? string.Empty, x.Cage, x.Slot, x.NetType ?? string.Empty, x.FFDPStaionNumber ?? string.Empty, x.LocalBoxNumber ?? string.Empty
			select x).ToList();
		List<IoFullData> list = new List<IoFullData>();
		list.AddRange(collection);
		list.AddRange(collection2);
		return list;
		static bool IsFFSignal(IoFullData item)
		{
			if (item.PowerType != null)
			{
				return item.PowerType.Contains("FF");
			}
			return false;
		}
	}

	/// <summary>
	/// 应用阀门排序规则（仅对箱内FF信号）
	/// 以就地箱号为单位，箱内有DP阀位顺序的记录按阀位顺序排在前面，空的排在后面
	/// 就地箱号为空的记录保持原有顺序不变
	/// 整个箱子都没有阀位顺序的也保持原有顺序不变
	/// </summary>
	private List<IoFullData> ApplyValveSorting(List<IoFullData> data)
	{
		List<IoFullData> list = new List<IoFullData>();
		var list2 = (from x in data.Select((IoFullData item, int index) => new
			{
				Item = item,
				OriginalIndex = index
			})
			group x by x.Item.LocalBoxNumber ?? string.Empty).ToList();
		foreach (var item in list2)
		{
			string key = item.Key;
			if (string.IsNullOrEmpty(key))
			{
				list.AddRange(from x in item
					orderby x.OriginalIndex
					select x.Item);
				continue;
			}
			if (!item.Any(x => !string.IsNullOrWhiteSpace(x.Item.DPTerminalChannel)))
			{
				list.AddRange(from x in item
					orderby x.OriginalIndex
					select x.Item);
				continue;
			}
			var source = item.Where(x => !string.IsNullOrWhiteSpace(x.Item.DPTerminalChannel)).ToList();
			var source2 = item.Where(x => string.IsNullOrWhiteSpace(x.Item.DPTerminalChannel)).ToList();
			List<IoFullData> collection = (from x in source
				orderby ExtractValvePositionNumber(x.Item.DPTerminalChannel)
				select x.Item).ToList();
			List<IoFullData> collection2 = (from x in source2
				orderby x.OriginalIndex
				select x.Item).ToList();
			list.AddRange(collection);
			list.AddRange(collection2);
		}
		return list;
	}

	/// <summary>
	/// 从阀位顺序字符串中提取第一个数字用于排序
	/// 支持F9、F3/F4、F10等多种格式的智能解析
	/// 对于F3/F4这种多值格式，以第一个数字（3）为准
	/// </summary>
	/// <param name="valvePosition">阀位顺序字符串</param>
	/// <returns>用于排序的数字，无法解析时返回int.MaxValue</returns>
	private static int ExtractValvePositionNumber(string valvePosition)
	{
		if (string.IsNullOrWhiteSpace(valvePosition))
		{
			return int.MaxValue;
		}
		MatchCollection matchCollection = Regex.Matches(valvePosition, "\\d+");
		if (matchCollection.Count > 0 && int.TryParse(matchCollection[0].Value, out var result))
		{
			return result;
		}
		return int.MaxValue;
	}

	/// <summary>
	/// UseFormula属性变更后的处理
	/// 仅切换模式，不自动触发计算
	/// 用户需要手动点击"重新计算"按钮来执行计算
	/// </summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnUseFormulaChanged(bool value)
	{
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.AllData" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.AllData" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnAllDataChanged(ObservableCollection<IoFullData>? value)
	{
		DisplayPoints = null;
		AllPointCount = AllData?.Count ?? 0;
		if (AllData != null)
		{
			if (Filters.Count == 0)
			{
				InitializeFilters();
			}
			RefreshFilterOptions();
			FilterAndSort();
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.DisplayPoints" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.DisplayPoints" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnDisplayPointsChanged(ObservableCollection<IoFullData>? value)
	{
		DisplayPointCount = value?.Count ?? 0;
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.IsAscending" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.IsAscending" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnIsAscendingChanged(bool value)
	{
		FilterAndSort();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.SortOption1" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.SortOption1" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnSortOption1Changed(string value)
	{
		string text = SortOption2;
		SortOption2 = "全部";
		if (SortOption1 == "全部")
		{
			SortOptions2 = new ObservableCollection<string> { "全部" };
		}
		else
		{
			ObservableCollection<string> observableCollection = new ObservableCollection<string>();
			observableCollection.Add("全部");
			foreach (string item in allSortOptions.Except(new _003C_003Ez__ReadOnlySingleElementList<string>(SortOption1)))
			{
				observableCollection.Add(item);
			}
			SortOptions2 = observableCollection;
		}
		if (text == "全部")
		{
			OnSortOption2Changed("全部");
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.SortOption2" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.SortOption2" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnSortOption2Changed(string value)
	{
		string text = SortOption3;
		SortOption3 = "全部";
		if (SortOption2 == "全部")
		{
			SortOptions3 = new ObservableCollection<string> { "全部" };
		}
		else
		{
			ObservableCollection<string> observableCollection = new ObservableCollection<string>();
			observableCollection.Add("全部");
			foreach (string item in allSortOptions.Except(new _003C_003Ez__ReadOnlyArray<string>(new string[2] { SortOption1, SortOption2 })))
			{
				observableCollection.Add(item);
			}
			SortOptions3 = observableCollection;
		}
		if (text == "全部")
		{
			OnSortOption3Changed("全部");
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.SortOption3" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.SortOption3" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnSortOption3Changed(string value)
	{
		string text = SortOption4;
		SortOption4 = "全部";
		if (SortOption3 == "全部")
		{
			SortOptions4 = new ObservableCollection<string> { "全部" };
		}
		else
		{
			ObservableCollection<string> observableCollection = new ObservableCollection<string>();
			observableCollection.Add("全部");
			foreach (string item in allSortOptions.Except(new _003C_003Ez__ReadOnlyArray<string>(new string[3] { SortOption1, SortOption2, SortOption3 })))
			{
				observableCollection.Add(item);
			}
			SortOptions4 = observableCollection;
		}
		if (text == "全部")
		{
			OnSortOption4Changed("全部");
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.SortOption4" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.SortOption4" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnSortOption4Changed(string value)
	{
		string text = SortOption5;
		SortOption5 = "全部";
		if (SortOption4 == "全部")
		{
			SortOptions5 = new ObservableCollection<string> { "全部" };
		}
		else
		{
			ObservableCollection<string> observableCollection = new ObservableCollection<string>();
			observableCollection.Add("全部");
			foreach (string item in allSortOptions.Except(new _003C_003Ez__ReadOnlyArray<string>(new string[4] { SortOption1, SortOption2, SortOption3, SortOption4 })))
			{
				observableCollection.Add(item);
			}
			SortOptions5 = observableCollection;
		}
		if (text == "全部")
		{
			OnSortOption5Changed("全部");
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.SortOption5" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.SortOption5" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnSortOption5Changed(string value)
	{
		FilterAndSort();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.Project" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.Project" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnProjectChanged(config_project? value)
	{
		Majors = null;
		if (value != null)
		{
			Majors = new ObservableCollection<config_project_major>(await (from x in context.Db.Queryable<config_project_major>()
				where (int)x.Department == 2
				where x.ProjectId == value.Id
				select x).ToListAsync());
			if (Majors.Count == 1)
			{
				Major = Majors[0];
			}
		}
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.Major" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.Major" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnMajorChanged(config_project_major? value)
	{
		SubProjects = null;
		if (Project != null && value != null)
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

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.SubProject" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.Pages.DepXT2ViewModel.SubProject" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private async void OnSubProjectChanged(config_project_subProject? value)
	{
		AllData = null;
		if (value != null)
		{
			SetPermissionBySubProject();
			model.Status.Busy("正在获取数据……");
			await ReloadAllData();
			model.Status.Reset();
		}
	}
}
