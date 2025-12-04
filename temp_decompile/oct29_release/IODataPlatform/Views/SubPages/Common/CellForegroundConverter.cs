using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace IODataPlatform.Views.SubPages.Common;

/// <summary>
/// 单元格前景色转换器：修改的字段显示红色
/// </summary>
public class CellForegroundConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length < 2 || !(values[0] is CableComparisonRow cableComparisonRow) || !(parameter is string fieldName))
		{
			return Brushes.Black;
		}
		if (cableComparisonRow.Type == ChangeType.修改 && cableComparisonRow.IsFieldChanged(fieldName))
		{
			return Brushes.Red;
		}
		return Brushes.Black;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
