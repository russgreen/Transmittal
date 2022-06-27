using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;
using Transmittal.Models;
using Transmittal.Requesters;

namespace Transmittal.ViewModels;

//[INotifyPropertyChanged]
internal partial class NewRevisionViewModel : BaseViewModel
{
    private readonly IRevisionRequester _callingViewModel;
    private readonly ISettingsService _settingsService = Ioc.Default.GetRequiredService<ISettingsService>();

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

    public List<Element> RevisionSequences { get; private set; }

    public NewRevisionViewModel(IRevisionRequester callingViewModel)
    {
        _callingViewModel = callingViewModel;

        RevisionSequences = RevisionNumberingSequence
            .GetAllRevisionNumberingSequences(App.RevitDocument)
            .Select(App.RevitDocument.GetElement)
            .ToList();

        _revisionSequence = RevisionSequences.FirstOrDefault();
    }

    [RelayCommand]
    private void SendRevision()
    {
#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
        //create a new revision model & pupulate the values from the form
        RevisionDataModel revisionModel = new RevisionDataModel
        {
            RevDate = _revisionDate.ToString(_settingsService.GlobalSettings.DateFormatString),
            Description = _description,
            IssuedBy = _issuedBy,
            IssuedTo = _issuedTo,
            Numbering = (RevisionNumberType)_revisionSequence
        };

#else
        //create a new revision model & pupulate the values from the form
        RevisionDataModel revisionModel = new RevisionDataModel
        {
            RevDate = RevisionDate.ToString(_settingsService.GlobalSettings.DateFormatString),
            Description = Description,
            IssuedBy = IssuedBy,
            IssuedTo = IssuedTo,
            SequenceId = (ElementId)RevisionSequenceID
        };
#endif


        _callingViewModel.RevisionComplete(revisionModel);
        this.OnClosingRequest();
    }
}