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
    /// 安全级模拟量
    ///</summary>
    [SugarTable("config_aqj_analog")]
    public class config_aqj_analog
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Display(Name = "Id", AutoGenerateField = false)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 设备类型 
        ///</summary>
        [Display(Name = "设备类型")]
        public string DeviceType { get; set; }

        /// <summary>
        /// 原理图类型 
        ///</summary>       
        [Display(Name = "原理图类型")]
        public string SchematicType { get; set; }

        /// <summary>
        /// SH 
        ///</summary>      
        [Display(Name = "SH")]
        public bool SH { get; set; }
        /// <summary>
        /// AH 
        ///</summary>      
        [Display(Name = "AH")]
        public bool AH { get; set; }

        /// <summary>
        /// AHH 
        ///</summary>      
        [Display(Name = "AHH")]
        public bool AHH { get; set; }

        /// <summary>
        /// AL 
        ///</summary>      
        [Display(Name = "AL")]
        public bool AL { get; set; }

        /// <summary>
        /// ALL 
        ///</summary>      
        [Display(Name = "ALL")]
        public bool ALL { get; set; }

        /// <summary>
        /// SAH 
        ///</summary>      
        [Display(Name = "SAH")]
        public bool SAH { get; set; }

        /// <summary>
        /// SAL 
        ///</summary>      
        [Display(Name = "SAL")]
        public bool SAL { get; set; }

        /// <summary>
        /// 是否包含"R" 
        ///</summary>      
        [Display(Name = "是否包含R")]
        public bool IsContainsR { get; set; }

    }
}
