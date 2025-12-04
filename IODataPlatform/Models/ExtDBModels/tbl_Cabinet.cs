using System;
using System.Linq;
using System.Text;

using SqlSugar;
namespace IODataPlatform.Models.ExtDBModels
{
    ///<summary>
    ///
    ///</summary>
    public partial class tbl_Cabinet
    {
        public tbl_Cabinet()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, ColumnName = "Cabinet")]
        public string Cabinet { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "Cabinet_Destination",IsIgnore =true)]
        public string Cabinet_Destination { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "Cabinet_Type")]
        public string Cabinet_Type { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "Room")]
        public string Room { get; set; }

    }
}
