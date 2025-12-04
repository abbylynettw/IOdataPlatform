using IODataPlatform.Models.DBModels;

namespace IODataPlatform.Models.ExcelModels;

public class 内部接线清单 {
    public string 位置 { get; set; } = string.Empty;
    public string 典回类型 { get; set; } = string.Empty;
    public string 信号名称 { get; set; } = string.Empty;
    public string 安全等级 { get; set; } = string.Empty;
    public string 安全列 { get; set; } = string.Empty;
    public string 端子组名 { get; set; } = string.Empty;
    public string 端子号 { get; set; } = string.Empty;
    public string SheetName { get; set; } = string.Empty;
}

/// <summary>
/// 读取的典回模板详情
/// </summary>
public class 典回 {
    public required string 典回类型 { get; set; } 
    public required List<config_termination_yjs> loopRows { get; set; }
}
