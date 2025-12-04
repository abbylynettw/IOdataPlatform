using System.Collections.Generic;

namespace IODataPlatform.Models.ExcelModels;

public class 每根电缆
{
	public 电缆类型 控制电缆 { get; set; }

	public List<CableData> Signals { get; set; }
}
