using Syncfusion.UI.Xaml.Grid;
using System.Windows.Input;

namespace Transmittal.Desktop.Controllers;
internal class GridCellSelectionControllerExt : GridCellSelectionController
{
    public GridCellSelectionControllerExt(SfDataGrid grid) : base(grid)
    {
    }

    protected override void ProcessKeyDown(KeyEventArgs args)
    {
        if (args.Key == Key.Enter)
        {
            args.Handled = false;
            return;
        }
        base.ProcessKeyDown(args);
    }
}
