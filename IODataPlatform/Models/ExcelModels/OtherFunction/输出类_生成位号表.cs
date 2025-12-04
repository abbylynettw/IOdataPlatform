using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Models.ExcelModels.OtherFunction
{
    public class AI_生成位号表
    {
        [Display(Name = "序号")]
       public string SequenceNumber { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "控制站地址")]
        public string ControlStationAddress { get; set; }

        [Display(Name = "地址")]
        public string Address { get; set; }

        [Display(Name = "量程下限")]
        public double RangeLowerLimit { get; set; }

        [Display(Name = "量程上限")]
        public double RangeUpperLimit { get; set; }

        [Display(Name = "单位")]
        public string Unit { get; set; }

        [Display(Name = "位号类型")]
        public string TagType { get; set; }

        [Display(Name = "模块类型")]
        public string ModuleType { get; set; }

        [Display(Name = "信号类型")]
        public string SignalType { get; set; }

        [Display(Name = "位号运行周期")]
        public string TagOperatingCycle { get; set; }

        [Display(Name = "数据类型")]
        public string DataType { get; set; }

        [Display(Name = "信号性质")]
        public string SignalNature { get; set; }

        [Display(Name = "状态码位置")]
        public string StatusCodePosition { get; set; }

        [Display(Name = "数据格式")]
        public string DataFormat { get; set; }

        [Display(Name = "转换类型")]
        public string ConversionType { get; set; }

        [Display(Name = "线性开方")]
        public string LinearSquareRoot { get; set; }

        [Display(Name = "小信号")]
        public string SmallSignal { get; set; }

        [Display(Name = "小信号切除值(%)")]
        public double SmallSignalCutoffValue { get; set; }

        [Display(Name = "滤波时间常数(秒)")]
        public double FilterTimeConstant { get; set; }

        [Display(Name = "扩展量程上限百分量(%)")]
        public double ExtendedRangeUpperPercentage { get; set; }

        [Display(Name = "扩展量程下限百分量(%)")]
        public double ExtendedRangeLowerPercentage { get; set; }

        [Display(Name = "超量程上限报警")]
        public string OverrangeUpperAlarm { get; set; }

        [Display(Name = "超量程下限报警")]
        public string OverrangeLowerAlarm { get; set; }

        [Display(Name = "输入原始码上限")]
        public double InputRawCodeUpperLimit { get; set; }

        [Display(Name = "输入原始码下限")]
        public double InputRawCodeLowerLimit { get; set; }

        [Display(Name = "高三限报警")]
        public string HighTripLimitAlarm { get; set; }

        [Display(Name = "高三限报警等级")]
        public string HighTripLimitAlarmLevel { get; set; }

        [Display(Name = "高三限报警值")]
        public double HighTripLimitAlarmValue { get; set; }

        [Display(Name = "高高限报警")]
        public string HighHighLimitAlarm { get; set; }

        [Display(Name = "高高限报警等级")]
        public string HighHighLimitAlarmLevel { get; set; }

        [Display(Name = "高高限报警值")]
        public double HighHighLimitAlarmValue { get; set; }

        [Display(Name = "高限报警")]
        public string HighLimitAlarm { get; set; }

        [Display(Name = "高限报警等级")]
        public string HighLimitAlarmLevel { get; set; }

        [Display(Name = "高限报警值")]
        public double HighLimitAlarmValue { get; set; }

        [Display(Name = "低限报警")]
        public string LowLimitAlarm { get; set; }

        [Display(Name = "低限报警等级")]
        public string LowLimitAlarmLevel { get; set; }

        [Display(Name = "低限报警值")]
        public double LowLimitAlarmValue { get; set; }

        [Display(Name = "低低限报警")]
        public string LowLowLimitAlarm { get; set; }

        [Display(Name = "低低限报警等级")]
        public string LowLowLimitAlarmLevel { get; set; }

        [Display(Name = "低低限报警值")]
        public double LowLowLimitAlarmValue { get; set; }

        [Display(Name = "低三限报警")]
        public string LowTripLimitAlarm { get; set; }

        [Display(Name = "低三限报警等级")]
        public string LowTripLimitAlarmLevel { get; set; }

        [Display(Name = "低三限报警值")]
        public double LowTripLimitAlarmValue { get; set; }

        [Display(Name = "高低限报警滞环值")]
        public double HighLowLimitAlarmHysteresisValue { get; set; }

        [Display(Name = "变化速率报警")]
        public string RateOfChangeAlarm { get; set; }

        [Display(Name = "变化速率报警等级")]
        public string RateOfChangeAlarmLevel { get; set; }

        [Display(Name = "变化速率报警值")]
        public double RateOfChangeAlarmValue { get; set; }

        [Display(Name = "故障报警")]
        public string FaultAlarm { get; set; }

        [Display(Name = "故障报警等级")]
        public string FaultAlarmLevel { get; set; }

        [Display(Name = "故障处理")]
        public string FaultHandling { get; set; }

        [Display(Name = "位号分组")]
        public string TagGroup { get; set; }

        [Display(Name = "位号等级")]
        public string TagLevel { get; set; }

        [Display(Name = "小数位数")]
       public string DecimalPlaces { get; set; }

    }
    public class AO_生成位号表
    {
        [Display(Name = "序号")]
       public string SequenceNumber { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "控制站地址")]
        public string ControlStationAddress { get; set; }

        [Display(Name = "地址")]
        public string Address { get; set; }

        [Display(Name = "量程下限")]
        public double RangeLowerLimit { get; set; }

        [Display(Name = "量程上限")]
        public double RangeUpperLimit { get; set; }

        [Display(Name = "单位")]
        public string Unit { get; set; }

        [Display(Name = "位号类型")]
        public string TagType { get; set; }

        [Display(Name = "模块类型")]
        public string ModuleType { get; set; }

        [Display(Name = "信号类型")]
        public string SignalType { get; set; }

        [Display(Name = "故障安全模式")]
        public string FaultSafetyMode { get; set; }

        [Display(Name = "故障状态设定值")]
        public double FaultStateSetValue { get; set; }

        [Display(Name = "位号运行周期")]
        public string TagOperatingCycle { get; set; }

        [Display(Name = "数据类型")]
        public string DataType { get; set; }

        [Display(Name = "信号性质")]
        public string SignalNature { get; set; }

        [Display(Name = "状态码位置")]
        public string StatusCodePosition { get; set; }

        [Display(Name = "数据格式")]
        public string DataFormat { get; set; }

        [Display(Name = "转换类型")]
        public string ConversionType { get; set; }

        [Display(Name = "正/反输出")]
        public string PositiveNegativeOutput { get; set; }

        [Display(Name = "扩展量程上限百分量(%)")]
        public double ExtendedRangeUpperPercentage { get; set; }

        [Display(Name = "扩展量程下限百分量(%)")]
        public double ExtendedRangeLowerPercentage { get; set; }

        [Display(Name = "超量程上限报警")]
        public string OverrangeUpperAlarm { get; set; }

        [Display(Name = "超量程下限报警")]
        public string OverrangeLowerAlarm { get; set; }

        [Display(Name = "输出原始码上限")]
        public double OutputRawCodeUpperLimit { get; set; }

        [Display(Name = "输出原始码下限")]
        public double OutputRawCodeLowerLimit { get; set; }

        [Display(Name = "输出高限限幅报警")]
        public string OutputHighLimitClippingAlarm { get; set; }

        [Display(Name = "输出高限限幅报警等级")]
        public string OutputHighLimitClippingAlarmLevel { get; set; }

        [Display(Name = "输出高限限幅值")]
        public double OutputHighLimitClippingValue { get; set; }

        [Display(Name = "输出低限限幅报警")]
        public string OutputLowLimitClippingAlarm { get; set; }

        [Display(Name = "输出低限限幅报警等级")]
        public string OutputLowLimitClippingAlarmLevel { get; set; }

        [Display(Name = "输出低限限幅值")]
        public double OutputLowLimitClippingValue { get; set; }

        [Display(Name = "故障报警")]
        public string FaultAlarm { get; set; }

        [Display(Name = "故障报警等级")]
        public string FaultAlarmLevel { get; set; }

        [Display(Name = "组态错误报警")]
        public string ConfigurationErrorAlarm { get; set; }

        [Display(Name = "组态错误报警等级")]
        public string ConfigurationErrorAlarmLevel { get; set; }

        [Display(Name = "位号分组")]
        public string TagGroup { get; set; }

        [Display(Name = "位号等级")]
        public string TagLevel { get; set; }

        [Display(Name = "小数位数")]
       public string DecimalPlaces { get; set; }
    }
    public class DI_生成位号表
    {
        [Display(Name = "序号")]
       public string SequenceNumber { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "控制站地址")]
        public string ControlStationAddress { get; set; }

        [Display(Name = "地址")]
        public string Address { get; set; }

        [Display(Name = "ON描述")]
        public string OnDescription { get; set; }

        [Display(Name = "OFF描述")]
        public string OffDescription { get; set; }

        [Display(Name = "位号类型")]
        public string TagType { get; set; }

        [Display(Name = "模块类型")]
        public string ModuleType { get; set; }

        [Display(Name = "位号运行周期")]
        public string TagOperatingCycle { get; set; }

        [Display(Name = "输入取反")]
        public string InputReverse { get; set; }

        [Display(Name = "ON状态报警")]
        public string OnStateAlarm { get; set; }

        [Display(Name = "ON状态报警等级")]
        public string OnStateAlarmLevel { get; set; }

        [Display(Name = "OFF状态报警")]
        public string OffStateAlarm { get; set; }

        [Display(Name = "OFF状态报警等级")]
        public string OffStateAlarmLevel { get; set; }

        [Display(Name = "正跳变报警")]
        public string PositiveJumpAlarm { get; set; }

        [Display(Name = "正跳变报警等级")]
        public string PositiveJumpAlarmLevel { get; set; }

        [Display(Name = "负跳变报警")]
        public string NegativeJumpAlarm { get; set; }

        [Display(Name = "负跳变报警等级")]
        public string NegativeJumpAlarmLevel { get; set; }

        [Display(Name = "故障报警")]
        public string FaultAlarm { get; set; }

        [Display(Name = "故障报警等级")]
        public string FaultAlarmLevel { get; set; }

        [Display(Name = "故障处理")]
        public string FaultHandling { get; set; }

        [Display(Name = "位号分组")]
        public string TagGroup { get; set; }

        [Display(Name = "位号等级")]
        public string TagLevel { get; set; }

        [Display(Name = "SOE硬点标记")]
        public string SoeHardPointMarker { get; set; }

        [Display(Name = "SOE标志")]
        public string SoeFlag { get; set; }

        [Display(Name = "SOE描述")]
        public string SoeDescription { get; set; }

        [Display(Name = "SOE设备组")]
        public string SoeDeviceGroup { get; set; }
    }
    public class DO_生成位号表
    {
        [Display(Name = "序号")]
       public string SequenceNumber { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "控制站地址")]
        public string ControlStationAddress { get; set; }

        [Display(Name = "地址")]
        public string Address { get; set; }

        [Display(Name = "ON描述")]
        public string OnDescription { get; set; }

        [Display(Name = "OFF描述")]
        public string OffDescription { get; set; }

        [Display(Name = "位号类型")]
        public string TagType { get; set; }

        [Display(Name = "模块类型")]
        public string ModuleType { get; set; }

        [Display(Name = "故障安全模式")]
        public string FaultSafetyMode { get; set; }

        [Display(Name = "故障状态设定值")]
        public string FaultStateSetting { get; set; }

        [Display(Name = "位号运行周期")]
        public string TagOperatingCycle { get; set; }

        [Display(Name = "输出取反")]
        public string OutputReverse { get; set; }

        [Display(Name = "ON状态报警")]
        public string OnStateAlarm { get; set; }

        [Display(Name = "ON状态报警等级")]
        public string OnStateAlarmLevel { get; set; }

        [Display(Name = "OFF状态报警")]
        public string OffStateAlarm { get; set; }

        [Display(Name = "OFF状态报警等级")]
        public string OffStateAlarmLevel { get; set; }

        [Display(Name = "故障报警")]
        public string FaultAlarm { get; set; }

        [Display(Name = "故障报警等级")]
        public string FaultAlarmLevel { get; set; }

        [Display(Name = "位号分组")]
        public string TagGroup { get; set; }

        [Display(Name = "位号等级")]
        public string TagLevel { get; set; }

        [Display(Name = "SOE标志")]
        public string SoeFlag { get; set; }

        [Display(Name = "SOE描述")]
        public string SoeDescription { get; set; }

        [Display(Name = "SOE设备组")]
        public string SoeDeviceGroup { get; set; }
    }
    public class NA_生成位号表
    {
        [Display(Name = "序号")]
       public string SequenceNumber { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "控制站地址")]
        public string ControlStationAddress { get; set; }

        [Display(Name = "量程下限")]
        public double RangeLowerLimit { get; set; }

        [Display(Name = "量程上限")]
        public double RangeUpperLimit { get; set; }

        [Display(Name = "单位")]
        public string Unit { get; set; }

        [Display(Name = "初始值")]
        public double InitialValue { get; set; }

        [Display(Name = "位号分组")]
        public string TagGroup { get; set; }

        [Display(Name = "位号等级")]
        public string TagLevel { get; set; }

        [Display(Name = "小数位数")]
       public string DecimalPlaces { get; set; }
    }
    public class ND_生成位号表
    {
        [Display(Name = "序号")]
       public string SequenceNumber { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "控制站地址")]
        public string ControlStationAddress { get; set; }

        [Display(Name = "量程下限")]
        public double RangeLowerLimit { get; set; }

        [Display(Name = "量程上限")]
        public double RangeUpperLimit { get; set; }

        [Display(Name = "单位")]
        public string Unit { get; set; }

        [Display(Name = "整型类型")]
        public string IntegerType { get; set; }

        [Display(Name = "初始值")]
        public double InitialValue { get; set; }

        [Display(Name = "位号分组")]
        public string TagGroup { get; set; }

        [Display(Name = "位号等级")]
        public string TagLevel { get; set; }
    }
    public class NN_生成位号表
    {
        [Display(Name = "序号")]
       public string SequenceNumber { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "控制站地址")]
        public string ControlStationAddress { get; set; }

        [Display(Name = "初始值")]
        public double InitialValue { get; set; }
    }
    public class PA_生成位号表
    {
        [Display(Name = "序号")]
       public string SequenceNumber { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "控制站地址")]
        public string ControlStationAddress { get; set; }

        [Display(Name = "初始值")]
        public double InitialValue { get; set; }
    }
    public class PD_生成位号表
    {
        [Display(Name = "序号")]
       public string SequenceNumber { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "控制站地址")]
        public string ControlStationAddress { get; set; }

        [Display(Name = "整型类型")]
        public string IntegerType { get; set; }

        [Display(Name = "初始值")]
        public double InitialValue { get; set; }
    }
    public class PN_生成位号表
    {
        [Display(Name = "序号")]
       public string SequenceNumber { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "控制站地址")]
        public string ControlStationAddress { get; set; }

        [Display(Name = "功能块地址")]
        public string FunctionBlockAddress { get; set; }

        [Display(Name = "功能块类型")]
        public string FunctionBlockType { get; set; }

        [Display(Name = "功能块库ID")]
        public string FunctionBlockLibraryId { get; set; }

        [Display(Name = "模块ID")]
        public string ModuleId { get; set; }

        [Display(Name = "所属程序名")]
        public string ProgramName { get; set; }

        [Display(Name = "位号分组")]
        public string TagGroup { get; set; }

        [Display(Name = "位号等级")]
        public string TagLevel { get; set; }
    }
    public class FB_生成位号表
    {
        [Display(Name = "序号")]
       public string SequenceNumber { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "控制站地址")]
        public string ControlStationAddress { get; set; }

        [Display(Name = "功能块地址")]
        public string FunctionBlockAddress { get; set; }

        [Display(Name = "功能块类型")]
        public string FunctionBlockType { get; set; }

        [Display(Name = "功能块库ID")]
        public string FunctionBlockLibraryId { get; set; }

        [Display(Name = "模块ID")]
        public string ModuleId { get; set; }

        [Display(Name = "所属程序名")]
        public string ProgramName { get; set; }

        [Display(Name = "位号分组")]
        public string TagGroup { get; set; }

        [Display(Name = "位号等级")]
        public string TagLevel { get; set; }
    }
}
