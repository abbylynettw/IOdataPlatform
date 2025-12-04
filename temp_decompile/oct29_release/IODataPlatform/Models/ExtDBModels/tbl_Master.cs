using SqlSugar;

namespace IODataPlatform.Models.ExtDBModels;

[SugarTable("tbl_Master")]
public class tbl_Master
{
	[SugarColumn(ColumnName = "Tag_Name")]
	public string TagName { get; set; }

	[SugarColumn(ColumnName = "Ref_Id")]
	public int RefId { get; set; }

	[SugarColumn(ColumnName = "Trans_No")]
	public string TransNo { get; set; }

	[SugarColumn(ColumnName = "List")]
	public string List { get; set; }

	[SugarColumn(ColumnName = "Serial_Number")]
	public string SerialNumber { get; set; }

	[SugarColumn(ColumnName = "Id_Code")]
	public string IdCode { get; set; }

	[SugarColumn(ColumnName = "Extension_Code")]
	public string ExtensionCode { get; set; }

	[SugarColumn(ColumnName = "Designation")]
	public string Designation { get; set; }

	[SugarColumn(ColumnName = "Safety_Class_Division")]
	public string SafetyClassDivision { get; set; }

	[SugarColumn(ColumnName = "Functional_Class")]
	public string FunctionalClass { get; set; }

	[SugarColumn(ColumnName = "Seismic")]
	public string Seismic { get; set; }

	[SugarColumn(ColumnName = "IO_Type")]
	public string IOType { get; set; }

	[SugarColumn(ColumnName = "Sensor_Type")]
	public string SensorType { get; set; }

	[SugarColumn(ColumnName = "Signal_Spec")]
	public string SignalSpec { get; set; }

	[SugarColumn(ColumnName = "Power_Supply")]
	public string PowerSupply { get; set; }

	[SugarColumn(ColumnName = "Meas_Unit")]
	public string MeasUnit { get; set; }

	[SugarColumn(ColumnName = "Rang_Min")]
	public double RangMin { get; set; }

	[SugarColumn(ColumnName = "Rang_Max")]
	public double RangMax { get; set; }

	[SugarColumn(ColumnName = "Defsig")]
	public string Defsig { get; set; }

	[SugarColumn(ColumnName = "SOE_TRA")]
	public string SOETRA { get; set; }

	[SugarColumn(ColumnName = "Load_Information")]
	public string LoadInformation { get; set; }

	[SugarColumn(ColumnName = "Diagram_Number")]
	public string DiagramNumber { get; set; }

	[SugarColumn(ColumnName = "Revision")]
	public string Revision { get; set; }

	[SugarColumn(ColumnName = "Remarks")]
	public string Remarks { get; set; }

	[SugarColumn(ColumnName = "Electrical_Interface")]
	public string ElectricalInterface { get; set; }

	[SugarColumn(ColumnName = "INT_EXT")]
	public string INT_EXT { get; set; }

	[SugarColumn(ColumnName = "Unit")]
	public string Unit { get; set; }

	[SugarColumn(ColumnName = "System")]
	public string System { get; set; }

	[SugarColumn(ColumnName = "Class")]
	public string Class { get; set; }

	[SugarColumn(ColumnName = "Train")]
	public string Train { get; set; }

	[SugarColumn(ColumnName = "Isolation")]
	public string Isolation { get; set; }

	[SugarColumn(ColumnName = "Equipment")]
	public string Equipment { get; set; }

	[SugarColumn(ColumnName = "Destination")]
	public string Destination { get; set; }

	[SugarColumn(ColumnName = "Power_Type")]
	public string PowerType { get; set; }

	[SugarColumn(ColumnName = "EES")]
	public string EES { get; set; }

	[SugarColumn(ColumnName = "SA")]
	public string SA { get; set; }

	[SugarColumn(ColumnName = "Loop_Voltage")]
	public string LoopVoltage { get; set; }

	[SugarColumn(ColumnName = "Cabinet_Power")]
	public string CabinetPower { get; set; }

	[SugarColumn(ColumnName = "Contact_Capacity")]
	public string ContactCapacity { get; set; }

	[SugarColumn(ColumnName = "Contact_Type")]
	public string Contact_Type { get; set; }

	[SugarColumn(ColumnName = "IO_Module")]
	public string IO_Module { get; set; }

	[SugarColumn(ColumnName = "Pin")]
	public int Pin { get; set; }

	[SugarColumn(ColumnName = "Relay_IO_Module")]
	public string Relay_IO_Module { get; set; }

	[SugarColumn(ColumnName = "Relay_Pin")]
	public int Relay_Pin { get; set; }

	[SugarColumn(ColumnName = "Remarks-1")]
	public string Remarks_1 { get; set; }

	[SugarColumn(ColumnName = "IDA/ITI")]
	public string IDA_ITI { get; set; }

	[SugarColumn(ColumnName = "AP - 是否判断报警")]
	public bool AP_是否判断报警 { get; set; }

	[SugarColumn(ColumnName = "ALMLEVEL - 报警级别")]
	public string ALMLEVEL_报警级别 { get; set; }

	[SugarColumn(ColumnName = "KA - 开关量报警标签")]
	public string KA_开关量报警标签 { get; set; }

	[SugarColumn(ColumnName = "AL_DESC - 报警描述")]
	public string AL_DESC_报警描述 { get; set; }

	[SugarColumn(ColumnName = "AF - 报警属性")]
	public string AF_报警属性 { get; set; }

	[SugarColumn(ColumnName = "TP - 信号类型")]
	public string TP_信号类型 { get; set; }

	[SugarColumn(ColumnName = "SQ - 是否开方")]
	public bool SQ_是否开方 { get; set; }

	[SugarColumn(ColumnName = "DSEL - 超限死区设置方式")]
	public string DSEL_超限死区设置方式 { get; set; }

	[SugarColumn(ColumnName = "DI - 阈值恢复死区(%)")]
	public double DI_阈值恢复死区 { get; set; }

	[SugarColumn(ColumnName = "ROUT - 是否反量程输出")]
	public bool ROUT_是否反量程输出 { get; set; }

	[SugarColumn(ColumnName = "H4AP - 是否判断高4限越限")]
	public bool H4AP_是否判断高4限越限 { get; set; }

	[SugarColumn(ColumnName = "H4 - 高4限阈值")]
	public double H4_高4限阈值 { get; set; }

	[SugarColumn(ColumnName = "H4Level - 高4限报警级")]
	public string H4Level_高4限报警级 { get; set; }

	[SugarColumn(ColumnName = "H4KA - 高4限报警标签")]
	public string H4KA_高4限报警标签 { get; set; }

	[SugarColumn(ColumnName = "H4_DESC - 高4报警描述")]
	public string H4_DESC_高4报警描述 { get; set; }

	[SugarColumn(ColumnName = "H4DL - 高4限死区宽度")]
	public double H4DL_高4限死区宽度 { get; set; }

	[SugarColumn(ColumnName = "H3AP - 是否判断高3限越限")]
	public bool H3AP_是否判断高3限越限 { get; set; }

	[SugarColumn(ColumnName = "H3 - 高3限阈值")]
	public double H3_高3限阈值 { get; set; }

	[SugarColumn(ColumnName = "H3Level - 高3限报警级")]
	public string H3Level_高3限报警级 { get; set; }

	[SugarColumn(ColumnName = "H3KA - 高3限报警标签")]
	public string H3KA_高3限报警标签 { get; set; }

	[SugarColumn(ColumnName = "H3_DESC - 高3报警描述")]
	public string H3_DESC_高3报警描述 { get; set; }

	[SugarColumn(ColumnName = "H3DL - 高3限死区宽度")]
	public double H3DL_高3限死区宽度 { get; set; }

	[SugarColumn(ColumnName = "H2AP - 是否判断高2限越限")]
	public bool H2AP_是否判断高2限越限 { get; set; }

	[SugarColumn(ColumnName = "H2 - 高2限阈值")]
	public double H2_高2限阈值 { get; set; }

	[SugarColumn(ColumnName = "H2Level - 高2限报警级")]
	public string H2Level_高2限报警级 { get; set; }

	[SugarColumn(ColumnName = "H2KA - 高2限报警标签")]
	public string H2KA_高2限报警标签 { get; set; }

	[SugarColumn(ColumnName = "H2_DESC - 高2报警描述")]
	public string H2_DESC_高2报警描述 { get; set; }

	[SugarColumn(ColumnName = "H2DL - 高2限死区宽度")]
	public double H2DL_高2限死区宽度 { get; set; }

	[SugarColumn(ColumnName = "H1AP - 是否判断高1限越限")]
	public bool H1AP_是否判断高1限越限 { get; set; }

	[SugarColumn(ColumnName = "H1 - 高1限阈值")]
	public double H1_高1限阈值 { get; set; }

	[SugarColumn(ColumnName = "H1Level - 高1限报警级")]
	public string H1Level_高1限报警级 { get; set; }

	[SugarColumn(ColumnName = "H1KA - 高1限报警标签")]
	public string H1KA_高1限报警标签 { get; set; }

	[SugarColumn(ColumnName = "H1_DESC - 高1报警描述")]
	public string H1_DESC_高1报警描述 { get; set; }

	[SugarColumn(ColumnName = "H1DL - 高1限死区宽度")]
	public double H1DL_高1限死区宽度 { get; set; }

	[SugarColumn(ColumnName = "L1AP - 是否判断低1限越限")]
	public bool L1AP_是否判断低1限越限 { get; set; }

	[SugarColumn(ColumnName = "L1 - 低1限阈值")]
	public double L1_低1限阈值 { get; set; }

	[SugarColumn(ColumnName = "L1Level - 低1限报警级")]
	public string L1Level_低1限报警级 { get; set; }

	[SugarColumn(ColumnName = "L1KA - 低1限报警标签")]
	public string L1KA_低1限报警标签 { get; set; }

	[SugarColumn(ColumnName = "L1_DESC - 低1报警描述")]
	public string L1_DESC_低1报警描述 { get; set; }

	[SugarColumn(ColumnName = "L1DL - 低1限死区宽度")]
	public double L1DL_低1限死区宽度 { get; set; }

	[SugarColumn(ColumnName = "L2AP - 是否判断低2限越限")]
	public bool L2AP_是否判断低2限越限 { get; set; }

	[SugarColumn(ColumnName = "L2 - 低2限阈值")]
	public double L2_低2限阈值 { get; set; }

	[SugarColumn(ColumnName = "L2Level - 低2限报警级")]
	public string L2Level_低2限报警级 { get; set; }

	[SugarColumn(ColumnName = "L2KA - 低2限报警标签")]
	public string L2KA_低2限报警标签 { get; set; }

	[SugarColumn(ColumnName = "L2_DESC - 低2报警描述")]
	public string L2_DESC_低2报警描述 { get; set; }

	[SugarColumn(ColumnName = "L2DL - 低2限死区宽度")]
	public double L2DL_低2限死区宽度 { get; set; }

	[SugarColumn(ColumnName = "L3AP - 是否判断低3限越限")]
	public bool L3AP_是否判断低3限越限 { get; set; }

	[SugarColumn(ColumnName = "L3 - 低3限阈值")]
	public double L3_低3限阈值 { get; set; }

	[SugarColumn(ColumnName = "L3Level - 低3限报警级")]
	public string L3Level_低3限报警级 { get; set; }

	[SugarColumn(ColumnName = "L3KA - 低3限报警标签")]
	public string L3KA_低3限报警标签 { get; set; }

	[SugarColumn(ColumnName = "L3_DESC - 低3报警描述")]
	public string L3_DESC_低3报警描述 { get; set; }

	[SugarColumn(ColumnName = "L3DL - 低3限死区宽度")]
	public double L3DL_低3限死区宽度 { get; set; }

	[SugarColumn(ColumnName = "L4AP - 是否判断低4限越限")]
	public bool L4AP_是否判断低4限越限 { get; set; }

	[SugarColumn(ColumnName = "L4 - 低4限阈值")]
	public double L4_低4限阈值 { get; set; }

	[SugarColumn(ColumnName = "L4Level - 低4限报警级")]
	public string L4Level_低4限报警级 { get; set; }

	[SugarColumn(ColumnName = "L4KA - 低4限报警标签")]
	public string L4KA_低4限报警标签 { get; set; }

	[SugarColumn(ColumnName = "L4_DESC - 低4报警描述")]
	public string L4_DESC_低4报警描述 { get; set; }

	[SugarColumn(ColumnName = "L4DL - 低4限死区宽度")]
	public double L4DL_低4限死区宽度 { get; set; }

	[SugarColumn(ColumnName = "RTDLINE - RTD接线方式")]
	public string RTDLINE_RTD接线方式 { get; set; }

	[SugarColumn(ColumnName = "ALLOCATION - 变量分组")]
	public string ALLOCATION_变量分组 { get; set; }

	[SugarColumn(ColumnName = "QFM - 输出信号替代方式")]
	public string QFM_输出信号替代方式 { get; set; }

	[SugarColumn(ColumnName = "QFIA - 输出替代值")]
	public double QFIA_输出替代值 { get; set; }

	[SugarColumn(ColumnName = "FILTER - 是否滤波")]
	public bool FILTER_是否滤波 { get; set; }

	[SugarColumn(ColumnName = "FILTER_TIME - 滤波时间")]
	public double FILTER_TIME_滤波时间 { get; set; }

	[SugarColumn(ColumnName = "PG - 脉冲当量")]
	public double PG_脉冲当量 { get; set; }

	[SugarColumn(ColumnName = "IC - 是否产生仪控故障")]
	public bool IC_是否产生仪控故障 { get; set; }

	[SugarColumn(ColumnName = "OF - 显示格式")]
	public string OF_显示格式 { get; set; }

	[SugarColumn(ColumnName = "Cabinet_Type")]
	public string Cabinet_Type { get; set; }

	[SugarColumn(ColumnName = "SUBNET - 子网")]
	public string SUBNET_子网 { get; set; }

	[SugarColumn(ColumnName = "SN - 站号")]
	public int SN_站号 { get; set; }

	[SugarColumn(ColumnName = "Cabinet_No")]
	public string Cabinet_No { get; set; }

	[SugarColumn(ColumnName = "Cabinet_Controller")]
	public string Cabinet_Controller { get; set; }

	[SugarColumn(ColumnName = "信号类型")]
	public string 信号类型 { get; set; }

	[SugarColumn(ColumnName = "信号规范")]
	public string 信号规范 { get; set; }

	[SugarColumn(ColumnName = "单位")]
	public string 单位 { get; set; }

	[SugarColumn(ColumnName = "供电")]
	public string 供电 { get; set; }

	[SugarColumn(ColumnName = "量程下限")]
	public string 量程下限 { get; set; }

	[SugarColumn(ColumnName = "量程上限")]
	public string 量程上限 { get; set; }

	[SugarColumn(ColumnName = "报警低低")]
	public string 报警低低 { get; set; }

	[SugarColumn(ColumnName = "报警低限")]
	public string 报警低限 { get; set; }

	[SugarColumn(ColumnName = "报警高限")]
	public string 报警高限 { get; set; }

	[SugarColumn(ColumnName = "报警高高")]
	public string 报警高高 { get; set; }

	[SugarColumn(ColumnName = "信号用途报警")]
	public string 信号用途报警 { get; set; }

	[SugarColumn(ColumnName = "信号用途控制")]
	public string 信号用途控制 { get; set; }

	[SugarColumn(ColumnName = "信号用途SOE")]
	public string 信号用途SOE { get; set; }

	[SugarColumn(ColumnName = "特殊处理")]
	public string 特殊处理 { get; set; }

	[SugarColumn(ColumnName = "信号起点")]
	public string 信号起点 { get; set; }

	[SugarColumn(ColumnName = "信号终点")]
	public string 信号终点 { get; set; }

	[SugarColumn(ColumnName = "控制站号")]
	public string 控制站号 { get; set; }

	[SugarColumn(ColumnName = "备注")]
	public string 备注 { get; set; }
}
