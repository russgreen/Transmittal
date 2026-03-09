using ClosedXML.Excel;
using System.Reflection;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Reports.OpenXML;

namespace Transmittal.Reports.OpenXML.Tests;

internal static class ReportsTestHelpers
{
    internal static Reports CreateSut(
        ISettingsService? settingsService = null,
        IContactDirectoryService? contactService = null,
        ITransmittalService? transmittalService = null)
    {
        settingsService ??= new FakeSettingsService();
        contactService ??= new FakeContactDirectoryService();
        transmittalService ??= new FakeTransmittalService();
        return new Reports(settingsService, contactService, transmittalService);
    }

    internal static int FindRowByValue(IXLWorksheet worksheet, int column, string value)
    {
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        for (var row = 1; row <= lastRow; row++)
        {
            if (worksheet.Cell(row, column).GetString().Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                return row;
            }
        }

        return 0;
    }

    internal static T InvokeInstancePrivate<T>(Reports instance, string methodName, Type[] parameterTypes, params object[] args)
    {
        var method = typeof(Reports).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: parameterTypes,
            modifiers: null);

        if (method == null)
        {
            throw new InvalidOperationException($"Method '{methodName}' not found.");
        }

        var result = method.Invoke(instance, args);
        return result is T typed ? typed : (T)result!;
    }

    internal static T InvokeStaticPrivate<T>(Type type, string methodName, Type[] parameterTypes, params object[] args)
    {
        var method = type.GetMethod(
            methodName,
            BindingFlags.Static | BindingFlags.NonPublic,
            binder: null,
            types: parameterTypes,
            modifiers: null);

        if (method == null)
        {
            throw new InvalidOperationException($"Method '{methodName}' not found.");
        }

        var result = method.Invoke(null, args);
        return result is T typed ? typed : (T)result!;
    }

    internal static void InvokePopulateRowsFromNamedRange<T>(
        Reports instance,
        IXLWorksheet worksheet,
        IXLRange templateRange,
        IReadOnlyList<T> rows,
        Func<T, Dictionary<string, string>> contextFactory)
    {
        var method = typeof(Reports).GetMethod(
            "PopulateRowsFromNamedRange",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (method == null)
        {
            throw new InvalidOperationException("Method 'PopulateRowsFromNamedRange' not found.");
        }

        var generic = method.MakeGenericMethod(typeof(T));
        generic.Invoke(instance, [worksheet, templateRange, rows, contextFactory]);
    }
}

internal sealed class FakeSettingsService : ISettingsService
{
    public SettingsModel GlobalSettings { get; set; } = new()
    {
        ProjectNumber = "0001",
        ProjectIdentifier = "PRJ",
        ProjectName = "Test Project",
        ClientName = "Client",
        Originator = "ORI",
        Role = "A",
        FileNameFilter = "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Status>-<Rev>",
        IssueSheetStore = Path.Combine(Path.GetTempPath(), "Transmittal.Tests"),
        ReportStore = string.Empty
    };

    public void GetSettings()
    {
    }

    public void UpdateSettings()
    {
    }
}

internal sealed class FakeContactDirectoryService : IContactDirectoryService
{
    private readonly Dictionary<int, PersonModel> _people = new();
    private readonly Dictionary<int, CompanyModel> _companies = new();

    public void AddPerson(PersonModel person) => _people[person.ID] = person;

    public void AddCompany(CompanyModel company) => _companies[company.ID] = company;

    public List<CompanyModel> GetCompanies_All() => _companies.Values.ToList();
    public CompanyModel GetCompany(int companyID) => _companies[companyID];
    public void CreateCompany(CompanyModel model) => _companies[model.ID] = model;
    public void UpdateCompany(CompanyModel model) => _companies[model.ID] = model;
    public void DeleteCompany(CompanyModel model) => _companies.Remove(model.ID);
    public List<PersonModel> GetPeople_ByCompany(int companyID) => _people.Values.Where(p => p.CompanyID == companyID).ToList();
    public List<PersonModel> GetPeople_All() => _people.Values.ToList();
    public PersonModel GetPerson(int personID) => _people[personID];
    public void CreatePerson(PersonModel model) => _people[model.ID] = model;
    public void UpdatePerson(PersonModel model) => _people[model.ID] = model;
    public void DeletePerson(PersonModel model) => _people.Remove(model.ID);
    public List<ProjectDirectoryModel> GetProjectDirectory(bool IncludeArchivedUsers = true) => new();
}

internal sealed class FakeTransmittalService : ITransmittalService
{
    public List<TransmittalModel> GetTransmittals() => new();
    public List<TransmittalModel> GetTransmittals_ByPerson(int personID) => new();
    public TransmittalModel GetTransmittal(int transmittalID) => new();
    public List<TransmittalItemModel> GetTransmittalItems_ByTransmittal(int transmittalID) => new();
    public List<TransmittalDistributionModel> GetTransmittalDistributions_ByTransmittal(int transmittalID) => new();
    public List<string> GetPackages() => new();
    public void CreateTransmittal(TransmittalModel model) { }
    public void UpdateTransmittal(TransmittalModel model) { }
    public void DeleteTransmittal(TransmittalModel model) { }
    public TransmittalModel MergeTransmittals(List<TransmittalModel> transmittalsToMerge) => new();
    public void CreateTransmittalItem(TransmittalItemModel model) { }
    public void UpdateTransmittalItem(TransmittalItemModel model) { }
    public void DeleteTransmittalItem(TransmittalItemModel model) { }
    public void CreateTransmittalDist(TransmittalDistributionModel model) { }
    public void UpdateTransmittalDist(TransmittalDistributionModel model) { }
    public void DeleteTransmittalDist(TransmittalDistributionModel model) { }
    public void CreateTransmittalItems(List<TransmittalItemModel> models) { }
    public void CreateTransmittalDistributions(List<TransmittalDistributionModel> models) { }
}
