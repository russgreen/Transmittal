using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Transmittal.Library.Helpers;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;

namespace Transmittal.FileRenamer.ViewModels;

internal partial class MainViewModel : BaseViewModel
{
    public string WindowTitle { get; private set; } = string.Empty;

    private readonly ILogger<MainViewModel> _logger;
    private readonly ISettingsService _settingsService;
    private readonly IMessageBoxService _messageBoxService;

    [ObservableProperty]
    private string _originalFileName = string.Empty;

    [ObservableProperty]
    private string _newFileName = string.Empty;

    [ObservableProperty]
    private DocumentModel _document;

    [ObservableProperty]
    private List<DocumentTypeModel> _documentTypes = new();

    [ObservableProperty]
    private List<RoleModel> _roles = new();

    [ObservableProperty]
    private List<DocumentStatusModel> _statuses = new();

    //[ObservableProperty]
    //private DocumentTypeModel _documentType;

    //[ObservableProperty]
    //private RoleModel _role;

    [ObservableProperty]
    private bool _hasStatus = true;

    //[ObservableProperty]
    //private DocumentStatusModel _status;

    [ObservableProperty]
    private bool _hasRevision = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RevisionPrefix))]
    [NotifyPropertyChangedFor(nameof(DocumentRevision))]
    private bool _preliminaryRevision = true;

    public string RevisionPrefix => PreliminaryRevision ? "P" : "C";

    [ObservableProperty]
    [Required]
    [MaxLength(3)]
    [NotifyPropertyChangedFor(nameof(DocumentRevision))]
    private string _revNo = "01";

    public string DocumentRevision => $"{RevisionPrefix}{RevNo}";

    public MainViewModel()
    {
        _logger = null!;
    }

    public MainViewModel(ILogger<MainViewModel> logger,
        ISettingsService settingsService,
        IMessageBoxService messageBoxService)
    {
        _logger = logger;
        _settingsService = settingsService;
        _messageBoxService = messageBoxService;

        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        WindowTitle = $"Transmittal Renamer {informationVersion}";

        //populate the dropdowns lists
        DocumentTypes = ISO19650.GetDocumentTypes();
        Roles = ISO19650.GetRoles();
        Statuses = ISO19650.GetDocumentStatuses();

        LoadDocument(App.Args[0]);
    }

    private void LoadDocument(string file)
    {
        var projectIdentifier = string.Empty;

        //check if we're using the project identifier on this project
        if (_settingsService.GlobalSettings.ProjectIdentifier is null || _settingsService.GlobalSettings.ProjectIdentifier == string.Empty)
        {
            projectIdentifier = _settingsService.GlobalSettings.ProjectNumber;
        }
        else
        {
            projectIdentifier = _settingsService.GlobalSettings.ProjectIdentifier;
        }

        Document = Library.Helpers.FilenameParser.GetDocumentModel(file, projectIdentifier,
    _settingsService.GlobalSettings.Originator,
    _settingsService.GlobalSettings.Role,
    _settingsService.GlobalSettings.FileNameFilter);
    }
}
