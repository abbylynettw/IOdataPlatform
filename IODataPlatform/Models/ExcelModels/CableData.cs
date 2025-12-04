using IODataPlatform.Models.DBModels;

using SqlSugar;

namespace IODataPlatform.Models.ExcelModels;

/// <summary>
/// 电缆表
///</summary>
public class CableData {
    public int Id { get; set; }
    /// <summary>
    /// 序号 
    ///</summary>
    public string 序号 { get; set; }
    /// <summary>
    /// 线缆编号 
    ///</summary>
    public string 线缆编号 { get; set; }
    /// <summary>
    /// 线缆列别 
    ///</summary>
    public string 线缆列别 { get; set; }
    /// <summary>
    /// 色标 
    ///</summary>
    public string 色标 { get; set; }
    /// <summary>
    /// 特性代码 
    ///</summary>
    public string 特性代码 { get; set; }
    /// <summary>
    /// 芯线对数号 
    ///</summary>
    public string 芯线对数号 { get; set; }
    /// <summary>
    /// 芯线号 
    ///</summary>
    public string 芯线号 { get; set; }
    /// <summary>
    /// 起点房间号 
    ///</summary>
    public string 起点房间号 { get; set; }
    /// <summary>
    /// 起点盘柜名称 
    ///</summary>
    public string 起点盘柜名称 { get; set; }
    public string 起点安全分级分组 { get; set; }
    public string 起点系统号 { get; set; }
    public string 起点IO类型 { get; set; }
    /// <summary>
    /// 起点设备名称 
    ///</summary>
    public string 起点设备名称 { get; set; }
    /// <summary>
    /// 起点接线点1 
    ///</summary>
    public string 起点接线点1 { get; set; }
    /// <summary>
    /// 起点接线点2 
    ///</summary>
    public string 起点接线点2 { get; set; }
    /// <summary>
    /// 起点接线点3 
    ///</summary>
    public string 起点接线点3 { get; set; }
    /// <summary>
    /// 起点接线点4 
    ///</summary>
    public string 起点接线点4 { get; set; }
    /// <summary>
    /// 起点屏蔽端 
    ///</summary>
    public string 起点屏蔽端 { get; set; }
    /// <summary>
    /// 起点信号位号 
    ///</summary>
    public string 起点信号位号 { get; set; }
    /// <summary>
    /// 电缆长度 
    ///</summary>
    public string 电缆长度 { get; set; }
    /// <summary>
    /// 终点房间号 
    ///</summary>
    public string 终点房间号 { get; set; }
    /// <summary>
    /// 终点盘柜名称 
    ///</summary>
    public string 终点盘柜名称 { get; set; }
    /// <summary>
    /// 终点设备名称 
    ///</summary>
    public string 终点设备名称 { get; set; }

    public string 终点安全分级分组 { get; set; }
    public string 终点系统号 { get; set; }
    public string 终点IO类型 { get; set; }
    /// <summary>
    /// 终点接线点1 
    ///</summary>
    public string 终点接线点1 { get; set; }
    /// <summary>
    /// 终点接线点2 
    ///</summary>
    public string 终点接线点2 { get; set; }
    /// <summary>
    /// 终点接线点3 
    ///</summary>
    public string 终点接线点3 { get; set; }
    /// <summary>
    /// 终点接线点4 
    ///</summary>
    public string 终点接线点4 { get; set; }
    /// <summary>
    /// 终点屏蔽端 
    ///</summary>
    public string 终点屏蔽端 { get; set; }
    /// <summary>
    /// 终点信号位号 
    ///</summary>
    public string 终点信号位号 { get; set; }
    /// <summary>
    /// IO类型 
    ///</summary>
    public string IO类型 { get; set; }
    /// <summary>
    /// 信号说明 
    ///</summary>
    public string 信号说明 { get; set; }
    /// <summary>
    /// 供货方 
    ///</summary>
    public string 供货方 { get; set; }
    /// <summary>
    /// 版本 
    ///</summary>
    public string 版本 { get; set; }
    /// <summary>
    /// 备注 
    ///</summary>
    public string 备注 { get; set; }

    /// <summary>
    /// 匹配情况 
    ///</summary>
    public string 匹配情况 { get; set; }

    public int StartNumber { get; set; }   
    public config_cable_systemNumber SystemNo { get; set; }

    public string 起点专业 { get; set; }
    public string 终点专业 { get; set; }

    public bool IsDeletedRow { get; set; }

}
