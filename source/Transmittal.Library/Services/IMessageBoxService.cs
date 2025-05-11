using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transmittal.Library.Services;
public interface IMessageBoxService
{
    /// <summary>
    /// Display a message box with Yes and No buttons
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <returns>True of Yes clicked, else false</returns>
    bool ShowYesNo(string title, string message);

    /// <summary>
    /// Display a message box with OK and Cancel buttons
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <returns>True of Ok clicked, else false</returns>
    bool ShowOkCancel(string title, string message);

    /// <summary>
    /// Display a message box with Cancel button 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <returns>True of Cancel clicked, else false</returns>
    bool ShowCancel(string title, string message);

    /// <summary>
    /// Display a message box with OK button
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <returns>True of Ok clicked, else false</returns>
    bool ShowOk(string title, string message);
}
