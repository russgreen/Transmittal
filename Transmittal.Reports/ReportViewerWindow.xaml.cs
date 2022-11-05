using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        this.reportViewer.ExportSettings = new BoldReports.Writer.ExportSettings();
        this.reportViewer.ExportSettings.FileName = _filePathName;
    }

    private void reportViewer_ReportExport(object sender, BoldReports.Windows.ReportExportEventArgs e)
    {
        var format = e.ExportFormatType;
    }
}
