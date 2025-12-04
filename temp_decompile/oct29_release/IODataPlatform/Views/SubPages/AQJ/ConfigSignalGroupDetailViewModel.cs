using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
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
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;
using LYSoft.Libs;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.AQJ;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class ConfigSignalGroupDetailViewModel : ObservableObject, INavigationAware
{
	[ObservableProperty]
	private config_aqj_signalGroup? signalGroup;

	[ObservableProperty]
	private ObservableCollection<config_aqj_signalGroupDetail>? signalGroupDetails;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.AQJ.ConfigSignalGroupDetailViewModel.DownLoadCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? downLoadCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.AQJ.ConfigSignalGroupDetailViewModel.ImportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? importCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.AQJ.ConfigSignalGroupDetailViewModel.DeleteCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? deleteCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.AQJ.ConfigSignalGroupDetailViewModel.EditCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<config_aqj_signalGroupDetail>? editCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.AQJ.ConfigSignalGroupDetailViewModel.signalGroup" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.AQJ.ConfigSignalGroupDetailViewModel.signalGroupDetails" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<config_aqj_signalGroupDetail>? SignalGroupDetails
	{
		get
		{
			return signalGroupDetails;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<config_aqj_signalGroupDetail>>.Default.Equals(signalGroupDetails, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SignalGroupDetails);
				signalGroupDetails = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SignalGroupDetails);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.AQJ.ConfigSignalGroupDetailViewModel.DownLoad" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand DownLoadCommand => downLoadCommand ?? (downLoadCommand = new AsyncRelayCommand(DownLoad));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.AQJ.ConfigSignalGroupDetailViewModel.Import" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ImportCommand => importCommand ?? (importCommand = new AsyncRelayCommand(Import));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.AQJ.ConfigSignalGroupDetailViewModel.Delete" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand DeleteCommand => deleteCommand ?? (deleteCommand = new AsyncRelayCommand(Delete));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.AQJ.ConfigSignalGroupDetailViewModel.Edit(IODataPlatform.Models.DBModels.config_aqj_signalGroupDetail)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<config_aqj_signalGroupDetail> EditCommand => editCommand ?? (editCommand = new AsyncRelayCommand<config_aqj_signalGroupDetail>(Edit));

	public ConfigSignalGroupDetailViewModel(SqlSugarContext context, ConfigSignalGroupViewModel signalGroupVm, IMessageService message, ExcelService excel, GlobalModel model, IPickerService picker)
	{
		_003Ccontext_003EP = context;
		_003CsignalGroupVm_003EP = signalGroupVm;
		_003Cexcel_003EP = excel;
		_003Cmodel_003EP = model;
		_003Cpicker_003EP = picker;
		base._002Ector();
	}

	[RelayCommand]
	private async Task DownLoad()
	{
		if (signalGroup == null)
		{
			throw new Exception("没有数据，无法下载");
		}
		_003Cmodel_003EP.Status.Busy("开始生成安全级室IO清册...");
		List<config_aqj_signalGroupDetail> data = SignalGroupDetails.ToList();
		DataTable data2 = await data.ToTableByDisplayAttributeAsync();
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		string filePath = Path.Combine(folderPath, signalGroup.signalGroupName + ".xlsx");
		await _003Cexcel_003EP.FastExportToDesktopAsync(data2, filePath);
		_003Cmodel_003EP.Status.Success("已下载到" + filePath);
	}

	[RelayCommand]
	private async Task Import()
	{
		string text = _003Cpicker_003EP.OpenFile("请选择导入的信号(*.xls;*.xlsm; *.xlsx)|*.xls;*.xlsm; *.xlsx");
		if (text == null || string.IsNullOrEmpty(text))
		{
			return;
		}
		using DataTable dataTable = await _003Cexcel_003EP.GetDataTableAsStringAsync(text, hasHeader: true);
		List<config_aqj_signalGroupDetail> list = (await Task.Run((Func<IEnumerable<config_aqj_signalGroupDetail>>)dataTable.StringTableToIEnumerableByDiplay<config_aqj_signalGroupDetail>)).ToList();
		foreach (config_aqj_signalGroupDetail item in list)
		{
			item.signalGroupId = (SignalGroup ?? throw new InvalidOperationException()).Id;
		}
		await Delete();
		await _003Ccontext_003EP.Db.Insertable(list).ExecuteCommandAsync();
		await Refresh();
	}

	[RelayCommand]
	private async Task Delete()
	{
		await (from t in _003Ccontext_003EP.Db.Deleteable<config_aqj_signalGroupDetail>()
			where t.signalGroupId == signalGroup.Id
			select t).ExecuteCommandAsync();
		await Refresh();
	}

	[RelayCommand]
	private async Task Edit(config_aqj_signalGroupDetail data)
	{
		config_aqj_signalGroupDetail config_aqj_signalGroupDetail = new config_aqj_signalGroupDetail().CopyPropertiesFrom(data);
		if (EditSignalGroupDetail(config_aqj_signalGroupDetail, "编辑"))
		{
			await _003Ccontext_003EP.Db.Updateable(config_aqj_signalGroupDetail).ExecuteCommandAsync();
			await Refresh();
		}
	}

	private bool EditSignalGroupDetail(config_aqj_signalGroupDetail data, string title)
	{
		EditorOptionBuilder<config_aqj_signalGroupDetail> editorOptionBuilder = data.CreateEditorBuilder();
		editorOptionBuilder.WithTitle(title).WithEditorHeight(600.0);
		editorOptionBuilder.AddProperty<string>("序号").WithHeader("序号").EditAsText();
		editorOptionBuilder.AddProperty<string>("信号名称").WithHeader("信号名称").EditAsText();
		editorOptionBuilder.AddProperty<string>("信号说明").WithHeader("信号说明").EditAsText();
		editorOptionBuilder.AddProperty<string>("原变量名").WithHeader("原变量名").EditAsText();
		editorOptionBuilder.AddProperty<string>("控制站").WithHeader("控制站").EditAsText();
		editorOptionBuilder.AddProperty<string>("机柜号").WithHeader("机柜号").EditAsText();
		editorOptionBuilder.AddProperty<string>("机箱号").WithHeader("机箱号").EditAsText();
		editorOptionBuilder.AddProperty<string>("槽位号").WithHeader("槽位号").EditAsText();
		editorOptionBuilder.AddProperty<string>("通道号").WithHeader("通道号").EditAsText();
		editorOptionBuilder.AddProperty<string>("安全分级").WithHeader("安全分级").EditAsText();
		editorOptionBuilder.AddProperty<string>("IO类型").WithHeader("I/O类型").EditAsText();
		editorOptionBuilder.AddProperty<string>("前卡件类型").WithHeader("前卡件类型").EditAsText();
		editorOptionBuilder.AddProperty<string>("后卡件类型").WithHeader("后卡件类型").EditAsText();
		editorOptionBuilder.AddProperty<string>("端子板类型").WithHeader("端子板类型").EditAsText();
		editorOptionBuilder.AddProperty<string>("端子板编号").WithHeader("端子板编号").EditAsText();
		editorOptionBuilder.AddProperty<string>("端子号1").WithHeader("端子号1").EditAsText();
		editorOptionBuilder.AddProperty<string>("端子号2").WithHeader("端子号2").EditAsText();
		editorOptionBuilder.AddProperty<string>("刻度类型").WithHeader("刻度类型").EditAsText();
		editorOptionBuilder.AddProperty<string>("信号特性").WithHeader("信号特性").EditAsText();
		editorOptionBuilder.AddProperty<string>("量程下限").WithHeader("量程下限").EditAsText();
		editorOptionBuilder.AddProperty<string>("超量程下限").WithHeader("超量程下限").EditAsText();
		editorOptionBuilder.AddProperty<string>("量程上限").WithHeader("量程上限").EditAsText();
		editorOptionBuilder.AddProperty<string>("超量程上限").WithHeader("超量程上限").EditAsText();
		editorOptionBuilder.AddProperty<string>("单位").WithHeader("单位").EditAsText();
		editorOptionBuilder.AddProperty<string>("缺省值").WithHeader("缺省值").EditAsText();
		editorOptionBuilder.AddProperty<string>("回路供电").WithHeader("回路供电").EditAsText();
		editorOptionBuilder.AddProperty<string>("电压等级").WithHeader("电压等级").EditAsText();
		editorOptionBuilder.AddProperty<string>("内部外部").WithHeader("内部/外部").EditAsText();
		editorOptionBuilder.AddProperty<string>("典型回路图").WithHeader("典型回路图").EditAsText();
		editorOptionBuilder.AddProperty<string>("源头目的").WithHeader("源头/目的").EditAsText();
		editorOptionBuilder.AddProperty<string>("是否隔离").WithHeader("是否隔离").EditAsText();
		editorOptionBuilder.AddProperty<string>("FDSAMA页码").WithHeader("FD/SAMA页码").EditAsText();
		editorOptionBuilder.AddProperty<string>("版本").WithHeader("版本").EditAsText();
		editorOptionBuilder.AddProperty<string>("传感器类型").WithHeader("传感器类型").EditAsText();
		editorOptionBuilder.AddProperty<string>("分配信息").WithHeader("分配信息").EditAsText();
		editorOptionBuilder.AddProperty<string>("备注").WithHeader("备注").EditAsText();
		return editorOptionBuilder.Build().EditWithWpfUI();
	}

	private async Task Refresh()
	{
		SignalGroupDetails = null;
		if (SignalGroup != null)
		{
			SignalGroupDetails = new ObservableCollection<config_aqj_signalGroupDetail>(await (from x in _003Ccontext_003EP.Db.Queryable<config_aqj_signalGroupDetail>()
				where x.signalGroupId == SignalGroup.Id
				select x).ToListAsync());
		}
	}

	public void OnNavigatedFrom()
	{
	}

	public async void OnNavigatedTo()
	{
		SignalGroup = _003CsignalGroupVm_003EP.SignalGroup ?? throw new InvalidOperationException();
		await Refresh();
	}
}
