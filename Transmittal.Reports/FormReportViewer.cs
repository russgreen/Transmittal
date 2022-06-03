using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Transmittal.Reports;
public partial class FormReportViewer : Form
{
    private string _filePathName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Report"); //set default


    public FormReportViewer()
    {
        InitializeComponent();
    }

    public FormReportViewer(string folderPath, string fileName)
    {
        InitializeComponent();
        
        _filePathName = System.IO.Path.Combine(folderPath, fileName);
    }

    private void FormReportViewer_Load(object sender, EventArgs e)
    {

        this.reportViewer1.RefreshReport();
    }

    private void reportViewer1_ReportExport(object sender, Microsoft.Reporting.WinForms.ReportExportEventArgs e)
    {
        e.Cancel = true;
        string mimeType;
        string encoding;
        string fileNameExtension;
        string[] streams;
        Microsoft.Reporting.WinForms.Warning[] warnings;

        Microsoft.Reporting.WinForms.Report report;
        if (reportViewer1.ProcessingMode == Microsoft.Reporting.WinForms.ProcessingMode.Local)
            report = reportViewer1.LocalReport;
        else
            report = reportViewer1.ServerReport;

        var bytes = report.Render(e.Extension.Name, e.DeviceInfo,
                        Microsoft.Reporting.WinForms.PageCountMode.Actual, out mimeType,
                        out encoding, out fileNameExtension, out streams, out warnings);

        var path = $@"{_filePathName}.{fileNameExtension}";
        System.IO.FileInfo file = new System.IO.FileInfo(path);
        file.Directory.Create();
        
        try
        {
            System.IO.File.WriteAllBytes(path, bytes);
            MessageBox.Show($"The report was exported to file {path}");
        }
        catch (Exception)
        {
            MessageBox.Show($"The report could not be saved to {path}. Check the file is not open and you have write access to the folder.");
        }

    }
}
