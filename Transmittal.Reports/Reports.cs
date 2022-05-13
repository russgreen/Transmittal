using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reporting.WinForms;
using System.Reflection;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.Extensions;
using System.IO;

namespace Transmittal.Reports
{
    public class Reports
    {
        private string _logoPath;

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
            
            //GetLogoPath(); - no longer used RDLC files should just be customised

            ConfigureDataModelMapping();

        }

        public void ShowProjectDirectoryReport(List<ProjectDirectoryModel> projectDirectory, string projectIdentifier, string projectName) //, EmployeeModel projectLeader, ProjectModel project)
        {
            //var report = Assembly.GetExecutingAssembly().GetManifestResourceStream("eProject.Reporting.Reports.ProjectDirectory.rdlc");
            //report.Seek(0L, SeekOrigin.Begin);
            var report = GetReport("ProjectDirectory.rdlc");

            var paraList = new List<ReportParameter>
            {
                //new ReportParameter("parImagePath", _logoPath),
                new ReportParameter("parProject", $"{projectIdentifier} {projectName}")
            };

            FormReportViewer frm = NewReportViewer(
                "Project Directory",
                paraList,
                report,
                _settingsService.GlobalSettings.DirectoryStore,
                $"{projectIdentifier}-{_settingsService.GlobalSettings.Originator}-ZZ-XX-DY-{_settingsService.GlobalSettings.Role}-0001-ProjectDirectory");

            //we need to pass in a list<T> to the datasets
            //List<CompanyModel> companies = new List<CompanyModel>
            //{
            //    _companyService.Company
            //};

            //List<EmployeeModel> projectLeaders = new List<EmployeeModel>
            //{
            //    projectLeader
            //};

            List<Models.ProjectDirectoryReportModel> projectDirectoryReportModels = new();
            var filteredProjectDirectory = projectDirectory.Where(x => x.ShowInReport == true).ToList();

            //map to the derived model type
            IMapper iMapper = _configDirectory.CreateMapper();

            foreach (var item in filteredProjectDirectory)
            {
                var newItem = iMapper.Map<ProjectDirectoryModel, Models.ProjectDirectoryReportModel>(item);

                projectDirectoryReportModels.Add(newItem);
            }

            //frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("Company", companies));
            //frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ProjectLeader", projectLeaders));
            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ProjectDirectory", projectDirectoryReportModels));

            frm.reportViewer1.RefreshReport();
            frm.ShowDialog();
        }

        public void ShowTransmittalReport(int transmittalID, bool useISO, string projectIdentifier, string projectName)
        {
            Stream report;
            if (useISO == true)
            {
                //report = Assembly.GetExecutingAssembly().GetManifestResourceStream("eProject.Reporting.Reports.TransmittalSheet1192.rdlc");
                report = GetReport("TransmittalSheetISO.rdlc");
            }
            else
            {
                //report = Assembly.GetExecutingAssembly().GetManifestResourceStream("eProject.Reporting.Reports.TransmittalSheet.rdlc");
                report = GetReport("TransmittalSheet.rdlc");
            }
            //report.Seek(0L, SeekOrigin.Begin);

            var paraList = new List<ReportParameter>
            {
                //new ReportParameter("parImagePath", _logoPath)
            };

            TransmittalModel transmittal = _transmittalService.GetTransmittal(transmittalID);
            //ProjectModel project = _projectService.GetProject(transmittal.ProjectID);

            FormReportViewer frm = NewReportViewer(
                "Transmittal Record",
                paraList,
                report,
                _settingsService.GlobalSettings.IssueSheetStore,
                $"{projectIdentifier}-{_settingsService.GlobalSettings.Originator}-ZZ-XX-TL-{_settingsService.GlobalSettings.Role}-{transmittal.ID.ToString().PadLeft(4, '0')}-TransmittalRecord");

            //map to the derived model type
            IMapper iMapper = _configTransmittal.CreateMapper();
            var transmittalReport = iMapper.Map<TransmittalModel, Models.TransmittalReportModel>(transmittal);
            transmittalReport.Project = $"{projectIdentifier} {projectName}";

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
                newDist.Company = _contactDirectoryService.GetCompany(newDist.Person.CompanyID);
                newDist.ContactName = newDist.DisplayName;

                transmittalDistributions.Add(newDist);
            }

            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsTransmittal", transmittals));
            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsTransmittalItems", transmittal.TransmittalItems));
            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsTransmittalDistribution", transmittalDistributions));

            frm.reportViewer1.RefreshReport();
            frm.ShowDialog();
        }

        public void ShowTransmittalSummaryReport(bool useISO, string projectIdentifier, string projectName)
        {
            Stream report;
            if (useISO == true)
            {
                //report = Assembly.GetExecutingAssembly().GetManifestResourceStream("eProject.Reporting.Reports.TransmittalSummary1192.rdlc");
                report = GetReport(  "TransmittalSummaryISO.rdlc");
            }
            else
            {
                //report = Assembly.GetExecutingAssembly().GetManifestResourceStream("eProject.Reporting.Reports.TransmittalSummary.rdlc");
                report = GetReport("TransmittalSummary.rdlc");
            }
            //report.Seek(0L, SeekOrigin.Begin);

            var paraList = new List<ReportParameter>
            {
                //new ReportParameter("parImagePath", _logoPath)
            };

            //ProjectModel project = _projectService.GetProject(projectID);

            FormReportViewer frm = NewReportViewer(
                "Transmittal Record",
                paraList,
                report,
                _settingsService.GlobalSettings.IssueSheetStore,
                $"{projectIdentifier}-{_settingsService.GlobalSettings.Originator}-ZZ-XX-MX-{_settingsService.GlobalSettings.Role}-0001-TransmittalSummary");

            List<TransmittalModel> transmittals = _transmittalService.GetTransmittals();

            List<Models.TransmittalItemReportModel> transmittalItems = new List<Models.TransmittalItemReportModel>();
            List<Models.TransmittalDistributionReportModel> transmittalDistributions = new List<Models.TransmittalDistributionReportModel>();
            
            IMapper iMapper;
            foreach (var transmittal in transmittals)
            {
                iMapper = _configTransmittalItem.CreateMapper();
                foreach (var item in transmittal.TransmittalItems)
                {
                    var newItem = iMapper.Map<TransmittalItemModel, Models.TransmittalItemReportModel>(item);
                    newItem.Project = $"{projectIdentifier} {projectName}";
                    newItem.TransDate = transmittal.TransDate;
                    transmittalItems.Add(newItem);
                }

                iMapper = _configTransmittalDist.CreateMapper();
                foreach (var dist in transmittal.Distribution)
                {
                    var newDist = iMapper.Map<TransmittalDistributionModel, Models.TransmittalDistributionReportModel>(dist);
                    newDist.TransDate = transmittal.TransDate;
                    newDist.Company = _contactDirectoryService.GetCompany(newDist.Person.CompanyID);
                    newDist.ContactName = newDist.DisplayName;
                    transmittalDistributions.Add(newDist);
                }
            }

            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsSummaryItems", transmittalItems));
            frm.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("dsSummaryDistribution", transmittalDistributions));

            frm.reportViewer1.RefreshReport();
            frm.ShowDialog();


        }


        private FormReportViewer NewReportViewer(
            string windowTitle,
            List<ReportParameter> paraList,
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
            frm.reportViewer1.LocalReport.SetParameters(paraList.ToArray());

            frm.reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
            frm.reportViewer1.ZoomMode = ZoomMode.PageWidth;

            frm.reportViewer1.LocalReport.DataSources.Clear();

            return frm;
        }

        private void GetLogoPath()
        {
            if (_settingsService.GlobalSettings.ReportStore.EndsWith(@"\"))
            {
                _logoPath = $@"file:///{_settingsService.GlobalSettings.ReportStore.TrimEnd(@"\"[0])}\logo.jpg";
            }
            else
            {
                _logoPath = $@"file:///{_settingsService.GlobalSettings.ReportStore}\logo.jpg";
            }
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
            string reportFilePath;
            if (_settingsService.GlobalSettings.ReportStore.EndsWith(@"\"))
            {
                reportFilePath = $@"{_settingsService.GlobalSettings.ReportStore.TrimEnd(@"\"[0])}\{reportName}";
            }
            else
            {
                reportFilePath = $@"{_settingsService.GlobalSettings.ReportStore}\{reportName}";
            }

            if (File.Exists(reportFilePath))
            {
                var report = File.OpenRead(reportFilePath);
                return report;
            }
            else
            {
                var report = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(Transmittal)}.{nameof(Transmittal.Reports)}.{nameof(Transmittal)}.{nameof(Transmittal.Reports.Reports)}.{reportName}");
                report.Seek(0L, SeekOrigin.Begin);
                return report;
            }
        }

    }

}
