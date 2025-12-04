﻿using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;

using IODataPlatform.Models;
using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.SubPages.Common;

public partial class ExtractPdfViewModel(
    GlobalModel model, IMessageService message, INavigationService navigation, 
    IPickerService picker, WordService word, ExcelService excel) : ObservableObject {

    [ObservableProperty]
    private ObservableCollection<string> ioFields = [];

    [ObservableProperty]
    private string dest = "请选择输出目录";

    public ObservableCollection<FileToExtract> PdfFiles { get; } = [];

    [RelayCommand]
    private void SetDest() {
        if (picker.PickFolder() is string folder) { Dest = folder; }
    }

    [RelayCommand]
    private void AppendFiles() {
        if (picker.OpenFiles("文件(*.pdf; *.doc; *.docx; *.xls; *.xlsx)|*.pdf; *.doc; *.docx; *.xls; *.xlsx") is not string[] files) { return; }
        PdfFiles.Reset(PdfFiles.AppendRange(files.Select(x => new FileToExtract(x))).Distinct().ToList());
        if (PdfFiles.GroupBy(x => x.FileName.ToLower()).Any(x => x.Count() > 1)) { 
            throw new("列表中有两个或多个文件名相同，如果开始提取，一部分结果文件会被覆盖");
        }
    }

    [RelayCommand]
    private async Task RemoveAllFiles() {
        if (!await message.ConfirmAsync("是否清空文件列表")) { return; }
        PdfFiles.Clear();
    }

    [RelayCommand]
    private void RemovePdfFile(FileToExtract file) {
        PdfFiles.Remove(file);
    }

    [RelayCommand]
    private void OpenPdfDestFile(FileToExtract file) {
        var path = Path.Combine(Dest, $"{file.FileName}.xlsx");
        if (!File.Exists(path)) { throw new("结果文件不存在，请先处理"); }
        Process.Start("explorer.exe", path);
    }

    [RelayCommand]
    private void GoBack() {
        navigation.GoBack();
    }

    [RelayCommand]
    private async Task CopyToClipboard(string param) {
        Clipboard.SetText(param);
        await message.SuccessAsync($"已复制到剪贴板：{param}");
    }

    [RelayCommand]
    private async Task Confirm() {
        if (PdfFiles.Count == 0) { throw new("请添加文件"); }
        if (Dest == "请选择输出目录") { throw new("请选择输出目录"); }
        if (!Directory.Exists(Dest)) { Directory.CreateDirectory(Dest); }
        model.Status.Busy("正在提取数据……");
        await Task.WhenAll(PdfFiles.Select(ExtractPdf));
        model.Status.Busy("正在合并数据……");
        MergeAllExcelData(PdfFiles.Where(x => x.Result == "处理完成").Select(x => Path.Combine(Dest, $"{x.FileName}.xlsx")));
        model.Status.Success($"已成功将信息提取到指定目录：{Dest}，合并后的文件名为：汇总.xlsx");
    }

    private void MergeAllExcelData(IEnumerable<string> files) {
        using var wb = excel.GetWorkbook();
        foreach (var file in files) {
            var filewb = excel.GetWorkbook(file);
            foreach (var filews in filewb.Worksheets) {
                var ws = wb.Worksheets[wb.Worksheets.Add()];
                ws.Copy(filews);
            }
        }

        foreach (var wsGroups in wb.Worksheets.GroupBy(x => x.Cells.MaxDataColumn)) {
            var wsFirst = wsGroups.First();
            int firstEmptyRow = wsFirst.Cells.MaxDataRow + 1;
            foreach (var ws in wsGroups.Skip(1)) {
                int maxRow = ws.Cells.MaxDataRow;
                for (int row = 0; row <= maxRow; row++) {
                    int maxColumn = ws.Cells.MaxDataColumn;
                    for (int column = 0; column <= maxColumn; column++) {
                        wsFirst.Cells[firstEmptyRow, column].Value = ws.Cells[row, column].Value;
                    }
                    firstEmptyRow++;
                }
                wb.Worksheets.RemoveAt(ws.Name);
            }
            wsFirst.Cells.RemoveDuplicates();
            wsFirst.Cells.DeleteBlankRows();
            if (wsFirst.Cells.MaxDataRow <= 0) { wb.Worksheets.RemoveAt(wsFirst.Name); }
        }

        wb.Save(Path.Combine(Dest, "汇总.xlsx"));
    }

    public async Task ExtractPdf(FileToExtract fileData) {
        fileData.Result = "处理中";
        var fullname = fileData.FullName;
        await Task.Run(() => {
            if (Path.GetExtension(fullname).Contains("xlsx", StringComparison.CurrentCultureIgnoreCase)) {
                var dest = Path.Combine(Dest, $"{Path.GetFileNameWithoutExtension(fullname)}.xlsx");
                if (File.Exists(dest)) { File.Delete(dest); }
                File.Copy(fullname, dest);
            } else if (Path.GetExtension(fullname).Contains("xls", StringComparison.CurrentCultureIgnoreCase)) {
                var workbook = excel.GetWorkbook(fullname);
                workbook.Save(Path.Combine(Dest, $"{Path.GetFileNameWithoutExtension(fullname)}.xlsx"));
            } else {

                var doc = word.GetDocument(fullname);
                var tables = word.GetTables(doc).ToArray();
                using var workbook = excel.GetWorkbook();

                for (int i = 0; i < tables.Length - 1; i++) {
                    workbook.Worksheets.Add();
                }

                for (int i = 0; i < tables.Length; i++) {
                    var table = tables[i];
                    var ws = workbook.Worksheets[i];
                    var cells = ws.Cells;

                    for (int r = 0; r < table.Rows.Count; r++) {
                        var row = table.Rows[r];
                        for (int c = 0; c < row.Cells.Count; c++) {
                            if (row.Cells[c].Range.Text != null) {
                                cells[r, c].Value = row.Cells[c].Range.Text.Replace("\a", "").Replace("\n", "").Replace("\r", "");
                            }
                        }
                    }
                }

                workbook.Save(Path.Combine(Dest, $"{Path.GetFileNameWithoutExtension(fullname)}.xlsx"));
            }


        });
        fileData.Result = "处理完成";
        GC.Collect();
    }

}

public partial class FileToExtract(string fullname) : ObservableObject {
    public string FullName { get; } = fullname;
    public string FileName { get; } = Path.GetFileNameWithoutExtension(fullname);
    [ObservableProperty]
    private string result = string.Empty;
}