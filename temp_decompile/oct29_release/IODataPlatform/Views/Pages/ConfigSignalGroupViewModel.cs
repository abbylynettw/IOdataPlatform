using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
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

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class ConfigSignalGroupViewModel : ObservableObject, INavigationAware
{
	[ObservableProperty]
	private ObservableCollection<config_aqj_signalGroup>? signalGroups;

	[ObservableProperty]
	private config_aqj_signalGroup? signalGroup;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.ConfigSignalGroupViewModel.AddSignalGroupCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addSignalGroupCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.ConfigSignalGroupViewModel.EditSignalGroupCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<config_aqj_signalGroup>? editSignalGroupCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.ConfigSignalGroupViewModel.ConfigureSystemCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<config_aqj_signalGroup>? configureSystemCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.ConfigSignalGroupViewModel.DeleteSignalGroupCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<config_aqj_signalGroup>? deleteSignalGroupCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.ConfigSignalGroupViewModel.ImportFilesCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? importFilesCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.ConfigSignalGroupViewModel.signalGroups" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<config_aqj_signalGroup>? SignalGroups
	{
		get
		{
			return signalGroups;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<config_aqj_signalGroup>>.Default.Equals(signalGroups, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SignalGroups);
				signalGroups = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SignalGroups);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.ConfigSignalGroupViewModel.signalGroup" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public config_aqj_signalGroup? SignalGroup
	{
		get
		{
			return signalGroup;
		}
		set
		{
			if (!EqualityComparer<config_aqj_signalGroup>.Default.Equals(signalGroup, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SignalGroup);
				signalGroup = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SignalGroup);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.ConfigSignalGroupViewModel.AddSignalGroup" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddSignalGroupCommand => addSignalGroupCommand ?? (addSignalGroupCommand = new AsyncRelayCommand(AddSignalGroup));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.ConfigSignalGroupViewModel.EditSignalGroup(IODataPlatform.Models.DBModels.config_aqj_signalGroup)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<config_aqj_signalGroup> EditSignalGroupCommand => editSignalGroupCommand ?? (editSignalGroupCommand = new AsyncRelayCommand<config_aqj_signalGroup>(EditSignalGroup));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.ConfigSignalGroupViewModel.ConfigureSystem(IODataPlatform.Models.DBModels.config_aqj_signalGroup)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<config_aqj_signalGroup> ConfigureSystemCommand => configureSystemCommand ?? (configureSystemCommand = new RelayCommand<config_aqj_signalGroup>(ConfigureSystem));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.ConfigSignalGroupViewModel.DeleteSignalGroup(IODataPlatform.Models.DBModels.config_aqj_signalGroup)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<config_aqj_signalGroup> DeleteSignalGroupCommand => deleteSignalGroupCommand ?? (deleteSignalGroupCommand = new AsyncRelayCommand<config_aqj_signalGroup>(DeleteSignalGroup));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.ConfigSignalGroupViewModel.ImportFiles" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ImportFilesCommand => importFilesCommand ?? (importFilesCommand = new AsyncRelayCommand(ImportFiles));

	public ConfigSignalGroupViewModel(SqlSugarContext context, GlobalModel model, StorageService storage, IMessageService message, INavigationService navigation, ConfigTableViewModel configvm, IPickerService picker, ExcelService excel)
	{
		_003Ccontext_003EP = context;
		_003Cmodel_003EP = model;
		_003Cstorage_003EP = storage;
		_003Cmessage_003EP = message;
		_003Cnavigation_003EP = navigation;
		_003Cpicker_003EP = picker;
		_003Cexcel_003EP = excel;
		base._002Ector();
	}

	public void OnNavigatedFrom()
	{
	}

	public async void OnNavigatedTo()
	{
		_003Cmodel_003EP.Status.Busy("正在刷新……");
		await RefreshAsync();
		_003Cmodel_003EP.Status.Reset();
	}

	[RelayCommand]
	private async Task AddSignalGroup()
	{
		config_aqj_signalGroup signalGroup = new config_aqj_signalGroup();
		if (Edit(signalGroup, "添加信号组"))
		{
			await _003Ccontext_003EP.Db.Insertable(signalGroup).ExecuteCommandIdentityIntoEntityAsync();
			List<config_aqj_signalGroupDetail> insertObjs = new List<config_aqj_signalGroupDetail>
			{
				new config_aqj_signalGroupDetail
				{
					signalGroupId = signalGroup.Id,
					序号 = string.Empty,
					信号名称 = string.Empty
				}
			};
			await _003Ccontext_003EP.Db.Insertable(insertObjs).ExecuteCommandAsync();
			await RefreshAsync();
		}
	}

	[RelayCommand]
	private async Task EditSignalGroup(config_aqj_signalGroup signalGroup)
	{
		config_aqj_signalGroup config_aqj_signalGroup = new config_aqj_signalGroup().CopyPropertiesFrom(signalGroup);
		if (Edit(config_aqj_signalGroup, "编辑信号组"))
		{
			await _003Ccontext_003EP.Db.Updateable(config_aqj_signalGroup).ExecuteCommandAsync();
			await RefreshAsync();
		}
	}

	[RelayCommand]
	private void ConfigureSystem(config_aqj_signalGroup signalGroup)
	{
		SignalGroup = signalGroup;
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(ConfigSignalGroupDetailPage));
	}

	[RelayCommand]
	private async Task DeleteSignalGroup(config_aqj_signalGroup signalGroup)
	{
		if (await _003Cmessage_003EP.ConfirmAsync("确认删除\"" + signalGroup.signalGroupName + "\"\r\n此操作会同时删除信号组相关的所有数据，包括信号组详情"))
		{
			_003Cmodel_003EP.Status.Busy("正在获取信号组详情数据……");
			List<config_aqj_signalGroupDetail> signalGroupDetails = await (from x in _003Ccontext_003EP.Db.Queryable<config_aqj_signalGroupDetail>()
				where x.signalGroupId == signalGroup.Id
				select x).ToListAsync();
			int[] array = signalGroupDetails.Select((config_aqj_signalGroupDetail x) => x.Id).ToArray();
			_003Cmodel_003EP.Status.Busy("正在删除信号组详情数据……");
			int[] array2 = array;
			foreach (int subProjectId in array2)
			{
				await _003Cstorage_003EP.DeleteSubprojectFolderAsync(subProjectId);
			}
			await _003Ccontext_003EP.Db.Deleteable(signalGroupDetails).ExecuteCommandAsync();
			await _003Ccontext_003EP.Db.Deleteable(signalGroup).ExecuteCommandAsync();
			await RefreshAsync();
			_003Cmodel_003EP.Status.Reset();
		}
	}

	private static bool Edit(config_aqj_signalGroup obj, string title)
	{
		EditorOptionBuilder<config_aqj_signalGroup> editorOptionBuilder = obj.CreateEditorBuilder();
		editorOptionBuilder.WithTitle(title).WithEditorHeight(300.0);
		editorOptionBuilder.AddProperty<string>("signalGroupName").WithHeader("信号组名称");
		editorOptionBuilder.AddProperty<string>("Note").WithHeader("备注").EditAsText()
			.WithMultiLine();
		return editorOptionBuilder.Build().EditWithWpfUI();
	}

	private async Task RefreshAsync()
	{
		SignalGroups = null;
		ObservableCollection<config_aqj_signalGroup> observableCollection = new ObservableCollection<config_aqj_signalGroup>();
		foreach (config_aqj_signalGroup item in await _003Ccontext_003EP.Db.Queryable<config_aqj_signalGroup>().ToListAsync())
		{
			observableCollection.Add(item);
		}
		SignalGroups = observableCollection;
	}

	[RelayCommand]
	private async Task ImportFiles()
	{
		string[] files = _003Cpicker_003EP.PickFolderAndGetFiles("Excel 文件(*.xls; *.xlsx)|*.xls; *.xlsx");
		if (files == null)
		{
			return;
		}
		_003Cmodel_003EP.Status.Busy("开始导入");
		string[] array = files;
		foreach (string filePath in array)
		{
			try
			{
				string groupName = _003Cpicker_003EP.GetFileNameWithoutExtension(filePath);
				config_aqj_signalGroup signalGroup = (await _003Ccontext_003EP.Db.Queryable<config_aqj_signalGroup>().FirstAsync((config_aqj_signalGroup g) => g.signalGroupName == groupName)) ?? new config_aqj_signalGroup
				{
					signalGroupName = groupName
				};
				if (signalGroup.Id == 0)
				{
					await _003Ccontext_003EP.Db.Insertable(signalGroup).ExecuteCommandIdentityIntoEntityAsync();
				}
				DataTable dataTable = await _003Cexcel_003EP.GetDataTableAsStringAsync(filePath, hasHeader: true);
				try
				{
					List<config_aqj_signalGroupDetail> list = (await Task.Run(() => dataTable.StringTableToIEnumerableByDiplay<config_aqj_signalGroupDetail>())).Select(delegate(config_aqj_signalGroupDetail item)
					{
						item.signalGroupId = signalGroup.Id;
						return item;
					}).ToList();
					await (from d in _003Ccontext_003EP.Db.Deleteable<config_aqj_signalGroupDetail>()
						where d.signalGroupId == signalGroup.Id
						select d).ExecuteCommandAsync();
					await _003Ccontext_003EP.Db.Insertable(list).ExecuteCommandAsync();
					await RefreshAsync();
				}
				finally
				{
					if (dataTable != null)
					{
						((IDisposable)dataTable).Dispose();
					}
				}
			}
			catch (Exception)
			{
			}
		}
		_003Cmodel_003EP.Status.Success($"导入成功，共导入{files.Count()}种信号组");
	}
}
