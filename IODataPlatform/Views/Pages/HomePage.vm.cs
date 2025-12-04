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
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SqlSugar;
using System.Collections.ObjectModel;

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

        /// <summary>项目数量</summary>
        [ObservableProperty]
        private int _projectCount;

        /// <summary>IO配置数量</summary>
        [ObservableProperty]
        private int _ioCount;

        /// <summary>电缆数量</summary>
        [ObservableProperty]
        private int _cableCount;

        /// <summary>数据统计项集合</summary>
        public ObservableCollection<StatisticItem> StatisticItems { get; } = new ObservableCollection<StatisticItem>();

        /// <summary>数据库上下文</summary>
        private readonly SqlSugarContext _context = context;

        /// <summary>全局模型</summary>
        private readonly GlobalModel _model = model;
       

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
            
            // 加载统计数据
            await LoadStatisticData();
            
            _isInitialized = true;
        }

        /// <summary>
        /// 加载统计数据
        /// 从数据库中获取项目、IO配置和电缆的数量
        /// </summary>
        private async Task LoadStatisticData()
        {
            try
            {
                // 显示加载状态
                _model.Status.Busy("正在加载统计数据...");

                // 异步获取统计数据
                var projectCountTask = Task.Run(() => _context.Db.Queryable<config_project>().Count());
                var ioCountTask = Task.Run(() => _context.Db.Queryable<publish_io>().Count());
                var cableCountTask = Task.Run(() => _context.Db.Queryable<publish_cable>().Count());

                // 等待所有任务完成
                await Task.WhenAll(projectCountTask, ioCountTask, cableCountTask);

                // 获取实际数量
                int actualProjectCount = await projectCountTask;
                int actualIoCount = await ioCountTask;
                int actualCableCount = await cableCountTask;

                // 初始化统计项
                StatisticItems.Clear();
                StatisticItems.Add(new StatisticItem { Title = "项目总数", Icon = "📁", TargetCount = actualProjectCount });
                StatisticItems.Add(new StatisticItem { Title = "IO配置", Icon = "⚙️", TargetCount = actualIoCount });
                StatisticItems.Add(new StatisticItem { Title = "电缆总数", Icon = "🔌", TargetCount = actualCableCount });

                // 启动计数动画
                StartCountAnimation();

                // 显示成功状态
                _model.Status.Success("统计数据加载完成");
            }
            catch (Exception ex)
            {
                // 显示错误状态
                _model.Status.Error($"加载统计数据失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 启动计数动画
        /// 让每个统计项的当前数量从0增加到目标数量
        /// </summary>
        private void StartCountAnimation()
        {
            foreach (var item in StatisticItems)
            {
                item.StartCountAnimation();
            }
        }

        /// <summary>
        /// 重新加载统计数据命令
        /// 用于手动刷新统计数据
        /// </summary>
        /// <returns>异步任务</returns>
        [RelayCommand]
        public async Task ReloadStatisticData()
        {
            await LoadStatisticData();
        }

        /// <summary>
        /// 统计项类
        /// 表示一个统计数据项，包含标题、图标、当前数量和目标数量
        /// 支持计数动画
        /// </summary>
        public class StatisticItem : ObservableObject
        {
            /// <summary>标题</summary>
            public string Title { get; set; }

            /// <summary>图标</summary>
            public string Icon { get; set; }

            /// <summary>目标数量</summary>
            public int TargetCount { get; set; }

            /// <summary>当前数量</summary>
            private int _currentCount;
            public int CurrentCount
            {
                get => _currentCount;
                set => SetProperty(ref _currentCount, value);
            }

            /// <summary>计数动画定时器</summary>
            private DispatcherTimer _countTimer;

            /// <summary>计数动画当前值</summary>
            private double _currentValue;

            /// <summary>计数动画步长</summary>
            private double _step;

            /// <summary>启动计数动画</summary>
            public void StartCountAnimation()
            {
                // 重置当前值
                _currentValue = 0;
                CurrentCount = 0;

                // 如果目标数量为0，则直接返回
                if (TargetCount == 0)
                {
                    return;
                }

                // 计算步长和动画持续时间
                double duration = 2.0; // 动画持续时间（秒）
                int steps = 60; // 动画步数
                _step = TargetCount / (duration * steps);

                // 创建定时器
                _countTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(duration / steps)
                };

                // 设置定时器事件处理程序
                _countTimer.Tick += (sender, e) =>
                {
                    _currentValue += _step;
                    
                    if (_currentValue >= TargetCount)
                    {
                        CurrentCount = TargetCount;
                        _countTimer.Stop();
                    }
                    else
                    {
                        CurrentCount = (int)Math.Floor(_currentValue);
                    }
                };

                // 启动定时器
                _countTimer.Start();
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
