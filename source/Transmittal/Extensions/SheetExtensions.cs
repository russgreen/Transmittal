using Autodesk.Revit.DB;
using Transmittal.Models;

namespace Transmittal.Extensions;
internal static class SheetExtensions
{
    public static FamilyInstance GetTitleBlockFamilyInstance(this ViewSheet sheet)
    {
        return new FilteredElementCollector(sheet.Document, sheet.Id)
            .OfCategory(BuiltInCategory.OST_TitleBlocks)
            .OfClass(typeof(FamilyInstance))
            .Cast<FamilyInstance>()
            .FirstOrDefault(fi => fi.OwnerViewId == sheet.Id);
    }

    public static string GetPaperSize(this ViewSheet sheet)
    {
        return sheet.GetTitleBlockFamilyInstance().GetPaperSizeModel().Name;
    }

    public static ExportPaperFormat GetExportPaperFormat(this ViewSheet sheet)
    {
        ExportPaperFormat retval;

        var paper = sheet.GetTitleBlockFamilyInstance().GetPaperSizeModel();

        if (paper is null)
        {
            retval = ExportPaperFormat.Default;
        }

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

        return retval;
    }

    public static PaperSizeModel GetPaperSizeModel(this FamilyInstance titleBlock)
    {
        var width = default(double);
        var height = default(double);

        if (titleBlock != null)
        {
            var p = titleBlock.get_Parameter(BuiltInParameter.SHEET_WIDTH);
            width = Math.Round(p.AsDouble().FootToMm(), 0);

            p = titleBlock.get_Parameter(BuiltInParameter.SHEET_HEIGHT);
            height = Math.Round(p.AsDouble().FootToMm(), 0);
        }

        var paper = (from p in SupportedPaperSizes() where 
                     p.Width == width & p.Height ==  height select p)
                     .FirstOrDefault();

        if (paper != null)
        {
            return paper;
        }

        //we didn't find a match, so we'll create a new one
        return new PaperSizeModel { Name = $"{width} x {height}", Width = width, Height = height};
    }

    private static List<Models.PaperSizeModel> SupportedPaperSizes()
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
}
