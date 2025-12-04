using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Models.ExcelModels
{
    public  class 系统二室网络设备IP表
    {
        public string 序号 { get; set; }
        public string A_Start_名称 { get; set; }
        public string A_Start_机柜或盘台 { get; set; }
        public string A_Start_位号 { get; set; }
        public string A_Start_端口 { get; set; }

        public string A_End_名称 { get; set; }
        public string A_End_机柜或盘台 { get; set; }
        public string A_End_位号 { get; set; }
        public string A_End_端口 { get; set; }


        public string B_Start_名称 { get; set; }
        public string B_Start_机柜或盘台 { get; set; }
        public string B_Start_位号 { get; set; }
        public string B_Start_端口 { get; set; }

        public string B_End_名称 { get; set; }
        public string B_End_机柜或盘台 { get; set; }
        public string B_End_位号 { get; set; }
        public string B_End_端口 { get; set; }

        public string 接口类型 { get; set; }

        public bool IsDeletedRow { get; set; }
    }

    public class 系统二室机柜信息表
    {
        public string 盘柜位号 { get; set; }

        public string 房间号 { get; set; }
    }
}
