using System.IO;
using System.Text.Json;

using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Services;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;

namespace IODataPlatform.Views.SubPages.Cable;

public partial class MatchViewModel(INavigationService navigation, SqlSugarContext context, StorageService storage, ExcelService excel, GlobalModel model, CableViewModel cable) : ObservableObject, INavigationAware {

    public void OnNavigatedFrom() { }

    public void OnNavigatedTo() {
        SubProject1 = cable.SubProject1 ?? throw new();
        SubProject2 = cable.SubProject2 ?? throw new();
        Header1 = $"{cable.Major1?.CableSystem} - {SubProject1.Name}";
        Header2 = $"{cable.Major2?.CableSystem} - {SubProject2.Name}";
    }

    [ObservableProperty]
    private MatchCableDataResult? result;

    [ObservableProperty]
    private ObservableCollection<MatchCableDataFail>? failist1;

    [ObservableProperty]
    private ObservableCollection<MatchCableDataFail>? failist2;
    
    [ObservableProperty]
    private ObservableCollection<CableData>? successList;

    [ObservableProperty]
    private int successDataCount = 0;

    [RelayCommand]
    private async Task Save() {
        model.Status.Busy("正在保存数据……");
        var file = storage.GetLocalAbsolutePath(storage.GetCableTempFileRelativePath());
        await File.WriteAllTextAsync(file, JsonSerializer.Serialize(Result ?? throw new("没有数据需要保存")));
        model.Status.Reset();
    }

    [RelayCommand]
    private async Task Load() {
        model.Status.Busy("正在加载数据……");
        var file = storage.GetLocalAbsolutePath(storage.GetCableTempFileRelativePath());
        try {
            Result = JsonSerializer.Deserialize<MatchCableDataResult>(await File.ReadAllTextAsync(file));
            model.Status.Reset();
        } catch {
            throw new("没有数据可以加载");
        }
    }

    [RelayCommand]
    private async Task Overwrite() {
        cable.AllData = [.. SuccessList];
        await cable.SaveAndUploadRealtimeFileAsync();
        navigation.GoBack();
    }

    [RelayCommand]
    private void Append() {
        throw new NotImplementedException();
    }

    [RelayCommand]
    private async Task UnMatch(CableData cable) {
        var result = await DataConverter.UnMatchCableData(cable);
        Failist1!.Add(new() { Data = result.Item1, Reason = "拆分" });
        Failist2!.Add(new() { Data = result.Item2, Reason = "拆分" });
        SuccessList!.Remove(cable);
        SuccessDataCount = SuccessList!.Count;
    }

    [RelayCommand]
    private async Task Match() {
        var major1 = cable.Major1 ?? throw new("请选择端接数据");
        var major2 = cable.Major2 ?? throw new("请选择端接数据");
        var subProjectId1 = SubProject1?.Id ?? throw new("请选择端接数据");
        var subProjectId2 = SubProject2?.Id ?? throw new("请选择端接数据");
        var publishId1 = Publish1?.Id ?? throw new("请选择端接数据");
        var publishId2 = Publish2?.Id ?? throw new("请选择端接数据");

        model.Status.Busy("正在获取端接数据……");
        var file1 = await storage.DownloadPublishTerminationFileAsync(subProjectId1, publishId1);
        var file2 = await storage.DownloadPublishTerminationFileAsync(subProjectId2, publishId2);

        var data1 =await Task.Run(async () => {
            var dataTable = await excel.GetDataTableAsStringAsync(file1, true);
            return dataTable.StringTableToIEnumerableByDiplay<TerminationData>();
        });

        var data2 = await Task.Run(async () => {
            var dataTable = await excel.GetDataTableAsStringAsync(file2, true);
            return dataTable.StringTableToIEnumerableByDiplay<TerminationData>();
        });

        model.Status.Busy("正在匹配端接数据……");
       
        Result = await DataConverter.MatchCableData(data1, data2, major1.CableSystem, major2.CableSystem);
        Failist1 = [.. Result.FailList1];
        Failist2 = [.. Result.FailList2];
        SuccessList = [.. Result.SuccessList];
        SuccessDataCount = Result.SuccessList.Count;
        model.Status.Reset();
    }

    [RelayCommand]
    private void MatchSelected() {
        _ = Failist1 ?? throw new("没有需要匹配的数据");
        _ = cable.Major1 ?? throw new("没有需要匹配的数据");
        _ = cable.Major2 ?? throw new("没有需要匹配的数据");
        var data1 = Failist1.SingleOrDefault(x => x.IsChecked) ?? throw new("请在两个列表中分别选择一个要匹配的数据");
        var data2 = Failist2.SingleOrDefault(x => x.IsChecked) ?? throw new("请在两个列表中分别选择一个要匹配的数据");

        model.Status.Busy("正在生成电缆数据……");
        var cableData = DataConverter.MatchCableData(data1.Data, data2.Data);
        SuccessList.Add(cableData);
        Failist1.Remove(data1);
        Failist2.Remove(data2);
        SuccessDataCount = SuccessList.Count;

        model.Status.Reset();
    }

}