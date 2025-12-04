using System.Collections.Generic;
using System.Data;

namespace IODataPlatform.Views.SubPages.Common;

/// <summary>
/// DataTable扩展方法
/// </summary>
public static class DataTableExtensions
{
	public static List<DynamicObject> StringTableToDynamicObjects(this DataTable table, string keyField)
	{
		List<DynamicObject> list = new List<DynamicObject>();
		foreach (DataRow row in table.Rows)
		{
			DynamicObject dynamicObject = new DynamicObject
			{
				Key = (row[keyField]?.ToString() ?? "")
			};
			foreach (DataColumn column in table.Columns)
			{
				dynamicObject.Properties[column.ColumnName] = row[column] ?? "";
			}
			list.Add(dynamicObject);
		}
		return list;
	}
}
