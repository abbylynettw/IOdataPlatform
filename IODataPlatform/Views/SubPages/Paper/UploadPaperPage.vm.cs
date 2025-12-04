using System.IO;
using System.IO.Compression;

using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using IODataPlatform.Views.Pages;

using LYSoft.Libs.ServiceInterfaces;

using Microsoft.Extensions.DependencyModel;

namespace IODataPlatform.Views.SubPages.Paper;

public partial class UploadPaperViewModel(SqlSugarContext context, StorageService storage, PaperViewModel paper, IMessageService message, GlobalModel model, IPickerService picker) : ObservableObject, INavigationAware {

    [ObservableProperty]
    private config_project? project;

    [ObservableProperty]
    private ObservableCollection<盘箱柜>? configs;
    
    [ObservableProperty]
    private ObservableCollection<待添加图纸文件夹信息> _data = [];

    [RelayCommand]
    private async Task Add() {
        if (picker.PickFolder() is not string folder) { return; }
        model.Status.Busy("正在分析全部子文件夹……");
        var result = await Task.Run(() => {
            var matchData = new List<待添加图纸文件夹信息>();
            foreach (var subFolder in Directory.GetDirectories(folder, "*", new EnumerationOptions() { RecurseSubdirectories = true }).Append(folder)) {
                if (Data.Select(x => x.DirectoryPath!.ToLower()).Contains(subFolder.ToLower())) { continue; }
                var d = new 待添加图纸文件夹信息(subFolder, Configs ?? throw new());
                if (!d.IsSuccess) { continue; }
                matchData.Add(d);
            }
            return matchData;
        });

        foreach (var data in result) {
            Data.Add(data);
        }

        model.Status.Reset();
    }

    [RelayCommand]
    private async Task Delete(待添加图纸文件夹信息 data) {
        if (!await message.ConfirmAsync("确认移除")) { return; }
        Data.Remove(data);
    }

    [RelayCommand]
    private async Task Upload() {
        if (!await message.ConfirmAsync("确认上传\r\n上传过程较慢，请耐心等待")) { return; }
        var total = Data.Count;
        for (int i = 0; i < Data.Count; i++) {
            var data = Data[i];
            model.Status.Busy($"正在上传 {i + 1} / {total}：{data.FullName}");
            var paper = new 图纸() {
                PDF文件名 = $"{data.FullName}.pdf",
                版本 = data.Version,
                压缩包文件名 = $"{data.FullName}.zip",
                名称 = data.FullName,
                盘箱柜表Id = data.盘箱柜Id,
                发布日期 = data.PublishDate,
            };

            var olds = context.Db.Queryable<图纸>().Where(x => x.盘箱柜表Id == paper.盘箱柜表Id).Where(x => x.版本 == paper.版本).ToList();
            if (olds.Count > 0) {
                foreach (var old in olds) {
                    try { await storage.WebDeleteFolderAsync(storage.GetPaperFolderRelativePath(old)); } catch { }
                    await context.Db.Deleteable(old).ExecuteCommandAsync();
                }
            }

            await context.Db.Insertable(paper).ExecuteCommandIdentityIntoEntityAsync();

            var zipPath = storage.GetWebFileLocalAbsolutePath(storage.GetPaperZipFileRelativePath(paper));
            await ZipFolderAsync(data.DirectoryPath, zipPath);
            await storage.UploadPaperZipFileAsync(paper);

            var pdfPath = storage.GetWebFileLocalAbsolutePath(storage.GetPaperPdfFileRelativePath(paper));
            File.Copy(Path.Combine(data.DirectoryPath, data.PdfFileName), pdfPath);
            await storage.UploadPaperPdfFileAsync(paper);
        }

        model.Status.Success("全部上传成功");
        Data.Clear();
    }

    private async Task ZipFolderAsync(string directoryPath, string zipPath) {
        if (File.Exists(zipPath)) { File.Delete(zipPath); }

        if (!Directory.Exists(directoryPath)) {
            throw new ArgumentException("The specified directory does not exist", nameof(directoryPath));
        }

        var directoryName = Path.GetDirectoryName(zipPath);
        if (!Directory.Exists(directoryName)) {
            Directory.CreateDirectory(directoryName!);
        }

       await Task.Run(() => ZipFile.CreateFromDirectory(directoryPath, zipPath));
    }

    public void OnNavigatedFrom() { }

    public async void OnNavigatedTo() {
        Project = paper.Project ?? throw new();
        Configs = [.. await context.Db.Queryable<盘箱柜>().Where(x => x.项目Id == Project.Id).ToListAsync()];
    }

}