using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;

namespace IODataPlatform.Utilities;

/// <summary>
/// 正则表达式工具类
/// 采用单例模式设计，提供常用的正则表达式验证和字符串处理功能
/// 支持中文字符、数字、英文字母、电话、邮箱、IP地址等多种格式验证
/// 广泛用于数据校验、格式化和正则匹配场景
/// </summary>
public class RegexDao
{
	/// <summary>单例对象实例</summary>
	private static RegexDao? instance;

	/// <summary>私有构造函数，防止外部实例化</summary>
	private RegexDao()
	{
	}

	/// <summary>
	/// 获取RegexDao的单例对象
	/// 实现线程安全的单例模式，确保全局唯一性
	/// </summary>
	/// <returns>返回RegexDao的单例对象</returns>
	public static RegexDao GetInstance()
	{
		return instance ?? (instance = new RegexDao());
	}

	/// <summary>
	/// 判断输入的字符串只包含汉字
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static bool IsChineseCh(string input)
	{
		return IsMatch("^[\\u4e00-\\u9fa5]+$", input);
	}

	/// <summary>
	/// 匹配3位或4位区号的电话号码，其中区号可以用小括号括起来，
	/// 也可以不用，区号与本地号间可以用连字号或空格间隔，
	/// 也可以没有间隔
	/// \(0\d{2}\)[- ]?\d{8}|0\d{2}[- ]?\d{8}|\(0\d{3}\)[- ]?\d{7}|0\d{3}[- ]?\d{7}
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static bool IsPhone(string input)
	{
		string pattern = "^\\(0\\d{2}\\)[- ]?\\d{8}$|^0\\d{2}[- ]?\\d{8}$|^\\(0\\d{3}\\)[- ]?\\d{7}$|^0\\d{3}[- ]?\\d{7}$";
		return IsMatch(pattern, input);
	}

	/// <summary>
	/// 判断输入的字符串是否是一个合法的手机号
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static bool IsMobilePhone(string input)
	{
		return IsMatch("^13\\\\d{9}$", input);
	}

	/// <summary>
	/// 判断输入的字符串只包含数字
	/// 可以匹配整数和浮点数
	/// ^-?\d+$|^(-?\d+)(\.\d+)?$
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static bool IsNumber(string input)
	{
		string pattern = "^-?\\d+$|^(-?\\d+)(\\.\\d+)?$";
		return IsMatch(pattern, input);
	}

	/// <summary>
	/// 匹配非负整数
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static bool IsNotNagtive(string input)
	{
		return IsMatch("^\\d+$", input);
	}

	/// <summary>
	/// 匹配正整数
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static bool IsUint(string input)
	{
		return IsMatch("^[0-9]*[1-9][0-9]*$", input);
	}

	/// <summary>
	/// 判断输入的字符串字包含英文字母
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static bool IsEnglisCh(string input)
	{
		return IsMatch("^[A-Za-z]+$", input);
	}

	/// <summary>
	/// 判断输入的字符串是否是一个合法的Email地址
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static bool IsEmail(string input)
	{
		string pattern = "^([\\w-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([\\w-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)$";
		return IsMatch(pattern, input);
	}

	/// <summary>
	/// 判断输入的字符串是否只包含数字和英文字母
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static bool IsNumAndEnCh(string input)
	{
		return IsMatch("^[A-Za-z0-9]+$", input);
	}

	/// <summary>
	/// 判断输入的字符串是否是一个超链接
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static bool IsURL(string input)
	{
		string pattern = "^[a-zA-Z]+://(\\w+(-\\w+)*)(\\.(\\w+(-\\w+)*))*(\\?\\S*)?$";
		return IsMatch(pattern, input);
	}

	public static float GetDigitsAsFloat(string str)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		foreach (char c in str)
		{
			if (char.IsDigit(c))
			{
				stringBuilder.Append(c);
			}
			else if (c == '.' && !flag)
			{
				stringBuilder.Append(c);
				flag = true;
			}
		}
		return float.Parse(stringBuilder.ToString(), CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// 判断输入的字符串是否是表示一个IP地址
	/// </summary>
	/// <param name="input">被比较的字符串</param>
	/// <returns>是IP地址则为True</returns>
	public static bool IsIPv4(string input)
	{
		string[] array = input.Split('.');
		for (int i = 0; i < array.Length; i++)
		{
			if (!IsMatch("^\\d+$", array[i]))
			{
				return false;
			}
			if (Convert.ToUInt16(array[i]) > 255)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// 判断输入的字符串是否是合法的IPV6 地址
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static bool IsIPV6(string input)
	{
		string[] array = input.Split(':');
		if (array.Length > 8)
		{
			return false;
		}
		int stringCount = GetStringCount(input, "::");
		if (stringCount > 1)
		{
			return false;
		}
		if (stringCount == 0)
		{
			return IsMatch("^([\\da-f]{1,4}:){7}[\\da-f]{1,4}$", input);
		}
		return IsMatch("^([\\da-f]{1,4}:){0,5}::([\\da-f]{1,4}:){0,5}[\\da-f]{1,4}$", input);
	}

	/// <summary>
	/// 计算字符串的字符长度，一个汉字字符将被计算为两个字符
	/// </summary>
	/// <param name="input">需要计算的字符串</param>
	/// <returns>返回字符串的长度</returns>
	public static int GetCount(string input)
	{
		return ChineseRegex().Replace(input, "aa").Length;
	}

	/// <summary>
	/// 调用Regex中IsMatch函数实现一般的正则表达式匹配
	/// </summary>
	/// <param name="pattern">要匹配的正则表达式模式。</param>
	/// <param name="input">要搜索匹配项的字符串</param>
	/// <returns>如果正则表达式找到匹配项，则为 true；否则，为 false。</returns>
	public static bool IsMatch(string pattern, string input)
	{
		if (input == null || input == "")
		{
			return false;
		}
		Regex regex = new Regex(pattern);
		return regex.IsMatch(input);
	}

	/// <summary>
	/// 从输入字符串中的第一个字符开始，用替换字符串替换指定的正则表达式模式的所有匹配项。
	/// </summary>
	/// <param name="pattern">模式字符串</param>
	/// <param name="input">输入字符串</param>
	/// <param name="replacement">用于替换的字符串</param>
	/// <returns>返回被替换后的结果</returns>
	public static string Replace(string pattern, string input, string replacement)
	{
		Regex regex = new Regex(pattern);
		return regex.Replace(input, replacement);
	}

	/// <summary>
	/// 在由正则表达式模式定义的位置拆分输入字符串。
	/// </summary>
	/// <param name="pattern">模式字符串</param>
	/// <param name="input">输入字符串</param>
	/// <returns></returns>
	public static string[] Split(string pattern, string input)
	{
		Regex regex = new Regex(pattern);
		return regex.Split(input);
	}

	/// <summary>
	/// 判断字符串compare 在 input字符串中出现的次数
	/// </summary>
	/// <param name="input">源字符串</param>
	/// <param name="compare">用于比较的字符串</param>
	/// <returns>字符串compare 在 input字符串中出现的次数</returns>
	private static int GetStringCount(string input, string compare)
	{
		int num = input.IndexOf(compare);
		if (num != -1)
		{
			int num2 = num + compare.Length;
			return 1 + GetStringCount(input.Substring(num2, input.Length - num2), compare);
		}
		return 0;
	}

	/// <remarks>
	/// Pattern:<br />
	/// <code>[\\u4e00-\\u9fa5/g]</code><br />
	/// Explanation:<br />
	/// <code>
	/// ○ Match a character in the set [/g\u4E00-\u9FA5].<br />
	/// </code>
	/// </remarks>
	[GeneratedRegex("[\\u4e00-\\u9fa5/g]")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.12.47513")]
	private static Regex ChineseRegex()
	{
		return _003CRegexGenerator_g_003EF762D5FD329DBBD70E19FA14A99B374DF9975376AED0BDD1C4EEE6BE27FD4145C__ChineseRegex_0.Instance;
	}
}
