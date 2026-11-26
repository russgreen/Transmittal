using System.Text.Json;
using Transmittal.Library.Enums;
using Transmittal.Library.Models;
using Transmittal.Library.Services;

namespace Transmittal.Library.Tests;

public class SummaryRevisionTrackerTests
{
    private string _folder = string.Empty;
    private string _databaseFile = string.Empty;

    [Before(Test)]
    public async Task Setup()
    {
        _folder = Path.Combine(Path.GetTempPath(), $"summary-revision-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_folder);
        _databaseFile = Path.Combine(_folder, "project.tdb");
        await Task.CompletedTask;
    }

    [After(Test)]
    public async Task Cleanup()
    {
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, true);
        }

        await Task.CompletedTask;
    }

    [Test]
    public async Task GetRevision_FirstRun_ReturnsDefaultP01AndWritesStateFile()
    {
        var revision = SummaryRevisionTracker.GetRevision(CreateTransmittals(), _databaseFile);

        await Assert.That(revision).IsEqualTo("P01");
        await Assert.That(File.Exists(Path.Combine(_folder, "project.summaryrevision.json"))).IsTrue();
    }

    [Test]
    public async Task GetRevision_UnchangedData_DoesNotIncrement()
    {
        SummaryRevisionTracker.GetRevision(CreateTransmittals(), _databaseFile);
        var revision = SummaryRevisionTracker.GetRevision(CreateTransmittals(), _databaseFile);

        await Assert.That(revision).IsEqualTo("P01");
    }

    [Test]
    public async Task GetRevision_NewTransmittal_Increments()
    {
        SummaryRevisionTracker.GetRevision(CreateTransmittals(), _databaseFile);

        var changed = CreateTransmittals();
        changed.Add(new TransmittalModel { ID = 3, TransDate = new DateTime(2024, 3, 1) });

        await Assert.That(SummaryRevisionTracker.GetRevision(changed, _databaseFile)).IsEqualTo("P02");
    }

    [Test]
    public async Task GetRevision_EditedItem_Increments()
    {
        SummaryRevisionTracker.GetRevision(CreateTransmittals(), _databaseFile);

        var changed = CreateTransmittals();
        changed[0].Items[0].DrgRev = "P02";

        await Assert.That(SummaryRevisionTracker.GetRevision(changed, _databaseFile)).IsEqualTo("P02");
    }

    [Test]
    public async Task GetRevision_DeletedTransmittal_Increments()
    {
        SummaryRevisionTracker.GetRevision(CreateTransmittals(), _databaseFile);

        var changed = CreateTransmittals();
        changed.RemoveAt(1);

        await Assert.That(SummaryRevisionTracker.GetRevision(changed, _databaseFile)).IsEqualTo("P02");
    }

    [Test]
    public async Task GetRevision_ListOrderDoesNotMatter()
    {
        SummaryRevisionTracker.GetRevision(CreateTransmittals(), _databaseFile);

        var reordered = CreateTransmittals();
        reordered.Reverse();

        await Assert.That(SummaryRevisionTracker.GetRevision(reordered, _databaseFile)).IsEqualTo("P01");
    }

    [Test]
    public async Task GetRevision_LetterStyleNoPrefix_FormatsAndIncrements()
    {
        SummaryRevisionTracker.GetRevision(CreateTransmittals(), _databaseFile);
        EditState(state =>
        {
            state.Prefix = string.Empty;
            state.Style = SummaryRevisionStyle.Letter;
        });

        // Formatting changes re-render the current sequence without incrementing.
        await Assert.That(SummaryRevisionTracker.GetRevision(CreateTransmittals(), _databaseFile)).IsEqualTo("A");

        var changed = CreateTransmittals();
        changed[0].Items[0].DrgName = "Renamed";
        await Assert.That(SummaryRevisionTracker.GetRevision(changed, _databaseFile)).IsEqualTo("B");
    }

    [Test]
    public async Task GetRevision_CorruptStateFile_RestartsAtDefault()
    {
        File.WriteAllText(Path.Combine(_folder, "project.summaryrevision.json"), "not json");

        await Assert.That(SummaryRevisionTracker.GetRevision(CreateTransmittals(), _databaseFile)).IsEqualTo("P01");
    }

    [Test]
    public async Task GetRevision_UnwritableLocation_DoesNotThrow()
    {
        var missingFolderDatabase = Path.Combine(_folder, "missing", "project.tdb");

        await Assert.That(SummaryRevisionTracker.GetRevision(CreateTransmittals(), missingFolderDatabase)).IsEqualTo("P01");
    }

    [Test]
    [Arguments(1, "A")]
    [Arguments(26, "Z")]
    [Arguments(27, "AA")]
    [Arguments(28, "AB")]
    [Arguments(52, "AZ")]
    [Arguments(53, "BA")]
    [Arguments(702, "ZZ")]
    [Arguments(703, "AAA")]
    public async Task ToLetters_ConvertsSequence(int sequence, string expected)
    {
        await Assert.That(SummaryRevisionTracker.ToLetters(sequence)).IsEqualTo(expected);
    }

    [Test]
    public async Task FormatRevision_NumberStyle_PadsToDigits()
    {
        var state = new SummaryRevisionState { Prefix = "C", Style = SummaryRevisionStyle.Number, Digits = 2, Sequence = 1 };

        await Assert.That(SummaryRevisionTracker.FormatRevision(state)).IsEqualTo("C01");

        state.Sequence = 100;
        await Assert.That(SummaryRevisionTracker.FormatRevision(state)).IsEqualTo("C100");
    }

    private void EditState(Action<SummaryRevisionState> edit)
    {
        var path = SummaryRevisionTracker.GetStateFilePath(_databaseFile);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
        var state = JsonSerializer.Deserialize<SummaryRevisionState>(File.ReadAllText(path), options)!;
        edit(state);
        File.WriteAllText(path, JsonSerializer.Serialize(state, options));
    }

    private static List<TransmittalModel> CreateTransmittals() =>
    [
        new TransmittalModel
        {
            ID = 1,
            TransDate = new DateTime(2024, 1, 1),
            Items = [new TransmittalItemModel { TransItemID = 1, TransID = 1, DrgNumber = "A-001", DrgRev = "P01", DrgName = "Doc A" }],
            Distribution = [new TransmittalDistributionModel { TransDistID = 1, TransID = 1, PersonID = 1, TransFormat = "E", TransCopies = 1 }]
        },
        new TransmittalModel
        {
            ID = 2,
            TransDate = new DateTime(2024, 2, 1),
            Items = [new TransmittalItemModel { TransItemID = 2, TransID = 2, DrgNumber = "A-002", DrgRev = "P01", DrgName = "Doc B" }]
        }
    ];
}
