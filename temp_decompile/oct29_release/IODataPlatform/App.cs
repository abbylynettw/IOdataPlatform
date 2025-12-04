using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using IODataPlatform.Models;
using IODataPlatform.Models.Configs;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using LYSoft.Libs;
using LYSoft.Libs.Config;
using LYSoft.Libs.ServiceInterfaces;
using LYSoft.Libs.Wpf.Services;
using LYSoft.Libs.Wpf.WpfUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wpf.Ui;

namespace IODataPlatform;

/// <summary>
/// App
/// </summary>
public class App : Application
{
	private static readonly IHost _host = Host.CreateDefaultBuilder().ConfigureAppConfiguration(delegate(IConfigurationBuilder c)
	{
		c.SetBasePath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Assets", "Settings")).SetConfigBasePath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Assets", "Configs"));
	}).ConfigureServices(delegate(HostBuilderContext context, IServiceCollection services)
	{
		services.AddHostedService<ApplicationHostService>();
		services.AddSqlSugar(context.Configuration.GetConnectionString("SqlSugarContext"), context.Configuration.GetConnectionString("SqlSugarContext2"));
		services.Configure<WebServiceConfig>(context.Configuration.GetSection("WebServiceConfig"));
		services.Configure<OtherPlatFormConfig>(context.Configuration.GetSection("OtherPlatFormConfig"));
		services.AddSingleton<IThemeService, ThemeService>();
		services.AddSingleton<ITaskBarService, TaskBarService>();
		services.AddSingleton<INavigationService, NavigationService>();
		services.AddSingleton<IContentDialogService, ContentDialogService>();
		services.AddSingleton<ISnackbarService, SnackbarService>();
		services.AddSingleton<IMessageService, WpfUIMessageService>();
		services.AddSingleton<IPickerService, PickerService>();
		services.AddSingleton<StorageService>();
		services.AddSingleton<DatabaseService>();
		services.AddSingleton<NavigationParameterService>();
		services.AddSingleton<CloudExportConfigService>();
		services.AddExcelService();
		services.AddPdfService();
		services.AddWordService();
		services.AddAllModels();
		services.AddAllViews();
	})
		.Build();

	private bool _contentLoaded;

	public static T GetService<T>() where T : class
	{
		return _host.Services.GetRequiredService<T>();
	}

	private void OnStartup(object sender, StartupEventArgs e)
	{
		_host.Start();
		base.ShutdownMode = ShutdownMode.OnExplicitShutdown;
	}

	private async void OnExit(object sender, ExitEventArgs e)
	{
		await _host.StopAsync();
		_host.Dispose();
	}

	private async void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		e.Handled = true;
		await GetService<IMessageService>().ErrorAsync(e.Exception.Message, "系统提示");
		try
		{
			GetService<Status>().Error(e.Exception.Message);
		}
		catch
		{
		}
	}

	/// <summary>
	/// InitializeComponent
	/// </summary>
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	public void InitializeComponent()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			base.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(OnDispatcherUnhandledException);
			base.Exit += OnExit;
			base.Startup += OnStartup;
			Uri resourceLocator = new Uri("/IODataPlatform;V1.2.0.0;component/app.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	/// <summary>
	/// Application Entry Point.
	/// </summary>
	[STAThread]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.10.0")]
	public static void Main()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		SplashScreen val = new SplashScreen("splashscreen.png");
		val.Show(true);
		App app = new App();
		app.InitializeComponent();
		app.Run();
	}
}
