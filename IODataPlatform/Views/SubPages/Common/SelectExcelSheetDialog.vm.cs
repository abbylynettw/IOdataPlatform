using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using LYSoft.Libs.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Views.SubPages.Common
{

    public partial class SelectExcelSheetDialogViewModel(SqlSugarContext context, GlobalModel model, IMessageService message,IPickerService picker,ExcelService excel) : ObservableObject
    {

        [ObservableProperty]
        private string selectFilePath = "请选择Excel文件";

        [ObservableProperty]
        private List<string>? sheetNames;

        [ObservableProperty]
        private string selectedSheetName;

        [ObservableProperty]
        private string currentSystemInfo = "当前系统：未选择项目";

        [ObservableProperty]
        private bool isLoadingSheets = false;

        [ObservableProperty]
        private string sheetCountText = "";

        /// <summary>
        /// 显示空提示：未选择文件且不在加载中
        /// </summary>
        public bool ShowEmptyHint => !IsLoadingSheets && (SheetNames == null || !SheetNames.Any());

        /// <summary>
        /// 显示 Sheet 列表：有数据且不在加载中
        /// </summary>
        public bool ShowSheetList => !IsLoadingSheets && SheetNames != null && SheetNames.Any();

        /// <summary>
        /// 设置当前系统信息
        /// </summary>
        /// <param name="projectName">项目名称</param>
        /// <param name="controlSystem">控制系统</param>
        public void SetCurrentSystemInfo(ControlSystem controlSystem)
        {
            CurrentSystemInfo = $"当前系统: {controlSystem}，请按照数据资产中心的配置表中当前系统相应的字段来导入";
        }

        [RelayCommand]
        private async Task SelectFile()
        {
            if (picker.OpenFile("请选择 Excel 文件|*.xls;*.xlsx;*.xlsm") is not string path) 
                return;
            
            SelectFilePath = path;
            
            // 显示加载状态
            IsLoadingSheets = true;
            SheetNames = null;
            SelectedSheetName = string.Empty;
            SheetCountText = "";
            
            // 通知 UI 更新
            OnPropertyChanged(nameof(ShowEmptyHint));
            OnPropertyChanged(nameof(ShowSheetList));

            try
            {
                // 获取全部 sheetNames
                SheetNames = await excel.GetSheetNames(SelectFilePath);
                
                // 自动选中第一个 Sheet
                if (SheetNames != null && SheetNames.Any())
                {
                    SelectedSheetName = SheetNames.First();
                    SheetCountText = $"(共 {SheetNames.Count} 个)";
                }
                else
                {
                    SheetCountText = "(暂无工作表)";
                }
            }
            catch (Exception ex)
            {
                await message.AlertAsync($"读取 Excel 文件失败：{ex.Message}");
                SheetCountText = "(读取失败)";
            }
            finally
            {
                // 隐藏加载状态
                IsLoadingSheets = false;
                
                // 通知 UI 更新
                OnPropertyChanged(nameof(ShowEmptyHint));
                OnPropertyChanged(nameof(ShowSheetList));
            }
        }
    }
}
