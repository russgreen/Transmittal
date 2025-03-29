using System;
using System.Linq;

namespace Transmittal;

public class WarningSwallower : Autodesk.Revit.DB.IFailuresPreprocessor
{
    public Autodesk.Revit.DB.FailureProcessingResult PreprocessFailures(Autodesk.Revit.DB.FailuresAccessor FailuresAccessor)
    {
        var msgAccessorList = FailuresAccessor.GetFailureMessages();
        foreach (Autodesk.Revit.DB.FailureMessageAccessor msgAccessor in msgAccessorList)
        {
            _ = FailuresAccessor.GetTransactionName();
            if (msgAccessor.GetDescriptionText().ToString().ToLower() == "revit will use raster printing" | 
                msgAccessor.GetDescriptionText().ToLower().ToLower() == "the <in-session> print settings will be used")
            {
                FailuresAccessor.DeleteWarning(msgAccessor);
            }
        }

        return Autodesk.Revit.DB.FailureProcessingResult.Continue;
    }
}
