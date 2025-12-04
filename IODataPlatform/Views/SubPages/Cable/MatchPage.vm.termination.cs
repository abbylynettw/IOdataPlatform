using IODataPlatform.Models.DBModels;

namespace IODataPlatform.Views.SubPages.Cable;

// 选择端接部分

partial class MatchViewModel {

    [ObservableProperty]
    private config_project_subProject? subProject1;

    [ObservableProperty]
    private publish_termination? publish1;

    [ObservableProperty]
    private string header1 = string.Empty;

    [ObservableProperty]
    private ObservableCollection<publish_termination>? publishes1;

    [ObservableProperty]
    private config_project_subProject? subProject2;

    [ObservableProperty]
    private publish_termination? publish2;

    [ObservableProperty]
    private string header2 = string.Empty;

    [ObservableProperty]
    private ObservableCollection<publish_termination>? publishes2;

    async partial void OnSubProject1Changed(config_project_subProject? value) {
        Publishes1 = null;
        if (SubProject1 is null) { return; }
        Publishes1 = new(await context.Db.Queryable<publish_termination>().Where(x => x.SubProjectId == SubProject1.Id).ToListAsync());
        if (Publishes1.Count == 1) { Publish1 = Publishes1[0]; }
    }  
    
    async partial void OnSubProject2Changed(config_project_subProject? value) {
        Publishes2 = null;
        if (SubProject2 is null) { return; }
        Publishes2 = new(await context.Db.Queryable<publish_termination>().Where(x => x.SubProjectId == SubProject2.Id).ToListAsync());
        if (Publishes2.Count == 1) { Publish2 = Publishes2[0]; }
    }

}