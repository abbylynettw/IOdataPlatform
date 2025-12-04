using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;

using Microsoft.Extensions.DependencyInjection;

using Wpf.Ui.Extensions;

namespace IODataPlatform.Views.Pages;

// 项目部分

partial class CableViewModel {

    [ObservableProperty]
    private ObservableCollection<config_project>? projects;

    [ObservableProperty]
    private ObservableCollection<config_project_major>? majors;

    [ObservableProperty]
    private ObservableCollection<config_project_subProject>? subProjects1;

    [ObservableProperty]
    private ObservableCollection<config_project_subProject>? subProjects2;

    [ObservableProperty]
    private ObservableCollection<publish_termination>? terminations1;

    [ObservableProperty]
    private ObservableCollection<publish_termination>? terminations2;

    [ObservableProperty]
    private config_project? project;

    [ObservableProperty]
    private config_project_major? major1;

    [ObservableProperty]
    private config_project_major? major2;

    [ObservableProperty]
    private config_project_subProject? subProject1;

    [ObservableProperty]
    private config_project_subProject? subProject2;

    private async Task RefreshProjects() {
        Projects = null;
        Projects = new(await context.Db.Queryable<config_project>().ToListAsync());
    }

    async partial void OnProjectChanged(config_project? value) {
        Major1 = null;
        Major2 = null;
        if (value is null) { return; }
        Majors = new(await context.Db.Queryable<config_project_major>().Where(x => x.ProjectId == value.Id).ToListAsync());
        if (Majors.Count == 1) { Major1 = Majors[0]; Major2 = Majors[0]; }
    }

    async partial void OnMajor1Changed(config_project_major? value) {
        SubProjects1 = null;
        if (value is null) { return; }
        SubProjects1 = new(await context.Db.Queryable<config_project_subProject>().Where(x => x.MajorId == value.Id).ToListAsync());
        if (SubProjects1.Count == 1) { SubProject1 = SubProjects1[0]; }
    }

    async partial void OnMajor2Changed(config_project_major? value) {
        SubProjects2 = null;
        if (value is null) { return; }
        SubProjects2 = new(await context.Db.Queryable<config_project_subProject>().Where(x => x.MajorId == value.Id).ToListAsync());
        if (SubProjects2.Count == 1) { SubProject2 = SubProjects2[0]; }
    }

    async partial void OnSubProject1Changed(config_project_subProject? value) {
        AllData = null;
        model.Status.Busy("正在获取数据……");
        await ReloadAllData();
        model.Status.Reset();
    }

    async partial void OnSubProject2Changed(config_project_subProject? value) {
        AllData = null;
        if (value is null) { return; }
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