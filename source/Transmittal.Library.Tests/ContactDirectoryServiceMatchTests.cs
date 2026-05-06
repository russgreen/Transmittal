using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Models;
using Transmittal.Library.Services;

namespace Transmittal.Library.Tests;

public class ContactDirectoryServiceMatchTests
{
    [Test]
    public async Task FindCompanyMatches_ShouldReturnExactMatchFirst()
    {
        var companies = new List<CompanyModel>
        {
            new() { ID = 1, CompanyName = "Acme Limited" },
            new() { ID = 2, CompanyName = "Acme Ltd" },
            new() { ID = 3, CompanyName = "Beta Design" }
        };

        var sut = CreateSut(companies, []);

        var matches = sut.FindCompanyMatches("Acme Ltd");

        await Assert.That(matches.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(matches[0].ID).IsEqualTo(2);
    }

    [Test]
    public async Task FindPersonMatches_ShouldReturnEmailMatchFirst()
    {
        var people = new List<PersonModel>
        {
            new() { ID = 10, FirstName = "John", LastName = "Smith", Email = "john.smith@acme.com", CompanyID = 1 },
            new() { ID = 11, FirstName = "Jon", LastName = "Smyth", Email = "", CompanyID = 1 }
        };

        var sut = CreateSut([], people);

        var matches = sut.FindPersonMatches("Johnny", "Smithe", "john.smith@acme.com");

        await Assert.That(matches.Count).IsEqualTo(1);
        await Assert.That(matches[0].ID).IsEqualTo(10);
    }

    [Test]
    public async Task FindPersonMatches_ShouldPrioritizeSameCompany()
    {
        var people = new List<PersonModel>
        {
            new() { ID = 20, FirstName = "Jane", LastName = "Doe", Email = "jane@one.com", CompanyID = 1 },
            new() { ID = 21, FirstName = "Jane", LastName = "Doe", Email = "jane@two.com", CompanyID = 2 }
        };

        var sut = CreateSut([], people);

        var matches = sut.FindPersonMatches("Jane", "Doe", "", 2);

        await Assert.That(matches.Count).IsEqualTo(1);
        await Assert.That(matches[0].ID).IsEqualTo(21);
    }

    [Test]
    public async Task FindPersonMatches_ShouldDetectSwappedFirstAndLastName()
    {
        var people = new List<PersonModel>
        {
            new() { ID = 30, FirstName = "Smith", LastName = "John", Email = "", CompanyID = 1 },
            new() { ID = 31, FirstName = "John", LastName = "Smythe", Email = "", CompanyID = 1 }
        };

        var sut = CreateSut([], people);

        var matches = sut.FindPersonMatches("John", "Smith", "", 1);

        await Assert.That(matches.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(matches[0].ID).IsEqualTo(30);
    }

    private static ContactDirectoryService CreateSut(List<CompanyModel> companies, List<PersonModel> people)
    {
        var dataConnection = new InMemoryDataConnection(companies, people);
        var settingsService = new TestSettingsService();
        var messageBox = new TestMessageBoxService();

        return new ContactDirectoryService(
            dataConnection,
            settingsService,
            NullLogger<ContactDirectoryService>.Instance,
            messageBox);
    }

    private sealed class InMemoryDataConnection : IDataConnection
    {
        private readonly List<CompanyModel> _companies;
        private readonly List<PersonModel> _people;

        public InMemoryDataConnection(List<CompanyModel> companies, List<PersonModel> people)
        {
            _companies = companies;
            _people = people;
        }

        public bool CheckConnection(string dbFilePath) => true;
        public T CreateData<T, U>(string dbFilePath, string sqlStatement, T model, U parameters, string keyPropertyName) => model;
        public void SaveData<T>(string dbFilePath, string sqlStatement, T data) { }
        public void BeginTransaction(string dbFilePath) { }
        public void CommitTransaction() { }
        public void RollbackTransaction() { }
        public void ExecuteInTransaction<T>(string sqlStatement, T parameters) { }
        public void UpgradeDatabase(string dbFilePath) { }
        public void CreateDatabaseSchema(string dbFilePath) { }

        public IEnumerable<T> LoadData<T, U>(string dbFilePath, string sqlStatement, U parameters)
        {
            if (typeof(T) == typeof(CompanyModel))
            {
                return (IEnumerable<T>)_companies.OrderBy(x => x.CompanyName).ToList();
            }

            if (typeof(T) == typeof(PersonModel))
            {
                if (sqlStatement.Contains("WHERE CompanyID = @CompanyID", StringComparison.OrdinalIgnoreCase))
                {
                    var companyId = (int?)parameters?.GetType().GetProperty("CompanyID")?.GetValue(parameters) ?? 0;
                    return (IEnumerable<T>)_people.Where(x => x.CompanyID == companyId).ToList();
                }

                return (IEnumerable<T>)_people.ToList();
            }

            return [];
        }
    }

    private sealed class TestSettingsService : ISettingsService
    {
        public SettingsModel GlobalSettings { get; set; } = new()
        {
            DatabaseFile = "test.tdb"
        };

        public void GetSettings() { }
        public void UpdateSettings() { }
    }

    private sealed class TestMessageBoxService : IMessageBoxService
    {
        public bool ShowYesNo(string title, string message) => false;
        public bool ShowOkCancel(string title, string message) => true;
        public bool ShowCancel(string title, string message) => true;
        public bool ShowOk(string title, string message) => true;
    }
}
