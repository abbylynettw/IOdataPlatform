using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Models.DBModels
{
    [SugarTable("config_aqj_stationCabinet")]
    public class config_aqj_stationCabinet
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Display(Name = "Id", AutoGenerateField = false)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 控制站号
        /// </summary>
        [Display(Name = "控制站号")]
        public string ControlStationNumber { get; set; }

        /// <summary>
        /// 机柜编号
        /// </summary>
        [Display(Name = "机柜编号")]
        public string CabinetNumber { get; set; }


    }
}