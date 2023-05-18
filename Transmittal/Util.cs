using Autodesk.Revit.DB;
using System.Drawing.Printing;
using System.Reflection;
using Transmittal.Extensions;

namespace Transmittal;

internal class Util
{

    public static List<Models.PaperSizeModel> SupportedPaperSizes()
    {
        List<Models.PaperSizeModel> paperSizes = new();

        paperSizes.Clear();
        paperSizes.Add(new Models.PaperSizeModel() { Width = 1189d, Height = 841d, Name = "A0" });
        paperSizes.Add(new Models.PaperSizeModel() { Width = 1188d, Height = 840d, Name = "A0" });
        paperSizes.Add(new Models.PaperSizeModel() { Width = 1190d, Height = 840d, Name = "A0" });
        paperSizes.Add(new Models.PaperSizeModel() { Width = 841d, Height = 594d, Name = "A1" });
        paperSizes.Add(new Models.PaperSizeModel() { Width = 840d, Height = 594d, Name = "A1" });
        paperSizes.Add(new Models.PaperSizeModel() { Width = 594d, Height = 420d, Name = "A2" });
        paperSizes.Add(new Models.PaperSizeModel() { Width = 420d, Height = 297d, Name = "A3" });
        paperSizes.Add(new Models.PaperSizeModel() { Width = 297d, Height = 210d, Name = "A4" });
        paperSizes.Add(new Models.PaperSizeModel() { Width = 210d, Height = 297d, Name = "A4" });
        paperSizes.Add(new Models.PaperSizeModel() { Width = 1414d, Height = 1000d, Name = "ISO B0" });
        paperSizes.Add(new Models.PaperSizeModel() { Width = 1000d, Height = 707d, Name = "ISO B1" });
        paperSizes.Add(new Models.PaperSizeModel() { Width = 707d, Height = 500d, Name = "ISO B2" });
        paperSizes.Add(new Models.PaperSizeModel() { Width = 500d, Height = 353d, Name = "ISO B3" });
        paperSizes.Add(new Models.PaperSizeModel() { Width = 353d, Height = 250d, Name = "ISO B4" });

        return paperSizes;
    }

    public static string GetPapersize(double Width, double Height)
    {
        string retval;
        var paper = (from p in SupportedPaperSizes()
                     where p.Width == Math.Round(Width, 0) & p.Height == Math.Round(Height, 0)
                     select p).FirstOrDefault();
        if (paper is null)
        {
            retval = Math.Round(Width, 0).ToString() + " x " + Math.Round(Height, 0).ToString();
        }
        else
        {
            retval = paper.Name;
        }


        // Select Case Math.Round(Width, 0).ToString & " x " & Math.Round(Height, 0).ToString
        // Case "1189 x 841" : retval = "A0"
        // Case "1188 x 840" : retval = "A0"
        // Case "1190 x 840" : retval = "A0"
        // Case "841 x 594" : retval = "A1"
        // Case "840 x 594" : retval = "A1"
        // Case "594 x 420" : retval = "A2"
        // Case "420 x 297" : retval = "A3"
        // Case "297 x 210" : retval = "A4"
        // Case "210 x 297" : retval = "A4"

        // Case "1414 x 1000" : retval = "ISO B0"
        // Case "1000 x 707" : retval = "ISO B1"
        // Case "707 x 500" : retval = "ISO B2"
        // Case "500 x 353" : retval = "ISO B3"
        // Case "353 x 250" : retval = "ISO B4"

        // Case Else : retval = Width.ToString & " x " & Height.ToString
        // End Select

        return retval;
    }

    public static bool IsPrinterInstalled(string PrinterName)
    {
        bool retval = false;
        foreach (var ptName in PrinterSettings.InstalledPrinters)
        {
            if ((ptName.ToString() ?? "") == (PrinterName ?? ""))
            {
                var pt = new PrinterSettings
                {
                    PrinterName = ptName.ToString()
                };

                if (pt.IsValid)
                {
                    retval = true;
                    break;
                }
            }
        }

        return retval;
    }

    public static void SetDefaultPrinter(string printername)
    {
        var type__1 = Type.GetTypeFromProgID("WScript.Network");
        var instance = Activator.CreateInstance(type__1);
        type__1.InvokeMember("SetDefaultPrinter", BindingFlags.InvokeMethod, null, instance, new object[] { printername });
    }

    public static ExportPaperFormat GetSheetsize(ViewSheet Sheet, Document doc)
    {
        ExportPaperFormat retval;

        // retrieve the title block instances:
        FilteredElementCollector a;
        Parameter param;
        a = new FilteredElementCollector(doc);
        a.OfCategory(BuiltInCategory.OST_TitleBlocks);
        a.OfClass(typeof(FamilyInstance));

        // get the sizes with from the titleblock instances
        var Width = default(double);
        var Height = default(double);
        foreach (FamilyInstance FI in a)
        {
            param = FI.get_Parameter(BuiltInParameter.SHEET_NUMBER);
            if ((param.AsString() ?? "") == (Sheet.SheetNumber ?? ""))
            {
                // we have the tb instance
                param = FI.get_Parameter(BuiltInParameter.SHEET_WIDTH);
                Width = param.AsDouble().FootToMm();
                // Width = p.AsDouble

                param = FI.get_Parameter(BuiltInParameter.SHEET_HEIGHT);
                Height = param.AsDouble().FootToMm();
                // Height = p.AsDouble
            }
        }

        var paper = (from p in SupportedPaperSizes()
                     where p.Width == Math.Round(Width, 0) & p.Height == Math.Round(Height, 0)
                     select p).FirstOrDefault();
        if (paper is null)
        {
            retval = ExportPaperFormat.Default;
        }
        else
        {
            switch (paper.Name ?? "")
            {
                case "A0":
                    {
                        retval = ExportPaperFormat.ISO_A0;
                        break;
                    }

                case "A1":
                    {
                        retval = ExportPaperFormat.ISO_A1;
                        break;
                    }

                case "A2":
                    {
                        retval = ExportPaperFormat.ISO_A2;
                        break;
                    }

                case "A3":
                    {
                        retval = ExportPaperFormat.ISO_A3;
                        break;
                    }

                case "A4":
                    {
                        retval = ExportPaperFormat.ISO_A4;
                        break;
                    }

                case "B0":
                    {
                        retval = ExportPaperFormat.Default; // ISO_B0 does not exist in API
                        break;
                    }

                case "B1":
                    {
                        retval = ExportPaperFormat.ISO_B1;
                        break;
                    }

                case "B2":
                    {
                        retval = ExportPaperFormat.ISO_B2;
                        break;
                    }

                case "B3":
                    {
                        retval = ExportPaperFormat.ISO_B3;
                        break;
                    }

                case "B4":
                    {
                        retval = ExportPaperFormat.ISO_B4;
                        break;
                    }

                default:
                    {
                        retval = ExportPaperFormat.Default;
                        break;
                    }
            }
        }

        // convert units to mm
        // Select Case Math.Round(Width, 0).ToString & " x " & Math.Round(Height, 0).ToString
        // Case "1189 x 841" : retval = ExportPaperFormat.ISO_A0
        // Case "1188 x 840" : retval = ExportPaperFormat.ISO_A0
        // Case "1190 x 840" : retval = ExportPaperFormat.ISO_A0
        // Case "841 x 594" : retval = ExportPaperFormat.ISO_A1
        // Case "840 x 594" : retval = ExportPaperFormat.ISO_A1
        // Case "594 x 420" : retval = ExportPaperFormat.ISO_A2
        // Case "420 x 297" : retval = ExportPaperFormat.ISO_A3
        // Case "297 x 210" : retval = ExportPaperFormat.ISO_A4
        // Case "210 x 297" : retval = ExportPaperFormat.ISO_A4
        // Case "1414 x 1000" : retval = ExportPaperFormat.Default  'ISO_B0 does not exist in API
        // Case "1000 x 707" : retval = ExportPaperFormat.ISO_B1
        // Case "707 x 500" : retval = ExportPaperFormat.ISO_B2
        // Case "500 x 353" : retval = ExportPaperFormat.ISO_B3
        // Case "353 x 250" : retval = ExportPaperFormat.ISO_B4
        // Case Else : retval = ExportPaperFormat.Default
        // End Select

        return retval;
    }

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
