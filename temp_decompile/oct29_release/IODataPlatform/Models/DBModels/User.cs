using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>
/// 用户数据库实体类
/// 对应数据库中的用户表，存储用户的基本信息、部门和权限
/// 使用SqlSugar ORM进行数据库映射
/// </summary>
public class User
{
	/// <summary>用户唯一标识符，主键，自增长</summary>
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	public int Id { get; set; }

	/// <summary>用户姓名</summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>用户登录账号</summary>
	public string Account { get; set; } = string.Empty;

	/// <summary>用户所属部门，支持多部门归属</summary>
	public Department Department { get; set; }

	/// <summary>用户权限，支持多权限组合</summary>
	public UserPermission Permission { get; set; }
}
