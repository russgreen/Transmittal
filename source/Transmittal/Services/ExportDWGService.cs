using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transmittal.Extensions;
using Transmittal.Models;
using Transmittal.Library.Services;
using Transmittal.Library.Extensions;
using Microsoft.Extensions.Logging;
using System.Windows.Media.Animation;

namespace Transmittal.Services;
internal class ExportDWGService : IExportDWGService
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<ExportDWGService> _logger;

    public ExportDWGService(ISettingsService settingsService,
        ILogger<ExportDWGService> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }
    
    public string ExportDWG(string exportFileName, string folderPath, DWGExportOptions dwgExportOptions, ViewSet views, Document exportDocument)
    {
        var fullPath = string.Empty;

        using (Transaction trans = new Transaction(exportDocument, "Export DWG"))
        {
            try
            {
                var failOpt = trans.GetFailureHandlingOptions();
                
                failOpt.SetFailuresPreprocessor(new WarningSwallower());
                trans.SetFailureHandlingOptions(failOpt);
                trans.Start();

                IList<ElementId> lviews = new List<ElementId>();
                foreach (Autodesk.Revit.DB.View View in views)
                {
                    lviews.Add(View.Id);
                }

                fullPath = Path.Combine(folderPath, exportFileName);

                if (Directory.Exists(folderPath) == false)
                {
                    Directory.CreateDirectory(folderPath);
                }

                if (File.Exists(fullPath) == true)
                {
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting existing DWG");
                        exportFileName.Replace(".dwg", $"({DateTime.Now.ToLongTimeString().Replace(":", "")}).dwg");
                        fullPath = Path.Combine(folderPath, exportFileName);
                    }
                }

                // export the sheet
                exportDocument.Export(folderPath, exportFileName, lviews, dwgExportOptions);

                //delete the PCP file
                var pcpFile = Path.Combine(folderPath, exportFileName.ToLower().Replace(".dwg" , ".pcp"));
                if (File.Exists(pcpFile))
                {
                    File.Delete(pcpFile);
                }

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

                        // only export floor plans and ceiling plans in shared coordinates to avoid exporting
                        // views that are not georeferenced and therefore not in the correct location in AutoCAD
                        if (usedView.ViewType == ViewType.FloorPlan ||
                            usedView.ViewType == ViewType.CeilingPlan )
                        {
                            usedViews.Insert(usedView);
                        }
                    }

                    foreach (Autodesk.Revit.DB.View v in usedViews)
                    {
                        lviews.Add(v.Id);
                        // export the view
#if REVIT2018
                        string viewFileName = exportFileName.Replace( ".dwg", "-view_" + v.ViewName + ".dwg");
                        exportDocument.Export(folderPath, viewFileName, lviews, dwgExportOptions);
#else
                        string viewFileName = exportFileName.Replace(".dwg", "-view_" + v.Name + ".dwg");
                        exportDocument.Export(folderPath, viewFileName, lviews, dwgExportOptions);
#endif

                        pcpFile = Path.Combine(folderPath, viewFileName.ToLower().Replace(".dwg", ".pcp"));
                        if (File.Exists(pcpFile))
                        {
                            File.Delete(pcpFile);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error exporting DWG");
            }
            finally
            {
                trans.RollBack();
            }
        }

        return fullPath;
    }

    public DWGExportOptions GetDocumentDWGExportOptions(Document exportDocument)
    {
        if (exportDocument == null)
        {
            throw new ArgumentNullException(nameof(exportDocument));
        }

        var activeSettings = ExportDWGSettings.GetActivePredefinedSettings(exportDocument);
        var exportOptions = activeSettings?.GetDWGExportOptions();

        exportOptions.MergedViews = true; //force this to merge the views by default

        return exportOptions ?? new DWGExportOptions();
    }

    public List<string> GetDocumentDWGLayerMappings(Document exportDocument)
    {
        if (exportDocument == null)
        {
            throw new ArgumentNullException(nameof(exportDocument));
        }

        var uniqueMappings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var settings = new FilteredElementCollector(exportDocument)
            .OfClass(typeof(ExportDWGSettings))
            .Cast<ExportDWGSettings>();

        foreach (var setting in settings)
        {
            var layerMapping = setting.GetDWGExportOptions()?.LayerMapping;
            if (!string.IsNullOrWhiteSpace(layerMapping))
            {
                uniqueMappings.Add(layerMapping);
            }
        }

        return uniqueMappings.ToList();
    }

    public void SaveDocumentDWGExportOptions(Document exportDocument, DWGExportOptions dwgExportOptions)
    {
        if (exportDocument == null)
        {
            throw new ArgumentNullException(nameof(exportDocument));
        }

        if (dwgExportOptions == null)
        {
            throw new ArgumentNullException(nameof(dwgExportOptions));
        }

        var activeSettings = ExportDWGSettings.GetActivePredefinedSettings(exportDocument);
        if (activeSettings == null)
        {
            _logger.LogInformation("No active predefined DWG export settings found for the current document.");
            return;
        }

        using (Transaction trans = new Transaction(exportDocument, "Update DWG Export Options"))
        {
            try
            {
                trans.Start();
                activeSettings.SetDWGExportOptions(dwgExportOptions);
                trans.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating DWG export options");
                if (trans.GetStatus() == TransactionStatus.Started)
                {
                    trans.RollBack();
                }
                throw;
            }
        }
    }

    public List<DWGLayerMappingModel> GetDWGLayerMappings()
    {
        return new()
        {
            new DWGLayerMappingModel() { Id = 0, Name = "BS1192", LayerMapping = "BS1192" },
            new DWGLayerMappingModel() { Id = 1, Name = "AIA", LayerMapping = "AIA" },
            new DWGLayerMappingModel() { Id = 2, Name = "CP83", LayerMapping = "CP83" },
            new DWGLayerMappingModel() { Id = 3, Name = "ISO13567", LayerMapping = "ISO13567" }
        };
    }
}
