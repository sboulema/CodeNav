using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CodeNav.Converters
{
    public class WidthMinusMarginConverter : IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {                
                return System.Convert.ToDouble(value) - System.Convert.ToDouble(parameter);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var itemsControlWidth = System.Convert.ToDouble(values[1]);
            var expanderWidth = values[0];

            if (expanderWidth == DependencyProperty.UnsetValue)
            {
                return itemsControlWidth - 4;
            }

            return System.Convert.ToDouble(expanderWidth) - System.Convert.ToDouble(parameter);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
