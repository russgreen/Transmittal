using Transmittal.Models;

namespace Transmittal.Requesters;
public interface IRevisionRequester
{
    void RevisionComplete(RevisionDataModel model);
}
