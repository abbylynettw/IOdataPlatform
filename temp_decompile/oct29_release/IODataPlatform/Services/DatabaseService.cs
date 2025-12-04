using System.Collections.Generic;
using IODataPlatform.Models.DBModels;

namespace IODataPlatform.Services;

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
		return (from it in context.Db.Queryable<config_controlSystem_mapping>()
			where it.LhOld != null && it.LhOld != ""
			select it.StdField).ToList();
	}

	/// <summary>
	/// 获取龙热系统的映射字段列表
	/// 查询config_controlSystem_mapping表中有效的龙热旧版本字段映射
	/// </summary>
	/// <param name="context">SqlSugar数据库上下文</param>
	/// <returns>返回龙热系统的标准字段名列表</returns>
	public List<string> GetLongLqFields(SqlSugarContext context)
	{
		return (from it in context.Db.Queryable<config_controlSystem_mapping>()
			where it.LqOld != null && it.LqOld != ""
			select it.StdField).ToList();
	}

	/// <summary>
	/// 获取安全级MN系统的映射字段列表
	/// 查询config_controlSystem_mapping表中有效的安全级MN旧版本字段映射
	/// </summary>
	/// <param name="context">SqlSugar数据库上下文</param>
	/// <returns>返回安全级MN系统的标准字段名列表</returns>
	public List<string> GetAQJMNFields(SqlSugarContext context)
	{
		return (from it in context.Db.Queryable<config_controlSystem_mapping>()
			where it.AQJMNOld != null && it.AQJMNOld != ""
			select it.StdField).ToList();
	}
}
