using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly IMessageBoxService _messageBox;

    public ContactDirectoryService(IDataConnection dataConnection,
        ISettingsService settingsService,
        ILogger<ContactDirectoryService> logger,
        IMessageBoxService messageBox)
    {
        _connection = dataConnection;
        _settingsService = settingsService;
        _logger = logger;
        _messageBox = messageBox;
    }

    public void CreateCompany(CompanyModel model)
    {
        _logger.LogDebug("Creating company {model}", model);

        try
        {
            string sql = "INSERT INTO Company (CompanyName, Role, OrganizationCode, Address, Tel, Fax, Website) " +
                "VALUES (@CompanyName, @Role, @OrganizationCode, @Address, @Tel, @Fax, @Website); " +
                "SELECT last_insert_rowid();";

            model.ID = _connection.CreateData<CompanyModel, dynamic>(
                _settingsService.GlobalSettings.DatabaseFile,
                sql, model, new
                {
                    CompanyName = model.CompanyName,
                    Role = model.Role,
                    OrganizationCode = model.OrganizationCode,
                    Address = model.Address,
                    Tel = model.Tel,
                    Fax = model.Fax,
                    Website = model.Website
                }, nameof(model.ID)).ID;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create company {model}", model);
            
            _messageBox.ShowOk("Failed to create company", ex.Message);
        }

    }

    public void CreatePerson(PersonModel model)
    {
        _logger.LogDebug("Creating person {model}", model);

        try
        {
            string sql = "INSERT INTO Person (LastName, FirstName, Email, Tel, Mobile, Position, Notes, CompanyID, ShowInReport, Archive) " +
                "VALUES (@LastName, @FirstName, @Email, @Tel, @Mobile, @Position, @Notes, @CompanyID, @ShowInReport, @Archive); " +
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
                    ShowInReport = model.ShowInReport,
                    Archive = model.Archive
                }, nameof(model.ID)).ID;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create person {model}", model);
            
            _messageBox.ShowOk("Failed to create person", ex.Message);
        }

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

    public List<CompanyModel> FindCompanyMatches(string companyName, int maxResults = 5)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            return new();
        }

        var normalizedTarget = NormalizeForCompare(companyName);

        return GetCompanies_All()
            .Select(company => new
            {
                Company = company,
                Score = CalculateCompanyMatchScore(normalizedTarget, NormalizeForCompare(company.CompanyName))
            })
            .Where(x => x.Score >= 75)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Company.CompanyName)
            .Take(Math.Max(1, maxResults))
            .Select(x => x.Company)
            .ToList();
    }

    public List<PersonModel> FindPersonMatches(string firstName, string lastName, string email, int? companyID = null, int maxResults = 5)
    {
        var normalizedFirstName = NormalizeForCompare(firstName);
        var normalizedLastName = NormalizeForCompare(lastName);
        var normalizedEmail = NormalizeEmail(email);
        var normalizedTargetName = NormalizeForCompare($"{firstName} {lastName}");

        if (string.IsNullOrWhiteSpace(normalizedTargetName) && string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return new();
        }

        var people = GetPeople_All().AsEnumerable();

        if (companyID.HasValue && companyID.Value > 0)
        {
            people = people.Where(x => x.CompanyID == companyID.Value);
        }

        return people
            .Select(person => new
            {
                Person = person,
                Score = CalculatePersonMatchScore(
                    normalizedTargetName,
                    normalizedFirstName,
                    normalizedLastName,
                    normalizedEmail,
                    person,
                    companyID)
            })
            .Where(x => x.Score >= 60)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Person.LastName)
            .ThenBy(x => x.Person.FirstName)
            .Take(Math.Max(1, maxResults))
            .Select(x => x.Person)
            .ToList();
    }

    public List<ProjectDirectoryModel> GetProjectDirectory(bool IncludeArchivedUsers = true)
    {
        _logger.LogDebug("Get project directory");

        var people = GetPeople_All().ToList();

        if (!IncludeArchivedUsers)
        {
            people = people.Where(p => p.Archive == false).ToList();
        }

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

        try
        {
            string sql = "UPDATE Company SET " +
                "CompanyName = @CompanyName, " +
                "Role = @Role, " +
                "OrganizationCode = @OrganizationCode, " +
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
                    OrganizationCode = model.OrganizationCode,
                    Address = model.Address,
                    Tel = model.Tel,
                    Fax = model.Fax,
                    Website = model.Website,
                    ID = model.ID
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update company {model}", model);
            
            _messageBox.ShowOk("Failed to update company", ex.Message);
        }

    }

    public void DeleteCompany(CompanyModel model) 
    {
        _logger.LogDebug("Delete company {model}", model);

        try
        {
            string sql = "DELETE FROM Company WHERE (ID = @ID);";

            _connection.SaveData(_settingsService.GlobalSettings.DatabaseFile,
                sql, new { ID = model.ID });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete company {model}", model);
            
            _messageBox.ShowOk("Failed to delete company", ex.Message);
        }

    }

    public void UpdatePerson(PersonModel model)
    {
        _logger.LogDebug("Update person {model}", model);

        try
        {
            string sql = "UPDATE Person SET " +
                "LastName = @LastName, " +
                "FirstName = @FirstName, " +
                "Email = @Email, " +
                "Tel = @Tel, " +
                "Mobile = @Mobile, " +
                "CompanyID = @CompanyID, " +
                "Position = @Position, " +
                "Notes = @Notes, " +
                "ShowInReport = @ShowInReport, " +
                "Archive = @Archive " +
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
                    Archive = model.Archive,
                    ID = model.ID
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update person {model}", model);
            
            _messageBox.ShowOk("Failed to update person", ex.Message);
        }

    }

    public void DeletePerson(PersonModel model)
    {
        _logger.LogDebug("Delete person {model}", model);

        try
        {
            string sql = "DELETE FROM Person WHERE (ID = @ID);";

            _connection.SaveData(_settingsService.GlobalSettings.DatabaseFile,
                sql, new { ID = model.ID });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete person {model}", model);
            
            _messageBox.ShowOk("Failed to delete person", ex.Message);
        }

    }

    private static int CalculateCompanyMatchScore(string normalizedTarget, string normalizedCandidate)
    {
        if (string.IsNullOrWhiteSpace(normalizedTarget) || string.IsNullOrWhiteSpace(normalizedCandidate))
        {
            return 0;
        }

        if (normalizedTarget == normalizedCandidate)
        {
            return 100;
        }

        if (normalizedTarget.Length >= 5 &&
            normalizedCandidate.Length >= 5 &&
            (normalizedTarget.Contains(normalizedCandidate) || normalizedCandidate.Contains(normalizedTarget)))
        {
            return 90;
        }

        var similarity = ComputeSimilarity(normalizedTarget, normalizedCandidate);
        return similarity >= 0.82 ? (int)Math.Round(similarity * 100) : 0;
    }

    private static int CalculatePersonMatchScore(
        string normalizedTargetName,
        string normalizedFirstName,
        string normalizedLastName,
        string normalizedEmail,
        PersonModel candidate,
        int? companyID)
    {
        var score = 0;

        var candidateFirst = NormalizeForCompare(candidate.FirstName);
        var candidateLast = NormalizeForCompare(candidate.LastName);
        var candidateName = NormalizeForCompare($"{candidate.FirstName} {candidate.LastName}");
        var candidateEmail = NormalizeEmail(candidate.Email);

        if (!string.IsNullOrWhiteSpace(normalizedEmail) &&
            !string.IsNullOrWhiteSpace(candidateEmail) &&
            normalizedEmail == candidateEmail)
        {
            score += 120;
        }

        if (!string.IsNullOrWhiteSpace(normalizedFirstName) && normalizedFirstName == candidateFirst)
        {
            score += 30;
        }

        if (!string.IsNullOrWhiteSpace(normalizedLastName) && normalizedLastName == candidateLast)
        {
            score += 35;
        }

        if (!string.IsNullOrWhiteSpace(normalizedFirstName) &&
            !string.IsNullOrWhiteSpace(normalizedLastName) &&
            normalizedFirstName == candidateLast &&
            normalizedLastName == candidateFirst)
        {
            // Handle data-entry mistakes where first and last names are reversed.
            score += 55;
        }

        if (!string.IsNullOrWhiteSpace(normalizedFirstName) &&
            !string.IsNullOrWhiteSpace(candidateFirst) &&
            normalizedFirstName[0] == candidateFirst[0])
        {
            score += 10;
        }

        if (!string.IsNullOrWhiteSpace(normalizedTargetName) && !string.IsNullOrWhiteSpace(candidateName))
        {
            score += (int)Math.Round(ComputeSimilarity(normalizedTargetName, candidateName) * 50);

            var swappedCandidateName = NormalizeForCompare($"{candidate.LastName} {candidate.FirstName}");
            score += (int)Math.Round(ComputeSimilarity(normalizedTargetName, swappedCandidateName) * 40);
        }

        if (companyID.HasValue && companyID.Value > 0 && companyID.Value == candidate.CompanyID)
        {
            score += 15;
        }

        return score;
    }

    private static string NormalizeForCompare(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalizedChars = value.Trim().ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
            .ToArray();

        return string.Join(" ",
            new string(normalizedChars)
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
    }

    private static string NormalizeEmail(string email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? string.Empty
            : email.Trim().ToLowerInvariant();
    }

    private static double ComputeSimilarity(string left, string right)
    {
        if (left == right)
        {
            return 1.0;
        }

        if (left.Length == 0 || right.Length == 0)
        {
            return 0.0;
        }

        var maxLength = Math.Max(left.Length, right.Length);
        var distance = ComputeLevenshteinDistance(left, right);
        return 1.0 - ((double)distance / maxLength);
    }

    private static int ComputeLevenshteinDistance(string left, string right)
    {
        var rows = left.Length + 1;
        var cols = right.Length + 1;
        var matrix = new int[rows, cols];

        for (var i = 0; i < rows; i++)
        {
            matrix[i, 0] = i;
        }

        for (var j = 0; j < cols; j++)
        {
            matrix[0, j] = j;
        }

        for (var i = 1; i < rows; i++)
        {
            for (var j = 1; j < cols; j++)
            {
                var cost = left[i - 1] == right[j - 1] ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[rows - 1, cols - 1];
    }
}
