﻿﻿using System.Text.RegularExpressions;

namespace LYSoft.Libs;

/// <summary>
/// 常规文本校验服务类
/// 提供常用文本格式的校验功能，包括密码、邮箱、电话号码、姓名等
/// 使用正则表达式进行模式匹配，提供高效的文本格式验证
/// 支持中文姓名、中国手机号码等本地化校验规则
/// </summary>
public partial class CommonTextCheckService {

    /// <summary>
    /// 校验是否为安全强度的密码
    /// 密码必须满足：长度不少于12位，同时包含数字、大写字母、小写字母、特殊字符中的至少三种
    /// </summary>
    /// <param name="text">要校验的密码文本</param>
    /// <returns>如果是安全密码返回true，否则返回false</returns>
    public bool IsPassword(string text) {
        if (string.IsNullOrEmpty(text)) { return false; }
        if (text.Length < 12) { return false; }
        var regexList = new[] { PasswordConditionRegex1(), PasswordConditionRegex2(), PasswordConditionRegex3(), PasswordConditionRegex4() };
        if (regexList.Count(x => x.IsMatch(text)) < 3) { return false; }
        return true;
    }

    /// <summary>
    /// 校验是否为正确的员工编码
    /// 员工编码必须为6-10位的纯数字
    /// </summary>
    /// <param name="text">要校验的员工编码文本</param>
    /// <returns>如果是正确的员工编码返回true，否则返回false</returns>
    public bool IsErp(string text) {
        return ErpRegex().IsMatch(text);
    }

    /// <summary>
    /// 校验是否为正确的中文姓名
    /// 姓名必须为2-4个中文字符
    /// </summary>
    /// <param name="text">要校验的姓名文本</param>
    /// <returns>如果是正确的中文姓名返回true，否则返回false</returns>
    public bool IsName(string text) {
        return NameRegex().IsMatch(text);
    }

    /// <summary>
    /// 校验是否为合法的用户名
    /// 用户名必须为6-10位的任意字符（包括字母、数字、特殊字符）
    /// </summary>
    /// <param name="text">要校验的用户名文本</param>
    /// <returns>如果是合法的用户名返回true，否则返回false</returns>
    public bool IsUsername(string text) {
        return UsernameRegex().IsMatch(text);
    }

    /// <summary>
    /// 校验是否为正确的电子邮箱地址
    /// 邮箱地址必须包含@符号，且@前后都不能为空
    /// </summary>
    /// <param name="text">要校验的邮箱地址文本</param>
    /// <returns>如果是正确的邮箱地址返回true，否则返回false</returns>
    public bool IsEmail(string text) {
        return EmailRegex().IsMatch(text);
    }

    /// <summary>
    /// 校验是否为正确的中国大陆手机号码
    /// 手机号码必须为11位数字，且以1开头
    /// </summary>
    /// <param name="text">要校验的手机号码文本</param>
    /// <returns>如果是正确的手机号码返回true，否则返回false</returns>
    public bool IsPhoneNumber(string text) {
        return PhoneNumberRegex().IsMatch(text);
    }

    [GeneratedRegex(@"^.{6,10}$")]
    private static partial Regex UsernameRegex();
    [GeneratedRegex(@"^[^@]+@[^@]+$")]
    private static partial Regex EmailRegex();
    [GeneratedRegex(@"^1\d{10}$")]
    private static partial Regex PhoneNumberRegex();
    [GeneratedRegex(@"^[\u4e00-\u9fa5]{2,4}$")]
    private static partial Regex NameRegex();
    [GeneratedRegex(@"^\d{6,10}$")]
    private static partial Regex ErpRegex();
    [GeneratedRegex(@"[0-9]")]
    private static partial Regex PasswordConditionRegex1();
    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex PasswordConditionRegex2();
    [GeneratedRegex(@"[a-z]")]
    private static partial Regex PasswordConditionRegex3();
    [GeneratedRegex(@"!@#\$%\^#&\*\(\),\.<>;':""/\?\[]{}\|")]
    private static partial Regex PasswordConditionRegex4();
}
