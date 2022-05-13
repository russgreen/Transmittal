﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transmittal.Extensions;
using Transmittal.Models;
using Transmittal.Library.Services;

namespace Transmittal.Services;
internal class ExportDWGService : IExportDWGService
{
    private readonly ISettingsService _settingsService;

    public ExportDWGService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }
    
    public void ExportDWG(string exportFileName, DWGExportOptions dwgExportOptions, ViewSet views, Document exportDocument)
    {
        Transaction trans = null;
        try
        {
            trans = new Transaction(exportDocument, "Export DWG");
            var failOpt = trans.GetFailureHandlingOptions();
            
            failOpt.SetFailuresPreprocessor(new WarningSwallower());
            trans.SetFailureHandlingOptions(failOpt);
            trans.Start();

            IList<ElementId> lviews = new List<ElementId>();
            foreach (Autodesk.Revit.DB.View View in views)
            {
                lviews.Add(View.Id);
            }

            string folderPath = _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(Enums.ExportFormatType.DWG.ToString());
            
            if (Directory.Exists(folderPath) == false)
            {
                Directory.CreateDirectory(folderPath);
            }

            // export the sheet
            exportDocument.Export(folderPath, exportFileName, lviews, dwgExportOptions);

            if (dwgExportOptions.SharedCoords == true)
            {
                // export each view as dwg to support shared coordinates
                lviews = new List<ElementId>();
                ViewSheet vs = null;
                foreach (ViewSheet sheet in views)
                {
                    vs = sheet;
                }

                // Autodesk.Revit.DB.ViewSheet.Views' is obsolete: 'This property is obsolete in Revit 2015.  Use GetAllPlacedViews() instead.'
                // For Each v As Autodesk.Revit.DB.View In vs.Views
                var usedViews = new ViewSet();
                foreach (ElementId id in vs.GetAllPlacedViews())
                {
                    Autodesk.Revit.DB.View usedView = exportDocument.GetElement(id) as Autodesk.Revit.DB.View;
                    usedViews.Insert(usedView);
                }

                foreach (Autodesk.Revit.DB.View v in usedViews)
                {
                    lviews.Add(v.Id);
                    // export the view
#if REVIT2018
                        string ViewFileName = exportFileName.Replace( ".dwg", "-view_" + v.ViewName + ".dwg");
                        exportDocument.Export(folderPath, ViewFileName, lviews, dwgExportOptions);
#else
                    string ViewFileName = exportFileName.Replace(".dwg", "-view_" + v.Name + ".dwg");
                    exportDocument.Export(folderPath, ViewFileName, lviews, dwgExportOptions);
#endif
                }
            }
        }
        catch (Exception ex)
        {
            //TODO - report crashes
        }
        finally
        {
            trans.RollBack();
        }
    }

    public List<DWGLayerMappingModel> GetDWGLayerMappings()
    {
        return new()
        {
            new DWGLayerMappingModel() { Id = 0, Name = "BS1192" },
            new DWGLayerMappingModel() { Id = 1, Name = "AIA" },
            new DWGLayerMappingModel() { Id = 2, Name = "CP83" },
            new DWGLayerMappingModel() { Id = 3, Name = "ISO13567" }
        };
    }
}
