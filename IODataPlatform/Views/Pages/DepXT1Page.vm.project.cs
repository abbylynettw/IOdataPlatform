using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using Wpf.Ui.Extensions;

namespace IODataPlatform.Views.Pages
{
    partial class DepXT1ViewModel
    {

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

        [RelayCommand]
        private async Task AddSubProject()
        {
            var projectId = Project?.Id ?? throw new("请先选择项目");
            var majorId = Major?.Id ?? throw new("请选择专业");

            var textbox = new TextBox() { VerticalAlignment = VerticalAlignment.Center, PlaceholderText = "请输入子项名", Width = 180 };

            var result = await dialog.ShowSimpleDialogAsync(
                new SimpleContentDialogCreateOptions()
                {
                    Title = "添加子项",
                    Content = textbox,
                    PrimaryButtonText = "确定",
                    CloseButtonText = "取消",
                }
            );

            if (result != ContentDialogResult.Primary) { return; }
            if (string.IsNullOrWhiteSpace(textbox.Text)) { throw new("请输入子项名"); }
            var subProjectName = textbox.Text;
            //await context.Db.Insertable(new config_project_subProject() { Name = subProjectName, ProjectId = projectId, MajorId = majorId, CreatorUserId = model.User.Id }).ExecuteCommandIdentityIntoEntityAsync();
            //SubProjects = new(await Task.Run(() => context.Db.Queryable<config_project_subProject>().Where(x => x.ProjectId == projectId&&x.MajorId==majorId).ToList()));
            SubProject = SubProjects.Where(x => x.Name == subProjectName).SingleOrDefault();
        }

        [RelayCommand]
        private async Task DeleteSubProject()
        {
            var projectId = Project?.Id ?? throw new("请先选择要删除的子项");
            var subProjectId = SubProject?.Id ?? throw new("请先选择要删除的子项");
            if (!await message.ConfirmAsync("是否删除子项")) { return; }
            await context.Db.Deleteable<publish_io>().Where(x => x.SubProjectId == subProjectId).ExecuteCommandAsync();
            await context.Db.Deleteable<config_project_subProject>().Where(x => x.Id == subProjectId).ExecuteCommandAsync();
            await storage.DeleteSubprojectFolderAsync(subProjectId);
            await RefreshProjects();
        }

        private async Task RefreshProjects()
        {
            Projects = null;
            Projects = new(await context.Db.Queryable<config_project>().ToListAsync());
        }

        async partial void OnProjectChanged(config_project? value)
        {
            Major = null;
            if (value is null) { return; }
            Majors = new(await context.Db.Queryable<config_project_major>().Where(x => x.Department == Department.系统一室).Where(x => x.ProjectId == value.Id).ToListAsync());
            if (Majors.Count == 1) { Major = Majors[0]; }
        }
        async partial void OnMajorChanged(config_project_major? value)
        {
            SubProjects = null;
            if (value is null) { return; }
            SubProjects = new(await context.Db.Queryable<config_project_subProject>().Where(x => x.MajorId == value.Id).ToListAsync());
            if (SubProjects.Count == 1) { SubProject = SubProjects[0]; }
        }
        async partial void OnSubProjectChanged(config_project_subProject? value)
        {
            AllData = null;
            if (value is null) { return; }
            model.Status.Busy("正在获取数据……");
            await ReloadAllData();
            model.Status.Reset();
        }

        [RelayCommand]
        private async Task RefreshAll()
        {
            if (!await message.ConfirmAsync("是否全部刷新")) { return; }
            await RefreshProjects();
        }

    }
}
