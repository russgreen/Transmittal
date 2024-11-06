using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reporting.WinForms;
using System.IO;
using System.Reflection;
using Transmittal.Library.Extensions;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Reports.Mapping;

namespace Transmittal.Reports
{
    public class Reports
    {
        private readonly ISettingsService _settingsService;
        private readonly IContactDirectoryService _contactDirectoryService;
        private readonly ITransmittalService _transmittalService;

        public Reports(IServiceProvider serviceProvider, string reportStore)
        {
            _settingsService = serviceProvider.GetRequiredService<ISettingsService>();
            _contactDirectoryService = serviceProvider.GetRequiredService<IContactDirectoryService>();
            _transmittalService = serviceProvider.GetRequiredService<ITransmittalService>();
        }

        public Reports(ISettingsService settingsService, 
            IContactDirectoryService contactDirectoryService, 
            ITransmittalService transmittalService)
        {
            _settingsService = settingsService;
            _contactDirectoryService = contactDirectoryService;
            _transmittalService = transmittalService; 
        }

        public void ShowProjectDirectoryReport(List<ProjectDirectoryModel> projectDirectory) //, string projectIdentifier, string projectName) //, EmployeeModel projectLeader, ProjectModel project)
        {
            var report = GetReport("ProjectDirectory.rdlc");

            var fileName = _settingsService.GlobalSettings.FileNameFilter.ParseFilename(_settingsService.GlobalSettings.ProjectNumber,
                _settingsService.GlobalSettings.ProjectIdentifier,
                _settingsService.GlobalSettings.ProjectName,
                _settingsService.GlobalSettings.Originator,
                "ZZ",
                "XX",
                "DY",
                _settingsService.GlobalSettings.Role,
                "0001",
                "ProjectDirectory",
                null, null, null);

            var frm = NewReportViewerWPF(
                "Project Directory",
                report,
                _settingsService.GlobalSettings.DirectoryStore.ParsePathWithEnvironmentVariables(),
                fileName);

            List<Models.ProjectDirectoryReportModel> projectDirectoryReportModels = new();
            var filteredProjectDirectory = projectDirectory.Where(x => x.Person.ShowInReport == true).ToList();   

            foreach (var item in filteredProjectDirectory)
            {
                var newItem = item.ToProjectDirectoryReportModel();

                projectDirectoryReportModels.Add(newItem);
            }

            //we need to pass in a list<T> to the datasets
            Models.ProjectModel project = new()
            {
                ProjectIdentifier = _settingsService.GlobalSettings.ProjectIdentifier,
                ProjectNumber = _settingsService.GlobalSettings.ProjectNumber,
                ProjectName = _settingsService.GlobalSettings.ProjectName,
                ClientName = _settingsService.GlobalSettings.ClientName,
            };
            List<Models.ProjectModel> projects = new List<Models.ProjectModel>
            {
                project
            };

            frm.reportViewer.LocalReport.DataSources.Add(new ReportDataSource("dsProject", projects));
            frm.reportViewer.LocalReport.DataSources.Add(new ReportDataSource("dsProjectDirectory", projectDirectoryReportModels));

            frm.reportViewer.RefreshReport();
            frm.ShowDialog();
        }

        public void ShowTransmittalReport(int transmittalID) //, bool useISO, string projectIdentifier, string projectName)
        {
            Stream report = GetReport("TransmittalSheet.rdlc");

            TransmittalModel transmittal = _transmittalService.GetTransmittal(transmittalID);

            var fileName = _settingsService.GlobalSettings.FileNameFilter.ParseFilename(_settingsService.GlobalSettings.ProjectNumber,
                            _settingsService.GlobalSettings.ProjectIdentifier,
                            _settingsService.GlobalSettings.ProjectName,
                            _settingsService.GlobalSettings.Originator,
                            "ZZ",
                            "XX",
                            "TL",
                            _settingsService.GlobalSettings.Role,
                            transmittal.ID.ToString().PadLeft(4, '0'),
                            "TransmittalRecord",
                            null, null, null);

            var frm = NewReportViewerWPF(
                "Transmittal Record",
                report,
                _settingsService.GlobalSettings.IssueSheetStore.ParsePathWithEnvironmentVariables(),
                fileName);

            //we need to pass in a list<T> to the datasets
            List<TransmittalModel> transmittals = new List<TransmittalModel>
            {
                transmittal
            };

            List<Models.TransmittalDistributionReportModel> transmittalDistributions = new List<Models.TransmittalDistributionReportModel>();

            foreach (var item in transmittal.Distribution)
            {
                var newDist = item.ToTransmittalDistributionReportModel();
                newDist.Person = _contactDirectoryService.GetPerson(newDist.PersonID);
                newDist.Company = _contactDirectoryService.GetCompany(newDist.Person.CompanyID);

                transmittalDistributions.Add(newDist);
            }

            Models.ProjectModel project = new()
            {
                ProjectIdentifier = _settingsService.GlobalSettings.ProjectIdentifier,
                ProjectNumber = _settingsService.GlobalSettings.ProjectNumber,
                ProjectName = _settingsService.GlobalSettings.ProjectName,
                ClientName = _settingsService.GlobalSettings.ClientName,
            };
            List<Models.ProjectModel> projects = new List<Models.ProjectModel>
            {
                project
            };

            frm.reportViewer.LocalReport.DataSources.Add(new ReportDataSource("dsProject", projects));
            frm.reportViewer.LocalReport.DataSources.Add(new ReportDataSource("dsTransmittal", transmittals));
            frm.reportViewer.LocalReport.DataSources.Add(new ReportDataSource("dsTransmittalItems", transmittal.Items));
            frm.reportViewer.LocalReport.DataSources.Add(new ReportDataSource("dsTransmittalDistribution", transmittalDistributions));

            frm.reportViewer.RefreshReport();
            frm.ShowDialog();
        }

        public void ShowTransmittalSummaryReport(List<TransmittalModel> transmittals = null)//bool useISO, string projectIdentifier, string projectName)
        {
            Stream report = GetReport(  "TransmittalSummary.rdlc");

            var fileName = _settingsService.GlobalSettings.FileNameFilter.ParseFilename(_settingsService.GlobalSettings.ProjectNumber,
                _settingsService.GlobalSettings.ProjectIdentifier,
                _settingsService.GlobalSettings.ProjectName,
                _settingsService.GlobalSettings.Originator,
                "ZZ",
                "XX",
                "MX",
                _settingsService.GlobalSettings.Role,
                "0001",
                "TransmittalSummary",
                null, null, null);

            var frm = NewReportViewerWPF(
                "Transmittal Summary",
                report,
                _settingsService.GlobalSettings.IssueSheetStore.ParsePathWithEnvironmentVariables(),
                fileName);

            if(transmittals == null)
            {
                transmittals = _transmittalService.GetTransmittals();
            }
            //List<TransmittalModel> transmittals = _transmittalService.GetTransmittals();

            List<Models.TransmittalItemReportModel> transmittalItems = new List<Models.TransmittalItemReportModel>();
            List<Models.TransmittalDistributionReportModel> transmittalDistributions = new List<Models.TransmittalDistributionReportModel>();
            
            foreach (var transmittal in transmittals)
            {
                foreach (var item in transmittal.Items)
                {
                    var newItem = item.ToTransmittalItemReportModel();
                    newItem.Project = $"{_settingsService.GlobalSettings.ProjectNumber} {_settingsService.GlobalSettings.ProjectName}";
                    newItem.TransDate = transmittal.TransDate;
                    transmittalItems.Add(newItem);
                }

                foreach (var dist in transmittal.Distribution)
                {
                    var newDist = dist.ToTransmittalDistributionReportModel();
                    newDist.TransDate = transmittal.TransDate;
                    newDist.Person = _contactDirectoryService.GetPerson(newDist.PersonID);
                    newDist.Company = _contactDirectoryService.GetCompany(newDist.Person.CompanyID);
                    transmittalDistributions.Add(newDist);
                }
            }

            //we need to pass in a list<T> to the datasets
            Models.ProjectModel project = new()
            {
                ProjectIdentifier = _settingsService.GlobalSettings.ProjectIdentifier,
                ProjectNumber = _settingsService.GlobalSettings.ProjectNumber,
                ProjectName = _settingsService.GlobalSettings.ProjectName,
                ClientName = _settingsService.GlobalSettings.ClientName,
            };
            List<Models.ProjectModel> projects = new List<Models.ProjectModel>
            {
                project
            };

            frm.reportViewer.LocalReport.DataSources.Add(new ReportDataSource("dsProject", projects));
            frm.reportViewer.LocalReport.DataSources.Add(new ReportDataSource("dsSummaryItems", transmittalItems));
            frm.reportViewer.LocalReport.DataSources.Add(new ReportDataSource("dsSummaryDistribution", transmittalDistributions));

            frm.reportViewer.RefreshReport();
            frm.ShowDialog();
        }


        public void ShowMasterDocumentsListReport()
        {
            Stream report = GetReport("MasterDocumentsList.rdlc");

            var fileName = _settingsService.GlobalSettings.FileNameFilter.ParseFilename(_settingsService.GlobalSettings.ProjectNumber,
                _settingsService.GlobalSettings.ProjectIdentifier,
                _settingsService.GlobalSettings.ProjectName,
                _settingsService.GlobalSettings.Originator,
                "ZZ",
                "XX",
                "MX",
                _settingsService.GlobalSettings.Role,
                "0002",
                "MasterDocumentsList",
                null, null, null);

            var frm = NewReportViewerWPF(
                "Master Documents List",
                report,
                _settingsService.GlobalSettings.IssueSheetStore.ParsePathWithEnvironmentVariables(),
                fileName);

            List<TransmittalModel> transmittals = _transmittalService.GetTransmittals();

            List<Models.TransmittalItemReportModel> transmittalItems = new List<Models.TransmittalItemReportModel>();

            foreach (var transmittal in transmittals)
            {
                foreach (var item in transmittal.Items)
                {
                    var newItem = item.ToTransmittalItemReportModel();
                    newItem.Project = $"{_settingsService.GlobalSettings.ProjectNumber} {_settingsService.GlobalSettings.ProjectName}";
                    newItem.TransDate = transmittal.TransDate;
                    transmittalItems.Add(newItem);
                }
            }

            //we need to pass in a list<T> to the datasets
            Models.ProjectModel project = new()
            {
                ProjectIdentifier = _settingsService.GlobalSettings.ProjectIdentifier,
                ProjectNumber = _settingsService.GlobalSettings.ProjectNumber,
                ProjectName = _settingsService.GlobalSettings.ProjectName,
                ClientName = _settingsService.GlobalSettings.ClientName,
            };
            List<Models.ProjectModel> projects = new List<Models.ProjectModel>
            {
                project
            };

            frm.reportViewer.LocalReport.DataSources.Add(new ReportDataSource("dsProject", projects));
            frm.reportViewer.LocalReport.DataSources.Add(new ReportDataSource("dsSummaryItems", transmittalItems));
           
            frm.reportViewer.RefreshReport();
            frm.ShowDialog();
        }

        private ReportViewerWindow NewReportViewerWPF(
            string windowTitle,
            Stream report,
            string folderPath,
            string fileName)
        {
            ReportViewerWindow reportViewerWindow = new(folderPath, fileName);

            reportViewerWindow.Title = windowTitle;

            reportViewerWindow.reportViewer.Reset();

            reportViewerWindow.reportViewer.LocalReport.LoadReportDefinition(report);
            reportViewerWindow.reportViewer.LocalReport.EnableExternalImages = true;

            reportViewerWindow.reportViewer.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewerWindow.reportViewer.ZoomMode = ZoomMode.PageWidth;

            reportViewerWindow.reportViewer.LocalReport.DataSources.Clear();

            return reportViewerWindow;
        }

        private System.IO.Stream GetReport(string reportName)
        {
            var folder = string.Empty;

            if(_settingsService.GlobalSettings.ReportStore != null)
            {
                folder = _settingsService.GlobalSettings.ReportStore.ParsePathWithEnvironmentVariables();   
            }

            string reportFilePath = Path.Combine(folder, reportName);
            var dir = Path.GetDirectoryName(reportFilePath);

            if (dir != string.Empty.ToString() &
                File.Exists(reportFilePath))
            {
                Stream report = new FileStream(reportFilePath, FileMode.Open, FileAccess.Read);
                report.Seek(0L, SeekOrigin.Begin);
                return report;
            }
            else
            {
                Stream report = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(Transmittal)}.{nameof(Transmittal.Reports)}.{nameof(Transmittal.Reports.Reports)}.{reportName}");
                report.Seek(0L, SeekOrigin.Begin);
                return report;
            }
        }
    }

}
