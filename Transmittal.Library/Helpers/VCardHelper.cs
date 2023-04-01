using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Transmittal.Library.Helpers;
public static class VCardHelper
{
    public static void ExportVCard(Models.ProjectDirectoryModel directoryModel)
    {
        System.Text.StringBuilder sb = new StringBuilder();

        sb.AppendFormat("BEGIN:VCARD{0}", System.Environment.NewLine);
        sb.AppendFormat("VERSION:4.0{0}", System.Environment.NewLine);
        sb.Append($"N:{directoryModel.Person.LastName};{directoryModel.Person.FirstName}{System.Environment.NewLine}");
        sb.Append($"FN:{directoryModel.Person.FullName}{System.Environment.NewLine}");
        sb.Append($"ORG:{directoryModel.Company.CompanyName}{System.Environment.NewLine}");
        sb.Append($"URL;WORK:{directoryModel.Company.Website}{System.Environment.NewLine}");
        sb.Append($"TITLE:{directoryModel.Person.Position}{System.Environment.NewLine}");
        sb.Append($"TEL;TYPE=work;type=pref;VOICE;VALUE=uri:{directoryModel.Company.Tel}{System.Environment.NewLine}");
        sb.Append($"TEL;TYPE=work;VOICE;VALUE=uri:{directoryModel.Person.Tel}{System.Environment.NewLine}");
        sb.Append($"TEL;TYPE=cell;VOICE;VALUE=uri:{directoryModel.Person.Mobile}{System.Environment.NewLine}");
        sb.Append($"TEL;TYPE=work;FAX:{directoryModel.Company.Fax}{System.Environment.NewLine}");
        sb.Append($"EMAIL;PREF;INTERNET:{directoryModel.Person.Email}{System.Environment.NewLine}");

        var address = string.Empty;
        if (directoryModel.Company.Address != null) 
        {
            address = directoryModel.Company.Address.Replace("\r\n", ", ");
        }
        sb.Append($"ADR;WORK;PREF;ENCODING=QUOTED-PRINTABLE:;;{address}{System.Environment.NewLine}");

        sb.Append($"NOTE:{directoryModel.Person.Notes}{System.Environment.NewLine}");
        sb.AppendFormat("END:VCARD{0}", System.Environment.NewLine);

        string vcfPath = $@"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Transmittal", "Contacts")}\{directoryModel.Person.FullName}.vcf";

        System.IO.FileInfo file = new System.IO.FileInfo(vcfPath);
        file.Directory.Create();

        if (file.Exists)
        {
            file.Delete();
        }
        
        File.WriteAllText(file.FullName, sb.ToString(), System.Text.Encoding.ASCII);

        ProcessStartInfo startInfo = new ProcessStartInfo(vcfPath);
        startInfo.UseShellExecute = true;
        Process.Start(startInfo);
    }

    public static Models.ProjectDirectoryModel ImportVCard(string vCardFilePath)
    {
        var directoryModel = new Models.ProjectDirectoryModel();

        var vCard = File.ReadAllText(vCardFilePath);

        var lines = vCard.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var key = line.Substring(0, line.IndexOf(":"));
            var value = line.Substring(line.IndexOf(":") + 1);

            switch (key)
            {
                case "N":
                    var names = value.Split(';');
                    directoryModel.Person.LastName = names[0];
                    directoryModel.Person.FirstName = names[1];
                    break;
                //case "FN":
                //    directoryModel.Person.FullName = value;
                //    break;
                case "ORG":
                    directoryModel.Company.CompanyName = value;
                    break;
                case "URL;WORK":
                    directoryModel.Company.Website = value;
                    break;
                case "TITLE":
                    directoryModel.Person.Position = value;
                    break;
                case "TEL;TYPE=work;type=pref;VOICE;VALUE=uri":
                    directoryModel.Company.Tel = value;
                    break;
                case "TEL;TYPE=work;VOICE;VALUE=uri":
                    directoryModel.Person.Tel = value;
                    break;
                case "TEL;TYPE=cell;VOICE;VALUE=uri":
                    directoryModel.Person.Mobile = value;
                    break;
                case "TEL;TYPE=work;FAX":
                    directoryModel.Company.Fax = value;
                    break;
                case "EMAIL;PREF;INTERNET":
                    directoryModel.Person.Email = value;
                    break;
                case "ADR;WORK;PREF;ENCODING=QUOTED-PRINTABLE":
                    directoryModel.Company.Address = value.Replace(";", System.Environment.NewLine);
                    break;
                case "NOTE":
                    directoryModel.Person.Notes = value;
                    break;
            }
        }

        return directoryModel;
    }
}
