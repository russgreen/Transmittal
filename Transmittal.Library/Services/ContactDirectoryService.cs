using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Media.Media3D;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Models;

namespace Transmittal.Library.Services;
public class ContactDirectoryService : IContactDirectoryService
{
    private readonly IDataConnection _connection;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<ContactDirectoryService> _logger;  

    public ContactDirectoryService(IDataConnection dataConnection,
        ISettingsService settingsService,
        ILogger<ContactDirectoryService> logger)
    {
        _connection = dataConnection;
        _settingsService = settingsService;
        _logger = logger;
    }

    public void CreateCompany(CompanyModel model)
    {
        _logger.LogDebug("Creating company {model}", model);

        string sql = "INSERT INTO Company (CompanyName, Role, Address, Tel, Fax, Website) " +
            "VALUES (@CompanyName, @Role, @Address, @Tel, @Fax, @Website); " +
            "SELECT last_insert_rowid();";

        model.ID = _connection.CreateData<CompanyModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile,
            sql, model, new
            {
                CompanyName = model.CompanyName,
                Role = model.Role,
                Address = model.Address,
                Tel = model.Tel,
                Fax = model.Fax,
                Website = model.Website
            }, nameof(model.ID)).ID;
    }

    public void CreatePerson(PersonModel model)
    {
        _logger.LogDebug("Creating person {model}", model);

        string sql = "INSERT INTO Person (LastName, FirstName, Email, Tel, Mobile, Position, Notes, CompanyID, ShowInReport) " +
            "VALUES (@LastName, @FirstName, @Email, @Tel, @Mobile, @Position, @Notes, @CompanyID, @ShowInReport); " +
            "SELECT last_insert_rowid();";

        model.ID = _connection.CreateData<PersonModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile, 
            sql, model, new
            {
                LastName = model.LastName,
                FirstName = model.FirstName,
                Email = model.Email,
                Tel = model.Tel,
                Mobile = model.Mobile,
                CompanyID = model.CompanyID,
                Position = model.Position,
                Notes = model.Notes,
                ShowInReport = model.ShowInReport
            }, nameof(model.ID)).ID;
    }

    public List<CompanyModel> GetCompanies_All()
    {
        _logger.LogDebug("Get companies");

        string sql = "SELECT * FROM Company ORDER BY CompanyName;";

        var companies = _connection.LoadData<CompanyModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile,
            sql, null).ToList();

        foreach (CompanyModel company in companies)
        {
            company.Contacts = GetPeople_ByCompany(company.ID);
        }

        return companies;
    }

    public CompanyModel GetCompany(int companyID)
    {
        _logger.LogDebug("Get company {id}", companyID);

        string sql = "SELECT * FROM Company WHERE ID = @ID;";

        return _connection.LoadData<CompanyModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile,
            sql, new { ID = companyID }).FirstOrDefault();
    }

    public List<PersonModel> GetPeople_ByCompany(int companyID)
    {
        _logger.LogDebug("Get people by company ID {id}", companyID);

        string sql = "SELECT * FROM Person " +
            "WHERE CompanyID = @CompanyID " +
            "ORDER BY LastName, FirstName;";

        return _connection.LoadData<PersonModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile,
            sql, new { CompanyID = companyID }).ToList();
    }

    public List<PersonModel> GetPeople_All()
    {
        _logger.LogDebug("Get people");

        string sql = "SELECT * FROM Person " +
            "ORDER BY LastName, FirstName;";

        return _connection.LoadData<PersonModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile,
            sql, null).ToList();
    }

    public PersonModel GetPerson(int personID)
    {
        _logger.LogDebug("Get person {id}", personID);

        string sql = "SELECT * FROM Person " +
            "WHERE ID = @ID;";

        return _connection.LoadData<PersonModel, dynamic>(
            _settingsService.GlobalSettings.DatabaseFile,
            sql, new { ID = personID }).FirstOrDefault();
    }

    public List<ProjectDirectoryModel> GetProjectDirectory()
    {
        _logger.LogDebug("Get project directory");

        var people = GetPeople_All();

        List<ProjectDirectoryModel> directoryContacts = new();

        foreach (PersonModel person in people)
        {
            ProjectDirectoryModel directoryContact = new();
            directoryContact.Person = person;

            if(person.CompanyID == 0)
            {
                directoryContact.Company = new CompanyModel();  
            }

            if(person.CompanyID > 0)
            {
                directoryContact.Company = GetCompany(directoryContact.Person.CompanyID);
            }
            
            directoryContacts.Add(directoryContact);
        }

        return directoryContacts;
    }

    public void UpdateCompany(CompanyModel model)
    {
        _logger.LogDebug("Update company {model}", model);

        string sql = "UPDATE Company SET " +
            "CompanyName = @CompanyName, " +
            "Role = @Role, " +
            "Address = @Address, " +
            "Tel = @Tel, " +
            "Fax = @Fax, " +
            "Website = @Website " +
            "WHERE(ID = @ID);";

        _connection.SaveData(
            _settingsService.GlobalSettings.DatabaseFile,
            sql, new
            {
                CompanyName = model.CompanyName,
                Role = model.Role,
                Address = model.Address,
                Tel = model.Tel,
                Fax = model.Fax,
                Website = model.Website,
                ID = model.ID
            });
    }

    public void DeleteCompany(CompanyModel model) 
    {
        _logger.LogDebug("Delete company {model}", model);

        string sql = "DELETE FROM Company WHERE (ID = @ID);";

        _connection.SaveData(_settingsService.GlobalSettings.DatabaseFile,
            sql, new { ID = model.ID });
    }

    public void UpdatePerson(PersonModel model)
    {
        _logger.LogDebug("Update person {model}", model);

        string sql = "UPDATE Person SET " +
            "LastName = @LastName, " +
            "FirstName = @FirstName, " +
            "Email = @Email, " +
            "Tel = @Tel, " +
            "Mobile = @Mobile, " +
            "CompanyID = @CompanyID, " +
            "Position = @Position, " +
            "Notes = @Notes, " +
            "ShowInReport = @ShowInReport " +
            "WHERE (ID = @ID);";

        _connection.SaveData(
            _settingsService.GlobalSettings.DatabaseFile,
            sql, new
            {
                LastName = model.LastName,
                FirstName = model.FirstName,
                Email = model.Email,
                Tel = model.Tel,
                Mobile = model.Mobile,
                CompanyID = model.CompanyID,
                Position = model.Position,
                Notes = model.Notes,
                ShowInReport = model.ShowInReport,
                ID = model.ID
            });
    }

    public void DeletePerson(PersonModel model)
    {
        _logger.LogDebug("Delete person {model}", model);

        string sql = "DELETE FROM Person WHERE (ID = @ID);";

        _connection.SaveData(_settingsService.GlobalSettings.DatabaseFile,
            sql, new { ID = model.ID });
    }
}
