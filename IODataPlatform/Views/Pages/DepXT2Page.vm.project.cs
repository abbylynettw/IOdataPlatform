using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;

using Wpf.Ui.Extensions;

namespace IODataPlatform.Views.Pages;

// 项目部分

partial class DepXT2ViewModel {

    [ObservableProperty]
    private ObservableCollection<config_project>? projects;
    
    [ObservableProperty]
    private ObservableCollection<config_project_major>? majors;

    [ObservableProperty]
    private ObservableCollection<config_project_subProject>? subProjects;

    [ObservableProperty]
    private config_project? project;
    
    [ObservableProperty]
    private config_project_major? major;

    [ObservableProperty]
    private config_project_subProject? subProject;
    

    private async Task RefreshProjects() {
        Projects = null;
        Projects = new(await context.Db.Queryable<config_project>().ToListAsync());
    }

    async partial void OnProjectChanged(config_project? value) {
        Majors = null;
        if (value is null) { return; }
        Majors = new(await context.Db.Queryable<config_project_major>().Where(x => x.Department == Department.系统二室).Where(x => x.ProjectId == value.Id).ToListAsync());
        if (Majors.Count == 1) { Major = Majors[0]; }
    }

    async partial void OnMajorChanged(config_project_major? value) {
        SubProjects = null;
        if (Project is null) { return; }
        if (value is null) { return; }
        SubProjects = new(await context.Db.Queryable<config_project_subProject>().Where(x => x.MajorId == value.Id).ToListAsync());
        if (SubProjects.Count == 1) { SubProject = SubProjects[0]; }
    }

    async partial void OnSubProjectChanged(config_project_subProject? value) {
        AllData = null;
        if (value is null) { return; }
        //设置权限
        SetPermissionBySubProject();
        model.Status.Busy("正在获取数据……");
        await ReloadAllData();
        model.Status.Reset();
    }

    [RelayCommand]
    private async Task RefreshAll() {
        if (!await message.ConfirmAsync("是否全部刷新")) { return; }
        await RefreshProjects();
    }

}