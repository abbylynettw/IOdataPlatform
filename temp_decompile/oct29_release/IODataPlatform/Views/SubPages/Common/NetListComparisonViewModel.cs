using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Aspose.Cells;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.SubPages.Common;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class NetListComparisonViewModel : ObservableObject
{
	[ObservableProperty]
	private string oldDataSource;

	[ObservableProperty]
	private string newDataSource;

	[ObservableProperty]
	private int addedCount;

	[ObservableProperty]
	private int deletedCount;

	[ObservableProperty]
	private int modifiedCount;

	[ObservableProperty]
	private bool hasComparisonResults;

	[ObservableProperty]
	private bool showAdded;

	[ObservableProperty]
	private bool showDeleted;

	[ObservableProperty]
	private bool showModified;

	[ObservableProperty]
	private bool showUnchanged;

	private ICollectionView? filteredResultsView;

	private List<CableData>? oldDataList;

	private List<CableData>? newDataList;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.SelectOldFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? selectOldFileCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.SelectNewFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? selectNewFileCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ExecuteComparisonCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? executeComparisonCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ExportReportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? exportReportCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ExpandAllCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? expandAllCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.CollapseAllCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? collapseAllCommand;

	public ObservableCollection<CableComparisonRow> ComparisonResults { get; }

	public ICollectionView? FilteredResults
	{
		get
		{
			if (filteredResultsView == null && ComparisonResults.Count > 0)
			{
				filteredResultsView = CollectionViewSource.GetDefaultView(ComparisonResults);
				filteredResultsView.Filter = FilterResult;
			}
			return filteredResultsView;
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.oldDataSource" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string OldDataSource
	{
		get
		{
			return oldDataSource;
		}
		[MemberNotNull("oldDataSource")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(oldDataSource, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.OldDataSource);
				oldDataSource = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.OldDataSource);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.newDataSource" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string NewDataSource
	{
		get
		{
			return newDataSource;
		}
		[MemberNotNull("newDataSource")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(newDataSource, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.NewDataSource);
				newDataSource = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.NewDataSource);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.addedCount" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int AddedCount
	{
		get
		{
			return addedCount;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(addedCount, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.AddedCount);
				addedCount = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.AddedCount);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.deletedCount" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int DeletedCount
	{
		get
		{
			return deletedCount;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(deletedCount, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DeletedCount);
				deletedCount = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DeletedCount);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.modifiedCount" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public int ModifiedCount
	{
		get
		{
			return modifiedCount;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(modifiedCount, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ModifiedCount);
				modifiedCount = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ModifiedCount);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.hasComparisonResults" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool HasComparisonResults
	{
		get
		{
			return hasComparisonResults;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(hasComparisonResults, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.HasComparisonResults);
				hasComparisonResults = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.HasComparisonResults);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.showAdded" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool ShowAdded
	{
		get
		{
			return showAdded;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(showAdded, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ShowAdded);
				showAdded = value;
				OnShowAddedChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ShowAdded);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.showDeleted" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool ShowDeleted
	{
		get
		{
			return showDeleted;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(showDeleted, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ShowDeleted);
				showDeleted = value;
				OnShowDeletedChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ShowDeleted);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.showModified" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool ShowModified
	{
		get
		{
			return showModified;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(showModified, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ShowModified);
				showModified = value;
				OnShowModifiedChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ShowModified);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.showUnchanged" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public bool ShowUnchanged
	{
		get
		{
			return showUnchanged;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(showUnchanged, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ShowUnchanged);
				showUnchanged = value;
				OnShowUnchangedChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ShowUnchanged);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.SelectOldFile" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand SelectOldFileCommand => selectOldFileCommand ?? (selectOldFileCommand = new AsyncRelayCommand(SelectOldFile));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.SelectNewFile" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand SelectNewFileCommand => selectNewFileCommand ?? (selectNewFileCommand = new AsyncRelayCommand(SelectNewFile));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ExecuteComparison" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ExecuteComparisonCommand => executeComparisonCommand ?? (executeComparisonCommand = new AsyncRelayCommand(ExecuteComparison));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ExportReport" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ExportReportCommand => exportReportCommand ?? (exportReportCommand = new AsyncRelayCommand(ExportReport));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ExpandAll" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ExpandAllCommand => expandAllCommand ?? (expandAllCommand = new RelayCommand(ExpandAll));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.CollapseAll" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand CollapseAllCommand => collapseAllCommand ?? (collapseAllCommand = new RelayCommand(CollapseAll));

	public NetListComparisonViewModel(GlobalModel model, IMessageService message, IPickerService picker, ExcelService excel, StorageService storage)
	{
		_003Cmodel_003EP = model;
		_003Cmessage_003EP = message;
		_003Cpicker_003EP = picker;
		_003Cexcel_003EP = excel;
		oldDataSource = "请选择旧版网络接线清单";
		newDataSource = "请选择新版网络接线清单";
		showAdded = true;
		showDeleted = true;
		showModified = true;
		ComparisonResults = new ObservableCollection<CableComparisonRow>();
		base._002Ector();
	}

	private void RefreshFilter()
	{
		ICollectionView? filteredResults = FilteredResults;
		if (filteredResults != null)
		{
			filteredResults.Refresh();
		}
	}

	private bool FilterResult(object obj)
	{
		if (!(obj is CableComparisonRow cableComparisonRow))
		{
			return false;
		}
		return cableComparisonRow.Type switch
		{
			ChangeType.新增 => ShowAdded, 
			ChangeType.删除 => ShowDeleted, 
			ChangeType.修改 => ShowModified, 
			ChangeType.无变化 => ShowUnchanged, 
			_ => false, 
		};
	}

	[RelayCommand]
	private async Task SelectOldFile()
	{
		string file = _003Cpicker_003EP.OpenFile("Excel文件|*.xlsx;*.xls");
		if (file != null)
		{
			try
			{
				_003Cmodel_003EP.Status.Busy("正在加载旧版清单...");
				oldDataList = await LoadCableDataFromExcel(file);
				OldDataSource = $"{Path.GetFileName(file)} ({oldDataList.Count}条记录)";
				_003Cmodel_003EP.Status.Success($"已加载 {oldDataList.Count} 条旧版数据");
			}
			catch (Exception ex)
			{
				_003Cmodel_003EP.Status.Error("加载失败：" + ex.Message);
			}
		}
	}

	[RelayCommand]
	private async Task SelectNewFile()
	{
		string file = _003Cpicker_003EP.OpenFile("Excel文件|*.xlsx;*.xls");
		if (file != null)
		{
			try
			{
				_003Cmodel_003EP.Status.Busy("正在加载新版清单...");
				newDataList = await LoadCableDataFromExcel(file);
				NewDataSource = $"{Path.GetFileName(file)} ({newDataList.Count}条记录)";
				_003Cmodel_003EP.Status.Success($"已加载 {newDataList.Count} 条新版数据");
			}
			catch (Exception ex)
			{
				_003Cmodel_003EP.Status.Error("加载失败：" + ex.Message);
			}
		}
	}

	[RelayCommand]
	private async Task ExecuteComparison()
	{
		if (oldDataList == null || newDataList == null)
		{
			await _003Cmessage_003EP.MessageAsync("提示", "请先选择新旧两个网络接线清单文件！");
			return;
		}
		try
		{
			_003Cmodel_003EP.Status.Busy("正在检查数据...");
			List<(string, int)> list = FindDuplicateConnections(oldDataList, "旧版清单");
			List<(string, int)> list2 = FindDuplicateConnections(newDataList, "新版清单");
			if (list.Count > 0 || list2.Count > 0)
			{
				string text = "⚠\ufe0f 检测到重复的连接记录（起点+终点），请处理后再进行对比！\n\n";
				if (list.Count > 0)
				{
					text += $"【旧版清单】发现 {list.Count} 组重复：\n";
					foreach (var item in list.Take(5))
					{
						text += $"  • {item.Item1} (出现 {item.Item2} 次)\n";
					}
					if (list.Count > 5)
					{
						text += $"  ... 及其他 {list.Count - 5} 组\n";
					}
					text += "\n";
				}
				if (list2.Count > 0)
				{
					text += $"【新版清单】发现 {list2.Count} 组重复：\n";
					foreach (var item2 in list2.Take(5))
					{
						text += $"  • {item2.Item1} (出现 {item2.Item2} 次)\n";
					}
					if (list2.Count > 5)
					{
						text += $"  ... 及其他 {list2.Count - 5} 组\n";
					}
				}
				_003Cmodel_003EP.Status.Reset();
				await _003Cmessage_003EP.AlertAsync(text, "数据错误");
				return;
			}
			_003Cmodel_003EP.Status.Busy("正在执行对比分析...");
			List<CableComparisonRow> list3 = await Task.Run(() => CompareCableLists(oldDataList, newDataList));
			ComparisonResults.Clear();
			foreach (CableComparisonRow item3 in list3)
			{
				ComparisonResults.Add(item3);
			}
			AddedCount = ComparisonResults.Count((CableComparisonRow x) => x.Type == ChangeType.新增);
			DeletedCount = ComparisonResults.Count((CableComparisonRow x) => x.Type == ChangeType.删除);
			ModifiedCount = ComparisonResults.Count((CableComparisonRow x) => x.Type == ChangeType.修改);
			filteredResultsView = null;
			OnPropertyChanged("FilteredResults");
			HasComparisonResults = ComparisonResults.Count > 0;
			_003Cmodel_003EP.Status.Success($"对比完成！新增{AddedCount}条，删除{DeletedCount}条，修改{ModifiedCount}条");
		}
		catch (Exception ex)
		{
			_003Cmodel_003EP.Status.Error("对比失败：" + ex.Message);
		}
	}

	/// <summary>
	/// 查找重复的连接点
	/// </summary>
	private List<(string Key, int Count)> FindDuplicateConnections(List<CableData> dataList, string sourceName)
	{
		return (from item in dataList
			group item by new ConnectionKey
			{
				StartCabinet = (item.起点盘柜名称 ?? ""),
				StartDevice = (item.起点设备名称 ?? ""),
				StartConnection = (item.起点接线点1 ?? ""),
				EndCabinet = (item.终点盘柜名称 ?? ""),
				EndDevice = (item.终点设备名称 ?? ""),
				EndConnection = (item.终点接线点1 ?? "")
			} into g
			where g.Count() > 1
			select (Key: g.Key.ToString(), Count: g.Count()) into x
			orderby x.Count descending
			select x).ToList();
	}

	/// <summary>
	/// 对比两个电缆清单，生成升版后的完整表格（按新版顺序）
	/// 对比策略：
	/// 1. 完全相同的行 → 无变化
	/// 2. 6字段匹配（起点盘柜、起点设备、起点接线点1、终点盘柜、终点设备、终点接线点1） → 修改
	/// 3. 5字段匹配（起点盘柜、起点设备、终点盘柜、终点设备、终点接线点1） → 修改
	/// 4. 4字段匹配（起点盘柜、起点设备、终点盘柜、终点设备） → 修改
	/// 5. 其他情况 → 新增或删除
	/// </summary>
	private List<CableComparisonRow> CompareCableLists(List<CableData> oldList, List<CableData> newList)
	{
		List<CableComparisonRow> list = new List<CableComparisonRow>();
		HashSet<CableData> hashSet = new HashSet<CableData>();
		List<CableData> list2 = oldList.Where((CableData x) => !x.IsDeletedRow).ToList();
		List<CableData> list3 = newList.Where((CableData x) => !x.IsDeletedRow).ToList();
		foreach (CableData item in list3)
		{
			CableData cableData = FindBestMatch(item, list2, hashSet);
			if (cableData != null)
			{
				if (IsCompletelyEqual(cableData, item))
				{
					list.Add(new CableComparisonRow
					{
						Data = item,
						Type = ChangeType.无变化,
						ChangedFields = new List<string>()
					});
				}
				else
				{
					List<string> changedFields = FindChangedFields(cableData, item);
					list.Add(new CableComparisonRow
					{
						Data = item,
						OldData = cableData,
						Type = ChangeType.修改,
						ChangedFields = changedFields
					});
				}
				hashSet.Add(cableData);
			}
			else
			{
				list.Add(new CableComparisonRow
				{
					Data = item,
					Type = ChangeType.新增,
					ChangedFields = new List<string>()
				});
			}
		}
		foreach (CableData item2 in list2)
		{
			if (!hashSet.Contains(item2))
			{
				list.Add(new CableComparisonRow
				{
					Data = item2,
					Type = ChangeType.删除,
					ChangedFields = new List<string>()
				});
			}
		}
		return list;
	}

	/// <summary>
	/// 查找最佳匹配的旧数据（按优先级：6字段 → 5字段 → 4字段）
	/// </summary>
	private CableData? FindBestMatch(CableData newItem, List<CableData> oldList, HashSet<CableData> processedOldItems)
	{
		string newStartCabinet = newItem.起点盘柜名称?.Trim() ?? "";
		string newStartDevice = newItem.起点设备名称?.Trim() ?? "";
		string newStartConnection = newItem.起点接线点1?.Trim() ?? "";
		string newEndCabinet = newItem.终点盘柜名称?.Trim() ?? "";
		string newEndDevice = newItem.终点设备名称?.Trim() ?? "";
		string newEndConnection = newItem.终点接线点1?.Trim() ?? "";
		CableData cableData = oldList.FirstOrDefault((CableData oldItem) => !processedOldItems.Contains(oldItem) && (oldItem.起点盘柜名称?.Trim() ?? "") == newStartCabinet && (oldItem.起点设备名称?.Trim() ?? "") == newStartDevice && (oldItem.起点接线点1?.Trim() ?? "") == newStartConnection && (oldItem.终点盘柜名称?.Trim() ?? "") == newEndCabinet && (oldItem.终点设备名称?.Trim() ?? "") == newEndDevice && (oldItem.终点接线点1?.Trim() ?? "") == newEndConnection);
		if (cableData != null)
		{
			return cableData;
		}
		CableData cableData2 = oldList.FirstOrDefault((CableData oldItem) => !processedOldItems.Contains(oldItem) && (oldItem.起点盘柜名称?.Trim() ?? "") == newStartCabinet && (oldItem.起点设备名称?.Trim() ?? "") == newStartDevice && (oldItem.终点盘柜名称?.Trim() ?? "") == newEndCabinet && (oldItem.终点设备名称?.Trim() ?? "") == newEndDevice && (oldItem.终点接线点1?.Trim() ?? "") == newEndConnection);
		if (cableData2 != null)
		{
			return cableData2;
		}
		CableData cableData3 = oldList.FirstOrDefault((CableData oldItem) => !processedOldItems.Contains(oldItem) && (oldItem.起点盘柜名称?.Trim() ?? "") == newStartCabinet && (oldItem.起点设备名称?.Trim() ?? "") == newStartDevice && (oldItem.终点盘柜名称?.Trim() ?? "") == newEndCabinet && (oldItem.终点设备名称?.Trim() ?? "") == newEndDevice);
		if (cableData3 != null)
		{
			return cableData3;
		}
		return null;
	}

	/// <summary>
	/// 判断两个电缆数据是否完全相同（所有字段）
	/// </summary>
	private bool IsCompletelyEqual(CableData oldData, CableData newData)
	{
		if ((oldData.序号?.Trim() ?? "") == (newData.序号?.Trim() ?? "") && (oldData.线缆编号?.Trim() ?? "") == (newData.线缆编号?.Trim() ?? "") && (oldData.线缆列别?.Trim() ?? "") == (newData.线缆列别?.Trim() ?? "") && (oldData.起点房间号?.Trim() ?? "") == (newData.起点房间号?.Trim() ?? "") && (oldData.起点盘柜名称?.Trim() ?? "") == (newData.起点盘柜名称?.Trim() ?? "") && (oldData.起点设备名称?.Trim() ?? "") == (newData.起点设备名称?.Trim() ?? "") && (oldData.起点接线点1?.Trim() ?? "") == (newData.起点接线点1?.Trim() ?? "") && (oldData.起点接线点2?.Trim() ?? "") == (newData.起点接线点2?.Trim() ?? "") && (oldData.电缆长度?.Trim() ?? "") == (newData.电缆长度?.Trim() ?? "") && (oldData.终点房间号?.Trim() ?? "") == (newData.终点房间号?.Trim() ?? "") && (oldData.终点盘柜名称?.Trim() ?? "") == (newData.终点盘柜名称?.Trim() ?? "") && (oldData.终点设备名称?.Trim() ?? "") == (newData.终点设备名称?.Trim() ?? "") && (oldData.终点接线点1?.Trim() ?? "") == (newData.终点接线点1?.Trim() ?? "") && (oldData.终点接线点2?.Trim() ?? "") == (newData.终点接线点2?.Trim() ?? "") && (oldData.供货方?.Trim() ?? "") == (newData.供货方?.Trim() ?? ""))
		{
			return (oldData.备注?.Trim() ?? "") == (newData.备注?.Trim() ?? "");
		}
		return false;
	}

	/// <summary>
	/// 查找两个电缆数据之间的变化字段
	/// </summary>
	private List<string> FindChangedFields(CableData oldData, CableData newData)
	{
		List<string> list = new List<string>();
		CompareField(list, "线缆编号", oldData.线缆编号, newData.线缆编号);
		CompareField(list, "线缆类别", oldData.线缆列别, newData.线缆列别);
		CompareField(list, "起点房间号", oldData.起点房间号, newData.起点房间号);
		CompareField(list, "起点接线点2", oldData.起点接线点2, newData.起点接线点2);
		CompareField(list, "电缆长度", oldData.电缆长度, newData.电缆长度);
		CompareField(list, "终点房间号", oldData.终点房间号, newData.终点房间号);
		CompareField(list, "终点接线点2", oldData.终点接线点2, newData.终点接线点2);
		CompareField(list, "供货方", oldData.供货方, newData.供货方);
		CompareField(list, "备注", oldData.备注, newData.备注);
		return list;
	}

	/// <summary>
	/// 对比单个字段
	/// </summary>
	private void CompareField(List<string> changedFields, string fieldName, string? oldValue, string? newValue)
	{
		oldValue = oldValue?.Trim() ?? "";
		newValue = newValue?.Trim() ?? "";
		if (oldValue != newValue)
		{
			changedFields.Add(fieldName);
		}
	}

	[RelayCommand]
	private async Task ExportReport()
	{
		if (!HasComparisonResults)
		{
			await _003Cmessage_003EP.MessageAsync("提示", "没有可导出的对比结果！");
			return;
		}
		try
		{
			string filename = $"网络接线清单对比报告_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
			string savePath = _003Cpicker_003EP.SaveFile("Excel文件|*.xlsx", filename);
			if (savePath == null)
			{
				return;
			}
			_003Cmodel_003EP.Status.Busy("正在生成对比报告...");
			Workbook workbook = _003Cexcel_003EP.GetWorkbook();
			Worksheet worksheet = workbook.Worksheets[0];
			worksheet.Name = "升版后清单";
			string[] array = new string[18]
			{
				"变更类型", "序号", "线缆编号", "线缆类别", "起点房间号", "起点盘柜", "起点设备", "起点接线点1", "起点接线点2", "电缆长度",
				"终点房间号", "终点盘柜", "终点设备", "终点接线点1", "终点接线点2", "供货方", "备注", "变更字段"
			};
			for (int i = 0; i < array.Length; i++)
			{
				Cell cell = worksheet.Cells[0, i];
				cell.PutValue(array[i]);
				cell.SetStyle(GetHeaderStyle());
			}
			int num = 1;
			Style style = workbook.CreateStyle();
			style.ForegroundColor = Color.FromArgb(245, 245, 245);
			style.Pattern = BackgroundType.Solid;
			style.Font.IsStrikeout = true;
			Style style2 = workbook.CreateStyle();
			style2.ForegroundColor = Color.FromArgb(245, 245, 245);
			style2.Pattern = BackgroundType.Solid;
			Style style3 = workbook.CreateStyle();
			style3.Font.Color = Color.Red;
			Style style4 = workbook.CreateStyle();
			style4.HorizontalAlignment = TextAlignmentType.Center;
			foreach (CableComparisonRow comparisonResult in ComparisonResults)
			{
				worksheet.Cells[num, 0].PutValue(comparisonResult.Type.ToString());
				worksheet.Cells[num, 1].PutValue(comparisonResult.Data.序号);
				worksheet.Cells[num, 2].PutValue(comparisonResult.Data.线缆编号);
				worksheet.Cells[num, 3].PutValue(comparisonResult.Data.线缆列别);
				worksheet.Cells[num, 4].PutValue(comparisonResult.Data.起点房间号);
				worksheet.Cells[num, 5].PutValue(comparisonResult.Data.起点盘柜名称);
				worksheet.Cells[num, 6].PutValue(comparisonResult.Data.起点设备名称);
				worksheet.Cells[num, 7].PutValue(comparisonResult.Data.起点接线点1);
				worksheet.Cells[num, 8].PutValue(comparisonResult.Data.起点接线点2);
				worksheet.Cells[num, 9].PutValue(comparisonResult.Data.电缆长度);
				worksheet.Cells[num, 10].PutValue(comparisonResult.Data.终点房间号);
				worksheet.Cells[num, 11].PutValue(comparisonResult.Data.终点盘柜名称);
				worksheet.Cells[num, 12].PutValue(comparisonResult.Data.终点设备名称);
				worksheet.Cells[num, 13].PutValue(comparisonResult.Data.终点接线点1);
				worksheet.Cells[num, 14].PutValue(comparisonResult.Data.终点接线点2);
				worksheet.Cells[num, 15].PutValue(comparisonResult.Data.供货方);
				worksheet.Cells[num, 16].PutValue(comparisonResult.Data.备注);
				worksheet.Cells[num, 17].PutValue((comparisonResult.ChangedFields.Count > 0) ? string.Join(", ", comparisonResult.ChangedFields) : "");
				if (comparisonResult.Type == ChangeType.删除)
				{
					for (int j = 0; j < array.Length; j++)
					{
						worksheet.Cells[num, j].SetStyle(style);
					}
					num++;
				}
				else if (comparisonResult.Type == ChangeType.修改 && comparisonResult.OldData != null)
				{
					string[] array2 = new string[17]
					{
						null,
						comparisonResult.OldData.序号,
						comparisonResult.OldData.线缆编号,
						comparisonResult.OldData.线缆列别,
						comparisonResult.OldData.起点房间号,
						comparisonResult.OldData.起点盘柜名称,
						comparisonResult.OldData.起点设备名称,
						comparisonResult.OldData.起点接线点1,
						comparisonResult.OldData.起点接线点2,
						comparisonResult.OldData.电缆长度,
						comparisonResult.OldData.终点房间号,
						comparisonResult.OldData.终点盘柜名称,
						comparisonResult.OldData.终点设备名称,
						comparisonResult.OldData.终点接线点1,
						comparisonResult.OldData.终点接线点2,
						comparisonResult.OldData.供货方,
						comparisonResult.OldData.备注
					};
					for (int k = 1; k <= 16; k++)
					{
						string a = worksheet.Cells[num, k].StringValue?.Trim() ?? "";
						string b = (array2[k] ?? "").Trim();
						if (!string.Equals(a, b, StringComparison.Ordinal))
						{
							worksheet.Cells[num, k].SetStyle(style3);
						}
					}
					int row = num + 1;
					worksheet.Cells[row, 0].PutValue("修改前");
					worksheet.Cells[row, 1].PutValue(comparisonResult.OldData.序号);
					worksheet.Cells[row, 2].PutValue(comparisonResult.OldData.线缆编号);
					worksheet.Cells[row, 3].PutValue(comparisonResult.OldData.线缆列别);
					worksheet.Cells[row, 4].PutValue(comparisonResult.OldData.起点房间号);
					worksheet.Cells[row, 5].PutValue(comparisonResult.OldData.起点盘柜名称);
					worksheet.Cells[row, 6].PutValue(comparisonResult.OldData.起点设备名称);
					worksheet.Cells[row, 7].PutValue(comparisonResult.OldData.起点接线点1);
					worksheet.Cells[row, 8].PutValue(comparisonResult.OldData.起点接线点2);
					worksheet.Cells[row, 9].PutValue(comparisonResult.OldData.电缆长度);
					worksheet.Cells[row, 10].PutValue(comparisonResult.OldData.终点房间号);
					worksheet.Cells[row, 11].PutValue(comparisonResult.OldData.终点盘柜名称);
					worksheet.Cells[row, 12].PutValue(comparisonResult.OldData.终点设备名称);
					worksheet.Cells[row, 13].PutValue(comparisonResult.OldData.终点接线点1);
					worksheet.Cells[row, 14].PutValue(comparisonResult.OldData.终点接线点2);
					worksheet.Cells[row, 15].PutValue(comparisonResult.OldData.供货方);
					worksheet.Cells[row, 16].PutValue(comparisonResult.OldData.备注);
					worksheet.Cells[row, 17].PutValue("");
					worksheet.Cells[row, 0].SetStyle(style4);
					for (int l = 0; l < array.Length; l++)
					{
						worksheet.Cells[row, l].SetStyle(style2);
					}
					num += 2;
				}
				else
				{
					num++;
				}
			}
			worksheet.AutoFitColumns();
			workbook.Save(savePath);
			_003Cmodel_003EP.Status.Success("对比报告已导出：" + Path.GetFileName(savePath));
			if (await _003Cmessage_003EP.ConfirmAsync("导出成功！是否立即打开文件？", "导出成功"))
			{
				Process.Start("explorer.exe", savePath);
			}
		}
		catch (Exception ex)
		{
			_003Cmodel_003EP.Status.Error("导出失败：" + ex.Message);
		}
	}

	[RelayCommand]
	private void ExpandAll()
	{
		foreach (CableComparisonRow comparisonResult in ComparisonResults)
		{
			if (comparisonResult.Type == ChangeType.修改)
			{
				comparisonResult.ShowOldData = true;
			}
		}
	}

	[RelayCommand]
	private void CollapseAll()
	{
		foreach (CableComparisonRow comparisonResult in ComparisonResults)
		{
			if (comparisonResult.Type == ChangeType.修改)
			{
				comparisonResult.ShowOldData = false;
			}
		}
	}

	private Style GetHeaderStyle()
	{
		Style style = new Style();
		style.Font.IsBold = true;
		style.Font.Size = 11;
		style.ForegroundColor = Color.LightGray;
		style.Pattern = BackgroundType.Solid;
		style.HorizontalAlignment = TextAlignmentType.Center;
		return style;
	}

	private async Task<List<CableData>> LoadCableDataFromExcel(string filePath)
	{
		return await Task.Run(delegate
		{
			Workbook workbook = _003Cexcel_003EP.GetWorkbook(filePath);
			Worksheet worksheet = workbook.Worksheets[0];
			List<CableData> list = new List<CableData>();
			for (int i = 4; i <= worksheet.Cells.MaxDataRow; i++)
			{
				bool isDeletedRow = false;
				int maxDataColumn = worksheet.Cells.MaxDataColumn;
				for (int j = 0; j <= maxDataColumn; j++)
				{
					Style style = worksheet.Cells[i, j]?.GetStyle();
					if (style != null && style.Font?.IsStrikeout == true)
					{
						isDeletedRow = true;
						break;
					}
				}
				CableData cableData = new CableData
				{
					序号 = worksheet.Cells[i, 0]?.StringValue?.Trim(),
					线缆编号 = worksheet.Cells[i, 1]?.StringValue?.Trim(),
					色标 = worksheet.Cells[i, 2]?.StringValue?.Trim(),
					特性代码 = worksheet.Cells[i, 3]?.StringValue?.Trim(),
					线缆列别 = worksheet.Cells[i, 4]?.StringValue?.Trim(),
					起点房间号 = worksheet.Cells[i, 5]?.StringValue?.Trim(),
					起点盘柜名称 = worksheet.Cells[i, 6]?.StringValue?.Trim(),
					起点设备名称 = worksheet.Cells[i, 7]?.StringValue?.Trim(),
					起点接线点1 = worksheet.Cells[i, 8]?.StringValue?.Trim(),
					起点接线点2 = worksheet.Cells[i, 9]?.StringValue?.Trim(),
					电缆长度 = worksheet.Cells[i, 10]?.StringValue?.Trim(),
					终点房间号 = worksheet.Cells[i, 11]?.StringValue?.Trim(),
					终点盘柜名称 = worksheet.Cells[i, 12]?.StringValue?.Trim(),
					终点设备名称 = worksheet.Cells[i, 13]?.StringValue?.Trim(),
					终点接线点1 = worksheet.Cells[i, 14]?.StringValue?.Trim(),
					终点接线点2 = worksheet.Cells[i, 15]?.StringValue?.Trim(),
					供货方 = worksheet.Cells[i, 16]?.StringValue?.Trim(),
					版本 = worksheet.Cells[i, 17]?.StringValue?.Trim(),
					备注 = worksheet.Cells[i, 18]?.StringValue?.Trim(),
					IsDeletedRow = isDeletedRow
				};
				if (!string.IsNullOrEmpty(cableData.起点盘柜名称) || !string.IsNullOrEmpty(cableData.起点设备名称) || !string.IsNullOrEmpty(cableData.起点接线点1))
				{
					list.Add(cableData);
				}
			}
			return list;
		});
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ShowAdded" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ShowAdded" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnShowAddedChanged(bool value)
	{
		RefreshFilter();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ShowDeleted" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ShowDeleted" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnShowDeletedChanged(bool value)
	{
		RefreshFilter();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ShowModified" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ShowModified" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnShowModifiedChanged(bool value)
	{
		RefreshFilter();
	}

	/// <summary>Executes the logic for when <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ShowUnchanged" /> just changed.</summary>
	/// <param name="value">The new property value that was set.</param>
	/// <remarks>This method is invoked right after the value of <see cref="P:IODataPlatform.Views.SubPages.Common.NetListComparisonViewModel.ShowUnchanged" /> is changed.</remarks>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	private void OnShowUnchangedChanged(bool value)
	{
		RefreshFilter();
	}
}
