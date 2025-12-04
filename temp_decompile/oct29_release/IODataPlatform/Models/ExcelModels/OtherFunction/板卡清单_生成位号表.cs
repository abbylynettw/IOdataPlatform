using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels.OtherFunction;

public class 板卡清单_生成位号表
{
	[Display(Name = "序号")]
	public int SequenceNumber { get; set; }

	[Display(Name = "机柜编号")]
	public string CabinetNumber { get; set; }

	[Display(Name = "控制器地址")]
	public string ControllerAddress { get; set; }

	[Display(Name = "机架号")]
	public string RackNumber { get; set; }

	[Display(Name = "插槽")]
	public string SlotNumber { get; set; }

	[Display(Name = "板卡类型")]
	public string CardType { get; set; }

	[Display(Name = "板卡编号")]
	public string CardNumber { get; set; }

	[Display(Name = "通讯板卡地址")]
	public string CommunicationCardAddress { get; set; }

	[Display(Name = "IO基座")]
	public string IoBase { get; set; }

	[Display(Name = "基座编号")]
	public string BaseNumber { get; set; }

	[Display(Name = "端子板类型")]
	public string TerminalBoardType { get; set; }

	[Display(Name = "端子板编号")]
	public string TerminalBoardNumber { get; set; }

	[Display(Name = "描述")]
	public string Description { get; set; }

	[Display(Name = "备注")]
	public string Remarks { get; set; }

	[Display(Name = "型号")]
	public string Model { get; set; }

	[Display(Name = "修改说明")]
	public string ModificationDescription { get; set; }
}
