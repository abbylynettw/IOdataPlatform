using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;

using Microsoft.Extensions.DependencyInjection;

namespace IODataPlatform.Views.Pages;

// 项目部分

partial class TerminationViewModel {

    [ObservableProperty]
    private ObservableCollection<config_project>? projects;//项目
    
    [ObservableProperty]
    private ObservableCollection<config_project_major>? majors;

    [ObservableProperty]
    private ObservableCollection<config_project_subProject>? subProjects;//子项
    
    [ObservableProperty]
    private config_project? project;//选中项目

    [ObservableProperty]
    private config_project_major? major;

    [ObservableProperty]
    private config_project_subProject? subProject;//选中子项

    private async Task RefreshProjects() {
        Projects = null;
        Projects = new(await context.Db.Queryable<config_project>().ToListAsync());//从项目表中获取项目
    }

    async partial void OnProjectChanged(config_project? value) {
        Majors = null;
        if (Project is null) { return; }
        Majors = new(await context.Db.Queryable<config_project_major>().Where(x => x.ProjectId == Project.Id).ToListAsync());//从数据库中获取所有子项
        if (Majors.Count == 1) { Major = Majors[0]; }
    }

    async partial void OnMajorChanged(config_project_major? value) {
        SubProjects = null;
        if (Major is null) { return; }
        SubProjects = new(await context.Db.Queryable<config_project_subProject>().Where(x => x.MajorId == Major.Id).ToListAsync());//从数据库中获取所有子项
        if (SubProjects.Count == 1) { SubProject = SubProjects[0]; }
    }

    async partial void OnSubProjectChanged(config_project_subProject? value) {
        AllData = null;
        if (SubProject is null) { return; }
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