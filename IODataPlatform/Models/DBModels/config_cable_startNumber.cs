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
    /// 电缆类别和颜色
    ///</summary>
    [SugarTable("config_cable_startNumber")]
    public class config_cable_startNumber
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Display(Name = "Id", AutoGenerateField = false)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 起点盘箱柜号 
        ///</summary>
        [SugarColumn(ColumnName = "StartCabinetNo")]
        [Display(Name = "起点盘箱柜号")]
        public string StartCabinetNo { get; set; }
        /// <summary>
        /// 序列 
        ///</summary>
        [SugarColumn(ColumnName = "Sequence")]
        [Display(Name = "序列")]
        public string Sequence { get; set; }
        /// <summary>
        /// 起始流水号 
        ///</summary>
        [SugarColumn(ColumnName = "StartNumber")]
        [Display(Name = "起始流水号")]
        public int StartNumber { get; set; }
        /// <summary>
        /// 最大流水号 
        ///</summary>
        [SugarColumn(ColumnName = "MaxNumber")]
        [Display(Name = "最大流水号")]
        public int MaxNumber { get; set; }
    }
}
