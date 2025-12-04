﻿using SqlSugar;

namespace IODataPlatform.Models.DBModels;

/// <summary>
/// 图纸数据库实体类
/// 存储工程项目中的技术图纸信息，包括图纸名称、版本、文件等
/// 与盘箱柜实体关联，实现图纸与设备的对应关系
/// </summary>
public class 图纸 {

    /// <summary>图纸唯一标识符，主键，自增长</summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>关联的盘箱柜表ID，外键</summary>
    [SugarColumn]
    public int 盘箱柜表Id { get; set; }

    /// <summary>图纸名称，最大长度150个字符，不允许为空</summary>
    [SugarColumn(Length = 150, IsNullable = false)]
    public string 名称 { get; set; } = string.Empty;
    
    /// <summary>图纸版本号，最大长度10个字符，不允许为空</summary>
    [SugarColumn(Length = 10, IsNullable = false)]
    public string 版本 { get; set; } = string.Empty;
    
    /// <summary>图纸发布日期，默认为当前时间</summary>
    [SugarColumn(IsNullable = false)]
    public DateTime 发布日期 { get; set; } = DateTime.Now;

    /// <summary>版本升级说明，可为空</summary>
    [SugarColumn(IsNullable = true)]
    public string 升版说明 { get; set; } = string.Empty;

    /// <summary>图纸压缩包文件名，最大长度150个字符，可为空</summary>
    [SugarColumn(Length = 150, IsNullable = true)]
    public string 压缩包文件名 { get; set; } = string.Empty;
    
    /// <summary>依据文件名，最大长度150个字符，可为空</summary>
    [SugarColumn(Length = 150, IsNullable = true)]
    public string 依据文件名 { get; set; } = string.Empty;

    /// <summary>PDF文件名，最大长度150个字符，可为空</summary>
    [SugarColumn(Length = 150, IsNullable = true)]
    public string PDF文件名 { get; set; } = string.Empty;

}