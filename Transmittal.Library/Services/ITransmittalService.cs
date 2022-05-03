using Transmittal.Library.Models;

namespace Transmittal.Library.Services;

public interface ITransmittalService
{
    /// <summary>
    /// Get all transmittals 
    /// </summary>
    /// <returns>List of TransmittalModel</returns>
    List<TransmittalModel> GetTransmittals();
    /// <summary>
    /// Get all transmittals issued to a person
    /// </summary>
    /// <param name="personID"></param>
    /// <returns>List of TransmittalModel</returns>
    /// <remarks>Use LINQ to filter by project</remarks>
    List<TransmittalModel> GetTransmittals_ByPerson(int personID);
    /// <summary>
    /// Get a single transmittal record by transmittal ID
    /// </summary>
    /// <param name="transmittalID"></param>
    /// <returns>TransmittalModel</returns>
    TransmittalModel GetTransmittal(int transmittalID);
    /// <summary>
    /// Get all items for a transmittal
    /// </summary>
    /// <param name="transmittalID"></param>
    /// <returns>List of TransmittalItemModel</returns>
    List<TransmittalItemModel> GetTransmittalItems_ByTransmittal(int transmittalID);
    /// <summary>
    /// Get all distribution records for a transittal
    /// </summary>
    /// <param name="transmittalID"></param>
    /// <returns>List of TransmittalDistributionModel</returns>
    List<TransmittalDistributionModel> GetTransmittalDistributions_ByTransmittal(int transmittalID);
    /// <summary>
    /// Create a new transmittal record
    /// </summary>
    /// <param name="model"></param>
    void CreateTransmittal(TransmittalModel model);
    void UpdateTransmittal(TransmittalModel model);
    void DeleteTransmittal(TransmittalModel model);
    TransmittalModel MergeTransmittals(List<TransmittalModel> transmittalsToMerge);
    void CreateTransmittalItem(TransmittalItemModel model);
    void UpdateTransmittalItem(TransmittalItemModel model);
    void DeleteTransmittalItem(TransmittalItemModel model);
    void CreateTransmittalDist(TransmittalDistributionModel model);
    void UpdateTransmittalDist(TransmittalDistributionModel model);
    void DeleteTransmittalDist(TransmittalDistributionModel model);
}
