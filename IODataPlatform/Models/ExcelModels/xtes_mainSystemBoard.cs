using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Models.ExcelModels
{
    /// <summary>
    /// 主系统板卡清单
    /// </summary>
    public class xtes_mainsystemboard_list
    {
        public string 机柜编号 { get; set; }
        public int 机架号 { get; set; }
        public int 插槽 { get; set; }
        public string 地址 { get; set; }
        public string 板卡类型 { get; set; }
        public string 板卡编号 { get; set; }
        public string 端子板类型 { get; set; }
       
        public string 端子板编号 { get; set; }
        public string 描述 { get; set; }
        public string 备注 { get; set; }
    }
}
