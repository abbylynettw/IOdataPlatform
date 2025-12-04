namespace IODataPlatform.Utilities;

/// <summary>
/// 属性差异信息类
/// 记录单个属性的变更详情，包括属性名称、旧值和新值
/// 支持DisplayAttribute显示名称，提供用户友好的差异展示
/// </summary>
public class DifferentProperty
{
	/// <summary>属性的显示名称（来自DisplayAttribute或属性名）</summary>
	public required string PropName { get; set; }

	/// <summary>属性的原始值（变更前的值）</summary>
	public required object? OldValue { get; set; }

	/// <summary>属性的新值（变更后的值）</summary>
	public required object? NewValue { get; set; }
}
