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
using IODataPlatform.Views.SubPages.Common;
using IODataPlatform.Views.SubPages.Project;
using LYSoft.Libs;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 项目管理页面视图模型类
/// 负责项目的全生命周期管理，包括项目的创建、编辑、删除和系统配置
/// 支持层级化项目结构（项目-专业-子项目）和复杂的数据关联删除
/// 提供丰富的项目配置管理和业务数据统计功能
/// </summary>
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class ProjectViewModel : ObservableObject, INavigationAware
{
	/// <summary>系统中所有项目的集合，按创建时间倒序排列</summary>
	[ObservableProperty]
	private ObservableCollection<config_project>? projects;

	/// <summary>当前选中或正在操作的项目对象</summary>
	[ObservableProperty]
	private config_project? project;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.ProjectViewModel.AddProjectCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? addProjectCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.ProjectViewModel.EditProjectCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<config_project>? editProjectCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.ProjectViewModel.ConfigureSystemCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<config_project>? configureSystemCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Pages.ProjectViewModel.DeleteProjectCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<config_project>? deleteProjectCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.ProjectViewModel.projects" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<config_project>? Projects
	{
		get
		{
			return projects;
		}
		set
		{
			if (!EqualityComparer<ObservableCollection<config_project>>.Default.Equals(projects, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Projects);
				projects = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Projects);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Pages.ProjectViewModel.project" />
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

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.ProjectViewModel.AddProject" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AddProjectCommand => addProjectCommand ?? (addProjectCommand = new AsyncRelayCommand(AddProject));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.ProjectViewModel.EditProject(IODataPlatform.Models.DBModels.config_project)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<config_project> EditProjectCommand => editProjectCommand ?? (editProjectCommand = new AsyncRelayCommand<config_project>(EditProject));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.ProjectViewModel.ConfigureSystem(IODataPlatform.Models.DBModels.config_project)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<config_project> ConfigureSystemCommand => configureSystemCommand ?? (configureSystemCommand = new RelayCommand<config_project>(ConfigureSystem));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.Pages.ProjectViewModel.DeleteProject(IODataPlatform.Models.DBModels.config_project)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<config_project> DeleteProjectCommand => deleteProjectCommand ?? (deleteProjectCommand = new AsyncRelayCommand<config_project>(DeleteProject));

	/// <summary>
	/// 项目管理页面视图模型类
	/// 负责项目的全生命周期管理，包括项目的创建、编辑、删除和系统配置
	/// 支持层级化项目结构（项目-专业-子项目）和复杂的数据关联删除
	/// 提供丰富的项目配置管理和业务数据统计功能
	/// </summary>
	public ProjectViewModel(SqlSugarContext context, GlobalModel model, StorageService storage, IMessageService message, INavigationService navigation, ConfigTableViewModel configvm)
	{
		_003Ccontext_003EP = context;
		_003Cmodel_003EP = model;
		_003Cstorage_003EP = storage;
		_003Cmessage_003EP = message;
		_003Cnavigation_003EP = navigation;
		base._002Ector();
	}

	/// <summary>
	/// 页面导航离开时触发
	/// 当前实现为空，预留用于后续数据保存或状态清理
	/// </summary>
	public void OnNavigatedFrom()
	{
	}

	/// <summary>
	/// 页面导航到此页面时触发
	/// 显示加载状态并刷新项目列表，确保显示最新数据
	/// </summary>
	public async void OnNavigatedTo()
	{
		_003Cmodel_003EP.Status.Busy("正在刷新……");
		await RefreshAsync();
		_003Cmodel_003EP.Status.Reset();
	}

	/// <summary>
	/// 添加新项目命令
	/// 创建一个新的项目对象，弹出编辑对话框供用户填写项目信息
	/// 保存成功后自动刷新项目列表并获取自增主键ID
	/// </summary>
	/// <returns>异步任务，表示添加操作的完成</returns>
	[RelayCommand]
	private async Task AddProject()
	{
		config_project config_project = new config_project();
		if (Edit(config_project, "添加项目"))
		{
			await _003Ccontext_003EP.Db.Insertable(config_project).ExecuteCommandIdentityIntoEntityAsync();
			await RefreshAsync();
		}
	}

	/// <summary>
	/// 编辑项目命令
	/// 创建项目对象的副本并弹出编辑对话框，避免直接修改原对象
	/// 用户确认修改后更新数据库并刷新界面显示
	/// </summary>
	/// <param name="project">要编辑的项目对象</param>
	/// <returns>异步任务，表示编辑操作的完成</returns>
	[RelayCommand]
	private async Task EditProject(config_project project)
	{
		config_project config_project = new config_project().CopyPropertiesFrom(project);
		if (Edit(config_project, "编辑项目"))
		{
			await _003Ccontext_003EP.Db.Updateable(config_project).ExecuteCommandAsync();
			await RefreshAsync();
		}
	}

	/// <summary>
	/// 配置项目系统命令
	/// 设置当前项目并导航到专业配置页面，进行项目下的专业和子项目管理
	/// </summary>
	/// <param name="project">要配置的项目对象</param>
	[RelayCommand]
	private void ConfigureSystem(config_project project)
	{
		Project = project;
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(MajorPage));
	}

	/// <summary>
	/// 删除项目命令
	/// 执行复杂的级联删除操作，包括项目及其所有关联数据的完整清理
	/// 包含专业、子项目、发布数据和文件系统中的相关文件夹
	/// 使用数据库事务确保数据一致性，包含完整的错误处理和回滚机制
	/// </summary>
	/// <param name="project">要删除的项目对象</param>
	/// <returns>异步任务，表示删除操作的完成</returns>
	[RelayCommand]
	private async Task DeleteProject(config_project project)
	{
		if (!(await _003Cmessage_003EP.ConfirmAsync("确认删除 \"" + project.Name + "\"\r\n此操作会同时删除项目相关的所有数据，包括子项目和发布数据")))
		{
			return;
		}
		_003Cmodel_003EP.Status.Busy("正在获取子项目和发布数据……");
		List<config_project_major> majors = await (from x in _003Ccontext_003EP.Db.Queryable<config_project_major>()
			where x.ProjectId == project.Id
			select x).ToListAsync();
		List<int> majorIds = majors.Select((config_project_major m) => m.Id).ToList();
		List<config_project_subProject> subProjects = await (from x in _003Ccontext_003EP.Db.Queryable<config_project_subProject>()
			where majorIds.Contains(x.MajorId)
			select x).ToListAsync();
		List<int> subProjectIds = subProjects.Select((config_project_subProject x) => x.Id).ToList();
		List<publish_io> ioPublishes = await (from x in _003Ccontext_003EP.Db.Queryable<publish_io>()
			where subProjectIds.Contains(x.SubProjectId)
			select x).ToListAsync();
		List<publish_termination> terminationPublishes = await (from x in _003Ccontext_003EP.Db.Queryable<publish_termination>()
			where subProjectIds.Contains(x.SubProjectId)
			select x).ToListAsync();
		_003Cmodel_003EP.Status.Busy("正在删除子项目和发布数据……");
		foreach (int item in subProjectIds)
		{
			await _003Cstorage_003EP.DeleteSubprojectFolderAsync(item);
		}
		try
		{
			await _003Ccontext_003EP.Db.Ado.BeginTranAsync();
			await _003Ccontext_003EP.Db.Deleteable<config_project_subProject>().Where(subProjects).ExecuteCommandAsync();
			await _003Ccontext_003EP.Db.Deleteable<config_project_major>().Where(majors).ExecuteCommandAsync();
			await _003Ccontext_003EP.Db.Deleteable<publish_io>().Where(ioPublishes).ExecuteCommandAsync();
			await _003Ccontext_003EP.Db.Deleteable<publish_termination>().Where(terminationPublishes).ExecuteCommandAsync();
			await (from x in _003Ccontext_003EP.Db.Deleteable<config_project>()
				where x.Id == project.Id
				select x).ExecuteCommandAsync();
			await _003Ccontext_003EP.Db.Ado.CommitTranAsync();
		}
		catch (Exception ex)
		{
			await _003Ccontext_003EP.Db.Ado.RollbackTranAsync();
			_003Cmodel_003EP.Status.Reset();
			await _003Cmessage_003EP.ErrorAsync("删除项目时出错：" + ex.Message);
			return;
		}
		await RefreshAsync();
		_003Cmodel_003EP.Status.Reset();
	}

	/// <summary>
	/// 项目编辑对话框工具方法
	/// 使用编辑器构建器创建动态的项目编辑对话框
	/// 配置项目名称和备注字段的编辑界面
	/// </summary>
	/// <param name="obj">要编辑的项目对象</param>
	/// <param name="title">对话框标题</param>
	/// <returns>返回true表示用户确认修改，false表示取消</returns>
	private static bool Edit(config_project obj, string title)
	{
		EditorOptionBuilder<config_project> editorOptionBuilder = obj.CreateEditorBuilder();
		editorOptionBuilder.WithTitle(title).WithEditorHeight(300.0);
		editorOptionBuilder.AddProperty<string>("Name").WithHeader("项目名称");
		editorOptionBuilder.AddProperty<string>("Note").WithHeader("备注").EditAsText()
			.WithMultiLine();
		return editorOptionBuilder.Build().EditWithWpfUI();
	}

	/// <summary>
	/// 刷新项目列表的私有方法
	/// 从数据库重新加载所有项目，按创建时间倒序排列
	/// 先清空当前集合再重新赋值，触发UI更新
	/// </summary>
	/// <returns>异步任务，表示刷新操作的完成</returns>
	private async Task RefreshAsync()
	{
		Projects = null;
		ObservableCollection<config_project> observableCollection = new ObservableCollection<config_project>();
		foreach (config_project item in await (from x in _003Ccontext_003EP.Db.Queryable<config_project>()
			orderby x.Creation descending
			select x).ToListAsync())
		{
			observableCollection.Add(item);
		}
		Projects = observableCollection;
	}
}
