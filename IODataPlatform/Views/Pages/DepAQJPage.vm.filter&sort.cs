using IODataPlatform.Models.ExcelModels;
using IODataPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Views.Pages
{
    partial class DepAQJViewModel
    {
       
        [ObservableProperty]
        private ObservableCollection<IoFullData>? displayPoints;//要显示的点

        [ObservableProperty]
        private int displayPointCount;//显示点数量       

        [ObservableProperty]
        private int allDataCount;//所有点数量

        [ObservableProperty]
        private bool isAscending = true; //升序？

        private bool isRefreshingOptions = false; //是否全部刷新？

        partial void OnDisplayPointsChanged(ObservableCollection<IoFullData>? value)
        {
            DisplayPointCount = value?.Count ?? 0; //过滤后的点的数量
        }

        partial void OnAllDataChanged(ObservableCollection<IoFullData>? value)
        {         
            AllDataCount = value?.Count ?? 0;           
            DisplayPoints = null;           
            RefreshFilterOptions();
            FilterAndSort();
        }


        public ImmutableList<CommonFilter> Filters { get; } = [
            new("机柜号"),new("机箱号"),new("槽位号"),new("通道号"),new ("板卡类型"),new ("IO类型")
    ];

        [RelayCommand]
        private async Task ClearAllFilterOptions()
        {
            if (!await message.ConfirmAsync("确认重置全部筛选条件")) { return; }
            isRefreshingOptions = true;
            Filters.AllDo(x => x.Option = "全部");
            isRefreshingOptions = false;
            FilterAndSort();
        }

        partial void OnIsAscendingChanged(bool value)
        {
            FilterAndSort();
        }

        private void RefreshFilterOptions()
        {
            isRefreshingOptions = true;

            if (AllData == null)
            {
                Filters.ForEach(x => x.ClearAll());
            }
            else
            {              
                var filterDic = Filters.ToDictionary(x => x.Title);
                filterDic["机柜号"].SetOptions(AllData.Select(x => x.CabinetNumber));             
                filterDic["机箱号"].SetOptions(AllData.Select(x => x.Cage.ToString()));
                filterDic["槽位号"].SetOptions(AllData.Select(x => x.Slot.ToString()));
                filterDic["通道号"].SetOptions(AllData.Select(x => x.Channel.ToString()));
                filterDic["板卡类型"].SetOptions(AllData.Select(x => x.CardType));
                filterDic["IO类型"].SetOptions(AllData.Select(x => x.IoType));
            }

            isRefreshingOptions = false;
        }

        [RelayCommand]
        private void FilterAndSort()
        {
            if (isRefreshingOptions) { return; }
            if (AllData is null) { return; }

            var filterDic = Filters.ToDictionary(x => x.Title);

            var data = AllData
                .WhereIf(x => x.CabinetNumber == filterDic["机柜号"].Option, filterDic["机柜号"].Option != "全部")
                .WhereIf(x => x.IoType == filterDic["机箱号"].Option, filterDic["机箱号"].Option != "全部")
                .WhereIf(x => x.IoType == filterDic["槽位号"].Option, filterDic["槽位号"].Option != "全部")
                .WhereIf(x => x.IoType == filterDic["通道号"].Option, filterDic["通道号"].Option != "全部")
                .WhereIf(x => x.IoType == filterDic["板卡类型"].Option, filterDic["板卡类型"].Option != "全部")
                .WhereIf(x => x.IoType == filterDic["IO类型"].Option, filterDic["IO类型"].Option != "全部");

            DisplayPoints = [..data];

        }
    }
}
