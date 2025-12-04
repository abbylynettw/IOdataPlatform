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
using LYSoft.Libs;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.SubPages.Project;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class SubProjectViewModel(SqlSugarContext context, MajorViewModel majorvm, IMessageService message, GlobalModel model) : ObservableObject(), INavigationAware
{
	[ObservableProperty]
	private config_project_major? major;

	[ObservableProperty]
	private ObservableCollection<config_project_subProject>? subProjects;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Project.SubProjectViewModel.AddCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Project.SubProjectViewModel.EditCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<config_project_subProject>? editCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Project.SubProjectViewModel.DeleteCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<config_project_subProject>? deleteCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Project.SubProjectViewModel.major" />
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

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Project.SubProjectViewModel.subProjects" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<config_project_subProject>? SubProjects
	{
		get
		{
			return subProjects;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<config_project_subProject>>.Default.Equals(subProjects, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SubProjects);
				subProjects = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SubProjects);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Project.SubProjectViewModel.Add" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddCommand => addCommand ?? (addCommand = new AsyncRelayCommand(Add));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Project.SubProjectViewModel.Edit(IODataPlatform.Models.DBModels.config_project_subProject)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<config_project_subProject> EditCommand => editCommand ?? (editCommand = new AsyncRelayCommand<config_project_subProject>(Edit));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Project.SubProjectViewModel.Delete(IODataPlatform.Models.DBModels.config_project_subProject)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<config_project_subProject> DeleteCommand => deleteCommand ?? (deleteCommand = new AsyncRelayCommand<config_project_subProject>(Delete));

	[RelayCommand]
	private async Task Add()
	{
		config_project_subProject config_project_subProject = new config_project_subProject
		{
			MajorId = (Major ?? throw new Exception()).Id,
			CreatorUserId = model.User.Id
		};
		if (EditSubProject(config_project_subProject, "添加"))
		{
			await context.Db.Insertable(config_project_subProject).ExecuteCommandAsync();
			await Refresh();
		}
	}

	[RelayCommand]
	private async Task Edit(config_project_subProject data)
	{
		config_project_subProject config_project_subProject = new config_project_subProject().CopyPropertiesFrom(data);
		if (EditSubProject(config_project_subProject, "编辑"))
		{
			await context.Db.Updateable(config_project_subProject).ExecuteCommandAsync();
			await Refresh();
		}
	}

	[RelayCommand]
	private async Task Delete(config_project_subProject data)
	{
		if (await message.ConfirmAsync("确认删除"))
		{
			await context.Db.Deleteable(data).ExecuteCommandAsync();
			await Refresh();
		}
	}

	private bool EditSubProject(config_project_subProject data, string title)
	{
		EditorOptionBuilder<config_project_subProject> editorOptionBuilder = data.CreateEditorBuilder();
		editorOptionBuilder.WithTitle(title).WithEditorHeight(330.0);
		editorOptionBuilder.AddProperty<string>("Name").WithHeader("子项名称").EditAsText();
		return editorOptionBuilder.Build().EditWithWpfUI();
	}

	private async Task Refresh()
	{
		if (Major == null)
		{
			SubProjects = null;
			return;
		}
		List<config_project_subProject> subProjects = await (from x in context.Db.Queryable<config_project_subProject>()
			where x.MajorId == Major.Id
			select x).ToListAsync();
		List<User> users = await context.Db.Queryable<User>().ToListAsync();
		subProjects.ForEach(delegate(config_project_subProject item)
		{
			item.UpdateCreatorName(users.FirstOrDefault((User u) => u.Id == item.CreatorUserId)?.Name);
		});
		ObservableCollection<config_project_subProject> observableCollection = new ObservableCollection<config_project_subProject>();
		foreach (config_project_subProject item in subProjects)
		{
			observableCollection.Add(item);
		}
		SubProjects = observableCollection;
	}

	public void OnNavigatedFrom()
	{
	}

	public async void OnNavigatedTo()
	{
		Major = majorvm.Major ?? throw new Exception();
		await Refresh();
	}
}
