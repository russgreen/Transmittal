using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Transmittal.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    internal class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool?)value == false ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
