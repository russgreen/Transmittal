using System;
using System.Collections.Generic;
using System.Text;
using Transmittal.Library.Models;

namespace Transmittal.Library.Services;
public class TransmittalService : ITransmittalService
{
    public TransmittalService()
    {
    }

    public void CreateTransmittal(TransmittalModel model)
    {
        throw new NotImplementedException();
    }

    public void CreateTransmittalDist(TransmittalDistributionModel model)
    {
        throw new NotImplementedException();
    }

    public void CreateTransmittalItem(TransmittalItemModel model)
    {
        throw new NotImplementedException();
    }

    public void DeleteTransmittal(TransmittalModel model)
    {
        throw new NotImplementedException();
    }

    public void DeleteTransmittalDist(TransmittalDistributionModel model)
    {
        throw new NotImplementedException();
    }

    public void DeleteTransmittalItem(TransmittalItemModel model)
    {
        throw new NotImplementedException();
    }

    public TransmittalModel GetTransmittal(int transmittalID)
    {
        throw new NotImplementedException();
    }

    public List<TransmittalDistributionModel> GetTransmittalDistributions_ByTransmittal(int transmittalID)
    {
        throw new NotImplementedException();
    }

    public List<TransmittalItemModel> GetTransmittalItems_ByTransmittal(int transmittalID)
    {
        throw new NotImplementedException();
    }

    public List<TransmittalModel> GetTransmittals()
    {
        throw new NotImplementedException();
    }

    public List<TransmittalModel> GetTransmittals_ByPerson(int personID)
    {
        throw new NotImplementedException();
    }

    public TransmittalModel MergeTransmittals(List<TransmittalModel> transmittalsToMerge)
    {
        throw new NotImplementedException();
    }

    public void UpdateTransmittal(TransmittalModel model)
    {
        throw new NotImplementedException();
    }

    public void UpdateTransmittalDist(TransmittalDistributionModel model)
    {
        throw new NotImplementedException();
    }

    public void UpdateTransmittalItem(TransmittalItemModel model)
    {
        throw new NotImplementedException();
    }
}
