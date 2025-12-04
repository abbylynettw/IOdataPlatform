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
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.AQJ;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class ImportViewModel(SqlSugarContext context, IMessageService message, GlobalModel model, DepAQJViewModel aqj, ExcelService excel, IPickerService picker, INavigationService navigation) : ObservableObject(), INavigationAware
{
	private List<IoFullData>? oldData;

	private readonly List<IoFullData> newData = new List<IoFullData>();

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.AQJ.ImportViewModel.ImportFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? importFileCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.AQJ.ImportViewModel.ViewDataCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<DifferentObject<string>>? viewDataCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.AQJ.ImportViewModel.ConfirmCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? confirmCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.AQJ.ImportViewModel.ConvertBackCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? convertBackCommand;

	public ObservableCollection<DifferentObject<string>> DiffObjects { get; } = new ObservableCollection<DifferentObject<string>>();

	public ObservableCollection<DifferentProperty> DiffProps { get; } = new ObservableCollection<DifferentProperty>();

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.AQJ.ImportViewModel.ImportFile" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ImportFileCommand => importFileCommand ?? (importFileCommand = new AsyncRelayCommand(ImportFile));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.AQJ.ImportViewModel.ViewData(IODataPlatform.Utilities.DifferentObject{System.String})" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<DifferentObject<string>> ViewDataCommand => viewDataCommand ?? (viewDataCommand = new RelayCommand<DifferentObject<string>>(ViewData));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.AQJ.ImportViewModel.Confirm" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ConfirmCommand => confirmCommand ?? (confirmCommand = new AsyncRelayCommand(Confirm));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.AQJ.ImportViewModel.ConvertBack" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ConvertBackCommand => convertBackCommand ?? (convertBackCommand = new RelayCommand(ConvertBack));

	public void OnNavigatedFrom()
	{
	}

	public void OnNavigatedTo()
	{
		if (aqj.Project == null)
		{
			throw new Exception("开发人员注意");
		}
		if (aqj.SubProject == null)
		{
			throw new Exception("开发人员注意");
		}
		if (aqj.AllData == null)
		{
			throw new Exception("开发人员注意");
		}
		oldData = aqj.AllData.ToList();
	}

	[RelayCommand]
	private async Task ImportFile()
	{
		string text = picker.OpenFile("Excel 文件(*.xls; *.xlsx)|*.xls; *.xlsx");
		if (text == null)
		{
			return;
		}
		if (oldData == null)
		{
			throw new Exception();
		}
		if (aqj.SubProject == null)
		{
			throw new Exception();
		}
		model.Status.Busy("正在获取数据……");
		DiffObjects.Clear();
		DiffProps?.Clear();
		using DataTable dataTable = await excel.GetDataTableAsStringAsync(text, hasHeader: true);
		ControlSystem controlSystem = (from it in context.Db.Queryable<config_project_major>()
			where it.Id == aqj.SubProject.MajorId
			select it).First().ControlSystem;
		newData.Reset(dataTable.ConvertOldDataTableToIoFullData(message, context.Db, controlSystem));
		model.Status.Busy("正在比对数据……");
		ICollection<DifferentObject<string>> diffObjects = DiffObjects;
		diffObjects.Reset(await DataComparator.ComparerAsync(newData, oldData, (IoFullData x) => x.TagName ?? ""));
		model.Status.Reset();
	}

	[RelayCommand]
	private void ViewData(DifferentObject<string> obj)
	{
		DiffProps.Reset(obj.DiffProps);
	}

	[RelayCommand]
	private async Task Confirm()
	{
		if (!(await message.ConfirmAsync("确认操作\r\n将会覆盖之前子项的全部内容")))
		{
			return;
		}
		model.Status.Success("正在提交数据……");
		DepAQJViewModel depAQJViewModel = aqj;
		ObservableCollection<IoFullData> observableCollection = new ObservableCollection<IoFullData>();
		foreach (IoFullData newDatum in newData)
		{
			observableCollection.Add(newDatum);
		}
		depAQJViewModel.AllData = observableCollection;
		await aqj.SaveAndUploadFileAsync();
		model.Status.Success("导入成功");
		navigation.GoBack();
	}

	[RelayCommand]
	private void ConvertBack()
	{
		navigation.GoBack();
	}
}
