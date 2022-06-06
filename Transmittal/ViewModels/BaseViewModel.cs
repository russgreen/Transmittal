using CommunityToolkit.Mvvm.ComponentModel;

namespace Transmittal.ViewModels;

public abstract class BaseViewModel : ObservableObject
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
