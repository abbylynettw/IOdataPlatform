using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Models.ExcelModels
{
        public class xtes_board_statistics
        {
            public int 序号 { get; set; }
            public string 板卡 { get; set; }
            public int 板卡数量 { get; set; }
            public string 连接板 { get; set; }
            public int 连接板数量 { get; set; }
            public string 端子板 { get; set; }
            public int 端子板数量 { get; set; }
        }
}
