using Transmittal.Library.Enums;

namespace Transmittal.Library.Models;

/// <summary>
/// Revision state persisted in the <c>.summaryrevision.json</c> file beside the project database.
/// </summary>
public class SummaryRevisionState
{
    public string Prefix { get; set; } = "P";
    public SummaryRevisionStyle Style { get; set; } = SummaryRevisionStyle.Number;
    public int Digits { get; set; } = 2;
    public int Sequence { get; set; } = 1;
    public int LatestTransmittalID { get; set; }
    public string DataHash { get; set; } = string.Empty;
}
