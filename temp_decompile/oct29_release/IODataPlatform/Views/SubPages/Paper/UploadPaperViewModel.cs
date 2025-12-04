using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using IODataPlatform.Views.Pages;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.Paper;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class UploadPaperViewModel(SqlSugarContext context, StorageService storage, PaperViewModel paper, IMessageService message, GlobalModel model, IPickerService picker) : ObservableObject(), INavigationAware
{
	[ObservableProperty]
	private config_project? project;

	[ObservableProperty]
	private ObservableCollection<盘箱柜>? configs;

	[ObservableProperty]
	private ObservableCollection<待添加图纸文件夹信息> _data = new ObservableCollection<待添加图纸文件夹信息>();

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Paper.UploadPaperViewModel.AddCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Paper.UploadPaperViewModel.DeleteCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<待添加图纸文件夹信息>? deleteCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Paper.UploadPaperViewModel.UploadCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? uploadCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Paper.UploadPaperViewModel.project" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Paper.UploadPaperViewModel.configs" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<盘箱柜>? Configs
	{
		get
		{
			return configs;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<盘箱柜>>.Default.Equals(configs, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Configs);
				configs = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Configs);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Paper.UploadPaperViewModel._data" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<待添加图纸文件夹信息> Data
	{
		get
		{
			return _data;
		}
		[MemberNotNull("_data")]
		set
		{
			if (!EqualityComparer<ObservableCollection<待添加图纸文件夹信息>>.Default.Equals(_data, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Data);
				_data = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Data);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Paper.UploadPaperViewModel.Add" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddCommand => addCommand ?? (addCommand = new AsyncRelayCommand(Add));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Paper.UploadPaperViewModel.Delete(IODataPlatform.Views.SubPages.Paper.待添加图纸文件夹信息)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<待添加图纸文件夹信息> DeleteCommand => deleteCommand ?? (deleteCommand = new AsyncRelayCommand<待添加图纸文件夹信息>(Delete));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Paper.UploadPaperViewModel.Upload" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand UploadCommand => uploadCommand ?? (uploadCommand = new AsyncRelayCommand(Upload));

	[RelayCommand]
	private async Task Add()
	{
		string folder = picker.PickFolder();
		if (folder == null)
		{
			return;
		}
		model.Status.Busy("正在分析全部子文件夹……");
		foreach (待添加图纸文件夹信息 item2 in await Task.Run(delegate
		{
			List<待添加图纸文件夹信息> list = new List<待添加图纸文件夹信息>();
			foreach (string item in Directory.GetDirectories(folder, "*", new EnumerationOptions
			{
				RecurseSubdirectories = true
			}).Append(folder))
			{
				if (!Data.Select((待添加图纸文件夹信息 x) => x.DirectoryPath.ToLower()).Contains(item.ToLower()))
				{
					待添加图纸文件夹信息 待添加图纸文件夹信息2 = new 待添加图纸文件夹信息(item, Configs ?? throw new Exception());
					if (待添加图纸文件夹信息2.IsSuccess)
					{
						list.Add(待添加图纸文件夹信息2);
					}
				}
			}
			return list;
		}))
		{
			Data.Add(item2);
		}
		model.Status.Reset();
	}

	[RelayCommand]
	private async Task Delete(待添加图纸文件夹信息 data)
	{
		if (await message.ConfirmAsync("确认移除"))
		{
			Data.Remove(data);
		}
	}

	[RelayCommand]
	private async Task Upload()
	{
		if (!(await message.ConfirmAsync("确认上传\r\n上传过程较慢，请耐心等待")))
		{
			return;
		}
		int total = Data.Count;
		for (int i = 0; i < Data.Count; i++)
		{
			待添加图纸文件夹信息 data = Data[i];
			model.Status.Busy($"正在上传 {i + 1} / {total}：{data.FullName}");
			图纸 paper = new 图纸
			{
				PDF文件名 = data.FullName + ".pdf",
				版本 = data.Version,
				压缩包文件名 = data.FullName + ".zip",
				名称 = data.FullName,
				盘箱柜表Id = data.盘箱柜Id,
				发布日期 = data.PublishDate
			};
			List<图纸> list = (from x in context.Db.Queryable<图纸>()
				where x.盘箱柜表Id == paper.盘箱柜表Id
				where x.版本 == paper.版本
				select x).ToList();
			if (list.Count > 0)
			{
				foreach (图纸 old in list)
				{
					try
					{
						await storage.WebDeleteFolderAsync(storage.GetPaperFolderRelativePath(old));
					}
					catch
					{
					}
					await context.Db.Deleteable(old).ExecuteCommandAsync();
				}
			}
			await context.Db.Insertable(paper).ExecuteCommandIdentityIntoEntityAsync();
			string webFileLocalAbsolutePath = storage.GetWebFileLocalAbsolutePath(storage.GetPaperZipFileRelativePath(paper));
			await ZipFolderAsync(data.DirectoryPath, webFileLocalAbsolutePath);
			await storage.UploadPaperZipFileAsync(paper);
			string webFileLocalAbsolutePath2 = storage.GetWebFileLocalAbsolutePath(storage.GetPaperPdfFileRelativePath(paper));
			File.Copy(Path.Combine(data.DirectoryPath, data.PdfFileName), webFileLocalAbsolutePath2);
			await storage.UploadPaperPdfFileAsync(paper);
		}
		model.Status.Success("全部上传成功");
		Data.Clear();
	}

	private async Task ZipFolderAsync(string directoryPath, string zipPath)
	{
		if (File.Exists(zipPath))
		{
			File.Delete(zipPath);
		}
		if (!Directory.Exists(directoryPath))
		{
			throw new ArgumentException("The specified directory does not exist", "directoryPath");
		}
		string directoryName = Path.GetDirectoryName(zipPath);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		await Task.Run(delegate
		{
			ZipFile.CreateFromDirectory(directoryPath, zipPath);
		});
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
		Configs = observableCollection;
	}
}
