namespace IODataPlatform.Views.SubPages.Common;

/// <summary>
/// 网络接线清单连接点唯一键（基于起点+终点信息）
/// </summary>
public record ConnectionKey
{
	public string StartCabinet { get; init; } = "";

	public string StartDevice { get; init; } = "";

	public string StartConnection { get; init; } = "";

	public string EndCabinet { get; init; } = "";

	public string EndDevice { get; init; } = "";

	public string EndConnection { get; init; } = "";

	public override string ToString()
	{
		return $"{StartCabinet}|{StartDevice}|{StartConnection} → {EndCabinet}|{EndDevice}|{EndConnection}";
	}
}
