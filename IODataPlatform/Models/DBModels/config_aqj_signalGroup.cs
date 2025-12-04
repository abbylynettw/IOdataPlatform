using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Models.DBModels
{
    /// <summary>
    /// 安全级信号组（设备）配置表
    ///</summary>
    [SugarTable("config_aqj_signalGroup")]
    public class config_aqj_signalGroup
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Display(Name = "Id", AutoGenerateField = false)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 信号组名称
        ///</summary>
        [Display(Name = "信号组名称")]
        public string signalGroupName { get; set; }       

        [Display(Name = "备注")]
        public string Note { get; set; } = string.Empty;

        [Display(Name = "创建日期")]
        public DateTime Creation { get; set; } = DateTime.Now;
    }

}
