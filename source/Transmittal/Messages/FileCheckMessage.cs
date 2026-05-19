using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transmittal.Messages;

internal class FileCheckMessage : ValueChangedMessage<string>
{
    public FileCheckMessage(string value) : base(value)
    {
    }
}
