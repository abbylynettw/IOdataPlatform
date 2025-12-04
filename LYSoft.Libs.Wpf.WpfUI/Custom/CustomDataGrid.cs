﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using DataGrid = Wpf.Ui.Controls.DataGrid;


namespace LYSoft.Libs.Wpf.WpfUI.Custom
{
    /// <summary>
    /// 自定义数据网格控件
    /// 继承自WpfUI的DataGrid控件，提供增强的列自动生成功能
    /// 支持通过DisplayAttribute特性自动设置列标题和可见性，简化数据绑定时的列配置工作
    /// 适用于需要动态生成列并自定义显示名称的数据展示场景
    /// </summary>
    public class CustomDataGrid : DataGrid
    {
        /// <summary>
        /// 初始化CustomDataGrid的新实例
        /// 构造函数中订阅AutoGeneratingColumn事件，用于在自动生成列时进行自定义处理
        /// 自动处理DisplayAttribute特性以设置列标题和可见性
        /// </summary>
        public CustomDataGrid()
        {
            this.AutoGeneratingColumn += CustomDataGrid_AutoGeneratingColumn;
        }

        /// <summary>
        /// 处理数据网格自动生成列的事件
        /// 当DataGrid自动生成列时触发，根据数据源属性的DisplayAttribute特性自动设置列标题和可见性
        /// 如果属性标记了DisplayAttribute.Name，则使用该名称作为列标题
        /// 如果属性的AutoGenerateField为false，则隐藏该列
        /// </summary>
        /// <param name="sender">触发事件的DataGrid对象</param>
        /// <param name="e">包含列生成信息的事件参数</param>
        /// <remarks>
        /// 通过反射获取数据源类型的属性信息，检查DisplayAttribute特性的配置
        /// 支持动态列标题设置和列可见性控制，提高数据展示的灵活性
        /// </remarks>
        private void CustomDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;
            var itemType = dataGrid.ItemsSource.GetType().GetGenericArguments()[0];

            // 从数据源类型中获取对应的属性
            var property = itemType.GetProperty(e.PropertyName);
            if (property != null)
            {
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    e.Column.Header = displayAttribute.Name;
                    if (!displayAttribute.GetAutoGenerateField().GetValueOrDefault(true))
                    {
                        e.Column.Visibility = System.Windows.Visibility.Hidden;
                    }
                }
            }
        }
    }
}
