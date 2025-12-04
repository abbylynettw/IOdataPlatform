using System.Collections.Generic;

namespace IODataPlatform.Services;

/// <summary>
/// 配置验证结果类
/// 包含验证状态和错误信息
/// </summary>
public class ValidationResult
{
	/// <summary>
	/// 验证是否通过
	/// </summary>
	public bool IsValid { get; set; }

	/// <summary>
	/// 验证错误信息列表
	/// </summary>
	public List<string> Errors { get; set; } = new List<string>();
}
