using System.Collections.Generic;
using IODataPlatform.Models.ExcelModels;

namespace IODataPlatform.Utilities;

/// <summary>匹配电缆数据结果</summary>
public class MatchCableDataResult
{
	/// <summary>匹配成功的电缆数据列表</summary>
	public List<CableData> SuccessList { get; set; }

	/// <summary>数据源1中匹配失败的数据列表</summary>
	public List<MatchCableDataFail> FailList1 { get; set; }

	/// <summary>数据源2中匹配失败的数据列表</summary>
	public List<MatchCableDataFail> FailList2 { get; set; }
}
