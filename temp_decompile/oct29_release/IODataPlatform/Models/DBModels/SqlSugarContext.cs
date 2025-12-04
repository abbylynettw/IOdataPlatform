using System;
using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>
/// SqlSugar ORM数据库上下文类
/// 管理多个数据库连接，支持SQL Server和Access数据库
/// 为应用程序提供统一的数据访问入口，实现连接池管理和资源优化
/// </summary>
public class SqlSugarContext : IDisposable
{
	private readonly string _connectionString;

	private readonly string _connectionString2;

	private readonly Lazy<SqlSugarClient> _db;

	private readonly Lazy<SqlSugarClient> _db2;

	private bool _disposed;

	/// <summary>
	/// 主数据库连接（SQL Server）
	/// 用于存储配置信息、用户数据、公式等核心业务数据
	/// </summary>
	public SqlSugarClient Db => _db.Value;

	/// <summary>
	/// 辅助数据库连接（Access）
	/// 用于存储临时数据或与旧系统的兼容
	/// </summary>
	public SqlSugarClient Db2 => _db2.Value;

	/// <summary>
	/// 构造函数，初始化数据库连接配置
	/// </summary>
	/// <param name="connectionString">主数据库连接字符串（SQL Server）</param>
	/// <param name="connectionString2">辅助数据库连接字符串（Access）</param>
	public SqlSugarContext(string connectionString, string connectionString2)
	{
		_connectionString = connectionString;
		_connectionString2 = connectionString2;
		_db = new Lazy<SqlSugarClient>(() => new SqlSugarClient(new ConnectionConfig
		{
			ConnectionString = _connectionString,
			DbType = DbType.SqlServer,
			IsAutoCloseConnection = true
		}));
		_db2 = new Lazy<SqlSugarClient>(() => new SqlSugarClient(new ConnectionConfig
		{
			ConnectionString = _connectionString2,
			DbType = DbType.Access,
			IsAutoCloseConnection = true
		}));
	}

	/// <summary>
	/// 释放数据库连接资源
	/// </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// 受保护的释放方法，确保资源正确释放
	/// </summary>
	/// <param name="disposing">是否正在释放托管资源</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			if (_db.IsValueCreated)
			{
				_db.Value?.Dispose();
			}
			if (_db2.IsValueCreated)
			{
				_db2.Value?.Dispose();
			}
			_disposed = true;
		}
	}
}
