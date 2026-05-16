namespace Transmittal.Library.Extensions;

public static class DateExtensions
{
    /// <summary>
    /// Returns a string representation in the format YY.MM.DD
    /// </summary>
    /// <param name="date"></param>
    public static string ToStringYYMMDDEx(this DateTime date)
    {
        return $"{date.ToStringYY()}.{date.ToStringMM()}.{date.ToStringDD()}";
    }

    /// <summary>
    /// Returns a string representation in the format YYMMDD
    /// </summary>
    /// <param name="date"></param>
    public static string ToStringYYMMDD(this DateTime date)
    {
        return $"{date.ToStringYY()}{date.ToStringMM()}{date.ToStringDD()}";
    }

    /// <summary>
    /// Returns a string representation if the year in the format YY
    /// </summary>
    /// <param name="date"></param>
    public static string ToStringYY(this DateTime date)
    {
        return date.Year.ToString().Substring(2, 2);
    }

    /// <summary>
    /// Returns a string representation if the month number in the format MM
    /// </summary>
    /// <param name="date"></param>
    public static string ToStringMM(this DateTime date)
    {
        return date.Month.ToString("D2");
    }

    /// <summary>
    /// Returns a string representation if the day number in the format DD
    /// </summary>
    /// <param name="date"></param>
    public static string ToStringDD(this DateTime date)
    {
        return date.Day.ToString("D2");
    }

    /// <summary>
    /// Reformats a date string to the specified format string
    /// </summary>
    /// <param name="dateString">The date string to reformat</param>
    /// <param name="targetFormat">The target format string (e.g., "dd.MM.yy")</param>
    /// <returns>Reformatted date string, or original string if parsing fails</returns>
    public static string ToDateFormat(this string dateString, string targetFormat)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return dateString;

        if (DateTime.TryParse(dateString, out DateTime parsedDate))
        {
            return parsedDate.ToString(targetFormat);
        }

        return dateString;
    }
}
