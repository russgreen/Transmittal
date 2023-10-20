using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Transmittal.Library.Messages;
public class LockFileMessage : ValueChangedMessage<string>
{
    public LockFileMessage(string value) : base(value)
    {
    }
}
