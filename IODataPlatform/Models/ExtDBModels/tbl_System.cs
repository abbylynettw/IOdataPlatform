using SqlSugar;

namespace IODataPlatform.Models.ExtDBModels
{
    public class tbl_System
    {
        public string System { get; set; }
        public string Ref_System { get; set; }
        public string Institute { get; set; }
        public string Chinese_Name { get; set; }
        public string English_Name { get; set; }
        public string LOT { get; set; }
        public string Batch { get; set; }
        public string Category { get; set; }
        public string Function { get; set; }
        public string Function_Description { get; set; }

        [SugarColumn(ColumnName = "Scope-Specification")]
        public string Scope_Specification { get; set; }
        public string Scope { get; set; }
        public string Designer { get; set; }
        public string 发货批次 { get; set; }
    }
}
