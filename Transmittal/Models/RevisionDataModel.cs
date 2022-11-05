using Autodesk.Revit.DB;

namespace Transmittal.Models;

public class RevisionDataModel
{
    public int Sequence { get; set; }

#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
    //nothing to add before 2022
    public RevisionNumberType Numbering { get; set; }
#else
    public string SequenceName { get; set; }
#endif

    public string RevDate { get; set; }
    public string Description { get; set; }
    public bool Issued { get; set; }

    public string IssuedBy { get; set; }

    public string IssuedTo { get; set; }

    public RevisionVisibility Show { get; set; }
    public ElementId SequenceId { get; set; }

    public RevisionDataModel(Revision r)
    {
        Sequence = r.SequenceNumber;
        RevDate = r.RevisionDate;
        Description = r.Description;
        Issued = r.Issued;
        IssuedTo = r.IssuedTo;
        IssuedBy = r.IssuedBy;
        Show = r.Visibility;

#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
        //nothing to add before 2022
        Numbering = r.NumberType;
#else
        SequenceId = r.RevisionNumberingSequenceId;

        using (var revisionNumberingSequence = (RevisionNumberingSequence)App.RevitDocument.GetElement(r.RevisionNumberingSequenceId))
        {
            SequenceName = revisionNumberingSequence.SequenceName;
        }
#endif
    }

    public RevisionDataModel()
    {

    }
}
