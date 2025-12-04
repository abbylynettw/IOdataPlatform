namespace IODataPlatform.Utilities;

/// <summary>
/// 字符串扩展方法集合
/// 提供针对字符串的常用操作扩展，增强字符串处理能力
/// 包含自定义填充、格式化等实用功能
/// </summary>
public static class StringExtensions
{
	/// <summary>
	/// 使用自定义字符在字符串左侧进行填充
	/// 如果输入字符串长度已经达到或超过目标宽度，则直接返回原字符串
	/// </summary>
	/// <param name="input">要填充的原始字符串</param>
	/// <param name="totalWidth">目标总宽度</param>
	/// <param name="paddingChar">用于填充的字符</param>
	/// <returns>返回填充后的字符串</returns>
	public static string PadLeftCustom(this string input, int totalWidth, char paddingChar)
	{
		if (input.Length >= totalWidth)
		{
			return input;
		}
		return new string(paddingChar, totalWidth - input.Length) + input;
	}
}
