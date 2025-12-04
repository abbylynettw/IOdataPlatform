using System.ComponentModel.DataAnnotations;

/// <summary>
/// 中控系统AI点（模拟量输入）
/// </summary>
public class xtes_zhongkong_AI
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
    public string 量程上限 { get; set; } = "100.0000";

    [Display(Name = "单位")]
    public string 单位 { get; set; } = "%";

    [Display(Name = "位号类型")]
    public string 位号类型 { get; set; } = "常规AI位号";

    [Display(Name = "模块类型")]
    public string 模块类型 { get; set; }

    [Display(Name = "信号类型")]
    public string 信号类型 { get; set; }

    [Display(Name = "位号运行周期")]
    public string 位号运行周期 { get; set; } = "基本扫描周期";

    [Display(Name = "数据类型")]
    public string 数据类型 { get; set; } = "2字节整数(有符号)";

    [Display(Name = "信号性质")]
    public string 信号性质 { get; set; } = "工程量";

    [Display(Name = "状态码位置")]
    public string 状态码位置 { get; set; } = "状态码在前";

    [Display(Name = "数据格式")]
    public string 数据格式 { get; set; } = "不转换";

    [Display(Name = "转换类型")]
    public string 转换类型 { get; set; } = "线性转换";

    [Display(Name = "线性开方")]
    public string 线性开方 { get; set; } = "不开方";

    [Display(Name = "小信号")]
    public string 小信号 { get; set; } = "不切除";

    [Display(Name = "小信号切除值(%)")]
    public string 小信号切除值 { get; set; } = "0.5000";

    [Display(Name = "滤波时间常数(秒)")]
    public string 滤波时间常数 { get; set; } = "0.0000";

    [Display(Name = "扩展量程上限百分量(%)")]
    public string 扩展量程上限百分量 { get; set; } = "10.0000";

    [Display(Name = "扩展量程下限百分量(%)")]
    public string 扩展量程下限百分量 { get; set; } = "10.0000";

    [Display(Name = "超量程上限报警")]
    public string 超量程上限报警 { get; set; } = "使能";

    [Display(Name = "超量程下限报警")]
    public string 超量程下限报警 { get; set; } = "使能";

    [Display(Name = "输入原始码上限")]
    public string 输入原始码上限 { get; set; } = "100.0000";

    [Display(Name = "输入原始码下限")]
    public string 输入原始码下限 { get; set; } = "0.0000";

    [Display(Name = "高三限报警")]
    public string 高三限报警 { get; set; } = "禁止";

    [Display(Name = "高三限报警等级")]
    public string 高三限报警等级 { get; set; } = "低";

    [Display(Name = "高三限报警值")]
    public string 高三限报警值 { get; set; } = "100.0000";

    [Display(Name = "高高限报警")]
    public string 高高限报警 { get; set; } = "禁止";

    [Display(Name = "高高限报警等级")]
    public string 高高限报警等级 { get; set; } = "低";

    [Display(Name = "高高限报警值")]
    public string 高高限报警值 { get; set; } = "95.0000";

    [Display(Name = "高限报警")]
    public string 高限报警 { get; set; } = "禁止";

    [Display(Name = "高限报警等级")]
    public string 高限报警等级 { get; set; } = "低";

    [Display(Name = "高限报警值")]
    public string 高限报警值 { get; set; } = "90.0000";

    [Display(Name = "低限报警")]
    public string 低限报警 { get; set; } = "禁止";

    [Display(Name = "低限报警等级")]
    public string 低限报警等级 { get; set; } = "低";

    [Display(Name = "低限报警值")]
    public string 低限报警值 { get; set; } = "10.0000";

    [Display(Name = "低低限报警")]
    public string 低低限报警 { get; set; } = "禁止";

    [Display(Name = "低低限报警等级")]
    public string 低低限报警等级 { get; set; } = "低";

    [Display(Name = "低低限报警值")]
    public string 低低限报警值 { get; set; } = "5.0000";

    [Display(Name = "低三限报警")]
    public string 低三限报警 { get; set; } = "禁止";

    [Display(Name = "低三限报警等级")]
    public string 低三限报警等级 { get; set; } = "低";

    [Display(Name = "低三限报警值")]
    public string 低三限报警值 { get; set; } = "0.0000";

    [Display(Name = "高低限报警滞环值")]
    public string 高低限报警滞环值 { get; set; } = "1";

    [Display(Name = "高低限报警滞环值转换方式")]
    public string 高低限报警滞环值转换方式 { get; set; } = "工程量";

    [Display(Name = "变化速率报警")]
    public string 变化速率报警 { get; set; } = "禁止";

    [Display(Name = "变化速率报警等级")]
    public string 变化速率报警等级 { get; set; } = "低";

    [Display(Name = "变化速率报警值")]
    public string 变化速率报警值 { get; set; } = "100.0000";

    [Display(Name = "故障报警")]
    public string 故障报警 { get; set; } = "使能";

    [Display(Name = "故障报警等级")]
    public string 故障报警等级 { get; set; } = "低";

    [Display(Name = "通讯故障报警产生延时(秒)")]
    public string 通讯故障报警产生延时 { get; set; } = "0.0000";

    [Display(Name = "故障处理")]
    public string 故障处理 { get; set; } = "保持";

    [Display(Name = "故障预置值")]
    public string 故障预置值 { get; set; } = "0.0000";

    [Display(Name = "位号分组")]
    public string 位号分组 { get; set; } = "位号分组 0";

    [Display(Name = "位号等级")]
    public string 位号等级 { get; set; } = "0级";

    [Display(Name = "小数位数")]
    public string 小数位数 { get; set; } = "2";

    [Display(Name = "面板")]
    public string 面板 { get; set; }
}
