using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.Common;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class PublishTerminationViewModel : ObservableObject, INavigationAware
{
	[ObservableProperty]
	private string title;

	[ObservableProperty]
	private string? publishedVersion;

	[ObservableProperty]
	private string? publishedReason;

	[ObservableProperty]
	private string? publisher;

	[ObservableProperty]
	private ObservableCollection<publish_termination>? publishTableData;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.PublishTerminationViewModel.PublishCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? publishCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.PublishTerminationViewModel.ImportPublishCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<publish_termination>? importPublishCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.PublishTerminationViewModel.DeletePublishCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<publish_termination>? deletePublishCommand;

	public Func<int?, Task> saveAction { get; set; }

	public Func<int, int, Task> downloadAndCoverAction { get; set; }

	public int? SubProjectId { get; set; }

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.PublishTerminationViewModel.title" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.PublishTerminationViewModel.publishedVersion" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.PublishTerminationViewModel.publishedReason" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.PublishTerminationViewModel.publisher" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.PublishTerminationViewModel.publishTableData" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<publish_termination>? PublishTableData
	{
		get
		{
			return publishTableData;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<publish_termination>>.Default.Equals(publishTableData, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PublishTableData);
				publishTableData = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PublishTableData);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.PublishTerminationViewModel.Publish" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand PublishCommand => publishCommand ?? (publishCommand = new AsyncRelayCommand(Publish));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.PublishTerminationViewModel.ImportPublish(IODataPlatform.Models.DBModels.publish_termination)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<publish_termination> ImportPublishCommand => importPublishCommand ?? (importPublishCommand = new AsyncRelayCommand<publish_termination>(ImportPublish));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.PublishTerminationViewModel.DeletePublish(IODataPlatform.Models.DBModels.publish_termination)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<publish_termination> DeletePublishCommand => deletePublishCommand ?? (deletePublishCommand = new AsyncRelayCommand<publish_termination>(DeletePublish));

	public PublishTerminationViewModel(SqlSugarContext context, IMessageService message, INavigationService navigation, GlobalModel model, StorageService storage)
	{
		_003Ccontext_003EP = context;
		_003Cmessage_003EP = message;
		_003Cmodel_003EP = model;
		_003Cstorage_003EP = storage;
		title = string.Empty;
		base._002Ector();
	}

	public void OnNavigatedFrom()
	{
	}

	public void OnNavigatedTo()
	{
		ReloadData();
	}

	private async void ReloadData()
	{
		List<publish_termination> list = await (from x in _003Ccontext_003EP.Db.Queryable<publish_termination>()
			where (int?)x.SubProjectId == SubProjectId
			select x).ToListAsync();
		ObservableCollection<publish_termination> observableCollection = new ObservableCollection<publish_termination>();
		foreach (publish_termination item in list)
		{
			observableCollection.Add(item);
		}
		PublishTableData = observableCollection;
	}

	[RelayCommand]
	private async Task Publish()
	{
		_ = SubProjectId ?? throw new Exception("开发人员注意");
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
		_003Cmodel_003EP.Status.Busy("正在发布版本……");
		if ((await (from x in _003Ccontext_003EP.Db.Queryable<publish_termination>()
			where (int?)x.SubProjectId == SubProjectId
			select x.PublishedVersion).ToListAsync()).Contains(PublishedVersion))
		{
			throw new Exception("版本已存在：" + PublishedVersion);
		}
		publish_termination publish = new publish_termination
		{
			SubProjectId = SubProjectId.Value,
			PublishedVersion = PublishedVersion,
			PublishedTime = DateTime.Now,
			PublishedReason = PublishedReason,
			Publisher = Publisher
		};
		await _003Ccontext_003EP.Db.Insertable(publish).ExecuteCommandIdentityIntoEntityAsync();
		await saveAction(publish.Id);
		_003Cmodel_003EP.Status.Success("发布成功");
		ReloadData();
	}

	[RelayCommand]
	private async Task ImportPublish(publish_termination data)
	{
		if (await _003Cmessage_003EP.ConfirmAsync("确认操作\r\n此操作使用已发布版本覆盖当前实时数据"))
		{
			List<publish_termination> source = await (from x in _003Ccontext_003EP.Db.Queryable<publish_termination>()
				where (int?)x.SubProjectId == SubProjectId
				select x).ToListAsync();
			_003Cmodel_003EP.Status.Busy("正在提取发布版本……");
			int id = source.Single((publish_termination x) => x.PublishedVersion == data.PublishedVersion).Id;
			await downloadAndCoverAction(SubProjectId.Value, id);
			_003Cmodel_003EP.Status.Success("覆盖成功");
		}
	}

	[RelayCommand]
	private async Task DeletePublish(publish_termination data)
	{
		if (await _003Cmessage_003EP.ConfirmAsync("是否删除该版本"))
		{
			await (from x in _003Ccontext_003EP.Db.Deleteable<publish_termination>()
				where x.Id == data.Id
				select x).ExecuteCommandAsync();
			await _003Cstorage_003EP.DeleteSubprojectPublishFolderAsync(data.SubProjectId, data.Id);
			ReloadData();
			_003Cmodel_003EP.Status.Success("删除成功");
			_003Cmodel_003EP.Status.Reset();
		}
	}
}
