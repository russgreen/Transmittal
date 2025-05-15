using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Models;

namespace Transmittal.Library.Services;
public class TransmittalService : ITransmittalService
{
    private readonly IDataConnection _connection;
    private readonly ISettingsService _settingsService;
    private readonly IContactDirectoryService _contactDirectoryService;
    private readonly ILogger<TransmittalService> _logger;
    private readonly IMessageBoxService _messageBox;

    public TransmittalService(IDataConnection dataConnection,
        ISettingsService settingsService,
        IContactDirectoryService contactDirectoryService,
        ILogger<TransmittalService> logger,
        IMessageBoxService messageBox)
    {
        _connection = dataConnection;
        _settingsService = settingsService;
        _contactDirectoryService = contactDirectoryService;
        _logger = logger;
        _messageBox = messageBox;
    }

    public void CreateTransmittal(TransmittalModel model)
    {
        _logger.LogDebug("Creating transmittal {model}", model);

        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create transmittal {model}", model);
            _messageBox.ShowOk("Failed to create transmittal", ex.Message);
        }

    }

    public void CreateTransmittalDist(TransmittalDistributionModel model)
    {
        _logger.LogDebug("Creating transmittal distribution {model}", model);

        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create transmittal distribution {model}", model);
            _messageBox.ShowOk("Failed to create transmittal distribution", ex.Message);
        }

    }

    public void CreateTransmittalItem(TransmittalItemModel model)
    {
        _logger.LogDebug("Creating transmittal item {model}", model);

        try
        {
            string sql = "INSERT INTO TransmittalItems ( TransID, DrgProj, DrgOriginator, DrgVolume, DrgLevel, DrgType, DrgRole, DrgNumber, DrgStatus, DrgRev, DrgName, DrgPaper, DrgScale, DrgDrawn, DrgChecked, DrgPackage ) " +
                "VALUES ( @TransID, @DrgProj, @DrgOriginator, @DrgVolume, @DrgLevel, @DrgType, @DrgRole, @DrgNo, @DrgStatus, @DrgRev, @DrgName, @DrgPaper, @DrgScale, @DrgDrawn, @DrgChecked, @DrgPackage ); " +
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
                    DrgChecked = model.DrgChecked,
                    DrgPackage = model.DrgPackage
                }, nameof(model.TransItemID)).TransItemID;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create transmittal item {model}", model);
            _messageBox.ShowOk("Failed to create transmittal item", ex.Message);
        }

    }

    public void DeleteTransmittal(TransmittalModel model)
    {
        _logger.LogDebug("Deleting transmittal {model}", model);

        try
        {
            string sql = "DELETE FROM Transmittal WHERE (ID = @ID);";

            _connection.SaveData(_settingsService.GlobalSettings.DatabaseFile,
                sql, new { ID = model.ID });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete transmittal {model}", model);
            _messageBox.ShowOk("Failed to delete transmittal", ex.Message);
        }

    }

    public void DeleteTransmittalDist(TransmittalDistributionModel model)
    {
        _logger.LogDebug("Deleting transmittal distribution {model}", model);

        try
        {
            string sql = "DELETE FROM TransmittalDistribution WHERE TransDistID = @TransDistID;";

            _connection.SaveData(_settingsService.GlobalSettings.DatabaseFile,
                sql, new { TransDistID = model.TransDistID });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete transmittal distribution {model}", model);
            _messageBox.ShowOk("Failed to delete transmittal distribution", ex.Message);
        }

    }

    public void DeleteTransmittalItem(TransmittalItemModel model)
    {
        _logger.LogDebug("Deleting transmittal item {model}", model);

        try
        {
            string sql = "DELETE FROM TransmittalItems WHERE TransItemID = @TransItemID;";

            _connection.SaveData(_settingsService.GlobalSettings.DatabaseFile,
                sql, new { TransItemID = model.TransItemID });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete transmittal item {model}", model);
            _messageBox.ShowOk("Failed to delete transmittal item", ex.Message);
        }

    }

    public TransmittalModel GetTransmittal(int transmittalID)
    {
        _logger.LogDebug("Get Transmittal {id}", transmittalID);

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
        _logger.LogDebug("Get Transmittal Distributions {id}", transmittalID);

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
        _logger.LogDebug("Get Transmittal Items {id}", transmittalID);

        string sql = "SELECT * FROM TransmittalItems " +
            "WHERE (TransID = @TransID);";

        return _connection.LoadData<TransmittalItemModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile,
            sql, new { TransID = transmittalID }).ToList();
    }

    public List<TransmittalModel> GetTransmittals()
    {
        _logger.LogDebug("Get Transmittals");

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
        _logger.LogDebug("Get Transmittals by Person {id}", personID);

        string sql = "SELECT Transmittal.*, TransmittalDistribution.PersonID " +
            "FROM Transmittal " +
            "INNER JOIN TransmittalDistribution ON Transmittal.ID = TransmittalDistribution.TransID " +
            "WHERE TransmittalDistribution.PersonID = @PersonID;";

        var transmittals = _connection.LoadData<TransmittalModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile,
            sql, new { PersonID = personID }).ToList();

        foreach (TransmittalModel transmittal in transmittals)
        {
            transmittal.Items = GetTransmittalItems_ByTransmittal(transmittal.ID);
            transmittal.Distribution = GetTransmittalDistributions_ByTransmittal(transmittal.ID)
                .Where(x => x.PersonID == personID)
                .ToList();
        }

        return transmittals;
    }

    public TransmittalModel MergeTransmittals(List<TransmittalModel> transmittalsToMerge)
    {
        _logger.LogDebug("Merge transmittals {transmittals}", transmittalsToMerge);

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

    public List<string> GetPackages()
    {
        _logger.LogDebug("Get Packages");

        List<string> packages = new();

        try
        {
            string sql = "SELECT DISTINCT DrgPackage FROM TransmittalItems ORDER BY DrgPackage ASC;";

            packages = _connection.LoadData<string, dynamic>(
                _settingsService.GlobalSettings.DatabaseFile,
                sql, null).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading packages from database. Check database has been upgraded to v2");
        }


        return packages;
    }

    public void UpdateTransmittal(TransmittalModel model)
    {
        _logger.LogDebug("Update Transmittal {transmittal}", model);

        try
        {
            string sql = "UPDATE Transmittal SET TransDate = @TransDate WHERE ((ID = @ID));";

            _connection.SaveData(_settingsService.GlobalSettings.DatabaseFile,
                sql, new
                {
                    TransDate = model.TransDate,
                    TransID = model.ID
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update transmittal {model}", model);
            _messageBox.ShowOk("Failed to update transmittal", ex.Message);
        }

    }

    public void UpdateTransmittalDist(TransmittalDistributionModel model)
    {
        _logger.LogDebug("Update Transmittal Distribution {transmittalDist}", model);

        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update transmittal distribution {model}", model);
            _messageBox.ShowOk("Failed to update transmittal distribution", ex.Message);
        }

    }

    public void UpdateTransmittalItem(TransmittalItemModel model)
    {
        _logger.LogDebug("Update Transmittal Item {transmittalItem}", model);

        try
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
                "DrgStatus = @DrgStatus, " +
                "DrgPackage = @DrgPackage " +
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
                    DrgPackage = model.DrgPackage,
                    TransItemID = model.TransItemID
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update transmittal item {model}", model);
            _messageBox.ShowOk("Failed to update transmittal item", ex.Message);
        }

    }

    public void CreateTransmittalItems(List<TransmittalItemModel> models)
    {
        _logger.LogDebug("Creating multiple transmittal items");

        try
        {
            _connection.BeginTransaction(_settingsService.GlobalSettings.DatabaseFile);

            string sql = "INSERT INTO TransmittalItems ( TransID, DrgProj, DrgOriginator, DrgVolume, DrgLevel, DrgType, DrgRole, DrgNumber, DrgStatus, DrgRev, DrgName, DrgPaper, DrgScale, DrgDrawn, DrgChecked, DrgPackage ) " +
                         "VALUES ( @TransID, @DrgProj, @DrgOriginator, @DrgVolume, @DrgLevel, @DrgType, @DrgRole, @DrgNo, @DrgStatus, @DrgRev, @DrgName, @DrgPaper, @DrgScale, @DrgDrawn, @DrgChecked, @DrgPackage );";

            foreach (var model in models)
            {
                _connection.ExecuteInTransaction(sql, new
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
                    DrgChecked = model.DrgChecked,
                    DrgPackage = model.DrgPackage
                });
            }

            _connection.CommitTransaction();
        }
        catch (Exception ex)
        {
            _connection.RollbackTransaction();
            _logger.LogError(ex, "Failed to create multiple transmittal items");
            _messageBox.ShowOk("Failed to create transmittal items", ex.Message);
        }
    }

    public void CreateTransmittalDistributions(List<TransmittalDistributionModel> models)
    {
        _logger.LogDebug("Creating multiple transmittal distributions");

        try
        {
            _connection.BeginTransaction(_settingsService.GlobalSettings.DatabaseFile);

            string sql = "INSERT INTO TransmittalDistribution (TransID, PersonID, TransFormat, TransCopies) " +
                         "VALUES (@TransID, @PersonID, @TransFormat, @TransCopies);";

            foreach (var model in models)
            {
                _connection.ExecuteInTransaction(sql, new
                {
                    TransID = model.TransID,
                    PersonID = model.PersonID,
                    TransFormat = model.TransFormat,
                    TransCopies = model.TransCopies
                });
            }

            _connection.CommitTransaction();
        }
        catch (Exception ex)
        {
            _connection.RollbackTransaction();
            _logger.LogError(ex, "Failed to create multiple transmittal distributions");
            _messageBox.ShowOk("Failed to create transmittal distributions", ex.Message);
        }
    }
}