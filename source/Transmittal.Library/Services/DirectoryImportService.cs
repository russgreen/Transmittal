using Microsoft.Extensions.Logging;
using System.IO;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Models;
using Transmittal.Library.Models.DirectoryImport;

namespace Transmittal.Library.Services;

public class DirectoryImportService : IDirectoryImportService
{
    private readonly IDataConnection _connection;
    private readonly IContactDirectoryService _contactDirectoryService;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<DirectoryImportService> _logger;

    public DirectoryImportService(IDataConnection dataConnection,
        IContactDirectoryService contactDirectoryService,
        ISettingsService settingsService,
        ILogger<DirectoryImportService> logger)
    {
        _connection = dataConnection;
        _contactDirectoryService = contactDirectoryService;
        _settingsService = settingsService;
        _logger = logger;
    }

    public DirectoryImportSource LoadSource(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            throw new FileNotFoundException("The selected database file could not be found.", sourcePath);
        }

        var targetPath = _settingsService.GlobalSettings.DatabaseFile;
        if (!string.IsNullOrWhiteSpace(targetPath) &&
            string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The selected file is the database that is currently open. Choose a different file to import from.");
        }

        var source = new DirectoryImportSource { FilePath = sourcePath };
        var index = TargetIndex.Build(_contactDirectoryService);

        // LoadData opens the source read-only, so it is never modified.
        {
            var tables = _connection.LoadData<string, object>(sourcePath, "SELECT name FROM sqlite_master WHERE type = 'table';", null).ToList();
            if (!tables.Contains("Company", StringComparer.OrdinalIgnoreCase) ||
                !tables.Contains("Person", StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The selected file is not a Transmittal database (no Company or Person table was found).");
            }

            ReadProjectDetails(sourcePath, tables, source);

            // Older databases may not have every column, so only select what exists.
            var companies = _connection.LoadData<CompanyModel, object>(sourcePath, BuildSelect(sourcePath, "Company", new Dictionary<string, string>
            {
                ["ID"] = null,
                ["CompanyName"] = null,
                ["Role"] = "NULL",
                ["OrganisationCode"] = "NULL",
                ["Address"] = "NULL",
                ["Tel"] = "NULL",
                ["Fax"] = "NULL",
                ["Website"] = "NULL"
            }), null).ToList();

            var people = _connection.LoadData<PersonModel, object>(sourcePath, BuildSelect(sourcePath, "Person", new Dictionary<string, string>
            {
                ["ID"] = null,
                ["LastName"] = null,
                ["FirstName"] = null,
                ["Email"] = "NULL",
                ["Tel"] = "NULL",
                ["Mobile"] = "NULL",
                ["Position"] = "NULL",
                ["Notes"] = "NULL",
                ["CompanyID"] = null,
                ["ShowInReport"] = "1",
                ["Archive"] = "0"
            }), null).ToList();

            var companyItems = new Dictionary<int, ImportCompanyItem>();
            foreach (var company in companies.Where(c => !string.IsNullOrWhiteSpace(c.CompanyName)))
            {
                var item = new ImportCompanyItem(company)
                {
                    ExistsInTarget = index.CompanyIds.ContainsKey(Normalise(company.CompanyName))
                };
                companyItems[company.ID] = item;
                source.Companies.Add(item);
            }

            // People that do not belong to a company that exists in the source cannot be imported (a company is required).
            foreach (var person in people)
            {
                if (!companyItems.TryGetValue(person.CompanyID, out var companyItem))
                {
                    _logger.LogDebug("Ignoring {person} in {source}: company {companyId} not found", person.FullName, sourcePath, person.CompanyID);
                    continue;
                }

                index.CompanyIds.TryGetValue(Normalise(companyItem.Company.CompanyName), out var targetCompanyId);

                companyItem.People.Add(new ImportPersonItem(person, companyItem)
                {
                    ExistsInTarget = index.PersonExists(person, targetCompanyId)
                });
            }
        }

        source.Companies.Sort((a, b) => string.Compare(a.Company.CompanyName, b.Company.CompanyName, StringComparison.OrdinalIgnoreCase));

        // Default selection: everything that is not already in the target.
        foreach (var companyItem in source.Companies)
        {
            companyItem.People.Sort((a, b) => string.Compare(a.Person.FullNameReversed, b.Person.FullNameReversed, StringComparison.OrdinalIgnoreCase));

            if (!companyItem.ExistsInTarget)
            {
                companyItem.IsSelected = true;
            }

            foreach (var personItem in companyItem.People)
            {
                personItem.IsSelected = !personItem.ExistsInTarget;
            }
        }

        return source;
    }

    public DirectoryImportResult Import(DirectoryImportSource source)
    {
        var result = new DirectoryImportResult();
        var index = TargetIndex.Build(_contactDirectoryService);

        foreach (var companyItem in source.Companies)
        {
            var selectedPeople = companyItem.People.Where(p => p.IsSelected).ToList();
            if (!companyItem.IsSelected && selectedPeople.Count == 0)
            {
                continue;
            }

            var key = Normalise(companyItem.Company.CompanyName);
            if (index.CompanyIds.TryGetValue(key, out var targetCompanyId))
            {
                result.CompaniesReused++;
            }
            else
            {
                var newCompany = new CompanyModel
                {
                    CompanyName = companyItem.Company.CompanyName.Trim(),
                    Role = companyItem.Company.Role,
                    OrganisationCode = companyItem.Company.OrganisationCode,
                    Address = companyItem.Company.Address,
                    Tel = companyItem.Company.Tel,
                    Fax = companyItem.Company.Fax,
                    Website = companyItem.Company.Website
                };

                _contactDirectoryService.CreateCompany(newCompany);

                if (newCompany.ID <= 0)
                {
                    _logger.LogWarning("Company {company} could not be imported", newCompany.CompanyName);
                    result.Failures += 1 + selectedPeople.Count;
                    continue;
                }

                targetCompanyId = newCompany.ID;
                index.CompanyIds[key] = targetCompanyId;
                result.CompaniesCreated++;
            }

            foreach (var personItem in selectedPeople)
            {
                if (index.PersonExists(personItem.Person, targetCompanyId))
                {
                    result.PeopleSkipped++;
                    continue;
                }

                var newPerson = new PersonModel
                {
                    LastName = personItem.Person.LastName,
                    FirstName = personItem.Person.FirstName,
                    Email = personItem.Person.Email,
                    Tel = personItem.Person.Tel,
                    Mobile = personItem.Person.Mobile,
                    Position = personItem.Person.Position,
                    Notes = personItem.Person.Notes,
                    CompanyID = targetCompanyId,
                    ShowInReport = personItem.Person.ShowInReport,
                    Archive = personItem.Person.Archive
                };

                _contactDirectoryService.CreatePerson(newPerson);

                if (newPerson.ID <= 0)
                {
                    _logger.LogWarning("Person {person} could not be imported", newPerson.FullName);
                    result.Failures++;
                    continue;
                }

                index.AddPerson(newPerson);
                result.PeopleCreated++;
            }
        }

        _logger.LogInformation("Directory import from {source}: {created} companies created, {reused} reused, {people} people created, {skipped} skipped, {failures} failures",
            source.FilePath, result.CompaniesCreated, result.CompaniesReused, result.PeopleCreated, result.PeopleSkipped, result.Failures);

        return result;
    }

    private void ReadProjectDetails(string sourcePath, List<string> tables, DirectoryImportSource source)
    {
        if (!tables.Contains("Settings", StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var sql = BuildSelect(sourcePath, "Settings", new Dictionary<string, string>
        {
            ["ProjectNumber"] = "NULL",
            ["ProjectName"] = "NULL"
        }) + " LIMIT 1";

        var row = _connection.LoadData<ProjectDetailsRow, object>(sourcePath, sql, null).FirstOrDefault();
        source.ProjectNumber = row?.ProjectNumber ?? string.Empty;
        source.ProjectName = row?.ProjectName ?? string.Empty;
    }

    private sealed class ProjectDetailsRow
    {
        public string ProjectNumber { get; set; }
        public string ProjectName { get; set; }
    }

    /// <summary>
    /// Builds a SELECT for the listed columns. A column with a null fallback is required; any other column that is
    /// missing from the table (older database) is replaced by its fallback expression.
    /// </summary>
    private string BuildSelect(string sourcePath, string table, Dictionary<string, string> columnsWithFallback)
    {
        var existing = new HashSet<string>(
            _connection.LoadData<string, object>(sourcePath, $"SELECT name FROM pragma_table_info('{table}');", null),
            StringComparer.OrdinalIgnoreCase);

        var parts = new List<string>();
        foreach (var column in columnsWithFallback)
        {
            if (existing.Contains(column.Key))
            {
                parts.Add($"\"{column.Key}\"");
            }
            else if (column.Value == null)
            {
                throw new InvalidOperationException($"The selected file is not a valid Transmittal database ({table}.{column.Key} is missing).");
            }
            else
            {
                parts.Add($"{column.Value} AS \"{column.Key}\"");
            }
        }

        return $"SELECT {string.Join(", ", parts)} FROM \"{table}\"";
    }

    private static string Normalise(string value) => (value ?? string.Empty).Trim();

    /// <summary>
    /// Lookup of what is already in the target database.
    /// </summary>
    private sealed class TargetIndex
    {
        public Dictionary<string, int> CompanyIds { get; } = new(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _emails = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _nameKeys = new(StringComparer.OrdinalIgnoreCase);

        public static TargetIndex Build(IContactDirectoryService contactDirectoryService)
        {
            var index = new TargetIndex();

            foreach (var company in contactDirectoryService.GetCompanies_All())
            {
                var name = Normalise(company.CompanyName);
                if (name.Length > 0 && !index.CompanyIds.ContainsKey(name))
                {
                    index.CompanyIds[name] = company.ID;
                }
            }

            foreach (var person in contactDirectoryService.GetPeople_All())
            {
                index.AddPerson(person);
            }

            return index;
        }

        public void AddPerson(PersonModel person)
        {
            var email = Normalise(person.Email);
            if (email.Length > 0)
            {
                _emails.Add(email);
            }

            _nameKeys.Add(NameKey(person, person.CompanyID));
        }

        /// <param name="targetCompanyId">The person's company in the target database, or 0 if the company is not in the target.</param>
        public bool PersonExists(PersonModel person, int targetCompanyId)
        {
            var email = Normalise(person.Email);
            if (email.Length > 0 && _emails.Contains(email))
            {
                return true;
            }

            return targetCompanyId > 0 && _nameKeys.Contains(NameKey(person, targetCompanyId));
        }

        private static string NameKey(PersonModel person, int companyId) =>
            $"{companyId}|{Normalise(person.FirstName)}|{Normalise(person.LastName)}";
    }
}
