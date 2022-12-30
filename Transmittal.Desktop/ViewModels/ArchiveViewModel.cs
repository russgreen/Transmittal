using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using Transmittal.Library.Extensions;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;

namespace Transmittal.Desktop.ViewModels;

internal partial class ArchiveViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
    private readonly IContactDirectoryService _contactDirectoryService = Ioc.Default.GetRequiredService<IContactDirectoryService>();
    private readonly ITransmittalService _transmittalService = Ioc.Default.GetRequiredService<ITransmittalService>();

    public string WindowTitle { get; private set; }

    [ObservableProperty]
    private ObservableCollection<TransmittalModel> _transmittals;
    [ObservableProperty]
    private ObservableCollection<object> _selectedTransmittals = new();

    [ObservableProperty]
    private ObservableCollection<TransmittalItemModel> _transmittalItems;
    [ObservableProperty]
    private ObservableCollection<TransmittalDistributionModel> _transmittalDistribution;

    [ObservableProperty]
    private TransmittalItemModel _selectedTransmittalItem;
    [ObservableProperty]
    private TransmittalDistributionModel _selectedTransmittalDistribution;

    [ObservableProperty]
    private ObservableCollection<ProjectDirectoryModel> _projectDirectory;

    [ObservableProperty]
    private bool _hasDatabase = true;
    [ObservableProperty]
    private bool _itemSelected = false;
    [ObservableProperty]
    private bool _canMergeTransmittals = false;

    public ArchiveViewModel()
    {
        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion} ({_settingsService.GlobalSettings.DatabaseFile})";

        if (_settingsService.GlobalSettings.RecordTransmittals == false)
        {
            HasDatabase = false;
            return;
        }

        LoadData();
    }

    private void WireUpTransmittalPropertyChangedEvents()
    {
        foreach (var transmittal in Transmittals)
        {
            foreach (var item in transmittal.Items)
            {
                item.PropertyChanged += TransmittalItem_PropertyChanged;
            }

            foreach (var distribution in transmittal.Distribution)
            {
                distribution.PropertyChanged += Distribution_PropertyChanged;
            }   
        }
            
    }

    private void TransmittalItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if(SelectedTransmittalItem != null)
        {
            _transmittalService.UpdateTransmittalItem(SelectedTransmittalItem);
        }
    }

    private void Distribution_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if(SelectedTransmittalDistribution != null)
        {
            _transmittalService.UpdateTransmittalDist(SelectedTransmittalDistribution);
        }
    }
    
    private void TransmittalItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            TransmittalModel transmittalModel = SelectedTransmittals.First() as TransmittalModel;
            TransmittalItemModel itemModel = (TransmittalItemModel)e.NewItems[0];
            
            itemModel.TransID = transmittalModel.ID;
            itemModel.PropertyChanged += TransmittalItem_PropertyChanged;

            _transmittalService.CreateTransmittalItem(itemModel);
            transmittalModel.Items.Add(itemModel);
        }
    }      
            
    private void TransmittalDistribution_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            TransmittalModel transmittalModel = SelectedTransmittals.First() as TransmittalModel;
            TransmittalDistributionModel distributionModel = (TransmittalDistributionModel)e.NewItems[0];

            distributionModel.TransID = transmittalModel.ID;
            distributionModel.PropertyChanged += Distribution_PropertyChanged;

            _transmittalService.CreateTransmittalDist(distributionModel);
            transmittalModel.Distribution.Add(distributionModel);
        }
    }

    private void SelectedTransmittals_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        CanMergeTransmittals = false;
        ItemSelected = (SelectedTransmittals.Count == 1);

        if (SelectedTransmittals.Count == 0 & TransmittalItems != null)
        {
            TransmittalItems.Clear();
            TransmittalDistribution.Clear();
        }
        
        if (SelectedTransmittals.Count == 1)
        {
            TransmittalModel transmittalModel = SelectedTransmittals.First() as TransmittalModel;
            TransmittalItems = new ObservableCollection<TransmittalItemModel>(transmittalModel.Items);
            TransmittalDistribution = new ObservableCollection<TransmittalDistributionModel>(transmittalModel.Distribution);

            TransmittalItems.CollectionChanged += TransmittalItems_CollectionChanged;
            TransmittalDistribution.CollectionChanged += TransmittalDistribution_CollectionChanged;
        }

        if (SelectedTransmittals.Count > 1)
        {
            CanMergeTransmittals = false;

            TransmittalItems.Clear();
            TransmittalDistribution.Clear();

            List<TransmittalModel> transmittals = SelectedTransmittals.Cast<TransmittalModel>().ToList();
            if (transmittals
                .TrueForAll(i => i.TransDate.Date == transmittals
                .FirstOrDefault().TransDate.Date))
            {
                CanMergeTransmittals = true;
            }

        }
    }

    [RelayCommand]
    private void LoadData()
    {
        ProjectDirectory = new ObservableCollection<ProjectDirectoryModel>(_contactDirectoryService.GetProjectDirectory());
        Transmittals = new ObservableCollection<TransmittalModel>(_transmittalService.GetTransmittals());

        WireUpTransmittalPropertyChangedEvents();

        SelectedTransmittals.CollectionChanged += SelectedTransmittals_CollectionChanged;
    }

    [RelayCommand]
    private void MergeTransmittals()
    {
        List<TransmittalModel> transmittalsToMerge = SelectedTransmittals.Cast<TransmittalModel>().ToList();
        //remove the selected items from the list
        foreach (TransmittalModel item in transmittalsToMerge)
        {
            Transmittals.Remove(item);
        }

        Transmittals.Add(_transmittalService.MergeTransmittals(transmittalsToMerge));

        CanMergeTransmittals = false;
    }

    [RelayCommand]
    private void DuplicateTransmittal()
    {
        TransmittalModel transmittalModel = SelectedTransmittals.First() as TransmittalModel;
        TransmittalModel newTransmittal = new();

        _transmittalService.CreateTransmittal(newTransmittal);

        foreach (TransmittalItemModel item in transmittalModel.Items)
        {
            TransmittalItemModel newItem = new();

            item.CopyPropertiesTo(newItem); 
            newItem.TransID = newTransmittal.ID;

            _transmittalService.CreateTransmittalItem(newItem);

            newTransmittal.Items.Add(newItem);
        }

        foreach (TransmittalDistributionModel dist in transmittalModel.Distribution)
        {
            TransmittalDistributionModel newDist = new();

            dist.CopyPropertiesTo(newDist);
            newDist.TransID = newTransmittal.ID;

            _transmittalService.CreateTransmittalDist(newDist);

            newTransmittal.Distribution.Add(newDist);
        }

        Transmittals.Add(newTransmittal);
        WireUpTransmittalPropertyChangedEvents(); 
    }

    [RelayCommand]
    private void DeleteTransmittal()
    {
        TransmittalModel transmittalModel = SelectedTransmittals.First() as TransmittalModel;

        if (transmittalModel.Items.Count == 0 &&
                transmittalModel.Distribution.Count == 0)
        {
            _transmittalService.DeleteTransmittal(transmittalModel);
            Transmittals.Remove(transmittalModel);
        }        
    }

    [RelayCommand]
    private void ShowSummaryReport()
    {
        Reports.Reports reports = new(_settingsService, _contactDirectoryService, _transmittalService);
        reports.ShowTransmittalSummaryReport();
    }

    [RelayCommand]
    private void ShowTransmittalReport()
    {
        TransmittalModel transmittalModel = SelectedTransmittals.First() as TransmittalModel;
        Reports.Reports reports = new(_settingsService, _contactDirectoryService, _transmittalService);
        reports.ShowTransmittalReport(transmittalModel.ID);
    }

    [RelayCommand]
    private void DeleteSelectedTransmittalItem()
    {
        if (SelectedTransmittalItem != null)
        {
            _transmittalService.DeleteTransmittalItem(SelectedTransmittalItem);

            var transmittal = SelectedTransmittals.First() as TransmittalModel;
            transmittal.Items.Remove(SelectedTransmittalItem);
        }
    }

    [RelayCommand]
    private void DeleteSelectedDistribution()
    {
        if (SelectedTransmittalDistribution != null)
        {
            _transmittalService.DeleteTransmittalDist(SelectedTransmittalDistribution);

            var transmittal = SelectedTransmittals.First() as TransmittalModel;
            transmittal.Distribution.Remove(SelectedTransmittalDistribution);
        }
    }

}
