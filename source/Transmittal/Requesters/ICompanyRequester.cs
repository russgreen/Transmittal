using Transmittal.Library.Models;

namespace Transmittal.Requesters;
public interface ICompanyRequester
{
    void CompanyComplete(CompanyModel model);
}
