using SqlSugar;

namespace IODataPlatform.Models.DBModels;

public class formular
{
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
	public int Id { get; set; }

	public string FieldName { get; set; } = string.Empty;

	public string TargetType { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;
}
