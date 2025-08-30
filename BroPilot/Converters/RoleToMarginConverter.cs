using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BroPilot.Converters
{
    public class RoleToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string role = value as string;
            return role == "user" ? new Thickness(0, 0,20,0) : new Thickness(20,0,0,0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
