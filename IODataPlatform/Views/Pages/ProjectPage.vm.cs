using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

using IODataPlatform.Models;
using IODataPlatform.Models.Configs;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using IODataPlatform.Views.SubPages.Common;
using IODataPlatform.Views.SubPages.Paper;
using IODataPlatform.Views.SubPages.Project;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;

using Microsoft.Extensions.Options;

using SqlSugar;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 项目管理页面视图模型类
/// 负责项目的全生命周期管理，包括项目的创建、编辑、删除和系统配置
/// 支持层级化项目结构（项目-专业-子项目）和复杂的数据关联删除
/// 提供丰富的项目配置管理和业务数据统计功能
/// </summary>
public partial class ProjectViewModel(SqlSugarContext context, GlobalModel model,
    StorageService storage, IMessageService message, INavigationService navigation, ConfigTableViewModel configvm) : ObservableObject, INavigationAware
{

    /// <summary>系统中所有项目的集合，按创建时间倒序排列</summary>
    [ObservableProperty]
    private ObservableCollection<config_project>? projects; //所有项目

    /// <summary>当前选中或正在操作的项目对象</summary>
    [ObservableProperty]
    private config_project? project; //当前项目

    /// <summary>
    /// 页面导航离开时触发
    /// 当前实现为空，预留用于后续数据保存或状态清理
    /// </summary>
    public void OnNavigatedFrom() { }

    /// <summary>
    /// 页面导航到此页面时触发
    /// 显示加载状态并刷新项目列表，确保显示最新数据
    /// </summary>
    public async void OnNavigatedTo()
    {
        model.Status.Busy("正在刷新……");
        await RefreshAsync();
        model.Status.Reset();
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
        var project = new config_project();
        if (!Edit(project, "添加项目")) { return; }
        await context.Db.Insertable(project).ExecuteCommandIdentityIntoEntityAsync();
        await RefreshAsync();
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
        var projectToEdit = new config_project().CopyPropertiesFrom(project);
        if (!Edit(projectToEdit, "编辑项目")) { return; }
        await context.Db.Updateable(projectToEdit).ExecuteCommandAsync();
        await RefreshAsync();
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
        navigation.NavigateWithHierarchy(typeof(MajorPage));
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
        // 用户确认对话框，警告级联删除的影响范围
        if (!await message.ConfirmAsync($"确认删除 \"{project.Name}\"\r\n此操作会同时删除项目相关的所有数据，包括子项目和发布数据")) { return; }

        model.Status.Busy("正在获取子项目和发布数据……");

        // 第一阶段：获取与项目相关的所有专业、子项目及其发布数据
        var majors = await context.Db.Queryable<config_project_major>().Where(x => x.ProjectId == project.Id).ToListAsync();
        var majorIds = majors.Select(m => m.Id).ToList();

        var subProjects = await context.Db.Queryable<config_project_subProject>().Where(x => majorIds.Contains(x.MajorId)).ToListAsync();
        var subProjectIds = subProjects.Select(x => x.Id).ToList();

        var ioPublishes = await context.Db.Queryable<publish_io>().Where(x => subProjectIds.Contains(x.SubProjectId)).ToListAsync();
        var terminationPublishes = await context.Db.Queryable<publish_termination>().Where(x => subProjectIds.Contains(x.SubProjectId)).ToListAsync();

        model.Status.Busy("正在删除子项目和发布数据……");

        // 第二阶段：删除与子项目相关的文件系统文件夹
        foreach (var subProjectId in subProjectIds)
        {
            await storage.DeleteSubprojectFolderAsync(subProjectId);
        }

        // 第三阶段：批量删除数据库数据，使用事务确保一致性
        try
        {
            await context.Db.Ado.BeginTranAsync(); // 开始数据库事务

            // 按依赖关系顺序删除：先删除子表，再删除主表
            await context.Db.Deleteable<config_project_subProject>().Where(subProjects).ExecuteCommandAsync(); // 删除子项目
            await context.Db.Deleteable<config_project_major>().Where(majors).ExecuteCommandAsync(); // 删除专业

            await context.Db.Deleteable<publish_io>().Where(ioPublishes).ExecuteCommandAsync(); // 删除IO发布数据
            await context.Db.Deleteable<publish_termination>().Where(terminationPublishes).ExecuteCommandAsync(); // 删除端接发布数据

            await context.Db.Deleteable<config_project>().Where(x => x.Id == project.Id).ExecuteCommandAsync(); // 最后删除项目本身

            await context.Db.Ado.CommitTranAsync(); // 提交事务
        }
        catch (Exception ex)
        {
            await context.Db.Ado.RollbackTranAsync(); // 发生异常时回滚事务
            model.Status.Reset();
            await message.ErrorAsync($"删除项目时出错：{ex.Message}");
            return;
        }

        await RefreshAsync(); // 刷新项目列表显示
        model.Status.Reset(); // 重置状态显示
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
        var builder = obj.CreateEditorBuilder();

        builder.WithTitle(title).WithEditorHeight(300);

        builder.AddProperty<string>(nameof(Project.Name)).WithHeader("项目名称");
        builder.AddProperty<string>(nameof(Project.Note)).WithHeader("备注").EditAsText().WithMultiLine();

        return builder.Build().EditWithWpfUI();
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
        Projects = [.. await context.Db.Queryable<config_project>().OrderByDescending(x => x.Creation).ToListAsync()];
    }
}