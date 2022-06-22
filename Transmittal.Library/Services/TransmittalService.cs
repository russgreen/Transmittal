using System.Data.Common;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Models;

namespace Transmittal.Library.Services;
public class TransmittalService : ITransmittalService
{
    private readonly IDataConnection _connection;
    private readonly ISettingsService _settingsService;
    private readonly IContactDirectoryService _contactDirectoryService;

    public TransmittalService(IDataConnection dataConnection,
        ISettingsService settingsService,
        IContactDirectoryService contactDirectoryService)
    {
        _connection = dataConnection;
        _settingsService = settingsService;
        _contactDirectoryService = contactDirectoryService;
    }

    public void CreateTransmittal(TransmittalModel model)
    {
        string sql = "INSERT INTO Transmittal (TransDate) VALUES (@TransDate); " +
            "SELECT last_insert_rowid();";

        model.ID = _connection.CreateData<TransmittalModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile,
            sql, model, new
            {
                TransDate = model.TransDate,
            }, nameof(model.ID)).ID;
    }

    public void CreateTransmittalDist(TransmittalDistributionModel model)
    {
        string sql = "INSERT INTO TransmittalDistribution (TransID, PersonID, TransFormat, TransCopies) " +
            "VALUES (@TransID, @PersonID, @TransFormat, @TransCopies); " +
            "SELECT last_insert_rowid();";

        model.TransDistID = _connection.CreateData<TransmittalDistributionModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile,
            sql, model, new
            {
                TransID = model.TransID,
                PersonID = model.PersonID,
                TransFormat = model.TransFormat,
                TransCopies = model.TransCopies
            }, nameof(model.TransDistID)).TransDistID;
    }

    public void CreateTransmittalItem(TransmittalItemModel model)
    {
        string sql = "INSERT INTO TransmittalItems ( TransID, DrgProj, DrgOriginator, DrgVolume, DrgLevel, DrgType, DrgRole, DrgNumber, DrgStatus, DrgRev, DrgName, DrgPaper, DrgScale, DrgDrawn, DrgChecked ) " +
            "VALUES ( @TransID, @DrgProj, @DrgOriginator, @DrgVolume, @DrgLevel, @DrgType, @DrgRole, @DrgNo, @DrgStatus, @DrgRev, @DrgName, @DrgPaper, @DrgScale, @DrgDrawn, @DrgChecked ); " +
            "SELECT last_insert_rowid();";

        model.TransItemID = _connection.CreateData<TransmittalItemModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile,
            sql, model, new
            {
                TransID = model.TransID,
                DrgProj = model.DrgProj,
                DrgOriginator = model.DrgOriginator,
                DrgVolume = model.DrgVolume,
                DrgLevel = model.DrgLevel,
                DrgType = model.DrgType,
                DrgRole = model.DrgRole,
                DrgNo = model.DrgNumber,
                DrgStatus = model.DrgStatus,
                DrgRev = model.DrgRev,
                DrgName = model.DrgName,
                DrgPaper = model.DrgPaper,
                DrgScale = model.DrgScale,
                DrgDrawn = model.DrgDrawn,
                DrgChecked = model.DrgChecked
            }, nameof(model.TransItemID)).TransItemID;
    }

    public void DeleteTransmittal(TransmittalModel model)
    {
        string sql = "DELETE FROM Transmittal WHERE (ID = @ID);";

        _connection.SaveData(_settingsService.GlobalSettings.DatabaseFile, 
            sql, new { ID = model.ID });
    }

    public void DeleteTransmittalDist(TransmittalDistributionModel model)
    {
        string sql = "DELETE FROM TransmittalDistribution WHERE TransDistID = @TransDistID;";

        _connection.SaveData(_settingsService.GlobalSettings.DatabaseFile, 
            sql, new { TransDistID = model.TransDistID });
    }

    public void DeleteTransmittalItem(TransmittalItemModel model)
    {
        string sql = "DELETE FROM TransmittalItems WHERE TransItemID = @TransItemID;";

        _connection.SaveData(_settingsService.GlobalSettings.DatabaseFile, 
            sql, new { TransItemID = model.TransItemID });
    }

    public TransmittalModel GetTransmittal(int transmittalID)
    {
        string sql = "SELECT * FROM Transmittal WHERE (ID = @ID);";

        var transmittal = _connection.LoadData<TransmittalModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile, 
            sql, new { ID = transmittalID }).FirstOrDefault();

        transmittal.Items = GetTransmittalItems_ByTransmittal(transmittalID);
        transmittal.Distribution = GetTransmittalDistributions_ByTransmittal(transmittalID);

        return transmittal;
    }

    public List<TransmittalDistributionModel> GetTransmittalDistributions_ByTransmittal(int transmittalID)
    {
        string sql = "SELECT * FROM TransmittalDistribution " +
            "WHERE (TransID = @TransID);";

        var transmittalDist = _connection.LoadData<TransmittalDistributionModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile, 
            sql, new { TransID = transmittalID }).ToList();

        //TODO check if this is need at this point
        //foreach (TransmittalDistributionModel item in transmittalDist)
        //{
        //    item.Person = _contactDirectoryService.GetPerson(item.PersonID);
        //}

        return transmittalDist;
    }

    public List<TransmittalItemModel> GetTransmittalItems_ByTransmittal(int transmittalID)
    {
        string sql = "SELECT * FROM TransmittalItems " +
            "WHERE (TransID = @TransID);";

        return _connection.LoadData<TransmittalItemModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile, 
            sql, new { TransID = transmittalID }).ToList();
    }

    public List<TransmittalModel> GetTransmittals()
    {
        string sql = "SELECT * FROM Transmittal;";

        var transmittals = _connection.LoadData<TransmittalModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile, 
            sql, null).ToList();

        foreach (TransmittalModel transmittal in transmittals)
        {
            transmittal.Items = GetTransmittalItems_ByTransmittal(transmittal.ID);
            transmittal.Distribution = GetTransmittalDistributions_ByTransmittal(transmittal.ID);
        }

        return transmittals;
    }

    public List<TransmittalModel> GetTransmittals_ByPerson(int personID)
    {
        throw new NotImplementedException();
    }

    public TransmittalModel MergeTransmittals(List<TransmittalModel> transmittalsToMerge)
    {
        //create a new item
        TransmittalModel newTransmittal = new TransmittalModel
        {
            TransDate = transmittalsToMerge.FirstOrDefault().TransDate
        };

        //save new item to db and get the new transmittal ID
        CreateTransmittal(newTransmittal);

        foreach (TransmittalModel transmittal in transmittalsToMerge)
        {
            //associate transmittal items to new item
            foreach (TransmittalItemModel item in transmittal.Items)
            {
                item.TransID = newTransmittal.ID;
                newTransmittal.Items.Add(item);

                UpdateTransmittalItem(item);
            }

            //associate distribution items to new item - do not copy duplicates
            foreach (TransmittalDistributionModel dist in transmittal.Distribution)
            {
                //ensure the distribution in unique . e.g.
                //NemesisList.Any(n => n.Dex_ID == IdToCheck)
                if (newTransmittal.Distribution.Any<TransmittalDistributionModel>(d => d.PersonID == dist.PersonID))
                {
                    //the new list already contains this contact so just delete it.
                    DeleteTransmittalDist(dist);
                }
                else
                {
                    //this is a unique record to add it
                    dist.TransID = newTransmittal.ID;
                    //transmittal.Distribution.Remove(dist);
                    newTransmittal.Distribution.Add(dist);

                    UpdateTransmittalDist(dist);
                }
            }

            //delete old items from db
            DeleteTransmittal(transmittal);
        }

        return newTransmittal;
    }

    public void UpdateTransmittal(TransmittalModel model)
    {
        string sql = "UPDATE Transmittal SET TransDate = @TransDate WHERE ((ID = @ID));";

        _connection.SaveData(_settingsService.GlobalSettings.DatabaseFile, 
            sql, new
            {
                TransDate = model.TransDate,
                TransID = model.ID
            });
    }

    public void UpdateTransmittalDist(TransmittalDistributionModel model)
    {
        string sql = "UPDATE TransmittalDistribution SET " +
            "TransID = @TransID, " +
            "PersonID = @PersonID, 	" +
            "TransFormat = @TransFormat, " +
            "TransCopies = @TransCopies " +
            "WHERE ((TransDistID = @TransDistID));";

        _connection.SaveData(
            _settingsService.GlobalSettings.DatabaseFile, 
            sql, new
            {
                TransID = model.TransID,
                PersonID = model.PersonID,
                TransFormat = model.TransFormat,
                TransCopies = model.TransCopies,
                TransDistID = model.TransDistID
            });
    }

    public void UpdateTransmittalItem(TransmittalItemModel model)
    {
        string sql = "UPDATE TransmittalItems SET " +
           "TransID = @TransID, " +
           "DrgNumber = @DrgNumber, " +
           "DrgRev = @DrgRev, " +
           "DrgName = @DrgName, " +
           "DrgPaper = @DrgPaper, " +
           "DrgScale = @DrgScale, " +
           "DrgDrawn = @DrgDrawn, " +
           "DrgChecked = @DrgChecked, " +
           "DrgProj = @DrgProj, " +
           "DrgOriginator = @DrgOriginator, " +
           "DrgVolume = @DrgVolume, " +
           "DrgLevel = @DrgLevel, " +
           "DrgType = @DrgType, " +
           "DrgRole = @DrgRole, " +
           "DrgStatus = @DrgStatus  " +
           "WHERE (TransItemID = @TransItemID);";

        _connection.SaveData(
            _settingsService.GlobalSettings.DatabaseFile, 
            sql, new
            {
                TransID = model.TransID,
                DrgNumber = model.DrgNumber,
                DrgRev = model.DrgRev,
                DrgName = model.DrgName,
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
                TransItemID = model.TransItemID
            });
    }
}
