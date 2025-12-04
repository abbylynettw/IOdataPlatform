using IODataPlatform.Models.ExcelModels;

namespace IODataPlatform.Utilities;

/// <summary>匹配电缆失败数据</summary>
public class MatchCableDataFail
{
	/// <summary>端接数据</summary>
	public required TerminationData Data { get; set; }

	/// <summary>失败原因</summary>
	public required string Reason { get; set; }

	/// <summary>被选中</summary>
	public bool IsChecked { get; set; }
}
