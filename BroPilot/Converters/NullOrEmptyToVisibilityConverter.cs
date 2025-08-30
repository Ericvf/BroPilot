using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BroPilot.Converters
{
    public class NullOrEmptyToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; } 

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNullOrEmpty = string.IsNullOrEmpty(value as string);
            bool invert = parameter?.ToString() == "Invert";
            return (isNullOrEmpty ^ invert) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
