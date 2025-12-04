using System;
using System.Linq;
using System.Text;

using SqlSugar;
namespace IODataPlatform.Models.ExtDBModels
{
    ///<summary>
    ///
    ///</summary>
    public partial class tbl_IO_Module
    {
        public tbl_IO_Module()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "Address")]
        public string Address { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, ColumnName = "IO_Module")]
        public string IO_Module { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "Note")]
        public string Note { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "Rack")]
        public string Rack { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "Type")]
        public string Type { get; set; }

    }
}
