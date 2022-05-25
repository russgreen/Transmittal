using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Transmittal.Converters;
internal class BrushColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value == true)
        {
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);

        }
        
        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 171, 173, 179));
        
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
