﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Utilities
{
    /// <summary>
    /// 显示信息容器类
    /// 存储类型的显示名称和字段显示名称的映射关系
    /// 用于支持DisplayAttribute特性的动态获取和缓存
    /// </summary>
    public class DisplayInfo
    {
        /// <summary>类的显示名称，来自DisplayAttribute或默认类名</summary>
        public string ClassName { get; set; }
        
        /// <summary>字段名到显示名称的映射字典</summary>
        public Dictionary<string, string> FieldDisplayNames { get; set; }
    }

    /// <summary>
    /// DisplayAttribute特性工具类
    /// 提供获取类型和属性的DisplayAttribute信息的工具方法
    /// 支持通过反射动态获取显示名称，用于界面显示和数据映射
    /// </summary>
    public static class DisplayAttributeHelper
    {
        /// <summary>
        /// 获取指定类型的显示信息
        /// 包括类的显示名称和所有属性的显示名称映射
        /// </summary>
        /// <typeparam name="T">要获取显示信息的类型</typeparam>
        /// <returns>返回包含类名和字段显示名称的DisplayInfo对象</returns>
        public static DisplayInfo GetDisplayInfo<T>()
        {
            Type type = typeof(T);
            DisplayInfo displayInfo = new DisplayInfo();
            displayInfo.FieldDisplayNames = new Dictionary<string, string>();

            // 获取类的 Display 特性，如果没有则使用类名
            DisplayAttribute classDisplayAttribute = type.GetCustomAttribute<DisplayAttribute>();
            displayInfo.ClassName = classDisplayAttribute?.Name ?? type.Name;

            // 遍历类的所有属性，获取各自的Display特性
            foreach (PropertyInfo property in type.GetProperties())
            {
                // 获取每个属性的 Display 特性
                DisplayAttribute displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    displayInfo.FieldDisplayNames.Add(property.Name, displayAttribute.Name);
                }
            }

            return displayInfo;
        }
    }
}
