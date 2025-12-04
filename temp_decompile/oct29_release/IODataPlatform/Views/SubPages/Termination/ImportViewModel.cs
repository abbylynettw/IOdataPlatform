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
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.Termination;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class ImportViewModel(GlobalModel model, TerminationViewModel termination, ExcelService excel, IPickerService picker, INavigationService navigation) : ObservableObject(), INavigationAware
{
	private readonly List<TerminationData> oldData = (termination.AllData ?? throw new Exception("开发人员注意")).ToList();

	private readonly List<TerminationData> newData = new List<TerminationData>();

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Termination.ImportViewModel.ImportFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? importFileCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Termination.ImportViewModel.ViewDataCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<DifferentObject<string>>? viewDataCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Termination.ImportViewModel.ConfirmCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? confirmCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Termination.ImportViewModel.ConvertBackCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? convertBackCommand;

	public ObservableCollection<DifferentObject<string>> DiffObjects { get; } = new ObservableCollection<DifferentObject<string>>();

	public ObservableCollection<DifferentProperty> DiffProps { get; } = new ObservableCollection<DifferentProperty>();

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Termination.ImportViewModel.ImportFile" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ImportFileCommand => importFileCommand ?? (importFileCommand = new AsyncRelayCommand(ImportFile));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Termination.ImportViewModel.ViewData(IODataPlatform.Utilities.DifferentObject{System.String})" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<DifferentObject<string>> ViewDataCommand => viewDataCommand ?? (viewDataCommand = new RelayCommand<DifferentObject<string>>(ViewData));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Termination.ImportViewModel.Confirm" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ConfirmCommand => confirmCommand ?? (confirmCommand = new AsyncRelayCommand(Confirm));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Termination.ImportViewModel.ConvertBack" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ConvertBackCommand => convertBackCommand ?? (convertBackCommand = new RelayCommand(ConvertBack));

	public void OnNavigatedFrom()
	{
	}

	public void OnNavigatedTo()
	{
		if (termination.Project == null)
		{
			throw new Exception("开发人员注意");
		}
		if (termination.SubProject == null)
		{
			throw new Exception("开发人员注意");
		}
		if (termination.AllData == null)
		{
			throw new Exception("开发人员注意");
		}
	}

	[RelayCommand]
	private async Task ImportFile()
	{
		string text = picker.OpenFile("Excel 文件(*.xls; *.xlsx)|*.xls; *.xlsx");
		if (text == null)
		{
			return;
		}
		model.Status.Busy("正在获取数据……");
		DiffObjects.Clear();
		DiffProps?.Clear();
		using DataTable dataTable = await excel.GetDataTableAsStringAsync(text, hasHeader: true);
		newData.Reset(dataTable.StringTableToIEnumerableByDiplay<TerminationData>());
		model.Status.Busy("正在比对数据……");
		ICollection<DifferentObject<string>> diffObjects = DiffObjects;
		diffObjects.Reset(await DataComparator.ComparerAsync(newData, oldData, (TerminationData x) => x.IOPointName ?? ""));
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
		model.Status.Success("正在提交数据……");
		termination.AllData = new ObservableCollection<TerminationData>(newData);
		await termination.SaveAndUploadRealtimeFileAsync();
		model.Status.Success("导入成功");
		navigation.GoBack();
	}

	[RelayCommand]
	private void ConvertBack()
	{
		navigation.GoBack();
	}
}
