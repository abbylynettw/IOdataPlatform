using IODataPlatform.Models.DBModels;

namespace IODataPlatform.Views.Pages;

public partial class PaperViewModel {

    [ObservableProperty]
    private ObservableCollection<config_project>? _projects;

    [ObservableProperty]
    private config_project? _project;

    partial void OnProjectsChanged(ObservableCollection<config_project>? value) {
        Project = null;
        if (Projects is null) { return; }
    }

    async partial void OnProjectChanged(config_project? value) {
        AllData = null;
        DisplayData = null;
        if (Project is null) { return; }

        var 盘箱柜列表 = await context.Db.Queryable<盘箱柜>().Where(x => x.项目Id == Project.Id).ToListAsync();
        var 图纸列表 = await context.Db.Queryable<图纸>().Where(x => 盘箱柜列表.Select(x => x.Id).Contains(x.盘箱柜表Id)).ToListAsync();

        AllData = [.. 图纸列表.Select(x => new PaperDisplay() { 图纸 = x, 盘箱柜 = 盘箱柜列表.Single(y => y.Id == x.盘箱柜表Id) })];
    }

    [RelayCommand]
    private async Task RefreshAll() {
        model.Status.Busy("正在刷新……");
        Projects = null;
        Projects = [.. await context.Db.Queryable<config_project>().ToListAsync()];
        model.Status.Reset();
    }

}