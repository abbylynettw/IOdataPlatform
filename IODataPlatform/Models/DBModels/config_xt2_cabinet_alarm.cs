using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.DBModels
{
    /// <summary>
    /// XT2控制系统机柜报警清单配置表
    /// 存储从Excel导入的机柜报警信息，用于后续导出机柜报警表时关联查询
    /// </summary>
    [SugarTable("config_xt2_cabinet_alarm")]
    public class config_xt2_cabinet_alarm
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Display(Name = "Id", AutoGenerateField = false)]
        public int Id { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        [Display(Name = "序号")]
        public int SerialNumber { get; set; }

        /// <summary>
        /// 机柜名称
        /// </summary>
        [Display(Name = "机柜名称")]
       
        public string CabinetName { get; set; }

        /// <summary>
        /// 机柜类型
        /// </summary>
        [Display(Name = "机柜类型")]
       
        public string CabinetType { get; set; }

        /// <summary>
        /// 房间号
        /// </summary>
        [Display(Name = "房间号")]
     
        public string Room { get; set; }

        /// <summary>
        /// 温度（AI点）数量
        /// </summary>
        [Display(Name = "温度（AI点）")]
        public int Temperature { get; set; }

        /// <summary>
        /// 综合报警（DI点）数量
        /// </summary>
        [Display(Name = "综合报警（DI点）")]
        public int ComprehensiveAlarm { get; set; }

        /// <summary>
        /// 机柜电源A故障数量
        /// </summary>
        [Display(Name = "机柜电源A故障")]
        public int PowerAFault { get; set; }

        /// <summary>
        /// 机柜电源报警B故障数量
        /// </summary>
        [Display(Name = "机柜电源报警B故障")]
        public int PowerBAlarmFault { get; set; }

        /// <summary>
        /// 机柜门开数量
        /// </summary>
        [Display(Name = "机柜门开")]
        public int DoorOpen { get; set; }

        /// <summary>
        /// 机柜温度高报警数量
        /// </summary>
        [Display(Name = "机柜温度高报警")]
        public int HighTemperatureAlarm { get; set; }

        /// <summary>
        /// 风扇故障数量
        /// </summary>
        [Display(Name = "风扇故障")]
        public int FanFault { get; set; }

        /// <summary>
        /// 网络故障数量
        /// </summary>
        [Display(Name = "网络故障")]
        public int NetworkFault { get; set; }

        /// <summary>
        /// RTU板卡位置（格式：1-0-04）
        /// </summary>
        [Display(Name = "RTU板卡位置")]
      
        public string RTUBoardPosition { get; set; }

        /// <summary>
        /// RTU板卡所在机柜号
        /// </summary>
        [Display(Name = "RTU板卡所在机柜号")]
     
        public string RTUCabinetNumber { get; set; }

       
    }
}
