
using SqlSugar;

namespace IODataPlatform.Models.ExtDBModels
{
    public class tbl_端接查询
    {
        public int? 编号 { get; set; }

        [SugarColumn(ColumnName = "I/O点名")]
        public string I_O点名 { get; set; }
        public string 机组号 { get; set; }
        public string LOT { get; set; }
        public string Batch { get; set; }
        public string 控制器站名 { get; set; }
        public string 房间号 { get; set; }
        public string 机柜名 { get; set; }

        [SugarColumn(ColumnName = "I/O模块型号")]
        public string I_O模块型号 { get; set; }
        [SugarColumn(ColumnName = "I/O模块编号")]
        public string I_O模块编号 { get; set; }
        public string IO_Module { get; set; }
        public string 通道号 { get; set; }
        public string 接线板编号 { get; set; }
        public string 隔离 { get; set; }
        public string 接线点1 { get; set; }
        public string 信号说明1 { get; set; }
        public string 接线点2 { get; set; }
        public string 信号说明2 { get; set; }
        public string 接线点3 { get; set; }
        public string 信号说明3 { get; set; }
        public string 接线点4 { get; set; }
        public string 信号说明4 { get; set; }
        public string 接线点5 { get; set; }
        public string 信号说明5 { get; set; }
        public string 接线点6 { get; set; }
        public string 信号说明6 { get; set; }
        [SugarColumn(ColumnName = "I/O类型")]
        public string I_O类型 { get; set; }
        public string 供电类型 { get; set; }
        public string 回路电压 { get; set; }
        public string 触点容量 { get; set; }
        public string 传感器类型 { get; set; }
        public string 信号特性 { get; set; }
        public string 信号终点 { get; set; }
        [SugarColumn(ColumnName = "内/外部点")]
        public string 内_外部点 { get; set; }
        public string 典型回路图 { get; set; }
        public string SA供电 { get; set; }
        public string EES供电 { get; set; }
        //public string SA供电 { get; set; }
        public string 系统名 { get; set; }
        [SugarColumn(ColumnName = "I_O清单类别")]
        public string I_O清单类别 { get; set; }
        public string 信号说明 { get; set; }
        public string 安全分级 { get; set; }
        public string 功能分级 { get; set; }

        [SugarColumn(ColumnName = "事故追忆/瞬态记录")]
        public string 事故追忆_瞬态记录 { get; set; }
        public string 电器接口类型 { get; set; }
        public string 图号 { get; set; }
        public string 传递单号 { get; set; }
        public string 抗震类别 { get; set; }
        public string 备注 { get; set; }


        public string IO卡件 { get; set; }
        public string? 序号 { get; set; }
        public string 信号位号 { get; set; }
        public string 扩展码 { get; set; }
        public string 测量单位 { get; set; }
        public string 测量下限 { get; set; }
        public string 测量上限 { get; set; }
        public string 缺省值 { get; set; }
        public string 负载信息 { get; set; }
        public string 版本 { get; set; }
        public string 电气接口 { get; set; }
        public string 安全等级 { get; set; }
        public string 电源列 { get; set; }
        public string 设备 { get; set; }
        public string 柜内供电方式 { get; set; }
        public string 触点类型 { get; set; }
        public string 修改说明 { get; set; }
    }
}
