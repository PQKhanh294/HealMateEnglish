using System;
using System.Globalization;
using System.Windows.Data;

namespace HealMateEnglish.Converters
{
    public class BooleanToTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMultipleChoice)
            {
                return isMultipleChoice
                    ? "Multiple answers may be correct. Select all that apply."
                    : "Choose the single correct answer.";
            }
            return "Select an answer";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
