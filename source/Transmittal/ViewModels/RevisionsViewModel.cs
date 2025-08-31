using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nice3point.Revit.Extensions;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;
using Transmittal.Models;
using Transmittal.Requesters;

namespace Transmittal.ViewModels;

internal partial class RevisionsViewModel : BaseViewModel, IRevisionRequester
{
    private readonly ISettingsService _settingsService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly IRevisionRequester _callingViewModel;
    
    [ObservableProperty]
    private ObservableCollection<RevisionDataModel> _revisions;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRevisionSelected))]
    private RevisionDataModel _selectedRevision;

    public bool IsRevisionSelected => SelectedRevision != null;

    public RevisionsViewModel(IRevisionRequester callingViewModel, 
        ISettingsService settingsService,
        IMessageBoxService messageBoxService)
    {
        _callingViewModel = callingViewModel;
        _settingsService = settingsService;
        _messageBoxService = messageBoxService;

        Revisions = new();
        Revisions.CollectionChanged += Revisions_CollectionChanged;

        LoadRevisions();
    }

    public void RevisionComplete(RevisionDataModel model)
    {
        //save the new revision into the model
        Transaction trans = null;
        try
        {
            trans = new Transaction(App.RevitDocument, "Create Revision");

            trans.Start();
            var newRevision = Revision.Create(App.RevitDocument);
            newRevision.Description = model.Description;
            newRevision.IssuedBy = model.IssuedBy;
            newRevision.IssuedTo = model.IssuedTo;
            newRevision.RevisionDate = model.RevDate;

#if REVIT2022_OR_GREATER
            newRevision.RevisionNumberingSequenceId = model.SequenceId;
#else
            //no sequence ID until 2022
            newRevision.NumberType = model.Numbering;
#endif

            trans.Commit();
        }
        catch (Exception ex)
        {
            _messageBoxService.ShowOk("Error creating revision", ex.Message);
            
            trans.RollBack();
        }

        LoadRevisions();

        SelectedRevision = Revisions.LastOrDefault();
    }

    public bool CanEditRevisions()
    {
        if (!App.RevitDocument.IsWorkshared)
        {
            return true;
        }

        var revisionSettings = RevisionSettings.GetRevisionSettings(App.RevitDocument);
        
        var status = WorksharingUtils.GetCheckoutStatus(App.RevitDocument, revisionSettings.Id);

        switch (status)
        {
            case CheckoutStatus.OwnedByOtherUser:
                _messageBoxService.ShowOk("Cannot create revisions", "Revisions settings are checked out by another user.");
                return false;

                default:
                break;
        }
             
        return true;
    }

    private void LoadRevisions()
    {
        Revisions.Clear();
        
        var ids = Revision.GetAllRevisionIds(App.RevitDocument);
        int n = ids.Count;
        var revision_data = new List<RevisionDataModel>(n);
        foreach (ElementId id in ids)
        {
            Revision r = (Revision)App.RevitDocument.GetElement(id);
            Revisions.Add(new RevisionDataModel(r));
        }
    }

    private void Revisions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        //throw new NotImplementedException();
    }

    [RelayCommand]
    private void SendRevision()
    {
        _callingViewModel.RevisionComplete(SelectedRevision);
        this.OnClosingRequest();
    }
}
