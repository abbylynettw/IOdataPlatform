using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace IODataPlatform.Views.Pages;

public class BoolToBrushConverter : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if ((bool)value) {
            return Application.Current.Resources["TextFillColorPrimaryBrush"];
        } else {
            return Brushes.Red;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }

}

public class RateToBrushConverter : IMultiValueConverter {

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        var rate = (int)(double)values[0];
        var comparisonValue = (int)values[1];

        if (rate >= comparisonValue) {
            return (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        } else {
            return Brushes.Red;
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }

}