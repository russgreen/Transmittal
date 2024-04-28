using Transmittal.Library.Models;
using Transmittal.Models;

namespace Transmittal.Requesters;
public interface IDirectoryRequester
{
    void DirectoryComplete(ProjectDirectoryModel model);
}
