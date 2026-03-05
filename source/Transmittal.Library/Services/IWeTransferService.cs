using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transmittal.Library.Services;

public interface IWeTransferService
{
    /// <summary>
    /// Setup draft we transfer upload with the provided file paths. This will create a draft transfer in WeTransfer and return true if successful.
    /// </summary>
    /// <param name="filePaths"></param>
    /// <returns></returns>
    Task<bool> PrepareWeTransferUploadAsync(List<string> filePaths);

    /// <summary>
    /// Setup draft we transfer upload with the provided file paths and list of recipients. This will create a draft transfer in WeTransfer and return true if successful.
    /// </summary>
    /// <param name="filePaths"></param>
    /// <returns></returns>
    Task<bool> PrepareWeTransferUploadAsync(List<string> filePaths, List<string> recipientsEmails);
}
