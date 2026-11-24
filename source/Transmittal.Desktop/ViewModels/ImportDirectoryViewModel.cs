using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.ComponentModel;
using Transmittal.Desktop.Services;
using Transmittal.Library.Models.DirectoryImport;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;

namespace Transmittal.Desktop.ViewModels;

internal partial class ImportDirectoryViewModel : BaseViewModel
{
    private readonly IDirectoryImportService _directoryImportService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ILogger<ImportDirectoryViewModel> _logger;

    public DirectoryImportSource Source { get; private set; }

    public string Header { get; private set; } = string.Empty;

    /// <summary>
    /// True once the import has run, so the owner knows to refresh.
    /// </summary>
    public bool ImportCompleted { get; private set; }

    [ObservableProperty]
    private string _summary = string.Empty;

    public ImportDirectoryViewModel(IDirectoryImportService directoryImportService,
        IMessageBoxService messageBoxService,
        ILogger<ImportDirectoryViewModel> logger)
    {
        _directoryImportService = directoryImportService;
        _messageBoxService = messageBoxService;
        _logger = logger;
    }

    public void Initialise(DirectoryImportSource source)
    {
        Source = source;

        var project = string.Join(" - ", new[] { source.ProjectNumber, source.ProjectName }.Where(x => !string.IsNullOrWhiteSpace(x)));
        Header = string.IsNullOrEmpty(project)
            ? source.FilePath
            : $"{project} ({source.FilePath})";

        foreach (var company in source.Companies)
        {
            company.PropertyChanged += Item_PropertyChanged;
            foreach (var person in company.People)
            {
                person.PropertyChanged += Item_PropertyChanged;
            }
        }

        RefreshSummary();
    }

    private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ImportCompanyItem.IsSelected))
        {
            RefreshSummary();
        }
    }

    private int CompaniesToAdd => Source.Companies.Count(c => !c.ExistsInTarget && (c.IsSelected || c.People.Any(p => p.IsSelected)));

    private int PeopleToAdd => Source.Companies.SelectMany(c => c.People).Count(p => p.IsSelected && !p.ExistsInTarget);

    private bool CanImport() => Source != null && (CompaniesToAdd > 0 || PeopleToAdd > 0);

    private void RefreshSummary()
    {
        var companies = CompaniesToAdd;
        var people = PeopleToAdd;

        Summary = companies == 0 && people == 0
            ? "Nothing selected to import."
            : $"{companies} {(companies == 1 ? "company" : "companies")} and {people} {(people == 1 ? "person" : "people")} will be added.";

        ImportCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var company in Source.Companies)
        {
            company.IsSelected = true;
        }
    }

    [RelayCommand]
    private void SelectNone()
    {
        foreach (var company in Source.Companies)
        {
            company.IsSelected = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanImport))]
    private void Import()
    {
        using (LogContext.PushProperty("UsageTracking", true))
        {
            _logger.LogInformation("{command}", nameof(ImportCommand));
        }

        try
        {
            var result = _directoryImportService.Import(Source);
            ImportCompleted = true;

            var message = $"Added {result.CompaniesCreated} {(result.CompaniesCreated == 1 ? "company" : "companies")} and {result.PeopleCreated} {(result.PeopleCreated == 1 ? "person" : "people")}.";
            if (result.PeopleSkipped > 0)
            {
                message += $"\n{result.PeopleSkipped} already existed and {(result.PeopleSkipped == 1 ? "was" : "were")} skipped.";
            }
            if (result.Failures > 0)
            {
                message += $"\n{result.Failures} could not be imported. See the log for details.";
            }

            _messageBoxService.ShowOk("Import complete", message);
            OnClosingRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import directory from {source}", Source.FilePath);
            _messageBoxService.ShowOk("Import failed", ex.Message);
        }
    }
}
