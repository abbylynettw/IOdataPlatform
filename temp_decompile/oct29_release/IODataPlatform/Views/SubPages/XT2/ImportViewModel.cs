using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;
using IODataPlatform.Views.SubPages.Common;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.XT2;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class ImportViewModel : ObservableObject, INavigationAware
{
	private List<IoFullData>? oldData;

	private readonly List<IoFullData> newData;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.XT2.ImportViewModel.ImportFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? importFileCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.XT2.ImportViewModel.ViewDataCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<DifferentObject<string>>? viewDataCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.XT2.ImportViewModel.ConfirmCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? confirmCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.XT2.ImportViewModel.ConvertBackCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? convertBackCommand;

	public ObservableCollection<DifferentObject<string>> DiffObjects { get; }

	public ObservableCollection<DifferentProperty> DiffProps { get; }

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.XT2.ImportViewModel.ImportFile" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ImportFileCommand => importFileCommand ?? (importFileCommand = new AsyncRelayCommand(ImportFile));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.XT2.ImportViewModel.ViewData(IODataPlatform.Utilities.DifferentObject{System.String})" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<DifferentObject<string>> ViewDataCommand => viewDataCommand ?? (viewDataCommand = new RelayCommand<DifferentObject<string>>(ViewData));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.XT2.ImportViewModel.Confirm" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ConfirmCommand => confirmCommand ?? (confirmCommand = new AsyncRelayCommand(Confirm));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.XT2.ImportViewModel.ConvertBack" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ConvertBackCommand => convertBackCommand ?? (convertBackCommand = new RelayCommand(ConvertBack));

	public ImportViewModel(SqlSugarContext context, IMessageService message, GlobalModel model, DepXT2ViewModel xt2, ExcelService excel, IPickerService picker, INavigationService navigation, IContentDialogService dialogService, SelectExcelSheetDialogViewModel vm)
	{
		_003Ccontext_003EP = context;
		_003Cmessage_003EP = message;
		_003Cmodel_003EP = model;
		_003Cxt2_003EP = xt2;
		_003Cexcel_003EP = excel;
		_003Cnavigation_003EP = navigation;
		_003CdialogService_003EP = dialogService;
		_003Cvm_003EP = vm;
		DiffObjects = new ObservableCollection<DifferentObject<string>>();
		DiffProps = new ObservableCollection<DifferentProperty>();
		newData = new List<IoFullData>();
		base._002Ector();
	}

	public void OnNavigatedFrom()
	{
	}

	public void OnNavigatedTo()
	{
		if (_003Cxt2_003EP.Project == null)
		{
			throw new Exception("开发人员注意");
		}
		if (_003Cxt2_003EP.SubProject == null)
		{
			throw new Exception("开发人员注意");
		}
		if (_003Cxt2_003EP.AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		oldData = _003Cxt2_003EP.AllData.ToList();
	}

	[RelayCommand]
	private async Task ImportFile()
	{
		if (oldData == null)
		{
			throw new Exception();
		}
		if (_003Cxt2_003EP.Major == null)
		{
			throw new Exception();
		}
		if (_003Cxt2_003EP.Project == null)
		{
			throw new Exception();
		}
		SelectExcelSheetDialogViewModel service = App.GetService<SelectExcelSheetDialogViewModel>();
		service.SetCurrentSystemInfo(_003Cxt2_003EP.Major.ControlSystem);
		SelectExcelSheetDialog selectExcelSheetDialog = new SelectExcelSheetDialog(service, _003CdialogService_003EP.GetContentPresenter());
		if (await selectExcelSheetDialog.ShowAsync() != ContentDialogResult.Primary)
		{
			return;
		}
		_003Cmodel_003EP.Status.Busy("正在获取数据……");
		DiffObjects.Clear();
		DiffProps?.Clear();
		using DataTable dataTable = await _003Cexcel_003EP.GetDataTableAsStringAsync(_003Cvm_003EP.SelectFilePath, _003Cvm_003EP.SelectedSheetName, hasHeader: true);
		newData.Reset(dataTable.ConvertOldDataTableToIoFullData(_003Cmessage_003EP, _003Ccontext_003EP.Db, _003Cxt2_003EP.Major.ControlSystem));
		_003Cmodel_003EP.Status.Reset();
		await _003Cmessage_003EP.AlertAsync("文件加载完成！\n\n当前已取消对比功能，请直接点击提交导入。");
	}

	[RelayCommand]
	private void ViewData(DifferentObject<string> obj)
	{
		DiffProps.Reset(obj.DiffProps);
	}

	[RelayCommand]
	private async Task Confirm()
	{
		if (!(await _003Cmessage_003EP.ConfirmAsync("确认操作\r\n将会覆盖之前子项的全部内容")))
		{
			return;
		}
		_003Cmodel_003EP.Status.Success("正在提交数据……");
		await _003Ccontext_003EP.Db.Queryable<config_card_type_judge>().ToListAsync();
		DepXT2ViewModel depXT2ViewModel = _003Cxt2_003EP;
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData item in newData.OrderBy((IoFullData n) => n.CabinetNumber))
		{
			observableCollection.Add(item);
		}
		depXT2ViewModel.AllData = observableCollection;
		await _003Cxt2_003EP.SaveAndUploadFileAsync();
		_003Cmodel_003EP.Status.Success("导入成功");
		_003Cnavigation_003EP.GoBack();
	}

	[RelayCommand]
	private void ConvertBack()
	{
		_003Cnavigation_003EP.GoBack();
	}
}
