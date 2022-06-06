using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Transmittal.Desktop.Converters;
internal class AddNewRowPositionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value == true)
        {
            return Syncfusion.UI.Xaml.Grid.AddNewRowPosition.Bottom;
        }

        return Syncfusion.UI.Xaml.Grid.AddNewRowPosition.None;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
