using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Models.ExcelModels
{
    public class FF从战箱端接表
    {
        public int SerialNumber { get; set; } // 序号

        public string LocalBoxNumber { get; set; } = string.Empty; // 就地箱位号

        public string TerminalBlockNumber { get; set; } = string.Empty; // 端子排编号

        public string Terminal { get; set; } = string.Empty; // 端子

        public string SignalFunction { get; set; } = string.Empty; // 信号功能

        public string SignalPositionNumber { get; set; } = string.Empty; // 信号位号



        public string CableSpecification { get; set; } = string.Empty; // 电缆型号及规范

        public string CableNumber { get; set; } = string.Empty; // 电缆编号
        public string CableDestination { get; set; } = string.Empty; // 电缆去向（标签名称）

        public string Version { get; set; } = string.Empty; // 版本

        public string Remarks { get; set; } = string.Empty; // 备注
    }

    public class FF总线箱端接表
    {
        public int SerialNumber { get; set; } // 序号

        public string LocalBoxNumber { get; set; } = string.Empty; // 就地箱位号

        public string TerminalBlockNumber { get; set; } = string.Empty; // 端子排编号

        public string Terminal { get; set; } = string.Empty; // 端子

        public string SignalFunction { get; set; } = string.Empty; // 端子描述

        public string SignalPositionNumber { get; set; } = string.Empty; // 电缆芯端接段子



        public string CableSpecification { get; set; } = string.Empty; // 电缆型号及规范

        public string CableNumber { get; set; } = string.Empty; // 电缆编号
        public string CableDestination { get; set; } = string.Empty; // 电缆去向（标签名称）

        public string Version { get; set; } = string.Empty; // 版本

        public string Remarks { get; set; } = string.Empty; // 备注
    }

}
