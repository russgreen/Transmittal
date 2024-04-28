using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Transmittal.Messages;
internal class CancelTransmittalMessage : ValueChangedMessage<bool>
{
    public CancelTransmittalMessage(bool value) : base(value)
    {
    }
}
