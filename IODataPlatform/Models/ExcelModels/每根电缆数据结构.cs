using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Models.ExcelModels
{
    public class 每根电缆
    {
        public 电缆类型 控制电缆 { get; set; }

        public List<CableData> Signals { get; set; }//信号数

        


    }

    public enum 电缆类型
    {
        控制电缆,
        测量电缆
    }
}
