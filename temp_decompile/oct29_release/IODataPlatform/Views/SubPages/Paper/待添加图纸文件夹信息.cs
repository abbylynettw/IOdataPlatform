using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using IODataPlatform.Models.DBModels;

namespace IODataPlatform.Views.SubPages.Paper;

public class 待添加图纸文件夹信息
{
	public bool IsSuccess { get; set; }

	public int 盘箱柜Id { get; set; }

	public string DirectoryPath { get; set; } = string.Empty;

	public string Version { get; set; } = string.Empty;

	public string InnerCode { get; set; } = string.Empty;

	public string Name { get; set; } = string.Empty;

	public string FullName { get; set; } = string.Empty;

	public string PdfFileName { get; set; } = string.Empty;

	public DateTime PublishDate { get; set; } = DateTime.Now;

	public 待添加图纸文件夹信息(string folder, ObservableCollection<盘箱柜> configs)
	{
		DirectoryPath = folder;
		FullName = Path.GetFileName(folder);
		PdfFileName = Path.Combine(folder, FullName + ".pdf");
		if (!File.Exists(PdfFileName))
		{
			IsSuccess = false;
			return;
		}
		PublishDate = new FileInfo(PdfFileName).CreationTime;
		string[] source = Path.GetFileName(folder).Split('_');
		Version = source.SkipLast(1).Last();
		盘箱柜 盘箱柜 = (from x in configs
			where FullName.Contains(x.内部编码)
			where FullName.Contains(x.名称)
			select x).SingleOrDefault();
		if (盘箱柜 == null)
		{
			IsSuccess = false;
			return;
		}
		盘箱柜Id = 盘箱柜.Id;
		InnerCode = 盘箱柜.内部编码;
		Name = 盘箱柜.名称;
		IsSuccess = true;
	}
}
