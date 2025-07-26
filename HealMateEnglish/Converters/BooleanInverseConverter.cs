using System;
using System.Globalization;
using System.Windows.Data;

namespace HealMateEnglish.Converters
{
    /// <summary>
    /// Converter that inverts a boolean value (true becomes false, false becomes true)
    /// Supports two-way binding for RadioButton IsChecked properties
    /// </summary>
    public class BooleanInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }
}
