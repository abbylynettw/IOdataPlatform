﻿using System.Diagnostics;
using System.IO;

using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using IODataPlatform.Views.SubPages.Paper;

using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.Pages;

/// <summary>
/// 图纸管理页面视图模型类
/// 负责工程图纸的综合管理，包括图纸浏览、下载、上传和依据文件管理
/// 支持PDF和ZIP格式图纸文件的在线预览和下载功能
/// 提供图纸上传、设备管理和搜索筛选等高级功能
/// </summary>
public partial class PaperViewModel(SqlSugarContext context, INavigationService navigation, 
    GlobalModel model, IPickerService picker, StorageService storage) : ObservableObject, INavigationAware {

    /// <summary>页面初始化状态标记，防止重复初始化操作</summary>
    private bool isInit = false;

    /// <summary>
    /// 页面导航离开时触发
    /// 当前实现为空，预留用于后续清理操作
    /// </summary>
    public void OnNavigatedFrom() {

    }

    /// <summary>
    /// 页面导航到此页面时触发
    /// 首次访问时执行全量刷新和初始化操作
    /// 非首次访问时触发项目变更事件和搜索操作
    /// </summary>
    public async void OnNavigatedTo() {
        if (!isInit) {
            await RefreshAll();
            isInit = true; 
        } else {
            OnProjectChanged(null);
            Search();
        }
    }

    /// <summary>所有图纸数据的完整集合，用于数据筛选和搜索</summary>
    [ObservableProperty]
    private ObservableCollection<PaperDisplay>? _allData; 
    
    /// <summary>当前界面显示的图纸数据集合，经过搜索和筛选后的结果</summary>
    [ObservableProperty]
    private ObservableCollection<PaperDisplay>? _displayData;

    /// <summary>
    /// 浏览PDF图纸命令
    /// 从服务器下载PDF图纸文件并使用默认应用程序打开
    /// 显示下载进度状态，完成后自动重置状态
    /// </summary>
    /// <param name="paper">要浏览的图纸对象</param>
    /// <returns>异步任务，表示下载和打开操作的完成</returns>
    [RelayCommand]
    private async Task BrowserPdf(PaperDisplay paper) {
        model.Status.Busy("正在下载……");
        var file = await storage.DownloadPaperPdfFileAsync(paper.图纸);
        model.Status.Reset();
        storage.RunFile(file);
    }

    /// <summary>
    /// 浏览ZIP图纸命令
    /// 获取ZIP图纸文件的下载链接并使用系统浏览器直接打开
    /// 适用于大文件或批量下载图纸的情况
    /// </summary>
    /// <param name="paper">要浏览的图纸对象</param>
    [RelayCommand]
    private void BrowserZip(PaperDisplay paper) {
        var url = storage.GetFileDownloadUrl(storage.GetPaperZipFileRelativePath(paper.图纸));
        Process.Start("explorer.exe", url);
    }

    /// <summary>
    /// 上传依据文件命令
    /// 允许用户为指定图纸上传相关的依据文件（如设计规范、技术标准等）
    /// 如果已存在依据文件则先删除旧文件，然后上传新文件
    /// 自动更新数据库中的文件名记录
    /// </summary>
    /// <param name="paper">要上传依据文件的图纸对象</param>
    /// <returns>异步任务，表示上传操作的完成</returns>
    [RelayCommand]
    private async Task UploadDep(PaperDisplay paper) {
        if (picker.OpenFile() is not string file) { return; }

        model.Status.Busy("正在上传……");
        
        // 如果已存在依据文件则先删除
        if (!string.IsNullOrEmpty(paper.图纸.依据文件名)) {
            await storage.WebDeleteFileAsync(storage.GetPaperDepFileRelativePath(paper.图纸));
        }

        // 更新数据库记录并上传新文件
        paper.图纸.依据文件名 = Path.GetFileName(file);
        await context.Db.Updateable(paper.图纸).ExecuteCommandAsync();
        var relativePath = storage.GetPaperDepFileRelativePath(paper.图纸);
        File.Copy(file, storage.GetWebFileLocalAbsolutePath(relativePath));
        await storage.UploadPaperDepFileAsync(paper.图纸);
        model.Status.Reset();
    }

    /// <summary>
    /// 查看依据文件命令
    /// 从服务器下载指定图纸的依据文件并使用默认应用程序打开
    /// 需要先检查是否已上传依据文件，未上传时抛出异常
    /// </summary>
    /// <param name="paper">要查看依据文件的图纸对象</param>
    /// <returns>异步任务，表示下载和打开操作的完成</returns>
    /// <exception cref="Exception">当尚未上传依据文件时抛出异常</exception>
    [RelayCommand]
    private async Task ViewDep(PaperDisplay paper) {
        if (string.IsNullOrEmpty(paper.图纸.依据文件名)) { throw new("还未上传依据文件"); }
        model.Status.Busy("正在下载……");
        var file = await storage.DownloadPaperDepFileAsync(paper.图纸);
        model.Status.Reset();
        storage.RunFile(file);
    }

    /// <summary>
    /// 编辑设备表命令
    /// 导航到设备编辑页面，用于管理图纸中涉及的设备信息
    /// </summary>
    [RelayCommand]
    private void EditDeviceTable() {
        navigation.NavigateWithHierarchy(typeof(EditDevicePage));
    }

    /// <summary>
    /// 上传图纸命令
    /// 导航到图纸上传页面，用于批量上传新的图纸文件
    /// </summary>
    [RelayCommand]
    private void UploadPaper() {
        navigation.NavigateWithHierarchy(typeof(UploadPaperPage));
    }

}