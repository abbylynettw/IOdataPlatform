using System.ComponentModel.DataAnnotations;

/// <summary>
/// 中控系统AO点（模拟量输出）
/// </summary>
public class xtes_zhongkong_AO
{
    [Display(Name = "名称")]
    public string 名称 { get; set; }

    [Display(Name = "描述")]
    public string 描述 { get; set; }

    [Display(Name = "控制站地址")]
    public string 控制站地址 { get; set; }

    [Display(Name = "地址")]
    public string 地址 { get; set; }

    [Display(Name = "量程下限")]
    public string 量程下限 { get; set; } = "0.0000";

    [Display(Name = "量程上限")]
    public string 量程上限 { get; set; } = "100.000";

    [Display(Name = "单位")]
    public string 单位 { get; set; } = "%";

    [Display(Name = "位号类型")]
    public string 位号类型 { get; set; } = "常规AO位号";

    [Display(Name = "模块类型")]
    public string 模块类型 { get; set; }

    [Display(Name = "信号类型")]
    public string 信号类型 { get; set; } = "电流(4mA～20mA)";

    [Display(Name = "故障安全模式")]
    public string 故障安全模式 { get; set; } = "输出保持";

    [Display(Name = "故障状态设定值")]
    public string 故障状态设定值 { get; set; } = "0.0000";

    [Display(Name = "位号运行周期")]
    public string 位号运行周期 { get; set; } = "基本扫描周期";

    [Display(Name = "数据类型")]
    public string 数据类型 { get; set; } = "2字节整数(无符号)";

    [Display(Name = "信号性质")]
    public string 信号性质 { get; set; } = "工程量";

    [Display(Name = "状态码位置")]
    public string 状态码位置 { get; set; } = "状态码在前";

    [Display(Name = "数据格式")]
    public string 数据格式 { get; set; } = "不转换";

    [Display(Name = "转换类型")]
    public string 转换类型 { get; set; } = "线性转换";

    [Display(Name = "正/反输出")]
    public string 正反输出 { get; set; } = "正输出";

    [Display(Name = "故障安全下位号输出使能")]
    public string 故障安全下位号输出使能 { get; set; } = "禁止";

    [Display(Name = "扩展量程上限百分量(%)")]
    public string 扩展量程上限百分量 { get; set; } = "0.0000";

    [Display(Name = "扩展量程下限百分量(%)")]
    public string 扩展量程下限百分量 { get; set; } = "0.0000";

    [Display(Name = "超量程上限报警")]
    public string 超量程上限报警 { get; set; } = "禁止";

    [Display(Name = "超量程下限报警")]
    public string 超量程下限报警 { get; set; } = "禁止";

    [Display(Name = "输出原始码上限")]
    public string 输出原始码上限 { get; set; } = "100.0000";

    [Display(Name = "输出原始码下限")]
    public string 输出原始码下限 { get; set; } = "0.0000";

    [Display(Name = "输出高限限幅报警")]
    public string 输出高限限幅报警 { get; set; } = "禁止";

    [Display(Name = "输出高限限幅报警等级")]
    public string 输出高限限幅报警等级 { get; set; } = "低";

    [Display(Name = "输出高限限幅值")]
    public string 输出高限限幅值 { get; set; } = "50.0000";

    [Display(Name = "输出低限限幅报警")]
    public string 输出低限限幅报警 { get; set; } = "禁止";

    [Display(Name = "输出低限限幅报警等级")]
    public string 输出低限限幅报警等级 { get; set; } = "低";

    [Display(Name = "输出低限限幅值")]
    public string 输出低限限幅值 { get; set; } = "0.0000";

    [Display(Name = "故障报警")]
    public string 故障报警 { get; set; } = "使能";

    [Display(Name = "故障报警等级")]
    public string 故障报警等级 { get; set; } = "低";

    [Display(Name = "组态错误报警")]
    public string 组态错误报警 { get; set; } = "使能";

    [Display(Name = "组态错误报警等级")]
    public string 组态错误报警等级 { get; set; } = "低";

    [Display(Name = "位号分组")]
    public string 位号分组 { get; set; } = "位号分组 0";

    [Display(Name = "位号等级")]
    public string 位号等级 { get; set; } = "0级";

    [Display(Name = "小数位数")]
    public string 小数位数 { get; set; } = "2";

    [Display(Name = "面板")]
    public string 面板 { get; set; } = "*";
}
