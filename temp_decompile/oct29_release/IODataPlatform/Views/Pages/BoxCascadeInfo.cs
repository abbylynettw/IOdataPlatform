namespace IODataPlatform.Views.Pages;

/// <summary>
/// 箱子串接信息
/// </summary>
public class BoxCascadeInfo
{
	/// <summary>
	/// 左边是否有箱子串接到当前箱子
	/// </summary>
	public bool HasLeftCascade { get; set; }

	/// <summary>
	/// 左边箱子号
	/// </summary>
	public string LeftBox { get; set; } = string.Empty;

	/// <summary>
	/// 右边是否有箱子被当前箱子串接
	/// </summary>
	public bool HasRightCascade { get; set; }

	/// <summary>
	/// 右边箱子号
	/// </summary>
	public string RightBox { get; set; } = string.Empty;
}
