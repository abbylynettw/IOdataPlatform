using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IODataPlatform.Models.ExcelModels
{
    public class xtes_AM_FF
    {
        [Display(Name = "PN - 点名")]
        public string PN { get; set; }

        [Display(Name = "DESC - 点描述")]
        public string DESC { get; set; }

        [Display(Name = "UNIT - 单位")]
        public string UNIT { get; set; }

        [Display(Name = "MU - 量程上限")]
        public string MU { get; set; }

        [Display(Name = "MD - 量程下限")]
        public string MD { get; set; }

        [Display(Name = "ALLOCATION - 报警分组")]
        public int ALLOCATION { get; set; }

        [Display(Name = "TRAIN - 安全列")]
        public string TRAIN { get; set; }

        [Display(Name = "RG - 关联画面")]
        public string RG { get; set; }

        [Display(Name = "IH - 是否进历史库")]
        public int IH { get; set; }

        [Display(Name = "SYS - 系统名")]
        public string SYS { get; set; }

        [Display(Name = "SUBNET - 子网")]
        public string SUBNET { get; set; }

        [Display(Name = "SN - 站号")]
        public string SN { get; set; }

        [Display(Name = "OF - 显示格式")]
        public string OF { get; set; }

        [Display(Name = "QFM - 输出信号替代方式")]
        public int QFM { get; set; }

        [Display(Name = "QFIA - 输出替代值")]
        public int QFIA { get; set; }

        [Display(Name = "SD - 超量程恢复死区")]
        public double SD { get; set; }

        [Display(Name = "OLQ - 超量程是否影响信号质量")]
        public int OLQ { get; set; }

        [Display(Name = "OEL - 超量程限值%")]
        public int OEL { get; set; }

        [Display(Name = "OLT - 超量程判断选择")]
        public int OLT { get; set; }

        [Display(Name = "ACUT - 一层是否报警切除")]
        public int ACUT { get; set; }

        [Display(Name = "INHIBIT - 报警冗余自动抑制")]
        public int INHIBIT { get; set; }

        [Display(Name = "DSEL - 超限死区设置方式")]
        public int DSEL { get; set; }

        [Display(Name = "DI - 阈值恢复死区(%)")]
        public int DI { get; set; }

        [Display(Name = "H4AP - 是否判断高4限越限")]
        public int H4AP { get; set; }

        [Display(Name = "H4 - 高4限阈值")]
        public double H4 { get; set; }

        [Display(Name = "H4LEVEL - 高4限报警级")]
        public int H4LEVEL { get; set; }

        [Display(Name = "H4KA - 高4限报警标签")]
        public string H4KA { get; set; }

        [Display(Name = "H4_DESC - 高4报警描述")]
        public string H4_DESC { get; set; }

        [Display(Name = "H4DEC - H4是否为DEC大修报警")]
        public int H4DEC { get; set; }

        [Display(Name = "H4SI - H4是否为安注报警")]
        public int H4SI { get; set; }

        [Display(Name = "H4DL - 高4限死区宽度")]
        public int H4DL { get; set; }

        [Display(Name = "H3AP - 是否判断高3限越限")]
        public int H3AP { get; set; }

        [Display(Name = "H3 - 高3限阈值")]
        public double H3 { get; set; }

        [Display(Name = "H3LEVEL - 高3限报警级")]
        public int H3LEVEL { get; set; }

        [Display(Name = "H3KA - 高3限报警标签")]
        public string H3KA { get; set; }

        [Display(Name = "H3_DESC - 高3报警描述")]
        public string H3_DESC { get; set; }

        [Display(Name = "H3DEC - H3是否为DEC大修报警")]
        public int H3DEC { get; set; }

        [Display(Name = "H3SI - H3是否为安注报警")]
        public int H3SI { get; set; }

        [Display(Name = "H3DL - 高3限死区宽度")]
        public int H3DL { get; set; }

        [Display(Name = "H2AP - 是否判断高2限越限")]
        public int H2AP { get; set; }

        [Display(Name = "H2 - 高2限阈值")]
        public double H2 { get; set; }

        [Display(Name = "H2LEVEL - 高2限报警级")]
        public int H2LEVEL { get; set; }

        [Display(Name = "H2KA - 高2限报警标签")]
        public string H2KA { get; set; }

        [Display(Name = "H2_DESC - 高2报警描述")]
        public string H2_DESC { get; set; }

        [Display(Name = "H2DEC - H2是否为DEC大修报警")]
        public int H2DEC { get; set; }

        [Display(Name = "H2SI - H2是否为安注报警")]
        public int H2SI { get; set; }

        [Display(Name = "H2DL - 高2限死区宽度")]
        public int H2DL { get; set; }

        [Display(Name = "H1AP - 是否判断高1限越限")]
        public int H1AP { get; set; }

        [Display(Name = "H1 - 高1限阈值")]
        public double H1 { get; set; }

        [Display(Name = "H1LEVEL - 高1限报警级")]
        public int H1LEVEL { get; set; }

        [Display(Name = "H1KA - 高1限报警标签")]
        public string H1KA { get; set; }

        [Display(Name = "H1_DESC - 高1报警描述")]
        public string H1_DESC { get; set; }

        [Display(Name = "H1DEC - H1是否为DEC大修报警")]
        public int H1DEC { get; set; }

        [Display(Name = "H1SI - H1是否为安注报警")]
        public int H1SI { get; set; }

        [Display(Name = "H1DL - 高1限死区宽度")]
        public int H1DL { get; set; }

        [Display(Name = "L1AP - 是否判断低1限越限")]
        public int L1AP { get; set; }

        [Display(Name = "L1 - 低1限阈值")]
        public double L1 { get; set; }

        [Display(Name = "L1LEVEL - 低1限报警级")]
        public int L1LEVEL { get; set; }

        [Display(Name = "L1KA - 低1限报警标签")]
        public string L1KA { get; set; }

        [Display(Name = "L1_DESC - 低1报警描述")]
        public string L1_DESC { get; set; }

        [Display(Name = "L1DEC - L1是否为DEC大修报警")]
        public int L1DEC { get; set; }

        [Display(Name = "L1SI - L1是否为安注报警")]
        public int L1SI { get; set; }

        [Display(Name = "L1DL - 低1限死区宽度")]
        public int L1DL { get; set; }

        [Display(Name = "L2AP - 是否判断低2限越限")]
        public int L2AP { get; set; }

        [Display(Name = "L2 - 低2限阈值")]
        public double L2 { get; set; }

        [Display(Name = "L2LEVEL - 低2限报警级")]
        public int L2LEVEL { get; set; }

        [Display(Name = "L2KA - 低2限报警标签")]
        public string L2KA { get; set; }

        [Display(Name = "L2_DESC - 低2报警描述")]
        public string L2_DESC { get; set; }

        [Display(Name = "L2DEC - L2是否为DEC大修报警")]
        public int L2DEC { get; set; }

        [Display(Name = "L2SI - L2是否为安注报警")]
        public int L2SI { get; set; }

        [Display(Name = "L2DL - 低2限死区宽度")]
        public int L2DL { get; set; }

        [Display(Name = "L3AP - 是否判断低3限越限")]
        public int L3AP { get; set; }

        [Display(Name = "L3 - 低3限阈值")]
        public float L3 { get; set; }

        [Display(Name = "L3LEVEL - 低3限报警级")]
        public int L3LEVEL { get; set; }

        [Display(Name = "L3KA - 低3限报警标签")]
        public string L3KA { get; set; }

        [Display(Name = "L3_DESC - 低3报警描述")]
        public string L3_DESC { get; set; }

        [Display(Name = "L3DEC - L3是否为DEC大修报警")]
        public int L3DEC { get; set; }

        [Display(Name = "L3SI - L3是否为安注报警")]
        public int L3SI { get; set; }

        [Display(Name = "L3DL - 低3限死区宽度")]
        public int L3DL { get; set; }

        [Display(Name = "L4AP - 是否判断低4限越限")]
        public int L4AP { get; set; }

        [Display(Name = "L4 - 低4限阈值")]
        public double L4 { get; set; }

        [Display(Name = "L4LEVEL - 低4限报警级")]
        public int L4LEVEL { get; set; }

        [Display(Name = "L4KA - 低4限报警标签")]
        public string L4KA { get; set; }

        [Display(Name = "L4_DESC - 低4报警描述")]
        public string L4_DESC { get; set; }

        [Display(Name = "L4DEC - L4是否为DEC大修报警")]
        public int L4DEC { get; set; }

        [Display(Name = "L4SI - L4是否为安注报警")]
        public int L4SI { get; set; }

        [Display(Name = "L4DL - 低4限死区宽度")]
        public int L4DL { get; set; }
    }
}
