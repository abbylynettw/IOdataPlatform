using System.ComponentModel.DataAnnotations;

namespace IODataPlatform.Models.ExcelModels
{
    /// <summary>
    /// 机柜报警表导出模型
    /// 用于导出XT2系统的机柜报警连接清单
    /// </summary>
    public class CabinetAlarmTable
    {
        /// <summary>
        /// RTU板卡所在机柜号
        /// </summary>
        [Display(Name = "RTU板卡所在机柜号", Order = 1)]
        public string RTUCabinetNumber { get; set; }

        /// <summary>
        /// RTU板卡位置（板卡编号）
        /// </summary>
        [Display(Name = "RTU板卡位置", Order = 2)]
        public string RTUBoardPosition { get; set; }

        /// <summary>
        /// RTU板卡地址
        /// </summary>
        [Display(Name = "RTU板卡地址", Order = 3)]
        public string RTUBoardAddress { get; set; }

        /// <summary>
        /// 端口号（COM0/COM1）
        /// </summary>
        [Display(Name = "端口号", Order = 4)]
        public string PortNumber { get; set; }

        /// <summary>
        /// 从站设备（机柜报警模块）
        /// </summary>
        [Display(Name = "从站设备（机柜报警模块）", Order = 5)]
        public string SlaveDevice { get; set; }

        /// <summary>
        /// 从站地址
        /// </summary>
        [Display(Name = "从站地址", Order = 6)]
        public string SlaveAddress { get; set; }

        /// <summary>
        /// 串接顺序
        /// </summary>
        [Display(Name = "串接顺序", Order = 7)]
        public string CascadeOrder { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        [Display(Name = "版本", Order = 8)]
        public string Version { get; set; }

        /// <summary>
        /// 修改记录
        /// </summary>
        [Display(Name = "修改记录", Order = 9)]
        public string ModificationRecord { get; set; }

        /// <summary>
        /// 修改日期
        /// </summary>
        [Display(Name = "修改日期", Order = 10)]
        public string ModificationDate { get; set; }

        /// <summary>
        /// 房间号
        /// </summary>
        [Display(Name = "房间号", Order = 11)]
        public string Room { get; set; }
    }
}
