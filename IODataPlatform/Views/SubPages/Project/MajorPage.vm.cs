using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using IODataPlatform.Views.Pages;
using IODataPlatform.Views.SubPages.Project;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;

namespace IODataPlatform.Views.SubPages.Project;

public partial class MajorViewModel(SqlSugarContext context, ProjectViewModel projectvm, IMessageService message, 
    INavigationService navigation,GlobalModel model, StorageService storage) : ObservableObject, INavigationAware
{

    [ObservableProperty]
    private config_project? project;

    [ObservableProperty]
    private config_project_major? major;

    [ObservableProperty]
    private ObservableCollection<config_project_major>? majors;

    [RelayCommand]
    private async Task Add()
    {
        var data = new config_project_major() { ProjectId = Project?.Id ?? throw new() };
        if (!EditMajor(data, "添加")) { return; }
        await context.Db.Insertable(data).ExecuteCommandAsync();
        await Refresh();
    }

    [RelayCommand]
    private async Task Edit(config_project_major data)
    {
        var dataToEdit = new config_project_major().CopyPropertiesFrom(data);
        if (!EditMajor(dataToEdit, "编辑")) { return; }
        await context.Db.Updateable(dataToEdit).ExecuteCommandAsync();
        await Refresh();
    }

    [RelayCommand]
    private async Task Delete(config_project_major major)
    {
        if (!await message.ConfirmAsync($"确认删除 \"{major.CableSystem}\"?\r\n此操作会同时删除该专业下的所有子项目及发布数据。")) { return; }

        model.Status.Busy("正在获取子项目和发布数据……");

        // 获取与专业相关的所有子项目及其发布数据
        var subProjects = await context.Db.Queryable<config_project_subProject>().Where(x => x.MajorId == major.Id).ToListAsync();
        var subProjectIds = subProjects.Select(x => x.Id).ToList();

        var ioPublishes = await context.Db.Queryable<publish_io>().Where(x => subProjectIds.Contains(x.SubProjectId)).ToListAsync();
        var terminationPublishes = await context.Db.Queryable<publish_termination>().Where(x => subProjectIds.Contains(x.SubProjectId)).ToListAsync();

        model.Status.Busy("正在删除子项目和发布数据……");

        // 删除与子项目相关的文件夹
        foreach (var subProjectId in subProjectIds)
        {
            await storage.DeleteSubprojectFolderAsync(subProjectId);
        }
        // 批量删除数据，确保事务一致性
        try
        {
            await context.Db.Ado.BeginTranAsync(); // 开始事务
            await context.Db.Deleteable<config_project_subProject>().Where(subProjects).ExecuteCommandAsync(); // 删除子项目

            await context.Db.Deleteable<publish_io>().Where(ioPublishes).ExecuteCommandAsync(); // 删除发布数据
            await context.Db.Deleteable<publish_termination>().Where(terminationPublishes).ExecuteCommandAsync(); // 删除发布数据

            await context.Db.Deleteable<config_project_major>().Where(x => x.Id == major.Id).ExecuteCommandAsync(); // 删除专业本身

            await context.Db.Ado.CommitTranAsync(); // 提交事务
        }
        catch (Exception ex)
        {
            await context.Db.Ado.RollbackTranAsync(); // 事务回滚
            model.Status.Reset();
            await message.ErrorAsync($"删除专业时出错：{ex.Message}");
            return;
        }

        await Refresh(); // 刷新视图
        model.Status.Reset(); // 重置状态
    }



    private bool EditMajor(config_project_major data, string title)
    {
        var builder = data.CreateEditorBuilder();
        builder.WithTitle(title).WithEditorHeight(330);

        builder.AddProperty<Department>(nameof(config_project_major.Department)).WithHeader("科室").EditAsCombo<Department>().WithOptions<Department>();
        builder.AddProperty<string>(nameof(config_project_major.CableSystem)).WithHeader("所属专业").EditAsText();
        builder.AddProperty<ControlSystem>(nameof(config_project_major.ControlSystem)).WithHeader("控制系统").EditAsCombo<ControlSystem>().WithOptions<ControlSystem>();

        return builder.Build().EditWithWpfUI();
    }

    private async Task Refresh()
    {
        Majors = null;
        if (Project is null) { return; }
        Majors = [.. await context.Db.Queryable<config_project_major>().Where(x => x.ProjectId == Project.Id).ToListAsync()];
    }
    [RelayCommand]
    private void ConfigureSubProject(config_project_major major)
    {
        Major = major;
        navigation.NavigateWithHierarchy(typeof(SubProjectPage));
    }
    public void OnNavigatedFrom() { }

    public async void OnNavigatedTo()
    {
        Project = projectvm.Project ?? throw new();
        await Refresh();
    }

}