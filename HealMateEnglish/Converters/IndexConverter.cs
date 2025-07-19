using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace HealMateEnglish.Converters
{
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var itemContainer = value as DependencyObject;
            if (itemContainer == null)
                return DependencyProperty.UnsetValue;

            var itemsControl = ItemsControl.ItemsControlFromItemContainer(itemContainer);
            if (itemsControl == null)
                return DependencyProperty.UnsetValue;

            int index = itemsControl.ItemContainerGenerator.IndexFromContainer(itemContainer);
            return (index + 1).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
