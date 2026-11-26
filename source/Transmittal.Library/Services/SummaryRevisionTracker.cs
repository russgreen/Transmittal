using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Transmittal.Library.Enums;
using Transmittal.Library.Extensions;
using Transmittal.Library.Models;

namespace Transmittal.Library.Services;

/// <summary>
/// Works out the revision of the full transmittal summary report. The sequence only increments when the
/// transmittal data has changed since the previous call. State is kept in a JSON file beside the database
/// so that no database or Revit settings schema is affected.
/// </summary>
public static class SummaryRevisionTracker
{
    public const string FileSuffix = ".summaryrevision.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    /// <summary>
    /// Gets the revision for the full summary, incrementing and persisting it if the data changed.
    /// </summary>
    /// <param name="transmittals">All transmittals, with items and distribution loaded.</param>
    /// <param name="databaseFile">The project database path. The state file is stored beside it.</param>
    /// <returns>The formatted revision, e.g. <c>A</c> or <c>P01</c>.</returns>
    public static string GetRevision(IEnumerable<TransmittalModel> transmittals, string databaseFile)
    {
        var path = GetStateFilePath(databaseFile);
        var all = (transmittals ?? Enumerable.Empty<TransmittalModel>()).ToList();
        var latestId = all.Count == 0 ? 0 : all.Max(t => t.ID);
        var hash = ComputeHash(all);

        var state = TryReadState(path, out var existing) ? existing : null;
        var changed = false;

        if (state == null)
        {
            state = new SummaryRevisionState();
            changed = true;
        }
        else if (state.LatestTransmittalID != latestId || !string.Equals(state.DataHash, hash, StringComparison.Ordinal))
        {
            state.Sequence = Math.Max(state.Sequence, 0) + 1;
            changed = true;
        }

        if (state.Sequence < 1)
        {
            state.Sequence = 1;
        }

        if (changed)
        {
            state.LatestTransmittalID = latestId;
            state.DataHash = hash;
            TryWriteState(path, state);
        }

        return FormatRevision(state);
    }

    /// <summary>
    /// Renders the state's sequence with its prefix and style.
    /// </summary>
    public static string FormatRevision(SummaryRevisionState state)
    {
        var sequence = Math.Max(state.Sequence, 1);
        var rendered = state.Style == SummaryRevisionStyle.Number
            ? sequence.ToString("D" + Math.Max(state.Digits, 1))
            : ToLetters(sequence);

        return (state.Prefix ?? string.Empty) + rendered;
    }

    /// <summary>
    /// Converts a 1-based number to Excel-style letters: 1 = A, 26 = Z, 27 = AA.
    /// </summary>
    public static string ToLetters(int sequence)
    {
        var builder = new StringBuilder();
        var n = Math.Max(sequence, 1);
        while (n > 0)
        {
            n--;
            builder.Insert(0, (char)('A' + (n % 26)));
            n /= 26;
        }

        return builder.ToString();
    }

    /// <summary>
    /// Hashes everything the summary report is built from, independent of list order.
    /// </summary>
    public static string ComputeHash(IEnumerable<TransmittalModel> transmittals)
    {
        var builder = new StringBuilder();

        foreach (var transmittal in transmittals.OrderBy(t => t.ID))
        {
            builder.Append("T|").Append(transmittal.ID).Append('|')
                .Append(transmittal.TransDate.ToString("o", System.Globalization.CultureInfo.InvariantCulture)).Append('\n');

            foreach (var item in transmittal.Items.OrderBy(i => i.TransItemID))
            {
                builder.Append("I|")
                    .Append(string.Join("|", item.TransItemID, item.DrgNumber, item.DrgRev, item.DrgName, item.DrgPaper, item.DrgScale,
                        item.DrgDrawn, item.DrgChecked, item.DrgProj, item.DrgOriginator, item.DrgVolume, item.DrgLevel,
                        item.DrgType, item.DrgRole, item.DrgStatus, item.DrgStatusDescription, item.DrgPackage,
                        item.DrgSheetCollection))
                    .Append('\n');
            }

            foreach (var dist in transmittal.Distribution.OrderBy(d => d.TransDistID))
            {
                builder.Append("D|")
                    .Append(string.Join("|", dist.TransDistID, dist.PersonID, dist.TransFormat, dist.TransCopies))
                    .Append('\n');
            }
        }

        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }

    public static string GetStateFilePath(string databaseFile)
    {
        var resolved = databaseFile.ParsePathWithEnvironmentVariables();
        var directory = Path.GetDirectoryName(resolved) ?? string.Empty;
        return Path.Combine(directory, Path.GetFileNameWithoutExtension(resolved) + FileSuffix);
    }

    private static bool TryReadState(string path, out SummaryRevisionState state)
    {
        state = null;
        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            state = JsonSerializer.Deserialize<SummaryRevisionState>(File.ReadAllText(path), JsonOptions);
            return state != null;
        }
        catch (Exception)
        {
            // Missing or corrupt state starts the sequence again.
            state = null;
            return false;
        }
    }

    private static void TryWriteState(string path, SummaryRevisionState state)
    {
        try
        {
            var temp = path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(state, JsonOptions));
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.Move(temp, path);
        }
        catch (Exception)
        {
            // A read-only location must not stop the report being generated.
        }
    }
}
