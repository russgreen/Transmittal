using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;
using Microsoft.Extensions.Logging;

namespace Transmittal;
internal class AppDocEvents
{
    private readonly ILogger<AppDocEvents> _logger;

    public AppDocEvents()
    {
        _logger = Host.GetService<ILogger<AppDocEvents>>();
    }

    public void EnableEvents()
    {
        App.CachedUiCtrApp.Idling += new EventHandler<IdlingEventArgs>(OnIdling);
        App.CachedUiCtrApp.ViewActivated += new EventHandler<ViewActivatedEventArgs>(OnViewActivated);
        App.CachedUiCtrApp.ApplicationClosing += new EventHandler<ApplicationClosingEventArgs>(ApplicationClosing);


        App.CtrApp.DocumentClosed += OnDocumentClosed;
        App.CtrApp.DocumentOpened += OnDocumentOpened;
        App.CtrApp.DocumentSaved += OnDocumentSaved;
        App.CtrApp.DocumentSavedAs += OnDocumentSavedAs;
    }

    public void DisableEvents()
    {
        //App.CachedUiCtrApp.Idling -= OnIdling;
        //App.CachedUiCtrApp.ViewActivated -= OnViewActivated;

        App.CtrApp.DocumentClosed -= OnDocumentClosed;
        App.CtrApp.DocumentOpened -= OnDocumentOpened;
        App.CtrApp.DocumentSaved -= OnDocumentSaved;
        App.CtrApp.DocumentSavedAs -= OnDocumentSavedAs;
    }

    private void OnDocumentSavedAs(object sender, DocumentSavedAsEventArgs e)
    {
        _logger.LogDebug("DocumentSavedAs");
    }

    private void OnDocumentSaved(object sender, DocumentSavedEventArgs e)
    {
        _logger.LogDebug("DocumentSaved");
    }

    private void OnDocumentOpened(object sender, DocumentOpenedEventArgs e)
    {
        _logger.LogDebug("DocumentOpened");
    }

    private void OnDocumentClosed(object sender, DocumentClosedEventArgs e)
    {
        _logger.LogDebug("DocumentClosed");
    }

    private void OnIdling(object sender, IdlingEventArgs e)
    {
    }

    private void OnViewActivated(object sender, ViewActivatedEventArgs e)
    {
        _logger.LogDebug("ViewActivated {view}", e.CurrentActiveView.Name);
    }

    private void ApplicationClosing(object sender, ApplicationClosingEventArgs e)
    {
        _logger.LogDebug("ApplicationClosing");
    }
}
