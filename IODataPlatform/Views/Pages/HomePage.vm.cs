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
using System.Net.Http;
using System.Text.Json;
using System.Windows.Threading;

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

        /// <summary>项目数量</summary>
        [ObservableProperty]
        public int projectCount;

        /// <summary>IO配置数量</summary>
        [ObservableProperty]
        public int ioCount;

        /// <summary>电缆数量</summary>
        [ObservableProperty]
        public int cableCount;

        /// <summary>
        /// 启动计数动画
        /// </summary>
        private void StartCountAnimation()
        {
            AnimateCount(0, ProjectCount, (value) => ProjectCount = value);
            AnimateCount(0, IoCount, (value) => IoCount = value);
            AnimateCount(0, CableCount, (value) => CableCount = value);
        }

        /// <summary>
        /// 执行单个计数动画
        /// </summary>
        /// <param name="startValue">起始值</param>
        /// <param name="endValue">结束值</param>
        /// <param name="updateCallback">更新回调函数</param>
        private void AnimateCount(int startValue, int endValue, Action<int> updateCallback)
        {
            if (endValue == 0)
            {
                updateCallback?.Invoke(0);
                return;
            }

            var timer = new DispatcherTimer();
            int currentValue = startValue;

            // 计算动画持续时间（根据结束值动态调整，确保动画速度一致）
            int durationMilliseconds = Math.Min(2000, Math.Max(500, endValue / 2));
            int steps = durationMilliseconds / 20; // 每20毫秒更新一次
            int stepValue = endValue / steps;

            timer.Interval = TimeSpan.FromMilliseconds(20);
            timer.Tick += (sender, e) =>
            {
                currentValue += stepValue;
                
                if (currentValue >= endValue)
                {
                    currentValue = endValue;
                    timer.Stop();
                }

                updateCallback?.Invoke(currentValue);
            };

            timer.Start();
        }

        /// <summary>页面初始化状态标记，防止重复初始化</summary>
        private bool _isInitialized = false;
       

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
            
            // 获取统计数据
            await GetStatistics();
            
            // 数据加载完成后启动计数动画
            StartCountAnimation();
            
            _isInitialized = true;
        }
        
        /// <summary>
        /// 获取统计数据
        /// </summary>
        private async Task GetStatistics()
        {
            try
            {
                // 创建HttpClient实例
                using (var httpClient = new HttpClient())
                {
                    // 发送GET请求到统计API
                    var response = await httpClient.GetAsync("http://localhost:5000/api/statistics");
                    
                    // 确保请求成功
                    response.EnsureSuccessStatusCode();
                    
                    // 读取响应内容
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    // 解析JSON响应
                    var statistics = JsonSerializer.Deserialize<StatisticsResponse>(responseContent);
                    
                    // 更新统计数据
                    ProjectCount = statistics.ProjectCount;
                    IoCount = statistics.IoCount;
                    CableCount = statistics.CableCount;
                }
            }
            catch (Exception ex)
            {
                // 处理异常
                await message.ErrorAsync($"获取统计数据失败：{ex.Message}");
            }
        }
        
        /// <summary>
        /// 统计响应数据模型
        /// </summary>
        private class StatisticsResponse
        {
            /// <summary>项目数量</summary>
            public int ProjectCount { get; set; }
            
            /// <summary>IO配置数量</summary>
            public int IoCount { get; set; }
            
            /// <summary>电缆数量</summary>
            public int CableCount { get; set; }
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
