using IODataPlatform.Models.DBModels;
using System.Reactive.Subjects;

namespace IODataPlatform.Views.Pages;

// 项目部分

partial class DataAssetCenterViewModel {

    [ObservableProperty]
    private ObservableCollection<config_project>? projects;

    [ObservableProperty]
    private ObservableCollection<config_project_major>? majors;

    [ObservableProperty]
    private ObservableCollection<config_project_subProject>? subProjects1;

    [ObservableProperty]
    private ObservableCollection<config_project_subProject>? subProjects2;

    [ObservableProperty]
    private ObservableCollection<string>? publishVersions;

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

    [ObservableProperty]
    private string? publishVersion;
    
    [ObservableProperty]
    private string category = "IO";

    private async Task RefreshProjects()
    {
        Projects = null;
        Projects = new(await context.Db.Queryable<config_project>().ToListAsync());
    }

    async partial void OnProjectChanged(config_project? value)
    {
        Major1 = null;
        Major2 = null;
        if (value is null) { return; }
        Majors = new(await context.Db.Queryable<config_project_major>().Where(x => x.ProjectId == value.Id).ToListAsync());
        if (Majors.Count == 1) { Major1 = Majors[0]; Major2 = Majors[0]; }
    }

    async partial void OnMajor1Changed(config_project_major? value)
    {
        SubProjects1 = null;
        if (value is null) { return; }
        SubProjects1 = new(await context.Db.Queryable<config_project_subProject>().Where(x => x.MajorId == value.Id).ToListAsync());
        if (SubProjects1.Count == 1) { SubProject1 = SubProjects1[0]; }
    }

    async partial void OnMajor2Changed(config_project_major? value)
    {
        SubProjects2 = null;
        if (value is null) { return; }
        SubProjects2 = new(await context.Db.Queryable<config_project_subProject>().Where(x => x.MajorId == value.Id).ToListAsync());
        if (SubProjects2.Count == 1) { SubProject2 = SubProjects2[0]; }
    }

    async partial void OnSubProject1Changed(config_project_subProject? value)
    {
        if (Project is null) { return; }
        if (Major1 is null) { return; }        
        if (value is null) { return; }
        switch (Category)
        {
            case "IO": PublishVersions = new(await context.Db.Queryable<publish_io>().Where(x => x.SubProjectId == value.Id).Select(s => s.PublishedVersion).ToListAsync()); break;
            case "端接": PublishVersions = new(await context.Db.Queryable<publish_termination>().Where(x => x.SubProjectId == value.Id).Select(s => s.PublishedVersion).ToListAsync()); break;
            case "电缆":
                if (SubProject2 is null) { return; }
                if (Major2 is null) { return; }
                PublishVersions = new(await context.Db.Queryable<publish_cable>().Where(x => x.SubProjectId1 == value.Id && x.SubProjectId2 == SubProject2.Id).Select(c => c.PublishedVersion).ToListAsync());
                if (PublishVersions.Count == 1) { PublishVersion = PublishVersions[0]; }
                break;
            default:
                break;
        }
    }

    async partial void OnSubProject2Changed(config_project_subProject? value)
    {
        if (Project is null) { return; }
        if (Major2 is null) { return; }
        if (value is null) { return; }
        switch (Category)
        {
            case "IO": PublishVersions = new(await context.Db.Queryable<publish_io>().Where(x => x.SubProjectId == value.Id).Select(s => s.PublishedVersion).ToListAsync()); break;
            case "端接": PublishVersions = new(await context.Db.Queryable<publish_termination>().Where(x => x.SubProjectId == value.Id).Select(s => s.PublishedVersion).ToListAsync()); break;
            case "电缆":
                if (SubProject1 is null) { return; }
                if (Major1 is null) { return; }
                PublishVersions = new(await context.Db.Queryable<publish_cable>().Where(x => x.SubProjectId1 == SubProject1.Id && x.SubProjectId2 == value.Id).Select(c => c.PublishedVersion).ToListAsync());
                if (PublishVersions.Count == 1) { PublishVersion = PublishVersions[0]; }
                break;
            default:
                break;
        }
    }  
    async partial void OnCategoryChanged(string value)
    {
        if (Project is null) { return; }
        if (Major1 is null) { return; }
        if (SubProject1 is null) { return; }
        switch (Category)
        {
            case "IO": PublishVersions = new(await context.Db.Queryable<publish_io>().Where(x => x.SubProjectId == SubProject1.Id).Select(s => s.PublishedVersion).ToListAsync()); break;
            case "端接": PublishVersions = new(await context.Db.Queryable<publish_termination>().Where(x => x.SubProjectId == SubProject1.Id).Select(s => s.PublishedVersion).ToListAsync()); break;
            case "电缆":
                if (SubProject2 is null) { return; }
                if (Major2 is null) { return; }
                PublishVersions = new(await context.Db.Queryable<publish_cable>().Where(x => x.SubProjectId1 == SubProject1.Id && x.SubProjectId2 == SubProject2.Id).Select(c => c.PublishedVersion).ToListAsync());
                if (PublishVersions.Count == 1) { PublishVersion = PublishVersions[0]; }
                break;
            default:
                break;
        }
        model.Status.Busy("正在获取数据……");
        await ReloadAllData();
        model.Status.Reset();
    }
    async partial void OnPublishVersionChanged(string? value) {
        AllData = null;
        if (PublishVersion is null) { return; }
        model.Status.Busy("正在获取数据……");
        await ReloadAllData();
        model.Status.Reset();
    }

   

    [RelayCommand]
    private async Task RefreshAll() {
        if (!await message.ConfirmAsync("是否全部刷新")) { return; }
        Category = "IO";
        await RefreshProjects();
    }

}