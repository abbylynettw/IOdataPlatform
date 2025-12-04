using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using IODataPlatform.Utilities;

namespace IODataPlatform.Views.SubPages.Common;

/// <summary>
/// 单元格前景色转换器：修改的字段显示红色
/// </summary>
public class GenericDataCellForegroundConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length >= 2 && values[0] is GenericComparisonRow genericComparisonRow)
		{
			string fieldName = parameter as string;
			if (fieldName != null)
			{
				if (genericComparisonRow.Type == GenericDataChangeType.修改 && genericComparisonRow.DiffProps.Any((DifferentProperty p) => p.PropName == fieldName))
				{
					return Brushes.Red;
				}
				return Brushes.Black;
			}
		}
		return Brushes.Black;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
