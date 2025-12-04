using System.Collections.Generic;

namespace IODataPlatform.Models.ExcelModels;

public class TotalSummaryInfo
{
	public required NumberCheck TotalPoints { get; set; }

	public required NumberCheck BackupPoints { get; set; }

	public required NumberCheck AlarmPoints { get; set; }

	public required NumberCheck NormalPoints { get; set; }

	public required List<CardSpareRate> RedundancyRates { get; set; }
}
