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
    /// 电缆型号表
    ///</summary>
    [SugarTable("config_cable_spec")]
    public class config_cable_spec
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Display(Name = "Id", AutoGenerateField = false)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 接线点数 
        ///</summary>
        [SugarColumn(ColumnName = "ConnectCount")]
        [Display(Name = "接线点数")]
        public int ConnectCount { get; set; }
        /// <summary>
        /// 电缆类型 
        ///</summary>
        [SugarColumn(ColumnName = "CableType")]
        [Display(Name = "电缆类型")]
        public string CableType { get; set; }
        /// <summary>
        /// 电缆规格 
        ///</summary>
        [SugarColumn(ColumnName = "CableSpecQuantity")]
        [Display(Name = "电缆规格")]
        public string CableSpecQuantity { get; set; }
        /// <summary>
        /// 特性代码 
        ///</summary>
        [SugarColumn(ColumnName = "FeatureCode")]
        [Display(Name = "特性代码")]
        public string FeatureCode { get; set; }

    }
}
