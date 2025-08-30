using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BroPilot.Converters
{
    public class ZeroToVisibilityConverter : IValueConverter
    {
        public Visibility ZeroValue { get; set; } = Visibility.Collapsed;
        public Visibility Value { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is int intValue && intValue == 0) ? ZeroValue : Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
