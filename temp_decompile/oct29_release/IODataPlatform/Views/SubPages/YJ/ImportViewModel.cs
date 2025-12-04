using System.CodeDom.Compiler;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Cells;
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

namespace IODataPlatform.Views.SubPages.YJ;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class ImportViewModel : ObservableObject, INavigationAware
{
	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.YJ.ImportViewModel.ImportFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? importFileCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.YJ.ImportViewModel.ViewDataCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<DifferentObject<string>>? viewDataCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.YJ.ImportViewModel.ConfirmCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? confirmCommand;

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.YJ.ImportViewModel.ImportFile" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ImportFileCommand => importFileCommand ?? (importFileCommand = new AsyncRelayCommand(ImportFile));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.YJ.ImportViewModel.ViewData(IODataPlatform.Utilities.DifferentObject{System.String})" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<DifferentObject<string>> ViewDataCommand => viewDataCommand ?? (viewDataCommand = new RelayCommand<DifferentObject<string>>(ViewData));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.YJ.ImportViewModel.Confirm" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ConfirmCommand => confirmCommand ?? (confirmCommand = new AsyncRelayCommand(Confirm));

	public ImportViewModel(GlobalModel model, DepYJViewModel yj, ExcelService excel, IPickerService picker, INavigationService navigation)
	{
		_003Cmodel_003EP = model;
		_003Cexcel_003EP = excel;
		_003Cpicker_003EP = picker;
		_003Cnavigation_003EP = navigation;
		base._002Ector();
	}

	public void OnNavigatedFrom()
	{
	}

	public void OnNavigatedTo()
	{
	}

	[RelayCommand]
	private async Task ImportFile()
	{
		string file = _003Cpicker_003EP.OpenFile("Excel 文件(*.xls; *.xlsx)|*.xls; *.xlsx");
		if (file == null)
		{
			return;
		}
		_003Cmodel_003EP.Status.Busy("正在获取数据……");
		await Task.Run(() => (from x in (from x in _003Cexcel_003EP.GetWorkbook(file).Worksheets.ExceptBy(new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "目录", "说明" }), (Worksheet x) => x.Name).AsParallel()
				select x.Cells.ExportDataTableAsString(2, 1, x.Cells.MaxDataRow - 1, 9)).SelectMany((DataTable x) => x.Rows.Cast<DataRow>())
			where $"{x[6]}" != "-"
			select new 内部接线清单
			{
				位置 = $"{x[0]}",
				典回类型 = $"{x[6]}",
				信号名称 = $"{x[2]}",
				安全等级 = $"{x[4]}",
				安全列 = $"{x[5]}",
				端子组名 = $"{x[7]}",
				端子号 = $"{x[8]}"
			}).ToList());
		_003Cmodel_003EP.Status.Reset();
	}

	[RelayCommand]
	private void ViewData(DifferentObject<string> obj)
	{
	}

	[RelayCommand]
	private async Task Confirm()
	{
		_003Cmodel_003EP.Status.Success("正在提交数据……");
		_003Cmodel_003EP.Status.Success("导入成功");
		_003Cnavigation_003EP.GoBack();
	}
}
