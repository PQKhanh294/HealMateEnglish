using System;
using System.Globalization;
using System.Windows.Data;

namespace HealMateEnglish.Converters
{
    /// <summary>
    /// Converts a percentage value (0-100) to a proportional width for progress bar visualization
    /// </summary>
    public class PercentageToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage)
            {
                // Ensure percentage is between 0 and 100
                percentage = Math.Max(0, Math.Min(100, percentage));

                // Return as a percentage string for use with Width property
                return $"{percentage}%";
            }

            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
