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
    /// 控制系统类别配置
    /// </summary>
    [SugarTable("config_controlSystem_mapping")]
    public class config_controlSystem_mapping
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Display(Name = "Id", AutoGenerateField = false)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 类别
        /// </summary>
        [Display(Name = "类别")]
        public string Category { get; set; }

        /// <summary>
        /// 标准化字段
        /// </summary>
        [Display(Name = "标准化字段")]
        public string StdField { get; set; }

        /// <summary>
        /// 龙鳍曾用名
        /// </summary>
        [Display(Name = "龙鳍曾用名")]
        public string LqOld { get; set; }

        /// <summary>
        /// 中控曾用名
        /// </summary>
        [Display(Name = "中控曾用名")]
        public string ZkOld { get; set; }

        /// <summary>
        /// 龙核曾用名
        /// </summary>
        [Display(Name = "龙核曾用名")]
        public string LhOld { get; set; }

        /// <summary>
        /// 系统一室曾用名
        /// </summary>
        [Display(Name = "系统一曾用名")]
        public string Xt1Old { get; set; }


        /// <summary>
        /// 模拟控制系统曾用名
        /// </summary>
        [Display(Name = "模拟控制系统曾用名")]
        public string AQJMNOld { get; set; }
    }


}
