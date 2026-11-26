using Transmittal.Library.Models;

namespace Transmittal.Library.Enums;

/// <summary>
/// How the summary report revision sequence is rendered.
/// </summary>
public enum SummaryRevisionStyle
{
    /// <summary>A, B ... Z, AA, AB ...</summary>
    Letter,

    /// <summary>1, 2 ... zero-padded to <see cref="SummaryRevisionState.Digits"/>.</summary>
    Number
}
