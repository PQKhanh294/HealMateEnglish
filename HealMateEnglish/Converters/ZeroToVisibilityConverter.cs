using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HealMateEnglish.Converters
{
    public class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // When the count is zero, show the "no items" message
            if (value is int intValue && intValue == 0)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
