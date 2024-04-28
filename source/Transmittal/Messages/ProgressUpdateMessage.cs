using CommunityToolkit.Mvvm.Messaging.Messages;
using Transmittal.Models;

namespace Transmittal.Messages;
internal class ProgressUpdateMessage : ValueChangedMessage<ProgressMessageModel>
{
    public ProgressUpdateMessage(ProgressMessageModel value) : base(value)
    {
    }
}
