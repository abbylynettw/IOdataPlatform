using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Aspose.Cells;
using Aspose.Words;
using Aspose.Words.Tables;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using IODataPlatform.Models;
using LYSoft.Libs;
using LYSoft.Libs.ServiceInterfaces;
using Wpf.Ui;

namespace IODataPlatform.Views.SubPages.Common;

/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
/// <inheritdoc />
public class ExtractPdfViewModel(GlobalModel model, IMessageService message, INavigationService navigation, IPickerService picker, WordService word, ExcelService excel) : ObservableObject()
{
	[ObservableProperty]
	private ObservableCollection<string> ioFields = new ObservableCollection<string>();

	[ObservableProperty]
	private string dest = "请选择输出目录";

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.SetDestCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? setDestCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.AppendFilesCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? appendFilesCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.RemoveAllFilesCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? removeAllFilesCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.RemovePdfFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<FileToExtract>? removePdfFileCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.OpenPdfDestFileCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand<FileToExtract>? openPdfDestFileCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.GoBackCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private RelayCommand? goBackCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.CopyToClipboardCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand<string>? copyToClipboardCommand;

	/// <summary>The backing field for <see cref="P:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.ConfirmCommand" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	private AsyncRelayCommand? confirmCommand;

	public ObservableCollection<FileToExtract> PdfFiles { get; } = new ObservableCollection<FileToExtract>();

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.ioFields" />
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableCollection<string> IoFields
	{
		get
		{
			return ioFields;
		}
		[MemberNotNull("ioFields")]
		set
		{
			if (!EqualityComparer<ObservableCollection<string>>.Default.Equals(ioFields, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IoFields);
				ioFields = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IoFields);
			}
		}
	}

	/// <inheritdoc cref="F:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.dest" />
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

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.SetDest" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand SetDestCommand => setDestCommand ?? (setDestCommand = new RelayCommand(SetDest));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.AppendFiles" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand AppendFilesCommand => appendFilesCommand ?? (appendFilesCommand = new RelayCommand(AppendFiles));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.RemoveAllFiles" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RemoveAllFilesCommand => removeAllFilesCommand ?? (removeAllFilesCommand = new AsyncRelayCommand(RemoveAllFiles));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.RemovePdfFile(IODataPlatform.Views.SubPages.Common.FileToExtract)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<FileToExtract> RemovePdfFileCommand => removePdfFileCommand ?? (removePdfFileCommand = new RelayCommand<FileToExtract>(RemovePdfFile));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.OpenPdfDestFile(IODataPlatform.Views.SubPages.Common.FileToExtract)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<FileToExtract> OpenPdfDestFileCommand => openPdfDestFileCommand ?? (openPdfDestFileCommand = new RelayCommand<FileToExtract>(OpenPdfDestFile));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.GoBack" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand GoBackCommand => goBackCommand ?? (goBackCommand = new RelayCommand(GoBack));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand`1" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.CopyToClipboard(System.String)" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<string> CopyToClipboardCommand => copyToClipboardCommand ?? (copyToClipboardCommand = new AsyncRelayCommand<string>(CopyToClipboard));

	/// <summary>Gets an <see cref="T:CommunityToolkit.Mvvm.Input.IAsyncRelayCommand" /> instance wrapping <see cref="M:IODataPlatform.Views.SubPages.Common.ExtractPdfViewModel.Confirm" />.</summary>
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.2.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ConfirmCommand => confirmCommand ?? (confirmCommand = new AsyncRelayCommand(Confirm));

	[RelayCommand]
	private void SetDest()
	{
		string text = picker.PickFolder();
		if (text != null)
		{
			Dest = text;
		}
	}

	[RelayCommand]
	private void AppendFiles()
	{
		string[] array = picker.OpenFiles("文件(*.pdf; *.doc; *.docx; *.xls; *.xlsx)|*.pdf; *.doc; *.docx; *.xls; *.xlsx");
		if (array != null)
		{
			PdfFiles.Reset(PdfFiles.AppendRange(array.Select((string x) => new FileToExtract(x))).Distinct().ToList());
			if ((from x in PdfFiles
				group x by x.FileName.ToLower()).Any((IGrouping<string, FileToExtract> x) => x.Count() > 1))
			{
				throw new Exception("列表中有两个或多个文件名相同，如果开始提取，一部分结果文件会被覆盖");
			}
		}
	}

	[RelayCommand]
	private async Task RemoveAllFiles()
	{
		if (await message.ConfirmAsync("是否清空文件列表"))
		{
			PdfFiles.Clear();
		}
	}

	[RelayCommand]
	private void RemovePdfFile(FileToExtract file)
	{
		PdfFiles.Remove(file);
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
		navigation.GoBack();
	}

	[RelayCommand]
	private async Task CopyToClipboard(string param)
	{
		Clipboard.SetText(param);
		await message.SuccessAsync("已复制到剪贴板：" + param);
	}

	[RelayCommand]
	private async Task Confirm()
	{
		if (PdfFiles.Count == 0)
		{
			throw new Exception("请添加文件");
		}
		if (Dest == "请选择输出目录")
		{
			throw new Exception("请选择输出目录");
		}
		if (!Directory.Exists(Dest))
		{
			Directory.CreateDirectory(Dest);
		}
		model.Status.Busy("正在提取数据……");
		await Task.WhenAll(PdfFiles.Select(ExtractPdf));
		model.Status.Busy("正在合并数据……");
		MergeAllExcelData(from x in PdfFiles
			where x.Result == "处理完成"
			select Path.Combine(Dest, x.FileName + ".xlsx"));
		model.Status.Success("已成功将信息提取到指定目录：" + Dest + "，合并后的文件名为：汇总.xlsx");
	}

	private void MergeAllExcelData(IEnumerable<string> files)
	{
		using Workbook workbook = excel.GetWorkbook();
		foreach (string file in files)
		{
			Workbook workbook2 = excel.GetWorkbook(file);
			foreach (Worksheet worksheet3 in workbook2.Worksheets)
			{
				Worksheet worksheet = workbook.Worksheets[workbook.Worksheets.Add()];
				worksheet.Copy(worksheet3);
			}
		}
		foreach (IGrouping<int, Worksheet> item in from x in workbook.Worksheets
			group x by x.Cells.MaxDataColumn)
		{
			Worksheet worksheet2 = item.First();
			int num = worksheet2.Cells.MaxDataRow + 1;
			foreach (Worksheet item2 in item.Skip(1))
			{
				int maxDataRow = item2.Cells.MaxDataRow;
				for (int num2 = 0; num2 <= maxDataRow; num2++)
				{
					int maxDataColumn = item2.Cells.MaxDataColumn;
					for (int num3 = 0; num3 <= maxDataColumn; num3++)
					{
						worksheet2.Cells[num, num3].Value = item2.Cells[num2, num3].Value;
					}
					num++;
				}
				workbook.Worksheets.RemoveAt(item2.Name);
			}
			worksheet2.Cells.RemoveDuplicates();
			worksheet2.Cells.DeleteBlankRows();
			if (worksheet2.Cells.MaxDataRow <= 0)
			{
				workbook.Worksheets.RemoveAt(worksheet2.Name);
			}
		}
		workbook.Save(Path.Combine(Dest, "汇总.xlsx"));
	}

	public async Task ExtractPdf(FileToExtract fileData)
	{
		fileData.Result = "处理中";
		string fullname = fileData.FullName;
		await Task.Run(delegate
		{
			if (Path.GetExtension(fullname).Contains("xlsx", StringComparison.CurrentCultureIgnoreCase))
			{
				string text = Path.Combine(Dest, Path.GetFileNameWithoutExtension(fullname) + ".xlsx");
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				File.Copy(fullname, text);
			}
			else
			{
				if (!Path.GetExtension(fullname).Contains("xls", StringComparison.CurrentCultureIgnoreCase))
				{
					Document document = word.GetDocument(fullname);
					Table[] array = word.GetTables(document).ToArray();
					using Workbook workbook = excel.GetWorkbook();
					for (int i = 0; i < array.Length - 1; i++)
					{
						workbook.Worksheets.Add();
					}
					for (int j = 0; j < array.Length; j++)
					{
						Table table = array[j];
						Worksheet worksheet = workbook.Worksheets[j];
						Cells cells = worksheet.Cells;
						for (int k = 0; k < table.Rows.Count; k++)
						{
							Aspose.Words.Tables.Row row = table.Rows[k];
							for (int l = 0; l < row.Cells.Count; l++)
							{
								if (row.Cells[l].Range.Text != null)
								{
									cells[k, l].Value = row.Cells[l].Range.Text.Replace("\a", "").Replace("\n", "").Replace("\r", "");
								}
							}
						}
					}
					workbook.Save(Path.Combine(Dest, Path.GetFileNameWithoutExtension(fullname) + ".xlsx"));
					return;
				}
				Workbook workbook2 = excel.GetWorkbook(fullname);
				workbook2.Save(Path.Combine(Dest, Path.GetFileNameWithoutExtension(fullname) + ".xlsx"));
			}
		});
		fileData.Result = "处理完成";
		GC.Collect();
	}
}
