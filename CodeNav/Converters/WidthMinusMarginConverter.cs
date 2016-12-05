using System;
using System.Windows.Data;

namespace CodeNav.Converters
{
    public class WidthMinusMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
            {                
                return System.Convert.ToDouble(value) - System.Convert.ToDouble(parameter);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
