namespace Transmittal;

public class WarningSwallower : Autodesk.Revit.DB.IFailuresPreprocessor
{
    //private List<Autodesk.Revit.DB.FailureSeverity> FailureSeverityList
    //{
    //    get
    //    {
    //        var list = new List<Autodesk.Revit.DB.FailureSeverity>
    //        { 
    //            Autodesk.Revit.DB.FailureSeverity.Warning,
    //            Autodesk.Revit.DB.FailureSeverity.Error
    //        };
    //        return list;
    //    }
    //}

    public Autodesk.Revit.DB.FailureProcessingResult PreprocessFailures(Autodesk.Revit.DB.FailuresAccessor FailuresAccessor)
    {
        var msgAccessorList = FailuresAccessor.GetFailureMessages();
        foreach (Autodesk.Revit.DB.FailureMessageAccessor msgAccessor in msgAccessorList)
        {
            _ = FailuresAccessor.GetTransactionName();
            if (msgAccessor.GetDescriptionText().ToString().ToLower()
                .Contains("revit will use raster printing") == true | 
                msgAccessor.GetDescriptionText().ToLower().ToLower()
                .Contains("the <in-session> print settings will be used"))
            {
                FailuresAccessor.DeleteWarning(msgAccessor);
            }
        }

        return Autodesk.Revit.DB.FailureProcessingResult.Continue;
    }
}
