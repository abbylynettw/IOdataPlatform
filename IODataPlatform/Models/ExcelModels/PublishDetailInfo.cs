using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Models.ExcelModels
{
    public class PublishDetailInfo
    {
        public string Version { get; set; }

        public string Reason { get; set; }

        public string Publisher { get; set; }

        public DateTime PublishTime { get; set; }
    }
}
