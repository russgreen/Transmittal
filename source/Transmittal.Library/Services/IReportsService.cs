using Transmittal.Library.Models;

namespace Transmittal.Library.Services;

public interface IReportsService
{
    void ShowProjectDirectoryReport(List<ProjectDirectoryModel> projectDirectory);
    void ShowTransmittalReport(int transmittalID);
    void ShowTransmittalSummaryReport(List<TransmittalModel> transmittals = null);
    void ShowMasterDocumentsListReport();
}
