using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Models.ExcelModels
{
    public class xtys_pdf_control_io : pdf_control_io
    {
        public override string 序号 { get; set; } = string.Empty;
        public override string 信号位号 { get; set; } = string.Empty;
        public override string 扩展码 { get; set; } = string.Empty;
        public override string 信号说明 { get; set; } = string.Empty;
        public override string 安全分级分组 { get; set; } = string.Empty;
        public override string 功能分级 { get; set; } = string.Empty;
        public override string 抗震类别 { get; set; } = string.Empty;
        public override string IO类型 { get; set; } = string.Empty;
        public override string 信号特性 { get; set; } = string.Empty;
        public override string 供电方 { get; set; } = string.Empty;
        public override string 测量单位 { get; set; } = string.Empty;
        public override string 量程下限 { get; set; } = string.Empty;
        public override string 量程上限 { get; set; } = string.Empty;
        public override string 缺省值 { get; set; } = string.Empty;
        public override string SOETRA { get; set; } = string.Empty;
        public override string 负载信息 { get; set; } = string.Empty;
        public override string 图号 { get; set; } = string.Empty;
        public override string 备注 { get; set; } = string.Empty;
        public override string 版本 { get; set; } = string.Empty;
    }


}
