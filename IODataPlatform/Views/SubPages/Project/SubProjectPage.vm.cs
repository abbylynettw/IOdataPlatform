using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Views.Pages;
using IODataPlatform.Views.SubPages.Paper;
using LYSoft.Libs.Editor;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.WpfUI;

namespace IODataPlatform.Views.SubPages.Project
{
    public partial class SubProjectViewModel(SqlSugarContext context, MajorViewModel majorvm, IMessageService message, GlobalModel model) : ObservableObject, INavigationAware
    {

        [ObservableProperty]
        private config_project_major? major;

        [ObservableProperty]
        private ObservableCollection<config_project_subProject>? subProjects;

        [RelayCommand]
        private async Task Add()
        {
            var data = new config_project_subProject() {  MajorId = Major?.Id ?? throw new(), CreatorUserId = model.User.Id };
            if (!EditSubProject(data, "添加")) { return; }
            await context.Db.Insertable(data).ExecuteCommandAsync();
            await Refresh();
        }

        [RelayCommand]
        private async Task Edit(config_project_subProject data)
        {
            var dataToEdit = new config_project_subProject().CopyPropertiesFrom(data);
            if (!EditSubProject(dataToEdit, "编辑")) { return; }
            await context.Db.Updateable(dataToEdit).ExecuteCommandAsync();
            await Refresh();
        }

        [RelayCommand]
        private async Task Delete(config_project_subProject data)
        {
            if (!await message.ConfirmAsync("确认删除")) { return; }
            await context.Db.Deleteable(data).ExecuteCommandAsync();
            await Refresh();
        }

        private bool EditSubProject(config_project_subProject data, string title)
        {

            var builder = data.CreateEditorBuilder();
            builder.WithTitle(title).WithEditorHeight(330);

            builder.AddProperty<string>(nameof(config_project_subProject.Name)).WithHeader("子项名称").EditAsText();
            return builder.Build().EditWithWpfUI();
        }

        private async Task Refresh()
        {
            if (Major is null) { SubProjects = null; return; }
            var subProjects = await context.Db.Queryable<config_project_subProject>().Where(x => x.MajorId == Major.Id).ToListAsync();
            var users = await context.Db.Queryable<User>().ToListAsync();
            subProjects.ForEach(item => item.UpdateCreatorName(users.FirstOrDefault(u => u.Id == item.CreatorUserId)?.Name));
            SubProjects = [.. subProjects];
        }


        public void OnNavigatedFrom() { }

        public async void OnNavigatedTo()
        {            
            Major = majorvm.Major ?? throw new();
            await Refresh();
        }

    }
}
