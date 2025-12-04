using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.Cable;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class PublishViewModel(CableViewModel cable, SqlSugarContext context, IMessageService message, StorageService storage, INavigationService navigation, GlobalModel model) : ObservableObject(), INavigationAware
{
	[ObservableProperty]
	private string title = "电缆发布";

	[ObservableProperty]
	private string? publishedVersion;

	[ObservableProperty]
	private string? publishedReason;

	[ObservableProperty]
	private string? publisher;

	[ObservableProperty]
	private ObservableCollection<publish_cable>? publishTableData;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Cable.PublishViewModel.PublishCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? publishCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Cable.PublishViewModel.ImportPublishCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<publish_cable>? importPublishCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Cable.PublishViewModel.DeletePublishCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<publish_cable>? deletePublishCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.PublishViewModel.title" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Title
	{
		get
		{
			return title;
		}
		[MemberNotNull("title")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(title, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Title);
				title = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Title);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.PublishViewModel.publishedVersion" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string? PublishedVersion
	{
		get
		{
			return publishedVersion;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(publishedVersion, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PublishedVersion);
				publishedVersion = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PublishedVersion);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.PublishViewModel.publishedReason" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string? PublishedReason
	{
		get
		{
			return publishedReason;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(publishedReason, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PublishedReason);
				publishedReason = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PublishedReason);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.PublishViewModel.publisher" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string? Publisher
	{
		get
		{
			return publisher;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(publisher, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Publisher);
				publisher = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Publisher);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Cable.PublishViewModel.publishTableData" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<publish_cable>? PublishTableData
	{
		get
		{
			return publishTableData;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<publish_cable>>.Default.Equals(publishTableData, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PublishTableData);
				publishTableData = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PublishTableData);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Cable.PublishViewModel.Publish" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand PublishCommand => publishCommand ?? (publishCommand = new AsyncRelayCommand(Publish));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Cable.PublishViewModel.ImportPublish(IODataPlatform.Models.DBModels.publish_cable)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<publish_cable> ImportPublishCommand => importPublishCommand ?? (importPublishCommand = new AsyncRelayCommand<publish_cable>(ImportPublish));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Cable.PublishViewModel.DeletePublish(IODataPlatform.Models.DBModels.publish_cable)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<publish_cable> DeletePublishCommand => deletePublishCommand ?? (deletePublishCommand = new AsyncRelayCommand<publish_cable>(DeletePublish));

	public void OnNavigatedFrom()
	{
	}

	public void OnNavigatedTo()
	{
		ReloadData();
	}

	private async void ReloadData()
	{
		int[] ids = new int[2]
		{
			(cable.SubProject1 ?? throw new Exception()).Id,
			(cable.SubProject2 ?? throw new Exception()).Id
		};
		List<publish_cable> list = await (from x in context.Db.Queryable<publish_cable>()
			where x.SubProjectId1 == ids.Min()
			where x.SubProjectId2 == ids.Max()
			select x).ToListAsync();
		ObservableCollection<publish_cable> observableCollection = new ObservableCollection<publish_cable>();
		foreach (publish_cable item in list)
		{
			observableCollection.Add(item);
		}
		PublishTableData = observableCollection;
	}

	[RelayCommand]
	private async Task Publish()
	{
		int[] ids = new int[2]
		{
			(cable.SubProject1 ?? throw new Exception()).Id,
			(cable.SubProject2 ?? throw new Exception()).Id
		};
		if (string.IsNullOrWhiteSpace(PublishedVersion))
		{
			throw new Exception("请输入发布版本号");
		}
		if (string.IsNullOrWhiteSpace(PublishedReason))
		{
			throw new Exception("请输入发布原因");
		}
		if (string.IsNullOrWhiteSpace(Publisher))
		{
			throw new Exception("请输入发布人");
		}
		model.Status.Busy("正在发布版本……");
		if ((await (from x in context.Db.Queryable<publish_cable>()
			where x.SubProjectId1 == ids.Min()
			where x.SubProjectId2 == ids.Max()
			select x.PublishedVersion).ToListAsync()).Contains(PublishedVersion))
		{
			throw new Exception("版本已存在：" + PublishedVersion);
		}
		publish_cable publish = new publish_cable
		{
			SubProjectId1 = ids.Min(),
			SubProjectId2 = ids.Max(),
			PublishedVersion = PublishedVersion,
			PublishedTime = DateTime.Now,
			PublishedReason = PublishedReason,
			Publisher = Publisher
		};
		await context.Db.Insertable(publish).ExecuteCommandIdentityIntoEntityAsync();
		await cable.SaveAndUploadRealtimeFileAsync(publish.Id);
		model.Status.Success("发布成功");
		model.Status.Reset();
		ReloadData();
	}

	[RelayCommand]
	private async Task ImportPublish(publish_cable data)
	{
		if (await message.ConfirmAsync("确认操作\r\n此操作使用已发布版本覆盖当前实时数据"))
		{
			model.Status.Busy("正在提取发布版本……");
			int id1 = data.SubProjectId1;
			int id2 = data.SubProjectId2;
			int versionId = data.Id;
			string sourceFileName = await storage.DownloadPublishCableFileAsync(id1, id2, versionId);
			string realtimeCableFileRelativePath = storage.GetRealtimeCableFileRelativePath(id1, id2);
			string webFileLocalAbsolutePath = storage.GetWebFileLocalAbsolutePath(realtimeCableFileRelativePath);
			File.Copy(sourceFileName, webFileLocalAbsolutePath);
			await storage.WebCopyFilesAsync(new _003C_003Ez__ReadOnlySingleElementList<(string, string)>((storage.GetPublishCableFileRelativePath(id1, id2, versionId), realtimeCableFileRelativePath)));
			await cable.ReloadAllData();
			model.Status.Success("覆盖成功");
			navigation.GoBack();
		}
	}

	[RelayCommand]
	private async Task DeletePublish(publish_cable data)
	{
		if (await message.ConfirmAsync("是否删除该版本"))
		{
			await (from x in context.Db.Deleteable<publish_cable>()
				where x.Id == data.Id
				select x).ExecuteCommandAsync();
			await storage.DeleteCableSubprojectPublishFolderAsync(data.SubProjectId1, data.SubProjectId2, data.Id);
			ReloadData();
			model.Status.Success("删除成功");
			model.Status.Reset();
		}
	}
}
