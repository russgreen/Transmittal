namespace Transmittal.Exceptions;

/// <summary>
/// Thrown when a Revit document contains a Transmittal schema version newer than the current application supports.
/// This indicates the user needs to update to a newer version of the application.
/// </summary>
public class SchemaVersionTooNewException : Exception
{
    public int DocumentSchemaVersion { get; }
    public int ApplicationSchemaVersion { get; }

    public SchemaVersionTooNewException(int documentSchemaVersion, int applicationSchemaVersion)
        : base($"Document schema version {documentSchemaVersion} is newer than application support (version {applicationSchemaVersion})")
    {
        DocumentSchemaVersion = documentSchemaVersion;
        ApplicationSchemaVersion = applicationSchemaVersion;
    }
}
