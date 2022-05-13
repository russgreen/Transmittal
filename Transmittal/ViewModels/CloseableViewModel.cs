using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transmittal.ViewModels;
public abstract class CloseableViewModel
{
    public event EventHandler ClosingRequest;

    protected void OnClosingRequest()
    {
        if (this.ClosingRequest != null)
        {
            this.ClosingRequest(this, EventArgs.Empty);
        }
    }
}
