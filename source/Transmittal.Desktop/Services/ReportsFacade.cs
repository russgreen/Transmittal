using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.Extensions;
using Microsoft.Extensions.Logging;
using System.IO;
using System;

namespace Transmittal.Desktop.Services;

internal sealed class ReportsFacade
{
    private readonly ISettingsService _settingsService;
    private readonly IContactDirectoryService _contactDirectoryService;
    private readonly ITransmittalService _transmittalService;
    private readonly ILogger _logger;

    public ReportsFacade(
        ISettingsService settingsService,
        IContactDirectoryService contactDirectoryService,
        ITransmittalService transmittalService,
        ILogger logger = null)
    {
        _settingsService = settingsService;
        _contactDirectoryService = contactDirectoryService;
        _transmittalService = transmittalService;
        _logger = logger;
    }

    public void ShowProjectDirectoryReport(List<ProjectDirectoryModel> projectDirectory)
    {
        if (TryInvokeOpenXmlReport("ProjectDirectory.xlsx", nameof(ShowProjectDirectoryReport), projectDirectory))
        {
            return;
        }

        var fallbackReports = new global::Transmittal.Reports.Reports(_settingsService, _contactDirectoryService, _transmittalService);
        fallbackReports.ShowProjectDirectoryReport(projectDirectory);
    }

    public void ShowTransmittalReport(int transmittalID)
    {
        if (TryInvokeOpenXmlReport("TransmittalSheet.xlsx", nameof(ShowTransmittalReport), transmittalID))
        {
            return;
        }

        var fallbackReports = new global::Transmittal.Reports.Reports(_settingsService, _contactDirectoryService, _transmittalService);
        fallbackReports.ShowTransmittalReport(transmittalID);
    }

    public void ShowTransmittalSummaryReport(List<TransmittalModel> transmittals = null)
    {
        if (TryInvokeOpenXmlReport("TransmittalSummary.xlsx", nameof(ShowTransmittalSummaryReport), transmittals))
        {
            return;
        }

        var fallbackReports = new global::Transmittal.Reports.Reports(_settingsService, _contactDirectoryService, _transmittalService);
        fallbackReports.ShowTransmittalSummaryReport(transmittals);
    }

    public void ShowMasterDocumentsListReport()
    {
        if (TryInvokeOpenXmlReport("MasterDocumentsList.xlsx", nameof(ShowMasterDocumentsListReport)))
        {
            return;
        }

        var fallbackReports = new global::Transmittal.Reports.Reports(_settingsService, _contactDirectoryService, _transmittalService);
        fallbackReports.ShowMasterDocumentsListReport();
    }

    private bool ShouldUseOpenXmlReport(string templateFileName, out string templatePath)
    {
        templatePath = null;

        var reportStore = _settingsService.GlobalSettings.ReportStore;
        if (string.IsNullOrWhiteSpace(reportStore))
        {
            return false;
        }

        var reportStorePath = reportStore.ParsePathWithEnvironmentVariables();
        if (string.IsNullOrWhiteSpace(reportStorePath))
        {
            return false;
        }

        templatePath = Path.Combine(reportStorePath, templateFileName);
        return File.Exists(templatePath);
    }

    private bool TryInvokeOpenXmlReport(string templateFileName, string methodName, params object[] arguments)
    {
        if (!ShouldUseOpenXmlReport(templateFileName, out var templatePath))
        {
            _logger?.LogInformation("Using RDLC report engine for {MethodName}: XLSX template not found ({TemplateFileName})", methodName, templateFileName);
            return false;
        }

        var reportsType = Type.GetType("Transmittal.Reports.OpenXML.Reports, Transmittal.Reports.OpenXML");
        if (reportsType == null)
        {
            _logger?.LogWarning("Using RDLC report engine for {MethodName}: OpenXML reports type could not be resolved while template exists at {TemplatePath}", methodName, templatePath);
            return false;
        }

        var constructor = reportsType.GetConstructor(new[]
        {
            typeof(ISettingsService),
            typeof(IContactDirectoryService),
            typeof(ITransmittalService)
        });

        if (constructor == null)
        {
            _logger?.LogWarning("Using RDLC report engine for {MethodName}: OpenXML reports constructor is unavailable", methodName);
            return false;
        }

        var reportsInstance = constructor.Invoke(new object[]
        {
            _settingsService,
            _contactDirectoryService,
            _transmittalService
        });

        var method = reportsType.GetMethod(methodName);
        if (method == null)
        {
            _logger?.LogWarning("Using RDLC report engine for {MethodName}: OpenXML method was not found", methodName);
            return false;
        }

        method.Invoke(reportsInstance, arguments);
        _logger?.LogInformation("Using OpenXML report engine for {MethodName} with template {TemplatePath}", methodName, templatePath);
        return true;
    }
}
