using Transmittal.Library.Models;
using Transmittal.Reports.Models;

namespace Transmittal.Reports.Mapping;

internal static class DomainToReportMapping
{
    public static ProjectDirectoryReportModel ToProjectDirectoryReportModel(this ProjectDirectoryModel model)
    {
        return new ProjectDirectoryReportModel
        {
            Person = model.Person,
            Company = model.Company,
        };
    }

    public static TransmittalItemReportModel ToTransmittalItemReportModel(this TransmittalItemModel model)
    {
        return new TransmittalItemReportModel
        {
            TransID = model.TransID,
            TransItemID = model.TransItemID,
            DrgNumber = model.DrgNumber,
            DrgName = model.DrgName,
            DrgRev = model.DrgRev,
            DrgPaper = model.DrgPaper,
            DrgScale = model.DrgScale,
            DrgDrawn = model.DrgDrawn,
            DrgChecked = model.DrgChecked,
            DrgProj = model.DrgProj,
            DrgOriginator = model.DrgOriginator,
            DrgVolume = model.DrgVolume,
            DrgLevel = model.DrgLevel,
            DrgType = model.DrgType,
            DrgRole = model.DrgRole,
            DrgStatus = model.DrgStatus,
            DrgStatusDescription = model.DrgStatusDescription,
            DrgPackage = model.DrgPackage,
            DrgSheetCollection = model.DrgSheetCollection,
        };
     }

    public static TransmittalDistributionReportModel ToTransmittalDistributionReportModel(this TransmittalDistributionModel model)
    {
        return new TransmittalDistributionReportModel
        {
            TransDistID = model.TransDistID,
            TransID = model.TransID,
            TransFormat = model.TransFormat,           
            TransCopies = model.TransCopies,
            PersonID = model.PersonID,
            Person = model.Person,
            Company = model.Company,
            ID = model.ID,
        };
    }
}





