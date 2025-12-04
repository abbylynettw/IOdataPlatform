using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Models.DBModels
{    /// <summary>
     /// 安全级数字量配置表
     ///</summary>
    public class config_aqj_control
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Display(Name = "Id", AutoGenerateField = false)]
        public int Id { get; set; } = 0;
        /// <summary> 设备类型 /// </summary>
        [Display(Name = "设备类型")]
        public string DeviceType { get; set; }

        /// <summary> 原理图类型 /// </summary>
        [Display(Name = "原理图类型")]
        public string SchematicType { get; set; }

        /// <summary> IO类型DO个数 </summary>
        [Display(Name = "DO个数")]
        public int IOTypeDONumber { get; set; }

        /// <summary> IO类型DI个数 </summary>
        [Display(Name = "DI个数")]
        public int IOTypeDINumber { get; set; }

        /// <summary> 功能描述 </summary>
        [Display(Name = "功能描述关键字")]
        public string FunctionDescription { get; set; }
    }
}
