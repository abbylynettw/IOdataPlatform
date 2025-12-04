using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Views.Pages;
using LYSoft.Libs;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace IODataPlatform.Views.Windows;

/// <inheritdoc />
/// <inheritdoc />
public class LoginWindowViewModel(IServiceProvider services, MainWindowViewModel mainvm, GlobalModel model, SqlSugarContext context) : ObservableObject()
{
	[ObservableProperty]
	private string _applicationTitle = "IO数据管理平台";

	[ObservableProperty]
	private string account = string.Empty;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.Windows.LoginWindowViewModel.LoginCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? loginCommand;

	/// <inheritdoc cref="F:IODataPlatform.Views.Windows.LoginWindowViewModel._applicationTitle" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string ApplicationTitle
	{
		get
		{
			return _applicationTitle;
		}
		[MemberNotNull("_applicationTitle")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(_applicationTitle, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ApplicationTitle);
				_applicationTitle = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ApplicationTitle);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.Windows.LoginWindowViewModel.account" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Account
	{
		get
		{
			return account;
		}
		[MemberNotNull("account")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(account, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Account);
				account = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Account);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.Windows.LoginWindowViewModel.Login" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand LoginCommand => loginCommand ?? (loginCommand = new AsyncRelayCommand(Login));

	[RelayCommand]
	private async Task Login()
	{
		if (string.IsNullOrEmpty(Account))
		{
			throw new Exception("请输入口令");
		}
		model.Status.Busy("正在登录……");
		List<User> list = await (from x in context.Db.Queryable<User>()
			where x.Account == Account
			select x).ToListAsync();
		if (list.Count == 0)
		{
			throw new Exception("口令错误");
		}
		model.User.CopyPropertiesFrom(list[0]);
		model.Status.Reset();
		ResetMenuItemVisibility();
	}

	public void ResetMenuItemVisibility()
	{
		ObservableCollection<NavigationViewItem> menuItems = mainvm.MenuItems;
		menuItems.Reverse().Skip(1).AllDo(delegate(NavigationViewItem x)
		{
			x.Visibility = Visibility.Collapsed;
		});
		if (model.User.Check(null, UserPermission.公式编辑))
		{
			if (menuItems.Count > 0)
			{
				menuItems[0].Visibility = Visibility.Visible;
			}
			if (menuItems.Count > 5)
			{
				menuItems[5].Visibility = Visibility.Visible;
			}
			if (menuItems.Count > 6)
			{
				menuItems[6].Visibility = Visibility.Visible;
			}
			if (menuItems.Count > 7)
			{
				menuItems[7].Visibility = Visibility.Visible;
			}
			if (menuItems.Count > 8)
			{
				menuItems[8].Visibility = Visibility.Visible;
			}
			if (menuItems.Count > 9)
			{
				menuItems[9].Visibility = Visibility.Visible;
			}
			if (menuItems.Count > 10)
			{
				menuItems[10].Visibility = Visibility.Visible;
			}
			if (menuItems.Count > 11)
			{
				menuItems[11].Visibility = Visibility.Visible;
			}
			if (menuItems.Count > 12)
			{
				menuItems[12].Visibility = Visibility.Visible;
			}
		}
		if (model.User.Check(Department.系统一室, null) && menuItems.Count > 1)
		{
			menuItems[1].Visibility = Visibility.Visible;
		}
		if (model.User.Check(Department.系统二室, null) && menuItems.Count > 2)
		{
			menuItems[2].Visibility = Visibility.Visible;
		}
		if (model.User.Check(Department.工程硬件室, null) && menuItems.Count > 3)
		{
			menuItems[3].Visibility = Visibility.Visible;
		}
		if (model.User.Check(Department.安全级室, null) && menuItems.Count > 4)
		{
			menuItems[4].Visibility = Visibility.Visible;
		}
		NavigationViewItem navigationViewItem = menuItems.FirstOrDefault((NavigationViewItem x) => x.Visibility == Visibility.Visible);
		if (navigationViewItem == null)
		{
			throw new Exception("您无权查看任何页面");
		}
		services.GetService<LoginWindow>().Hide();
		INavigationWindow service = services.GetService<INavigationWindow>();
		service.ShowWindow();
		service.Navigate(typeof(SettingsPage));
		service.Navigate(navigationViewItem.TargetPageType);
	}
}
