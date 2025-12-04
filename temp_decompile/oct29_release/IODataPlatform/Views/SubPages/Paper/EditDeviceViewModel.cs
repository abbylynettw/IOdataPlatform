using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Cells;
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

namespace IODataPlatform.Views.SubPages.Paper;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class EditDeviceViewModel(PaperViewModel paper, GlobalModel model, IPickerService picker, ExcelService excel, IMessageService message, SqlSugarContext context) : ObservableObject(), INavigationAware
{
	[ObservableProperty]
	private config_project? project;

	[ObservableProperty]
	private ObservableCollection<盘箱柜>? _data;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Paper.EditDeviceViewModel.ImportCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? importCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Paper.EditDeviceViewModel.DownloadTemplateCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? downloadTemplateCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Paper.EditDeviceViewModel.AddCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Paper.EditDeviceViewModel.EditCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<盘箱柜>? editCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Paper.EditDeviceViewModel.DeleteCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<盘箱柜>? deleteCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Paper.EditDeviceViewModel.project" />
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
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Project);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Paper.EditDeviceViewModel._data" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<盘箱柜>? Data
	{
		get
		{
			return _data;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<盘箱柜>>.Default.Equals(_data, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Data);
				_data = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Data);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Paper.EditDeviceViewModel.Import" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ImportCommand => importCommand ?? (importCommand = new AsyncRelayCommand(Import));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Paper.EditDeviceViewModel.DownloadTemplate" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand DownloadTemplateCommand => downloadTemplateCommand ?? (downloadTemplateCommand = new RelayCommand(DownloadTemplate));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Paper.EditDeviceViewModel.Add" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddCommand => addCommand ?? (addCommand = new AsyncRelayCommand(Add));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Paper.EditDeviceViewModel.Edit(IODataPlatform.Models.DBModels.盘箱柜)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<盘箱柜> EditCommand => editCommand ?? (editCommand = new AsyncRelayCommand<盘箱柜>(Edit));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Paper.EditDeviceViewModel.Delete(IODataPlatform.Models.DBModels.盘箱柜)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<盘箱柜> DeleteCommand => deleteCommand ?? (deleteCommand = new AsyncRelayCommand<盘箱柜>(Delete));

	[RelayCommand]
	private async Task Import()
	{
		string text = picker.OpenFile("Excel 文件(*.xlsx; *.xls)|*.xlsx; *.xls");
		if (text == null)
		{
			return;
		}
		model.Status.Busy("正在导入……");
		int projectId = Project.Id;
		using DataTable data = await excel.GetDataTableAsStringAsync(text, hasHeader: true);
		List<盘箱柜> list = data.StringTableToIEnumerableByDiplay<盘箱柜>().ToList().AllDo(delegate(盘箱柜 x)
		{
			x.项目Id = projectId;
		})
			.ToList();
		await context.Db.Insertable(list).ExecuteCommandIdentityIntoEntityAsync();
		ObservableCollection<盘箱柜> observableCollection = new ObservableCollection<盘箱柜>();
		foreach (盘箱柜 datum in Data)
		{
			observableCollection.Add(datum);
		}
		foreach (盘箱柜 item in list)
		{
			observableCollection.Add(item);
		}
		Data = observableCollection;
		model.Status.Reset();
	}

	[RelayCommand]
	private void DownloadTemplate()
	{
		string text = picker.SaveFile("Excel 文件(*.xlsx; *.xls)|*.xlsx; *.xls", "盘箱柜批量导入模板");
		if (text == null)
		{
			return;
		}
		using Workbook workbook = excel.GetWorkbook();
		using Worksheet worksheet = workbook.Worksheets[0];
		worksheet.Cells[0, 0].Value = "名称";
		worksheet.Cells[0, 1].Value = "类别";
		worksheet.Cells[0, 2].Value = "子类别";
		worksheet.Cells[0, 3].Value = "房间号";
		worksheet.Cells[0, 4].Value = "内部编码";
		worksheet.Cells[0, 5].Value = "外部编码";
		worksheet.Cells[0, 6].Value = "子项";
		worksheet.Cells[0, 7].Value = "LOT";
		worksheet.Cells[0, 8].Value = "Batch";
		workbook.Save(text);
	}

	[RelayCommand]
	private async Task Add()
	{
		盘箱柜 obj = new 盘箱柜
		{
			项目Id = Project.Id
		};
		if (EditWithEditor(obj, "添加"))
		{
			await context.Db.Insertable(obj).ExecuteCommandIdentityIntoEntityAsync();
			Data.Add(obj);
		}
	}

	[RelayCommand]
	private async Task Edit(盘箱柜 data)
	{
		盘箱柜 obj = new 盘箱柜().CopyPropertiesFrom(data);
		if (!EditWithEditor(obj, "编辑"))
		{
			return;
		}
		await context.Db.Updateable(obj).ExecuteCommandAsync();
		data.CopyPropertiesFrom(obj);
		ObservableCollection<盘箱柜> observableCollection = new ObservableCollection<盘箱柜>();
		foreach (盘箱柜 datum in Data)
		{
			observableCollection.Add(datum);
		}
		Data = observableCollection;
	}

	[RelayCommand]
	private async Task Delete(盘箱柜 data)
	{
		if (await message.ConfirmAsync("确认删除"))
		{
			await context.Db.Deleteable(data).ExecuteCommandAsync();
			Data.Remove(data);
		}
	}

	public void OnNavigatedFrom()
	{
	}

	public async void OnNavigatedTo()
	{
		Project = paper.Project ?? throw new Exception();
		ObservableCollection<盘箱柜> observableCollection = new ObservableCollection<盘箱柜>();
		foreach (盘箱柜 item in await (from x in context.Db.Queryable<盘箱柜>()
			where x.项目Id == Project.Id
			select x).ToListAsync())
		{
			observableCollection.Add(item);
		}
		Data = observableCollection;
	}

	private static bool EditWithEditor(盘箱柜 data, string title)
	{
		EditorOptionBuilder<盘箱柜> editorOptionBuilder = data.CreateEditorBuilder();
		editorOptionBuilder.WithTitle(title).WithObject(data).WithEditorHeight(660.0)
			.WithValidator(delegate(盘箱柜 x)
			{
				if (string.IsNullOrEmpty(x.名称))
				{
					return "请输入盘箱柜名称";
				}
				if (string.IsNullOrEmpty(x.房间号))
				{
					return "请输入房间号";
				}
				if (string.IsNullOrEmpty(x.内部编码))
				{
					return "请输入内部编码";
				}
				if (string.IsNullOrEmpty(x.外部编码))
				{
					return "请输入外部编码";
				}
				if (string.IsNullOrEmpty(x.LOT))
				{
					return "请输入LOT";
				}
				return string.IsNullOrEmpty(x.Batch) ? "请输入Batch" : string.Empty;
			});
		editorOptionBuilder.AddProperty<string>("名称").WithHeader("名称").EditAsText()
			.ShorterThan(50);
		editorOptionBuilder.AddProperty<盘箱柜类别>("类别").WithHeader("类别").EditAsCombo<盘箱柜类别>()
			.WithOptions<盘箱柜类别>();
		editorOptionBuilder.AddProperty<string>("子类别").WithHeader("子类别").EditAsText()
			.ShorterThan(50);
		editorOptionBuilder.AddProperty<string>("房间号").WithHeader("房间号").EditAsText()
			.ShorterThan(20);
		editorOptionBuilder.AddProperty<string>("内部编码").WithHeader("内部编码").EditAsText()
			.ShorterThan(50);
		editorOptionBuilder.AddProperty<string>("外部编码").WithHeader("外部编码").EditAsText()
			.ShorterThan(50);
		editorOptionBuilder.AddProperty<string>("子项").WithHeader("子项").EditAsText()
			.ShorterThan(50);
		editorOptionBuilder.AddProperty<string>("LOT").WithHeader("LOT").EditAsText()
			.ShorterThan(50);
		editorOptionBuilder.AddProperty<string>("Batch").WithHeader("Batch").EditAsText()
			.ShorterThan(50);
		return editorOptionBuilder.Build().EditWithWpfUI();
	}
}
