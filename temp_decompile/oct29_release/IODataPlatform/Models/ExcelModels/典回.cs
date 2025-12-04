using System.Collections.Generic;
using IODataPlatform.Models.DBModels;

namespace IODataPlatform.Models.ExcelModels;

/// <summary>
/// 读取的典回模板详情
/// </summary>
public class 典回
{
	public required string 典回类型 { get; set; }

	public required List<config_termination_yjs> loopRows { get; set; }
}
