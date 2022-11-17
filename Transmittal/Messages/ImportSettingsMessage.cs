using CommunityToolkit.Mvvm.Messaging.Messages;
using Transmittal.Library.Models;
using Transmittal.Models;

namespace Transmittal.Messages;
internal class ImportSettingsMessage : ValueChangedMessage<ImportSettingsModel>
{
    public ImportSettingsMessage(ImportSettingsModel value) : base(value)
    {
    }
}
