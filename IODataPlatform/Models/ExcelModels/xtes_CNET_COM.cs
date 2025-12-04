using System.ComponentModel.DataAnnotations;

public class xtes_CNET_COM
{
    [Display(Name = "PN - 点名")]
    public string PN { get; set; }

    [Display(Name = "DESC - 点描述")]
    public string DESC { get; set; }

    [Display(Name = "DATATYPE - 数据类型")]
    public string DATATYPE { get; set; }

    [Display(Name = "OFFSET - 地址偏移")]
    public string OFFSET { get; set; }

    [Display(Name = "OUTPUTMODE - 输出替代方式")]
    public string OUTPUTMODE { get; set; }

    [Display(Name = "OUTPUTVALUE - 输出替代值")]
    public string OUTPUTVALUE { get; set; }
}
