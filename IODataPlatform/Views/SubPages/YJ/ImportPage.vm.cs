using System.Data;

using IODataPlatform.Models;
using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using IODataPlatform.Views.Pages;

using LYSoft.Libs.ServiceInterfaces;

namespace IODataPlatform.Views.SubPages.YJ;

public partial class ImportViewModel(GlobalModel model, DepYJViewModel yj, ExcelService excel, IPickerService picker, INavigationService navigation) : ObservableObject, INavigationAware {

    public void OnNavigatedFrom() { }

    public void OnNavigatedTo() {
        //_ = yj.config_project ?? throw new("开发人员注意");
        //_ = yj.config_project_subProject ?? throw new("开发人员注意");
        //_ = yj.AllData ?? throw new("开发人员注意");
    }

    [RelayCommand]
    private async Task ImportFile() {
        if (picker.OpenFile("Excel 文件(*.xls; *.xlsx)|*.xls; *.xlsx") is not string file) { return; }
        model.Status.Busy("正在获取数据……");

        var result = await Task.Run(() => excel.GetWorkbook(file).Worksheets.ExceptBy(["目录", "说明"], x => x.Name).AsParallel()
                .Select(x => x.Cells.ExportDataTableAsString(2, 1, x.Cells.MaxDataRow - 1, 9))
                .SelectMany(x => x.Rows.Cast<DataRow>()).Where(x => $"{x[6]}" != "-")
                .Select(x => new 内部接线清单() { 位置 = $"{x[0]}", 典回类型 = $"{x[6]}", 信号名称 = $"{x[2]}",
                    安全等级 = $"{x[4]}", 安全列 = $"{x[5]}", 端子组名 = $"{x[7]}", 端子号 = $"{x[8]}", })
                .ToList());
 
        model.Status.Reset();
    }

    //private void Save() {
    //    //只添加新增典回
    //    List<string> loops = DBHelper.yJS_TypicalLoopContext.GetAll().Select(t => t.LoopType).Distinct().ToList();
    //    //导入到典回库中 往表里插入数据
    //    List<YJS_TypicalLoop> yJS_TypicalLoops = new List<YJS_TypicalLoop>();
    //    foreach (var item in TypicalLoopInfos.Where(t => !loops.Contains(t.典回类型))) {
    //        yJS_TypicalLoops.Add(new YJS_TypicalLoop {
    //            LoopType = item.典回类型,
    //            ExtensionCode = "A",
    //            SignalLocation = item.位置,
    //            DeviceType = "A",
    //            FunctionDescription = "A",
    //            SignalName = item.信号名称,
    //            SafetyLevel = item.安全等级,
    //            SafetyColumn = item.安全列,
    //            TerminalGroupName = item.端子组名
    //        }); ;

    //    }
    //    var result = DBHelper.yJS_TypicalLoopContext.BatchInsert(yJS_TypicalLoops);
    //    if (result.IsSuccess) {
    //        MessageBox.Show($"插入成功{result.AffectCount}条数据");
    //    } else {
    //        MessageBox.Show($"插入失败" + result.ResultMsg);
    //    }
    //    if (DialogHost.IsDialogOpen(DialogHostName)) {
    //        DialogParameters param = new DialogParameters();
    //        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
    //    }
    //}

    //public void CreateTypicalLoop() {
    //    if (string.IsNullOrEmpty(FileName2)) return;
    //    List<内部接线清单> typicalLoopInfos = new DataHelper().GetQingdanData(FileName2);
    //    List<内部接线清单> template = new List<内部接线清单>();
    //    //先根据设备名称分组，再根据典回分组
    //    var grLoop = typicalLoopInfos.GroupBy(g => g.典回类型);
    //    //根据信号名称分组的结果
    //    foreach (var loop in grLoop) {
    //        var key = loop.Key;//一种典回               
    //        var grXinhao = loop.ToList().GroupBy(g => g.信号名称);
    //        //一种典回中的一种信号
    //        var first = grXinhao.FirstOrDefault().ToList();
    //        template.AddRange(first);
    //    }
    //    TypicalLoopInfos = DataConvert.Convert(template);
    //    RowCount = TypicalLoopInfos.Count;
    //}

    ///// <summary>
    ///// 获取典回模板数据
    ///// </summary>
    ///// <returns></returns>
    //public List<典回> GetLoopData() {
    //    //读取模板
    //    List<典回> loopDetails = new List<典回>();

    //    List<YJS_TypicalLoop> typicalLoopInfos = DBHelper.Instance.yJS_TypicalLoopContext.GetAll();
    //    var gr = typicalLoopInfos.GroupBy(g => g.LoopType);
    //    foreach (var g in gr) {
    //        典回 典回 = new 典回();
    //        典回.典回类型 = g.Key;
    //        典回.loopRows = g.ToList();
    //        loopDetails.Add(典回);
    //    }
    //    return loopDetails;
    //}

    [RelayCommand]
    private void ViewData(DifferentObject<string> obj) {
        
    }

    [RelayCommand]
    private async Task Confirm() {
        model.Status.Success("正在提交数据……");

        model.Status.Success("导入成功");
        navigation.GoBack();
    }

}
