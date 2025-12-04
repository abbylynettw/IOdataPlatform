﻿using IODataPlatform.Models.DBModels;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Services
{
    /// <summary>
    /// 数据库服务类
    /// 封装数据库操作相关的业务逻辑，主要针对控制系统映射配置的查询
    /// 提供不同控制系统（龙和、龙热、安全级MN）的字段映射获取功能
    /// </summary>
    public class DatabaseService
    {

        /// <summary>
        /// 获取龙和系统的映射字段列表
        /// 查询config_controlSystem_mapping表中有效的龙和旧版本字段映射
        /// </summary>
        /// <param name="context">SqlSugar数据库上下文</param>
        /// <returns>返回龙和系统的标准字段名列表</returns>
        public List<string> GetLongHeFields(SqlSugarContext context)
        {
            var columnsInit = context.Db.Queryable<config_controlSystem_mapping>()
                                .Where(it => it.LhOld != null && it.LhOld != "")
                                .Select(it => it.StdField)
                                .ToList();

            return columnsInit;
        }
        /// <summary>
        /// 获取龙热系统的映射字段列表
        /// 查询config_controlSystem_mapping表中有效的龙热旧版本字段映射
        /// </summary>
        /// <param name="context">SqlSugar数据库上下文</param>
        /// <returns>返回龙热系统的标准字段名列表</returns>
        public List<string> GetLongLqFields(SqlSugarContext context)
        {
            var columnsInit = context.Db.Queryable<config_controlSystem_mapping>()
                                .Where(it => it.LqOld != null && it.LqOld != "")
                                .Select(it => it.StdField)
                                .ToList();

            return columnsInit;
        }

        /// <summary>
        /// 获取安全级MN系统的映射字段列表
        /// 查询config_controlSystem_mapping表中有效的安全级MN旧版本字段映射
        /// </summary>
        /// <param name="context">SqlSugar数据库上下文</param>
        /// <returns>返回安全级MN系统的标准字段名列表</returns>
        public List<string> GetAQJMNFields(SqlSugarContext context)
        {
            var columnsInit = context.Db.Queryable<config_controlSystem_mapping>()
                                .Where(it => it.AQJMNOld != null && it.AQJMNOld != "")
                                .Select(it => it.StdField)
                                .ToList();
            return columnsInit;
        }
    }
}
