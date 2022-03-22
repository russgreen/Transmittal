using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;

namespace Transmittal;
internal class AppDocEvents
{

    public AppDocEvents()
    {
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
    }

    private void OnDocumentSaved(object sender, DocumentSavedEventArgs e)
    {
    }

    private void OnDocumentOpened(object sender, DocumentOpenedEventArgs e)
    {
    }

    private void OnDocumentClosed(object sender, DocumentClosedEventArgs e)
    {
    }

    private void OnIdling(object sender, IdlingEventArgs e)
    {
    }

    private void OnViewActivated(object sender, ViewActivatedEventArgs e)
    {
    }

    private void ApplicationClosing(object sender, ApplicationClosingEventArgs e)
    {
    }
}
