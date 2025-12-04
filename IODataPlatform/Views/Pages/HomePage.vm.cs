using IODataPlatform.Models;
using LYSoft.Libs.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using IODataPlatform.Models.Configs;
using Microsoft.Extensions.Options;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using System.IO;

namespace IODataPlatform.Views.Pages
{
    /// <summary>
    /// 首页视图模型类
    /// 应用程序主页的ViewModel，负责显示应用版本信息、用户信息和快捷操作
    /// 提供禅道链接访问、用户手册下载等核心功能的入口
    /// 实现INavigationAware接口以支持页面导航生命周期管理
    /// </summary>
    public partial class HomeViewModel(SqlSugarContext context, INavigationService navigation,
    GlobalModel model, IPickerService picker, ExcelService excel, StorageService storage, IMessageService message, IOptions<OtherPlatFormConfig> config) : ObservableObject, INavigationAware
    {
        /// <summary>获取当前登录用户信息，用于页面显示和权限控制</summary>
        public UserInfo User { get; } = model.User;

        /// <summary>禅道项目管理系统的访问URL地址</summary>
        [ObservableProperty]
        public string zentaoUrl;

        /// <summary>当前应用程序的版本号信息</summary>
        [ObservableProperty]
        public string appVersion;

        /// <summary>页面初始化状态标记，防止重复初始化</summary>
        private bool _isInitialized = false;

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>系统中项目的总数</summary>
        [ObservableProperty]
        public int projectCount;

        /// <summary>系统中IO配置的总数</summary>
        [ObservableProperty]
        public int ioConfigCount;

        /// <summary>系统中电缆的总数</summary>
        [ObservableProperty]
        public int cableCount;
       

        /// <summary>
        /// 页面导航到此页面时触发
        /// 首次访问时执行初始化操作，获取应用版本和外部系统配置信息
        /// </summary>
        public void OnNavigatedTo()
        {
            if (!_isInitialized) { InitializeViewModel(); }
        }

        /// <summary>
        /// 页面导航离开时触发
        /// 当前实现为空，预留用于后续功能扩展
        /// </summary>
        public void OnNavigatedFrom() { }

        /// <summary>
        /// 初始化视图模型数据
        /// 获取应用程序版本信息和禅道系统URL配置
        /// 设置初始化完成标记以避免重复初始化
        /// </summary>
        private async void InitializeViewModel()
        {
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            ZentaoUrl = config.Value.ZentaoUrl;
            await LoadStatisticsData();
            _isInitialized = true;
        }

        /// <summary>
        /// 加载统计数据
        /// 从数据库获取项目、IO配置和电缆的数量
        /// </summary>
        private async Task LoadStatisticsData()
        {
            try
            {
                // 获取项目数量
                ProjectCount = await context.Db.Queryable<config_project>().CountAsync();

                // 获取IO配置数量（从publish_io表统计）
                IoConfigCount = await context.Db.Queryable<publish_io>().CountAsync();

                // 获取电缆数量（从publish_cable表统计）
                CableCount = await context.Db.Queryable<publish_cable>().CountAsync();
            }
            catch (Exception ex)
            {
                model.Status.Error($"加载统计数据失败：{ex.Message}");
            }
            finally
            {
                // 数据加载完成
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// 复制文本到剪贴板命令
        /// 将指定的文本内容复制到系统剪贴板，并显示成功提示
        /// </summary>
        /// <param name="param">要复制到剪贴板的文本内容</param>
        /// <returns>异步任务，表示复制操作的完成</returns>
        [RelayCommand]
        private async Task CopyToClipboard(string param)
        {
            Clipboard.SetText(param);
            await message.SuccessAsync($"已复制到剪贴板：{param}");
        }
        
        /// <summary>
        /// 跳转到工作页面命令
        /// 预留的命令方法，用于导航到主要工作界面
        /// 当前实现为空，等待后续功能开发
        /// </summary>
        /// <returns>异步任务，表示导航操作的完成</returns>
        [RelayCommand]
        public async Task GoWorkPage()
        {
            // TODO: 实现工作页面导航逻辑
        }

        /// <summary>
        /// 下载用户手册命令
        /// 从服务器下载IO管理软件用户手册PDF文件到桌面
        /// 包含完整的下载进度提示和错误处理机制
        /// </summary>
        /// <returns>异步任务，表示下载操作的完成</returns>
        [RelayCommand]
        public async Task DownLoadHelpDoc()
        {
            try {
                // 显示下载进度状态
                model.Status.Busy("正在下载IO管理软件用户手册……");
                
                // 从服务器下载模板文件
                var userDoc = await storage.DownloadtemplatesDepFileAsync("IO管理软件用户手册.pdf");
                
                // 复制到桌面并提供用户反馈
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "IO管理软件用户手册.pdf");
                File.Copy(userDoc, filePath);
                model.Status.Success($"已成功下载到{filePath}");
            } catch (Exception ex) {
                model.Status.Error($"下载失败：{ex.Message}");
            }
        }
    }
}
