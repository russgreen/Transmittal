using Autodesk.Revit.DB;
using Krypton.Toolkit;
using System;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Models;

namespace Transmittal.Services;

internal class SettingsServiceRvt : ISettingsServiceRvt
{
    private const string _paramTransmittalDBName = "TransmittalDB";
    private const string _paramTransmittalDBGuid = "dafd0e0e-37a1-4028-966f-6a2060de079d";

    private const string _paramTransmittalDBTemplateName = "TransmittalDBTemplate";
    private const string _paramTransmittalDBTemplateGuid = "7d00fc2a-08e5-4973-8f08-8115ee93e1e2";

    private const string _paramTransmittalFilenameFilterName = "TransmittalFilenameFilter";
    private const string _paramTransmittalFilenameFilterGuid = "39a8f82d-9c51-404d-936f-76f8b56876c5";

    private const string _paramTransmittalSaveLocationName = "TransmittalSaveLocation";
    private const string _paramTransmittalSaveLocationGuid = "722d4894-9345-4ca8-9290-b6ccdccc6994";

    private const string _paramTransmittalUseExtranetName = "TransmittalUseExtranet";
    private const string _paramTransmittalUseExtranetGuid = "8a995d7e-0e5c-40d3-9f67-2252ff309b98";

    private const string _paramTransmittalUseISO19650Name = "TransmittalUseISO19650";
    private const string _paramTransmittalUseISO19650Guid = "27538622-6e36-44e5-88d5-9dcccfeb255f";

    private Autodesk.Revit.DB.ProjectInfo _projectInfo = null;

    private readonly ISettingsService _settingsService;
    private readonly IDataConnection _dataConnection;

    public SettingsServiceRvt(ISettingsService settingsService,
        IDataConnection dataConnection)
    {
        _settingsService = settingsService;
        _dataConnection = dataConnection;
    }

    public bool GetSettingsRvt(Document rvtDoc)
    {
        // first get the project information 
        _projectInfo = rvtDoc.ProjectInformation;
        _settingsService.GlobalSettings.ProjectNumber = _projectInfo.Number;
        _settingsService.GlobalSettings.ProjectName = _projectInfo.Name;

        // check revit for the TransmittalDB parameter
        _settingsService.GlobalSettings.DatabaseFile = Util.GetParameterValueString(_projectInfo, _paramTransmittalDBGuid);

        // if the value is not empty try and open the database and read the settings
        if (CheckDatabaseFileExists(_settingsService.GlobalSettings.DatabaseFile))
        {
            _settingsService.GetSettings();

            return true;
        }
        else
        {
            // check revit for the templateDB parameter
            _settingsService.GlobalSettings.DatabaseTemplateFile = Util.GetParameterValueString(_projectInfo, _paramTransmittalDBTemplateGuid);

            if (CheckDatabaseFileExists(_settingsService.GlobalSettings.DatabaseTemplateFile, false))
            {
                // seems there is a template database but we need the settings
                // dialog open to setup this project so return false
                return false;
            }             
        }

        // both db paramaters are not found check for TransmittalFilenameFilter, TransmittalSaveLocation, TransmittalUseISO19650, TransmittalUseExtranet
        // if all params exist we can continue with a limited feature set
        var paramFilenameFilter = Util.GetParameterValueString(_projectInfo, _paramTransmittalFilenameFilterGuid);
        if(paramFilenameFilter.Length > 0)
        {
            _settingsService.GlobalSettings.FileNameFilter = paramFilenameFilter;
        }

        var paramSaveLocation = Util.GetParameterValueString(_projectInfo, _paramTransmittalSaveLocationGuid);
        if(paramSaveLocation.Length > 0)
        {
            _settingsService.GlobalSettings.DrawingIssueStore = paramSaveLocation;
        }
        
        var paramUseISO19650 = Util.GetParameterValueInt(_projectInfo, _paramTransmittalUseISO19650Guid);
        _settingsService.GlobalSettings.UseISO19650 = Convert.ToBoolean(paramUseISO19650);  

        var paramUseExtranet = Util.GetParameterValueInt(_projectInfo, _paramTransmittalUseExtranetGuid);
        _settingsService.GlobalSettings.UseExtranet = Convert.ToBoolean(paramUseExtranet);

        // at this point a database file is not being used so try and get the additional settings from parameters 
        _settingsService.GlobalSettings.Originator = Util.GetParameterValueString(_projectInfo, _settingsService.GlobalSettings.OriginatorParamGuid);
        _settingsService.GlobalSettings.Role = Util.GetParameterValueString(_projectInfo, _settingsService.GlobalSettings.RoleParamGuid);
        _settingsService.GlobalSettings.ProjectIdentifier = Util.GetParameterValueString(_projectInfo, _settingsService.GlobalSettings.ProjectIdentifierParamGuid);

        return true;
    }

    public void UpdateSettingsRvt(Document rvtDoc)  
    {
       //throw new NotImplementedException();
    }

    private bool CheckDatabaseFileExists(string filepath, bool checkConnection = true)
    {
        if(checkConnection == true)
        {
            if (System.IO.File.Exists(filepath))
            {
                return _dataConnection.CheckConnection(filepath);
            }
        }

        return System.IO.File.Exists(filepath);
    }



}
    