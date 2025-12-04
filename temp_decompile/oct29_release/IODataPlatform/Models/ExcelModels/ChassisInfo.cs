using System.Collections.Generic;

namespace IODataPlatform.Models.ExcelModels;

/// <summary>机笼</summary>
public class ChassisInfo
{
	/// <summary>序号</summary>
	public required int Index { get; set; }

	/// <summary>插槽列表</summary>
	public required List<SlotInfo> Slots { get; set; }
}
