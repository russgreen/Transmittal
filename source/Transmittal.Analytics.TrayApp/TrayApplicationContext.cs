using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Transmittal.Analytics.TrayApp;

/// <summary>
/// Application context for the system tray application
/// </summary>
public class TrayApplicationContext : ApplicationContext
{
    private readonly ILogger<TrayApplicationContext> _logger;
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _contextMenu;

    public TrayApplicationContext(ILogger<TrayApplicationContext> logger)
    {
        _logger = logger;
        
        // Create context menu
        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.Add("About", null, OnAbout);
        _contextMenu.Items.Add("Exit", null, OnExit);

        // Create tray icon
        _trayIcon = new NotifyIcon
        {
            Icon = GetApplicationIcon(),
            ContextMenuStrip = _contextMenu,
            Text = "Transmittal Analytics Service",
            Visible = true
        };

        _trayIcon.DoubleClick += OnTrayIconDoubleClick;

        _logger.LogInformation("Tray application context initialized");
    }

    private Icon GetApplicationIcon()
    {
        try
        {
            // Try to load the embedded icon resource
            var assembly = Assembly.GetExecutingAssembly();
            var iconStream = assembly.GetManifestResourceStream("Transmittal.Analytics.TrayApp.Transmittal.ico");
            
            if (iconStream != null)
            {
                return new Icon(iconStream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load application icon, using default");
        }

        // Fallback to system icon
        return SystemIcons.Application;
    }

    private void OnTrayIconDoubleClick(object sender, EventArgs e)
    {
        ShowBalloonTip("Transmittal Analytics", "Analytics service is running", ToolTipIcon.Info);
    }

    private void OnAbout(object sender, EventArgs e)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
        MessageBox.Show(
            $"Transmittal Analytics Service\nVersion: {version}\n\nThis service collects usage analytics for the Transmittal application.",
            "About Transmittal Analytics",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void OnExit(object sender, EventArgs e)
    {
        _logger.LogInformation("Exit requested from tray menu");
        ExitThread();
    }

    private void ShowBalloonTip(string title, string text, ToolTipIcon icon)
    {
        _trayIcon.ShowBalloonTip(3000, title, text, icon);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _trayIcon?.Dispose();
            _contextMenu?.Dispose();
        }
        base.Dispose(disposing);
    }
}