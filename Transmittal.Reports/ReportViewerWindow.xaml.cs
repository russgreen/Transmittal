using Microsoft.Reporting.WinForms;
using System.Windows;

namespace Transmittal.Reports;
/// <summary>
/// Interaction logic for ReportViewerWindow.xaml
/// </summary>
public partial class ReportViewerWindow : Window
{
    private string _filePathName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Report");

    public ReportViewerWindow()
    {
        InitializeComponent();
    }


    public ReportViewerWindow(string folderPath, string fileName)
    {
        InitializeComponent();

        _filePathName = System.IO.Path.Combine(folderPath, fileName);

        reportViewer.ReportExport += ReportViewer_ReportExport;
    }

    private void ReportViewer_ReportExport(object sender, ReportExportEventArgs e)
    {
        e.Cancel = true;
        string mimeType;
        string encoding;
        string fileNameExtension;
        string[] streams;
        Microsoft.Reporting.WinForms.Warning[] warnings;

        Microsoft.Reporting.WinForms.Report report;
        if (reportViewer.ProcessingMode == Microsoft.Reporting.WinForms.ProcessingMode.Local)
        {
            report = reportViewer.LocalReport;
        }
        else
        {
            report = reportViewer.ServerReport;
        }

        var bytes = report.Render(e.Extension.Name, e.DeviceInfo,
                        Microsoft.Reporting.WinForms.PageCountMode.Actual, out mimeType,
                        out encoding, out fileNameExtension, out streams, out warnings);

        var path = $@"{_filePathName}.{fileNameExtension}";
        System.IO.FileInfo file = new System.IO.FileInfo(path);
        file.Directory.Create();

        try
        {
            System.IO.File.WriteAllBytes(path, bytes);
            System.Windows.MessageBox.Show($"The report was exported to file {path}");
        }
        catch
        {
            System.Windows.MessageBox.Show($"The report could not be saved to {path}. Check the file is not open and you have write access to the folder.");
        }
    }
}
