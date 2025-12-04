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
using IODataPlatform.Views.Pages;
using LYSoft.Libs;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.Project;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class MajorViewModel(SqlSugarContext context, ProjectViewModel projectvm, IMessageService message, INavigationService navigation, GlobalModel model, StorageService storage) : ObservableObject(), INavigationAware
{
	[ObservableProperty]
	private config_project? project;

	[ObservableProperty]
	private config_project_major? major;

	[ObservableProperty]
	private ObservableCollection<config_project_major>? majors;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Project.MajorViewModel.AddCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Project.MajorViewModel.EditCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<config_project_major>? editCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Project.MajorViewModel.DeleteCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<config_project_major>? deleteCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Project.MajorViewModel.ConfigureSubProjectCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<config_project_major>? configureSubProjectCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Project.MajorViewModel.project" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Project.MajorViewModel.major" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public config_project_major? Major
	{
		get
		{
			return major;
		}
		set
		{
			if (!EqualityComparer<config_project_major>.Default.Equals(major, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Major);
				major = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Major);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Project.MajorViewModel.majors" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<config_project_major>? Majors
	{
		get
		{
			return majors;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<config_project_major>>.Default.Equals(majors, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Majors);
				majors = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Majors);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Project.MajorViewModel.Add" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddCommand => addCommand ?? (addCommand = new AsyncRelayCommand(Add));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Project.MajorViewModel.Edit(IODataPlatform.Models.DBModels.config_project_major)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<config_project_major> EditCommand => editCommand ?? (editCommand = new AsyncRelayCommand<config_project_major>(Edit));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Project.MajorViewModel.Delete(IODataPlatform.Models.DBModels.config_project_major)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<config_project_major> DeleteCommand => deleteCommand ?? (deleteCommand = new AsyncRelayCommand<config_project_major>(Delete));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Project.MajorViewModel.ConfigureSubProject(IODataPlatform.Models.DBModels.config_project_major)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<config_project_major> ConfigureSubProjectCommand => configureSubProjectCommand ?? (configureSubProjectCommand = new RelayCommand<config_project_major>(ConfigureSubProject));

	[RelayCommand]
	private async Task Add()
	{
		config_project_major config_project_major = new config_project_major
		{
			ProjectId = (Project ?? throw new Exception()).Id
		};
		if (EditMajor(config_project_major, "添加"))
		{
			await context.Db.Insertable(config_project_major).ExecuteCommandAsync();
			await Refresh();
		}
	}

	[RelayCommand]
	private async Task Edit(config_project_major data)
	{
		config_project_major config_project_major = new config_project_major().CopyPropertiesFrom(data);
		if (EditMajor(config_project_major, "编辑"))
		{
			await context.Db.Updateable(config_project_major).ExecuteCommandAsync();
			await Refresh();
		}
	}

	[RelayCommand]
	private async Task Delete(config_project_major major)
	{
		if (!(await message.ConfirmAsync("确认删除 \"" + major.CableSystem + "\"?\r\n此操作会同时删除该专业下的所有子项目及发布数据。")))
		{
			return;
		}
		model.Status.Busy("正在获取子项目和发布数据……");
		List<config_project_subProject> subProjects = await (from x in context.Db.Queryable<config_project_subProject>()
			where x.MajorId == major.Id
			select x).ToListAsync();
		List<int> subProjectIds = subProjects.Select((config_project_subProject x) => x.Id).ToList();
		List<publish_io> ioPublishes = await (from x in context.Db.Queryable<publish_io>()
			where subProjectIds.Contains(x.SubProjectId)
			select x).ToListAsync();
		List<publish_termination> terminationPublishes = await (from x in context.Db.Queryable<publish_termination>()
			where subProjectIds.Contains(x.SubProjectId)
			select x).ToListAsync();
		model.Status.Busy("正在删除子项目和发布数据……");
		foreach (int item in subProjectIds)
		{
			await storage.DeleteSubprojectFolderAsync(item);
		}
		try
		{
			await context.Db.Ado.BeginTranAsync();
			await context.Db.Deleteable<config_project_subProject>().Where(subProjects).ExecuteCommandAsync();
			await context.Db.Deleteable<publish_io>().Where(ioPublishes).ExecuteCommandAsync();
			await context.Db.Deleteable<publish_termination>().Where(terminationPublishes).ExecuteCommandAsync();
			await (from x in context.Db.Deleteable<config_project_major>()
				where x.Id == major.Id
				select x).ExecuteCommandAsync();
			await context.Db.Ado.CommitTranAsync();
		}
		catch (Exception ex)
		{
			await context.Db.Ado.RollbackTranAsync();
			model.Status.Reset();
			await message.ErrorAsync("删除专业时出错：" + ex.Message);
			return;
		}
		await Refresh();
		model.Status.Reset();
	}

	private bool EditMajor(config_project_major data, string title)
	{
		EditorOptionBuilder<config_project_major> editorOptionBuilder = data.CreateEditorBuilder();
		editorOptionBuilder.WithTitle(title).WithEditorHeight(330.0);
		editorOptionBuilder.AddProperty<Department>("Department").WithHeader("科室").EditAsCombo<Department>()
			.WithOptions<Department>();
		editorOptionBuilder.AddProperty<string>("CableSystem").WithHeader("所属专业").EditAsText();
		editorOptionBuilder.AddProperty<ControlSystem>("ControlSystem").WithHeader("控制系统").EditAsCombo<ControlSystem>()
			.WithOptions<ControlSystem>();
		return editorOptionBuilder.Build().EditWithWpfUI();
	}

	private async Task Refresh()
	{
		Majors = null;
		if (Project == null)
		{
			return;
		}
		ObservableCollection<config_project_major> observableCollection = new ObservableCollection<config_project_major>();
		foreach (config_project_major item in await (from x in context.Db.Queryable<config_project_major>()
			where x.ProjectId == Project.Id
			select x).ToListAsync())
		{
			observableCollection.Add(item);
		}
		Majors = observableCollection;
	}

	[RelayCommand]
	private void ConfigureSubProject(config_project_major major)
	{
		Major = major;
		navigation.NavigateWithHierarchy(typeof(SubProjectPage));
	}

	public void OnNavigatedFrom()
	{
	}

	public async void OnNavigatedTo()
	{
		Project = projectvm.Project ?? throw new Exception();
		await Refresh();
	}
}
