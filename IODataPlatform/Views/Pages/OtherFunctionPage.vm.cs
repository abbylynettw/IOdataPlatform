﻿using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Services;
using IODataPlatform.Views.SubPages.OtherFunction;
using IODataPlatform.Views.SubPages.Paper;
using IODataPlatform.Views.SubPages.XT2;
using LYSoft.Libs.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IODataPlatform.Views.SubPages.Common;

namespace IODataPlatform.Views.Pages
{
    /// <summary>
    /// 其他功能页面视图模型类
    /// 提供系统的扩展功能和工具集合，包括网表生成、数据处理等专业工具
    /// 作为功能扩展的统一入口，支持各种辅助性业务操作和数据处理任务
    /// 实现INavigationAware接口以支持页面导航和状态管理
    /// </summary>
    public partial class OtherFunctionViewModel(SqlSugarContext context, INavigationService navigation,
     GlobalModel model,IPickerService picker,ExcelService excel) : ObservableObject, INavigationAware
    {
        /// <summary>
        /// 页面导航离开时触发
        /// 当前实现为空，预留用于后续状态清理或数据保存操作
        /// </summary>
        public void OnNavigatedFrom()
        {

        }

        /// <summary>
        /// 页面导航到此页面时触发
        /// 当前实现为空，预留用于后续初始化或数据加载操作
        /// </summary>
        public async void OnNavigatedTo()
        {
            
        }

        /// <summary>
        /// 生成网表命令
        /// 导航到Excel数据提取转网表页面，用于将Excel格式的工程数据转换为标准网表格式
        /// 广泛用于电气设计和仿真软件的数据交换，支持CAD工具链的数据传输
        /// </summary>
        [RelayCommand]
        private void GenerateNetList()
        {
            navigation.NavigateWithHierarchy(typeof(ExtractExcelToNetListPage));
        }

        /// <summary>
        /// 通用数据对比命令
        /// 导航到通用数据对比页面，用于对比任意两个Excel数据文件的差异
        /// 支持指定主键字段进行精确对比，提供新增、删除、修改三种变更类型的识别
        /// </summary>
        [RelayCommand]
        private void GenericDataComparison()
        {
            navigation.NavigateWithHierarchy(typeof(GenericDataComparisonPage));
        }
    }
}