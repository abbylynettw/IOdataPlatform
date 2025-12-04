using System.Threading;

namespace IODataPlatform.Services;

/// <summary>
/// 缓存统计信息类
/// 记录缓存的性能指标和使用情况
/// </summary>
public class CacheStatistics
{
	private long _hits;

	private long _misses;

	private long _sets;

	private long _removes;

	private long _errors;

	/// <summary>缓存命中次数</summary>
	public long Hits => _hits;

	/// <summary>缓存未命中次数</summary>
	public long Misses => _misses;

	/// <summary>缓存设置次数</summary>
	public long Sets => _sets;

	/// <summary>缓存移除次数</summary>
	public long Removes => _removes;

	/// <summary>错误次数</summary>
	public long Errors => _errors;

	/// <summary>当前缓存项数量</summary>
	public int CurrentItemCount { get; set; }

	/// <summary>缓存命中率</summary>
	public double HitRatio
	{
		get
		{
			if (_hits + _misses <= 0)
			{
				return 0.0;
			}
			return (double)_hits / (double)(_hits + _misses);
		}
	}

	/// <summary>记录缓存命中</summary>
	internal void RecordHit()
	{
		Interlocked.Increment(ref _hits);
	}

	/// <summary>记录缓存未命中</summary>
	internal void RecordMiss()
	{
		Interlocked.Increment(ref _misses);
	}

	/// <summary>记录缓存设置</summary>
	internal void RecordSet()
	{
		Interlocked.Increment(ref _sets);
	}

	/// <summary>记录缓存移除</summary>
	internal void RecordRemove()
	{
		Interlocked.Increment(ref _removes);
	}

	/// <summary>记录错误</summary>
	internal void RecordError()
	{
		Interlocked.Increment(ref _errors);
	}

	/// <summary>记录清空操作</summary>
	internal void RecordClear(int count)
	{
		Interlocked.Add(ref _removes, count);
	}
}
