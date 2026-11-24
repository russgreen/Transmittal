using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Models;
using Transmittal.Library.Services;

namespace Transmittal.Library.Tests;

public class DirectoryImportServiceTests
{
    private string _sourcePath = string.Empty;
    private string _targetPath = string.Empty;
    private SQLiteDataAccess _dataAccess = null!;
    private DirectoryImportService _service = null!;

    [Before(Test)]
    public async Task Setup()
    {
        var id = Guid.NewGuid().ToString("N");
        _sourcePath = Path.Combine(Path.GetTempPath(), $"transmittal-import-source-{id}.tdb");
        _targetPath = Path.Combine(Path.GetTempPath(), $"transmittal-import-target-{id}.tdb");

        _dataAccess = new SQLiteDataAccess(new Mock<ILogger<SQLiteDataAccess>>().Object, new Mock<IMessageBoxService>().Object);
        _dataAccess.CreateDatabaseSchema(_targetPath);
        // Copy rather than create so the source is never registered in the recent files list by test setup.
        File.Copy(_targetPath, _sourcePath);

        var settings = new TestSettingsService { GlobalSettings = new SettingsModel { DatabaseFile = _targetPath } };
        var directory = new ContactDirectoryService(
            _dataAccess,
            settings,
            new Mock<ILogger<ContactDirectoryService>>().Object,
            new Mock<IMessageBoxService>().Object);

        _service = new DirectoryImportService(_dataAccess, directory, settings, new Mock<ILogger<DirectoryImportService>>().Object);
        await Task.CompletedTask;
    }

    [After(Test)]
    public async Task Cleanup()
    {
        SqliteConnection.ClearAllPools();
        foreach (var path in new[] { _sourcePath, _targetPath })
        {
            DeleteIfExists(path);
            DeleteIfExists($"{path}-wal");
            DeleteIfExists($"{path}-shm");
        }
        await Task.CompletedTask;
    }

    [Test]
    public async Task Import_ShouldCopyNewCompanyWithAllDetailsAndRemapPeopleToNewCompanyId()
    {
        // Make the source company ID differ from whatever the target assigns.
        InsertCompany(_targetPath, "Existing Co");
        var sourceCompany = InsertCompany(_sourcePath, "Acme Ltd", role: "Architect", address: "1 High St", tel: "0123", fax: "0456", website: "acme.example", orgCode: "ACM");
        InsertPerson(_sourcePath, sourceCompany, "Smith", "Jane", email: "jane@acme.example", position: "Director", notes: "Lead");

        var source = _service.LoadSource(_sourcePath);
        var result = _service.Import(source);

        await Assert.That(result.CompaniesCreated).IsEqualTo(1);
        await Assert.That(result.PeopleCreated).IsEqualTo(1);

        var company = QueryTarget<CompanyModel>("SELECT * FROM Company WHERE CompanyName = 'Acme Ltd';").Single();
        await Assert.That(company.Role).IsEqualTo("Architect");
        await Assert.That(company.Address).IsEqualTo("1 High St");
        await Assert.That(company.Tel).IsEqualTo("0123");
        await Assert.That(company.Fax).IsEqualTo("0456");
        await Assert.That(company.Website).IsEqualTo("acme.example");
        await Assert.That(company.OrganisationCode).IsEqualTo("ACM");

        var person = QueryTarget<PersonModel>("SELECT * FROM Person;").Single();
        await Assert.That(person.CompanyID).IsEqualTo(company.ID);
        await Assert.That(person.Position).IsEqualTo("Director");
        await Assert.That(person.Notes).IsEqualTo("Lead");
    }

    [Test]
    public async Task Import_ShouldReuseExistingCompanyMatchedByNameIgnoringCaseAndWhitespace()
    {
        var targetCompany = InsertCompany(_targetPath, "Acme Ltd");
        var sourceCompany = InsertCompany(_sourcePath, "  ACME LTD ");
        InsertPerson(_sourcePath, sourceCompany, "Smith", "Jane", email: "jane@acme.example");

        var source = _service.LoadSource(_sourcePath);
        var result = _service.Import(source);

        await Assert.That(result.CompaniesCreated).IsEqualTo(0);
        await Assert.That(result.CompaniesReused).IsEqualTo(1);
        await Assert.That(QueryTarget<int>("SELECT COUNT(*) FROM Company;").Single()).IsEqualTo(1);

        var person = QueryTarget<PersonModel>("SELECT * FROM Person;").Single();
        await Assert.That(person.CompanyID).IsEqualTo(targetCompany);
    }

    [Test]
    public async Task Import_ShouldSkipPeopleMatchedByEmail()
    {
        var targetCompany = InsertCompany(_targetPath, "Acme Ltd");
        InsertPerson(_targetPath, targetCompany, "Smith", "Jane", email: "Jane@Acme.example");

        var sourceCompany = InsertCompany(_sourcePath, "Acme Ltd");
        // Different name, same email (case-insensitive).
        InsertPerson(_sourcePath, sourceCompany, "Smyth", "J", email: "jane@acme.example");

        var source = _service.LoadSource(_sourcePath);
        await Assert.That(source.Companies.Single().People.Single().ExistsInTarget).IsTrue();

        // Force the existing person to be ticked so the import itself has to skip it.
        source.Companies.Single().People.Single().IsSelected = true;
        var result = _service.Import(source);

        await Assert.That(result.PeopleCreated).IsEqualTo(0);
        await Assert.That(result.PeopleSkipped).IsEqualTo(1);
        await Assert.That(QueryTarget<int>("SELECT COUNT(*) FROM Person;").Single()).IsEqualTo(1);
    }

    [Test]
    public async Task Import_ShouldSkipPeopleMatchedByNameUnderSameCompany_ButImportSameNameUnderDifferentCompany()
    {
        var targetAcme = InsertCompany(_targetPath, "Acme Ltd");
        InsertPerson(_targetPath, targetAcme, "Smith", "Jane");

        var sourceAcme = InsertCompany(_sourcePath, "Acme Ltd");
        InsertPerson(_sourcePath, sourceAcme, "SMITH", "jane");
        var sourceOther = InsertCompany(_sourcePath, "Other Co");
        InsertPerson(_sourcePath, sourceOther, "Smith", "Jane");

        var source = _service.LoadSource(_sourcePath);
        var result = _service.Import(source);

        await Assert.That(result.PeopleCreated).IsEqualTo(1);
        await Assert.That(QueryTarget<int>("SELECT COUNT(*) FROM Person;").Single()).IsEqualTo(2);

        var otherId = QueryTarget<int>("SELECT ID FROM Company WHERE CompanyName = 'Other Co';").Single();
        await Assert.That(QueryTarget<int>("SELECT COUNT(*) FROM Person WHERE CompanyID = @id;", new { id = otherId }).Single()).IsEqualTo(1);
    }

    [Test]
    public async Task Import_ShouldNotDuplicateWhenRunTwice()
    {
        var sourceCompany = InsertCompany(_sourcePath, "Acme Ltd");
        InsertPerson(_sourcePath, sourceCompany, "Smith", "Jane", email: "jane@acme.example");

        _service.Import(_service.LoadSource(_sourcePath));
        var secondSource = _service.LoadSource(_sourcePath);
        var second = _service.Import(secondSource);

        await Assert.That(second.CompaniesCreated).IsEqualTo(0);
        await Assert.That(second.PeopleCreated).IsEqualTo(0);
        await Assert.That(QueryTarget<int>("SELECT COUNT(*) FROM Company;").Single()).IsEqualTo(1);
        await Assert.That(QueryTarget<int>("SELECT COUNT(*) FROM Person;").Single()).IsEqualTo(1);
    }

    [Test]
    public async Task Import_ShouldOnlyImportSelectedPeople_AndCopyArchiveAndReportFlags()
    {
        var sourceCompany = InsertCompany(_sourcePath, "Acme Ltd");
        InsertPerson(_sourcePath, sourceCompany, "Smith", "Jane", archive: true, showInReport: false);
        InsertPerson(_sourcePath, sourceCompany, "Jones", "Bob");

        var source = _service.LoadSource(_sourcePath);
        source.Companies.Single().People.Single(p => p.Person.LastName == "Jones").IsSelected = false;

        var result = _service.Import(source);

        await Assert.That(result.PeopleCreated).IsEqualTo(1);
        var person = QueryTarget<PersonModel>("SELECT * FROM Person;").Single();
        await Assert.That(person.LastName).IsEqualTo("Smith");
        await Assert.That(person.Archive).IsTrue();
        await Assert.That(person.ShowInReport).IsFalse();
    }

    [Test]
    public async Task Import_ShouldImportCompanyOnly_WhenNoPeopleSelected()
    {
        var sourceCompany = InsertCompany(_sourcePath, "Acme Ltd");
        InsertPerson(_sourcePath, sourceCompany, "Smith", "Jane");

        var source = _service.LoadSource(_sourcePath);
        foreach (var person in source.Companies.Single().People)
        {
            person.IsSelected = false;
        }

        var result = _service.Import(source);

        await Assert.That(result.CompaniesCreated).IsEqualTo(1);
        await Assert.That(result.PeopleCreated).IsEqualTo(0);
    }

    [Test]
    public async Task Selection_ShouldTickCompanyWhenPersonTicked_AndTickAllPeopleWhenCompanyTicked()
    {
        var targetCompany = InsertCompany(_targetPath, "Acme Ltd");
        InsertPerson(_targetPath, targetCompany, "Existing", "Eve", email: "eve@acme.example");

        var sourceCompany = InsertCompany(_sourcePath, "Acme Ltd");
        InsertPerson(_sourcePath, sourceCompany, "Existing", "Eve", email: "eve@acme.example");
        InsertPerson(_sourcePath, sourceCompany, "Smith", "Jane");
        InsertPerson(_sourcePath, sourceCompany, "Jones", "Bob");

        var company = _service.LoadSource(_sourcePath).Companies.Single();

        // Existing company with new people: the new people default to ticked, which ticks the company.
        await Assert.That(company.IsSelected).IsTrue();
        await Assert.That(company.People.Single(p => p.Person.LastName == "Existing").IsSelected).IsFalse();

        // Unticking a person leaves the company and the other people alone.
        company.People.Single(p => p.Person.LastName == "Smith").IsSelected = false;
        await Assert.That(company.IsSelected).IsTrue();
        await Assert.That(company.People.Single(p => p.Person.LastName == "Jones").IsSelected).IsTrue();

        // Unticking the company unticks everyone; ticking it ticks everyone.
        company.IsSelected = false;
        await Assert.That(company.People.Any(p => p.IsSelected)).IsFalse();
        company.IsSelected = true;
        await Assert.That(company.People.All(p => p.IsSelected)).IsTrue();

        // Ticking a single person under an unticked company ticks only the company.
        company.IsSelected = false;
        company.People.Single(p => p.Person.LastName == "Smith").IsSelected = true;
        await Assert.That(company.IsSelected).IsTrue();
        await Assert.That(company.People.Count(p => p.IsSelected)).IsEqualTo(1);
    }

    [Test]
    public async Task LoadSourceAndImport_ShouldNotModifyTheSourceDatabase()
    {
        var sourceCompany = InsertCompany(_sourcePath, "Acme Ltd");
        InsertPerson(_sourcePath, sourceCompany, "Smith", "Jane");
        var bytesBefore = File.ReadAllBytes(_sourcePath);

        // Importing writes to the target only.
        _service.Import(_service.LoadSource(_sourcePath));
        await Assert.That(File.ReadAllBytes(_sourcePath).SequenceEqual(bytesBefore)).IsTrue();
    }

    [Test]
    public async Task LoadSource_ShouldReadProjectDetails()
    {
        using (var conn = OpenConnection(_sourcePath))
        {
            conn.Execute("UPDATE Settings SET ProjectNumber = 'P123', ProjectName = 'Big Project' WHERE ID = 1;");
        }

        var source = _service.LoadSource(_sourcePath);

        await Assert.That(source.ProjectNumber).IsEqualTo("P123");
        await Assert.That(source.ProjectName).IsEqualTo("Big Project");
    }

    [Test]
    public async Task LoadSource_ShouldLoadDatabaseWithoutNewerColumns()
    {
        // Simulate a pre-v4 source: no OrganisationCode on Company.
        using (var conn = OpenConnection(_sourcePath))
        {
            conn.Execute("ALTER TABLE Company DROP COLUMN OrganisationCode;");
            conn.Execute("INSERT INTO Company (CompanyName) VALUES ('Old Co');");
        }

        var source = _service.LoadSource(_sourcePath);

        await Assert.That(source.Companies.Single().Company.CompanyName).IsEqualTo("Old Co");
        await Assert.That(source.Companies.Single().Company.OrganisationCode).IsNull();
    }

    [Test]
    public async Task LoadSource_ShouldRejectTheOpenDatabase()
    {
        await Assert.That(() => _service.LoadSource(_targetPath)).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task LoadSource_ShouldRejectFileThatIsNotATransmittalDatabase()
    {
        using (var conn = OpenConnection(_sourcePath))
        {
            conn.Execute("DROP TABLE Person;");
            conn.Execute("DROP TABLE Company;");
        }

        await Assert.That(() => _service.LoadSource(_sourcePath)).Throws<InvalidOperationException>();
    }

    private int InsertCompany(string dbPath, string name, string? role = null, string? address = null,
        string? tel = null, string? fax = null, string? website = null, string? orgCode = null)
    {
        using var conn = OpenConnection(dbPath);
        return conn.QuerySingle<int>(
            "INSERT INTO Company (CompanyName, Role, OrganisationCode, Address, Tel, Fax, Website) " +
            "VALUES (@name, @role, @orgCode, @address, @tel, @fax, @website); SELECT last_insert_rowid();",
            new { name, role, orgCode, address, tel, fax, website });
    }

    private int InsertPerson(string dbPath, int companyId, string lastName, string firstName, string? email = null,
        string? position = null, string? notes = null, bool archive = false, bool showInReport = true)
    {
        using var conn = OpenConnection(dbPath);
        return conn.QuerySingle<int>(
            "INSERT INTO Person (LastName, FirstName, Email, Position, Notes, CompanyID, ShowInReport, Archive) " +
            "VALUES (@lastName, @firstName, @email, @position, @notes, @companyId, @showInReport, @archive); SELECT last_insert_rowid();",
            new { lastName, firstName, email, position, notes, companyId, showInReport, archive });
    }

    private List<T> QueryTarget<T>(string sql, object? parameters = null)
    {
        using var conn = OpenConnection(_targetPath);
        return conn.Query<T>(sql, parameters).ToList();
    }

    private static SqliteConnection OpenConnection(string dbPath)
    {
        var conn = new SqliteConnection($"Data Source={dbPath};Pooling=False");
        conn.Open();
        return conn;
    }

    private static void DeleteIfExists(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
            // best effort cleanup of temp files
        }
    }

    private sealed class TestSettingsService : ISettingsService
    {
        public SettingsModel GlobalSettings { get; set; } = new();

        public void GetSettings() { }
        public void UpdateSettings() { }
    }
}
