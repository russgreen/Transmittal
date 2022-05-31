﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Windows.Controls;
using System.Xml.Linq;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Models;

namespace Transmittal.Services;

internal class SettingsServiceRvt : ISettingsServiceRvt
{
    private const string _schemaName = "TransmittalSettings";
    private const string _schemaGuid = "E9E8F8E9-F8E9-4F8E-8E9F-E9E8F8E9F8E9";
    private const string _vendorID = "Transmittal";

    private const string _paramTransmittalDBTemplateName = "TransmittalDBTemplate";
    private const string _paramTransmittalDBTemplateGuid = "7d00fc2a-08e5-4973-8f08-8115ee93e1e2";


    private Autodesk.Revit.DB.ProjectInfo _projectInfo = null;

    private readonly ISettingsService _settingsService;
    private readonly IDataConnection _dataConnection;

    private Schema _schema = null;

    public SettingsServiceRvt(IDataConnection dataConnection, ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _dataConnection = dataConnection;

        _schema = null;
    }
    
    public bool GetSettingsRvt(Document rvtDoc)
    {
        //reset the global settings to handle switching between models
        _settingsService.GlobalSettings = new();

        //set the default parameter values
        SetParameters();

        // first get the project information 
        _projectInfo = rvtDoc.ProjectInformation;
        _settingsService.GlobalSettings.ProjectNumber = _projectInfo.Number;
        _settingsService.GlobalSettings.ProjectName = _projectInfo.Name;

        // check for the schema and load data from it if it exists
        _schema = GetSchema();
        if (_schema != null)
        {
            // get the settings from the schema
            GetSettingsFromSchema();
        }
        else
        {           
            // create the schema and load the settings from it
            CreateSchema();
            SaveSettingsToSchema(); //saves the default settings only at this point
            GetSettingsFromSchema(); //don't really need this call but its here to help debugging
        }

        // if the value is not empty try and open the database and read the settings
        if(_settingsService.GlobalSettings.RecordTransmittals == true)
        {
            if (CheckDatabaseFileExists(_settingsService.GlobalSettings.DatabaseFile))
            {
                _settingsService.GetSettings();

                return true;
            }
            else
            {
                // check revit for the templateDB parameter as we may be using a custom template database set in a revit template
                var templateDB = Util.GetParameterValueString(_projectInfo, _paramTransmittalDBTemplateGuid);

                if (CheckDatabaseFileExists(templateDB, false))
                {
                    // seems there is a template database but we need the settings
                    // dialog open to setup this project so return false
                    _settingsService.GlobalSettings.DatabaseTemplateFile = templateDB;

                    return false;
                }             
            }            
        }

        return true;
    }

    public void UpdateSettingsRvt()  
    {
        SaveSettingsToSchema();
    }

    public void SetParameters()
    {
        // project paramaters
        _settingsService.GlobalSettings.ProjectIdentifierParamName = "ProjectIdentifier";
        _settingsService.GlobalSettings.ProjectIdentifierParamGuid = "ce8c18ee-3b90-4f42-8938-ae90e3af5a6a";
        _settingsService.GlobalSettings.OriginatorParamName = "Originator";
        _settingsService.GlobalSettings.OriginatorParamGuid = "e45313b7-8419-4803-92f0-68558f9278b2";
        _settingsService.GlobalSettings.RoleParamName = "Role";
        _settingsService.GlobalSettings.RoleParamGuid = "67fcb5e8-4ffb-43b8-8ec9-c664fd997267";
        // sheet parameters
        _settingsService.GlobalSettings.SheetVolumeParamName = "SheetVolume";
        _settingsService.GlobalSettings.SheetVolumeParamGuid = "9c16757c-175a-451a-a5d4-c4a6ff291acb";
        _settingsService.GlobalSettings.SheetLevelParamName = "SheetLevel";
        _settingsService.GlobalSettings.SheetLevelParamGuid = "e51af162-9025-48a0-bd2c-bc833fab0db0";
        _settingsService.GlobalSettings.DocumentTypeParamName = "DocumentType";
        _settingsService.GlobalSettings.DocumentTypeParamGuid = "eb57d296-7d9c-459f-ace1-0bdaf95c3b29";
        _settingsService.GlobalSettings.SheetStatusParamName = "SheetStatus";
        _settingsService.GlobalSettings.SheetStatusParamGuid = "3304f169-ceb9-40b9-a69d-d8f3eb0a3fb9";
        _settingsService.GlobalSettings.SheetStatusDescriptionParamName = "SheetStatusDescription";
        _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid = "4effad6a-f05d-43dd-afb1-c2b6c5cb5b9a";
    }
    
    public bool CheckDatabaseFileExists(string filepath, bool checkConnection = true)
    {
        if(filepath != "[NONE]" || filepath != null)
        {
            if (System.IO.File.Exists(filepath))
            {
                if(checkConnection == true)
                {
                    return _dataConnection.CheckConnection(filepath);
                }                
            }

            return System.IO.File.Exists(filepath); 
        }

        return false;
    }
    
    private List<IssueFormatModel> GetIssueFormats()
    {
        //build the issue formats list  
        var issueFormats = new List<IssueFormatModel>
        {
            new IssueFormatModel() { Code = "E", Description = "Email" },
            new IssueFormatModel() { Code = "C", Description = "Cloud" },
            new IssueFormatModel() { Code = "P", Description = "Paper" }
        };

        return issueFormats;
    }
        
    private void CreateSchema()
    {
        using (Transaction createSchema = new Transaction(App.RevitDocument, "TransmittalSettings"))
        {
            createSchema.Start();
            
            //build the schema
            SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid(_schemaGuid));
            schemaBuilder.SetReadAccessLevel(AccessLevel.Public); // allow anyone to read the object
            schemaBuilder.SetWriteAccessLevel(AccessLevel.Public); // TODO why does it not work when we restrict writing to this vendor only
            schemaBuilder.SetVendorId(_vendorID); // required because of restricted write-access
            schemaBuilder.SetSchemaName(_schemaName);
            
            FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.FileNameFilter), typeof(string));   
            fieldBuilder.SetDocumentation("The filename filter rule for transmittal exports");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.DrawingIssueStore), typeof(string));
            fieldBuilder.SetDocumentation("The location to save the transmittal exports");   

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.UseISO19650), typeof(bool));
            fieldBuilder.SetDocumentation("Use the ISO19650 for transmittal exports");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.UseExtranet), typeof(bool));
            fieldBuilder.SetDocumentation("Use the extranet for transmittal exports");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.DateFormatString), typeof(string));
            fieldBuilder.SetDocumentation("The date format string for revisions exports");
            

            fieldBuilder = schemaBuilder.AddMapField(nameof(_settingsService.GlobalSettings.IssueFormats), typeof(string), typeof(string));
            fieldBuilder.SetDocumentation("The issue formats for transmittal exports");

            fieldBuilder = schemaBuilder.AddMapField(nameof(_settingsService.GlobalSettings.DocumentStatuses), typeof(string), typeof(string));
            fieldBuilder.SetDocumentation("The document statuses for transmittal exports");
            

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.RecordTransmittals), typeof(bool));
            fieldBuilder.SetDocumentation("Record transmittals in the database");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.DatabaseTemplateFile), typeof(string));
            fieldBuilder.SetDocumentation("The location of the template database");

            fieldBuilder = schemaBuilder.AddSimpleField(  nameof(_settingsService.GlobalSettings.DatabaseFile), typeof(string));
            fieldBuilder.SetDocumentation("The location of the database");
            

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.IssueSheetStore), typeof(string));
            fieldBuilder.SetDocumentation("The location to save the transmittal reports");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.DirectoryStore), typeof(string));
            fieldBuilder.SetDocumentation("The location to save the directory reports");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.ReportStore), typeof(string));
            fieldBuilder.SetDocumentation("The location of customised report templates");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.UseCustomSharedParameters), typeof(bool));
            fieldBuilder.SetDocumentation("Use custom shared parameters");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.ProjectIdentifierParamGuid), typeof(string));
            fieldBuilder.SetDocumentation("The project identifier parameter guid");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.OriginatorParamGuid), typeof(string));
            fieldBuilder.SetDocumentation("The originator parameter guid");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.RoleParamGuid), typeof(string));
            fieldBuilder.SetDocumentation("The role parameter guid");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.SheetVolumeParamGuid), typeof(string));
            fieldBuilder.SetDocumentation("The sheet volume parameter guid");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.SheetLevelParamGuid), typeof(string));
            fieldBuilder.SetDocumentation("The sheet level parameter guid");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.DocumentTypeParamGuid), typeof(string));
            fieldBuilder.SetDocumentation("The document type parameter guid");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.SheetStatusParamGuid), typeof(string));
            fieldBuilder.SetDocumentation("The sheet status parameter guid");

            fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.SheetStatusDescriptionParamGuid), typeof(string));
            fieldBuilder.SetDocumentation("The sheet status description parameter guid");

            _schema = schemaBuilder.Finish(); // register the Schema object
            
            Entity entity = new Entity(_schema); // create an entity (object) for this schema (class)
                                                
            App.RevitDocument.ProjectInformation.SetEntity(entity); // store the entity in the element

            createSchema.Commit();
        }
    }
    
    private void SaveSettingsToSchema()
    {
        using (Transaction storeData = new Transaction(App.RevitDocument, "TransmittalSettings"))
        {
            storeData.Start();

            _schema = GetSchema();
            
            Entity entity = App.RevitDocument.ProjectInformation.GetEntity(_schema);

            entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.FileNameFilter)), _settingsService.GlobalSettings.FileNameFilter);
            entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.DrawingIssueStore)), _settingsService.GlobalSettings.DrawingIssueStore);
            entity.Set<bool>(_schema.GetField(nameof(_settingsService.GlobalSettings.UseISO19650)), _settingsService.GlobalSettings.UseISO19650);
            entity.Set<bool>(_schema.GetField(nameof(_settingsService.GlobalSettings.UseExtranet)), _settingsService.GlobalSettings.UseExtranet);
            entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.DateFormatString)), _settingsService.GlobalSettings.DateFormatString);

            entity.Set<IDictionary<string, string>>(_schema.GetField(nameof(_settingsService.GlobalSettings.IssueFormats)), 
                ListOfIssueFormatToDictionary(_settingsService.GlobalSettings.IssueFormats));
            entity.Set<IDictionary<string, string>>(_schema.GetField(nameof(_settingsService.GlobalSettings.DocumentStatuses)),
                ListOfDocumentStatusToDictionary(_settingsService.GlobalSettings.DocumentStatuses));

            entity.Set<bool>(_schema.GetField(nameof(_settingsService.GlobalSettings.RecordTransmittals)), _settingsService.GlobalSettings.RecordTransmittals);
            //entity.Set<string>(schema.GetField(nameof(_settingsService.GlobalSettings.DatabaseTemplateFile)), _settingsService.GlobalSettings.DatabaseTemplateFile);
            entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.DatabaseFile)), _settingsService.GlobalSettings.DatabaseFile);

            //entity.Set<string>(schema.GetField(nameof(_settingsService.GlobalSettings.IssueSheetStore)), _settingsService.GlobalSettings.IssueSheetStore);
            //entity.Set<string>(schema.GetField(nameof(_settingsService.GlobalSettings.DirectoryStore)), _settingsService.GlobalSettings.DirectoryStore);
            //entity.Set<string>(schema.GetField(nameof(_settingsService.GlobalSettings.ReportStore)), _settingsService.GlobalSettings.ReportStore);

            entity.Set<bool>(_schema.GetField(nameof(_settingsService.GlobalSettings.UseCustomSharedParameters)), _settingsService.GlobalSettings.UseCustomSharedParameters);

            entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.ProjectIdentifierParamGuid)), _settingsService.GlobalSettings.ProjectIdentifierParamGuid);
            entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.OriginatorParamGuid)), _settingsService.GlobalSettings.OriginatorParamGuid);
            entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.RoleParamGuid)), _settingsService.GlobalSettings.RoleParamGuid);
            entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.SheetVolumeParamGuid)), _settingsService.GlobalSettings.SheetVolumeParamGuid);
            entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.SheetLevelParamGuid)), _settingsService.GlobalSettings.SheetLevelParamGuid);
            entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.DocumentTypeParamGuid)), _settingsService.GlobalSettings.DocumentTypeParamGuid);
            entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.SheetStatusParamGuid)), _settingsService.GlobalSettings.SheetStatusParamGuid);
            entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.SheetStatusDescriptionParamGuid)), _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid);

            App.RevitDocument.ProjectInformation.SetEntity(entity);

            storeData.Commit();
        }

    }
    
    private void GetSettingsFromSchema()
    {
        Schema schema = GetSchema();

        Entity entity = App.RevitDocument.ProjectInformation.GetEntity(schema);

        _settingsService.GlobalSettings.FileNameFilter = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.FileNameFilter)));
        _settingsService.GlobalSettings.DrawingIssueStore = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.DrawingIssueStore)));

        _settingsService.GlobalSettings.UseISO19650 = entity.Get<bool>(
            schema.GetField(nameof(_settingsService.GlobalSettings.UseISO19650)));
        _settingsService.GlobalSettings.UseExtranet = entity.Get<bool>(
            schema.GetField(nameof(_settingsService.GlobalSettings.UseExtranet)));

        _settingsService.GlobalSettings.DateFormatString = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.DateFormatString)));


        _settingsService.GlobalSettings.IssueFormats = DictionaryToListOfIssueFormat(
            entity.Get<IDictionary<string, string>>(schema.GetField(nameof(_settingsService.GlobalSettings.IssueFormats))));
        _settingsService.GlobalSettings.DocumentStatuses = DictionaryToListOfDocumentStatus(
    entity.Get<IDictionary<string, string>>(schema.GetField(nameof(_settingsService.GlobalSettings.DocumentStatuses))));

        _settingsService.GlobalSettings.RecordTransmittals = entity.Get<bool>(
            schema.GetField(nameof(_settingsService.GlobalSettings.RecordTransmittals)));        
        _settingsService.GlobalSettings.DatabaseFile = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.DatabaseFile)));
        //_settingsService.GlobalSettings.DatabaseTemplateFile = entity.Get<string>(schema.GetField(
        //    nameof(_settingsService.GlobalSettings.DatabaseTemplateFile)));

        //_settingsService.GlobalSettings.IssueSheetStore = entity.Get<string>(
        //    schema.GetField(nameof(_settingsService.GlobalSettings.IssueSheetStore)));
        //_settingsService.GlobalSettings.DirectoryStore = entity.Get<string>(
        //    schema.GetField(nameof(_settingsService.GlobalSettings.DirectoryStore)));
        //_settingsService.GlobalSettings.ReportStore = entity.Get<string>(
        //    schema.GetField(nameof(_settingsService.GlobalSettings.ReportStore)));

        _settingsService.GlobalSettings.UseCustomSharedParameters = entity.Get<bool>(
            schema.GetField(nameof(_settingsService.GlobalSettings.UseCustomSharedParameters)));

        _settingsService.GlobalSettings.ProjectIdentifierParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.ProjectIdentifierParamGuid)));
        _settingsService.GlobalSettings.OriginatorParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.OriginatorParamGuid)));
        _settingsService.GlobalSettings.RoleParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.RoleParamGuid)));
        _settingsService.GlobalSettings.SheetVolumeParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.SheetVolumeParamGuid)));
        _settingsService.GlobalSettings.SheetLevelParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.SheetLevelParamGuid)));
        _settingsService.GlobalSettings.DocumentTypeParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.DocumentTypeParamGuid)));
        _settingsService.GlobalSettings.SheetStatusParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.SheetStatusParamGuid)));
        _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.SheetStatusDescriptionParamGuid)));
    }

    private Schema GetSchema()
    {
        //Schema schema = null;
        //IList<Schema> schemas = Schema.ListSchemas();
        //if (schemas != null && schemas.Count > 0)
        //{
        //    // get schema
        //    foreach (Schema s in schemas)
        //    {
        //        if (s.SchemaName == _schemaName)
        //        {
        //            schema = s;
        //            break;
        //        }
        //    }
        //}
        //return schema;

        Schema s = Schema.ListSchemas().FirstOrDefault(q => q.SchemaName == _schemaName);
        //if (s == null)
        //{
        //    // no schema found, create one
        //}
        //else
        //{
        //    // schema found, use it
        //}
        return s;        
    }

    private bool SchemaExists()
    {
        bool result = false;
        if (GetSchema() != null)
        {
            result = true;
        }
        return result;
    }

    private IDictionary<string, string> ListOfIssueFormatToDictionary(List<IssueFormatModel> listToConvert)
    {
        IDictionary<string, string> dictionary = new Dictionary<string, string>();

        foreach (IssueFormatModel item in listToConvert)
        {
            dictionary.Add(item.Code, item.Description);
        }

        return dictionary;
    }

    private IDictionary<string, string> ListOfDocumentStatusToDictionary(List<DocumentStatusModel> listToConvert)
    {
        IDictionary<string, string> dictionary = new Dictionary<string, string>();

        foreach (DocumentStatusModel item in listToConvert)
        {
            dictionary.Add(item.Code, item.Description);
        }

        return dictionary;
    }    

    private List<IssueFormatModel> DictionaryToListOfIssueFormat(IDictionary<string, string> dictionaryToConvert)
    {
        List<IssueFormatModel> list = new();

        foreach (KeyValuePair<string, string> item in dictionaryToConvert)
        {
            list.Add(new IssueFormatModel(item.Key, item.Value));
        }

        return list;
    }

    private List<DocumentStatusModel> DictionaryToListOfDocumentStatus(IDictionary<string, string> dictionaryToConvert)
    {
        List<DocumentStatusModel> list = new();

        foreach (KeyValuePair<string, string> item in dictionaryToConvert)
        {
            list.Add(new DocumentStatusModel(item.Key, item.Value));
        }

        return list;
    }    
}
    