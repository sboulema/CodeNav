using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CodeNav.Properties;

namespace CodeNav.Converters
{
    public class WidthMinusMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {            
            var expanderWidth = values[0];
            var itemsControlWidth = System.Convert.ToDouble(values[1]);

            var marginWidth = Settings.Default.Width;

            if (expanderWidth == DependencyProperty.UnsetValue)
            {
                if (itemsControlWidth == marginWidth) return itemsControlWidth - 10;
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
