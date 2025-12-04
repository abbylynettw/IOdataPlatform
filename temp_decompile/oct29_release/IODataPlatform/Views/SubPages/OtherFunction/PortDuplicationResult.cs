using System.Collections.Generic;
using System.Linq;

namespace IODataPlatform.Views.SubPages.OtherFunction;

/// <summary>
/// 端口重复检测结果
/// </summary>
public class PortDuplicationResult
{
	public Dictionary<string, List<PortUsageInfo>> DuplicatePorts { get; set; } = new Dictionary<string, List<PortUsageInfo>>();

	public bool HasDuplicates => DuplicatePorts.Any();
}
