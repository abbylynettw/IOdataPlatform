using System.Globalization;
using System.Windows.Data;

namespace IODataPlatform.Views.Pages;

partial class CategoryToVisibilityConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return $"{value}" == "电缆" ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}