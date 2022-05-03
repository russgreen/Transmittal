using Transmittal.Library.Models;

namespace Transmittal.Requesters;

public interface IStatusRequester
{
    void StatusComplete(DocumentStatusModel model);
}
