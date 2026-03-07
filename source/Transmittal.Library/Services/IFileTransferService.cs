namespace Transmittal.Library.Services;

public interface IFileTransferService
{
    /// <summary>
    /// Setup draft we transfer upload with the provided file paths. This will create a draft transfer and return true if successful.
    /// </summary>
    /// <param name="filePaths"></param>
    /// <returns></returns>
    Task<bool> PrepareFileTransferUploadAsync(List<string> filePaths);

    /// <summary>
    /// Setup draft we transfer upload with the provided file paths and list of recipients. This will create a draft transfer in and return true if successful.
    /// </summary>
    /// <param name="filePaths"></param>
    /// <returns></returns>
    Task<bool> PrepareFileTransferUploadAsync(List<string> filePaths, List<string> recipientsEmails);
}
