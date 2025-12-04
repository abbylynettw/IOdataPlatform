using System.IO;
using System.Text.RegularExpressions;

using IODataPlatform.Models.DBModels;

namespace IODataPlatform.Views.SubPages.Paper;

public partial class 待添加图纸文件夹信息 {

    public 待添加图纸文件夹信息(string folder, ObservableCollection<盘箱柜> configs) {

        DirectoryPath = folder;
        FullName = Path.GetFileName(folder);
        PdfFileName = Path.Combine(folder, $"{FullName}.pdf");
        if (!File.Exists(PdfFileName)) { IsSuccess = false; return; }
        PublishDate = new FileInfo(PdfFileName).CreationTime;

        var parts = Path.GetFileName(folder).Split('_');
        //if (parts.Length != 4) { IsSuccess = false; return; }
        //InnerCode = parts[0];
        Version = parts.SkipLast(1).Last();

        //if (NameRegex().Match(FullName) is not { Success: true } match) { IsSuccess = false; return; }
        //Name = match.Groups[1].Value;

        if (configs.Where(x => FullName.Contains(x.内部编码)).Where(x => FullName.Contains(x.名称)).SingleOrDefault() is not 盘箱柜 data) {
            IsSuccess = false; return;
        }

        盘箱柜Id = data.Id;
        InnerCode = data.内部编码;
        Name = data.名称;
        IsSuccess = true;
    }

    public bool IsSuccess { get; set; }

    public int 盘箱柜Id { get; set; } = 0;

    public string DirectoryPath { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string InnerCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string PdfFileName { get; set; } = string.Empty;

    public DateTime PublishDate { get; set; } = DateTime.Now;

    //[GeneratedRegex("系统(.*?)机柜装配图")]
    //private static partial Regex NameRegex();

}