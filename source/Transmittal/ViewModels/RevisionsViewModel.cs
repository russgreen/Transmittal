using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;
using Transmittal.Models;
using Transmittal.Requesters;

namespace Transmittal.ViewModels;

internal partial class RevisionsViewModel : BaseViewModel, IRevisionRequester
{
    private readonly ISettingsService _settingsService = Host.GetService<ISettingsService>();
    private readonly IMessageBoxService _messageBox = Host.GetService<IMessageBoxService>();
    private readonly IRevisionRequester _callingViewModel;
    
    [ObservableProperty]
    private ObservableCollection<RevisionDataModel> _revisions;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRevisionSelected))]
    private RevisionDataModel _selectedRevision;

    public bool IsRevisionSelected => SelectedRevision != null;

    public RevisionsViewModel(IRevisionRequester callingViewModel)
    {
        _callingViewModel = callingViewModel;
        
        Revisions = new();
        Revisions.CollectionChanged += Revisions_CollectionChanged;

        LoadRevisions();
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
            _messageBox.ShowOk("Error creating revision", ex.Message);
            
            trans.RollBack();
        }

        LoadRevisions();

        SelectedRevision = Revisions.LastOrDefault();
    }

    [RelayCommand]
    private void SendRevision()
    {
        _callingViewModel.RevisionComplete(SelectedRevision);
        this.OnClosingRequest();
    }
}
