using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Transmittal.Messages;
public class CancelTransmittalMessage : ValueChangedMessage<bool>
{
    public CancelTransmittalMessage(bool value) : base(value)
    {
    }
}
