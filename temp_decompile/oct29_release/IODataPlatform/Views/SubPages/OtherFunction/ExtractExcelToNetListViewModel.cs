using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Aspose.Cells;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Views.SubPages.Common;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui;

namespace IODataPlatform.Views.SubPages.OtherFunction;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class ExtractExcelToNetListViewModel : ObservableObject
{
	private string templateName;

	[ObservableProperty]
	private string dest;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.SetDestCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? setDestCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.UpLoadTemplateCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? upLoadTemplateCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.AppendFilesCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? appendFilesCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.RemoveAllFilesCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? removeAllFilesCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.RemovePdfFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<FileToExtract>? removePdfFileCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.OpenPdfDestFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<FileToExtract>? openPdfDestFileCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.GoBackCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? goBackCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.OpenNetListComparisonCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? openNetListComparisonCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.CopyToClipboardCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<string>? copyToClipboardCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.ConfirmCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? confirmCommand;

	public ObservableCollection<FileToExtract> ExcelFiles { get; }

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.dest" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public string Dest
	{
		get
		{
			return dest;
		}
		[MemberNotNull("dest")]
		set
		{
			if (!EqualityComparer<string>.Default.Equals(dest, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Dest);
				dest = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Dest);
			}
		}
	}

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.SetDest" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand SetDestCommand => setDestCommand ?? (setDestCommand = new RelayCommand(SetDest));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.UpLoadTemplate" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand UpLoadTemplateCommand => upLoadTemplateCommand ?? (upLoadTemplateCommand = new RelayCommand(UpLoadTemplate));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.AppendFiles" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand AppendFilesCommand => appendFilesCommand ?? (appendFilesCommand = new RelayCommand(AppendFiles));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.RemoveAllFiles" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RemoveAllFilesCommand => removeAllFilesCommand ?? (removeAllFilesCommand = new AsyncRelayCommand(RemoveAllFiles));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.RemovePdfFile(IODataPlatform.Views.SubPages.Common.FileToExtract)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<FileToExtract> RemovePdfFileCommand => removePdfFileCommand ?? (removePdfFileCommand = new RelayCommand<FileToExtract>(RemovePdfFile));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.OpenPdfDestFile(IODataPlatform.Views.SubPages.Common.FileToExtract)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<FileToExtract> OpenPdfDestFileCommand => openPdfDestFileCommand ?? (openPdfDestFileCommand = new RelayCommand<FileToExtract>(OpenPdfDestFile));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.GoBack" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand GoBackCommand => goBackCommand ?? (goBackCommand = new RelayCommand(GoBack));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.OpenNetListComparison" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand OpenNetListComparisonCommand => openNetListComparisonCommand ?? (openNetListComparisonCommand = new RelayCommand(OpenNetListComparison));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.CopyToClipboard(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<string> CopyToClipboardCommand => copyToClipboardCommand ?? (copyToClipboardCommand = new AsyncRelayCommand<string>(CopyToClipboard));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.OtherFunction.ExtractExcelToNetListViewModel.Confirm" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ConfirmCommand => confirmCommand ?? (confirmCommand = new AsyncRelayCommand(Confirm));

	public ExtractExcelToNetListViewModel(GlobalModel model, IMessageService message, INavigationService navigation, IPickerService picker, WordService word, ExcelService excel, StorageService storage)
	{
		_003Cmodel_003EP = model;
		_003Cmessage_003EP = message;
		_003Cnavigation_003EP = navigation;
		_003Cpicker_003EP = picker;
		_003Cexcel_003EP = excel;
		_003Cstorage_003EP = storage;
		templateName = "网络设计清单模板.xlsx";
		dest = "请选择输出目录";
		ExcelFiles = new ObservableCollection<FileToExtract>();
		base._002Ector();
	}

	[RelayCommand]
	private void SetDest()
	{
		string text = _003Cpicker_003EP.PickFolder();
		if (text != null)
		{
			Dest = text;
		}
	}

	[RelayCommand]
	private async void UpLoadTemplate()
	{
		string text = _003Cpicker_003EP.OpenFile();
		if (text != null)
		{
			_003Cmodel_003EP.Status.Busy("正在上传……");
			string relativePath = _003Cstorage_003EP.GettemplatesDepFileRelativePath(templateName);
			string webFileLocalAbsolutePath = _003Cstorage_003EP.GetWebFileLocalAbsolutePath(relativePath);
			if (File.Exists(webFileLocalAbsolutePath))
			{
				File.Delete(webFileLocalAbsolutePath);
			}
			File.Copy(text, webFileLocalAbsolutePath);
			await _003Cstorage_003EP.UploadtemplatesDepFileAsync(templateName);
			_003Cmodel_003EP.Status.Success("上传成功！");
		}
	}

	[RelayCommand]
	private void AppendFiles()
	{
		string[] array = _003Cpicker_003EP.OpenFiles("文件(*.xls; *.xlsx)| *.xls; *.xlsx");
		if (array != null)
		{
			ExcelFiles.Reset(ExcelFiles.AppendRange(array.Select((string x) => new FileToExtract(x))).Distinct().ToList());
			if ((from x in ExcelFiles
				group x by x.FileName.ToLower()).Any((IGrouping<string, FileToExtract> x) => x.Count() > 1))
			{
				throw new Exception("列表中有两个或多个文件名相同，如果开始提取，一部分结果文件会被覆盖");
			}
		}
	}

	[RelayCommand]
	private async Task RemoveAllFiles()
	{
		if (await _003Cmessage_003EP.ConfirmAsync("是否清空文件列表"))
		{
			ExcelFiles.Clear();
		}
	}

	[RelayCommand]
	private void RemovePdfFile(FileToExtract file)
	{
		ExcelFiles.Remove(file);
	}

	[RelayCommand]
	private void OpenPdfDestFile(FileToExtract file)
	{
		string text = Path.Combine(Dest, file.FileName + ".xlsx");
		if (!File.Exists(text))
		{
			throw new Exception("结果文件不存在，请先处理");
		}
		Process.Start("explorer.exe", text);
	}

	[RelayCommand]
	private void GoBack()
	{
		_003Cnavigation_003EP.GoBack();
	}

	[RelayCommand]
	private void OpenNetListComparison()
	{
		_003Cnavigation_003EP.NavigateWithHierarchy(typeof(NetListComparisonPage));
	}

	[RelayCommand]
	private async Task CopyToClipboard(string param)
	{
		Clipboard.SetText(param);
		await _003Cmessage_003EP.SuccessAsync("已复制到剪贴板：" + param);
	}

	[RelayCommand]
	private async Task Confirm()
	{
		if (ExcelFiles.Count < 2)
		{
			throw new Exception("请添加至少IP表和配置信息统计表两个文件");
		}
		if (Dest == "请选择输出目录")
		{
			throw new Exception("请选择输出目录");
		}
		if (ExcelFiles.Where((FileToExtract f) => f.FileName.Contains("配置信息统计表")).Count() != 1)
		{
			throw new Exception("未识别到[配置信息统计表]关键字，或[配置信息统计表]大于1个");
		}
		if (ExcelFiles.Where((FileToExtract f) => f.FileName.Contains("IP表")).Count() == 0)
		{
			throw new Exception("未识别到IP表，请添加包含[IP表]关键字的文件");
		}
		FileToExtract cabinetRoomFile = ExcelFiles.FirstOrDefault((FileToExtract f) => f.FileName.Contains("配置信息统计表"));
		List<FileToExtract> list = new List<FileToExtract>();
		list.AddRange(ExcelFiles.Where((FileToExtract e) => e.FileName.Contains("IP")));
		IEnumerable<FileToExtract> ipFiles = new _003C_003Ez__ReadOnlyList<FileToExtract>(list);
		if (!Directory.Exists(Dest))
		{
			Directory.CreateDirectory(Dest);
		}
		_003Cmodel_003EP.Status.Busy("正在检查数据格式……");
		await Task.WhenAll(ipFiles.Select(CheckExcel));
		if (ipFiles.Where((FileToExtract x) => x.Result == "处理完成").Count() > 0)
		{
			_003Cmodel_003EP.Status.Busy("正在处理数据……");
			string templatePath = await DownLoadTemplate();
			if (!(await CheckAndPromptDuplication(from x in ipFiles
				where x.Result == "处理完成"
				select x.FullName)))
			{
				_003Cmodel_003EP.Status.Reset();
				return;
			}
			string filename = $"网络接线清单_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
			string savePath = _003Cpicker_003EP.SaveFile("Excel文件|*.xlsx", filename);
			if (savePath == null)
			{
				_003Cmodel_003EP.Status.Reset();
				return;
			}
			MergeAllExcelData(from x in ipFiles
				where x.Result == "处理完成"
				select x.FullName, templatePath, cabinetRoomFile.FullName, savePath);
			_003Cmodel_003EP.Status.Success("已成功生成网络接线清单：" + Path.GetFileName(savePath));
			if (await _003Cmessage_003EP.ConfirmAsync("生成成功！是否立即打开文件？", "生成成功"))
			{
				Process.Start("explorer.exe", savePath);
			}
		}
		else
		{
			_003Cmodel_003EP.Status.Error("生成失败！");
		}
	}

	private async Task<string> DownLoadTemplate()
	{
		_003Cmodel_003EP.Status.Busy("正在下载……");
		string result = await _003Cstorage_003EP.DownloadtemplatesDepFileAsync(templateName);
		_003Cmodel_003EP.Status.Reset();
		return result;
	}

	public async Task CheckExcel(FileToExtract fileData)
	{
		fileData.Result = "处理中";
		string fullname = fileData.FullName;
		await Task.Run(delegate
		{
			if (new string[2] { ".xlsx", ".xls" }.Contains<string>(Path.GetExtension(fullname), StringComparer.CurrentCultureIgnoreCase))
			{
				if (!CheckExcelStandard(fullname))
				{
					fileData.Result = "文件不标准";
				}
				else
				{
					fileData.Result = "处理完成";
				}
			}
		});
		GC.Collect();
	}

	private bool CheckExcelStandard(string filePath)
	{
		Workbook workbook = _003Cexcel_003EP.GetWorkbook(filePath);
		Dictionary<(int Row, int Column), string> expectedValues = new Dictionary<(int, int), string>
		{
			{
				(1, 21),
				"接口类型"
			},
			{
				(3, 2),
				"机柜或盘台"
			},
			{
				(3, 3),
				"位号"
			},
			{
				(3, 4),
				"端口"
			},
			{
				(3, 6),
				"机柜或盘台"
			},
			{
				(3, 7),
				"位号"
			},
			{
				(3, 8),
				"端口"
			},
			{
				(3, 11),
				"机柜或盘台"
			},
			{
				(3, 12),
				"位号"
			},
			{
				(3, 13),
				"端口"
			},
			{
				(3, 15),
				"机柜或盘台"
			},
			{
				(3, 16),
				"位号"
			},
			{
				(3, 17),
				"端口"
			}
		};
		return workbook.Worksheets.All((Worksheet worksheet) => expectedValues.All<KeyValuePair<(int, int), string>>((KeyValuePair<(int Row, int Column), string> cell) => worksheet.Cells[cell.Key.Row, cell.Key.Column]?.Value?.ToString() == cell.Value));
	}

	/// <summary>
	/// 检测端口重复并提示用户
	/// </summary>
	private async Task<bool> CheckAndPromptDuplication(IEnumerable<string> files)
	{
		List<系统二室网络设备IP表> ipDatas = ExtractData(files);
		PortDuplicationResult portDuplicationResult = CheckPortDuplication(ipDatas);
		if (portDuplicationResult.HasDuplicates)
		{
			string text = BuildDuplicateWarningMessage(portDuplicationResult);
			_003Cmodel_003EP.Status.Reset();
			return await _003Cmessage_003EP.ConfirmAsync("⚠\ufe0f 检测到端口重复使用\n\n" + text + "\n是否仍要继续生成接线清单？", "端口重复检测");
		}
		return true;
	}

	private void MergeAllExcelData(IEnumerable<string> files, string templatePath, string cabinetRoomPath, string savePath)
	{
		Dictionary<string, int> cabinetConnectionCount = new Dictionary<string, int>();
		using Workbook workbook = _003Cexcel_003EP.GetWorkbook(templatePath);
		Worksheet worksheet = workbook.Worksheets[0];
		List<系统二室机柜信息表> cabinetRooms = ExtractData(cabinetRoomPath);
		List<系统二室网络设备IP表> list = ExtractData(files);
		int nextRow = 4;
		for (int i = 0; i < list.Count; i++)
		{
			系统二室网络设备IP表 系统二室网络设备IP表 = list[i];
			string 接口类型 = 系统二室网络设备IP表.接口类型;
			if (!(接口类型 == "电口"))
			{
				if (接口类型 == "光口")
				{
					WriteOpticalConnections(worksheet, cabinetRooms, list, ref nextRow, 系统二室网络设备IP表, cabinetConnectionCount);
				}
			}
			else
			{
				WriteRows(worksheet, cabinetRooms, ref nextRow, 系统二室网络设备IP表, "网线", 系统二室网络设备IP表.A_Start_名称, 系统二室网络设备IP表.A_End_名称, 系统二室网络设备IP表.B_Start_名称, 系统二室网络设备IP表.B_End_名称);
			}
		}
		workbook.Save(savePath);
	}

	private List<系统二室网络设备IP表> ExtractData(IEnumerable<string> files)
	{
		List<系统二室网络设备IP表> list = new List<系统二室网络设备IP表>();
		foreach (string file in files)
		{
			Workbook workbook = _003Cexcel_003EP.GetWorkbook(file);
			foreach (Worksheet worksheet in workbook.Worksheets)
			{
				for (int i = 4; i <= worksheet.Cells.MaxDataRow; i++)
				{
					list.Add(new 系统二室网络设备IP表
					{
						接口类型 = worksheet.Cells[i, 21]?.Value?.ToString(),
						序号 = worksheet.Cells[i, 0]?.Value?.ToString(),
						A_Start_名称 = worksheet.Cells[i, 1]?.Value?.ToString(),
						A_Start_机柜或盘台 = worksheet.Cells[i, 2]?.Value?.ToString(),
						A_Start_位号 = worksheet.Cells[i, 3]?.Value?.ToString(),
						A_Start_端口 = worksheet.Cells[i, 4]?.Value?.ToString(),
						A_End_名称 = worksheet.Cells[i, 5]?.Value?.ToString(),
						A_End_机柜或盘台 = worksheet.Cells[i, 6]?.Value?.ToString(),
						A_End_位号 = worksheet.Cells[i, 7]?.Value?.ToString(),
						A_End_端口 = worksheet.Cells[i, 8]?.Value?.ToString(),
						B_Start_名称 = worksheet.Cells[i, 10]?.Value?.ToString(),
						B_Start_机柜或盘台 = worksheet.Cells[i, 11]?.Value?.ToString(),
						B_Start_位号 = worksheet.Cells[i, 12]?.Value?.ToString(),
						B_Start_端口 = worksheet.Cells[i, 13]?.Value?.ToString(),
						B_End_名称 = worksheet.Cells[i, 14]?.Value?.ToString(),
						B_End_机柜或盘台 = worksheet.Cells[i, 15]?.Value?.ToString(),
						B_End_位号 = worksheet.Cells[i, 16]?.Value?.ToString(),
						B_End_端口 = worksheet.Cells[i, 17]?.Value?.ToString(),
						IsDeletedRow = CheckRowStrikeout(worksheet, i)
					});
				}
			}
		}
		return list;
	}

	private List<系统二室机柜信息表> ExtractData(string path)
	{
		List<系统二室机柜信息表> list = new List<系统二室机柜信息表>();
		Workbook workbook = _003Cexcel_003EP.GetWorkbook(path);
		foreach (Worksheet worksheet in workbook.Worksheets)
		{
			for (int i = 1; i <= worksheet.Cells.MaxDataRow; i++)
			{
				list.Add(new 系统二室机柜信息表
				{
					盘柜位号 = worksheet.Cells[i, 2]?.Value?.ToString(),
					房间号 = worksheet.Cells[i, 3]?.Value?.ToString()
				});
			}
		}
		return list;
	}

	private void WriteRows(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, ref int nextRow, 系统二室网络设备IP表 rowData, string cableType, string aStart, string aEnd, string bStart, string bEnd)
	{
		if (!string.IsNullOrEmpty(rowData.A_Start_机柜或盘台) && !string.IsNullOrEmpty(rowData.A_End_机柜或盘台) && rowData.A_Start_机柜或盘台 != "/" && rowData.A_End_机柜或盘台 != "/")
		{
			WriteRow(worksheet, cabinetRooms, ref nextRow, rowData.序号, rowData.A_Start_机柜或盘台, rowData.A_End_机柜或盘台, rowData.A_Start_位号, rowData.A_Start_端口, rowData.A_End_位号, rowData.A_End_端口, cableType, aEnd + "连接" + aStart);
			if (rowData.IsDeletedRow)
			{
				ApplyStrikeoutToRow(worksheet, nextRow - 1);
			}
		}
		if (!string.IsNullOrEmpty(rowData.B_Start_机柜或盘台) && !string.IsNullOrEmpty(rowData.B_End_机柜或盘台) && rowData.B_Start_机柜或盘台 != "/" && rowData.B_End_机柜或盘台 != "/")
		{
			WriteRow(worksheet, cabinetRooms, ref nextRow, rowData.序号, rowData.B_Start_机柜或盘台, rowData.B_End_机柜或盘台, rowData.B_Start_位号, rowData.B_Start_端口, rowData.B_End_位号, rowData.B_End_端口, cableType, bEnd + "连接" + bStart);
			if (rowData.IsDeletedRow)
			{
				ApplyStrikeoutToRow(worksheet, nextRow - 1);
			}
		}
	}

	private void WriteRow(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, ref int nextRow, string 序号, string startCabinet, string endCabinet, string startPos, string startPort, string endPos, string endPort, string cableType, string leftConnectRight)
	{
		string text = cabinetRooms.FirstOrDefault((系统二室机柜信息表 c) => c.盘柜位号 == startCabinet)?.房间号;
		string text2 = cabinetRooms.FirstOrDefault((系统二室机柜信息表 c) => c.盘柜位号 == endCabinet)?.房间号;
		worksheet.Cells[nextRow, 0].Value = 序号;
		worksheet.Cells[nextRow, 1].Value = ((startCabinet == endCabinet) ? "/" : "设计院确定");
		worksheet.Cells[nextRow, 4].Value = cableType;
		worksheet.Cells[nextRow, 5].Value = text ?? "";
		worksheet.Cells[nextRow, 6].Value = startCabinet;
		worksheet.Cells[nextRow, 7].Value = startPos;
		worksheet.Cells[nextRow, 8].Value = startPort;
		worksheet.Cells[nextRow, 9].Value = "/";
		worksheet.Cells[nextRow, 10].Value = ((startCabinet == endCabinet) ? "/" : "设计院确定");
		worksheet.Cells[nextRow, 11].Value = text2 ?? "";
		worksheet.Cells[nextRow, 12].Value = endCabinet;
		worksheet.Cells[nextRow, 13].Value = endPos;
		worksheet.Cells[nextRow, 14].Value = endPort;
		worksheet.Cells[nextRow, 15].Value = "/";
		worksheet.Cells[nextRow, 18].Value = leftConnectRight;
		nextRow++;
	}

	private void WriteOpticalConnections(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, List<系统二室网络设备IP表> list, ref int nextRow, 系统二室网络设备IP表 rowData, Dictionary<string, int> cabinetConnectionCount)
	{
		bool isStartD = list.Any(delegate(系统二室网络设备IP表 l)
		{
			if (l.A_Start_机柜或盘台 == rowData.A_Start_机柜或盘台)
			{
				string a_Start_名称 = l.A_Start_名称;
				if (a_Start_名称 != null && a_Start_名称.Contains("控制器"))
				{
					return true;
				}
			}
			return l.A_End_机柜或盘台 == rowData.A_Start_机柜或盘台 && (l.A_End_名称?.Contains("控制器") ?? false);
		});
		bool isEndD = list.Any(delegate(系统二室网络设备IP表 l)
		{
			if (l.A_Start_机柜或盘台 == rowData.A_End_机柜或盘台)
			{
				string a_Start_名称 = l.A_Start_名称;
				if (a_Start_名称 != null && a_Start_名称.Contains("控制器"))
				{
					return true;
				}
			}
			return l.A_End_机柜或盘台 == rowData.A_End_机柜或盘台 && (l.A_End_名称?.Contains("控制器") ?? false);
		});
		if (!string.IsNullOrEmpty(rowData.A_Start_机柜或盘台) && !string.IsNullOrEmpty(rowData.A_End_机柜或盘台) && rowData.A_Start_机柜或盘台 != "/" && rowData.A_End_机柜或盘台 != "/")
		{
			WriteComplexOpticalRow(worksheet, cabinetRooms, ref nextRow, rowData, cabinetConnectionCount, rowData.A_Start_机柜或盘台, rowData.A_End_机柜或盘台, isStartD, isEndD, rowData.A_Start_名称, rowData.A_End_名称);
		}
		bool isStartD2 = list.Any(delegate(系统二室网络设备IP表 l)
		{
			if (l.B_Start_机柜或盘台 == rowData.B_Start_机柜或盘台)
			{
				string b_Start_名称 = l.B_Start_名称;
				if (b_Start_名称 != null && b_Start_名称.Contains("控制器"))
				{
					return true;
				}
			}
			return l.B_End_机柜或盘台 == rowData.B_Start_机柜或盘台 && (l.B_End_名称?.Contains("控制器") ?? false);
		});
		bool isEndD2 = list.Any(delegate(系统二室网络设备IP表 l)
		{
			if (l.B_Start_机柜或盘台 == rowData.B_End_机柜或盘台)
			{
				string b_Start_名称 = l.B_Start_名称;
				if (b_Start_名称 != null && b_Start_名称.Contains("控制器"))
				{
					return true;
				}
			}
			return l.B_End_机柜或盘台 == rowData.B_End_机柜或盘台 && (l.B_End_名称?.Contains("控制器") ?? false);
		});
		if (!string.IsNullOrEmpty(rowData.B_Start_机柜或盘台) && !string.IsNullOrEmpty(rowData.B_End_机柜或盘台) && rowData.B_Start_机柜或盘台 != "/" && rowData.B_End_机柜或盘台 != "/")
		{
			WriteComplexOpticalRowB(worksheet, cabinetRooms, ref nextRow, rowData, cabinetConnectionCount, rowData.B_Start_机柜或盘台, rowData.B_End_机柜或盘台, isStartD2, isEndD2, rowData.B_Start_名称, rowData.B_End_名称);
		}
	}

	private void WriteComplexOpticalRow(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, ref int nextRow, 系统二室网络设备IP表 rowData, Dictionary<string, int> cabinetConnectionCount, string startCabinet, string endCabinet, bool isStartD, bool isEndD, string startName, string EndName)
	{
		if (startCabinet == endCabinet)
		{
			WriteRow(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, rowData.A_Start_位号, rowData.A_Start_端口, rowData.A_End_位号, rowData.A_End_端口, "单模双芯LC光纤跳线", EndName + "连接" + startName);
			if (rowData.IsDeletedRow)
			{
				ApplyStrikeoutToRow(worksheet, nextRow - 1);
			}
			return;
		}
		int startConnections = IncrementCabinetConnection(cabinetConnectionCount, startCabinet);
		int endConnections = IncrementCabinetConnection(cabinetConnectionCount, endCabinet);
		WriteOpticalRow1(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, rowData.A_Start_位号, rowData.A_Start_端口, isStartD, startConnections, EndName + "连接" + startName);
		if (rowData.IsDeletedRow)
		{
			ApplyStrikeoutToRow(worksheet, nextRow - 1);
		}
		WriteOpticalCableRow(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, startConnections, endConnections, isStartD, isEndD, EndName + "连接" + startName);
		if (rowData.IsDeletedRow)
		{
			ApplyStrikeoutToRow(worksheet, nextRow - 1);
		}
		WriteOpticalRow2(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, rowData.A_End_位号, rowData.A_End_端口, isEndD, endConnections, EndName + "连接" + startName);
		if (rowData.IsDeletedRow)
		{
			ApplyStrikeoutToRow(worksheet, nextRow - 1);
		}
	}

	private void WriteComplexOpticalRowB(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, ref int nextRow, 系统二室网络设备IP表 rowData, Dictionary<string, int> cabinetConnectionCount, string startCabinet, string endCabinet, bool isStartD, bool isEndD, string startName, string EndName)
	{
		if (startCabinet == endCabinet)
		{
			WriteRow(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, rowData.B_Start_位号, rowData.B_Start_端口, rowData.B_End_位号, rowData.B_End_端口, "单模双芯LC光纤跳线", EndName + "连接" + startName);
			if (rowData.IsDeletedRow)
			{
				ApplyStrikeoutToRow(worksheet, nextRow - 1);
			}
			return;
		}
		int startConnections = IncrementCabinetConnection(cabinetConnectionCount, startCabinet);
		int endConnections = IncrementCabinetConnection(cabinetConnectionCount, endCabinet);
		WriteOpticalRow1(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, rowData.B_Start_位号, rowData.B_Start_端口, isStartD, startConnections, EndName + "连接" + startName);
		if (rowData.IsDeletedRow)
		{
			ApplyStrikeoutToRow(worksheet, nextRow - 1);
		}
		WriteOpticalCableRow(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, startConnections, endConnections, isStartD, isEndD, EndName + "连接" + startName);
		if (rowData.IsDeletedRow)
		{
			ApplyStrikeoutToRow(worksheet, nextRow - 1);
		}
		WriteOpticalRow2(worksheet, cabinetRooms, ref nextRow, rowData.序号, startCabinet, endCabinet, rowData.B_End_位号, rowData.B_End_端口, isEndD, endConnections, EndName + "连接" + startName);
		if (rowData.IsDeletedRow)
		{
			ApplyStrikeoutToRow(worksheet, nextRow - 1);
		}
	}

	private void WriteOpticalRow1(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, ref int nextRow, string 序号, string startCabinet, string endCabinet, string startPos, string startPort, bool isStartControl, int startConnections, string connectName)
	{
		string text = cabinetRooms.FirstOrDefault((系统二室机柜信息表 c) => c.盘柜位号 == startCabinet)?.房间号;
		worksheet.Cells[nextRow, 0].Value = 序号;
		worksheet.Cells[nextRow, 1].Value = "/";
		worksheet.Cells[nextRow, 4].Value = "单模双芯LC光纤跳线";
		worksheet.Cells[nextRow, 5].Value = text ?? "";
		worksheet.Cells[nextRow, 6].Value = startCabinet;
		worksheet.Cells[nextRow, 7].Value = startPos ?? "";
		worksheet.Cells[nextRow, 8].Value = startPort + "-TX";
		worksheet.Cells[nextRow, 9].Value = startPort + "-RX";
		worksheet.Cells[nextRow, 10].Value = "/";
		worksheet.Cells[nextRow, 11].Value = text ?? "";
		worksheet.Cells[nextRow, 12].Value = startCabinet;
		worksheet.Cells[nextRow, 13].Value = $"{CalculateBpNumber(isStartControl, startConnections)}BP";
		worksheet.Cells[nextRow, 14].Value = $"{CalculateCurrentConnection(isStartControl, startConnections)}-TX";
		worksheet.Cells[nextRow, 15].Value = $"{CalculateCurrentConnection(isStartControl, startConnections)}-RX";
		worksheet.Cells[nextRow, 18].Value = connectName;
		nextRow++;
	}

	private void WriteOpticalCableRow(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, ref int nextRow, string 序号, string startCabinet, string endCabinet, int startConnections, int endConnections, bool isStartControl, bool isEndControl, string connectName)
	{
		string text = cabinetRooms.FirstOrDefault((系统二室机柜信息表 c) => c.盘柜位号 == startCabinet)?.房间号;
		string text2 = cabinetRooms.FirstOrDefault((系统二室机柜信息表 c) => c.盘柜位号 == endCabinet)?.房间号;
		worksheet.Cells[nextRow, 0].Value = 序号;
		worksheet.Cells[nextRow, 1].Value = ((startCabinet == endCabinet) ? "/" : "设计院确定");
		worksheet.Cells[nextRow, 4].Value = "四芯单模光缆";
		worksheet.Cells[nextRow, 5].Value = text2 ?? "";
		worksheet.Cells[nextRow, 6].Value = endCabinet;
		worksheet.Cells[nextRow, 7].Value = $"{CalculateBpNumber(isEndControl, endConnections)}BP";
		worksheet.Cells[nextRow, 8].Value = $"{CalculateCurrentConnection(isEndControl, endConnections)}-RX";
		worksheet.Cells[nextRow, 9].Value = $"{CalculateCurrentConnection(isEndControl, endConnections)}-TX";
		worksheet.Cells[nextRow, 10].Value = ((startCabinet == endCabinet) ? "/" : "设计院确定");
		worksheet.Cells[nextRow, 11].Value = text ?? "";
		worksheet.Cells[nextRow, 12].Value = startCabinet;
		worksheet.Cells[nextRow, 13].Value = $"{CalculateBpNumber(isStartControl, startConnections)}BP";
		worksheet.Cells[nextRow, 14].Value = $"{CalculateCurrentConnection(isStartControl, startConnections)}-TX";
		worksheet.Cells[nextRow, 15].Value = $"{CalculateCurrentConnection(isStartControl, startConnections)}-RX";
		worksheet.Cells[nextRow, 18].Value = connectName;
		nextRow++;
	}

	private void WriteOpticalRow2(Worksheet worksheet, List<系统二室机柜信息表> cabinetRooms, ref int nextRow, string 序号, string startCabinet, string endCabinet, string endPos, string endPort, bool isEndControl, int endConnections, string connectName)
	{
		string text = cabinetRooms.FirstOrDefault((系统二室机柜信息表 c) => c.盘柜位号 == endCabinet)?.房间号;
		worksheet.Cells[nextRow, 0].Value = 序号;
		worksheet.Cells[nextRow, 1].Value = "/";
		worksheet.Cells[nextRow, 4].Value = "单模双芯LC光纤跳线";
		worksheet.Cells[nextRow, 5].Value = text ?? "";
		worksheet.Cells[nextRow, 6].Value = endCabinet;
		worksheet.Cells[nextRow, 7].Value = $"{CalculateBpNumber(isEndControl, endConnections)}BP";
		worksheet.Cells[nextRow, 8].Value = $"{CalculateCurrentConnection(isEndControl, endConnections)}-RX";
		worksheet.Cells[nextRow, 9].Value = $"{CalculateCurrentConnection(isEndControl, endConnections)}-TX";
		worksheet.Cells[nextRow, 10].Value = "/";
		worksheet.Cells[nextRow, 11].Value = text ?? "";
		worksheet.Cells[nextRow, 12].Value = endCabinet;
		worksheet.Cells[nextRow, 13].Value = endPos ?? "";
		worksheet.Cells[nextRow, 14].Value = endPort + "-RX";
		worksheet.Cells[nextRow, 15].Value = endPort + "-TX";
		worksheet.Cells[nextRow, 18].Value = connectName;
		nextRow++;
	}

	private int IncrementCabinetConnection(Dictionary<string, int> cabinetConnectionCount, string cabinet)
	{
		if (!cabinetConnectionCount.ContainsKey(cabinet))
		{
			cabinetConnectionCount[cabinet] = 1;
		}
		else
		{
			cabinetConnectionCount[cabinet]++;
		}
		return cabinetConnectionCount[cabinet];
	}

	private int CalculateBpNumber(bool isControlCabinet, int totalConnections)
	{
		int num = (isControlCabinet ? 3 : 10);
		return 100 + ((totalConnections % num == 0) ? (totalConnections / num) : (1 + totalConnections / num));
	}

	private int CalculateCurrentConnection(bool isControlCabinet, int totalConnections)
	{
		int num = (isControlCabinet ? 3 : 10);
		if (totalConnections % num != 0)
		{
			return totalConnections % num;
		}
		return num;
	}

	private bool CheckRowStrikeout(Worksheet worksheet, int row)
	{
		int maxDataColumn = worksheet.Cells.MaxDataColumn;
		for (int i = 0; i <= maxDataColumn; i++)
		{
			Cell cell = worksheet.Cells[row, i];
			if (cell != null)
			{
				Aspose.Cells.Style style = cell.GetStyle();
				if (style != null && style.Font != null && style.Font.IsStrikeout)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ApplyStrikeoutToRow(Worksheet worksheet, int row, int startCol = 0, int? endCol = null)
	{
		int num = endCol ?? worksheet.Cells.MaxDataColumn;
		Workbook workbook = worksheet.Workbook;
		Aspose.Cells.Style style = workbook.CreateStyle();
		style.Font.IsStrikeout = true;
		for (int i = startCol; i <= num; i++)
		{
			worksheet.Cells[row, i].SetStyle(style);
		}
	}

	/// <summary>
	/// 检测端口重复使用
	/// </summary>
	private PortDuplicationResult CheckPortDuplication(List<系统二室网络设备IP表> ipDatas)
	{
		PortDuplicationResult portDuplicationResult = new PortDuplicationResult();
		Dictionary<string, List<PortUsageInfo>> dictionary = new Dictionary<string, List<PortUsageInfo>>();
		for (int i = 0; i < ipDatas.Count; i++)
		{
			系统二室网络设备IP表 系统二室网络设备IP表 = ipDatas[i];
			int dataRowIndex = i + 1;
			if (!string.IsNullOrEmpty(系统二室网络设备IP表.A_Start_机柜或盘台) && 系统二室网络设备IP表.A_Start_机柜或盘台 != "/" && !string.IsNullOrEmpty(系统二室网络设备IP表.A_Start_位号) && !string.IsNullOrEmpty(系统二室网络设备IP表.A_Start_端口))
			{
				CheckAndRecordPort(dictionary, 系统二室网络设备IP表.A_Start_机柜或盘台, 系统二室网络设备IP表.A_Start_位号, 系统二室网络设备IP表.A_Start_端口, dataRowIndex, "A路起点", 系统二室网络设备IP表.序号);
			}
			if (!string.IsNullOrEmpty(系统二室网络设备IP表.A_End_机柜或盘台) && 系统二室网络设备IP表.A_End_机柜或盘台 != "/" && !string.IsNullOrEmpty(系统二室网络设备IP表.A_End_位号) && !string.IsNullOrEmpty(系统二室网络设备IP表.A_End_端口))
			{
				CheckAndRecordPort(dictionary, 系统二室网络设备IP表.A_End_机柜或盘台, 系统二室网络设备IP表.A_End_位号, 系统二室网络设备IP表.A_End_端口, dataRowIndex, "A路终点", 系统二室网络设备IP表.序号);
			}
			if (!string.IsNullOrEmpty(系统二室网络设备IP表.B_Start_机柜或盘台) && 系统二室网络设备IP表.B_Start_机柜或盘台 != "/" && !string.IsNullOrEmpty(系统二室网络设备IP表.B_Start_位号) && !string.IsNullOrEmpty(系统二室网络设备IP表.B_Start_端口))
			{
				CheckAndRecordPort(dictionary, 系统二室网络设备IP表.B_Start_机柜或盘台, 系统二室网络设备IP表.B_Start_位号, 系统二室网络设备IP表.B_Start_端口, dataRowIndex, "B路起点", 系统二室网络设备IP表.序号);
			}
			if (!string.IsNullOrEmpty(系统二室网络设备IP表.B_End_机柜或盘台) && 系统二室网络设备IP表.B_End_机柜或盘台 != "/" && !string.IsNullOrEmpty(系统二室网络设备IP表.B_End_位号) && !string.IsNullOrEmpty(系统二室网络设备IP表.B_End_端口))
			{
				CheckAndRecordPort(dictionary, 系统二室网络设备IP表.B_End_机柜或盘台, 系统二室网络设备IP表.B_End_位号, 系统二室网络设备IP表.B_End_端口, dataRowIndex, "B路终点", 系统二室网络设备IP表.序号);
			}
		}
		foreach (KeyValuePair<string, List<PortUsageInfo>> item in dictionary)
		{
			if (item.Value.Count > 1)
			{
				portDuplicationResult.DuplicatePorts.Add(item.Key, item.Value);
			}
		}
		return portDuplicationResult;
	}

	/// <summary>
	/// 记录端口使用情况
	/// </summary>
	private void CheckAndRecordPort(Dictionary<string, List<PortUsageInfo>> portUsageDict, string cabinet, string position, string port, int dataRowIndex, string location, string 序号)
	{
		string key = $"{cabinet}|{position}|{port}";
		if (!portUsageDict.ContainsKey(key))
		{
			portUsageDict[key] = new List<PortUsageInfo>();
		}
		portUsageDict[key].Add(new PortUsageInfo
		{
			Cabinet = cabinet,
			Position = position,
			Port = port,
			DataRowIndex = dataRowIndex,
			Location = location,
			序号 = 序号
		});
	}

	/// <summary>
	/// 构建重复端口的警告信息
	/// </summary>
	private string BuildDuplicateWarningMessage(PortDuplicationResult result)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
		handler.AppendLiteral("共发现 ");
		handler.AppendFormatted(result.DuplicatePorts.Count);
		handler.AppendLiteral(" 个端口重复使用：");
		stringBuilder3.AppendLine(ref handler);
		stringBuilder.AppendLine();
		int num = 1;
		foreach (KeyValuePair<string, List<PortUsageInfo>> item in result.DuplicatePorts.OrderBy<KeyValuePair<string, List<PortUsageInfo>>, string>((KeyValuePair<string, List<PortUsageInfo>> x) => x.Value.First().Cabinet))
		{
			List<PortUsageInfo> value = item.Value;
			PortUsageInfo portUsageInfo = value.First();
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(24, 4, stringBuilder2);
			handler.AppendLiteral("【");
			handler.AppendFormatted(num);
			handler.AppendLiteral("】机柜/盘台：");
			handler.AppendFormatted(portUsageInfo.Cabinet);
			handler.AppendLiteral("  |  位号：");
			handler.AppendFormatted(portUsageInfo.Position);
			handler.AppendLiteral("  |  端口：");
			handler.AppendFormatted(portUsageInfo.Port);
			stringBuilder4.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
			handler.AppendLiteral("     重复使用 ");
			handler.AppendFormatted(value.Count);
			handler.AppendLiteral(" 次：");
			stringBuilder5.AppendLine(ref handler);
			foreach (PortUsageInfo item2 in value.OrderBy((PortUsageInfo u) => u.DataRowIndex))
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder6 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(17, 3, stringBuilder2);
				handler.AppendLiteral("     • 序号 ");
				handler.AppendFormatted(item2.序号);
				handler.AppendLiteral("（第");
				handler.AppendFormatted(item2.DataRowIndex);
				handler.AppendLiteral("条数据，");
				handler.AppendFormatted(item2.Location);
				handler.AppendLiteral("）");
				stringBuilder6.AppendLine(ref handler);
			}
			stringBuilder.AppendLine();
			num++;
		}
		stringBuilder.AppendLine("提示：您可以复制此信息进行核对。");
		return stringBuilder.ToString();
	}
}
