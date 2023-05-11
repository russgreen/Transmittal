using Autodesk.Revit.DB;

namespace Transmittal.Models;

public class RevisionDataModel
{
    public int Sequence { get; set; }

#if REVIT2022_OR_GREATER
    public string SequenceName { get; set; }
#else
    public RevisionNumberType Numbering { get; set; }
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

#if REVIT2022_OR_GREATER
        SequenceId = r.RevisionNumberingSequenceId;

        using (var revisionNumberingSequence = (RevisionNumberingSequence)App.RevitDocument.GetElement(r.RevisionNumberingSequenceId))
        {
            SequenceName = revisionNumberingSequence.SequenceName;
        }
#else
        Numbering = r.NumberType;
#endif
    }

    public RevisionDataModel()
    {

    }
}
