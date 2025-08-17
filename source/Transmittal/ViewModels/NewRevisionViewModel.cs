using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;
using Transmittal.Models;
using Transmittal.Requesters;

namespace Transmittal.ViewModels;

internal partial class NewRevisionViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly IRevisionRequester _callingViewModel;

    [ObservableProperty]
    private DateTime _revisionDate = DateTime.Now;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private object _revisionSequenceID;

    [ObservableProperty]
    private object _revisionSequence;

    [ObservableProperty]
    private string _issuedBy = string.Empty;

    [ObservableProperty]
    private string _issuedTo = string.Empty;

#if REVIT2022_OR_GREATER
    public List<Element> RevisionSequences { get; private set; }
#else
    public List<object> RevisionSequences { get; private set; }
#endif
    public NewRevisionViewModel(IRevisionRequester callingViewModel, 
        ISettingsService settingsService)
    {
        _callingViewModel = callingViewModel;
        _settingsService = settingsService;

#if REVIT2022_OR_GREATER
        RevisionSequences = RevisionNumberingSequence
            .GetAllRevisionNumberingSequences(App.RevitDocument)
            .Select(App.RevitDocument.GetElement)
            .ToList();
#else
        RevisionSequences = new ();
        foreach (var i in Enum.GetValues(typeof(Autodesk.Revit.DB.RevisionNumberType)))
        {
            RevisionSequences.Add(i);
        }
#endif

        _revisionSequence = RevisionSequences.FirstOrDefault();
    }

    [RelayCommand]
    private void SendRevision()
    {
#if REVIT2022_OR_GREATER
        //create a new revision model & pupulate the values from the form
        RevisionDataModel revisionModel = new()
        {
            RevDate = RevisionDate.ToString(_settingsService.GlobalSettings.DateFormatString),
            Description = Description,
            IssuedBy = IssuedBy,
            IssuedTo = IssuedTo,
            SequenceId = (ElementId)RevisionSequenceID
        };
#else
        //create a new revision model & pupulate the values from the form
        RevisionDataModel revisionModel = new RevisionDataModel
        {
            RevDate = RevisionDate.ToString(_settingsService.GlobalSettings.DateFormatString),
            Description = Description,
            IssuedBy = IssuedBy,
            IssuedTo = IssuedTo,
            Numbering = (RevisionNumberType)RevisionSequence
        };
#endif

        _callingViewModel.RevisionComplete(revisionModel);
        this.OnClosingRequest();
    }
}