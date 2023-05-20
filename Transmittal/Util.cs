using Autodesk.Revit.DB;
using System.Drawing.Printing;
using System.Reflection;
using Transmittal.Extensions;

namespace Transmittal;

internal class Util
{
    public static string GetParameterValueString(Element _element, string paramGuidString)
    {
        var value = string.Empty;

        Parameter param = _element.get_Parameter(new Guid(paramGuidString));

        if (param != null)
        {
            //guid is fine but just in case lets check we have the correct name as well
            //if(param.Definition.Name != paramName)
            //{
            //    return (exists, value);
            //}

            if (param.StorageType == StorageType.Integer)
            {
                value = param.AsInteger().ToString();
            }

            if (param.StorageType == StorageType.Double)
            {
                value = param.AsDouble().ToString();
            }

            if (param.StorageType == StorageType.String)
            {
#if REVIT2022_OR_GREATER
                value = param.AsValueString();
#else
               value = param.AsString();
#endif
            }
        }

        return value;
    }

    public static int GetParameterValueInt(Element _element, string paramGuidString)
    {
        var value = 0;

        Parameter param = _element.get_Parameter(new Guid(paramGuidString));

        if (param != null)
        {
            if (param.StorageType == StorageType.Integer)
            {
                value = param.AsInteger();
            }
        }

        return value;
    }
}
