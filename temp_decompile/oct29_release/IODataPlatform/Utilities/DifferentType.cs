namespace IODataPlatform.Utilities;

/// <summary>
/// 数据变更类型枚举
/// 定义数据对比中支持的三种基本变更类型
/// 使用位标志实现，支持组合操作和筛选
/// </summary>
public enum DifferentType
{
	/// <summary>新增的数据记录（在新数据中存在，但在旧数据中不存在）</summary>
	新增 = 1,
	/// <summary>移除的数据记录（在旧数据中存在，但在新数据中不存在）</summary>
	移除 = 2,
	/// <summary>覆盖的数据记录（在两个数据集合中都存在，但属性值发生了变化）</summary>
	覆盖 = 4
}
