using System.ComponentModel;

namespace Transmittal.Models;
/// <summary>
/// A base class to inherit in other models that need to implelement INotifyPropertyChanged. 
/// PropertyChanged.Fody does the magic on this.
/// </summary>
internal class BaseModel : INotifyPropertyChanged
{
#pragma warning disable CS0067

    public event PropertyChangedEventHandler PropertyChanged;

#pragma warning restore CS0067
}
