﻿using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>
/// 盘箱柜数据库实体类
/// 存储工程项目中的盘箱柜设备信息，包括机柜、盘台、阀箱等
/// 与项目实体关联，支持不同类别的设备管理和编码管理
/// </summary>
public class 盘箱柜 {

    /// <summary>盘箱柜唯一标识符，主键，自增长</summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>关联的项目ID，外键</summary>
    [SugarColumn]
    public int 项目Id { get; set; }

    /// <summary>盘箱柜名称，最大长度50个字符，不允许为空</summary>
    [SugarColumn(Length = 50, IsNullable = false)]
    public string 名称 { get; set; } = string.Empty;

    /// <summary>盘箱柜类别（机柜、盘台、阀箱），不允许为空</summary>
    [SugarColumn(IsNullable = false)]
    public 盘箱柜类别 类别 { get; set; }

    /// <summary>设备子类别，最大长度50个字符，可为空</summary>
    [SugarColumn(Length = 50, IsNullable = true)]
    public string 子类别 { get; set; } = string.Empty;

    /// <summary>设备所在房间号，最大长度20个字符，可为空</summary>
    [SugarColumn(Length = 20, IsNullable = true)]
    public string 房间号 { get; set; } = string.Empty;

    /// <summary>内部编码，最大长度50个字符，可为空</summary>
    [SugarColumn(Length = 50, IsNullable = true)]
    public string 内部编码 { get; set; } = string.Empty;

    /// <summary>外部编码，最大长度50个字符，可为空</summary>
    [SugarColumn(Length = 50, IsNullable = true)]
    public string 外部编码 { get; set; } = string.Empty;

    /// <summary>子项信息，最大长度50个字符，可为空</summary>
    [SugarColumn(Length = 50, IsNullable = true)]
    public string 子项 { get; set; } = string.Empty;

    /// <summary>LOT号，最大长度50个字符，可为空</summary>
    [SugarColumn(Length = 50, IsNullable = true)]
    public string LOT { get; set; } = string.Empty;

    /// <summary>Batch号，最大长度50个字符，可为空</summary>
    [SugarColumn(Length = 50, IsNullable = true)]
    public string Batch { get; set; } = string.Empty;

}