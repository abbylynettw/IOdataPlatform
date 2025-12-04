﻿﻿﻿using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Views.Pages;

using Microsoft.Extensions.DependencyInjection;

namespace IODataPlatform.Views.Windows;

public partial class LoginWindowViewModel(IServiceProvider services, MainWindowViewModel mainvm, GlobalModel model, SqlSugarContext context) : ObservableObject {

    [ObservableProperty]
    private string _applicationTitle = "IO数据管理平台";

    [ObservableProperty]
    private string account = "admin";

    [RelayCommand]
    private async Task Login() {
        if (string.IsNullOrEmpty(Account)) { throw new("请输入口令"); }
        model.Status.Busy("正在登录……");
        var userList = await context.Db.Queryable<User>().Where(x => x.Account == Account).ToListAsync();
        if (userList.Count == 0) { throw new("口令错误"); }
        model.User.CopyPropertiesFrom(userList[0]);

        // model.User.Department = (Department)15;
        // model.User.Permission = (UserPermission)7;

        model.Status.Reset();
        ResetMenuItemVisibility();
    }

    public void ResetMenuItemVisibility() {

        // todo 权限检查方式1：通过调整可见性设置权限
        // 如需包含多种部门或权限检查，可使用如下代码
        // 例：既有需要用户公式编辑权限也需要用户有IO编辑权限
        // model.User.Check(null, UserPermission.公式编辑 | UserPermission.IO编辑);

        var menu = mainvm.MenuItems;
        menu.Reverse().Skip(1).AllDo(x => x.Visibility = Visibility.Collapsed);

        if (model.User.Check(null, UserPermission.公式编辑)) {
            // 使用更安全的方式访问菜单项，避免硬编码索引
            if (menu.Count > 0) menu[0].Visibility = Visibility.Visible;  // 首页
            if (menu.Count > 5) menu[5].Visibility = Visibility.Visible;  // 安全级室生成IO
            if (menu.Count > 6) menu[6].Visibility = Visibility.Visible;  // 生成与发布端接
            if (menu.Count > 7) menu[7].Visibility = Visibility.Visible;  // 生成与发布电缆
            if (menu.Count > 8) menu[8].Visibility = Visibility.Visible;  // 数据资产中心
            if (menu.Count > 9) menu[9].Visibility = Visibility.Visible;  // 公式编辑器
            if (menu.Count > 10) menu[10].Visibility = Visibility.Visible;  // 文档管理
            if (menu.Count > 11) menu[11].Visibility = Visibility.Visible;  // 图纸管理
            if (menu.Count > 12) menu[12].Visibility = Visibility.Visible;  // 其他功能
        }

        if (model.User.Check(Department.系统一室, null) && menu.Count > 1) { menu[1].Visibility = Visibility.Visible; }
        if (model.User.Check(Department.系统二室, null) && menu.Count > 2) { menu[2].Visibility = Visibility.Visible; }
        if (model.User.Check(Department.工程硬件室, null) && menu.Count > 3) { menu[3].Visibility = Visibility.Visible; }
        if (model.User.Check(Department.安全级室, null) && menu.Count > 4) { menu[4].Visibility = Visibility.Visible; }

        if (menu.FirstOrDefault(x => x.Visibility == Visibility.Visible) is not NavigationViewItem item) { throw new("您无权查看任何页面"); }

        services.GetService<LoginWindow>()!.Hide();
        var mainwindow = services.GetService<INavigationWindow>()!;
        mainwindow.ShowWindow();       
        mainwindow.Navigate(typeof(SettingsPage));
        mainwindow.Navigate(item.TargetPageType!);
    }

}