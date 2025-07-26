using System;
using System.Globalization;
using System.Windows.Data;

namespace HealMateEnglish.Converters
{
    public class BooleanToShowHideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Hide" : "Show";
            }
            return "Show";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
