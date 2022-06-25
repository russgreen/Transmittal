using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reporting.WinForms;
using System.Reflection;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.Extensions;
using System.IO;
using System.Linq;

namespace Transmittal.Reports
{
    public class Reports
    {
        private MapperConfiguration _configDirectory;
        private MapperConfiguration _configTransmittal;
        private MapperConfiguration _configTransmittalItem;
        private MapperConfiguration _configTransmittalDist;

        private readonly ISettingsService _settingsService;
        private readonly IContactDirectoryService _contactDirectoryService;
        private readonly ITransmittalService _transmittalService;

        public Reports(IServiceProvider serviceProvider, string reportStore)
        {
            _settingsService = serviceProvider.GetRequiredService<ISettingsService>();
            _contactDirectoryService = serviceProvider.GetRequiredService<IContactDirectoryService>();
            _transmittalService = serviceProvider.GetRequiredService<ITransmittalService>();

            ConfigureDataModelMapping();
        }

        public Reports(ISettingsService settingsService, 
            IContactDirectoryService contactDirectoryService, 
            ITransmittalService transmittalService)
        {
            _settingsService = settingsService;
            _contactDirectoryService = contactDirectoryService;
            _transmittalService = transmittalService;          

            ConfigureDataModelMapping();
        }

        public void ShowProjectDirectoryReport(List<ProjectDirectoryModel> projectDirectory) //, string projectIdentifier, string projectName) //, EmployeeModel projectLeader, ProjectModel project)
        {
            var report = GetReport("ProjectDirectory.rdlc");

            FormReportViewer frm = NewReportViewer(
                "Project Directory",
                report,
                _settingsService.GlobalSettings.DirectoryStore.ParsePathWithEnvironmentVariables(),
                $"{_settingsService.GlobalSettings.ProjectNumber}-{_settingsService.GlobalSettings.Originator}-ZZ-XX-DY-{_settingsService.GlobalSettings.Role}-0001-ProjectDirectory");

            List<Models.ProjectDirectoryReportModel> projectDirectoryReportModels = new();
            var filteredProjectDirectory = projectDirectory.Where(x => x.Person.ShowInReport == true).ToList();   

            //map to the derived model type
            IMapper iMapper = _configDirectory.CreateMapper();

            foreach (var item in filteredProjectDirectory)
            {
                var newItem = iMapper.Map<ProjectDirectoryModel, Models.ProjectDirectoryReportModel>(item);

                projectDirectoryReportModels.Add(newItem);
            }

            //we need to pass in a list<T> to the datasets
            Models.ProjectModel project = new()
            {
                ProjectIdentifier = _settingsService.GlobalSettings.ProjectIdentifier,
                ProjectNumber = _settingsService.GlobalSettings.ProjectNumber,
                ProjectName = _settingsService.GlobalSettings.ProjectName,
            };
            List<Models.ProjectModel> projects = new List<Models.ProjectModel>
            {
                project
            };

            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsProject", projects));
            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsProjectDirectory", projectDirectoryReportModels));

            frm.reportViewer1.RefreshReport();
            frm.ShowDialog();
            //frm.Show();
        }

        public void ShowTransmittalReport(int transmittalID) //, bool useISO, string projectIdentifier, string projectName)
        {
            Stream report = GetReport("TransmittalSheet.rdlc");

            TransmittalModel transmittal = _transmittalService.GetTransmittal(transmittalID);

            FormReportViewer frm = NewReportViewer(
                "Transmittal Record",
                report,
                _settingsService.GlobalSettings.IssueSheetStore.ParsePathWithEnvironmentVariables(),
                $"{_settingsService.GlobalSettings.ProjectNumber}-{_settingsService.GlobalSettings.Originator}-ZZ-XX-TL-{_settingsService.GlobalSettings.Role}-{transmittal.ID.ToString().PadLeft(4, '0')}-TransmittalRecord");

            //map to the derived model type
            IMapper iMapper = _configTransmittal.CreateMapper();
            var transmittalReport = iMapper.Map<TransmittalModel, Models.TransmittalReportModel>(transmittal);

            //we need to pass in a list<T> to the datasets
            List<Models.TransmittalReportModel> transmittals = new List<Models.TransmittalReportModel>
            {
                transmittalReport 
            };
            List<Models.TransmittalDistributionReportModel> transmittalDistributions = new List<Models.TransmittalDistributionReportModel>();

            iMapper = _configTransmittalDist.CreateMapper();
            foreach (var item in transmittal.Distribution)
            {
                var newDist = iMapper.Map<TransmittalDistributionModel, Models.TransmittalDistributionReportModel>(item);
                newDist.Person = _contactDirectoryService.GetPerson(newDist.PersonID);
                newDist.Company = _contactDirectoryService.GetCompany(newDist.Person.CompanyID);

                transmittalDistributions.Add(newDist);
            }

            Models.ProjectModel project = new()
            {
                ProjectIdentifier = _settingsService.GlobalSettings.ProjectIdentifier,
                ProjectNumber = _settingsService.GlobalSettings.ProjectNumber,
                ProjectName = _settingsService.GlobalSettings.ProjectName,
            };
            List<Models.ProjectModel> projects = new List<Models.ProjectModel>
            {
                project
            };

            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsProject", projects));
            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsTransmittal", transmittals));
            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsTransmittalItems", transmittal.Items));
            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsTransmittalDistribution", transmittalDistributions));

            frm.reportViewer1.RefreshReport();
            frm.ShowDialog();
        }

        public void ShowTransmittalSummaryReport()//bool useISO, string projectIdentifier, string projectName)
        {
            Stream report = GetReport(  "TransmittalSummary.rdlc");

            FormReportViewer frm = NewReportViewer(
                "Transmittal Record",
                report,
                _settingsService.GlobalSettings.IssueSheetStore.ParsePathWithEnvironmentVariables(),
                $"{_settingsService.GlobalSettings.ProjectNumber}-{_settingsService.GlobalSettings.Originator}-ZZ-XX-MX-{_settingsService.GlobalSettings.Role}-0001-TransmittalSummary");

            List<TransmittalModel> transmittals = _transmittalService.GetTransmittals();

            List<Models.TransmittalItemReportModel> transmittalItems = new List<Models.TransmittalItemReportModel>();
            List<Models.TransmittalDistributionReportModel> transmittalDistributions = new List<Models.TransmittalDistributionReportModel>();
            
            IMapper iMapper;
            foreach (var transmittal in transmittals)
            {
                iMapper = _configTransmittalItem.CreateMapper();
                foreach (var item in transmittal.Items)
                {
                    var newItem = iMapper.Map<TransmittalItemModel, Models.TransmittalItemReportModel>(item);
                    newItem.Project = $"{_settingsService.GlobalSettings.ProjectNumber} {_settingsService.GlobalSettings.ProjectName}";
                    newItem.TransDate = transmittal.TransDate;
                    transmittalItems.Add(newItem);
                }

                iMapper = _configTransmittalDist.CreateMapper();
                foreach (var dist in transmittal.Distribution)
                {
                    var newDist = iMapper.Map<TransmittalDistributionModel, Models.TransmittalDistributionReportModel>(dist);
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
            };
            List<Models.ProjectModel> projects = new List<Models.ProjectModel>
            {
                project
            };

            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsProject", projects));
            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsSummaryItems", transmittalItems));
            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsSummaryDistribution", transmittalDistributions));

            frm.reportViewer1.RefreshReport();
            frm.ShowDialog();


        }


        private FormReportViewer NewReportViewer(
            string windowTitle,
            Stream report,
            string folderPath,
            string fileName)
        {
            FormReportViewer frm = new FormReportViewer(folderPath, fileName)
            {
                Text = windowTitle
            };
            
            frm.reportViewer1.Reset();

            frm.reportViewer1.LocalReport.LoadReportDefinition(report);
            frm.reportViewer1.LocalReport.EnableExternalImages = true;

            frm.reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
            frm.reportViewer1.ZoomMode = ZoomMode.PageWidth;

            frm.reportViewer1.LocalReport.DataSources.Clear();

            return frm;
        }

        private void ConfigureDataModelMapping()
        {
            _configDirectory = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProjectDirectoryModel, Models.ProjectDirectoryReportModel>();
            });

            _configTransmittal = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TransmittalModel, Models.TransmittalReportModel>();
            });

            _configTransmittalItem = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TransmittalItemModel, Models.TransmittalItemReportModel>();
            });

            _configTransmittalDist = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TransmittalDistributionModel, Models.TransmittalDistributionReportModel>();
            });
        }

        private Stream GetReport(string reportName)
        {
            var folder = string.Empty;

            if(_settingsService.GlobalSettings.ReportStore != null)
            {
                folder = _settingsService.GlobalSettings.ReportStore.ParsePathWithEnvironmentVariables();   
            }

            string reportFilePath = Path.Combine(folder, reportName);
            var dir = Path.GetDirectoryName(reportFilePath);

            if (dir != string.Empty.ToString() ||
                File.Exists(reportFilePath))
            {
                var report = File.OpenRead(reportFilePath);
                return report;
            }
            else
            {
                var report = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(Transmittal)}.{nameof(Transmittal.Reports)}.{nameof(Transmittal.Reports.Reports)}.{reportName}");
                report.Seek(0L, SeekOrigin.Begin);
                return report;
            }
        }

    }

}
