using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        foreach (var transmittal in _transmittals)
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
        if(_selectedTransmittalItem != null)
        {
            _transmittalService.UpdateTransmittalItem(_selectedTransmittalItem);
        }
    }

    private void Distribution_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if(_selectedTransmittalDistribution != null)
        {
            _transmittalService.UpdateTransmittalDist(_selectedTransmittalDistribution);
        }
    }
    
    private void TransmittalItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            TransmittalModel transmittalModel = _selectedTransmittals.First() as TransmittalModel;
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
            TransmittalModel transmittalModel = _selectedTransmittals.First() as TransmittalModel;
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
        ItemSelected = (_selectedTransmittals.Count == 1);

        if (_selectedTransmittals.Count == 0 & TransmittalItems != null)
        {
            TransmittalItems.Clear();
            TransmittalDistribution.Clear();
        }
        
        if (_selectedTransmittals.Count == 1)
        {
            TransmittalModel transmittalModel = _selectedTransmittals.First() as TransmittalModel;
            TransmittalItems = new ObservableCollection<TransmittalItemModel>(transmittalModel.Items);
            TransmittalDistribution = new ObservableCollection<TransmittalDistributionModel>(transmittalModel.Distribution);

            TransmittalItems.CollectionChanged += TransmittalItems_CollectionChanged;
            TransmittalDistribution.CollectionChanged += TransmittalDistribution_CollectionChanged;
        }

        if (_selectedTransmittals.Count > 1)
        {
            CanMergeTransmittals = false;

            TransmittalItems.Clear();
            TransmittalDistribution.Clear();

            List<TransmittalModel> transmittals = _selectedTransmittals.Cast<TransmittalModel>().ToList();
            if (transmittals
                .TrueForAll(i => i.TransDate.Date == transmittals
                .FirstOrDefault().TransDate.Date))
            {
                CanMergeTransmittals = true;
            }

        }
    }

    [ICommand]
    private void LoadData()
    {
        ProjectDirectory = new ObservableCollection<ProjectDirectoryModel>(_contactDirectoryService.GetProjectDirectory());
        Transmittals = new ObservableCollection<TransmittalModel>(_transmittalService.GetTransmittals());

        WireUpTransmittalPropertyChangedEvents();

        SelectedTransmittals.CollectionChanged += SelectedTransmittals_CollectionChanged;
    }

    [ICommand]
    private void MergeTransmittals()
    {
        List<TransmittalModel> transmittalsToMerge = _selectedTransmittals.Cast<TransmittalModel>().ToList();
        //remove the selected items from the list
        foreach (TransmittalModel item in transmittalsToMerge)
        {
            _transmittals.Remove(item);
        }

        _transmittals.Add(_transmittalService.MergeTransmittals(transmittalsToMerge));

        CanMergeTransmittals = false;
    }

    [ICommand]
    private void ShowSummaryReport()
    {
        Reports.Reports reports = new(_settingsService, _contactDirectoryService, _transmittalService);
        reports.ShowTransmittalSummaryReport();
    }

    [ICommand]
    private void ShowTransmittalReport()
    {
        TransmittalModel transmittalModel = _selectedTransmittals.First() as TransmittalModel;
        Reports.Reports reports = new(_settingsService, _contactDirectoryService, _transmittalService);
        reports.ShowTransmittalReport(transmittalModel.ID);
    }
}
