#pragma warning disable CA1822 // 将成员标记为 static

using System.IO;

using IODataPlatform.Utilities;

namespace IODataPlatform.Views.Pages;

// 导出部分

partial class DataAssetCenterViewModel {

    [RelayCommand]
    private async Task Export() {
        if (AllData == null) { throw new("没有可导出的数据"); }

        if (picker.SaveFile("Excel 文件(*.xlsx)|*.xlsx") is not string file) { return; }
        if (File.Exists(file)) { File.Delete(file); }

        model.Status.Busy($"正在导出数据……");

        await excel.FastExportAsync(await AllData.ToTableByDisplayAttributeAsync(), file);

        model.Status.Success($"已成功导出数据：{file}");
    }
    [RelayCommand]
    private async Task CopyToClipboard(string param)
    {
        Clipboard.SetText(param);
        await message.SuccessAsync($"已复制到剪贴板：{param}");
    }
}